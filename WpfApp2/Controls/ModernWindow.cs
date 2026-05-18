using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Som3a_WPF_UI.Helpers;

namespace Som3a_WPF_UI.Controls
{
    public class ModernWindow : Window
    {
        private bool _useSafeMode;

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
        }

        private void InitializeWindow()
        {
           _useSafeMode = WindowChromeHelper.IsVstoHosted;

           if (_useSafeMode)
            {
                WindowChromeHelper.ApplyWindowChrome(this, true);
            }
            else
            {
                WindowChromeHelper.ApplyWindowChrome(this, false);
            }

            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

           Loaded += OnLoaded;
            StateChanged += OnStateChanged;
           Closing += OnClosing;

            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void SetupAnimations()
        {
            Opacity = 0;
        }

        private void SetupCommands()
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, fadeIn);
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            var clone = this;
            clone.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CloseOnEscape)
            {
                Close();
            }
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
    }
}