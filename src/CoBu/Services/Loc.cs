using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace CoBu.Services
{
    /// <summary>
    /// UI strings in English and Russian. The language comes from the Windows UI language ("Auto")
    /// or from the user's choice in Settings, and can be switched at runtime: XAML uses <c>{loc:T key}</c>,
    /// which binds to this class, so every bound text updates immediately.
    /// </summary>
    public sealed class Loc : INotifyPropertyChanged
    {
        public static Loc Instance { get; } = new();

        public const string Auto = "Auto";

        private Dictionary<string, string> _current = English;

        /// <summary>The language in use right now: "en" or "ru".</summary>
        public string Language { get; private set; } = "en";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key]
        {
            get
            {
                if (_current.TryGetValue(key, out var value)) return value;
                return English.TryGetValue(key, out var fallback) ? fallback : key;
            }
        }

        public static string T(string key) => Instance[key];

        public static string T(string key, params object[] args) => string.Format(Instance[key], args);

        /// <summary>Maps the stored setting ("Auto", "en", "ru") to a concrete language.</summary>
        public static string Resolve(string? setting)
        {
            if (string.Equals(setting, "ru", StringComparison.OrdinalIgnoreCase)) return "ru";
            if (string.Equals(setting, "en", StringComparison.OrdinalIgnoreCase)) return "en";
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "ru" : "en";
        }

        public void SetLanguage(string? setting)
        {
            string language = Resolve(setting);
            if (language == Language && ReferenceEquals(_current, language == "ru" ? Russian : English))
                return;

            Language = language;
            _current = language == "ru" ? Russian : English;

            // "Item[]" tells every {loc:T ...} binding to re-read its string
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }

        private static readonly Dictionary<string, string> English = new()
        {
            ["tray_toggle"] = "Show / Hide",
            ["tray_folder"] = "Storage folder",
            ["tray_settings"] = "Settings",
            ["tray_exit"] = "Exit",
            ["tray_tip"] = "CoBu — Smart clipboard (Alt+V)",

            ["tip_open_folder"] = "Open the folder with saved files",
            ["tip_settings"] = "Settings",
            ["tip_hide"] = "Hide (Alt+V)",
            ["tip_back"] = "Back to clipboard history (Esc)",
            ["tip_hide_window"] = "Hide window (Alt+V)",
            ["tip_pin"] = "Pin to top",
            ["tip_download"] = "Save to a chosen folder...",
            ["tip_open_file"] = "Open the saved file",
            ["tip_delete"] = "Delete from history",

            ["filter_all"] = "All",
            ["filter_text"] = "Text",
            ["filter_images"] = "Images",
            ["filter_pinned"] = "Pinned",

            ["empty_title"] = "History is empty",
            ["empty_hint"] = "Copy some text or an image (Ctrl + C),\nand it will be saved here automatically!",
            ["copied"] = "Copied to clipboard!",

            ["settings_title"] = "CoBu settings",
            ["set_language"] = "Language",
            ["set_language_desc"] = "Interface language",
            ["lang_auto"] = "Automatic (Windows language)",
            ["lang_en"] = "English",
            ["lang_ru"] = "Русский",
            ["set_ontop"] = "Always on top",
            ["set_ontop_desc"] = "The window is never covered by other apps",
            ["set_hotkey"] = "Show / hide shortcut",
            ["set_hotkey_desc"] = "Opens and hides the window from anywhere",
            ["set_anim"] = "Window opening style",
            ["set_anim_desc"] = "Smooth slide from the tray or a soft fade",
            ["anim_juicy"] = "Juicy (slide from the tray)",
            ["anim_fade"] = "Smooth fade",
            ["anim_none"] = "No animation",
            ["set_autostart"] = "Start with Windows",
            ["set_autostart_desc"] = "Start CoBu silently in the tray at system startup",
            ["set_limit"] = "History limit",
            ["set_limit_desc"] = "Maximum number of entries (0 = unlimited)",
            ["set_save"] = "Save settings",

            ["type_image"] = "Image",
            ["type_text"] = "Text",
            ["type_other"] = "Data",
            ["today"] = "Today, {0}",
            ["title_image"] = "Image {0} ({1})",
            ["title_note"] = "Text note",

            ["dlg_save_title"] = "Save image to a folder",
            ["dlg_save_filter"] = "PNG image (*.png)|*.png|All files (*.*)|*.*",
            ["err_save"] = "Could not save the file: {0}",
            ["err_title"] = "Error",
        };

        private static readonly Dictionary<string, string> Russian = new()
        {
            ["tray_toggle"] = "Показать / Скрыть",
            ["tray_folder"] = "Папка с файлами",
            ["tray_settings"] = "Настройки",
            ["tray_exit"] = "Выход",
            ["tray_tip"] = "CoBu — Умный буфер обмена (Alt+V)",

            ["tip_open_folder"] = "Открыть папку с сохранёнными файлами",
            ["tip_settings"] = "Настройки",
            ["tip_hide"] = "Скрыть (Alt+V)",
            ["tip_back"] = "Назад к истории буфера (Esc)",
            ["tip_hide_window"] = "Скрыть окно (Alt+V)",
            ["tip_pin"] = "Закрепить вверху",
            ["tip_download"] = "Скачать в выбранную папку...",
            ["tip_open_file"] = "Открыть сохранённый файл",
            ["tip_delete"] = "Удалить из истории",

            ["filter_all"] = "Все",
            ["filter_text"] = "Текст",
            ["filter_images"] = "Картинки",
            ["filter_pinned"] = "Закреплённые",

            ["empty_title"] = "История пока пуста",
            ["empty_hint"] = "Скопируйте текст или изображение (Ctrl + C),\nи они автоматически сохранятся сюда!",
            ["copied"] = "Скопировано в буфер обмена!",

            ["settings_title"] = "Настройки CoBu",
            ["set_language"] = "Язык",
            ["set_language_desc"] = "Язык интерфейса",
            ["lang_auto"] = "Автоматически (язык Windows)",
            ["lang_en"] = "English",
            ["lang_ru"] = "Русский",
            ["set_ontop"] = "Поверх всех приложений",
            ["set_ontop_desc"] = "Окно не перекрывается другими программами",
            ["set_hotkey"] = "Комбинация клавиш вызова",
            ["set_hotkey_desc"] = "Открывает и скрывает окно из любого места",
            ["set_anim"] = "Стиль открытия окна",
            ["set_anim_desc"] = "Плавный выезд из трея или мягкий переход",
            ["anim_juicy"] = "Juicy (Выезд из трея)",
            ["anim_fade"] = "Плавный Fade",
            ["anim_none"] = "Без анимации",
            ["set_autostart"] = "Автозапуск с Windows",
            ["set_autostart_desc"] = "Запускать CoBu тихо в трее при старте системы",
            ["set_limit"] = "Лимит истории буфера",
            ["set_limit_desc"] = "Максимум записей (0 — без ограничений)",
            ["set_save"] = "Сохранить настройки",

            ["type_image"] = "Картинка",
            ["type_text"] = "Текст",
            ["type_other"] = "Данные",
            ["today"] = "Сегодня, {0}",
            ["title_image"] = "Изображение {0} ({1})",
            ["title_note"] = "Текстовая заметка",

            ["dlg_save_title"] = "Сохранить изображение в выбранную папку",
            ["dlg_save_filter"] = "Изображение PNG (*.png)|*.png|Все файлы (*.*)|*.*",
            ["err_save"] = "Не удалось сохранить файл: {0}",
            ["err_title"] = "Ошибка",
        };
    }

    /// <summary>XAML: <c>Text="{loc:T tray_exit}"</c>, a live-updating localized string.</summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class TExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public TExtension() { }

        public TExtension(string key) => Key = key;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding("[" + Key + "]")
            {
                Source = Loc.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
