using System.Windows.Controls;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class DurationEstimatorPage : Page
    {
        private DurationEstimatorPageViewModel _vm;

        public DurationEstimatorPage()
        {
            InitializeComponent();
            _vm = App.Container.Resolve<DurationEstimatorPageViewModel>();
            DataContext = _vm;
            _vm.RequestClose += () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack();
        }
    }
}
