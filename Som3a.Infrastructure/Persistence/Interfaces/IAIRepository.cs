using Som3a.Infrastructure.Persistence.Models;

namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface IAIRepository
{
    Task LogExecutionAsync(AIExecutionRecord record, CancellationToken ct = default);

    Task<AIExecutionRecord?> GetExecutionAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AIExecutionRecord>> QueryExecutionsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? providerName = null,
        string? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    Task<int> CountExecutionsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? providerName = null,
        string? status = null,
        CancellationToken ct = default);

    Task UpsertRuntimeRecordAsync(AIRuntimeRecord record, CancellationToken ct = default);

    Task<AIRuntimeRecord?> GetRuntimeRecordAsync(string sessionId, string providerName, CancellationToken ct = default);

    Task<IReadOnlyList<AIRuntimeRecord>> GetSessionRuntimeRecordsAsync(string sessionId, CancellationToken ct = default);

    Task<int> CleanupAsync(DateTime cutoffDate, CancellationToken ct = default);
}
