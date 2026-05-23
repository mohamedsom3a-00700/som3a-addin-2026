using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Som3a_WPF_UI.Helpers;
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
        private readonly EventHandler<NavigationEventArgs> _navigationChangedHandler;

        public ShellWindow()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<ShellViewModel>();
            DataContext = _viewModel;

            _viewModel.WelcomePageType = typeof(WelcomePage);
            _viewModel.NavigateToPageCommand = new RelayCommand(p => OnNavigateCommand(p as string));

            NavigationService.Instance.SetShellWindow(this);
            _navigationChangedHandler = (s, args) => _hasPendingNavigation = true;
            NavigationService.Instance.NavigationChanged += _navigationChangedHandler;

            Sidebar.SelectionChanged += OnSidebarSelectionChanged;

            if (Workspace != null)
            {
                Workspace.NavigationCompleted += OnWorkspaceNavigationCompleted;
            }

            OnShellInitialize();

            Loaded += OnShellLoaded;
            Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigationChanged -= _navigationChangedHandler;
        }

        private void OnShellLoaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstRun && !_hasPendingNavigation)
            {
                ShowWelcomePage();
            }
            _isFirstRun = false;
        }

        private void OnNavigateCommand(string key)
        {
            NavigationService.Instance.NavigateTo(key);
        }

        private void OnSidebarSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Sidebar.SelectedItem is NavigationDestination destination)
            {
                NavigationService.Instance.NavigateTo(destination.Key);
            }
        }

        public void NavigateToDestination(NavigationDestination destination)
        {
            if (destination == null) return;

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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

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
    }
}
