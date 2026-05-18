using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using WpfWindow = System.Windows.Window;
using System;
using System.Windows.Interop;

namespace Som3a_WPF_UI.Helpers
{
    public static class ExcelOwnerHelper
    {
        public static void SetExcelOwner(WpfWindow window, ExcelApp excelApp)
        {
            if (window == null || excelApp == null) return;

            try
            {
                var helper = new WindowInteropHelper(window);
                helper.Owner = new IntPtr(excelApp.Hwnd);
            }
            catch { }
        }
    }
}
