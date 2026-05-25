using Som3a.Contracts;

namespace Som3a.Diagnostics.AI
{
    public class AIRequestLogger : IDiagnosticsProvider
    {
        private readonly List<DiagnosticEvent> _events = new();

        public string ProviderId => "ai-request-logger";
        public string ProviderName => "AI Request Logger";

        public event EventHandler<DiagnosticEvent>? DiagnosticLogged;

        public Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new DiagnosticsSnapshot
            {
                ProviderId = ProviderId,
                Version = "1.0.0",
                ActiveOperationCount = _events.Count
            });
        }

        public Task<HealthReport> ReportHealthAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new HealthReport
            {
                Status = HealthStatus.Healthy,
                StatusMessage = "AI logging operational",
                CheckedAt = DateTimeOffset.UtcNow
            });
        }

        public void LogDiagnostic(DiagnosticEvent evt)
        {
            _events.Add(evt);
            DiagnosticLogged?.Invoke(this, evt);
        }

        public void LogRequest(string providerId, string promptPreview, TimeSpan duration, int promptTokens, int completionTokens)
        {
            LogDiagnostic(new DiagnosticEvent
            {
                Level = DiagnosticLevel.Information,
                Source = "Som3a.AI",
                Category = "AI.Request",
                Message = $"AI request to {providerId} — {promptTokens}P + {completionTokens}C tokens in {duration.TotalSeconds:F2}s",
                Properties = new Dictionary<string, object>
                {
                    ["providerId"] = providerId,
                    ["promptTokens"] = promptTokens,
                    ["completionTokens"] = completionTokens,
                    ["durationMs"] = duration.TotalMilliseconds
                }
            });
        }

        public void LogError(string providerId, string error)
        {
            LogDiagnostic(new DiagnosticEvent
            {
                Level = DiagnosticLevel.Error,
                Source = "Som3a.AI",
                Category = "AI.Request",
                Message = $"AI error from {providerId}: {error}"
            });
        }
    }
}
