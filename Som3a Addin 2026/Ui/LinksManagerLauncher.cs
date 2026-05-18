using Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Views;
using System.Windows;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace Som3a_Addin_2026.Ui
{
    public static class LinksManagerLauncher
    {
        public static void Show(ExcelApp excelApp)
        {
            var win = new LinksManagerWindow(excelApp);
            win.ShowInTaskbar = false;
            win.ShowDialog();
        }

    }
}
