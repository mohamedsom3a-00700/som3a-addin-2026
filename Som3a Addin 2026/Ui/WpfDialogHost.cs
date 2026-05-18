using System;
using System.Windows;
using System.Windows.Interop;

namespace Som3a_Addin_2026.UI
{
    public static class WpfDialogHost
    {
        public static void Show(int ownerHwnd, Func<Window> windowFactory)
        {
            var win = windowFactory();

            var helper = new WindowInteropHelper(win)
            {
                Owner = new IntPtr(ownerHwnd)
            };

            win.Show();
            win.Activate();
        }
    }
}
