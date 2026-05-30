using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Tests;

public class TestHelpers : IDisposable
{
    private readonly string _sharedDbName;

    public TestHelpers()
    {
        _sharedDbName = $"test{Guid.NewGuid():N}";
    }

    public SQLiteConfiguration CreateSharedMemoryConfiguration()
    {
        return new SQLiteConfiguration
        {
            DataDirectory = ".",
            FileName = _sharedDbName,
            EnableWAL = false,
            EnableForeignKeys = true,
            BusyTimeoutMs = 1000
        };
    }

    public async Task<DatabaseFactory> CreateFactoryAsync()
    {
        var config = new SQLiteConfiguration
        {
            DataDirectory = Path.GetTempPath(),
            FileName = $"test_{Guid.NewGuid():N}.db",
            EnableWAL = false,
            EnableForeignKeys = true,
            BusyTimeoutMs = 5000
        };

        var factory = new DatabaseFactory(config);
        await factory.InitializeAsync();
        return factory;
    }

    public void Dispose()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), _sharedDbName);
        if (File.Exists(dbPath)) File.Delete(dbPath);
    }

    public static SQLiteConfiguration CreateFileConfiguration(string dbPath)
    {
        return new SQLiteConfiguration
        {
            DataDirectory = Path.GetDirectoryName(dbPath) ?? ".",
            FileName = Path.GetFileName(dbPath),
            EnableWAL = false,
            EnableForeignKeys = true,
            BusyTimeoutMs = 1000
        };
    }

    public static async Task<DatabaseFactory> CreateFileFactoryAsync(string dbPath)
    {
        var config = CreateFileConfiguration(dbPath);
        var factory = new DatabaseFactory(config);
        await factory.InitializeAsync();
        return factory;
    }
}
