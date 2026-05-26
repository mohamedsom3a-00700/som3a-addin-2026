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

        private void OnScrollViewerPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _sidebarPanel = FindName("SidebarPanel") as FrameworkElement;
            _sidebarListBox = FindName("SidebarList") as ListBox;
            _containerGrid = _sidebarPanel?.Parent as Grid;
            if (_containerGrid != null)
                _originalColumnWidth = _containerGrid.ColumnDefinitions[0].Width;
            ApplySidebarState();

            if (DataContext is ViewModels.SettingsViewModel vm)
            {
                if (FindName("DebugCount") is System.Windows.Controls.TextBlock tb)
                {
                    tb.Text = $"Categories: {vm.Categories.Count} items";
                    if (vm.Categories.Count > 0)
                        tb.Text += $" | First: '{vm.Categories[0].DisplayName}'";
                    else
                        tb.Text += " | EMPTY!";
                    tb.Foreground = System.Windows.Media.Brushes.Lime;
                }

                System.Diagnostics.Trace.WriteLine($"[SettingsPage] DataContext: SettingsViewModel with {vm.Categories.Count} categories");
                if (vm.Categories.Count > 0)
                {
                    System.Diagnostics.Trace.WriteLine($"[SettingsPage] First category: '{vm.Categories[0].DisplayName}' Icon='{vm.Categories[0].Icon}'");
                }
            }
            else
            {
                if (FindName("DebugCount") is System.Windows.Controls.TextBlock tb)
                {
                    tb.Text = "DataContext is NULL or wrong type!";
                    tb.Foreground = System.Windows.Media.Brushes.Red;
                }
                System.Diagnostics.Trace.WriteLine("[SettingsPage] DataContext is null or wrong type!");
            }
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
