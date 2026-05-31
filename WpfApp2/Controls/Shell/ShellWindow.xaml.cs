using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Som3a.Localization.Contracts;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Pages;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Som3a_WPF_UI.Controls.Shell
{
    public partial class ShellViewModel : ViewModelBase
    {
        public ObservableCollection<NavigationDestination> Destinations { get; } =
            NavigationService.Instance.Destinations;

        [ObservableProperty]
        private NavigationDestination? _selectedDestination;

        private NavigationDestination? _oldSelectedDestination;

        partial void OnSelectedDestinationChanging(NavigationDestination? value)
        {
            _oldSelectedDestination = _selectedDestination;
        }

        partial void OnSelectedDestinationChanged(NavigationDestination? value)
        {
            if (_oldSelectedDestination != null)
                _oldSelectedDestination.IsSelected = false;
            if (_selectedDestination != null)
                _selectedDestination.IsSelected = true;
        }

        [ObservableProperty]
        private Type? _welcomePageType;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [RelayCommand]
        private void NavigateToPage(string? key)
        {
            if (!string.IsNullOrEmpty(key))
                NavigationService.Instance.NavigateTo(key);
        }
    }

    public partial class ShellWindow
    {
        private readonly ShellViewModel _viewModel;
        private readonly ILocalizationService _localization;
        private bool _isFirstRun = true;
        private bool _hasPendingNavigation;
        private NavigationDestination _pendingDestination;
        private readonly EventHandler<NavigationEventArgs> _navigationChangedHandler;
        private bool _suppressSelectionChanged;

        public ShellWindow()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<ShellViewModel>();
            _localization = App.Container.Resolve<ILocalizationService>();
            DataContext = _viewModel;

            _viewModel.WelcomePageType = typeof(HomePage);

            NavigationService.Instance.SetShellWindow(this);
            _navigationChangedHandler = (s, args) => _hasPendingNavigation = true;
            NavigationService.Instance.NavigationChanged += _navigationChangedHandler;

            Sidebar.SelectionChanged += OnSidebarSelectionChanged;

            if (Workspace != null)
            {
                Workspace.NavigationCompleted += OnWorkspaceNavigationCompleted;
            }

            PreviewKeyDown += OnShellPreviewKeyDown;

            OnShellInitialize();

            Loaded += OnShellLoaded;
            Closed += OnClosed;

            ShellRTLManager.Instance.RegisterFlowElement(RootGrid, "ShellRootGrid");
            ShellRTLManager.Instance.RegisterFlowElement(Sidebar, "ShellSidebar");
            ShellRTLManager.Instance.RegisterFlowElement(Workspace, "ShellWorkspace");

            if (_localization.IsRTL)
            {
                ShellRTLManager.Instance.ApplyLayout(true);
            }
        }

        /// <summary>Gets the currently displayed page, if any.</summary>
        public Page CurrentPage => Workspace?.CurrentPage;

        private void OnClosed(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigationChanged -= _navigationChangedHandler;
        }

        private void OnShellLoaded(object sender, RoutedEventArgs e)
        {
            if (_pendingDestination != null)
            {
                var pending = _pendingDestination;
                _pendingDestination = null;
                _isFirstRun = false;
                PerformNavigation(pending);
            }
            else if (_isFirstRun && !_hasPendingNavigation)
            {
                ShowWelcomePage();
                _isFirstRun = false;
            }
        }

        private void OnNavigateCommand(string key)
        {
            NavigationService.Instance.NavigateTo(key);
        }

        private void OnSidebarSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged) return;

            var previous = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as NavigationDestination : null;

            if (Sidebar.SelectedItem is NavigationDestination destination)
            {
                var navigated = NavigationService.Instance.RequestNavigation(destination.Key);
                if (!navigated)
                {
                    _suppressSelectionChanged = true;
                    Sidebar.SelectedItem = previous;
                    _suppressSelectionChanged = false;
                }
            }
        }

        public void NavigateToDestination(NavigationDestination destination)
        {
            if (destination == null) return;

            if (_isFirstRun)
            {
                _pendingDestination = destination;
                _viewModel.SelectedDestination = destination;
                return;
            }

            PerformNavigation(destination);
        }



        private void PerformNavigation(NavigationDestination destination)
        {
            _viewModel.SelectedDestination = destination;
            _viewModel.StatusMessage = $"Navigating to {destination.Label}...";

            if (Workspace != null)
            {
                Workspace.LazyNavigate(() =>
                {
                    var page = NavigationService.Instance.CreatePage(destination.Key);
                    if (page != null)
                        page.Tag = destination.Key;
                    return page;
                });
            }
        }

        private void OnWorkspaceNavigationCompleted(object sender, NavigationEventArgs e)
        {
            _viewModel.StatusMessage = e.Success ? "Ready" : $"Error: {e.Error}";
        }

        private void OnShellPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.K && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ToggleCommandPalette();
                e.Handled = true;
            }
        }

        private void ToggleCommandPalette()
        {
            if (Palette.IsOpen)
                Palette.Close();
            else
                Palette.Open();
        }

        public void ShowWelcomePage()
        {
            Workspace?.ShowWelcome();
        }

        private void OnSkipLinkClick(object sender, RoutedEventArgs e)
        {
            Workspace?.Focus();
            Keyboard.Focus(Workspace);
        }

        private void OnThemeToggle(object sender, RoutedEventArgs e)
        {
            var current = ThemeManager.Instance.CurrentTheme;
            if (current == "Dark")
                ThemeManager.Instance.ApplyTheme("Light");
            else
                ThemeManager.Instance.ApplyTheme("Dark");
        }

        private void OnLanguageToggle(object sender, RoutedEventArgs e)
        {
            var currentCode = _localization.CurrentLanguageCode;
            var newCode = currentCode == "en-US" ? "ar-SA" : "en-US";
            if (_localization.SetLanguage(newCode))
                _localization.SaveLanguagePreference();
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.NavigateTo("home");
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                DragMove();
        }

        protected override void OnShellInitialize()
        {
        }

        public void SetOwner(IntPtr excelHwnd)
        {
            if (excelHwnd != IntPtr.Zero)
            {
                var helper = new WindowInteropHelper(this);
                helper.Owner = excelHwnd;
            }
        }
    }
}
