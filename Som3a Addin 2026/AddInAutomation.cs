using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Pages;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_Addin_2026
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class AddInAutomation : IAddInAutomation
    {
        private readonly Dictionary<string, RouteInfo> _openWindows = new Dictionary<string, RouteInfo>();

        public sealed class RouteInfo
        {
            public string Route { get; set; }
            public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        }

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
                    case "WBS Templates":
                    case "WBS Template Browser":
                        route = "planning.wbs.browser";
                        break;
                    case "WBS Generator":
                    case "AI WBS Generation":
                        route = "planning.wbs.generator";
                        break;
                    case "WBS Editor":
                        route = "planning.wbs.editor";
                        break;
                case "BOQ Activity Generator":
                    route = "planning.boq.activity";
                    break;
                case "Duration Estimator":
                    route = "planning.duration";
                    break;
                    default:
                        return "WINDOW_NOT_FOUND";
                }
                nav.NavigateTo(route);
                // Wait for navigation to complete (ShellWindow may defer on first-run)
                var deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline)
                {
                    var shell = nav.ShellWindow;
                    if (shell != null && shell.CurrentPage != null)
                        break;
                    var disp = Application.Current?.Dispatcher;
                    if (disp != null)
                    {
                        // Pump dispatcher at Loaded priority to let Loaded events process
                        disp.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                    System.Threading.Thread.Sleep(50);
                }
                _openWindows[name] = new RouteInfo { Route = route };
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
                if (_openWindows.TryGetValue(name, out _))
                {
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

        public string VerifyTheme()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    string ResolveBrush(string key)
                    {
                        try
                        {
                            var resource = Application.Current?.FindResource(key);
                            if (resource is SolidColorBrush scb)
                                return BgStr(scb.Color);
                            return resource != null ? "UNSUPPORTED" : "MISSING";
                        }
                        catch
                        {
                            return "MISSING";
                        }
                    }

                    string BgStr(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

                    return $"BG={ResolveBrush("Brush.Background.Primary")}|TXT={ResolveBrush("Brush.Text.Primary")}|SF={ResolveBrush("SurfaceBrush")}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string ExportWBSTemplate(string templateName)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var app = System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                    var templateSvc = new Som3a_WPF_UI.Services.WBS.WBSTemplateService();
                    var templates = templateSvc.ListTemplatesAsync(null).GetAwaiter().GetResult();
                    var match = templates.FirstOrDefault(t =>
                        t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                        return "TEMPLATE_NOT_FOUND: " + templateName;
                    templateSvc.ExportTemplateToExcelAsync(match.Id, app).GetAwaiter().GetResult();
                    return "OK|Exported:" + match.Name;
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string ImportWBSTemplateFromExcel(string category)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var app = System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                    var templateSvc = new Som3a_WPF_UI.Services.WBS.WBSTemplateService();
                    var imported = templateSvc.ImportTemplateFromExcelAsync(app, null, category).GetAwaiter().GetResult();
                    return "OK|Imported:" + imported.Name + "|Category:" + imported.Category;
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string SetWBSCodeMode(string mode)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    if (!System.Enum.TryParse<Som3a_WPF_UI.Services.WBS.WBSCodeMode>(mode, ignoreCase: true, out var parsed))
                        parsed = Som3a_WPF_UI.Services.WBS.WBSCodeMode.Numeric;
                    Som3a_WPF_UI.Services.WBS.WBSCodeGenerator.DefaultMode = parsed;
                    Som3a_WPF_UI.Properties.Settings.Default.WBSCodeMode = mode;
                    Som3a_WPF_UI.Properties.Settings.Default.Save();
                    return "OK";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string SetWBSExportStyle(string style)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    Som3a_WPF_UI.Properties.Settings.Default.WBSExportStyle = style;
                    Som3a_WPF_UI.Properties.Settings.Default.Save();
                    var styleId = style switch
                    {
                        "Blue Gradient" => 2,
                        "Primavera" => 3,
                        "Dark Mode" => 4,
                        "Soft Pastel" => 5,
                        _ => 1
                    };
                    Som3a.Shared.Core.UserSettings.SelectedStyle = styleId;
                    return "OK";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string GetWBSMode()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var mode = Som3a_WPF_UI.Services.WBS.WBSCodeGenerator.DefaultMode.ToString();
                    var styleName = Som3a_WPF_UI.Properties.Settings.Default.WBSExportStyle;
                    var selectedStyle = Som3a.Shared.Core.UserSettings.SelectedStyle;
                    return $"OK|Mode:{mode}|ExportStyle:{styleName}|SelectedStyleId:{selectedStyle}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string GetTemplatePreviewData(string templateName)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var svc = new Som3a_WPF_UI.Services.WBS.WBSTemplateService();
                    var templates = svc.ListTemplatesAsync(null).GetAwaiter().GetResult();
                    var match = templates.FirstOrDefault(t =>
                        t.Name.Equals(templateName, System.StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                        return "TEMPLATE_NOT_FOUND:" + templateName;
                    var template = svc.GetTemplateAsync(match.Id).GetAwaiter().GetResult();
                    if (template?.RootNode == null)
                        return "NO_ROOT_NODE";
                    var gen = new Som3a_WPF_UI.Services.WBS.WBSCodeGenerator
                    {
                        Mode = Som3a_WPF_UI.Services.WBS.WBSCodeMode.Alpha
                    };
                    gen.RenumberSubtree(template.RootNode, template.Name);
                    var lines = new System.Collections.Generic.List<string>();
                    DumpNode(template.RootNode, "", lines);
                    return "OK\n" + string.Join("\n", lines);
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        private static void DumpNode(Som3a_WPF_UI.Services.WBS.WBSNode node, string indent, System.Collections.Generic.List<string> lines)
        {
            lines.Add($"{indent}{node.Code} - {node.Name} (Level {node.Level}, FullPath: {node.FullPath})");
            foreach (var child in node.Children)
                DumpNode(child, indent + "  ", lines);
        }

        private static void PumpDispatcherWhile(System.Func<bool> condition, int timeoutMs = 15000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline && condition())
            {
                var frame = new System.Windows.Threading.DispatcherFrame();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => frame.Continue = false));
                System.Windows.Threading.Dispatcher.PushFrame(frame);
            }
        }

        public string BoqConfigureOllama(string model = "deepseek-coder", string endpoint = "http://localhost:11434")
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    Som3a_WPF_UI.Services.AISettings.IsAIEnabled = true;
                    Som3a_WPF_UI.Services.AISettings.ProviderType = Som3a_WPF_UI.Services.AIProviderType.Ollama;
                    Som3a_WPF_UI.Services.AISettings.OllamaModel = model;
                    Som3a_WPF_UI.Services.AISettings.OllamaEndpoint = endpoint;
                    return $"OK|Ollama:{model}@{endpoint}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        // ---- BOQ Automation ----

        private BOQActivityGeneratorViewModel FindBoqViewModel(int timeoutMs = 10000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                var nav = NavigationService.Instance;
                var shell = nav?.ShellWindow;
                if (shell == null)
                {
                    shell = Application.Current?.Windows
                        .OfType<ShellWindow>()
                        .FirstOrDefault();
                }
                if (shell?.CurrentPage is BOQActivityGeneratorPage page)
                {
                    var vm = page.DataContext as BOQActivityGeneratorViewModel;
                    if (vm != null) return vm;
                }
                // Pump dispatcher instead of blocking the UI thread
                var frame = new System.Windows.Threading.DispatcherFrame();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => frame.Continue = false));
                System.Windows.Threading.Dispatcher.PushFrame(frame);
            }
            return null;
        }

        public string BoqLoad()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindBoqViewModel();
                    if (vm == null) return "ERROR: BOQ page not open";
                    if (vm.LoadBoqCommand.CanExecute(null))
                        vm.LoadBoqCommand.Execute(null);
                    PumpDispatcherWhile(() => vm.IsBusy);
                    return $"OK|BoqItems:{vm.BoqItems?.Count ?? 0}|HasBoqLoaded:{vm.HasBoqLoaded}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string BoqConsent()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindBoqViewModel();
                    if (vm == null) return "ERROR: BOQ page not open";
                    vm.ConsentCommand.Execute(true);
                    return vm.HasConsented ? "OK|Consented:true" : "OK|Consented:false";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string BoqGenerate()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindBoqViewModel();
                    if (vm == null) return "ERROR: BOQ page not open";
                    if (!vm.HasConsented) return "ERROR: Not consented";
                    if (vm.GenerateCommand.CanExecute(null))
                        vm.GenerateCommand.Execute(null);
                    PumpDispatcherWhile(() => vm.IsBusy, 120000);
                    return $"OK|Activities:{vm.Activities?.Count ?? 0}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string BoqGetStatus()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindBoqViewModel();
                    if (vm == null) return "ERROR: BOQ page not open";
                    return $"OK|HasBoqLoaded:{vm.HasBoqLoaded}|HasConsented:{vm.HasConsented}|" +
                           $"IsBusy:{vm.IsBusy}|BoqItems:{vm.BoqItems?.Count ?? 0}|" +
                           $"Activities:{vm.Activities?.Count ?? 0}|CanGenerate:{vm.CanGenerate}|" +
                           $"Status:'{vm.GenerationStatus}'|StatusText:'{vm.StatusText}'";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string BoqGetProviderInfo()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var detected = LocalProviderDetector.Detect();
                    var hasLocal = detected.Count > 0;
                    var hasApiKey = !string.IsNullOrEmpty(AISettings.CloudApiKey);
                    var providerType = AISettings.ProviderType.ToString();
                    var isEnabled = AISettings.IsAIEnabled;
                    var ollamaModel = AISettings.OllamaModel;
                    var ollamaEndpoint = AISettings.OllamaEndpoint;
                    var cloudModel = AISettings.CloudMainModel;

                    var localInfo = hasLocal
                        ? string.Join(";", detected.Select(d => $"{d.Id}@{d.Endpoint}"))
                        : "None";

                    return $"OK|IsEnabled:{isEnabled}|ProviderType:{providerType}|" +
                           $"HasLocalAI:{hasLocal}|HasCloudKey:{hasApiKey}|" +
                           $"LocalProviders:{localInfo}|" +
                           $"OllamaModel:{ollamaModel}|OllamaEndpoint:{ollamaEndpoint}|" +
                           $"CloudModel:{cloudModel}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string BoqDebug()
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var app = Application.Current;
                    var parts = new List<string>();
                    parts.Add($"AppCurr:{app != null}");
                    if (app != null)
                    {
                        var wCount = app.Windows.Count;
                        parts.Add($"WinCount:{wCount}");
                        for (int i = 0; i < wCount; i++)
                        {
                            var w = app.Windows[i];
                            parts.Add($"Win{i}:{w?.GetType().Name ?? "null"}/visible:{w?.IsVisible ?? false}");
                            if (w is ShellWindow sw)
                            {
                                parts.Add($"  SW_CP:{sw.CurrentPage?.GetType().Name ?? "null"}");
                            }
                        }
                    }
                    var nav = NavigationService.Instance;
                    parts.Add($"NavInst:{nav != null}");
                    if (nav != null)
                    {
                        parts.Add($"NavSW:{nav.ShellWindow != null}");
                        if (nav.ShellWindow != null)
                        {
                            parts.Add($"NavSW_CP:{nav.ShellWindow.CurrentPage?.GetType().Name ?? "null"}");
                        }
                    }
                    return string.Join("|", parts);
                }
                catch (Exception ex)
                {
                    return "DEBUG_ERROR: " + ex.Message;
                }
            });
        }
        // ---- Duration Estimator Automation (Phase 22) ----

        private DurationEstimatorPageViewModel FindDeViewModel(int timeoutMs = 10000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                var nav = NavigationService.Instance;
                var shell = nav?.ShellWindow;
                if (shell == null)
                {
                    shell = Application.Current?.Windows
                        .OfType<ShellWindow>()
                        .FirstOrDefault();
                }
                if (shell?.CurrentPage is DurationEstimatorPage page)
                {
                    var vm = page.DataContext as DurationEstimatorPageViewModel;
                    if (vm != null) return vm;
                }
                var frame = new System.Windows.Threading.DispatcherFrame();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => frame.Continue = false));
                System.Windows.Threading.Dispatcher.PushFrame(frame);
            }
            return null;
        }

        public string DeCalculateDuration(string activityId, decimal quantity, decimal rate, int crew, decimal hours)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindDeViewModel();
                    if (vm == null) return "ERROR: Duration Estimator page not open";
                    vm.Quantity = quantity;
                    vm.ProductivityRate = rate;
                    vm.CrewSize = crew;
                    vm.HoursPerDay = hours;
                    if (vm.CalculateCommand.CanExecute(null))
                        vm.CalculateCommand.Execute(null);
                    return $"OK|Estimates:{vm.EstimateCount}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }

        public string DeSearchBenchmarks(string category, string query)
        {
            return InvokeOnUI(() =>
            {
                try
                {
                    var vm = FindDeViewModel();
                    if (vm == null) return "ERROR: Duration Estimator page not open";
                    vm.SelectedCategory = category ?? "All";
                    vm.SearchQuery = query ?? "";
                    if (vm.SearchBenchmarksCommand.CanExecute(null))
                        vm.SearchBenchmarksCommand.Execute(null);
                    return $"OK|Benchmarks:{vm.BenchmarkCount}";
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            });
        }
    }
}
