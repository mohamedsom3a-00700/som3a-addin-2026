using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
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
        private DispatcherTimer _debounceTimer;
        private string _pendingThemeName;
        private string _pendingAccentColor;

        public string CurrentTheme => _currentTheme.ToString();
        public string CurrentAccentColor => _currentAccentColor;

        private ThemeManager()
        {
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            _debounceTimer.Tick += OnDebounceTimerTick;
        }

        private void OnDebounceTimerTick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            if (_pendingThemeName != null)
            {
                ApplyThemeInternal(_pendingThemeName, _pendingAccentColor);
                _pendingThemeName = null;
                _pendingAccentColor = null;
            }
        }

        public void ApplyTheme(string themeName, string accentColor = null)
        {
            if (Application.Current?.Resources == null)
<<<<<<< HEAD
=======
                return;

            _pendingThemeName = themeName;
            _pendingAccentColor = accentColor;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void ApplyThemeInternal(string themeName, string accentColor)
        {
            if (Application.Current?.Resources == null)
                return;

            if (!Enum.TryParse<AppTheme>(themeName, true, out var theme))
                theme = AppTheme.Dark;

            var prevTheme = _currentTheme.ToString();
            var prevAccent = _currentAccentColor;

            if (theme == _currentTheme && string.IsNullOrEmpty(accentColor))
                return;

            var effectiveAccent = !string.IsNullOrEmpty(accentColor) ? accentColor : _currentAccentColor;

            var dicts = Application.Current.Resources.MergedDictionaries;

            var existingTheme = dicts.FirstOrDefault(d =>
                d.Source?.ToString().Contains("/Theme/Dark/") == true ||
                d.Source?.ToString().Contains("/Theme/Light/") == true ||
                d.Source?.ToString().Contains("/Theme/Custom/") == true);

            Uri themeUri;
            switch (theme)
>>>>>>> 0cd6593 (feat(themes): Update Themes Manager - fix bugs and add semantic tokens)
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
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load theme: {ex.Message}");
                return;
            }

            ResourceDictionary removedDict = null;
            try
            {
                if (existingTheme != null)
                {
                    removedDict = existingTheme;
                    dicts.Remove(existingTheme);
                }

                dicts.Add(themeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Dictionary replacement failed: {ex.Message}");
                try
                {
                    if (removedDict != null)
                        dicts.Add(removedDict);
                }
                catch { }
                return;
            }

            _currentTheme = theme;

            if (ApplyAccentColor(effectiveAccent))
            {
                _currentAccentColor = effectiveAccent;
            }

            SaveCurrentTheme();

<<<<<<< HEAD
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(prevTheme, _currentTheme.ToString(), prevAccent, _currentAccentColor));
>>>>>>> 872b4be (feat: Fluent Theme Engine - ScrollViewer fixes, ModernWindow shadow, accent color refactor)
=======
            var handler = ThemeChanged;
            if (handler != null && Application.Current?.Dispatcher != null)
            {
                try
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        handler?.Invoke(Instance, new ThemeChangedEventArgs(prevTheme, _currentTheme.ToString(), prevAccent, _currentAccentColor));
                    }, DispatcherPriority.Background);
                }
                catch
                {
                    handler?.Invoke(Instance, new ThemeChangedEventArgs(prevTheme, _currentTheme.ToString(), prevAccent, _currentAccentColor));
                }
            }
>>>>>>> 0cd6593 (feat(themes): Update Themes Manager - fix bugs and add semantic tokens)
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

                _debounceTimer.Stop();
                _pendingThemeName = null;
                _pendingAccentColor = null;
                ApplyThemeInternal(themeName, accent);
            }
            catch
            {
                ApplyThemeInternal("Dark", "#3A86FF");
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