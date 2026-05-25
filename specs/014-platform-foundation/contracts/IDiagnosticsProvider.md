# Contract: IDiagnosticsProvider

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the contract for diagnostic data collection and reporting across all platform layers. .NET 8.0 libraries emit structured diagnostics through this contract's channel to the existing DiagnosticsService in the WPF host.

## Interface

```csharp
public interface IDiagnosticsProvider
{
    string ProviderId { get; }
    string ProviderName { get; }

    Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default);
    Task<HealthReport> ReportHealthAsync(CancellationToken ct = default);
    void LogDiagnostic(DiagnosticEvent evt);
    event EventHandler<DiagnosticEvent>? DiagnosticLogged;
}
```

## Diagnostic Event

```csharp
public class DiagnosticEvent
{
    public string EventId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DiagnosticLevel Level { get; set; }
    public string Source { get; set; }              // e.g., "Som3a.AI", "Som3a.Exporting"
    public string Category { get; set; }            // e.g., "AI.Request", "Plugin.Load", "Export.Write"
    public string Message { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public string? ExceptionDetail { get; set; }
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

public class DiagnosticsSnapshot
{
    public DateTimeOffset CapturedAt { get; set; }
    public string ProviderId { get; set; }
    public string Version { get; set; }
    public long MemoryUsageBytes { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ActiveOperationCount { get; set; }
    public Dictionary<string, object>? Metrics { get; set; }
}

public class HealthReport
{
    public HealthStatus Status { get; set; }        // Healthy / Degraded / Unhealthy
    public string StatusMessage { get; set; }
    public List<HealthCheck> Checks { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}

public class HealthCheck
{
    public string Name { get; set; }
    public HealthStatus Status { get; set; }
    public string? Detail { get; set; }
    public TimeSpan Duration { get; set; }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
```

## Shared Channel Pattern

.NET 8.0 libraries emit `DiagnosticEvent` instances through the interop bridge to the WPF host's `DiagnosticsService`:

```text
.NET 8.0 Library
    ↓ DiagnosticEvent (serialized as JSON)
Som3a.Bridge (DiagnosticsChannel)
    ↓
WPF Host (DiagnosticsService)
    ↓
AppData/Som3a/Logs/ (5MB rollover, 3-file rotation)
```

## Event Categories

| Category | Source | Events |
|----------|--------|--------|
| AI.Request | Som3a.AI | Prompt sent, response received, token usage, error, timeout |
| Plugin.Load | Som3a.Plugin.SDK | Plugin discovered, validated, initialized, failed, unloaded |
| Plugin.Conflict | Som3a.Plugin.SDK | Navigation order conflict, settings section name conflict |
| Export.Write | Som3a.Exporting | Export started, completed, failed, row count, duration |
| Bridge.Error | Som3a.Bridge | Interop failure, marshalling error, graceful degradation activated |
| Security.KeyStore | Som3a.Infrastructure | Key encrypted, key decrypted, access denied |
