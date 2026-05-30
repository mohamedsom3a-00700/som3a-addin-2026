using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly ConnectionManager _connectionManager;

    public TemplateRepository(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task AddTemplateAsync(TemplateRecord template, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TemplateRecord (Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt)
                VALUES (@id, @type, @name, @category, @content, @version, @isDefault, @modified, @created)";
            AddTemplateParameters(command, template);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task UpdateTemplateAsync(TemplateRecord template, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE TemplateRecord SET
                    Name = @name, Category = @category, Content = @content,
                    Version = Version + 1, IsDefault = @isDefault, LastModifiedAt = @modified
                WHERE Id = @id";
            command.Parameters.AddWithValue("@id", template.Id.ToString());
            command.Parameters.AddWithValue("@name", template.Name);
            command.Parameters.AddWithValue("@category", template.Category);
            command.Parameters.AddWithValue("@content", template.Content);
            command.Parameters.AddWithValue("@isDefault", template.IsDefault ? 1 : 0);
            command.Parameters.AddWithValue("@modified", DateTime.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<TemplateRecord?> GetTemplateAsync(Guid id, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt FROM TemplateRecord WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());
        return await ReadSingleTemplateAsync(command, ct);
    }

    public async Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAsync(string templateType, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt FROM TemplateRecord WHERE TemplateType = @type ORDER BY Name";
        command.Parameters.AddWithValue("@type", templateType);
        return await ReadTemplateListAsync(command, ct);
    }

    public async Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAndCategoryAsync(string templateType, string category, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt FROM TemplateRecord WHERE TemplateType = @type AND Category = @category ORDER BY Name";
        command.Parameters.AddWithValue("@type", templateType);
        command.Parameters.AddWithValue("@category", category);
        return await ReadTemplateListAsync(command, ct);
    }

    public async Task<IReadOnlyList<TemplateRecord>> GetDefaultTemplatesAsync(string templateType, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt FROM TemplateRecord WHERE TemplateType = @type AND IsDefault = 1 ORDER BY Name";
        command.Parameters.AddWithValue("@type", templateType);
        return await ReadTemplateListAsync(command, ct);
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TemplateRecord WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<IReadOnlyList<TemplateRecord>> SearchTemplatesAsync(string query, string? templateType = null, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        var sql = "SELECT Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt FROM TemplateRecord WHERE Name LIKE @query";
        var conditions = new List<string>();

        if (!string.IsNullOrEmpty(templateType))
        {
            conditions.Add("TemplateType = @type");
            command.Parameters.AddWithValue("@type", templateType);
        }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        sql += " ORDER BY Name";
        command.Parameters.AddWithValue("@query", $"%{query}%");
        command.CommandText = sql;

        return await ReadTemplateListAsync(command, ct);
    }

    private static void AddTemplateParameters(SqliteCommand command, TemplateRecord template)
    {
        command.Parameters.AddWithValue("@id", template.Id.ToString());
        command.Parameters.AddWithValue("@type", template.TemplateType);
        command.Parameters.AddWithValue("@name", template.Name);
        command.Parameters.AddWithValue("@category", template.Category);
        command.Parameters.AddWithValue("@content", template.Content);
        command.Parameters.AddWithValue("@version", template.Version);
        command.Parameters.AddWithValue("@isDefault", template.IsDefault ? 1 : 0);
        command.Parameters.AddWithValue("@modified", template.LastModifiedAt.ToString("O"));
        command.Parameters.AddWithValue("@created", template.CreatedAt.ToString("O"));
    }

    private static async Task<TemplateRecord?> ReadSingleTemplateAsync(SqliteCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return MapTemplate(reader);
        return null;
    }

    private static async Task<IReadOnlyList<TemplateRecord>> ReadTemplateListAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<TemplateRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(MapTemplate(reader));
        return results;
    }

    private static TemplateRecord MapTemplate(SqliteDataReader reader)
    {
        return new TemplateRecord
        {
            Id = Guid.Parse(reader.GetString(0)),
            TemplateType = reader.GetString(1),
            Name = reader.GetString(2),
            Category = reader.GetString(3),
            Content = reader.GetString(4),
            Version = reader.GetInt32(5),
            IsDefault = reader.GetInt32(6) == 1,
            LastModifiedAt = DateTime.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind),
            CreatedAt = DateTime.Parse(reader.GetString(8), null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
