using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Views;

namespace Som3a_Addin_2026
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class AddInAutomation : IAddInAutomation
    {
        private readonly Dictionary<string, Window> _openWindows = new Dictionary<string, Window>();

        private T InvokeOnUI<T>(Func<T> action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                return dispatcher.Invoke(action);
            return action();
        }

        private void InvokeOnUI(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke(action);
            else
                action();
        }

        public string OpenWindow(string name)
        {
            return InvokeOnUI(() =>
            {
            try
            {
                Window window = null;
                switch (name)
                {
                    case "Home":
                    case "Som3a Add-in":
                        window = new Som3a_WPF_UI.Controls.Shell.ShellWindow();
                        break;
                    case "Comparsion":
                    case "Comparison P6 Activity":
                        window = new Som3a_WPF_UI.MainWindow();
                        break;
                    case "Trades Codes":
                        window = new Som3a_WPF_UI.AssignTradeCodesWindow();
                        break;
                    case "Daily Report":
                        window = new Som3a_WPF_UI.SubDailyReportWindow(Globals.ThisAddIn.Application);
                        break;
                    case "Links Manager":
                        window = new Som3a_WPF_UI.Views.LinksManagerWindow(Globals.ThisAddIn.Application);
                        break;
                    case "Project Analysis":
                    case "Revised Baseline (Split Activity)":
                        window = new Som3a_WPF_UI.Ui.ProjectAnalysisWindow(Globals.ThisAddIn.Application);
                        break;
                    case "XER Editor":
                    case "Xer Editor":
                        window = new Som3a_WPF_UI.XerEditorWindow();
                        break;
                    case "WBS Color Styles":
                    case "Color WBS Setting":
                        window = new Som3a_WPF_UI.StyleSelectorWindow();
                        break;
                    case "Unmerge Fill Down":
                        window = new Som3a_WPF_UI.UnmergeFillDownWindow(Globals.ThisAddIn.Application);
                        break;
                    case "Float Path Analyzer":
                        window = new Som3a_WPF_UI.Float_path();
                        break;
                    case "Fix Pie Chart Colors":
                        window = new Som3a_WPF_UI.FixPieColorsWindow();
                        break;
                    case "Primavera Compare":
                    case "Comparsion by Xer":
                        window = new Som3a_WPF_UI.Windows.PrimaveraComparison.PrimaveraCompareWindow();
                        break;
                    case "Settings":
                    case "Add in Setting":
                        window = new Som3a_WPF_UI.Views.SettingsWindow();
                        break;
                    default:
                        return "WINDOW_NOT_FOUND";
                }

                if (window != null)
                {
                    _openWindows[name] = window;
                    var capturedWindow = window;
                    EventHandler handler = null;
                    handler = (s, e) =>
                    {
                        capturedWindow.Closed -= handler;
                        if (_openWindows.TryGetValue(name, out var existing) && existing == capturedWindow)
                            _openWindows.Remove(name);
                    };
                    capturedWindow.Closed += handler;
                    capturedWindow.Show();
                    return "OK";
                }
                return "NULL_WINDOW";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            });
        }

        public string SwitchTheme(string theme)
        {
            return InvokeOnUI(() =>
            {
            try
            {
                if (ThemeManager.Instance == null)
                    return "ERROR: ThemeManager not initialized";

                theme = theme.ToLowerInvariant();
                string resolved = theme switch
                {
                    "dark" => "Dark",
                    "light" => "Light",
                    "custom" => "Custom",
                    _ => null
                };

                if (resolved == null)
                    return "INVALID_THEME: " + theme;

                ThemeManager.Instance.ApplyTheme(resolved);
                return "OK";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            });
        }

        public string GetCurrentTheme()
        {
            try
            {
                if (ThemeManager.Instance == null)
                    return "NOT_INITIALIZED";
                return ThemeManager.Instance.CurrentTheme ?? "UNKNOWN";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public string GetStatus()
        {
            return InvokeOnUI(() =>
            {
            try
            {
                var mem = Environment.WorkingSet / 1024.0 / 1024.0;
                var gcMem = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
                var openWindows = Application.Current?.Windows.Count ?? 0;
                var theme = GetCurrentTheme();

                return $"THEME:{theme}|MEM:{mem:F1}|GC:{gcMem:F1}|WINDOWS:{openWindows}|OPEN_MANAGED:{_openWindows.Count}";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            });
        }

        public string CloseWindow(string name)
        {
            return InvokeOnUI(() =>
            {
            try
            {
                if (_openWindows.TryGetValue(name, out var window))
                {
                    window.Close();
                    _openWindows.Remove(name);
                    return "OK";
                }
                return "NOT_OPEN";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            });
        }

        public string RunDiagnostics()
        {
            try
            {
                var status = GetStatus();
                return "DIAGNOSTICS_START|" + status + "|DIAGNOSTICS_END";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}
