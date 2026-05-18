using Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.ViewModels;
using System.Windows;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace Som3a_WPF_UI.Views
{
    public partial class LinksManagerWindow : Som3a_WPF_UI.Controls.ModernWindow
    {
        private readonly LinksManagerViewModel _vm;

        public LinksManagerWindow(ExcelApp excelApp)
        {
            InitializeComponent();

            _vm = new LinksManagerViewModel(excelApp, this.Dispatcher);
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
