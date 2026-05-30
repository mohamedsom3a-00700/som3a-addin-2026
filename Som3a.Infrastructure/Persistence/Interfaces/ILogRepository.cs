using Som3a.Infrastructure.Persistence.Models;

namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface ILogRepository
{
    Task WriteLogAsync(DiagnosticsLog entry, CancellationToken ct = default);

    Task WriteLogBatchAsync(IReadOnlyList<DiagnosticsLog> entries, CancellationToken ct = default);

    Task<IReadOnlyList<DiagnosticsLog>> QueryLogsAsync(
        string? minSeverity = null,
        DateTime? from = null,
        DateTime? to = null,
        string? component = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    Task RecordCrashAsync(CrashReport report, CancellationToken ct = default);

    Task<IReadOnlyList<CrashReport>> GetRecentCrashesAsync(int count = 10, CancellationToken ct = default);

    Task RecordExportAsync(ExportHistoryRecord record, CancellationToken ct = default);

    Task<IReadOnlyList<ExportHistoryRecord>> QueryExportHistoryAsync(
        string? format = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    Task<int> CleanupLogsAsync(DateTime cutoffDate, CancellationToken ct = default);

    Task<int> CleanupCrashesAsync(DateTime cutoffDate, CancellationToken ct = default);

    Task<int> CleanupExportHistoryAsync(DateTime cutoffDate, CancellationToken ct = default);
}
