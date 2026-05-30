using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Threading;
using Som3a.Bridge;
using Som3a_WPF_UI.Services;
using WpfApp2 = Som3a_WPF_UI;

namespace Som3a_Addin_2026
{
    public static class Som3aAddinBridge
    {
        private static Application _wpfApp;
        private static Dispatcher _wpfDispatcher;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            if (Application.Current == null)
            {
                _wpfApp = new Application();
                _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _wpfDispatcher = Dispatcher.CurrentDispatcher;
            }

            WpfApp2.Services.ThemeManager.LoadSettings();
            _initialized = true;
        }

        public static void OpenSettings()
        {
            if (_wpfDispatcher != null && _wpfDispatcher.CheckAccess())
            {
                NavigationService.Instance.NavigateTo("settings.general");
                return;
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    NavigationService.Instance.NavigateTo("settings.general"));
            }
            else
            {
                var thread = new System.Threading.Thread(() =>
                {
                    _wpfApp = new Application();
                    _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    _wpfDispatcher = Dispatcher.CurrentDispatcher;
                    WpfApp2.Services.ThemeManager.LoadSettings();
                    NavigationService.Instance.NavigateTo("settings.general");
                    Dispatcher.Run();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public static void Shutdown()
        {
            _wpfApp?.Shutdown();
            _wpfApp = null;
            _initialized = false;
        }

        public static object ExecuteCommand(PipeMessage message)
        {
            try
            {
                if (message.Payload is not JsonElement payload)
                    return new ExcelResponsePayload { Success = false, Error = "Invalid payload" };

                var cmd = payload.ValueKind switch
                {
                    JsonValueKind.String => Enum.Parse<ExcelCommandType>(payload.GetString(), ignoreCase: true),
                    _ => JsonSerializer.Deserialize<ExcelCommandPayload>(payload.GetRawText(), _jsonOptions)?.Cmd
                        ?? throw new InvalidOperationException("Missing command type")
                };

                var sheet = payload.TryGetProperty("sheet", out var s) ? s.GetString() : null;
                var row = payload.TryGetProperty("row", out var r) ? r.GetInt32() : 0;
                var col = payload.TryGetProperty("col", out var c) ? c.GetInt32() : 0;
                var value = payload.TryGetProperty("value", out var v) ? v.GetString() : null;
                var range = payload.TryGetProperty("range", out var rng) ? rng.GetString() : null;

                var excelApp = Globals.ThisAddIn.Application;

                switch (cmd)
                {
                    case ExcelCommandType.WriteCell:
                        var ws = excelApp.ActiveWorkbook?.Sheets[sheet ?? excelApp.ActiveSheet.Name];
                        if (ws != null && row > 0 && col > 0)
                            ws.Cells[row, col] = value;
                        return new ExcelResponsePayload { Success = true };

                    case ExcelCommandType.ReadCell:
                        var readWs = excelApp.ActiveWorkbook?.Sheets[sheet ?? excelApp.ActiveSheet.Name];
                        if (readWs != null && row > 0 && col > 0)
                        {
                            var cellValue = readWs.Cells[row, col]?.Value?.ToString();
                            return new ExcelResponsePayload { Success = true, Data = cellValue };
                        }
                        return new ExcelResponsePayload { Success = false, Error = "Invalid cell" };

                    case ExcelCommandType.GetSheetNames:
                        var names = new System.Collections.Generic.List<string>();
                        foreach (Excel.Worksheet sht in excelApp.ActiveWorkbook?.Sheets)
                            names.Add(sht.Name);
                        return new ExcelResponsePayload { Success = true, Data = names };

                    case ExcelCommandType.ActivateSheet:
                        var target = excelApp.ActiveWorkbook?.Sheets[sheet];
                        if (target != null)
                            target.Activate();
                        return new ExcelResponsePayload { Success = target != null };

                    default:
                        return new ExcelResponsePayload { Success = false, Error = $"Unsupported command: {cmd}" };
                }
            }
            catch (Exception ex)
            {
                return new ExcelResponsePayload { Success = false, Error = ex.Message };
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }
}