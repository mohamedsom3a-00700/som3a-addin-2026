using Som3a.Infrastructure.Persistence.Migrations;
using Xunit;

namespace Som3a.Infrastructure.Tests.Migrations;

public class MigrationEngineTests
{
    private static void ClearPoolAndDelete(string dbPath)
    {
        var connStr = $"Data Source={dbPath};";
        using var clearConn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
        Microsoft.Data.Sqlite.SqliteConnection.ClearPool(clearConn);

        if (File.Exists(dbPath))
            File.Delete(dbPath);
    }

    [Fact]
    public async Task ApplyMigrations_StartsAtVersionZero()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_mig_{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = $"Data Source={dbPath};";
            var engine = new MigrationEngine(connectionString);

            var version = await engine.GetCurrentVersionAsync();

            Assert.Equal(0, version);
        }
        finally
        {
            ClearPoolAndDelete(dbPath);
        }
    }

    [Fact]
    public async Task ApplyMigrations_IncrementsVersion()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_mig_{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = $"Data Source={dbPath};";
            var engine = new MigrationEngine(connectionString);

            await engine.ApplyMigrationsAsync();
            var version = await engine.GetCurrentVersionAsync();

            Assert.True(version >= 1, $"Expected version >= 1, got {version}");
        }
        finally
        {
            ClearPoolAndDelete(dbPath);
        }
    }

    [Fact]
    public async Task ApplyMigrations_CreatesExpectedTables()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_mig_{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = $"Data Source={dbPath};";
            var engine = new MigrationEngine(connectionString);

            await engine.ApplyMigrationsAsync();

            await using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await connection.OpenAsync();

            var tables = new[] { "SettingsRecord", "AIExecutionRecord", "PluginRecord", "DiagnosticsLog", "CrashReport", "TemplateRecord", "BackupManifest" };
            foreach (var table in tables)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='{table}'";
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                Assert.True(count > 0, $"Table '{table}' was not created.");
            }
        }
        finally
        {
            ClearPoolAndDelete(dbPath);
        }
    }
}
