using Microsoft.Data.Sqlite;

namespace Som3a.Infrastructure.Persistence.SQLite;

public class ConnectionManager : IDisposable
{
    private readonly SQLiteConfiguration _configuration;
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private SqliteConnection? _writeConnection;
    private bool _disposed;

    public ConnectionManager(SQLiteConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<SqliteConnection> GetWriteConnectionAsync(CancellationToken ct = default)
    {
        if (_writeConnection is not null)
            return _writeConnection;

        _writeConnection = new SqliteConnection(_configuration.ConnectionString);
        await _writeConnection.OpenAsync(ct);
        await ApplyPragmasAsync(_writeConnection, ct);
        return _writeConnection;
    }

    public async Task<SqliteConnection> GetReadConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqliteConnection(_configuration.ConnectionString);
        await connection.OpenAsync(ct);
        await ApplyPragmasAsync(connection, ct);
        return connection;
    }

    public async Task WaitForWriteGateAsync(CancellationToken ct = default)
    {
        await _writeGate.WaitAsync(ct);
    }

    public void ReleaseWriteGate()
    {
        _ = _writeGate.Release();
    }

    private async Task ApplyPragmasAsync(SqliteConnection connection, CancellationToken ct)
    {
        var command = connection.CreateCommand();

        if (_configuration.EnableWAL)
        {
            command.CommandText = "PRAGMA journal_mode=WAL;";
            await command.ExecuteNonQueryAsync(ct);
        }

        command.CommandText = "PRAGMA synchronous=NORMAL;";
        await command.ExecuteNonQueryAsync(ct);

        command.CommandText = $"PRAGMA busy_timeout={_configuration.BusyTimeoutMs};";
        await command.ExecuteNonQueryAsync(ct);

        if (_configuration.EnableForeignKeys)
        {
            command.CommandText = "PRAGMA foreign_keys=ON;";
            await command.ExecuteNonQueryAsync(ct);
        }

        command.CommandText = "PRAGMA temp_store=MEMORY;";
        await command.ExecuteNonQueryAsync(ct);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _writeConnection?.Close();
        _writeConnection?.Dispose();
        _writeGate.Dispose();
    }
}
