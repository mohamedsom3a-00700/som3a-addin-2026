using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Som3a_WPF_UI.Properties;

namespace Som3a_WPF_UI.Services
{
    public enum AppTheme
    {
        Dark,
        Light,
        Custom
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public string PreviousTheme { get; }
        public string NewTheme { get; }
        public string PreviousAccent { get; }
        public string NewAccent { get; }

        public ThemeChangedEventArgs(string prevTheme, string newTheme, string prevAccent, string newAccent)
        {
            PreviousTheme = prevTheme;
            NewTheme = newTheme;
            PreviousAccent = prevAccent;
            NewAccent = newAccent;
        }
    }

    public sealed class ThemeManager
    {
        private static readonly ThemeManager _instance = new ThemeManager();
        private static readonly object _lock = new object();

        public static ThemeManager Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance;
                }
            }
        }

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        private AppTheme _currentTheme = AppTheme.Dark;
        private string _currentAccentColor = "#3A86FF";

        public string CurrentTheme => _currentTheme.ToString();
        public string CurrentAccentColor => _currentAccentColor;

        private ThemeManager() { }

        public void ApplyTheme(string themeName, string accentColor = null)
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

<<<<<<< HEAD
            _current.CurrentTheme = theme;
            SaveSettings();
=======
            ResourceDictionary themeDict;
            try
            {
                themeDict = new ResourceDictionary { Source = themeUri };
            }
            catch (Exception ex)
            {
            {
                case AppTheme.Light:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Light/LightTheme.xaml");
                    break;
                case AppTheme.Custom:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Custom/CustomTheme.xaml");
                    break;
                default:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Dark/DarkTheme.xaml");
                    theme = AppTheme.Dark;
                    break;
            }

            ResourceDictionary themeDict;
            try
            {
                themeDict = new ResourceDictionary { Source = themeUri };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load theme: {ex.Message}");
                return;
            }

            if (existingTheme != null)
                dicts.Remove(existingTheme);

            dicts.Add(themeDict);

            _currentTheme = theme;

            if (!string.IsNullOrEmpty(accentColor))
            {
                if (ApplyAccentColor(accentColor))
                {
                    _currentAccentColor = accentColor;
                }
            }

            SaveCurrentTheme();

            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(prevTheme, _currentTheme.ToString(), prevAccent, _currentAccentColor));
>>>>>>> 872b4be (feat: Fluent Theme Engine - ScrollViewer fixes, ModernWindow shadow, accent color refactor)
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
                var themeName = Settings.Default.SelectedTheme;
                var accent = Settings.Default.AccentColor;

                if (string.IsNullOrEmpty(themeName))
                    themeName = "Dark";

                if (string.IsNullOrEmpty(accent))
                    accent = "#3A86FF";

                ApplyTheme(themeName, accent);
            }
            catch
            {
                ApplyTheme("Dark");
            }
        }

        public static void LoadSettings() => Instance.LoadThemeFromSettings();

        public void SaveCurrentTheme()
        {
            try
            {
                Settings.Default.SelectedTheme = _currentTheme.ToString();
                Settings.Default.AccentColor = _currentAccentColor;
                Settings.Default.Save();
            }
            catch { }
        }

        public void ResetToDefault()
        {
            ApplyTheme("Dark", "#3A86FF");
        }
    }
}