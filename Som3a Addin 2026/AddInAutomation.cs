using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Som3a_WPF_UI.Services;

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
                var nav = Som3a_WPF_UI.Services.NavigationService.Instance;
                string route;
                switch (name)
                {
                    case "Home":
                    case "Som3a Add-in":
                        route = "welcome";
                        break;
                    case "Trades Codes":
                        route = "excel.tradecodes";
                        break;
                    case "Daily Report":
                        route = "excel.subdaily";
                        break;
                    case "Links Manager":
                        route = "excel.links";
                        break;
                    case "Project Analysis":
                    case "Revised Baseline (Split Activity)":
                        route = "planning.analysis";
                        break;
                    case "XER Editor":
                    case "Xer Editor":
                        route = "planning.xereditor";
                        break;
                    case "WBS Color Styles":
                    case "Color WBS Setting":
                        route = "excel.styles";
                        break;
                    case "Unmerge Fill Down":
                        route = "excel.unmerge";
                        break;
                    case "Float Path Analyzer":
                        route = "analysis.floatpath";
                        break;
                    case "Fix Pie Chart Colors":
                        route = "excel.piecolors";
                        break;
                    case "Comparsion":
                        route = "main";
                        break;
                    case "Primavera Compare":
                    case "Comparsion by Xer":
                        route = "planning.primavera.compare";
                        break;
                    case "Settings":
                    case "Add in Setting":
                        route = "settings.general";
                        break;
                    default:
                        return "WINDOW_NOT_FOUND";
                }
                nav.NavigateTo(route);
                if (!_openWindows.ContainsKey(name) && Application.Current?.MainWindow != null)
                    _openWindows[name] = Application.Current.MainWindow;
                return "OK";
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
            return InvokeOnUI(() =>
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
            });
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
