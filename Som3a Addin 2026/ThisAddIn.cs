using Microsoft.Office.Tools.Excel;
using Som3a.Bridge;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

namespace Som3a_Addin_2026
{
    public partial class ThisAddIn
    {
        private IAddInAutomation _automation;
        private NamedPipeServerStream _pipeServer;
        private CancellationTokenSource _pipeCts;
        private StreamReader _pipeReader;
        private StreamWriter _pipeWriter;
        private StreamWriter _heartbeatWriter;
        private JsonSerializerOptions _heartbeatOptions;
        private readonly SemaphoreSlim _writeSemaphore = new(1, 1);
        private System.Timers.Timer _heartbeatTimer;
        private int _heartbeatSequence;
        private bool _wpfLaunched;

        protected override object RequestComAddInAutomationService()
        {
            if (_automation == null)
                _automation = new AddInAutomation();
            return _automation;
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            System.Windows.Application app = System.Windows.Application.Current;

            if (app == null)
            {
                app = new System.Windows.Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
            }

            Som3a_WPF_UI.CompositionRoot.RegisterServices(Som3a_WPF_UI.App.Container);
            Som3a_WPF_UI.CompositionRoot.InitializeModules(Som3a_WPF_UI.App.Container.Resolve<Som3a_WPF_UI.Services.IModuleRegistry>());

            try
            {
                var pluginLoader = Som3a_WPF_UI.App.Container.Resolve<PluginLoader>();
                var orchestrator = Som3a_WPF_UI.App.Container.Resolve<ModuleLoadOrchestrator>();

                try
                {
                    orchestrator.SetNavigationService(NavigationService.Instance);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Failed to set navigation service: {ex.Message}");
                }

                try
                {
                    var manifests = pluginLoader.DiscoverModules();
                    orchestrator.OnModulesDiscovered(manifests);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Module discovery failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Plugin bootstrap failed: {ex.Message}");
            }

            ThemeManager.InitializeApplicationResources();

            ThemeManager.LoadSettings();

            try
            {
                var wbsMode = Som3a_WPF_UI.Properties.Settings.Default.WBSCodeMode;
                Som3a_WPF_UI.Services.WBS.WBSCodeGenerator.DefaultMode =
                    wbsMode == "Alpha" ? Som3a_WPF_UI.Services.WBS.WBSCodeMode.Alpha : Som3a_WPF_UI.Services.WBS.WBSCodeMode.Numeric;
            }
            catch { }

            try
            {
                var sidebarRegistration = Som3a_WPF_UI.App.Container.Resolve<ISidebarRegistrationProvider>();
                sidebarRegistration.RegisterStaticPages();

                var orchestrator = Som3a_WPF_UI.App.Container.Resolve<ModuleLoadOrchestrator>();
                var pluginTypes = orchestrator.GetAllPluginPageTypes();
                if (pluginTypes.Count > 0)
                    sidebarRegistration.RegisterPluginPages(pluginTypes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Sidebar registration failed: {ex.Message}");
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            StopPipeServer();
        }

        public async Task LaunchWpfProcessAsync()
        {
            if (_wpfLaunched) return;
            _wpfLaunched = true;

            try
            {
                var wpfPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..\\WpfApp2\\bin\\Debug\\net8.0-windows\\Som3a_WPF_UI.exe");

                if (!File.Exists(wpfPath))
                {
                    wpfPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Som3a\\Som3a_WPF_UI.exe");
                }

                if (!File.Exists(wpfPath))
                {
                    System.Diagnostics.Trace.WriteLine($"[ThisAddIn] WPF binary not found at: {wpfPath}");
                    return;
                }

                _pipeCts = new CancellationTokenSource();

                _pipeServer = new NamedPipeServerStream(
                    PipeConstants.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = wpfPath,
                        Arguments = "-standalone",
                        UseShellExecute = true
                    }
                };
                process.Start();

                System.Diagnostics.Trace.WriteLine("[ThisAddIn] Waiting for WPF pipe connection...");
                await _pipeServer.WaitForConnectionAsync(_pipeCts.Token);
                System.Diagnostics.Trace.WriteLine("[ThisAddIn] WPF pipe connected.");

                _pipeReader = new StreamReader(_pipeServer, Encoding.UTF8, leaveOpen: true);
                _pipeWriter = new StreamWriter(_pipeServer, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

                var handshakeJson = await _pipeReader.ReadLineAsync();
                if (string.IsNullOrEmpty(handshakeJson))
                {
                    System.Diagnostics.Trace.WriteLine("[ThisAddIn] No handshake received.");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                var handshakeMessage = JsonSerializer.Deserialize<PipeMessage>(handshakeJson, options);
                if (handshakeMessage?.Type != MessageType.handshake)
                {
                    System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Unexpected message type: {handshakeMessage?.Type}");
                    return;
                }

                var excelHwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle.ToInt32();
                var pid = System.Diagnostics.Process.GetCurrentProcess().Id;

                var response = new PipeMessage
                {
                    Type = MessageType.handshake,
                    CorrelationId = handshakeMessage.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    Payload = new { excelHwnd, processId = pid, version = PipeConstants.ProtocolVersion }
                };

                var responseJson = JsonSerializer.Serialize(response, options);
                await _writeSemaphore.WaitAsync();
                try
                {
                    await _pipeWriter.WriteLineAsync(responseJson);
                }
                finally
                {
                    _writeSemaphore.Release();
                }

                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Handshake complete. HWND={excelHwnd}, PID={pid}");

                _heartbeatWriter = _pipeWriter;
                _heartbeatOptions = options;
                StartHeartbeatTimer();

                _ = HandlePipeMessagesAsync(_pipeReader, _pipeWriter, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Cold start failed: {ex.Message}");
            }
        }

        private async Task HandlePipeMessagesAsync(StreamReader reader, StreamWriter writer, JsonSerializerOptions options)
        {
            try
            {
                while (!_pipeCts.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var message = JsonSerializer.Deserialize<PipeMessage>(line, options);
                    if (message == null) continue;

                    switch (message.Type)
                    {
                        case MessageType.heartbeat_ack:
                            System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Heartbeat ack: seq={message.CorrelationId}");
                            break;
                        case MessageType.excel_command:
                            await HandleExcelCommandAsync(message, writer, options);
                            break;
                        case MessageType.shutdown_ack:
                            System.Diagnostics.Trace.WriteLine("[ThisAddIn] WPF acknowledged shutdown.");
                            break;
                        default:
                            System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Unhandled message type: {message.Type}");
                            break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Pipe handler error: {ex.Message}");
            }
        }

        private async Task HandleExcelCommandAsync(PipeMessage message, StreamWriter writer, JsonSerializerOptions options)
        {
            try
            {
                var result = Som3aAddinBridge.ExecuteCommand(message);
                var response = new PipeMessage
                {
                    Type = MessageType.excel_response,
                    CorrelationId = message.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    Payload = result
                };
                var json = JsonSerializer.Serialize(response, options);
                await _writeSemaphore.WaitAsync();
                try
                {
                    await writer.WriteLineAsync(json);
                }
                finally
                {
                    _writeSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Command execution error: {ex.Message}");
            }
        }

        private void StartHeartbeatTimer()
        {
            _heartbeatTimer = new System.Timers.Timer(PipeConstants.HeartbeatIntervalMs);
            _heartbeatTimer.Elapsed += async (_, _) =>
            {
                try
                {
                    _heartbeatSequence++;
                    var heartbeat = new PipeMessage
                    {
                        Type = MessageType.heartbeat,
                        CorrelationId = Guid.NewGuid().ToString("D"),
                        Timestamp = DateTime.UtcNow,
                        Payload = new { sequence = _heartbeatSequence, status = "ok" }
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(heartbeat, _heartbeatOptions);
                    await _writeSemaphore.WaitAsync();
                    try
                    {
                        await _heartbeatWriter.WriteLineAsync(json);
                    }
                    finally
                    {
                        _writeSemaphore.Release();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[ThisAddIn] Heartbeat error: {ex.Message}");
                }
            };
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
        }

        private void StopPipeServer()
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;

            if (_pipeServer?.IsConnected == true)
            {
                try
                {
                    var shutdown = new PipeMessage
                    {
                        Type = MessageType.shutdown,
                        CorrelationId = Guid.NewGuid().ToString("D"),
                        Timestamp = DateTime.UtcNow,
                        Payload = new { reason = "ExcelClosing" }
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(shutdown,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                        });
                    _heartbeatWriter?.WriteLineAsync(json);
                    _heartbeatWriter?.Flush();
                }
                catch { }
            }

            _pipeCts?.Cancel();
            _pipeReader?.Dispose();
            _pipeReader = null;
            _pipeWriter?.Dispose();
            _pipeWriter = null;
            _heartbeatWriter = null;
            _pipeServer?.Dispose();
            _pipeServer = null;
            _writeSemaphore.Dispose();
        }
    }
}

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
