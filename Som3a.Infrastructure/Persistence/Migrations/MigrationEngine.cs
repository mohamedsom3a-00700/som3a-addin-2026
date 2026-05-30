using System.Reflection;
using Microsoft.Data.Sqlite;

namespace Som3a.Infrastructure.Persistence.Migrations;

public class MigrationEngine
{
    private readonly string _connectionString;

    public MigrationEngine(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> GetCurrentVersionAsync(CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA user_version;";
        var result = await pragmaCommand.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task ApplyMigrationsAsync(CancellationToken ct = default)
    {
        var currentVersion = await GetCurrentVersionAsync(ct);
        var migrations = GetPendingMigrations(currentVersion);

        foreach (var migration in migrations)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var tx = (Microsoft.Data.Sqlite.SqliteTransaction)await connection.BeginTransactionAsync(ct);
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = migration.Sql;
                command.Transaction = tx;
                await command.ExecuteNonQueryAsync(ct);

                await using var versionCommand = connection.CreateCommand();
                versionCommand.CommandText = $"PRAGMA user_version = {migration.Version};";
                versionCommand.Transaction = tx;
                await versionCommand.ExecuteNonQueryAsync(ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }

    private List<MigrationScript> GetPendingMigrations(int currentVersion)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql") && n.Contains(".Migrations."))
            .OrderBy(n => n)
            .ToList();

        var migrations = new List<MigrationScript>();

        foreach (var resourceName in resourceNames)
        {
            var version = ParseVersion(resourceName);
            if (version > currentVersion)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null) continue;
                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
                migrations.Add(new MigrationScript { Version = version, Sql = sql });
            }
        }

        return migrations;
    }

    private static int ParseVersion(string resourceName)
    {
        var segments = resourceName.Split('.');
        if (segments.Length < 2) return 0;
        var fileName = segments[^2];
        var parts = fileName.Split('_');
        if (parts.Length > 0 && int.TryParse(parts[0], out var version))
            return version;
        return 0;
    }

    private record MigrationScript
    {
        public int Version { get; init; }
        public string Sql { get; init; } = string.Empty;
    }
}
