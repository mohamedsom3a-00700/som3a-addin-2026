# Quickstart: Persistence Layer

## Prerequisites

- .NET 8.0 SDK
- Existing Som3a solution with `Som3a.Infrastructure` project
- NuGet: `Microsoft.Data.Sqlite` 8.0+

## Step 1: Initialize the Database

Call `DatabaseFactory` once at application startup. This creates the database file, applies all pending migrations, and runs seed data.

```csharp
var config = new SQLiteConfiguration
{
    DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Som3a"),
    FileName = "platform.db"
};

var factory = new DatabaseFactory(config);
await factory.InitializeAsync();

// Store singleton for app lifetime
App.DatabaseFactory = factory;
```

## Step 2: Create a Unit of Work

Use `IUnitOfWork` for any operation spanning multiple repositories.

```csharp
var uow = App.DatabaseFactory.CreateUnitOfWork();
await uow.BeginTransactionAsync(ct);

try
{
    await uow.Settings.SetSettingAsync("Theme", "Mode", "Dark", ct: ct);
    await uow.Settings.SetSettingAsync("Theme", "AccentColor", "#2D9CFF", ct: ct);
    await uow.CommitAsync(ct);
}
catch
{
    await uow.RollbackAsync(ct);
    throw;
}
```

## Step 3: Log an AI Execution

```csharp
await uow.AI.LogExecutionAsync(new AIExecutionRecord
{
    Id = Guid.NewGuid(),
    ProviderName = "Claude",
    ModelName = "claude-3-opus",
    PromptText = prompt,
    ResponseText = response,
    TokenInput = 1500,
    TokenOutput = 420,
    DurationMs = 3400,
    Status = "Success",
    ExecutedAt = DateTime.UtcNow
}, ct);
```

## Step 4: Query Plugin State at Startup

```csharp
var enabledPlugins = await uow.Plugins.GetEnabledPluginsAsync(ct);
foreach (var plugin in enabledPlugins)
{
    PluginHost.Load(plugin.PluginId, plugin.Version);
}
```

## Step 5: Create a Backup

```csharp
var backupService = new BackupService(App.DatabaseFactory);
var manifest = await backupService.CreateBackupAsync(ct);
// manifest.FilePath points to the backup file
```

## Step 6: Run Data Retention Cleanup (Startup)

```csharp
var retention = new DataRetentionService(App.DatabaseFactory);
await retention.CleanupIfNeededAsync(ct);
```

## Directory Structure

```
Som3a.Infrastructure/Persistence/
├── SQLite/
│   ├── DatabaseContext.cs        # Connection management, PRAGMA setup
│   ├── DatabaseFactory.cs        # Factory for UnitOfWork + initialization
│   ├── ConnectionManager.cs      # Reader/writer split connections
│   └── SQLiteConfiguration.cs    # Config DTO
├── Repositories/
│   ├── SettingsRepository.cs
│   ├── AIRepository.cs
│   ├── PluginRepository.cs
│   ├── DiagnosticsRepository.cs
│   └── TemplateRepository.cs
├── Interfaces/
│   ├── ISettingsRepository.cs
│   ├── IAIRepository.cs
│   ├── IPluginRepository.cs
│   ├── ILogRepository.cs
│   └── ITemplateRepository.cs
├── Migrations/
│   ├── 001_initial_schema.sql
│   └── ...
├── Seeders/
├── UnitOfWork/
│   └── UnitOfWork.cs
└── Backup/
    └── BackupService.cs
```

## Key Configuration

```csharp
// Applied on every connection open:
// PRAGMA journal_mode = WAL;
// PRAGMA synchronous = NORMAL;
// PRAGMA busy_timeout = 5000;
// PRAGMA foreign_keys = ON;
// PRAGMA temp_store = MEMORY;
```
