namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync(CancellationToken ct = default);

    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);

    bool IsInTransaction { get; }

    ISettingsRepository Settings { get; }
    IAIRepository AI { get; }
    IPluginRepository Plugins { get; }
    ILogRepository Logs { get; }
    ITemplateRepository Templates { get; }
}
