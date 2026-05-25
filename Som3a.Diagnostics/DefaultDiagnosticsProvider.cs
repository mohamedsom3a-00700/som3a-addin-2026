using Som3a.Contracts;

namespace Som3a.Diagnostics
{
    public class DefaultDiagnosticsProvider : IDiagnosticsProvider
    {
        private readonly List<DiagnosticEvent> _events = new();

        public string ProviderId { get; }
        public string ProviderName { get; }

        public event EventHandler<DiagnosticEvent>? DiagnosticLogged;

        public DefaultDiagnosticsProvider(string providerId, string providerName)
        {
            ProviderId = providerId;
            ProviderName = providerName;
        }

        public Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default)
        {
            var snapshot = new DiagnosticsSnapshot
            {
                ProviderId = ProviderId,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                MemoryUsageBytes = GC.GetTotalMemory(false),
                Uptime = TimeSpan.Zero,
                ActiveOperationCount = _events.Count
            };
            return Task.FromResult(snapshot);
        }

        public Task<HealthReport> ReportHealthAsync(CancellationToken ct = default)
        {
            var report = new HealthReport
            {
                Status = HealthStatus.Healthy,
                StatusMessage = "Operational",
                CheckedAt = DateTimeOffset.UtcNow,
                Checks = new List<HealthCheck>
                {
                    new HealthCheck
                    {
                        Name = "Memory",
                        Status = HealthStatus.Healthy,
                        Duration = TimeSpan.Zero
                    }
                }
            };
            return Task.FromResult(report);
        }

        public void LogDiagnostic(DiagnosticEvent evt)
        {
            _events.Add(evt);
            DiagnosticLogged?.Invoke(this, evt);
        }
    }
}
