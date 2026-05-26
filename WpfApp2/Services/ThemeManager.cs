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
        private string _currentFontFamily = "Segoe UI";
        private string _backgroundImagePath = "";
        private double _backgroundBlurIntensity = 0.0;
        private bool _backgroundBlurEnabled = false;
        private string _windowBackdropStyle = "Solid";

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

        public static void FreezeResources()
        {
            if (Application.Current?.Resources?.MergedDictionaries == null) return;

            FreezeDictionaryRecursive(Application.Current.Resources);
        }

        private static void FreezeDictionaryRecursive(ResourceDictionary dict)
        {
            foreach (var key in dict.Keys)
            {
                if (dict[key] is Freezable freezable && !freezable.IsFrozen && freezable.CanFreeze)
                {
                    freezable.Freeze();
                }
            }

            foreach (var mergedDict in dict.MergedDictionaries)
            {
                FreezeDictionaryRecursive(mergedDict);
            }
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
                _logger?.Log("ERROR", "Theme", $"Failed to load theme dictionary: {ex.Message}", "ThemeManager", ex.ToString());
                ActivateFallbackMode(new List<string> { themeUri.ToString() }, new List<string> { ex.Message });
                return;
            }

            dicts.Add(themeDict);

            if (existingTheme != null)
            {
                try { dicts.Remove(existingTheme); } catch { }
            }

            // Swap MaterialDesign theme to match current theme
            var materialThemeUri = theme == AppTheme.Light
                ? new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml")
                : new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
            var existingMaterial = dicts.FirstOrDefault(d =>
                d.Source?.ToString().Contains("MaterialDesignTheme.") == true);
            if (existingMaterial != null)
            {
                try
                {
                    var idx = dicts.IndexOf(existingMaterial);
                    dicts[idx] = new ResourceDictionary { Source = materialThemeUri };
                }
                catch { }
            }
            else
            {
                try { dicts.Add(new ResourceDictionary { Source = materialThemeUri }); } catch { }
            }

            IsFallbackActive = false;

            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    window.InvalidateVisual();
                    window.UpdateLayout();
                }
            }
            catch { }

            _currentTheme = theme;

            if (ApplyAccentColor(effectiveAccent))
                _currentAccentColor = effectiveAccent;

            ValidateTokenIntegrity();

            ApplyBackground(_backgroundImagePath, _backgroundBlurIntensity);
            ApplyFont(_currentFontFamily);

            if (!string.IsNullOrEmpty(effectiveAccent))
            {
                try
                {
                    var baseColor = (Color)ColorConverter.ConvertFromString(effectiveAccent);
                    GenerateAccentVariants(baseColor);
                }
                catch { }
            }

            ValidateContrast();
            SaveCurrentTheme();

            foreach (Window window in Application.Current.Windows)
            {
                window.InvalidateVisual();
                window.UpdateLayout();
            }

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
                        if (newEffect.CanFreeze) newEffect.Freeze();
                        Application.Current.Resources[key] = newEffect;
                    }
                }
                FreezeResources();
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

                _windowBackdropStyle = Settings.Default.WindowBackdropStyle ?? "Solid";
                _backgroundImagePath = Settings.Default.BackgroundImagePath ?? "";
                _backgroundBlurIntensity = Settings.Default.BackgroundBlurIntensity;
                _backgroundBlurEnabled = Settings.Default.BackgroundBlurEnabled;
                _currentFontFamily = Settings.Default.SelectedFontFamily ?? "Segoe UI";

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
                Settings.Default.WindowBackdropStyle = _windowBackdropStyle;
                Settings.Default.BackgroundImagePath = _backgroundImagePath;
                Settings.Default.BackgroundBlurIntensity = _backgroundBlurIntensity;
                Settings.Default.BackgroundBlurEnabled = _backgroundBlurEnabled;
                Settings.Default.SelectedFontFamily = _currentFontFamily;
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

        private void RefreshCriticalResources()
        {
            if (Application.Current?.Resources == null) return;

            foreach (Window window in Application.Current.Windows)
            {
                window.ClearValue(Window.BackgroundProperty);
                window.ClearValue(Window.ForegroundProperty);
                window.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                window.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
                window.InvalidateVisual();
                window.UpdateLayout();
            }
        }

        public void ApplyBackground(string imagePath, double blurIntensity, bool? blurEnabled = null)
        {
            _backgroundImagePath = imagePath ?? "";
            _backgroundBlurIntensity = Math.Max(0.0, Math.Min(1.0, blurIntensity));
            if (blurEnabled.HasValue)
                _backgroundBlurEnabled = blurEnabled.Value;

            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Controls.ModernWindow modernWindow)
                    {
                        modernWindow.SetBackground(imagePath, blurIntensity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Log("WARN", "Background", $"Failed to apply background: {ex.Message}", "ThemeManager");
            }
        }

        public void ApplyFont(string fontFamilyName)
        {
            if (string.IsNullOrEmpty(fontFamilyName))
                fontFamilyName = "Segoe UI";

            _currentFontFamily = fontFamilyName;

            try
            {
                var fontFamily = new FontFamily(fontFamilyName);
                Application.Current.Resources["CustomFontFamily"] = fontFamily;
                SetResource("CustomFontFamily", fontFamily);
            }
            catch (Exception ex)
            {
                _logger?.Log("WARN", "Font", $"Failed to apply font '{fontFamilyName}', falling back to Segoe UI: {ex.Message}", "ThemeManager");
                try
                {
                    var fallbackFont = new FontFamily("Segoe UI");
                    Application.Current.Resources["CustomFontFamily"] = fallbackFont;
                    SetResource("CustomFontFamily", fallbackFont);
                }
                catch { }
            }
        }

        public void GenerateAccentVariants(Color baseColor)
        {
            try
            {
                var (h, s, l) = RgbToHsl(baseColor);

                var hover = HslToRgb(h, s, Math.Min(1.0, l + 0.10));
                var pressed = HslToRgb(h, s, Math.Max(0.0, l - 0.10));
                var glow = HslToRgb(h, Math.Min(1.0, s + 0.15), Math.Min(1.0, l + 0.05));
                var border = HslToRgb(h, Math.Max(0.0, s - 0.10), Math.Min(1.0, l + 0.20));
                var subtle = HslToRgb(h, Math.Max(0.0, s - 0.30), Math.Min(1.0, l + 0.30));

                Application.Current.Resources["Accent.Color.Hover"] = hover;
                Application.Current.Resources["Accent.Color.Pressed"] = pressed;
                Application.Current.Resources["Accent.Color.Glow"] = glow;
                Application.Current.Resources["Accent.Color.Border"] = border;
                Application.Current.Resources["Accent.Color.Subtle"] = subtle;

                Application.Current.Resources["Accent.Brush.Hover"] = new SolidColorBrush(hover);
                Application.Current.Resources["Accent.Brush.Pressed"] = new SolidColorBrush(pressed);
                Application.Current.Resources["Accent.Brush.Glow"] = new SolidColorBrush(glow);
                Application.Current.Resources["Accent.Brush.Border"] = new SolidColorBrush(border);
                Application.Current.Resources["Accent.Brush.Subtle"] = new SolidColorBrush(subtle);
            }
            catch (Exception ex)
            {
                _logger?.Log("ERROR", "Accent", $"Failed to generate accent variants: {ex.Message}", "ThemeManager");
            }
        }

        public void ValidateContrast()
        {
            if (Application.Current?.Resources == null) return;

            var tokenPairs = new (string Foreground, string Background, double RequiredRatio)[]
            {
                ("Brush.Text.Primary", "Brush.Background.Primary", 4.5),
                ("Brush.Text.Secondary", "Brush.Background.Primary", 4.5),
                ("Brush.Text.Primary", "Brush.Background.Card", 4.5),
                ("Brush.Accent.Primary", "Brush.Background.Primary", 3.0),
                ("Brush.Text.OnAccent", "Brush.Accent.Primary", 4.5),
            };

            foreach (var pair in tokenPairs)
            {
                var fg = Application.Current.Resources[pair.Foreground] as SolidColorBrush;
                var bg = Application.Current.Resources[pair.Background] as SolidColorBrush;

                if (fg == null || bg == null)
                {
                    _logger?.Log("WARN", "Contrast", $"Missing token for contrast check: {pair.Foreground} or {pair.Background}", "ThemeManager");
                    continue;
                }

                double ratio = ComputeContrastRatio(fg.Color, bg.Color);

                if (ratio < pair.RequiredRatio)
                {
                    _logger?.Log("WARN", "Contrast",
                        $"Contrast ratio {ratio:F2}:1 for {pair.Foreground} vs {pair.Background} " +
                        $"is below required {pair.RequiredRatio}:1", "ThemeManager");
                    System.Diagnostics.Debug.WriteLine(
                        $"[ThemeManager] WARNING: Contrast {ratio:F2}:1 for {pair.Foreground} vs {pair.Background}");
                }
            }
        }

        private static double ComputeContrastRatio(Color fg, Color bg)
        {
            double l1 = RelativeLuminance(fg);
            double l2 = RelativeLuminance(bg);
            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);
            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double RelativeLuminance(Color c)
        {
            double Linearize(double channel)
            {
                double v = channel / 255.0;
                return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Linearize(c.R) + 0.7152 * Linearize(c.G) + 0.0722 * Linearize(c.B);
        }

        private static (double H, double S, double L) RgbToHsl(Color c)
        {
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;
            if (delta > 0.0001)
            {
                if (max == r) h = 60 * (((g - b) / delta) % 6);
                else if (max == g) h = 60 * (((b - r) / delta) + 2);
                else if (max == b) h = 60 * (((r - g) / delta) + 4);
                if (h < 0) h += 360;
            }

            double l = (max + min) / 2.0;
            double s = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * l - 1));

            return (h / 360.0, s, l);
        }

        private static Color HslToRgb(double h, double s, double l)
        {
            h = Math.Max(0, Math.Min(1, h)) * 360;
            s = Math.Max(0, Math.Min(1, s));
            l = Math.Max(0, Math.Min(1, l));

            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r1, g1, b1;
            if (h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            return Color.FromRgb(
                (byte)Math.Round((r1 + m) * 255),
                (byte)Math.Round((g1 + m) * 255),
                (byte)Math.Round((b1 + m) * 255));
        }

        private static void CopyKeysToRoot(ResourceDictionary source)
        {
            if (Application.Current?.Resources == null || source == null) return;

            CopyDictRecursive(source, Application.Current.Resources);
        }

        private static void CopyDictRecursive(ResourceDictionary source, ResourceDictionary target)
        {
            foreach (var key in source.Keys)
            {
                try
                {
                    var value = source[key];
                    if (value != null && value is not System.Windows.Media.Effects.DropShadowEffect)
                    {
                        target[key] = value;
                    }
                }
                catch { }
            }

            for (int i = 0; i < source.MergedDictionaries.Count; i++)
            {
                CopyDictRecursive(source.MergedDictionaries[i], target);
            }
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
                if (!Application.Current.Resources.Contains(key))
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
                if (!Application.Current.Resources.Contains(key))
                {
                    _logger?.Log("WARN", "Validation", $"Missing brush token: {key}", "ThemeManager");
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] WARNING: Missing brush token: {key}");
                }
            }
        }

        private void ActivateFallbackMode(List<string> failedDictionaries, List<string> failureReasons)
        {
            IsFallbackActive = true;

            LoadHardcodedFallbackTheme();
            var fallbackCount = Application.Current?.Resources?.MergedDictionaries
                .LastOrDefault()?.Count ?? 0;

            FallbackManifest = new FallbackManifest
            {
                IsActive = true,
                FailedDictionaries = failedDictionaries.ToArray(),
                FailureReasons = failureReasons.ToArray(),
                ActivatedAt = DateTime.UtcNow,
                HardcodedResourcesCount = fallbackCount
            };

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

            var priorFallbacks = dicts
                .Where(d => d.Source == null && d.Contains("Brush.Background.Primary"))
                .ToList();
            foreach (var fb in priorFallbacks)
            {
                try { dicts.Remove(fb); } catch { }
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
                FreezeResources();
            }
            catch (Exception ex)
            {
                _logger?.Log("FATAL", "Theme", $"Failed to load hardcoded fallback: {ex.Message}", "ThemeManager", ex.ToString());
            }
        }
    }
}