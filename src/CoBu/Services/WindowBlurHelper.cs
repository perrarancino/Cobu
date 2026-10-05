using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CoBu.Services
{
    public static class WindowBlurHelper
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private static double GetScale(IntPtr hwnd)
        {
            try
            {
                uint dpi = GetDpiForWindow(hwnd);
                return dpi > 0 ? dpi / 96.0 : 1.0;
            }
            catch
            {
                return 1.0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public static void ApplyAcrylicBlur(Window window, int cornerRadius = 26)
        {
            try
            {
                var wih = new WindowInteropHelper(window);
                IntPtr hwnd = wih.EnsureHandle();

                // Sizes passed here are in DIPs; ApplyAcrylicBlurToHwnd converts them to the real pixel size
                int width = (int)Math.Ceiling(window.Width);
                int height = (int)Math.Ceiling(window.Height);

                ApplyAcrylicBlurToHwnd(hwnd, width, height, cornerRadius);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply acrylic blur: {ex.Message}");
            }
        }

        public static void ApplyAcrylicBlurToHwnd(IntPtr hwnd, int width, int height, int cornerRadius = 16)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                // SetWindowRgn works in physical pixels, while width/height/cornerRadius arrive in DIPs.
                // At 125-200% display scaling a DIP-sized region would clip the window to its top-left part.
                double scale = GetScale(hwnd);
                int pxWidth = (int)Math.Ceiling(width * scale);
                int pxHeight = (int)Math.Ceiling(height * scale);
                if (GetWindowRect(hwnd, out RECT rect) && rect.Right > rect.Left && rect.Bottom > rect.Top)
                {
                    // the real window size is the most reliable source
                    pxWidth = rect.Right - rect.Left;
                    pxHeight = rect.Bottom - rect.Top;
                }
                int pxCorner = (int)Math.Round(cornerRadius * scale);

                if (pxWidth > 0 && pxHeight > 0)
                {
                    IntPtr hRgn = CreateRoundRectRgn(0, 0, pxWidth + 1, pxHeight + 1, pxCorner * 2, pxCorner * 2);
                    SetWindowRgn(hwnd, hRgn, true);
                }

                var accent = new AccentPolicy
                {
                    AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                    AccentFlags = 0,
                    GradientColor = unchecked((int)0x42261418) // Translucent midnight tint matching MidnightCanvasBgBrush
                };

                int size = Marshal.SizeOf(accent);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(accent, ptr, false);
                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        Data = ptr,
                        SizeOfData = size
                    };

                    int res = SetWindowCompositionAttribute(hwnd, ref data);
                    if (res != 0)
                    {
                        accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                        Marshal.StructureToPtr(accent, ptr, false);
                        SetWindowCompositionAttribute(hwnd, ref data);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply acrylic blur to hwnd: {ex.Message}");
            }
        }
    }
}
