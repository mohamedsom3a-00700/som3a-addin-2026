using Microsoft.Data.Sqlite;

namespace Som3a.Infrastructure.Persistence.Diagnostics;

public class DatabaseHealthCheck
{
    private readonly string _connectionString;
    private readonly string _databasePath;
    private readonly string? _backupDirectory;

    public DatabaseHealthCheck(string connectionString, string databasePath, string? backupDirectory = null)
    {
        _connectionString = connectionString;
        _databasePath = databasePath;
        _backupDirectory = backupDirectory;
    }

    public async Task<bool> RunIntegrityCheckAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_databasePath))
            return false;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        var result = await command.ExecuteScalarAsync(ct);
        return result?.ToString() == "ok";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct = default)
    {
        var result = new HealthCheckResult();

        result.FileExists = File.Exists(_databasePath);
        if (!result.FileExists)
        {
            result.Status = HealthStatus.Missing;
            result.Message = "Database file does not exist.";
            return result;
        }

        var fileInfo = new FileInfo(_databasePath);
        result.FileSizeBytes = fileInfo.Length;
        result.LastModified = fileInfo.LastWriteTimeUtc;

        result.IntegrityPassed = await RunIntegrityCheckAsync(ct);
        if (!result.IntegrityPassed)
        {
            result.Status = HealthStatus.Corrupted;
            result.Message = "Database integrity check failed.";
            return result;
        }

        result.Status = HealthStatus.Healthy;
        result.Message = "Database is healthy.";
        return result;
    }

    public async Task<bool> TryRepairFromBackupAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_backupDirectory) || !Directory.Exists(_backupDirectory))
            return false;

        var backupFiles = Directory.GetFiles(_backupDirectory, "platform_backup_*.db")
            .OrderByDescending(f => f)
            .ToList();

        foreach (var backup in backupFiles)
        {
            await using var connection = new SqliteConnection($"Data Source={backup};");
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = await command.ExecuteScalarAsync(ct);

            if (result?.ToString() == "ok")
            {
                var tempPath = _databasePath + ".restoring";
                File.Copy(backup, tempPath, overwrite: true);

                if (File.Exists(_databasePath))
                    File.Delete(_databasePath);

                File.Move(tempPath, _databasePath);
                return true;
            }
        }

        return false;
    }

    public enum HealthStatus
    {
        Healthy,
        Corrupted,
        Missing
    }

    public class HealthCheckResult
    {
        public HealthStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool FileExists { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime LastModified { get; set; }
        public bool IntegrityPassed { get; set; }
    }
}
