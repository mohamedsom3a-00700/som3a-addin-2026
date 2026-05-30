using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Repositories;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ConnectionManager _connectionManager;
    private SqliteTransaction? _transaction;
    private SqliteConnection? _connection;
    private bool _disposed;

    private ISettingsRepository? _settings;
    private IAIRepository? _ai;
    private IPluginRepository? _plugins;
    private ILogRepository? _logs;
    private ITemplateRepository? _templates;

    public UnitOfWork(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction is already active.");

        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            _connection = await _connectionManager.GetWriteConnectionAsync(ct);
            _transaction = _connection.BeginTransaction();
        }
        catch
        {
            _connectionManager.ReleaseWriteGate();
            throw;
        }

        _connectionManager.ReleaseWriteGate();
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _transaction.CommitAsync(ct);
        _transaction.Dispose();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public bool IsInTransaction => _transaction is not null;

    public ISettingsRepository Settings =>
        _settings ??= new SettingsRepository(_connectionManager);

    public IAIRepository AI =>
        _ai ??= new AIRepository(_connectionManager);

    public IPluginRepository Plugins =>
        _plugins ??= new PluginRepository(_connectionManager);

    public ILogRepository Logs =>
        _logs ??= new DiagnosticsRepository(_connectionManager);

    public ITemplateRepository Templates =>
        _templates ??= new TemplateRepository(_connectionManager);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_transaction is not null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
    }
}
