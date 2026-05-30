# Contract: ILogRepository

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces`

## Purpose

Persists diagnostics logs, crash reports, and export history for auditing and troubleshooting.

## Interface

```csharp
public interface ILogRepository
{
    /// Writes a single diagnostics log entry.
    Task WriteLogAsync(DiagnosticsLog entry, CancellationToken ct = default);

    /// Writes multiple log entries in a batch (used by batched diagnostics service).
    Task WriteLogBatchAsync(IReadOnlyList<DiagnosticsLog> entries, CancellationToken ct = default);

    /// Queries log entries with severity and date range filtering.
    Task<IReadOnlyList<DiagnosticsLog>> QueryLogsAsync(
        string? minSeverity = null,
        DateTime? from = null,
        DateTime? to = null,
        string? component = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    /// Records a crash report.
    Task RecordCrashAsync(CrashReport report, CancellationToken ct = default);

    /// Retrieves recent crash reports.
    Task<IReadOnlyList<CrashReport>> GetRecentCrashesAsync(int count = 10, CancellationToken ct = default);

    /// Records a completed export operation.
    Task RecordExportAsync(ExportHistoryRecord record, CancellationToken ct = default);

    /// Queries export history with format and date range filtering.
    Task<IReadOnlyList<ExportHistoryRecord>> QueryExportHistoryAsync(
        string? format = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    /// Deletes log records older than the specified cutoff date.
    Task<int> CleanupLogsAsync(DateTime cutoffDate, CancellationToken ct = default);

    /// Deletes crash reports older than the specified cutoff date.
    Task<int> CleanupCrashesAsync(DateTime cutoffDate, CancellationToken ct = default);

    /// Deletes export history records older than the specified cutoff date.
    Task<int> CleanupExportHistoryAsync(DateTime cutoffDate, CancellationToken ct = default);
}
```
