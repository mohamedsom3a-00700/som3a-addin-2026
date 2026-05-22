using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Som3a_WPF_UI.Controls.Toast;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Controls.Toast
{
    public partial class ToastWindow : Window
    {
        private readonly ToastModel _model;
        private readonly DispatcherTimer _timer;
        private readonly bool _useSafeMode;

        public ToastWindow(ToastModel model)
        {
            InitializeComponent();
            _model = model;

            var renderService = RenderModeService.Instance;
            renderService.Initialize();
            _useSafeMode = renderService.IsSafeModeRequired();

            ConfigureToast();
            PositionWindow();

            if (!_useSafeMode)
            {
                Opacity = 0;
                RenderTransform = new TranslateTransform(0, 20);
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(model.DurationMs)
            };
            _timer.Tick += (s, e) => CloseToast();

            Loaded += (s, e) =>
            {
                ShowToast();
                _timer.Start();
            };
        }

        private void ConfigureToast()
        {
            switch (_model.Type)
            {
                case ToastType.Success:
                    ToastBorder.Background = (System.Windows.Media.Brush)FindResource("Brush.Accent.Success");
                    IconText.Text = "✓";
                    IconText.Foreground = (System.Windows.Media.Brush)FindResource("Brush.Text.OnAccent");
                    break;

                case ToastType.Error:
                    ToastBorder.Background = (System.Windows.Media.Brush)FindResource("Brush.Accent.Danger");
                    IconText.Text = "✗";
                    IconText.Foreground = (System.Windows.Media.Brush)FindResource("Brush.Text.OnAccent");
                    break;

                case ToastType.Warning:
                    ToastBorder.Background = (System.Windows.Media.Brush)FindResource("Brush.Accent.Warning");
                    IconText.Text = "⚠";
                    IconText.Foreground = (System.Windows.Media.Brush)FindResource("Brush.Text.OnAccent");
                    break;

                case ToastType.Info:
                default:
                    ToastBorder.Background = (System.Windows.Media.Brush)FindResource("Brush.Accent.Primary");
                    IconText.Text = "ℹ";
                    IconText.Foreground = (System.Windows.Media.Brush)FindResource("Brush.Text.OnAccent");
                    break;
            }

            MessageText.Text = _model.Message;
        }

        private void PositionWindow()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - ActualWidth - 20;
            Top = workingArea.Bottom - ActualHeight - 20;
        }

        private void ShowToast()
        {
            if (_useSafeMode)
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

            if (_useSafeMode)
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