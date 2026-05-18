using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Som3a_WPF_UI.Controls.Toast;

namespace Som3a_WPF_UI.Controls.Toast
{
    public partial class ToastWindow : Window
    {
        private readonly ToastModel _model;
        private readonly DispatcherTimer _timer;

        public ToastWindow(ToastModel model)
        {
            InitializeComponent();
            _model = model;

            ConfigureToast();
            PositionWindow();

            Opacity = 0;
            RenderTransform = new System.Windows.Media.TranslateTransform(0, 20);

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
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(46, 213, 115));
                    IconText.Text = "✓";
                    IconText.Foreground = Brushes.White;
                    break;

                case ToastType.Error:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(255, 71, 87));
                    IconText.Text = "✗";
                    IconText.Foreground = Brushes.White;
                    break;

                case ToastType.Warning:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(255, 165, 2));
                    IconText.Text = "⚠";
                    IconText.Foreground = Brushes.White;
                    break;

                case ToastType.Info:
                default:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(58, 134, 255));
                    IconText.Text = "ℹ";
                    IconText.Foreground = Brushes.White;
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
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(250));
            slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            BeginAnimation(OpacityProperty, fadeIn);
            RenderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUp);
        }

        private void CloseToast()
        {
            _timer.Stop();

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            var slideDown = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(150));
            slideDown.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            fadeOut.Completed += (s, e) => Close();
            RenderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDown);
            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}