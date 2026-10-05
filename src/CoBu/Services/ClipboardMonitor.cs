using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoBu.Services
{
    public class ClipboardMonitor : IDisposable
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private IntPtr _hwnd;
        private HwndSource? _hwndSource;
        private bool _isDisposed;
        private volatile bool _isInternalCopy;
        private DateTime _lastInternalCopyTime = DateTime.MinValue;
        private string? _lastProcessedText;
        private DateTime _lastProcessedTextTime = DateTime.MinValue;
        private DateTime _lastProcessedImageTime = DateTime.MinValue;

        public event Func<string, Task>? TextCopied;
        public event Func<byte[], int, int, Task>? ImageCopied; // PNG bytes, width, height

        public void Initialize(Window window)
        {
            var wih = new WindowInteropHelper(window);
            _hwnd = wih.EnsureHandle();

            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource?.AddHook(WndProc);

            bool success = AddClipboardFormatListener(_hwnd);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Failed to register clipboard format listener: {error}");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }
            return IntPtr.Zero;
        }

        private async void OnClipboardChanged()
        {
            // If the copy action originated from CoBu, skip recording it as a new duplicate
            if (_isInternalCopy || (DateTime.UtcNow - _lastInternalCopyTime).TotalMilliseconds < 600)
            {
                _isInternalCopy = false;
                return;
            }

            // Retry reading clipboard in case another app holds it briefly
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    // Delay slightly to let the sending process finish writing clipboard data
                    if (attempt > 0)
                    {
                        await Task.Delay(40);
                    }

                    // Password managers mark secrets with this format: never record them
                    if (Clipboard.ContainsData("ExcludeClipboardContentFromMonitorProcessing"))
                    {
                        return;
                    }

                    if (Clipboard.ContainsImage())
                    {
                        if ((DateTime.UtcNow - _lastProcessedImageTime).TotalMilliseconds < 1200)
                        {
                            return;
                        }

                        var image = Clipboard.GetImage();
                        if (image != null)
                        {
                            _lastProcessedImageTime = DateTime.UtcNow;
                            // Encode to PNG once; this also detaches the data from clipboard memory
                            int width = image.PixelWidth;
                            int height = image.PixelHeight;
                            byte[] png = EncodePng(image);
                            image = null;
                            if (ImageCopied != null)
                            {
                                await ImageCopied.Invoke(png, width, height);
                            }
                            return;
                        }
                    }

                    if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            if (text == _lastProcessedText && (DateTime.UtcNow - _lastProcessedTextTime).TotalMilliseconds < 1200)
                            {
                                return;
                            }

                            _lastProcessedText = text;
                            _lastProcessedTextTime = DateTime.UtcNow;

                            if (TextCopied != null)
                            {
                                await TextCopied.Invoke(text);
                            }
                            return;
                        }
                    }

                    // Nothing to capture or not a format we track
                    return;
                }
                catch (COMException)
                {
                    // Clipboard locked by another app, retry
                    await Task.Delay(30);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Clipboard processing error: {ex.Message}");
                    return;
                }
            }
        }

        private static byte[] EncodePng(BitmapSource source)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }

        public void CopyText(string text)
        {
            _isInternalCopy = true;
            _lastInternalCopyTime = DateTime.UtcNow;

            try
            {
                Clipboard.SetDataObject(text, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set clipboard text: {ex.Message}");
            }
        }

        public void CopyImage(string imagePath)
        {
            _isInternalCopy = true;
            _lastInternalCopyTime = DateTime.UtcNow;

            try
            {
                if (File.Exists(imagePath))
                {
                    // WPF's DataObject.SetImage leaks ~the whole bitmap (tens of MB for a screenshot) on every call,
                    // so the clipboard is filled through the Win32 API instead: PNG (alpha-aware apps), DIB (everything
                    // else) and a file drop list (pasting into Explorer).
                    if (!SetImageToClipboard(imagePath))
                    {
                        Debug.WriteLine("Failed to set clipboard image: clipboard is busy");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set clipboard image: {ex.Message}");
            }
        }

        // ---- direct Win32 clipboard writing ----

        private const uint CF_DIB = 8;
        private const uint CF_HDROP = 15;
        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint GMEM_ZEROINIT = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        private bool SetImageToClipboard(string imagePath)
        {
            byte[] png = File.ReadAllBytes(imagePath);
            IntPtr hDib = IntPtr.Zero, hPng = IntPtr.Zero, hDrop = IntPtr.Zero;

            try
            {
                hDib = BuildDib(png);
                hPng = AllocGlobal(png);
                hDrop = BuildDropFiles(imagePath);

                bool opened = false;
                for (int attempt = 0; attempt < 10 && !opened; attempt++)
                {
                    opened = OpenClipboard(_hwnd);
                    if (!opened)
                        System.Threading.Thread.Sleep(20);
                }
                if (!opened)
                    return false;

                try
                {
                    EmptyClipboard();

                    // After a successful SetClipboardData the system owns the memory block
                    if (hDib != IntPtr.Zero && SetClipboardData(CF_DIB, hDib) != IntPtr.Zero)
                        hDib = IntPtr.Zero;

                    uint pngFormat = RegisterClipboardFormat("PNG");
                    if (hPng != IntPtr.Zero && pngFormat != 0 && SetClipboardData(pngFormat, hPng) != IntPtr.Zero)
                        hPng = IntPtr.Zero;

                    if (hDrop != IntPtr.Zero && SetClipboardData(CF_HDROP, hDrop) != IntPtr.Zero)
                        hDrop = IntPtr.Zero;
                }
                finally
                {
                    CloseClipboard();
                }

                return true;
            }
            finally
            {
                // blocks the clipboard did not take
                if (hDib != IntPtr.Zero) GlobalFree(hDib);
                if (hPng != IntPtr.Zero) GlobalFree(hPng);
                if (hDrop != IntPtr.Zero) GlobalFree(hDrop);
            }
        }

        private static IntPtr AllocGlobal(byte[] data)
        {
            IntPtr h = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)data.Length);
            if (h == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr p = GlobalLock(h);
            if (p == IntPtr.Zero)
            {
                GlobalFree(h);
                return IntPtr.Zero;
            }
            Marshal.Copy(data, 0, p, data.Length);
            GlobalUnlock(h);
            return h;
        }

        // CF_DIB: BITMAPINFOHEADER + 24-bit bottom-up pixels. Pixels are copied row by row straight from the
        // lazily decoded PNG into the global block, so no full-size managed copy of the image is ever made.
        private static IntPtr BuildDib(byte[] png)
        {
            using var stream = new MemoryStream(png, writable: false);
            var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            var converted = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Bgr24, null, 0);

            int width = converted.PixelWidth;
            int height = converted.PixelHeight;
            int stride = (width * 3 + 3) & ~3;
            const int headerSize = 40;
            long total = headerSize + (long)stride * height;

            IntPtr h = GlobalAlloc(GMEM_MOVEABLE | GMEM_ZEROINIT, (UIntPtr)(ulong)total);
            if (h == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr p = GlobalLock(h);
            if (p == IntPtr.Zero)
            {
                GlobalFree(h);
                return IntPtr.Zero;
            }

            try
            {
                Marshal.WriteInt32(p, 0, headerSize);          // biSize
                Marshal.WriteInt32(p, 4, width);               // biWidth
                Marshal.WriteInt32(p, 8, height);              // biHeight (positive = bottom-up)
                Marshal.WriteInt16(p, 12, 1);                  // biPlanes
                Marshal.WriteInt16(p, 14, 24);                 // biBitCount
                Marshal.WriteInt32(p, 16, 0);                  // biCompression = BI_RGB
                Marshal.WriteInt32(p, 20, stride * height);    // biSizeImage

                for (int y = 0; y < height; y++)
                {
                    IntPtr row = p + headerSize + (height - 1 - y) * stride;
                    converted.CopyPixels(new Int32Rect(0, y, width, 1), row, stride, stride);
                }
            }
            catch
            {
                GlobalUnlock(h);
                GlobalFree(h);
                return IntPtr.Zero;
            }

            GlobalUnlock(h);
            return h;
        }

        // CF_HDROP: DROPFILES header (20 bytes) + wide, double-null terminated path list
        private static IntPtr BuildDropFiles(string path)
        {
            byte[] chars = System.Text.Encoding.Unicode.GetBytes(path + "\0\0");
            byte[] data = new byte[20 + chars.Length];
            BitConverter.GetBytes(20).CopyTo(data, 0);   // pFiles: offset of the file list
            BitConverter.GetBytes(1).CopyTo(data, 16);   // fWide: Unicode paths
            chars.CopyTo(data, 20);
            return AllocGlobal(data);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            if (_hwnd != IntPtr.Zero)
            {
                RemoveClipboardFormatListener(_hwnd);
                _hwndSource?.RemoveHook(WndProc);
            }
        }
    }
}
