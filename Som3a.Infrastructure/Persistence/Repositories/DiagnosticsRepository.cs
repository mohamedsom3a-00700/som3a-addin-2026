using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Persistence.Repositories;

public class DiagnosticsRepository : ILogRepository
{
    private static readonly HashSet<string> ValidSeverities = new(StringComparer.OrdinalIgnoreCase)
    { "Debug", "Info", "Warning", "Error", "Fatal" };

    private readonly ConnectionManager _connectionManager;

    public DiagnosticsRepository(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task WriteLogAsync(DiagnosticsLog entry, CancellationToken ct = default)
    {
        ValidateSeverity(entry.Severity);
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO DiagnosticsLog (Id, Severity, Component, Message, StackTrace, PlatformState, LoggedAt)
                VALUES (@id, @severity, @component, @message, @stackTrace, @platformState, @loggedAt)";
            AddLogParameters(command, entry);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task WriteLogBatchAsync(IReadOnlyList<DiagnosticsLog> entries, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var transaction = connection.BeginTransaction();

            foreach (var entry in entries)
            {
                ValidateSeverity(entry.Severity);
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO DiagnosticsLog (Id, Severity, Component, Message, StackTrace, PlatformState, LoggedAt)
                    VALUES (@id, @severity, @component, @message, @stackTrace, @platformState, @loggedAt)";
                AddLogParameters(command, entry);
                await command.ExecuteNonQueryAsync(ct);
            }

            transaction.Commit();
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<IReadOnlyList<DiagnosticsLog>> QueryLogsAsync(
        string? minSeverity = null, DateTime? from = null, DateTime? to = null,
        string? component = null, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        var sql = "SELECT Id, Severity, Component, Message, StackTrace, PlatformState, LoggedAt FROM DiagnosticsLog WHERE 1=1";
        var conditions = new List<string>();

        if (!string.IsNullOrEmpty(minSeverity))
        {
            conditions.Add("Severity IN (@sev0, @sev1, @sev2)");
            var severityLevels = GetSeverityLevels(minSeverity);
            for (int i = 0; i < severityLevels.Count; i++)
                command.Parameters.AddWithValue($"@sev{i}", severityLevels[i]);
        }
        if (from.HasValue) { conditions.Add("LoggedAt >= @from"); command.Parameters.AddWithValue("@from", from.Value.ToString("O")); }
        if (to.HasValue) { conditions.Add("LoggedAt <= @to"); command.Parameters.AddWithValue("@to", to.Value.ToString("O")); }
        if (!string.IsNullOrEmpty(component)) { conditions.Add("Component = @component"); command.Parameters.AddWithValue("@component", component); }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        sql += " ORDER BY LoggedAt DESC LIMIT @take OFFSET @skip";
        command.Parameters.AddWithValue("@take", take);
        command.Parameters.AddWithValue("@skip", skip);
        command.CommandText = sql;

        return await ReadLogListAsync(command, ct);
    }

    public async Task RecordCrashAsync(CrashReport report, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO CrashReport (Id, LastOperation, MemoryUsageMb, ThreadState, ExcelInteropStatus, CrashDump, LoggedAt)
                VALUES (@id, @lastOp, @mem, @threadState, @excelStatus, @crashDump, @loggedAt)";
            command.Parameters.AddWithValue("@id", report.Id.ToString());
            command.Parameters.AddWithValue("@lastOp", report.LastOperation);
            command.Parameters.AddWithValue("@mem", report.MemoryUsageMb);
            command.Parameters.AddWithValue("@threadState", (object?)report.ThreadState ?? DBNull.Value);
            command.Parameters.AddWithValue("@excelStatus", (object?)report.ExcelInteropStatus ?? DBNull.Value);
            command.Parameters.AddWithValue("@crashDump", (object?)report.CrashDump ?? DBNull.Value);
            command.Parameters.AddWithValue("@loggedAt", report.LoggedAt.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<IReadOnlyList<CrashReport>> GetRecentCrashesAsync(int count = 10, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, LastOperation, MemoryUsageMb, ThreadState, ExcelInteropStatus, CrashDump, LoggedAt FROM CrashReport ORDER BY LoggedAt DESC LIMIT @count";
        command.Parameters.AddWithValue("@count", count);

        var results = new List<CrashReport>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new CrashReport
            {
                Id = Guid.Parse(reader.GetString(0)),
                LastOperation = reader.GetString(1),
                MemoryUsageMb = reader.GetInt32(2),
                ThreadState = reader.IsDBNull(3) ? null : reader.GetString(3),
                ExcelInteropStatus = reader.IsDBNull(4) ? null : reader.GetString(4),
                CrashDump = reader.IsDBNull(5) ? null : reader.GetString(5),
                LoggedAt = DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }
        return results;
    }

    public async Task RecordExportAsync(ExportHistoryRecord record, CancellationToken ct = default)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ExportHistoryRecord (Id, Format, RowCount, FileSize, DurationMs, Status, ErrorMessage, ExportedAt)
                VALUES (@id, @format, @rows, @fileSize, @duration, @status, @error, @exportedAt)";
            command.Parameters.AddWithValue("@id", record.Id.ToString());
            command.Parameters.AddWithValue("@format", record.Format);
            command.Parameters.AddWithValue("@rows", record.RowCount);
            command.Parameters.AddWithValue("@fileSize", record.FileSize);
            command.Parameters.AddWithValue("@duration", record.DurationMs);
            command.Parameters.AddWithValue("@status", record.Status);
            command.Parameters.AddWithValue("@error", (object?)record.ErrorMessage ?? DBNull.Value);
            command.Parameters.AddWithValue("@exportedAt", record.ExportedAt.ToString("O"));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }

    public async Task<IReadOnlyList<ExportHistoryRecord>> QueryExportHistoryAsync(
        string? format = null, DateTime? from = null, DateTime? to = null,
        int skip = 0, int take = 100, CancellationToken ct = default)
    {
        await using var connection = await _connectionManager.GetReadConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        var sql = "SELECT Id, Format, RowCount, FileSize, DurationMs, Status, ErrorMessage, ExportedAt FROM ExportHistoryRecord WHERE 1=1";
        var conditions = new List<string>();

        if (!string.IsNullOrEmpty(format)) { conditions.Add("Format = @format"); command.Parameters.AddWithValue("@format", format); }
        if (from.HasValue) { conditions.Add("ExportedAt >= @from"); command.Parameters.AddWithValue("@from", from.Value.ToString("O")); }
        if (to.HasValue) { conditions.Add("ExportedAt <= @to"); command.Parameters.AddWithValue("@to", to.Value.ToString("O")); }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        sql += " ORDER BY ExportedAt DESC LIMIT @take OFFSET @skip";
        command.Parameters.AddWithValue("@take", take);
        command.Parameters.AddWithValue("@skip", skip);
        command.CommandText = sql;

        var results = new List<ExportHistoryRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new ExportHistoryRecord
            {
                Id = Guid.Parse(reader.GetString(0)),
                Format = reader.GetString(1),
                RowCount = reader.GetInt32(2),
                FileSize = reader.GetInt32(3),
                DurationMs = reader.GetInt32(4),
                Status = reader.GetString(5),
                ErrorMessage = reader.IsDBNull(6) ? null : reader.GetString(6),
                ExportedAt = DateTime.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }
        return results;
    }

    public async Task<int> CleanupLogsAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        return await DeleteOlderThanAsync("DiagnosticsLog", "LoggedAt", cutoffDate, ct);
    }

    public async Task<int> CleanupCrashesAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        return await DeleteOlderThanAsync("CrashReport", "LoggedAt", cutoffDate, ct);
    }

    public async Task<int> CleanupExportHistoryAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        return await DeleteOlderThanAsync("ExportHistoryRecord", "ExportedAt", cutoffDate, ct);
    }

    private static void ValidateSeverity(string severity)
    {
        if (!ValidSeverities.Contains(severity))
            throw new ArgumentException($"Invalid severity '{severity}'. Must be one of: {string.Join(", ", ValidSeverities)}");
    }

    private static List<string> GetSeverityLevels(string minSeverity)
    {
        var allLevels = new[] { "Debug", "Info", "Warning", "Error", "Fatal" };
        var startIndex = Array.IndexOf(allLevels, minSeverity);
        if (startIndex < 0) startIndex = 0;
        return allLevels.Skip(startIndex).ToList();
    }

    private static void AddLogParameters(SqliteCommand command, DiagnosticsLog entry)
    {
        command.Parameters.AddWithValue("@id", entry.Id.ToString());
        command.Parameters.AddWithValue("@severity", entry.Severity);
        command.Parameters.AddWithValue("@component", entry.Component);
        command.Parameters.AddWithValue("@message", entry.Message);
        command.Parameters.AddWithValue("@stackTrace", (object?)entry.StackTrace ?? DBNull.Value);
        command.Parameters.AddWithValue("@platformState", (object?)entry.PlatformState ?? DBNull.Value);
        command.Parameters.AddWithValue("@loggedAt", entry.LoggedAt.ToString("O"));
    }

    private static async Task<IReadOnlyList<DiagnosticsLog>> ReadLogListAsync(SqliteCommand command, CancellationToken ct)
    {
        var results = new List<DiagnosticsLog>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new DiagnosticsLog
            {
                Id = Guid.Parse(reader.GetString(0)),
                Severity = reader.GetString(1),
                Component = reader.GetString(2),
                Message = reader.GetString(3),
                StackTrace = reader.IsDBNull(4) ? null : reader.GetString(4),
                PlatformState = reader.IsDBNull(5) ? null : reader.GetString(5),
                LoggedAt = DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }
        return results;
    }

    private async Task<int> DeleteOlderThanAsync(string table, string dateColumn, DateTime cutoffDate, CancellationToken ct)
    {
        await _connectionManager.WaitForWriteGateAsync(ct);
        try
        {
            var connection = await _connectionManager.GetWriteConnectionAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM {table} WHERE {dateColumn} < @cutoff";
            command.Parameters.AddWithValue("@cutoff", cutoffDate.ToString("O"));
            return await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            _connectionManager.ReleaseWriteGate();
        }
    }
}
