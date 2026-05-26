using Som3a_WPF_UI.Services;
using System;
using System.Windows;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_Addin_2026.UIHost
{
    public static class WpfWindowManager
    {
        public static void ShowProjectAnalysis(Excel.Application app)
        {
            Som3a_WPF_UI.Services.NavigationService.Instance.NavigateTo("planning.analysis");
        }
    }
}
