# Contract: IUnitOfWork

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces`

## Purpose

Provides atomic transaction boundaries across multiple repository operations. Ensures transaction safety, plugin isolation, and rollback support per the enterprise persistence architecture.

## Interface

```csharp
public interface IUnitOfWork : IDisposable
{
    /// Begins a new database transaction using BEGIN IMMEDIATE to acquire
    /// the write lock upfront (prevents SQLITE_BUSY in concurrent scenarios).
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// Commits all changes made within the current transaction.
    /// Throws if no transaction is active.
    Task CommitAsync(CancellationToken ct = default);

    /// Rolls back all changes made within the current transaction.
    /// Safe to call even if no transaction is active (no-op).
    Task RollbackAsync(CancellationToken ct = default);

    /// Returns true if a transaction is currently active.
    bool IsInTransaction { get; }

    // Repository accessors
    ISettingsRepository Settings { get; }
    IAIRepository AI { get; }
    IPluginRepository Plugins { get; }
    ILogRepository Logs { get; }
    ITemplateRepository Templates { get; }
}
```

## Usage Pattern

```csharp
public async Task ConfigurePluginAsync(string pluginId, bool enabled, string apiKey)
{
    using var uow = _unitOfWorkFactory.Create();
    await uow.BeginTransactionAsync(ct);

    try
    {
        await uow.Plugins.SetPluginEnabledAsync(pluginId, enabled, ct);
        await uow.Settings.SetSettingAsync("AI", "ApiKey", apiKey, pluginId, ct);
        await uow.CommitAsync(ct);
    }
    catch
    {
        await uow.RollbackAsync(ct);
        throw;
    }
}
```
