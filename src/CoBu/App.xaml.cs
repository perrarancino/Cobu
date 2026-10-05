using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace CoBu
{
    public partial class App : Application
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const string MutexName = @"Local\CoBu_SingleInstance_Mutex_2026";
        private const string EventName = @"Local\CoBu_ShowWindow_Event_2026";

        private static Mutex? _mutex;
        private static EventWaitHandle? _showEvent;
        private static bool _isMutexOwner;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
                try
                {
                    string path = CoBu.Services.AppPaths.CrashLogPath;
                    File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] [AppDomain] {ev.ExceptionObject}\r\n");
                }
                catch { }
            };

            DispatcherUnhandledException += (s, ev) =>
            {
                try
                {
                    string path = CoBu.Services.AppPaths.CrashLogPath;
                    File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] [Dispatcher] {ev.Exception}\r\n");
                }
                catch { }

                // A tray app that silently lives all day should survive a UI glitch (it is logged to crash.log)
                ev.Handled = true;
            };

            try
            {
                _mutex = new Mutex(true, MutexName, out bool createdNew);
                if (createdNew)
                {
                    _isMutexOwner = true;
                }
                else
                {
                    _isMutexOwner = _mutex.WaitOne(TimeSpan.Zero, true);
                }
            }
            catch (AbandonedMutexException)
            {
                _isMutexOwner = true;
            }
            catch
            {
                _isMutexOwner = true;
            }

            // "--tray": silent start (autostart) - live in the tray, do not pop the window up
            bool startInTray = e.Args.Any(a => string.Equals(a, "--tray", StringComparison.OrdinalIgnoreCase));

            if (!_isMutexOwner)
            {
                // A silent autostart launch must not pop up the window of the already running instance
                if (!startInTray)
                {
                    try
                    {
                        using var existingEvent = EventWaitHandle.OpenExisting(EventName);
                        existingEvent.Set();
                    }
                    catch { }

                    try
                    {
                        IntPtr hwnd = FindWindow(null, "CoBu");
                        if (hwnd != IntPtr.Zero)
                        {
                            SetForegroundWindow(hwnd);
                        }
                    }
                    catch { }
                }

                Shutdown();
                return;
            }

            try
            {
                _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    while (_showEvent != null && !_showEvent.SafeWaitHandle.IsClosed)
                    {
                        try
                        {
                            if (_showEvent.WaitOne())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    if (MainWindow is MainWindow mw)
                                    {
                                        mw.ShowWithAnimation();
                                    }
                                });
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                });
            }
            catch { }

            base.OnStartup(e);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            if (startInTray)
            {
                mainWindow.StartHiddenInTray();
            }
            else
            {
                mainWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_isMutexOwner && _mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                }
                catch { }
            }

            try
            {
                _showEvent?.Dispose();
            }
            catch { }

            base.OnExit(e);
        }
    }
}
