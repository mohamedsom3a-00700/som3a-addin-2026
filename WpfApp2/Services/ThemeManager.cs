using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Som3a_WPF_UI.Services
{
    public static class ThemeManager
    {
        private static ThemeSettings _current = new ThemeSettings();

        public static void ApplyTheme(ThemeType theme)
        {
            if (Application.Current?.Resources == null)
            {
                _current.CurrentTheme = theme;
                SaveSettings();
                return;
            }

            var dicts = Application.Current.Resources.MergedDictionaries;
            var existing = dicts.FirstOrDefault(d =>
                d.Source?.ToString().Contains("FluentWhite") == true ||
                d.Source?.ToString().Contains("FluentDark") == true);

            if (existing != null)
                dicts.Remove(existing);

            if (theme == ThemeType.FluentWhite)
            {
                var whiteTheme = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Fluent/FluentWhite.xaml")
                };
                dicts.Add(whiteTheme);
            }

            _current.CurrentTheme = theme;
            SaveSettings();
        }

        public static void ChangeAccent(string hexColor)
        {
            if (Application.Current?.Resources == null) return;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                Application.Current.Resources["AccentColor"] = color;
                Application.Current.Resources["AccentBrush"] = new SolidColorBrush(color);
                _current.AccentColor = hexColor;
                SaveSettings();
            }
            catch { }
        }

        public static void SetResource(string key, object value)
        {
            if (Application.Current?.Resources != null)
                Application.Current.Resources[key] = value;
        }

        public static ThemeSettings GetCurrentSettings() => _current;

        public static void SaveSettings()
        {
            try
            {
                var dir = Path.GetDirectoryName(ThemeSettings.SettingsPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(ThemeSettings.SettingsPath,
                    JsonConvert.SerializeObject(_current, Formatting.Indented));
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(ThemeSettings.SettingsPath)) return;

                var json = File.ReadAllText(ThemeSettings.SettingsPath);
                _current = JsonConvert.DeserializeObject<ThemeSettings>(json)
                           ?? new ThemeSettings();

                ApplyTheme(_current.CurrentTheme);
                ChangeAccent(_current.AccentColor);
            }
            catch { _current = new ThemeSettings(); }
        }

        public static void SetHighPerformance(bool enabled)
        {
            _current.HighPerformance = enabled;

            if (Application.Current?.Resources == null) return;

            if (enabled)
            {
                SetResource("CardShadow", null);
                SetResource("FocusGlow", null);
                SetResource("WindowShadow", null);
            }
        }
    }
}