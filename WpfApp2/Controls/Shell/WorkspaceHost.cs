using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Som3a_WPF_UI.Pages;

namespace Som3a_WPF_UI.Controls.Shell
{
    public interface IPageHost
    {
        void Navigate(Page page);
        void ShowError(string message, Action retryAction);
        void ShowWelcome();
        void Clear();
        event EventHandler<NavigationEventArgs> NavigationCompleted;
    }

    public class WorkspaceHost : ContentControl, IPageHost
    {
        private Frame _frame;
        private Border _errorOverlay;
        private bool _isFirstNavigation = true;
        private Action _retryAction;

        public static readonly DependencyProperty WelcomePageTypeProperty =
            DependencyProperty.Register(
                nameof(WelcomePageType),
                typeof(Type),
                typeof(WorkspaceHost),
                new PropertyMetadata(null));

        public Type WelcomePageType
        {
            get => (Type)GetValue(WelcomePageTypeProperty);
            set => SetValue(WelcomePageTypeProperty, value);
        }

        public event EventHandler<NavigationEventArgs> NavigationCompleted;

        public WorkspaceHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(WorkspaceHost),
                new FrameworkPropertyMetadata(typeof(WorkspaceHost)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _frame = GetTemplateChild("PART_Frame") as Frame;
            _errorOverlay = GetTemplateChild("PART_ErrorOverlay") as Border;

            if (_frame != null)
            {
                _frame.Navigated += OnFrameNavigated;
                _frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
                _frame.JournalOwnership = JournalOwnership.OwnsJournal;
            }
        }

        public void Navigate(Page page)
        {
            if (_frame == null) return;

            try
            {
                _frame.Navigate(page);

                if (!_isFirstNavigation)
                {
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    _frame.BeginAnimation(OpacityProperty, fadeIn);
                }

                _isFirstNavigation = false;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, () => Navigate(page));
            }
        }

        public void ShowError(string message, Action retryAction)
        {
            if (_errorOverlay == null) return;

            _errorOverlay.Visibility = Visibility.Visible;
            _frame.Visibility = Visibility.Collapsed;
            _retryAction = retryAction;

            if (_errorOverlay.Child is FrameworkElement errorContent)
            {
                if (errorContent.FindName("PART_ErrorMessage") is TextBlock errorText)
                    errorText.Text = message;

                if (errorContent.FindName("PART_RetryButton") is Button retryBtn)
                {
                    retryBtn.Click -= OnRetryClick;
                    retryBtn.Click += OnRetryClick;
                }
            }
        }

        public void ShowWelcome()
        {
            if (WelcomePageType != null)
            {
                var welcomePage = Activator.CreateInstance(WelcomePageType) as Page;
                if (welcomePage != null)
                {
                    Navigate(welcomePage);
                    return;
                }
            }
            Clear();
        }

        public void Clear()
        {
            if (_frame != null)
            {
                _frame.Content = null;
                _frame.Visibility = Visibility.Visible;
            }
            if (_errorOverlay != null)
                _errorOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is Page page && page.Tag is string tag)
            {
                OnNavigationCompleted(new NavigationEventArgs
                {
                    NewKey = tag,
                    Success = true
                });
            }
        }

        private void OnRetryClick(object sender, RoutedEventArgs e)
        {
            _errorOverlay.Visibility = Visibility.Collapsed;
            _frame.Visibility = Visibility.Visible;
            _retryAction?.Invoke();
        }

        protected virtual void OnNavigationCompleted(NavigationEventArgs e)
        {
            NavigationCompleted?.Invoke(this, e);
        }
    }
}
