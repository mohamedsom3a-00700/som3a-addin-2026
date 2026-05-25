using System;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class UnmergeFillDownPage : Page
    {
        private Excel.Application _excelApp;

        public UnmergeFillDownPage()
        {
            InitializeComponent();
        }

        public void InitializeWithExcel(Excel.Application excelApp)
        {
            _excelApp = excelApp ?? throw new ArgumentNullException(nameof(excelApp));
            DataContext = new UnmergeFillDownViewModel(App.Container, _excelApp, () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack());
        }
    }
}
