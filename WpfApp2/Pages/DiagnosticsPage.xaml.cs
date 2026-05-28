using System.Windows;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class DiagnosticsPage : PageBase
    {
        private DiagnosticsViewModel _viewModel;

        public DiagnosticsPage()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<DiagnosticsViewModel>();
            DataContext = _viewModel;
        }

        protected override void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            base.OnPageLoaded(sender, e);
            _viewModel.RefreshSnapshot();
        }

        protected override void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnPageUnloaded(sender, e);
        }
    }
}
