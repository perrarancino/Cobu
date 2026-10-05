using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CoBu.Services
{
    public class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        private const uint VK_V = 0x56;
        private const uint VK_C = 0x43;
        private const uint VK_OEM_3 = 0xC0; // `~ key

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _hwnd;
        private HwndSource? _hwndSource;
        private readonly List<int> _registeredIds = new();
        private bool _isDisposed;

        public event Action? HotkeyPressed;

        public void Initialize(Window window, string initialHotkey = "Alt + V")
        {
            var wih = new WindowInteropHelper(window);
            _hwnd = wih.EnsureHandle();

            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource?.AddHook(WndProc);

            ApplyHotkey(initialHotkey);
        }

        public void ApplyHotkey(string hotkeyName)
        {
            if (_hwnd == IntPtr.Zero) return;

            // Unregister old hotkeys
            foreach (var id in _registeredIds)
            {
                UnregisterHotKey(_hwnd, id);
            }
            _registeredIds.Clear();

            switch (hotkeyName)
            {
                case "Ctrl + Shift + V":
                    TryRegister(1, MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_V, "Ctrl + Shift + V");
                    break;
                case "Ctrl + ~":
                    TryRegister(1, MOD_CONTROL | MOD_NOREPEAT, VK_OEM_3, "Ctrl + ~");
                    break;
                case "Alt + C":
                    TryRegister(1, MOD_ALT | MOD_NOREPEAT, VK_C, "Alt + C");
                    break;
                case "Win + Shift + V":
                    TryRegister(1, MOD_WIN | MOD_SHIFT | MOD_NOREPEAT, VK_V, "Win + Shift + V");
                    break;
                case "Alt + V":
                default:
                    TryRegister(1, MOD_ALT | MOD_NOREPEAT, VK_V, "Alt + V");
                    break;
            }
        }

        private void TryRegister(int id, uint modifiers, uint vk, string name)
        {
            bool success = RegisterHotKey(_hwnd, id, modifiers, vk);
            if (success)
            {
                _registeredIds.Add(id);
                Debug.WriteLine($"Hotkey registered: {name} (ID: {id})");
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Could not register hotkey {name}: error {error}");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_registeredIds.Contains(id))
                {
                    HotkeyPressed?.Invoke();
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
            foreach (var id in _registeredIds)
            {
                UnregisterHotKey(_hwnd, id);
            }
            _registeredIds.Clear();
            _hwndSource?.RemoveHook(WndProc);
        }
    }
}
