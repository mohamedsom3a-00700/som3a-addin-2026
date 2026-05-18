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
                return;

            if (!Enum.TryParse<AppTheme>(themeName, true, out var theme))
                theme = AppTheme.Dark;

            var prevTheme = _currentTheme.ToString();
            var prevAccent = _currentAccentColor;

            if (theme == _currentTheme && string.IsNullOrEmpty(accentColor))
                return;

            var dicts = Application.Current.Resources.MergedDictionaries;

            var existingTheme = dicts.FirstOrDefault(d =>
                d.Source?.ToString().Contains("/Theme/Dark/") == true ||
                d.Source?.ToString().Contains("/Theme/Light/") == true ||
                d.Source?.ToString().Contains("/Theme/Custom/") == true);

            if (existingTheme != null)
                dicts.Remove(existingTheme);

            Uri themeUri;
            switch (theme)
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

            try
            {
                var themeDict = new ResourceDictionary { Source = themeUri };
                dicts.Add(themeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load theme: {ex.Message}");
                return;
            }

            _currentTheme = theme;

            if (!string.IsNullOrEmpty(accentColor))
            {
                _currentAccentColor = accentColor;
                ApplyAccentColor(accentColor);
            }

            SaveCurrentTheme();

            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(prevTheme, _currentTheme.ToString(), prevAccent, _currentAccentColor));
        }

        public void ApplyAccentColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                Application.Current.Resources["AccentColor"] = color;
                Application.Current.Resources["AccentBrush"] = new SolidColorBrush(color);
                Application.Current.Resources["AccentColorBrush"] = new SolidColorBrush(color);

                var glowKeys = new[] { "Glow.Focus", "Glow.ButtonHover", "Glow.Primary", "Glow.Selection", "Glow.Accent", "Glow.ThemeCard.Selected" };
                foreach (var key in glowKeys)
                {
                    if (Application.Current.Resources[key] is System.Windows.Media.Effects.DropShadowEffect effect)
                    {
                        var newEffect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = color,
                            BlurRadius = effect.BlurRadius,
                            ShadowDepth = effect.ShadowDepth,
                            Opacity = effect.Opacity,
                            Direction = effect.Direction
                        };
                        Application.Current.Resources[key] = newEffect;
                    }
                }
            }
            catch { }
        }

        public void LoadThemeFromSettings()
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