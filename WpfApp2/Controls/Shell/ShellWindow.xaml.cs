using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Pages;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private NavigationDestination _selectedDestination;
        private string _statusMessage = "Ready";
        private Type _welcomePageType;

        public ObservableCollection<NavigationDestination> Destinations { get; } =
            NavigationService.Instance.Destinations;

        public NavigationDestination SelectedDestination
        {
            get => _selectedDestination;
            set
            {
                if (_selectedDestination != value)
                {
                    if (_selectedDestination != null)
                        _selectedDestination.IsSelected = false;
                    _selectedDestination = value;
                    if (_selectedDestination != null)
                        _selectedDestination.IsSelected = true;
                    OnPropertyChanged();
                }
            }
        }

        public Type WelcomePageType
        {
            get => _welcomePageType;
            set { _welcomePageType = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand NavigateToPageCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class ShellWindow
    {
        private readonly ShellViewModel _viewModel;
        private bool _isFirstRun = true;
        private bool _hasPendingNavigation;
        private NavigationDestination _pendingDestination;
        private readonly EventHandler<NavigationEventArgs> _navigationChangedHandler;
        private bool _suppressSelectionChanged;

        public ShellWindow()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<ShellViewModel>();
            DataContext = _viewModel;

            _viewModel.WelcomePageType = typeof(HomePage);
            _viewModel.NavigateToPageCommand = new RelayCommand(p => OnNavigateCommand(p as string));

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

            if (Services.LocalizationBridgeService.Instance.IsRTL)
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
                var page = NavigationService.Instance.CreatePage(destination.Key);
                if (page != null)
                {
                    page.Tag = destination.Key;
                    Workspace.Navigate(page);
                }
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
            var bridge = Services.LocalizationBridgeService.Instance;
            var currentCode = bridge.CurrentLanguageCode;
            var newCode = currentCode == "en-US" ? "ar-SA" : "en-US";
            bridge.SetLanguage(newCode);
            bridge.SaveLanguagePreference();
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
    }
}
