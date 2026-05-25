using Som3a.Shared.Core;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace Som3a_WPF_UI.Pages
{
    public partial class LinksManagerPage : Page
    {
        private LinksManagerViewModel _vm;

        public LinksManagerPage()
        {
            InitializeComponent();
        }

        public void InitializeWithExcel(ExcelApp excelApp)
        {
            var svc = new LinksManagerService(excelApp);
            _vm = new LinksManagerViewModel(App.Container, excelApp, this.Dispatcher, svc);
            _vm.RequestClose += () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack();
            DataContext = _vm;
        }
    }
}
