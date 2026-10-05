using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CoBu.Services
{
    public class TrayIconManager : IDisposable
    {
        private const int WM_USER = 0x0400;
        public const int WM_TRAYICON = WM_USER + 101;

        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;

        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;

        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x0010;
        private const uint LR_DEFAULTSIZE = 0x0040;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpdata);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint PrivateExtractIcons(string szFileName, int nIconIndex, int cxIcon, int cyIcon, out IntPtr phicon, out uint piconid, uint nIcons, uint flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        private const int SM_CXSMICON = 49;
        private const int SM_CYSMICON = 50;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string lpString);

        private IntPtr _hwnd;
        private HwndSource? _hwndSource;
        private IntPtr _hIcon = IntPtr.Zero;
        private bool _isAdded;
        private bool _isDisposed;

        // Broadcast by Explorer when the taskbar is (re)created: at logon (autostart starts us before the
        // tray exists) and after an Explorer restart. The icon has to be added again then.
        private uint _taskbarCreatedMessage;
        private System.Windows.Threading.DispatcherTimer? _retryTimer;
        private int _retryCount;

        public event Action? TrayLeftClicked;
        public event Action<Point>? TrayRightClicked;

        public void Initialize(Window window)
        {
            var wih = new WindowInteropHelper(window);
            _hwnd = wih.EnsureHandle();

            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource?.AddHook(WndProc);

            _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");

            LoadTrayIcon();
            AddTrayIcon();

            if (!_isAdded)
            {
                // Explorer is not ready yet (autostart): retry for a while, TaskbarCreated also covers this
                _retryTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _retryTimer.Tick += (s, e) =>
                {
                    AddTrayIcon();
                    if (_isAdded || ++_retryCount >= 20)
                    {
                        _retryTimer?.Stop();
                    }
                };
                _retryTimer.Start();
            }
        }

        private void LoadTrayIcon()
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(iconPath))
            {
                int cx = GetSystemMetrics(SM_CXSMICON);
                int cy = GetSystemMetrics(SM_CYSMICON);
                if (cx <= 0) cx = 16;
                if (cy <= 0) cy = 16;

                uint count = PrivateExtractIcons(iconPath, 0, cx, cy, out _hIcon, out _, 1, 0);
                if (count == 0 || _hIcon == IntPtr.Zero)
                {
                    _hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, cx, cy, LR_LOADFROMFILE);
                }
            }

            if (_hIcon == IntPtr.Zero)
            {
                // Fallback to standard system application icon
                _hIcon = LoadIcon(IntPtr.Zero, (IntPtr)32512); // IDI_APPLICATION
            }
        }

        private void AddTrayIcon()
        {
            if (_hwnd == IntPtr.Zero || _isAdded)
                return;

            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1001,
                uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage = WM_TRAYICON,
                hIcon = _hIcon,
                szTip = Loc.T("tray_tip")
            };

            _isAdded = Shell_NotifyIcon(NIM_ADD, ref nid);
        }

        /// <summary>Re-reads the tooltip text (after the UI language changed).</summary>
        public void UpdateTooltip()
        {
            if (!_isAdded || _hwnd == IntPtr.Zero)
                return;

            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1001,
                uFlags = NIF_TIP,
                szTip = Loc.T("tray_tip")
            };

            Shell_NotifyIcon(NIM_MODIFY, ref nid);
        }

        private void RemoveTrayIcon()
        {
            if (!_isAdded || _hwnd == IntPtr.Zero)
                return;

            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1001
            };

            Shell_NotifyIcon(NIM_DELETE, ref nid);
            _isAdded = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_taskbarCreatedMessage != 0 && (uint)msg == _taskbarCreatedMessage)
            {
                _isAdded = false;
                AddTrayIcon();
                return IntPtr.Zero;
            }

            if (msg == WM_TRAYICON)
            {
                int mouseEvent = lParam.ToInt32();
                if (mouseEvent == WM_LBUTTONUP || mouseEvent == WM_LBUTTONDBLCLK)
                {
                    TrayLeftClicked?.Invoke();
                    handled = true;
                }
                else if (mouseEvent == WM_RBUTTONUP)
                {
                    SetForegroundWindow(_hwnd);
                    if (GetCursorPos(out POINT p))
                    {
                        TrayRightClicked?.Invoke(new Point(p.X, p.Y));
                    }
                    else
                    {
                        TrayRightClicked?.Invoke(new Point(0, 0));
                    }
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _retryTimer?.Stop();
            RemoveTrayIcon();
            _hwndSource?.RemoveHook(WndProc);

            if (_hIcon != IntPtr.Zero)
            {
                DestroyIcon(_hIcon);
                _hIcon = IntPtr.Zero;
            }
        }
    }
}
