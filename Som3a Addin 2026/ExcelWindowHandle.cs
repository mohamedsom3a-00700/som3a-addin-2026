using System;
using System.Windows;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_Addin_2026
{
    internal static class ExcelWindowHandle
    {
        public static void SetOwnerToExcel(Window wpfWindow, Excel.Application app)
        {
            if (wpfWindow == null) return;
            if (app == null) return;

            IntPtr hwnd = new IntPtr(app.Hwnd);
            var helper = new WindowInteropHelper(wpfWindow) { Owner = hwnd };
        }
    }
}
