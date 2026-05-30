using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Views
{
    public partial class SplashWindow : Window
    {
        private readonly DispatcherTimer _timeoutTimer;
        private const int MaxSplashDurationMs = 3000;
        private bool _isAnimationComplete;
        private bool _isStaticFallback;

        public event EventHandler SplashComplete;

        public SplashWindow()
        {
            InitializeComponent();

            var detectedMode = WindowRenderModeDetector.DetectOptimalMode();
            _isStaticFallback = detectedMode == WindowRenderMode.FallbackSafe;

            if (_isStaticFallback)
            {
                ConfigureStaticFallback();
            }

            _timeoutTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(MaxSplashDurationMs)
            };
            _timeoutTimer.Tick += OnTimeout;
            _timeoutTimer.Start();

            Loaded += OnLoaded;
        }

        private void ConfigureStaticFallback()
        {
            BlueprintCanvas.Visibility = Visibility.Collapsed;
            LogoText.Opacity = 1;
            SubtitleText.Opacity = 1;
            ProgressBar.Width = 300;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isStaticFallback)
            {
                BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
                StartAnimations();
            }
            else
            {
                AnimateProgressBar();
            }
        }

        private void StartAnimations()
        {
            var blueprintStoryboard = new Storyboard();

            var lineAnim1 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(lineAnim1, BlueprintLine1);
            Storyboard.SetTargetProperty(lineAnim1, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(lineAnim1);

            var lineAnim2 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(700),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(lineAnim2, BlueprintLine2);
            Storyboard.SetTargetProperty(lineAnim2, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(lineAnim2);

            var lineAnim3 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(1100),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(lineAnim3, BlueprintLine3);
            Storyboard.SetTargetProperty(lineAnim3, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(lineAnim3);

            var lineAnim4 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(lineAnim4, BlueprintLine4);
            Storyboard.SetTargetProperty(lineAnim4, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(lineAnim4);

            var nodeAnim1 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                BeginTime = TimeSpan.FromMilliseconds(600),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(nodeAnim1, BlueprintNode1);
            Storyboard.SetTargetProperty(nodeAnim1, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(nodeAnim1);

            var nodeAnim2 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                BeginTime = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(nodeAnim2, BlueprintNode2);
            Storyboard.SetTargetProperty(nodeAnim2, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(nodeAnim2);

            var nodeAnim3 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                BeginTime = TimeSpan.FromMilliseconds(1400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(nodeAnim3, BlueprintNode3);
            Storyboard.SetTargetProperty(nodeAnim3, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(nodeAnim3);

            var logoAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(800))
            {
                BeginTime = TimeSpan.FromMilliseconds(1800),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(logoAnim, LogoText);
            Storyboard.SetTargetProperty(logoAnim, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(logoAnim);

            var subtitleAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(2300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(subtitleAnim, SubtitleText);
            Storyboard.SetTargetProperty(subtitleAnim, new PropertyPath(OpacityProperty));
            blueprintStoryboard.Children.Add(subtitleAnim);

            blueprintStoryboard.Completed += (s, args) =>
            {
                _isAnimationComplete = true;
                AnimateProgressBar();
            };

            blueprintStoryboard.Begin();
        }

        private void AnimateProgressBar()
        {
            var progressAnim = new DoubleAnimation(300, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(progressAnim, ProgressBar);
            Storyboard.SetTargetProperty(progressAnim, new PropertyPath(WidthProperty));

            StatusText.Text = "Finalizing...";

            var progressStoryboard = new Storyboard();
            progressStoryboard.Children.Add(progressAnim);
            progressStoryboard.Completed += (s, args) =>
            {
                if (!_isAnimationComplete)
                {
                    var pendingAnim = new DoubleAnimation(ProgressBar.ActualWidth, 300, TimeSpan.FromMilliseconds(200));
                    Storyboard.SetTarget(pendingAnim, ProgressBar);
                    Storyboard.SetTargetProperty(pendingAnim, new PropertyPath(WidthProperty));
                    var sb = new Storyboard();
                    sb.Children.Add(pendingAnim);
                    sb.Begin();
                }
                DismissSplash();
            };

            progressStoryboard.Begin();
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            DismissSplash();
        }

        private void DismissSplash()
        {
            if (!IsLoaded) return;

            _timeoutTimer.Stop();

            var fadeOut = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, args) =>
            {
                SplashComplete?.Invoke(this, EventArgs.Empty);
                Close();
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }

        public void UpdateStatus(string status)
        {
            if (Dispatcher.CheckAccess())
            {
                StatusText.Text = status;
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => StatusText.Text = status));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _timeoutTimer.Stop();
            base.OnClosing(e);
        }
    }
}
