using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Pages
{
    public partial class AssignTradeCodesPage : Page
    {
        private readonly AssignTradeCodesViewModel _vm;

        public AssignTradeCodesPage()
        {
            InitializeComponent();

            _vm = App.Container.Resolve<AssignTradeCodesViewModel>();
            DataContext = _vm;

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _vm.Initialize();
        }
    }
}
