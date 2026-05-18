using System;
using System.IO;

namespace Som3a_WPF_UI.Services
{
    public enum ThemeType
    {
        FluentDarkBlue,
        FluentWhite
    }

    public class ThemeSettings
    {
        public ThemeType CurrentTheme { get; set; } = ThemeType.FluentDarkBlue;
        public string AccentColor { get; set; } = "#3A86FF";
        public bool HighPerformance { get; set; } = false;

        public static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "theme.json");
    }
}