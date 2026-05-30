using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Som3a.Contracts;

namespace Som3a.Diagnostics
{
    public class CrashDiagnosticsCollector : IDisposable
    {
        private readonly string _crashReportPath;
        private readonly List<DiagnosticEvent> _recentOperations;
        private const int MaxRecentOperations = 50;
        private bool _disposed;

        public CrashDiagnosticsCollector(string crashReportPath)
        {
            _crashReportPath = crashReportPath;
            _recentOperations = new List<DiagnosticEvent>();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public void RecordOperation(DiagnosticEvent evt)
        {
            lock (_recentOperations)
            {
                _recentOperations.Add(evt);
                if (_recentOperations.Count > MaxRecentOperations)
                    _recentOperations.RemoveAt(0);
            }
        }

        public DiagnosticsSnapshot CaptureSnapshot(Exception? ex = null)
        {
            var proc = Process.GetCurrentProcess();
            var snapshot = new DiagnosticsSnapshot
            {
                CapturedAt = DateTimeOffset.UtcNow,
                ProviderId = "crash-diagnostics",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                MemoryUsageBytes = proc.WorkingSet64,
                Uptime = DateTime.Now - proc.StartTime,
                ActiveOperationCount = _recentOperations.Count,
                Metrics = new Dictionary<string, object>
                {
                    ["process.id"] = proc.Id,
                    ["process.name"] = proc.ProcessName,
                    ["os.platform"] = RuntimeInformation.OSDescription,
                    ["os.architecture"] = RuntimeInformation.OSArchitecture.ToString(),
                    ["runtime.version"] = RuntimeInformation.FrameworkDescription,
                    ["active.plugins"] = AppDomain.CurrentDomain.GetAssemblies()
                        .Count(a => a.FullName?.StartsWith("Som3a.") == true),
                    ["recent.operations"] = _recentOperations.Count
                }
            };

            if (ex != null)
            {
                snapshot.Metrics["error.type"] = ex.GetType().FullName ?? "Unknown";
                snapshot.Metrics["error.message"] = ex.Message;
                snapshot.Metrics["error.stacktrace"] = ex.ToString();
                if (ex.InnerException != null)
                    snapshot.Metrics["error.inner"] = ex.InnerException.Message;
            }

            return snapshot;
        }

        public async Task<string> ExportSnapshotAsync(DiagnosticsSnapshot snapshot)
        {
            Directory.CreateDirectory(_crashReportPath);

            var timestamp = snapshot.CapturedAt.ToString("yyyyMMdd-HHmmss");
            var fileName = $"crash-{timestamp}.json";
            var filePath = Path.Combine(_crashReportPath, fileName);

            var exportData = new
            {
                capturedAt = snapshot.CapturedAt,
                providerId = snapshot.ProviderId,
                version = snapshot.Version,
                memoryMB = snapshot.MemoryUsageBytes / (1024.0 * 1024.0),
                uptime = snapshot.Uptime.ToString(@"dd\.hh\:mm\:ss"),
                activeOperationCount = snapshot.ActiveOperationCount,
                metrics = snapshot.Metrics
            };

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            return filePath;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                var snapshot = CaptureSnapshot(ex);
                var filePath = ExportSnapshotAsync(snapshot).GetAwaiter().GetResult();
                Trace.TraceError($"Crash diagnostics written to {filePath}");
            }
            catch
            {
                // Silently handle — crash recovery must not cause secondary failures
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                _disposed = true;
            }
        }
    }
}
