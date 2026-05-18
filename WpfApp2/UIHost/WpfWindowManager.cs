using System;
using System.Windows;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_Addin_2026.UIHost
{
    public static class WpfWindowManager
    {
        private static Window? _projectAnalysisWin;

        public static void ShowProjectAnalysis(Excel.Application app)
        {
            if (_projectAnalysisWin == null)
            {
                _projectAnalysisWin = new Som3a_WPF_UI.Ui.ProjectAnalysisWindow(app);
                _projectAnalysisWin.Closed += (_, __) => _projectAnalysisWin = null;

                var hwnd = new IntPtr(app.Hwnd);
                var helper = new WindowInteropHelper(_projectAnalysisWin);
                helper.Owner = hwnd;
            }

            _projectAnalysisWin.Show();
            _projectAnalysisWin.Activate();
        }
    }
}
