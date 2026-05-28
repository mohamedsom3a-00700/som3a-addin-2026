using System.Windows;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class HomePage : PageBase
    {
        private HomeViewModel _viewModel;

        public HomePage()
        {
            InitializeComponent();
            _viewModel = new HomeViewModel(App.Container, Services.NavigationService.Instance);
            DataContext = _viewModel;
        }

        protected override async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            base.OnPageLoaded(sender, e);
            await _viewModel.LoadAsync();
        }

        protected override void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnPageUnloaded(sender, e);
            _viewModel?.Cleanup();
        }
    }
}
