using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Som3a_WPF_UI.Models;
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
                try
                {
                    var themeDict = new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Som3a_WPF_UI;component/Theme/ThemeResources.xaml",
                            UriKind.Relative)
                    };
                    Application.Current.Resources.MergedDictionaries.Add(themeDict);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load ThemeResources: {ex.Message}");
                }
            }

            bool hasDarkTheme =
                Application.Current.Resources.MergedDictionaries.Any(
                    d => d.Source != null &&
                         d.Source.OriginalString.Contains("DarkTheme.xaml"));

            if (!hasDarkTheme)
            {
                try
                {
                    var darkDict = new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Som3a_WPF_UI;component/Theme/Dark/DarkTheme.xaml",
                            UriKind.Relative)
                    };
                    Application.Current.Resources.MergedDictionaries.Add(darkDict);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load DarkTheme: {ex.Message}");
                }
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
        private ILoggingService _logger;

        public string CurrentTheme => _currentTheme.ToString();
        public string CurrentAccentColor => _currentAccentColor;
        public bool IsFallbackActive { get; private set; }
        public FallbackManifest FallbackManifest { get; private set; } = new FallbackManifest();

        private ThemeManager()
        {
            try
            {
                _logger = App.Container?.Resolve<ILoggingService>();
            }
            catch
            {
            }
        }

        public FallbackManifest GetFallbackManifest() => FallbackManifest;

        public void SetLogger(ILoggingService logger)
        {
            _logger = logger;
        }

        public void ApplyTheme(string themeName, string accentColor = null)
        {
            if (Application.Current?.Resources == null)
                return;

            ApplyThemeInternal(themeName, accentColor);
        }

        private void ApplyThemeInternal(string themeName, string accentColor)
        {
            if (Application.Current?.Resources == null)
                return;

            _logger?.Log("INFO", "Theme", $"Applying theme: {themeName}", "ThemeManager");

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

            ResourceDictionary themeDict = null;
            var failedDictionaries = new List<string>();
            var failureReasons = new List<string>();
            bool loadSucceeded = false;

            try
            {
                themeDict = new ResourceDictionary { Source = themeUri };
                loadSucceeded = true;
            }
            catch (Exception ex)
            {
                _logger?.Log("ERROR", "Theme", $"Failed to load theme dictionary: {ex.Message}", "ThemeManager", ex.ToString());
                failedDictionaries.Add(themeUri.ToString());
                failureReasons.Add(ex.Message);
            }

            if (loadSucceeded && themeDict != null)
            {
                try
                {
                    if (existingTheme != null)
                    {
                        int index = dicts.IndexOf(existingTheme);
                        if (index >= 0)
                            dicts.RemoveAt(index);
                        dicts.Add(themeDict);
                    }
                    else
                    {
                        dicts.Add(themeDict);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Log("ERROR", "Theme", $"Dictionary replacement failed: {ex.Message}", "ThemeManager", ex.ToString());
                    failedDictionaries.Add(themeUri.ToString());
                    failureReasons.Add(ex.Message);
                    loadSucceeded = false;
                }
            }

            if (!loadSucceeded)
            {
                ActivateFallbackMode(failedDictionaries, failureReasons);
                return;
            }

            IsFallbackActive = false;
            FallbackManifest = new FallbackManifest();

            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    window.InvalidateVisual();
                    window.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                _logger?.Log("WARN", "Render", $"Window invalidation failed: {ex.Message}", "ThemeManager", ex.ToString());
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Window invalidation failed: {ex.Message}");
            }
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] Dictionaries Count: {dicts.Count}");

            _currentTheme = theme;

            if (ApplyAccentColor(effectiveAccent))
            {
                _currentAccentColor = effectiveAccent;
            }

            ValidateTokenIntegrity();

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

                ApplyThemeInternal(themeName, accent);
            }
            catch (Exception ex)
            {
                _logger?.Log("ERROR", "Theme", "Failed to load theme from settings, falling back to Dark", "ThemeManager", ex.ToString());
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
            catch (Exception ex)
            {
                _logger?.Log("ERROR", "Theme", $"Failed to save theme settings: {ex.Message}", "ThemeManager");
            }
        }

        public void ResetToDefault()
        {
            ApplyTheme("Dark", "#3A86FF");
        }

        public void ValidateTokenIntegrity()
        {
            if (Application.Current?.Resources == null) return;

            var requiredTokenKeys = new[]
            {
                "Primitive.Blue.500", "Primitive.Slate.900", "Primitive.White.95",
                "Brush.Background.Primary", "Brush.Text.Primary", "Brush.Accent.Primary",
                "Brush.Control.Background", "Brush.Button.Background",
                "Brush.ScrollBar.Thumb", "Brush.GroupBox.Background", "Brush.Overlay.Background",
                "Elevation.Card", "Elevation.Popup", "Elevation.Window",
                "Shadow.Card", "Shadow.Popup", "Shadow.Window",
                "AccentColorBrush", "AccentColorValue", "AccentColorLight"
            };

            foreach (var key in requiredTokenKeys)
            {
                var resource = Application.Current.Resources[key];
                if (resource == null)
                {
                    _logger?.Log("WARN", "Validation", $"Missing required token key: {key}", "ThemeManager");
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] WARNING: Missing required token key: {key}");
                }
            }

            var brushKeys = new[]
            {
                "Brush.Background.Primary", "Brush.Background.Secondary", "Brush.Background.Card",
                "Brush.Text.Primary", "Brush.Text.Secondary", "Brush.Text.Disabled",
                "Brush.Accent.Primary", "Brush.Accent.Success", "Brush.Accent.Warning", "Brush.Accent.Danger",
                "Brush.Stroke.Card", "Brush.Control.Background", "Brush.Control.Stroke",
                "Brush.Button.Background", "Brush.Button.HoverBackground", "Brush.Button.PressedBackground",
                "Brush.ComboBox.HoverBorder", "Brush.ComboBox.HoverBackground",
                "Brush.DataGrid.AlternatingRow", "Brush.DataGrid.SelectedRow"
            };

            foreach (var key in brushKeys)
            {
                var resource = Application.Current.Resources[key];
                if (resource == null)
                {
                    _logger?.Log("WARN", "Validation", $"Missing brush token: {key}", "ThemeManager");
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] WARNING: Missing brush token: {key}");
                }
            }
        }

        private void ActivateFallbackMode(List<string> failedDictionaries, List<string> failureReasons)
        {
            IsFallbackActive = true;
            FallbackManifest = new FallbackManifest
            {
                IsActive = true,
                FailedDictionaries = failedDictionaries.ToArray(),
                FailureReasons = failureReasons.ToArray(),
                ActivatedAt = DateTime.UtcNow,
                HardcodedResourcesCount = 6
            };

            LoadHardcodedFallbackTheme();

            _logger?.Log("ERROR", "Theme", $"Fallback mode activated. Failed dictionaries: {string.Join(", ", failedDictionaries)}", "ThemeManager");

            try
            {
                ToastService.Warning("Theme loading failed. Fallback mode activated.");
            }
            catch
            {
            }

            try
            {
                ThemeChanged?.Invoke(Instance, new ThemeChangedEventArgs("", "Fallback", "", "#000000"));
            }
            catch
            {
            }
        }

        private void LoadHardcodedFallbackTheme()
        {
            var dicts = Application.Current?.Resources?.MergedDictionaries;
            if (dicts == null) return;

            var existingTheme = dicts.FirstOrDefault(d =>
                d.Source?.ToString().Contains("/Theme/Dark/") == true ||
                d.Source?.ToString().Contains("/Theme/Light/") == true ||
                d.Source?.ToString().Contains("/Theme/Custom/") == true);

            if (existingTheme != null)
            {
                try
                {
                    dicts.Remove(existingTheme);
                }
                catch { }
            }

            var fallback = new ResourceDictionary();

            fallback["Brush.Background.Primary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(14, 23, 32));
            fallback["Brush.Background.Secondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 43, 58));
            fallback["Brush.Background.Card"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(21, 32, 43));
            fallback["Brush.Text.Primary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            fallback["Brush.Text.Secondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(191, 191, 191));
            fallback["Brush.Accent.Primary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 134, 255));
            fallback["AccentColor"] = System.Windows.Media.Color.FromRgb(58, 134, 255);
            fallback["AccentBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 134, 255));
            fallback["AccentColorValue"] = System.Windows.Media.Color.FromRgb(58, 134, 255);
            fallback["AccentColorBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 134, 255));
            fallback["WindowBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(14, 23, 32));
            fallback["TextMainBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            fallback["TextSubBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(191, 191, 191));

            try
            {
                dicts.Add(fallback);
            }
            catch (Exception ex)
            {
                _logger?.Log("FATAL", "Theme", $"Failed to load hardcoded fallback: {ex.Message}", "ThemeManager", ex.ToString());
            }
        }
    }
}