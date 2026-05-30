using Microsoft.Data.Sqlite;

namespace Som3a.Infrastructure.Persistence.SQLite;

public class DatabaseContext
{
    private readonly ConnectionManager _connectionManager;

    public DatabaseContext(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<SqliteConnection> GetWriteConnectionAsync(CancellationToken ct = default)
    {
        return await _connectionManager.GetWriteConnectionAsync(ct);
    }

    public async Task<SqliteConnection> GetReadConnectionAsync(CancellationToken ct = default)
    {
        return await _connectionManager.GetReadConnectionAsync(ct);
    }

    public async Task ExecuteAsync(string sql, IEnumerable<SqliteParameter>? parameters = null, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            if (parameters is not null)
            {
                foreach (var p in parameters)
                    command.Parameters.Add(p);
            }
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Func<SqliteDataReader, T> map, IEnumerable<SqliteParameter>? parameters = null, CancellationToken ct = default) where T : class
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (parameters is not null)
        {
            foreach (var p in parameters)
                command.Parameters.Add(p);
        }
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return map(reader);
        return null;
    }

    public async Task<List<T>> QueryListAsync<T>(string sql, Func<SqliteDataReader, T> map, IEnumerable<SqliteParameter>? parameters = null, CancellationToken ct = default)
    {
        var results = new List<T>();
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (parameters is not null)
        {
            foreach (var p in parameters)
                command.Parameters.Add(p);
        }
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(map(reader));
        return results;
    }
}
