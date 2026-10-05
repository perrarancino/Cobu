using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CoBu.Models
{
    public enum ClipType
    {
        Text,
        Image
    }

    public class ClipItem : INotifyPropertyChanged
    {
        private bool _isPinned;
        private bool _isCopiedFeedback;

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        
        public ClipType Type { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [JsonIgnore]
        public string FormattedTime => Timestamp.Date == DateTime.Today
            ? $"Сегодня, {Timestamp:HH:mm:ss}"
            : Timestamp.ToString("dd.MM.yyyy HH:mm");

        /// <summary>
        /// Full text, kept in memory only for short clips. Long texts live in their .txt file
        /// (see <see cref="GetFullText"/>) so a big copy does not stay in RAM for good.
        /// </summary>
        public string ContentText { get; set; } = string.Empty;

        public string PreviewText { get; set; } = string.Empty;

        /// <summary>First lines of the text (newlines kept) shown on the card.</summary>
        public string? Snippet { get; set; }

        [JsonIgnore]
        public string CardText => !string.IsNullOrEmpty(Snippet) ? Snippet : MakeSnippet(ContentText.Length > 0 ? ContentText : PreviewText);

        public static string MakeSnippet(string text)
        {
            const int max = 300;
            string s = text.Replace("\r\n", "\n").Replace('\r', '\n');
            return s.Length > max ? s[..max] + "…" : s;
        }

        public string GetFullText()
        {
            if (ContentText.Length > 0)
                return ContentText;

            try
            {
                string path = FullFilePath;
                if (path.Length > 0 && File.Exists(path))
                    return File.ReadAllText(path);
            }
            catch { }

            return PreviewText;
        }

        public bool TextEquals(string text)
        {
            if (ContentText.Length > 0)
                return ContentText == text;

            return CharCount == text.Length && GetFullText() == text;
        }

        public string? RelativeFilePath { get; set; }

        public int CharCount { get; set; }
        public int LineCount { get; set; }

        public string? Dimensions { get; set; }
        public string? FileSize { get; set; }
        public string? ImageHash { get; set; }

        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (_isPinned != value)
                {
                    _isPinned = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AuraTheme));
                }
            }
        }

        private int _visualIndex = 0;

        [JsonIgnore]
        public int VisualIndex
        {
            get => _visualIndex;
            set
            {
                if (_visualIndex != value)
                {
                    _visualIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AuraTheme));
                }
            }
        }

        [JsonIgnore]
        public string AuraTheme
        {
            get
            {
                return VisualIndex switch
                {
                    0 => "Green0", // 1st element: 100% full vibrant electric lime (as now)
                    1 => "Green1", // 2nd element: ~65% green intensity
                    2 => "Green2", // 3rd element: ~40% green intensity
                    3 => "Green3", // 4th element: ~20% green intensity
                    _ => "Green4"  // 5th+ elements: ~10% green, deep midnight frosted glass
                };
            }
        }

        [JsonIgnore]
        public bool IsCopiedFeedback
        {
            get => _isCopiedFeedback;
            set
            {
                if (_isCopiedFeedback != value)
                {
                    _isCopiedFeedback = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public string DisplayTitle => Type switch
        {
            ClipType.Image => $"Изображение {Dimensions} ({FileSize})",
            ClipType.Text when !string.IsNullOrWhiteSpace(PreviewText) => 
                PreviewText.Length > 60 ? PreviewText[..60] + "..." : PreviewText,
            _ => "Текстовая заметка"
        };

        [JsonIgnore]
        public string FullFilePath => string.IsNullOrWhiteSpace(RelativeFilePath) 
            ? string.Empty 
            : Path.GetFullPath(Path.Combine(CoBu.Services.AppPaths.StorageRoot, RelativeFilePath));

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
