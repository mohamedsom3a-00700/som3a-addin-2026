using Microsoft.Office.Interop.Excel;
using Som3a.Shared.Core;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.ViewModels;
using System.Windows;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using Som3a_WPF_UI;

namespace Som3a_WPF_UI.Views
{
    public partial class LinksManagerWindow : Som3a_WPF_UI.Controls.ModernWindow
    {
        private readonly LinksManagerViewModel _vm;

        public LinksManagerWindow(ExcelApp excelApp)
        {
            InitializeComponent();

            var svc = new LinksManagerService(excelApp);
            _vm = new LinksManagerViewModel(App.Container, excelApp, this.Dispatcher, svc);
            _vm.RequestClose += () => this.Close();

            DataContext = _vm;

            ExcelOwnerHelper.SetExcelOwner(this, excelApp);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
