# Contract: IAIRepository

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces`

## Purpose

Persists and queries AI execution history and runtime token usage data.

## Interface

```csharp
public interface IAIRepository
{
    /// Records a completed AI execution (prompt + response + metadata).
    Task LogExecutionAsync(AIExecutionRecord record, CancellationToken ct = default);

    /// Retrieves an execution record by ID.
    Task<AIExecutionRecord?> GetExecutionAsync(Guid id, CancellationToken ct = default);

    /// Queries execution history with optional filters.
    Task<IReadOnlyList<AIExecutionRecord>> QueryExecutionsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? providerName = null,
        string? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    /// Returns total execution count matching the given filters.
    Task<int> CountExecutionsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? providerName = null,
        string? status = null,
        CancellationToken ct = default);

    /// Updates or creates a runtime aggregation record for a session.
    Task UpsertRuntimeRecordAsync(AIRuntimeRecord record, CancellationToken ct = default);

    /// Gets runtime aggregation for a session.
    Task<AIRuntimeRecord?> GetRuntimeRecordAsync(string sessionId, string providerName, CancellationToken ct = default);

    /// Gets all runtime records for a session.
    Task<IReadOnlyList<AIRuntimeRecord>> GetSessionRuntimeRecordsAsync(string sessionId, CancellationToken ct = default);

    /// Deletes execution and runtime records older than the specified cutoff date.
    Task<int> CleanupAsync(DateTime cutoffDate, CancellationToken ct = default);
}
```
