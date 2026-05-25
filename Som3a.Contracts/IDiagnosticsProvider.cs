namespace Som3a.Contracts
{
    public interface IDiagnosticsProvider
    {
        string ProviderId { get; }
        string ProviderName { get; }

        Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default);
        Task<HealthReport> ReportHealthAsync(CancellationToken ct = default);
        void LogDiagnostic(DiagnosticEvent evt);
        event EventHandler<DiagnosticEvent>? DiagnosticLogged;
    }

    public enum DiagnosticLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    public class DiagnosticEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public DiagnosticLevel Level { get; set; } = DiagnosticLevel.Information;
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? Properties { get; set; }
        public string? ExceptionDetail { get; set; }
    }

    public class DiagnosticsSnapshot
    {
        public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;
        public string ProviderId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public long MemoryUsageBytes { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ActiveOperationCount { get; set; }
        public Dictionary<string, object>? Metrics { get; set; }
    }

    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public class HealthReport
    {
        public HealthStatus Status { get; set; } = HealthStatus.Healthy;
        public string StatusMessage { get; set; } = string.Empty;
        public List<HealthCheck> Checks { get; set; } = new();
        public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class HealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public HealthStatus Status { get; set; } = HealthStatus.Healthy;
        public string? Detail { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
