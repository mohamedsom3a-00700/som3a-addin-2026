using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.VisualBasic;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_Addin_2026.Ui;
using Som3a_Addin_2026.UIHost;
using Som3a_WPF_UI;
using Som3a_WPF_UI.ViewModels;
using Som3a_WPF_UI.Windows.PrimaveraComparison;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;


namespace Som3a_Addin_2026
{
    public partial class Ribbon1
    {
        private MainWindow _window;
        private FixPieColorsWindow _pieWindow;

        // ✅ Trades Codes window
        private AssignTradeCodesWindow _tradeWindow;

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void comparsion_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                if (_window == null || !_window.IsVisible)
                {
                    Excel.Application xlApp = Globals.ThisAddIn.Application;

                    _window = new MainWindow();
                    _window.AttachExcel(xlApp);

                    var helper = new WindowInteropHelper(_window);
                    helper.Owner = new IntPtr(xlApp.Hwnd);

                    _window.Closed += (s, args) => { _window = null; };

                    _window.Show();
                    _window.Activate();
                }
                else
                {
                    _window.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Excel.Application xlApp = Globals.ThisAddIn.Application;

                if (_pieWindow == null || !_pieWindow.IsVisible)
                {
                    _pieWindow = new FixPieColorsWindow();
                    _pieWindow.AttachExcel(xlApp);

                    var helper = new WindowInteropHelper(_pieWindow);
                    helper.Owner = new IntPtr(xlApp.Hwnd);

                    _pieWindow.Closed += (s, args) => { _pieWindow = null; };

                    _pieWindow.Show();
                    _pieWindow.Activate();
                }
                else
                {
                    _pieWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
            }
        }

        // ✅ Trades Codes button
        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Excel.Application xlApp = Globals.ThisAddIn.Application;

                if (_tradeWindow == null || !_tradeWindow.IsVisible)
                {
                    _tradeWindow = new AssignTradeCodesWindow();

                    // لو نافذتك فيها AttachExcel زي باقي النوافذ (مُستحسن)
                    // _tradeWindow.AttachExcel(xlApp);

                    var helper = new WindowInteropHelper(_tradeWindow);
                    helper.Owner = new IntPtr(xlApp.Hwnd);

                    _tradeWindow.Closed += (s, args) => { _tradeWindow = null; };

                    _tradeWindow.Show();
                    _tradeWindow.Activate();
                }
                else
                {
                    _tradeWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
            }
        }
        private void btnUnmergeFillDown_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Application app = Globals.ThisAddIn.Application;
            var win = new UnmergeFillDownWindow(app);
            WpfDialogHost.ShowDialog(win, app);
        }

        private void btnDailyReport_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Excel.Application app = Globals.ThisAddIn.Application;

                var w = new SubDailyReportWindow(app);


                ExcelWindowHandle.SetOwnerToExcel(w, app);


                w.ShowDialog();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "DailyReport Error");
            }
        }
        private void btnLinksManager_Click(object sender, RibbonControlEventArgs e)
        {
            var app = Globals.ThisAddIn.Application;
            LinksManagerLauncher.Show(app);
        }

        private void btnProjectAnalysis_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                Som3a_Addin_2026.UI.WpfDialogHost.Show(app.Hwnd, () =>
                    new Som3a_WPF_UI.Ui.ProjectAnalysisWindow(app));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Open Project Analysis Failed");
            }
        }


        private void Coloringwbs_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Excel.Application app = Globals.ThisAddIn.Application;

                var style = WbsStyleFactory.GetStyle(UserSettings.SelectedStyle);
                var svc = new WbsColoringService(style);
                svc.ApplyToActiveSheet(app, "Activity ID", 15);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Coloringwbs");
            }

        }


        private void ExportPaletteHex_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;

                // 1) يكتب HEX جنب الألوان
                new PaletteExportService().ExportSelectedRangeToAdjacentCells(app, false);

                // 2) يكتب اسم المستوى في العمود K بناءً على لون الصف
                var ws = (Excel.Worksheet)app.ActiveSheet;
                var svc = new WbsColoringService();          // <-- لازم تتعرّف هنا
                //svc.WriteWbsLevelNameByRowColor(ws, 11);     // K = 11
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Export Palette HEX");
            }
        }
        private void GroupWbs_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var ws = (Excel.Worksheet)app.ActiveSheet;

                var svc = new WbsColoringService();

                // لو عندك كل مستوى = مسافتين خليها 2
                int spacesPerLevel = 1;

                // preferColorLevels = true => لو الألوان موجودة اعتمد عليها، وإلا ارجع للمسافات
                svc.GroupRowsFull(ws, "Activity ID", 15, spacesPerLevel, true);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Group WBS Full");
            }
        }


        private void btnSafeClone_Click(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;

                if (app.ActiveWorkbook == null)
                {
                    MessageBox.Show("No active workbook.", "Som3a Tools");
                    return;
                }

                using var sfd = new SaveFileDialog();
                sfd.Filter = "Excel Macro-Enabled Workbook (*.xlsm)|*.xlsm|Excel Workbook (*.xlsx)|*.xlsx";
                sfd.FileName = $"{System.IO.Path.GetFileNameWithoutExtension(app.ActiveWorkbook.Name)}_CLEANED.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                // ممكن تعمل نافذة Progress بسيطة أو تستخدم StatusBar
                app.StatusBar = "Safe Clone: starting...";

                var svc = new Som3a.Shared.Core.WorkbookCloneService(app);

                svc.SafeCloneActiveWorkbook(sfd.FileName, p =>
                {
                    app.StatusBar = $"Safe Clone: {p}%";
                });

                app.StatusBar = false;
                MessageBox.Show("Safe Clone created successfully.", "Som3a Tools");
            }
            catch (Exception ex)
            {
                try { Globals.ThisAddIn.Application.StatusBar = false; } catch { }
                System.Windows.MessageBox.Show(ex.Message, "Som3a Tools - Error");
            }
        }

        private void BtnTool_Click(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
        {
            var btn = (Microsoft.Office.Tools.Ribbon.RibbonButton)sender;

            switch (btn.Name)
            {
                case "btnDaysFraction":
                    RunDaysFractionFix();
                    break;

                case "btnPobsCleaner":
                    RunPobsCleaner();
                    break;

                case "btnCalendarsCleanup":
                    RunCalendarsCleanup();
                    break;

                case "btnWbsNormalizer":
                    RunWbsNormalizer();
                    break;

                case "btnIdsRebuild":
                    RunIdsRebuild();
                    break;
            }
        }

        private void RunDaysFractionFix() { /* TODO */ }
        private void RunPobsCleaner() { /* TODO */ }
        private void RunCalendarsCleanup() { /* TODO */ }
        private void RunWbsNormalizer() { /* TODO */ }
        private void RunIdsRebuild() { /* TODO */ }
        private void splitXerTools_Click(object sender, RibbonControlEventArgs e)
        {
            // اختياري: خلي الضغط على جسم الزر يشغل "Run All" أو يفتح Tool رئيسي
            RunDaysFractionFix();
            RunPobsCleaner();
            RunCalendarsCleanup();
            RunWbsNormalizer();
            RunIdsRebuild();
        }

        private void splitButton1_Click(object sender, RibbonControlEventArgs e)
        {

        }
        private void BtnFormatsTool_Click(object sender, RibbonControlEventArgs e)
        {
            var btn = (RibbonButton)sender;

            switch (btn.Name)
            {
                case "btnTool1": /* TODO */ break;
                case "btnTool2": /* TODO */ break;
                case "btnTool3": /* TODO */ break;
                case "btnTool4": /* TODO */ break;
                case "btnTool5": /* TODO */ break;
            }
        }

        private void btnHardReload_Click(object sender, RibbonControlEventArgs e)
        {
            var app = Globals.ThisAddIn.Application;

            try
            {
                CloseAllToolWindows();

                const string addinName = "Som3a Addin 2026"; // بالظبط زي اللي في الديالوج

                Office.COMAddIn target = null;

                foreach (Office.COMAddIn a in app.COMAddIns)
                {
                    if (!string.IsNullOrWhiteSpace(a.Description) &&
                        a.Description.Equals(addinName, StringComparison.OrdinalIgnoreCase))
                    {
                        target = a;
                        break;
                    }
                }

                if (target == null)
                {
                    MessageBox.Show("Som3a Add-in not found in COMAddIns.", "Reload Add-in");
                    return;
                }

                // Disconnect
                target.Connect = false;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(800);

                // Reconnect
                target.Connect = true;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(300);

                MessageBox.Show("Som3a Add-in reloaded successfully.", "Reload Add-in");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Reload Add-in failed");
            }
        }




        private void CloseAllToolWindows()
        {
            try { if (_window != null) { _window.Close(); _window = null; } } catch { }
            try { if (_pieWindow != null) { _pieWindow.Close(); _pieWindow = null; } } catch { }
            try { if (_tradeWindow != null) { _tradeWindow.Close(); _tradeWindow = null; } } catch { }

            // لو عندك DialogHost / أي Hosts تانية اقفلها هنا برضه
        }
        public void PrepareForReloadInternal()
        {
            CloseAllToolWindows();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void PrepareForReload()
        {
            try
            {
                Globals.Ribbons.Ribbon1?.PrepareForReloadInternal();
            }
            catch { }
        }

        private void PreForReload_Click(object sender, RibbonControlEventArgs e)
        {
            CloseAllToolWindows();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void button30_Click(object sender, RibbonControlEventArgs e)
        {

        }
        private Float_path _Float_path;

        private void Float_Path_Click(object sender, RibbonControlEventArgs e)
        {
            if (_Float_path == null || !_Float_path.IsLoaded)
            {
                _Float_path = new Float_path();
                _Float_path.Show();
            }
            else
            {
                _Float_path.Activate();
            }
        }

        private void color_setting_Click(object sender, RibbonControlEventArgs e)
        {
            var win = new StyleSelectorWindow();
            win.ShowDialog();
        }

        private void Xer_Editor_Click(object sender, RibbonControlEventArgs e)
        {
            var win = new Som3a_WPF_UI.XerEditorWindow();

            if (win == null || !win.IsLoaded)
            {
                win.Show();
            }
            else
            {
                win.Activate();
            }

        }

        private void Com_Xer_Click(object sender, RibbonControlEventArgs e)
        {
                var window =
                    new PrimaveraCompareWindow();

                window.ShowDialog();
            }

        private void addin_setting_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                if (System.Windows.Application.Current == null)
                {
                    new System.Windows.Application
                    {
                        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
                    };
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var existing = System.Windows.Application.Current.Windows
                        .OfType<Som3a_WPF_UI.Views.SettingsWindow>()
                        .FirstOrDefault();

                    if (existing != null)
                    {
                        existing.Activate();
                        existing.Focus();
                        return;
                    }

                    Som3a_WPF_UI.Services.ThemeManager.LoadSettings();

                    var win = new Som3a_WPF_UI.Views.SettingsWindow();

                    var excelHandle = new IntPtr(
                        Globals.ThisAddIn.Application.Hwnd);

                    new System.Windows.Interop.WindowInteropHelper(win)
                    {
                        Owner = excelHandle
                    };

                    win.WindowStartupLocation =
                        System.Windows.WindowStartupLocation.CenterScreen;

                    win.Topmost = true;
                    win.Activate();

                    win.ShowDialog();
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Settings Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
    }


