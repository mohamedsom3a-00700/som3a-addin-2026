using Som3a.Contracts;

namespace Som3a.Diagnostics.Export
{
    public class ExportDiagnostics : IDiagnosticsProvider
    {
        private readonly List<DiagnosticEvent> _events = new();

        public string ProviderId => "export-diagnostics";
        public string ProviderName => "Export Diagnostics";

        public event EventHandler<DiagnosticEvent>? DiagnosticLogged;

        public Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new DiagnosticsSnapshot
            {
                ProviderId = ProviderId,
                Version = "1.0.0",
                ActiveOperationCount = 0
            });
        }

        public Task<HealthReport> ReportHealthAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new HealthReport
            {
                Status = HealthStatus.Healthy,
                StatusMessage = "Export system ready",
                CheckedAt = DateTimeOffset.UtcNow
            });
        }

        public void LogDiagnostic(DiagnosticEvent evt)
        {
            _events.Add(evt);
            DiagnosticLogged?.Invoke(this, evt);
        }

        public void LogExportStarted(string format, int rowCount)
        {
            LogDiagnostic(new DiagnosticEvent
            {
                Level = DiagnosticLevel.Information,
                Source = "Som3a.Exporting",
                Category = "Export.Write",
                Message = $"Export started: {format}, {rowCount} rows"
            });
        }

        public void LogExportCompleted(string format, int rowCount, int sheetCount, TimeSpan duration)
        {
            LogDiagnostic(new DiagnosticEvent
            {
                Level = DiagnosticLevel.Information,
                Source = "Som3a.Exporting",
                Category = "Export.Write",
                Message = $"Export completed: {format}, {rowCount} rows, {sheetCount} sheets, {duration.TotalSeconds:F2}s",
                Properties = new Dictionary<string, object>
                {
                    ["format"] = format,
                    ["rowCount"] = rowCount,
                    ["sheetCount"] = sheetCount,
                    ["durationMs"] = duration.TotalMilliseconds
                }
            });
        }

        public void LogExportError(string format, string error)
        {
            LogDiagnostic(new DiagnosticEvent
            {
                Level = DiagnosticLevel.Error,
                Source = "Som3a.Exporting",
                Category = "Export.Write",
                Message = $"Export error: {format} — {error}"
            });
        }
    }
}
