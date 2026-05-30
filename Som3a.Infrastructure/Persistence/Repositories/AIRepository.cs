using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.Repositories;

public class AIRepository : IAIRepository
{
    private readonly ConnectionManager _connectionManager;

    public AIRepository(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task LogExecutionAsync(AIExecutionRecord record, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO AIExecutionRecord (Id, ProviderName, ModelName, PromptText, ResponseText,
                    TokenInput, TokenOutput, DurationMs, Status, ErrorMessage, RetryCount, PluginId, ExecutedAt)
                VALUES (@id, @provider, @model, @prompt, @response,
                    @tokenIn, @tokenOut, @duration, @status, @error, @retry, @pluginId, @executedAt)";
            AddExecutionParameters(command, record);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<AIExecutionRecord?> GetExecutionAsync(Guid id, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProviderName, ModelName, PromptText, ResponseText, TokenInput, TokenOutput, DurationMs, Status, ErrorMessage, RetryCount, PluginId, ExecutedAt FROM AIExecutionRecord WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());
        return await ReadSingleExecutionAsync(command, ct);
    }

    public async Task<IReadOnlyList<AIExecutionRecord>> QueryExecutionsAsync(
        DateTime? from = null, DateTime? to = null,
        string? providerName = null, string? status = null,
        int skip = 0, int take = 100, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        var sql = "SELECT Id, ProviderName, ModelName, PromptText, ResponseText, TokenInput, TokenOutput, DurationMs, Status, ErrorMessage, RetryCount, PluginId, ExecutedAt FROM AIExecutionRecord WHERE 1=1";
        var conditions = new List<string>();

        if (from.HasValue) { conditions.Add("ExecutedAt >= @from"); command.Parameters.AddWithValue("@from", from.Value.ToString("O")); }
        if (to.HasValue) { conditions.Add("ExecutedAt <= @to"); command.Parameters.AddWithValue("@to", to.Value.ToString("O")); }
        if (!string.IsNullOrEmpty(providerName)) { conditions.Add("ProviderName = @provider"); command.Parameters.AddWithValue("@provider", providerName); }
        if (!string.IsNullOrEmpty(status)) { conditions.Add("Status = @status"); command.Parameters.AddWithValue("@status", status); }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        sql += " ORDER BY ExecutedAt DESC LIMIT @take OFFSET @skip";
        command.Parameters.AddWithValue("@take", take);
        command.Parameters.AddWithValue("@skip", skip);
        command.CommandText = sql;

        return await ReadExecutionListAsync(command, ct);
    }

    public async Task<int> CountExecutionsAsync(
        DateTime? from = null, DateTime? to = null,
        string? providerName = null, string? status = null,
        CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        var sql = "SELECT COUNT(1) FROM AIExecutionRecord WHERE 1=1";
        var conditions = new List<string>();

        if (from.HasValue) { conditions.Add("ExecutedAt >= @from"); command.Parameters.AddWithValue("@from", from.Value.ToString("O")); }
        if (to.HasValue) { conditions.Add("ExecutedAt <= @to"); command.Parameters.AddWithValue("@to", to.Value.ToString("O")); }
        if (!string.IsNullOrEmpty(providerName)) { conditions.Add("ProviderName = @provider"); command.Parameters.AddWithValue("@provider", providerName); }
        if (!string.IsNullOrEmpty(status)) { conditions.Add("Status = @status"); command.Parameters.AddWithValue("@status", status); }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task UpsertRuntimeRecordAsync(AIRuntimeRecord record, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO AIRuntimeRecord (Id, ProviderName, SessionId, TokenInputTotal, TokenOutputTotal,
                    OperationCount, EstimatedCost, StartedAt, UpdatedAt)
                VALUES (@id, @provider, @session, @tokenIn, @tokenOut, @ops, @cost, @started, @updated)
                ON CONFLICT(Id) DO UPDATE SET
                    TokenInputTotal = @tokenIn,
                    TokenOutputTotal = @tokenOut,
                    OperationCount = @ops,
                    EstimatedCost = @cost,
                    UpdatedAt = @updated";
            command.Parameters.AddWithValue("@id", record.Id.ToString());
            command.Parameters.AddWithValue("@provider", record.ProviderName);
            command.Parameters.AddWithValue("@session", record.SessionId);
            command.Parameters.AddWithValue("@tokenIn", record.TokenInputTotal);
            command.Parameters.AddWithValue("@tokenOut", record.TokenOutputTotal);
            command.Parameters.AddWithValue("@ops", record.OperationCount);
            command.Parameters.AddWithValue("@cost", record.EstimatedCost);
            command.Parameters.AddWithValue("@started", record.StartedAt.ToString("O"));
            command.Parameters.AddWithValue("@updated", record.UpdatedAt.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<AIRuntimeRecord?> GetRuntimeRecordAsync(string sessionId, string providerName, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProviderName, SessionId, TokenInputTotal, TokenOutputTotal, OperationCount, EstimatedCost, StartedAt, UpdatedAt FROM AIRuntimeRecord WHERE SessionId = @session AND ProviderName = @provider";
        command.Parameters.AddWithValue("@session", sessionId);
        command.Parameters.AddWithValue("@provider", providerName);
        return await ReadSingleRuntimeAsync(command, ct);
    }

    public async Task<IReadOnlyList<AIRuntimeRecord>> GetSessionRuntimeRecordsAsync(string sessionId, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProviderName, SessionId, TokenInputTotal, TokenOutputTotal, OperationCount, EstimatedCost, StartedAt, UpdatedAt FROM AIRuntimeRecord WHERE SessionId = @session";
        command.Parameters.AddWithValue("@session", sessionId);
        return await ReadRuntimeListAsync(command, ct);
    }

    public async Task<int> CleanupAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM AIExecutionRecord WHERE ExecutedAt < @cutoff";
            command.Parameters.AddWithValue("@cutoff", cutoffDate.ToString("O"));
            return await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    private static void AddExecutionParameters(SqliteCommand command, AIExecutionRecord record)
    {
        command.Parameters.AddWithValue("@id", record.Id.ToString());
        command.Parameters.AddWithValue("@provider", record.ProviderName);
        command.Parameters.AddWithValue("@model", record.ModelName);
        command.Parameters.AddWithValue("@prompt", record.PromptText);
        command.Parameters.AddWithValue("@response", record.ResponseText);
        command.Parameters.AddWithValue("@tokenIn", record.TokenInput);
        command.Parameters.AddWithValue("@tokenOut", record.TokenOutput);
        command.Parameters.AddWithValue("@duration", record.DurationMs);
        command.Parameters.AddWithValue("@status", record.Status);
        command.Parameters.AddWithValue("@error", (object?)record.ErrorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("@retry", record.RetryCount);
        command.Parameters.AddWithValue("@pluginId", record.PluginId ?? "");
        command.Parameters.AddWithValue("@executedAt", record.ExecutedAt.ToString("O"));
    }

    private static async Task<AIExecutionRecord?> ReadSingleExecutionAsync(SqliteCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return MapExecution(reader);
        return null;
    }

    private static async Task<IReadOnlyList<AIExecutionRecord>> ReadExecutionListAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<AIExecutionRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(MapExecution(reader));
        return results;
    }

    private static AIExecutionRecord MapExecution(SqliteDataReader reader)
    {
        return new AIExecutionRecord
        {
            Id = Guid.Parse(reader.GetString(0)),
            ProviderName = reader.GetString(1),
            ModelName = reader.GetString(2),
            PromptText = reader.GetString(3),
            ResponseText = reader.GetString(4),
            TokenInput = reader.GetInt32(5),
            TokenOutput = reader.GetInt32(6),
            DurationMs = reader.GetInt32(7),
            Status = reader.GetString(8),
            ErrorMessage = reader.IsDBNull(9) ? null : reader.GetString(9),
            RetryCount = reader.GetInt32(10),
            PluginId = reader.IsDBNull(11) ? null : reader.GetString(11),
            ExecutedAt = DateTime.Parse(reader.GetString(12), null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }

    private static async Task<AIRuntimeRecord?> ReadSingleRuntimeAsync(SqliteCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return MapRuntime(reader);
        return null;
    }

    private static async Task<IReadOnlyList<AIRuntimeRecord>> ReadRuntimeListAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<AIRuntimeRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(MapRuntime(reader));
        return results;
    }

    private static AIRuntimeRecord MapRuntime(SqliteDataReader reader)
    {
        return new AIRuntimeRecord
        {
            Id = Guid.Parse(reader.GetString(0)),
            ProviderName = reader.GetString(1),
            SessionId = reader.GetString(2),
            TokenInputTotal = reader.GetInt32(3),
            TokenOutputTotal = reader.GetInt32(4),
            OperationCount = reader.GetInt32(5),
            EstimatedCost = reader.GetDouble(6),
            StartedAt = DateTime.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(reader.GetString(8), null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
