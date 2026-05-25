using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Controls.Toast
{
    public partial class ToastWindow : ModernWindow
    {
        private readonly DispatcherTimer _timer;
        private readonly bool _useSafeMode;
        private static bool IsReducedMotionEnabled =>
            !SystemParameters.ClientAreaAnimation;

        public ToastWindow(ToastViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            var renderService = RenderModeService.Instance;
            renderService.Initialize();
            _useSafeMode = renderService.IsSafeModeRequired();

            PositionWindow();

            if (!_useSafeMode)
            {
                Opacity = 0;
                RenderTransform = new TranslateTransform(0, 20);
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(viewModel.DurationMs)
            };
            _timer.Tick += (s, e) => CloseToast();

            Loaded += (s, e) =>
            {
                ShowToast();
                _timer.Start();
            };
        }

        private void PositionWindow()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - ActualWidth - 20;
            Top = workingArea.Bottom - ActualHeight - 20;
        }

        private void ShowToast()
        {
            if (_useSafeMode || IsReducedMotionEnabled)
                return;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(200));
            slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            BeginAnimation(OpacityProperty, fadeIn);
            RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void CloseToast()
        {
            _timer.Stop();

            if (_useSafeMode || IsReducedMotionEnabled)
            {
                Close();
                return;
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            var slideDown = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(150));
            slideDown.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            fadeOut.Completed += (s, e) => Close();
            RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);
            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
