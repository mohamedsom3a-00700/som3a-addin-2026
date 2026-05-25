using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Pages
{
    public partial class SettingsPage : Page
    {
        private FrameworkElement _sidebarPanel;
        private ListBox _sidebarListBox;
        private Grid _containerGrid;
        private bool _isCollapsed;
        private GridLength _originalColumnWidth = new GridLength(220);

        public SettingsPage()
        {
            InitializeComponent();
            var vm = App.Container.Resolve<ViewModels.SettingsViewModel>();
            vm.CloseWindow = result =>
            {
                Som3a_WPF_UI.Services.NavigationService.Instance.GoBack();
            };
            DataContext = vm;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            Unloaded += OnPageUnloaded;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
                vm.Cleanup();
            SizeChanged -= OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _sidebarPanel = FindName("SidebarPanel") as FrameworkElement;
            _sidebarListBox = FindName("SidebarList") as ListBox;
            _containerGrid = _sidebarPanel?.Parent as Grid;
            if (_containerGrid != null)
                _originalColumnWidth = _containerGrid.ColumnDefinitions[0].Width;
            ApplySidebarState();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var collapsed = e.NewSize.Width < 750;
            if (collapsed != _isCollapsed)
            {
                _isCollapsed = collapsed;
                ApplySidebarState();
            }
        }

        private void ApplySidebarState()
        {
            if (_sidebarPanel == null) return;

            if (_isCollapsed)
            {
                _sidebarPanel.Width = 60;
                _sidebarPanel.MinWidth = 60;
                _sidebarPanel.MaxWidth = 60;

                if (_containerGrid != null && _containerGrid.ColumnDefinitions.Count > 0)
                    _containerGrid.ColumnDefinitions[0].Width = new GridLength(60);

                if (_sidebarListBox != null)
                {
                    _sidebarListBox.ItemTemplate = FindResource("SidebarIconTemplate") as DataTemplate;
                }
            }
            else
            {
                _sidebarPanel.ClearValue(WidthProperty);
                _sidebarPanel.ClearValue(MinWidthProperty);
                _sidebarPanel.ClearValue(MaxWidthProperty);

                if (_containerGrid != null && _containerGrid.ColumnDefinitions.Count > 0)
                    _containerGrid.ColumnDefinitions[0].Width = _originalColumnWidth;

                if (_sidebarListBox != null)
                {
                    _sidebarListBox.ItemTemplate = FindResource("SidebarFullTemplate") as DataTemplate;
                }
            }
        }
    }
}
