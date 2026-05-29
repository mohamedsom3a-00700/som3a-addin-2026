using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Som3a_WPF_UI.Pages;
using Som3a_WPF_UI.Services;
using NavEventArgs = System.Windows.Navigation.NavigationEventArgs;

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
        private ProgressBar _loadingIndicator;
        private Border _errorOverlay;
        private bool _isFirstNavigation = true;
        private Action _retryAction;
        private Page _currentPage;
        private int _isNavigating;

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

        static WorkspaceHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(WorkspaceHost),
                new FrameworkPropertyMetadata(typeof(WorkspaceHost)));
        }

        public WorkspaceHost()
        {
            LocalizationBridgeService.Instance.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            var direction = e.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            if (_frame != null)
                _frame.FlowDirection = direction;
            if (_currentPage != null)
                _currentPage.FlowDirection = direction;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _frame = GetTemplateChild("PART_Frame") as Frame;
            _loadingIndicator = GetTemplateChild("PART_LoadingIndicator") as ProgressBar;
            _errorOverlay = GetTemplateChild("PART_ErrorOverlay") as Border;

            if (_frame != null)
            {
                _frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
                _frame.LoadCompleted += OnFrameLoadCompleted;
                _frame.NavigationFailed += OnFrameNavigationFailed;
                _frame.NavigationStopped += OnFrameNavigationStopped;
                _frame.FlowDirection = LocalizationBridgeService.Instance.IsRTL
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            }
        }

        public Page CurrentPage => _currentPage;

        public void Navigate(Page page)
        {
            if (_frame == null) return;
            if (Interlocked.Exchange(ref _isNavigating, 1) == 1) return;

            try
            {
                _currentPage = page;
                page.FlowDirection = LocalizationBridgeService.Instance.IsRTL
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
                if (_frame != null)
                {
                    _frame.FlowDirection = LocalizationBridgeService.Instance.IsRTL
                        ? FlowDirection.RightToLeft
                        : FlowDirection.LeftToRight;
                }
                if (_errorOverlay != null)
                    _errorOverlay.Visibility = Visibility.Collapsed;
                if (_frame != null)
                    _frame.Visibility = Visibility.Visible;

                if (_loadingIndicator != null)
                    _loadingIndicator.Visibility = Visibility.Visible;

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
                if (_loadingIndicator != null)
                    _loadingIndicator.Visibility = Visibility.Collapsed;
                Interlocked.Exchange(ref _isNavigating, 0);
                ShowError(ex.Message, () => Navigate(page));
            }
        }

        private void OnFrameLoadCompleted(object sender, NavEventArgs e)
        {
            if (_loadingIndicator != null)
                _loadingIndicator.Visibility = Visibility.Collapsed;
            Interlocked.Exchange(ref _isNavigating, 0);

            if (_currentPage != null)
            {
                _currentPage.Loaded += (s, args) =>
                {
                    _currentPage.Focus();
                    _currentPage.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                };
            }

            OnNavigationCompleted(new NavigationEventArgs { Success = true });
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
            // Navigate directly via _frame to avoid setting _isNavigating,
            // so subsequent Navigate() calls (e.g. from PerformNavigation) are not blocked.
            if (_frame != null && WelcomePageType != null)
            {
                var welcomePage = Activator.CreateInstance(WelcomePageType) as Page;
                if (welcomePage != null)
                {
                    var isRTL = LocalizationBridgeService.Instance.IsRTL;
                    welcomePage.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    _frame.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    _frame.Navigate(welcomePage);
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

        private void OnRetryClick(object sender, RoutedEventArgs e)
        {
            if (_errorOverlay != null)
                _errorOverlay.Visibility = Visibility.Collapsed;
            if (_frame != null)
                _frame.Visibility = Visibility.Visible;
            _retryAction?.Invoke();
        }

        private void OnFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (_loadingIndicator != null)
                _loadingIndicator.Visibility = Visibility.Collapsed;
            Interlocked.Exchange(ref _isNavigating, 0);
            OnNavigationCompleted(new NavigationEventArgs { Success = false, Error = $"Navigation failed: {e.Exception?.Message ?? "Unknown error"}" });
        }

        private void OnFrameNavigationStopped(object sender, NavEventArgs e)
        {
            if (_loadingIndicator != null)
                _loadingIndicator.Visibility = Visibility.Collapsed;
            Interlocked.Exchange(ref _isNavigating, 0);
            OnNavigationCompleted(new NavigationEventArgs { Success = false, Error = "Navigation stopped" });
        }

        protected virtual void OnNavigationCompleted(NavigationEventArgs e)
        {
            NavigationCompleted?.Invoke(this, e);
        }
    }
}
