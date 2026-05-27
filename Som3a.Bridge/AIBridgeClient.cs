using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a.Bridge
{
    public class AIBridgeClient : IAIBridge, IDisposable
    {
        public const string DefaultPipeName = "Som3a.AI.Bridge";

        private readonly string _pipeName;
        private readonly string _hostPath;
        private Process? _hostProcess;
        private NamedPipeClientStream? _pipeClient;
        private bool _disposed;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public AIBridgeClient(string? hostPath = null, string? pipeName = null)
        {
            _pipeName = pipeName ?? DefaultPipeName;
            _hostPath = hostPath ?? FindDefaultHostPath();
        }

        public bool IsHostRunning =>
            _hostProcess != null && !_hostProcess.HasExited;

        public async Task StartHostAsync()
        {
            if (IsHostRunning) return;

            if (!File.Exists(_hostPath))
            {
                var logPath = Path.Combine(Path.GetTempPath(), "Som3a.AI.Host.log");
                try { File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] Host not found at: {_hostPath}. BaseDir={AppDomain.CurrentDomain.BaseDirectory}{Environment.NewLine}"); } catch { }
                throw new FileNotFoundException(
                    $"AI host executable not found at: {_hostPath}. " +
                    $"BaseDir={AppDomain.CurrentDomain.BaseDirectory}. " +
                    "Build Som3a.AI.Host project first.", _hostPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = _hostPath,
                Arguments = _pipeName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            _hostProcess = new Process { StartInfo = psi };
            _hostProcess.Start();

            // Wait for host to be ready (connect with retries)
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
                    await _pipeClient.ConnectAsync(1000);
                    return;
                }
                catch
                {
                    await Task.Delay(200);
                }
            }

            throw new TimeoutException("Failed to connect to AI host within 7 seconds.");
        }

        public async Task StopHostAsync()
        {
            if (_hostProcess != null && !_hostProcess.HasExited)
            {
                try { _hostProcess.Kill(); } catch { }
                _hostProcess.Dispose();
                _hostProcess = null;
            }

            if (_pipeClient != null)
            {
                _pipeClient.Dispose();
                _pipeClient = null;
            }
        }

        public async Task<AIBridgeResponse> ExecutePromptAsync(AIBridgeRequest request, CancellationToken ct = default)
        {
            if (!IsHostRunning || _pipeClient == null)
                await StartHostAsync();

            ct.ThrowIfCancellationRequested();

            var requestJson = request.Serialize();
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            try
            {
                await _pipeClient.WriteAsync(BitConverter.GetBytes(requestBytes.Length), 0, 4, ct);
                await _pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length, ct);
                await _pipeClient.FlushAsync(ct);

                var lenBuffer = new byte[4];
                var bytesRead = 0;
                while (bytesRead < 4)
                {
                    var n = await _pipeClient.ReadAsync(lenBuffer, bytesRead, 4 - bytesRead, ct);
                    if (n == 0) throw new EndOfStreamException("Pipe closed by host.");
                    bytesRead += n;
                }

                var responseLen = BitConverter.ToInt32(lenBuffer, 0);
                if (responseLen <= 0 || responseLen > 10 * 1024 * 1024)
                    throw new InvalidDataException($"Invalid response length: {responseLen}");

                var responseBytes = new byte[responseLen];
                bytesRead = 0;
                while (bytesRead < responseLen)
                {
                    var n = await _pipeClient.ReadAsync(responseBytes, bytesRead, responseLen - bytesRead, ct);
                    if (n == 0) throw new EndOfStreamException("Pipe closed by host.");
                    bytesRead += n;
                }

                var responseJson = Encoding.UTF8.GetString(responseBytes);
                var response = AIBridgeResponse.Deserialize(responseJson);
                return response ?? new AIBridgeResponse { IsSuccess = false, ErrorMessage = "Failed to deserialize response" };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return new AIBridgeResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Pipe communication error: {ex.Message}"
                };
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_hostProcess != null && !_hostProcess.HasExited)
            {
                try { _hostProcess.Kill(); } catch { }
                _hostProcess.Dispose();
            }
            _pipeClient?.Dispose();
        }

        private static string FindDefaultHostPath()
        {
            try
            {
                var assemblyPath = new Uri(typeof(AIBridgeClient).Assembly.Location).LocalPath;
                var dir = Path.GetDirectoryName(assemblyPath);
                if (dir != null)
                {
                    // Check AIHost subdirectory first (clean .NET 8.0 deployment)
                    var aiHostDir = Path.Combine(dir, "AIHost");
                    var candidate = Path.Combine(aiHostDir, "Som3a.AI.Host.exe");
                    if (File.Exists(candidate))
                        return candidate;

                    // Check assembly directory
                    candidate = Path.Combine(dir, "Som3a.AI.Host.exe");
                    if (File.Exists(candidate))
                        return candidate;

                    // Check parent directory (shadow copy scenarios)
                    var parentDir = Path.GetDirectoryName(dir);
                    if (parentDir != null)
                    {
                        candidate = Path.Combine(parentDir, "Som3a.AI.Host.exe");
                        if (File.Exists(candidate))
                            return candidate;

                        aiHostDir = Path.Combine(parentDir, "AIHost");
                        candidate = Path.Combine(aiHostDir, "Som3a.AI.Host.exe");
                        if (File.Exists(candidate))
                            return candidate;
                    }
                }
            }
            catch { }

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var baseAiHost = Path.Combine(baseDir, "AIHost", "Som3a.AI.Host.exe");
            if (File.Exists(baseAiHost))
                return baseAiHost;
            return Path.Combine(baseDir, "Som3a.AI.Host.exe");
        }
    }
}
