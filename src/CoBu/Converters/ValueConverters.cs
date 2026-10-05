using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CoBu.Models;
using CoBu.Services;

namespace CoBu.Converters
{
    public class ImageFileConverter : IValueConverter
    {
        // Small LRU cache of decoded thumbnails: the list is virtualized, so cards are recreated while scrolling
        private const int CacheSize = 24;
        private static readonly LinkedList<string> CacheOrder = new();
        private static readonly Dictionary<string, BitmapImage> Cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                if (Cache.TryGetValue(path, out var cached))
                {
                    CacheOrder.Remove(path);
                    CacheOrder.AddFirst(path);
                    return cached;
                }

                if (!File.Exists(path))
                    return null;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.DecodePixelHeight = 160;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    Cache[path] = bitmap;
                    CacheOrder.AddFirst(path);
                    while (CacheOrder.Count > CacheSize)
                    {
                        Cache.Remove(CacheOrder.Last!.Value);
                        CacheOrder.RemoveLast();
                    }
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ClipTypeToTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ClipType type)
            {
                return type switch
                {
                    ClipType.Image => Loc.T("type_image"),
                    ClipType.Text => Loc.T("type_text"),
                    _ => Loc.T("type_other")
                };
            }
            return Loc.T("type_other");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is Visibility v && v == Visibility.Visible) ? false : true;
        }
    }

    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = value switch
            {
                null => true,
                string s => string.IsNullOrWhiteSpace(s),
                _ => false
            };
            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AuraThemeToBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string theme = value as string ?? "Green0";
            if (theme == "Lime") theme = "Green0";
            string resourceKey = $"CardAura_{theme}";
            if (Application.Current?.Resources.Contains(resourceKey) == true)
            {
                return Application.Current.Resources[resourceKey];
            }
            return Application.Current?.Resources["CardAura_Green0"] ?? new SolidColorBrush(Color.FromRgb(22, 24, 34));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AuraThemeToRimConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string theme = value as string ?? "Green0";
            if (theme == "Lime") theme = "Green0";
            string resourceKey = $"CardGlassRim_{theme}";
            if (Application.Current?.Resources.Contains(resourceKey) == true)
            {
                return Application.Current.Resources[resourceKey];
            }
            return Application.Current?.Resources["CardGlassRim_Green0"] ?? new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AuraThemeToAccentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string theme = value as string ?? "Green0";
            return theme switch
            {
                "Green0" or "Lime" => new SolidColorBrush(Color.FromRgb(0, 230, 118)), // 1st element: Electric Neon Lime
                "Green1" => new SolidColorBrush(Color.FromRgb(20, 205, 105)),          // 2nd element: Vivid Emerald
                "Green2" => new SolidColorBrush(Color.FromRgb(55, 175, 100)),          // 3rd element: Forest Mint
                "Green3" => new SolidColorBrush(Color.FromRgb(80, 150, 110)),          // 4th element: Muted Sage
                "Green4" => new SolidColorBrush(Color.FromRgb(100, 140, 120)),         // 5th+ elements: Soft Dark Glass Accent
                _ => new SolidColorBrush(Color.FromRgb(0, 230, 118))
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
