using System.Windows.Controls;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Pages
{
    public partial class StyleSelectorPage : Page
    {
        public StyleSelectorPage()
        {
            InitializeComponent();
            var vm = App.Container.Resolve<ViewModels.WbsStyleSelectorViewModel>();
            vm.CloseWindow = () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack();
            DataContext = vm;
        }
    }
}
