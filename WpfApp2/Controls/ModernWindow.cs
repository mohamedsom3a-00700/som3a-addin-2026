using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace Som3a_WPF_UI.Controls
{
    public enum WindowBackdrop
    {
        Solid,
        Gradient,
        Image
    }

    public class ModernWindow : Window
    {
        private bool _useSafeMode;
        private bool _isHighContrast;
        private ResourceDictionary _originalThemeResources;

        //static ModernWindow()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(
        //        typeof(ModernWindow),
        //        new FrameworkPropertyMetadata(typeof(ModernWindow)));
        //}

        public ModernWindow()
        {
            Title = "Planova Platform";
            InitializeWindow();
            SetupCommands();
            LoadThemeFromSettings();
            SetDefaultIcon();
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }

        private void LoadThemeFromSettings()
        {
            try
            {
                ThemeManager.Instance.LoadThemeFromSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModernWindow] LoadThemeFromSettings failed: {ex.Message}");
            }
        }

        private void SetDefaultIcon()
        {
            try
            {
                Icon = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Som3a_WPF_UI;component/Assets/Branding/Logos/ICO/Logo-Dark.ico"));
            }
            catch
            {
            }
        }

        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            SetTitleIconFromTheme();
        }

        private void SetTitleIconFromTheme()
        {
            var logoSource = TryFindResource("Logo.ImageSource") as ImageSource;
            if (logoSource != null)
            {
                TitleIcon = logoSource;
            }
        }

        private void InitializeWindow()
        {
            var detectedMode = WindowRenderModeDetector.DetectOptimalMode();
            var currentMode = detectedMode == WindowRenderMode.WindowChrome
                ? Services.RenderMode.WindowChrome
                : Services.RenderMode.FallbackSafe;
            SetValue(RenderModeProperty, currentMode);

            _useSafeMode = detectedMode == WindowRenderMode.FallbackSafe;
            SetValue(IsSafeModePropertyKey, _useSafeMode);

            if (_useSafeMode)
            {
                WindowChromeHelper.ApplyWindowChrome(this, true);
                AllowsTransparency = false;
                ApplySafeModeShadowOverrides();
            }
            else
            {
                WindowChromeHelper.ApplyWindowChrome(this, false);
            }

            SetValue(DpiScaleProperty, Helpers.DpiHelper.GetCurrentDpiScale(this));

            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            ApplyHighContrastMode();

            Loaded += OnLoaded;
            StateChanged += OnStateChanged;
            Closing += OnClosing;

            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void ApplyHighContrastMode()
        {
            _isHighContrast = SystemParameters.HighContrast;
            if (!_isHighContrast)
                return;

            Resources["Brush.Background.Primary"] = SystemColors.WindowBrush;
            Resources["Brush.Background.Secondary"] = SystemColors.WindowBrush;
            Resources["Brush.Background.Card"] = SystemColors.WindowBrush;
            Resources["Brush.Text.Primary"] = SystemColors.WindowTextBrush;
            Resources["Brush.Text.Secondary"] = SystemColors.WindowTextBrush;
            Resources["Brush.Accent.Primary"] = SystemColors.HighlightBrush;
            Resources["Brush.Button.Background"] = SystemColors.ControlBrush;
            Resources["Brush.Button.HoverBackground"] = SystemColors.ControlLightBrush;
            Resources["Brush.Button.PressedBackground"] = SystemColors.ControlDarkBrush;
            Resources["Brush.Control.Background"] = SystemColors.WindowBrush;
            Resources["Brush.Control.Stroke"] = SystemColors.WindowTextBrush;
        }

        private void SetupAnimations()
        {
            Opacity = 0;
        }

        private void SetupCommands()
        {
        }

        private void ApplySafeModeShadowOverrides()
        {
            var safeOverrides = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Shadow.Window", "Shadow.Window.Safe" },
                { "Shadow.Card", "Shadow.Card.Safe" },
                { "Shadow.Popup", "Shadow.Popup.Safe" },
                { "Shadow.Large", "Shadow.Large.Safe" },
                { "Elevation.Window", "Elevation.Window.Safe" },
                { "Elevation.Card", "Elevation.Card.Safe" },
                { "Elevation.Popup", "Elevation.Popup.Safe" },
                { "Elevation.Large", "Elevation.Large.Safe" }
            };

            foreach (var kvp in safeOverrides)
            {
                var safeEffect = TryFindResource(kvp.Value) as DropShadowEffect;
                if (safeEffect != null)
                {
                    Resources[kvp.Key] = safeEffect;
                }
            }
        }

        private static bool IsReducedMotionEnabled =>
            !SystemParameters.ClientAreaAnimation;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetTitleIconFromTheme();
            if (!_useSafeMode && !IsReducedMotionEnabled)
            {
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(OpacityProperty, fadeIn);
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_useSafeMode || IsReducedMotionEnabled)
                return;

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CloseOnEscape)
            {
                Close();
            }
        }

        public static readonly DependencyProperty RenderModeProperty =
            DependencyProperty.Register(
                nameof(RenderMode),
                typeof(Services.RenderMode),
                typeof(ModernWindow),
                new PropertyMetadata(Services.RenderMode.FallbackSafe));

        public Services.RenderMode RenderMode
        {
            get => (Services.RenderMode)GetValue(RenderModeProperty);
            set => SetValue(RenderModeProperty, value);
        }

        private static readonly DependencyPropertyKey IsSafeModePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsSafeMode),
                typeof(bool),
                typeof(ModernWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSafeModeProperty = IsSafeModePropertyKey.DependencyProperty;

        public bool IsSafeMode
        {
            get => (bool)GetValue(IsSafeModeProperty);
            private set => SetValue(IsSafeModePropertyKey, value);
        }

        public static readonly DependencyProperty WindowBackdropProperty =
            DependencyProperty.Register(
                nameof(WindowBackdrop),
                typeof(WindowBackdrop),
                typeof(ModernWindow),
                new PropertyMetadata(WindowBackdrop.Solid));

        public WindowBackdrop WindowBackdrop
        {
            get => (WindowBackdrop)GetValue(WindowBackdropProperty);
            set => SetValue(WindowBackdropProperty, value);
        }

        public static readonly DependencyProperty DpiScaleProperty =
            DependencyProperty.Register(
                nameof(DpiScale),
                typeof(double),
                typeof(ModernWindow),
                new PropertyMetadata(1.0));

        public double DpiScale
        {
            get => (double)GetValue(DpiScaleProperty);
            set => SetValue(DpiScaleProperty, value);
        }

        public static readonly DependencyProperty CloseOnEscapeProperty =
            DependencyProperty.Register(
                nameof(CloseOnEscape),
                typeof(bool),
                typeof(ModernWindow),
                new PropertyMetadata(true));

        public bool CloseOnEscape
        {
            get => (bool)GetValue(CloseOnEscapeProperty);
            set => SetValue(CloseOnEscapeProperty, value);
        }

        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register(
                nameof(TitleBarHeight),
                typeof(double),
                typeof(ModernWindow),
                new PropertyMetadata(40.0));

        public double TitleBarHeight
        {
            get => (double)GetValue(TitleBarHeightProperty);
            set => SetValue(TitleBarHeightProperty, value);
        }

        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register(
                nameof(ShowMinimizeButton),
                typeof(bool),
                typeof(ModernWindow),
                new PropertyMetadata(true));

        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register(
                nameof(ShowMaximizeButton),
                typeof(bool),
                typeof(ModernWindow),
                new PropertyMetadata(true));

        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(
                nameof(ShowCloseButton),
                typeof(bool),
                typeof(ModernWindow),
                new PropertyMetadata(true));

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        public static readonly DependencyProperty TitleIconProperty =
            DependencyProperty.Register(
                nameof(TitleIcon),
                typeof(object),
                typeof(ModernWindow),
                new PropertyMetadata(null));

        public object TitleIcon
        {
            get => GetValue(TitleIconProperty);
            set => SetValue(TitleIconProperty, value);
        }

        public ICommand MinimizeCommand => new RelayCommand(
            () => WindowState = WindowState.Minimized,
            () => ShowMinimizeButton && WindowState != WindowState.Minimized);

        public ICommand MaximizeCommand => new RelayCommand(
            () => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized,
            () => ShowMaximizeButton);

        public ICommand CloseCommand => new RelayCommand(
            () => Close(),
            () => ShowCloseButton);

        public ICommand RestoreCommand => new RelayCommand(
            () => WindowState = WindowState.Normal,
            () => WindowState == WindowState.Maximized);

        public ICommand ThemeToggleCommand => new RelayCommand(() =>
        {
            var current = ThemeManager.Instance.CurrentTheme;
            if (current == "Dark")
                ThemeManager.Instance.ApplyTheme("Light");
            else
                ThemeManager.Instance.ApplyTheme("Dark");
        });

        public void SetBackground(string imagePath)
        {
            SetBackground(imagePath, 0.0);
        }

        public void SetBackground(string imagePath, double blurIntensity)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            var hwnd = helper.Handle;

            if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
            {
                Background = TryFindResource("Brush.Background.SolidFallback") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(14, 23, 32));
                DisableDwmBlur(hwnd);
                return;
            }

            try
            {
                var ext = System.IO.Path.GetExtension(imagePath).ToLowerInvariant();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".bmp")
                {
                    Background = TryFindResource("Brush.Background.SolidFallback") as Brush
                        ?? new SolidColorBrush(Color.FromRgb(14, 23, 32));
                    DisableDwmBlur(hwnd);
                    return;
                }

                var fileInfo = new System.IO.FileInfo(imagePath);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    Background = TryFindResource("Brush.Background.SolidFallback") as Brush
                        ?? new SolidColorBrush(Color.FromRgb(14, 23, 32));
                    DisableDwmBlur(hwnd);
                    return;
                }

                var img = new System.Windows.Media.Imaging.BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(imagePath);
                img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();

                if (img.PixelWidth > 4096 || img.PixelHeight > 4096)
                {
                    Background = TryFindResource("Brush.Background.SolidFallback") as Brush
                        ?? new SolidColorBrush(Color.FromRgb(14, 23, 32));
                    DisableDwmBlur(hwnd);
                    return;
                }

                var clampedBlur = Math.Max(0.0, Math.Min(1.0, blurIntensity));
                ImageSource imageSource = img;

                if (clampedBlur > 0.0)
                {
                    imageSource = RenderBlurredBitmap(img, clampedBlur * 20.0);
                }

                var brush = new ImageBrush(imageSource)
                {
                    Stretch = System.Windows.Media.Stretch.Fill,
                    AlignmentX = System.Windows.Media.AlignmentX.Center,
                    AlignmentY = System.Windows.Media.AlignmentY.Center
                };

                Background = brush;

                if (hwnd != IntPtr.Zero && hwnd != new IntPtr(-1))
                {
                    DwmBlurService.EnableBlur(hwnd, 0.3);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModernWindow] Failed to set background: {ex.Message}");
                Background = TryFindResource("Brush.Background.SolidFallback") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(14, 23, 32));
                DisableDwmBlur(hwnd);
            }
        }

        private void DisableDwmBlur(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero && hwnd != new IntPtr(-1))
            {
                DwmBlurService.DisableBlur(hwnd);
            }
        }

        private static ImageSource RenderBlurredBitmap(System.Windows.Media.Imaging.BitmapImage source, double radius)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            }

            var tempRender = new RenderTargetBitmap(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                PixelFormats.Pbgra32);
            tempRender.Render(visual);

            var blurImage = new System.Windows.Controls.Image
            {
                Source = tempRender,
                Effect = new BlurEffect
                {
                    Radius = radius,
                    KernelType = KernelType.Gaussian,
                    RenderingBias = RenderingBias.Quality
                }
            };

            blurImage.Measure(new System.Windows.Size(source.PixelWidth, source.PixelHeight));
            blurImage.Arrange(new Rect(0, 0, source.PixelWidth, source.PixelHeight));

            var result = new RenderTargetBitmap(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                PixelFormats.Pbgra32);
            result.Render(blurImage);
            result.Freeze();

            return result;
        }

        public bool IsMaximized => WindowState == WindowState.Maximized;

        protected virtual void OnShellInitialize()
        {
        }
    }
}