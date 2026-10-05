namespace CoBu.Models
{
    public class AppSettings
    {
        public bool AlwaysOnTop { get; set; } = true;
        public string Hotkey { get; set; } = "Alt + V";
        public string AnimationStyle { get; set; } = "Juicy"; // "Juicy", "Fade", "None"
        public bool StartWithWindows { get; set; } = false;
        public int MaxItemsLimit { get; set; } = 100; // 0 = infinite limit, >0 = maximum items in history
    }
}
