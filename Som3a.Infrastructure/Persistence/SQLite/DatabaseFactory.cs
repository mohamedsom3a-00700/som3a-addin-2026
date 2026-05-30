using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Migrations;

namespace Som3a.Infrastructure.Persistence.SQLite;

public class DatabaseFactory : IDisposable
{
    private readonly SQLiteConfiguration _configuration;
    private readonly ConnectionManager _connectionManager;
    private readonly DatabaseContext _databaseContext;
    private readonly MigrationEngine _migrationEngine;
    private bool _initialized;

    public DatabaseFactory(SQLiteConfiguration configuration)
    {
        _configuration = configuration;
        _connectionManager = new ConnectionManager(configuration);
        _databaseContext = new DatabaseContext(_connectionManager);
        _migrationEngine = new MigrationEngine(configuration.ConnectionString);
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        var dir = Path.GetDirectoryName(_configuration.DatabasePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await _migrationEngine.ApplyMigrationsAsync(ct);
        _initialized = true;
    }

    public IUnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork.UnitOfWork(_connectionManager);
    }

    public ConnectionManager ConnectionManager => _connectionManager;
    public DatabaseContext Context => _databaseContext;
    public SQLiteConfiguration Configuration => _configuration;

    public void Dispose()
    {
        _connectionManager.Dispose();
    }
}
