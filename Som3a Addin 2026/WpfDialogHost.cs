using System;
using System.Windows;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_Addin_2026
{
    internal static class WpfDialogHost
    {
        public static void ShowDialog(Window window, Excel.Application app)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            if (app == null) throw new ArgumentNullException(nameof(app));

            // Owner = Excel
            new WindowInteropHelper(window) { Owner = new IntPtr(app.Hwnd) };

            // Helps avoid “behind Excel” issues
            window.ShowInTaskbar = false;
            window.Topmost = true;
            window.Loaded += (_, __) => window.Topmost = false;

            window.ShowDialog();
        }
    }
}
