using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI.Views
{
    public partial class SettingsWindow : ModernWindow
    {
        private FrameworkElement? _sidebarPanel;
        private ListBox? _sidebarListBox;
        private Grid? _containerGrid;
        private bool _isCollapsed;
        private GridLength _originalColumnWidth = new GridLength(220);

        public SettingsWindow()
        {
            InitializeComponent();
            var vm = App.Container.Resolve<ViewModels.SettingsViewModel>();
            vm.CloseWindow = result => { DialogResult = result; Close(); };
            DataContext = vm;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
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

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
                vm.Cleanup();
            SizeChanged -= OnSizeChanged;
            base.OnClosed(e);
        }
    }
}
