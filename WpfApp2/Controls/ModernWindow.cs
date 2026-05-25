using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;

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
            InitializeWindow();
          // SetupAnimations();
            SetupCommands();

            ThemeManager.Instance.LoadThemeFromSettings();
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
                { "Elevation.Window", "Elevation.Window.Safe" },
                { "Elevation.Card", "Elevation.Card.Safe" },
                { "Elevation.Popup", "Elevation.Popup.Safe" }
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

        public bool IsMaximized => WindowState == WindowState.Maximized;

        protected virtual void OnShellInitialize()
        {
        }
    }
}