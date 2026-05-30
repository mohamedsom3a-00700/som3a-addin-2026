using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly ConnectionManager _connectionManager;

    public SettingsRepository(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<string?> GetSettingAsync(string category, string name, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM SettingsRecord WHERE Category = @category AND Name = @name AND PluginId = ''";
        command.Parameters.AddWithValue("@category", category);
        command.Parameters.AddWithValue("@name", name);
        return await command.ExecuteScalarAsync(ct) as string;
    }

    public async Task<IReadOnlyList<SettingsRecord>> GetSettingsByCategoryAsync(string category, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Category, Name, Value, PluginId, UpdatedAt, CreatedAt FROM SettingsRecord WHERE Category = @category";
        command.Parameters.AddWithValue("@category", category);
        return await ReadSettingsAsync(command, ct);
    }

    public async Task<IReadOnlyList<SettingsRecord>> GetPluginSettingsAsync(string pluginId, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Category, Name, Value, PluginId, UpdatedAt, CreatedAt FROM SettingsRecord WHERE PluginId = @pluginId";
        command.Parameters.AddWithValue("@pluginId", pluginId);
        return await ReadSettingsAsync(command, ct);
    }

    public async Task SetSettingAsync(string category, string name, string value, string? pluginId = null, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SettingsRecord (Id, Category, Name, Value, PluginId, UpdatedAt, CreatedAt)
                VALUES (@id, @category, @name, @value, @pluginId, @now, @now)
                ON CONFLICT(Category, Name, PluginId) DO UPDATE SET
                    Value = @value,
                    UpdatedAt = @now";
            command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@value", value);
            command.Parameters.AddWithValue("@pluginId", pluginId ?? "");
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task SetSettingsBulkAsync(IReadOnlyList<(string Category, string Name, string Value, string? PluginId)> settings, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var transaction = connection.BeginTransaction();

            foreach (var (category, name, value, pluginId) in settings)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO SettingsRecord (Id, Category, Name, Value, PluginId, UpdatedAt, CreatedAt)
                    VALUES (@id, @category, @name, @value, @pluginId, @now, @now)
                    ON CONFLICT(Category, Name, PluginId) DO UPDATE SET
                        Value = @value,
                        UpdatedAt = @now";
                command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@category", category);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@value", value);
                command.Parameters.AddWithValue("@pluginId", pluginId ?? "");
                command.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));
                await command.ExecuteNonQueryAsync(ct);
            }

            transaction.Commit();
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task DeleteSettingAsync(string category, string name, string? pluginId = null, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM SettingsRecord WHERE Category = @category AND Name = @name AND PluginId = @pluginId";
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@pluginId", pluginId ?? "");
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task DeletePluginSettingsAsync(string pluginId, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM SettingsRecord WHERE PluginId = @pluginId";
            command.Parameters.AddWithValue("@pluginId", pluginId);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<bool> HasCategoryAsync(string category, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM SettingsRecord WHERE Category = @category";
        command.Parameters.AddWithValue("@category", category);
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<IReadOnlyList<SettingsRecord>> ReadSettingsAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<SettingsRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new SettingsRecord
            {
                Id = Guid.Parse(reader.GetString(0)),
                Category = reader.GetString(1),
                Name = reader.GetString(2),
                Value = reader.GetString(3),
                PluginId = reader.GetString(4),
                UpdatedAt = DateTime.Parse(reader.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind),
                CreatedAt = DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }
        return results;
    }
}
