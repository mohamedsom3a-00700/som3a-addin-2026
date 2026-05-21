using System;
using System.IO;

namespace Som3a_WPF_UI.Services
{
    public class ThemeSettings
    {
        public AppTheme CurrentTheme { get; set; } = AppTheme.Dark;
        public string AccentColor { get; set; } = "#3A86FF";
        public bool HighPerformance { get; set; } = false;

        public static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "theme.json");
    }
}