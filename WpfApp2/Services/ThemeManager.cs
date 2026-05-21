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
        public static void InitializeApplicationResources()
        {
            if (Application.Current == null)
            {
                new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
            }

            bool hasThemeResources =
                Application.Current.Resources.MergedDictionaries.Any(
                    d => d.Source != null &&
                         d.Source.OriginalString.Contains("ThemeResources.xaml"));

            if (!hasThemeResources)
            {
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Som3a_WPF_UI;component/Theme/ThemeResources.xaml",
                            UriKind.Relative)
                    });
            }

            bool hasDarkTheme =
                Application.Current.Resources.MergedDictionaries.Any(
                    d => d.Source != null &&
                         d.Source.OriginalString.Contains("DarkTheme.xaml"));

            if (!hasDarkTheme)
            {
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Som3a_WPF_UI;component/Theme/Dark/DarkTheme.xaml",
                            UriKind.Relative)
                    });
            }
        }

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
            {
                case AppTheme.Light:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Light/LightTheme.xaml");
                    break;
                case AppTheme.Custom:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Custom/CustomTheme.xaml");
                    break;
                default:
                    themeUri = new Uri("pack://application:,,,/Som3a_WPF_UI;component/Theme/Dark/DarkTheme.xaml");
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
        }

        public bool ApplyAccentColor(string hexColor)
        {
            if (Application.Current?.Resources == null) return false;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                Application.Current.Resources["AccentColor"] = color;
                Application.Current.Resources["AccentBrush"] = new SolidColorBrush(color);
                Application.Current.Resources["AccentColorBrush"] = new SolidColorBrush(color);
                Application.Current.Resources["AccentColorValue"] = color;

                var lightColor = Color.FromArgb(
                    color.A,
                    (byte)Math.Min(255, color.R + 102),
                    (byte)Math.Min(255, color.G + 102),
                    (byte)Math.Min(255, color.B + 102));
                Application.Current.Resources["AccentColorLight"] = lightColor;

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
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetResource(string key, object value)
        {
            if (Application.Current?.Resources != null)
                Application.Current.Resources[key] = value;
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