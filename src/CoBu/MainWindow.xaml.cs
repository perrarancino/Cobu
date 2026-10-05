using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using CoBu.Models;
using CoBu.Services;

namespace CoBu
{
    public partial class MainWindow : Window
    {
        private readonly StorageService _storageService;
        private readonly SettingsService _settingsService;
        private readonly ClipboardMonitor _clipboardMonitor;
        private readonly HotkeyManager _hotkeyManager;
        private readonly TrayIconManager _trayIconManager;

        private ICollectionView _filteredView = null!;
        private string _currentFilterTag = "All";
        private bool _isAnimating;
        private bool _isInitialized;
        private readonly DispatcherTimer _trimTimer;

        public MainWindow()
        {
            // Settings first: the UI language must be known before the XAML is loaded
            _storageService = new StorageService();
            _settingsService = new SettingsService(_storageService.BaseStoragePath);
            Loc.Instance.SetLanguage(_settingsService.Current.Language);

            InitializeComponent();

            _clipboardMonitor = new ClipboardMonitor();
            _hotkeyManager = new HotkeyManager();
            _trayIconManager = new TrayIconManager();

            _trimTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _trimTimer.Tick += (s, e) =>
            {
                _trimTimer.Stop();
                // Only trim while hidden: a working-set trim of a visible window would cause page-fault hitches
                if (Visibility != Visibility.Visible)
                {
                    MemoryTrimmer.Trim();
                }
            };

            SetupCollectionView();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Starts the app in the tray: creates the window handle and Win32 hooks (clipboard, hotkey, tray icon)
        /// without ever showing the window. Used for autostart (<c>--tray</c>).
        /// </summary>
        public void StartHiddenInTray()
        {
            Visibility = Visibility.Hidden;
            EnsureInitialized();
            ScheduleTrim();
        }

        private void ScheduleTrim()
        {
            _trimTimer.Stop();
            _trimTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                return;

            EnsureInitialized();

            // Juicy slide-up from tray on initial launch
            ShowWithAnimation();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;
            _isInitialized = true;

            try
            {
                // Apply authentic Windows Acrylic / Backdrop Blur with rounded squircle clipping
                WindowBlurHelper.ApplyAcrylicBlur(this, 26);
            }
            catch { }

            // Apply settings
            Topmost = _settingsService.Current.AlwaysOnTop;
            HeaderHotkeyBadge.Text = _settingsService.Current.Hotkey;

            // Initialize Win32 hooks
            _clipboardMonitor.Initialize(this);
            _clipboardMonitor.TextCopied += OnTextCopiedAsync;
            _clipboardMonitor.ImageCopied += OnImageCopiedAsync;

            _hotkeyManager.Initialize(this, _settingsService.Current.Hotkey);
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;

            _trayIconManager.Initialize(this);
            _trayIconManager.TrayLeftClicked += OnTrayLeftClicked;
            _trayIconManager.TrayRightClicked += OnTrayRightClicked;
        }

        private void SetupCollectionView()
        {
            _filteredView = CollectionViewSource.GetDefaultView(_storageService.Items);
            _filteredView.Filter = FilterPredicate;

            // Sort: pinned first, then newest first
            _filteredView.SortDescriptions.Clear();
            _filteredView.SortDescriptions.Add(new SortDescription(nameof(ClipItem.IsPinned), ListSortDirection.Descending));
            _filteredView.SortDescriptions.Add(new SortDescription(nameof(ClipItem.Timestamp), ListSortDirection.Descending));

            ClipsList.ItemsSource = _filteredView;
            UpdateCountAndPlaceholder();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not ClipItem item)
                return false;

            return _currentFilterTag switch
            {
                "Text" => item.Type == ClipType.Text,
                "Image" => item.Type == ClipType.Image,
                "Pinned" => item.IsPinned,
                _ => true
            };
        }

        private void UpdateCountAndPlaceholder()
        {
            int index = 0;
            foreach (var obj in _filteredView)
            {
                if (obj is ClipItem item)
                {
                    item.VisualIndex = index++;
                }
            }

            EmptyPlaceholder.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task OnTextCopiedAsync(string text)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var item = await _storageService.SaveTextClipAsync(text, _settingsService.Current.MaxItemsLimit);
                _filteredView.Refresh();
                UpdateCountAndPlaceholder();
                ScheduleTrim();
            });
        }

        private async Task OnImageCopiedAsync(byte[] png, int width, int height)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var item = await _storageService.SaveImageClipAsync(png, width, height, _settingsService.Current.MaxItemsLimit);
                _filteredView.Refresh();
                UpdateCountAndPlaceholder();
                ScheduleTrim();
            });
        }

        private void OnHotkeyPressed()
        {
            Dispatcher.Invoke(ToggleVisibility);
        }

        private void OnTrayLeftClicked()
        {
            Dispatcher.Invoke(ToggleVisibility);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void OnTrayRightClicked(Point screenPoint)
        {
            Dispatcher.Invoke(() =>
            {
                if (Resources["TrayContextMenu"] is ContextMenu menu)
                {
                    menu.IsOpen = false;

                    var wih = new WindowInteropHelper(this);
                    SetForegroundWindow(wih.Handle);

                    menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                    menu.PlacementTarget = null;
                    menu.IsOpen = true;
                }
            });
        }

        private void CloseTrayContextMenu()
        {
            if (Resources["TrayContextMenu"] is ContextMenu menu)
            {
                menu.IsOpen = false;
            }
        }

        public void ToggleVisibility()
        {
            if (Visibility == Visibility.Visible && RootBorder.Opacity > 0.1)
            {
                HideWithAnimation();
            }
            else
            {
                SettingsView.Visibility = Visibility.Collapsed;
                ClipsView.Visibility = Visibility.Visible;
                ShowWithAnimation();
            }
        }

        public void ShowWithAnimation()
        {
            if (_isAnimating && Visibility == Visibility.Visible && RootBorder.Opacity > 0.8)
                return;

            // Reset any active animations so it never gets stuck
            BeginAnimation(TopProperty, null);
            RootBorder.BeginAnimation(UIElement.OpacityProperty, null);
            _isAnimating = false;

            var workArea = SystemParameters.WorkArea;
            double targetLeft = workArea.Right - Width - 14;
            double targetTop = workArea.Bottom - Height - 14;

            Left = targetLeft;
            Topmost = _settingsService.Current.AlwaysOnTop;
            // Explicit Show(): a window started in the tray was never shown, and Activate() throws before the first Show()
            Show();

            var wih = new WindowInteropHelper(this);
            if (wih.Handle != IntPtr.Zero)
            {
                SetForegroundWindow(wih.Handle);
            }

            string anim = _settingsService.Current.AnimationStyle;

            if (anim == "None")
            {
                Top = targetTop;
                RootBorder.Opacity = 1.0;
                Visibility = Visibility.Visible;
                Activate();
                Focus();
                return;
            }

            _isAnimating = true;

            if (anim.StartsWith("Fade") || anim.Contains("Fade"))
            {
                Top = targetTop;
                RootBorder.Opacity = 0.0;
                Visibility = Visibility.Visible;
                Activate();
                Focus();

                var fade = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                fade.Completed += (s, e) =>
                {
                    RootBorder.BeginAnimation(UIElement.OpacityProperty, null);
                    RootBorder.Opacity = 1.0;
                    _isAnimating = false;
                };
                RootBorder.BeginAnimation(UIElement.OpacityProperty, fade);
            }
            else // Juicy (slide from the tray) - signature smooth spring-loaded slide
            {
                // Physical window starts down near/in taskbar (+85px)
                Top = targetTop + 85;
                RootBorder.Opacity = 0.0;
                Visibility = Visibility.Visible;
                Activate();
                Focus();

                // 1. Physical window slides smoothly up
                var slide = new DoubleAnimation
                {
                    From = targetTop + 85,
                    To = targetTop,
                    Duration = TimeSpan.FromMilliseconds(340),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.24 }
                };

                // 2. Translucent opacity fades in
                var fade = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(260),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                slide.Completed += (s, e) =>
                {
                    BeginAnimation(TopProperty, null);
                    Top = targetTop;
                    RootBorder.BeginAnimation(UIElement.OpacityProperty, null);
                    RootBorder.Opacity = 1.0;
                    _isAnimating = false;
                };

                BeginAnimation(TopProperty, slide);
                RootBorder.BeginAnimation(UIElement.OpacityProperty, fade);
            }
        }

        public void HideWithAnimation()
        {
            if (_isAnimating || Visibility != Visibility.Visible)
                return;

            string anim = _settingsService.Current.AnimationStyle;

            if (anim == "None")
            {
                Visibility = Visibility.Hidden;
                ScheduleTrim();
                return;
            }

            _isAnimating = true;

            var workArea = SystemParameters.WorkArea;
            double currentTop = Top;
            double hideTop = currentTop + 70;

            var slide = new DoubleAnimation
            {
                From = currentTop,
                To = hideTop,
                Duration = TimeSpan.FromMilliseconds(190),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fade = new DoubleAnimation
            {
                From = RootBorder.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(190),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            slide.Completed += (s, e) =>
            {
                BeginAnimation(TopProperty, null);
                RootBorder.BeginAnimation(UIElement.OpacityProperty, null);
                _isAnimating = false;
                Visibility = Visibility.Hidden;
                ScheduleTrim();
            };

            BeginAnimation(TopProperty, slide);
            RootBorder.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void PositionWindowNearTray()
        {
            // Position directly above the notification tray area on primary monitor
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 14;
            Top = workArea.Bottom - Height - 14;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ClipItem item)
            {
                // Copy item to clipboard
                if (item.Type == ClipType.Text)
                {
                    _clipboardMonitor.CopyText(item.GetFullText());
                }
                else if (item.Type == ClipType.Image && !string.IsNullOrWhiteSpace(item.FullFilePath))
                {
                    _clipboardMonitor.CopyImage(item.FullFilePath);
                }

                // Show visual feedback on card
                item.IsCopiedFeedback = true;
                await Task.Delay(1200);
                item.IsCopiedFeedback = false;
            }
        }

        private async void ItemPin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ClipItem item)
            {
                item.IsPinned = !item.IsPinned;
                await _storageService.SaveHistoryAsync();
                _filteredView.Refresh();
                UpdateCountAndPlaceholder();
            }
            e.Handled = true;
        }

        private void ItemOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ClipItem item)
            {
                _storageService.OpenFileInDefaultApp(item);
            }
            e.Handled = true;
        }

        private async void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ClipItem item)
            {
                await _storageService.DeleteItemAsync(item);
                _filteredView.Refresh();
                UpdateCountAndPlaceholder();
            }
            e.Handled = true;
        }

        private void ItemDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ClipItem item)
            {
                if (item.Type == ClipType.Image && !string.IsNullOrWhiteSpace(item.FullFilePath) && File.Exists(item.FullFilePath))
                {
                    var sfd = new SaveFileDialog
                    {
                        Title = Loc.T("dlg_save_title"),
                        Filter = Loc.T("dlg_save_filter"),
                        DefaultExt = ".png",
                        FileName = $"CoBu_Image_{item.Timestamp:yyyyMMdd_HHmmss}.png"
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        try
                        {
                            File.Copy(item.FullFilePath, sfd.FileName, true);
                            item.IsCopiedFeedback = true;
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(1300);
                                Dispatcher.Invoke(() => item.IsCopiedFeedback = false);
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Loc.T("err_save", ex.Message), Loc.T("err_title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                _currentFilterTag = tag;
                _filteredView.Refresh();
                UpdateCountAndPlaceholder();
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            _storageService.OpenFolderInExplorer();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideWithAnimation();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (SettingsView.Visibility == Visibility.Visible)
                {
                    CloseSettings_Click(sender, e);
                }
                else
                {
                    HideWithAnimation();
                }
                e.Handled = true;
            }
        }

        // ==================== SETTINGS IN-WINDOW LOGIC ====================
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            CloseTrayContextMenu();

            if (Visibility != Visibility.Visible || RootBorder.Opacity < 0.2)
            {
                ShowWithAnimation();
            }

            SettingAlwaysOnTop.IsChecked = _settingsService.Current.AlwaysOnTop;
            SettingAutostart.IsChecked = _settingsService.Current.StartWithWindows;
            SettingMaxItems.Text = _settingsService.Current.MaxItemsLimit.ToString();

            // Set Hotkey combo selection
            string curHotkey = _settingsService.Current.Hotkey;
            foreach (ComboBoxItem item in SettingHotkeyCombo.Items)
            {
                if (item.Content?.ToString() == curHotkey)
                {
                    SettingHotkeyCombo.SelectedItem = item;
                    break;
                }
            }
            if (SettingHotkeyCombo.SelectedItem == null && SettingHotkeyCombo.Items.Count > 0)
                SettingHotkeyCombo.SelectedIndex = 0;

            // Set Animation combo selection (items carry a language-independent Tag)
            SelectComboByTag(SettingAnimationCombo, _settingsService.Current.AnimationStyle);

            // Set Language combo selection
            SelectComboByTag(SettingLanguageCombo, _settingsService.Current.Language);

            // Open settings directly inside main window
            ClipsView.Visibility = Visibility.Collapsed;
            SettingsView.Visibility = Visibility.Visible;
        }

        private static void SelectComboByTag(ComboBox combo, string? tag)
        {
            combo.SelectedItem = null;
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
            if (combo.SelectedItem == null && combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsView.Visibility = Visibility.Collapsed;
            ClipsView.Visibility = Visibility.Visible;
        }

        private void SettingMaxItems_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void SettingMaxItems_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!text.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Current.AlwaysOnTop = SettingAlwaysOnTop.IsChecked == true;
            _settingsService.Current.StartWithWindows = SettingAutostart.IsChecked == true;

            if (int.TryParse(SettingMaxItems.Text?.Trim(), out int maxLimit))
            {
                _settingsService.Current.MaxItemsLimit = Math.Max(0, maxLimit);
            }
            else
            {
                _settingsService.Current.MaxItemsLimit = 100;
            }

            if (SettingHotkeyCombo.SelectedItem is ComboBoxItem hotkeyItem)
            {
                _settingsService.Current.Hotkey = hotkeyItem.Content?.ToString() ?? "Alt + V";
            }

            if (SettingAnimationCombo.SelectedItem is ComboBoxItem animItem)
            {
                _settingsService.Current.AnimationStyle = animItem.Tag?.ToString() ?? "Juicy";
            }

            if (SettingLanguageCombo.SelectedItem is ComboBoxItem langItem)
            {
                _settingsService.Current.Language = langItem.Tag?.ToString() ?? Loc.Auto;
            }

            _settingsService.Save();

            // Apply immediately
            Loc.Instance.SetLanguage(_settingsService.Current.Language);
            _trayIconManager.UpdateTooltip();
            Topmost = _settingsService.Current.AlwaysOnTop;
            HeaderHotkeyBadge.Text = _settingsService.Current.Hotkey;
            _hotkeyManager.ApplyHotkey(_settingsService.Current.Hotkey);

            if (_settingsService.Current.MaxItemsLimit > 0)
            {
                await _storageService.EnforceLimitAsync(_settingsService.Current.MaxItemsLimit);
            }

            // Card texts that are computed in code (time, type, titles) must be re-read in the new language
            foreach (var clip in _storageService.Items)
            {
                clip.NotifyLanguageChanged();
            }
            _filteredView.Refresh();
            UpdateCountAndPlaceholder();

            SettingsView.Visibility = Visibility.Collapsed;
            ClipsView.Visibility = Visibility.Visible;
        }

        private void TrayContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    var hwndSource = (HwndSource)PresentationSource.FromVisual(menu);
                    if (hwndSource != null)
                    {
                        int width = (int)Math.Ceiling(menu.ActualWidth > 0 ? menu.ActualWidth : 260);
                        int height = (int)Math.Ceiling(menu.ActualHeight > 0 ? menu.ActualHeight : 180);
                        WindowBlurHelper.ApplyAcrylicBlurToHwnd(hwndSource.Handle, width, height, 16);
                    }
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void TrayMenu_Toggle_Click(object sender, RoutedEventArgs e)
        {
            CloseTrayContextMenu();
            ToggleVisibility();
        }

        private void TrayMenu_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CloseTrayContextMenu();
            _storageService.OpenFolderInExplorer();
        }

        private async void TrayMenu_Clear_Click(object sender, RoutedEventArgs e)
        {
            CloseTrayContextMenu();
            await _storageService.ClearAllAsync(keepPinned: true);
            _filteredView.Refresh();
            UpdateCountAndPlaceholder();
        }

        private void TrayMenu_Exit_Click(object sender, RoutedEventArgs e)
        {
            CloseTrayContextMenu();
            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Clean up Win32 hooks and resources
            _clipboardMonitor.Dispose();
            _hotkeyManager.Dispose();
            _trayIconManager.Dispose();
        }
    }
}