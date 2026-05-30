using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.Repositories;

public class PluginRepository : IPluginRepository
{
    private readonly ConnectionManager _connectionManager;

    public PluginRepository(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task RegisterPluginAsync(PluginRecord plugin, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO PluginRecord (Id, PluginId, Name, Version, Description, Author, Dependencies,
                    IsEnabled, HealthStatus, HealthMessage, Settings, InstalledAt, UpdatedAt)
                VALUES (@id, @pluginId, @name, @version, @desc, @author, @deps,
                    @enabled, @health, @healthMsg, @settings, @installed, @updated)
                ON CONFLICT(PluginId) DO UPDATE SET
                    Name = @name, Version = @version, Description = @desc, Author = @author,
                    Dependencies = @deps, Settings = @settings, UpdatedAt = @updated";
            command.Parameters.AddWithValue("@id", plugin.Id.ToString());
            command.Parameters.AddWithValue("@pluginId", plugin.PluginId);
            command.Parameters.AddWithValue("@name", plugin.Name);
            command.Parameters.AddWithValue("@version", plugin.Version);
            command.Parameters.AddWithValue("@desc", plugin.Description);
            command.Parameters.AddWithValue("@author", plugin.Author);
            command.Parameters.AddWithValue("@deps", plugin.Dependencies);
            command.Parameters.AddWithValue("@enabled", plugin.IsEnabled ? 1 : 0);
            command.Parameters.AddWithValue("@health", plugin.HealthStatus);
            command.Parameters.AddWithValue("@healthMsg", (object?)plugin.HealthMessage ?? DBNull.Value);
            command.Parameters.AddWithValue("@settings", plugin.Settings);
            command.Parameters.AddWithValue("@installed", plugin.InstalledAt.ToString("O"));
            command.Parameters.AddWithValue("@updated", plugin.UpdatedAt.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<PluginRecord?> GetPluginAsync(string pluginId, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PluginId, Name, Version, Description, Author, Dependencies, IsEnabled, HealthStatus, HealthMessage, Settings, InstalledAt, UpdatedAt FROM PluginRecord WHERE PluginId = @pluginId";
        command.Parameters.AddWithValue("@pluginId", pluginId);
        return await ReadSinglePluginAsync(command, ct);
    }

    public async Task<IReadOnlyList<PluginRecord>> GetAllPluginsAsync(CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PluginId, Name, Version, Description, Author, Dependencies, IsEnabled, HealthStatus, HealthMessage, Settings, InstalledAt, UpdatedAt FROM PluginRecord ORDER BY Name";
        return await ReadPluginListAsync(command, ct);
    }

    public async Task<IReadOnlyList<PluginRecord>> GetEnabledPluginsAsync(CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PluginId, Name, Version, Description, Author, Dependencies, IsEnabled, HealthStatus, HealthMessage, Settings, InstalledAt, UpdatedAt FROM PluginRecord WHERE IsEnabled = 1 ORDER BY Name";
        return await ReadPluginListAsync(command, ct);
    }

    public async Task SetPluginEnabledAsync(string pluginId, bool isEnabled, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE PluginRecord SET IsEnabled = @enabled, UpdatedAt = @now WHERE PluginId = @pluginId";
            command.Parameters.AddWithValue("@enabled", isEnabled ? 1 : 0);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@pluginId", pluginId);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task SetPluginHealthAsync(string pluginId, string healthStatus, string? message = null, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE PluginRecord SET HealthStatus = @health, HealthMessage = @msg, UpdatedAt = @now WHERE PluginId = @pluginId";
            command.Parameters.AddWithValue("@health", healthStatus);
            command.Parameters.AddWithValue("@msg", (object?)message ?? DBNull.Value);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@pluginId", pluginId);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task RecordVersionAsync(PluginVersionRecord versionRecord, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO PluginVersionRecord (Id, PluginRecordId, Version, InstalledAt, InstalledBy)
                VALUES (@id, @pluginRecordId, @version, @installed, @by)";
            command.Parameters.AddWithValue("@id", versionRecord.Id.ToString());
            command.Parameters.AddWithValue("@pluginRecordId", versionRecord.PluginRecordId.ToString());
            command.Parameters.AddWithValue("@version", versionRecord.Version);
            command.Parameters.AddWithValue("@installed", versionRecord.InstalledAt.ToString("O"));
            command.Parameters.AddWithValue("@by", versionRecord.InstalledBy);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<IReadOnlyList<PluginVersionRecord>> GetVersionHistoryAsync(string pluginId, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT pvr.Id, pvr.PluginRecordId, pvr.Version, pvr.InstalledAt, pvr.InstalledBy
            FROM PluginVersionRecord pvr
            INNER JOIN PluginRecord pr ON pr.Id = pvr.PluginRecordId
            WHERE pr.PluginId = @pluginId
            ORDER BY pvr.InstalledAt DESC";
        command.Parameters.AddWithValue("@pluginId", pluginId);

        var results = new List<PluginVersionRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new PluginVersionRecord
            {
                Id = Guid.Parse(reader.GetString(0)),
                PluginRecordId = Guid.Parse(reader.GetString(1)),
                Version = reader.GetString(2),
                InstalledAt = DateTime.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind),
                InstalledBy = reader.GetString(4)
            });
        }
        return results;
    }

    public async Task UnregisterPluginAsync(string pluginId, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM PluginRecord WHERE PluginId = @pluginId";
            command.Parameters.AddWithValue("@pluginId", pluginId);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    private static async Task<PluginRecord?> ReadSinglePluginAsync(SqliteCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return MapPlugin(reader);
        return null;
    }

    private static async Task<IReadOnlyList<PluginRecord>> ReadPluginListAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<PluginRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(MapPlugin(reader));
        return results;
    }

    private static PluginRecord MapPlugin(SqliteDataReader reader)
    {
        return new PluginRecord
        {
            Id = Guid.Parse(reader.GetString(0)),
            PluginId = reader.GetString(1),
            Name = reader.GetString(2),
            Version = reader.GetString(3),
            Description = reader.GetString(4),
            Author = reader.GetString(5),
            Dependencies = reader.GetString(6),
            IsEnabled = reader.GetInt32(7) == 1,
            HealthStatus = reader.GetString(8),
            HealthMessage = reader.IsDBNull(9) ? null : reader.GetString(9),
            Settings = reader.GetString(10),
            InstalledAt = DateTime.Parse(reader.GetString(11), null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(reader.GetString(12), null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
