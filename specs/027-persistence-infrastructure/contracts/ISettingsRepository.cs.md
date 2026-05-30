# Contract: ISettingsRepository

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces` (or `Planova.Infrastructure.Persistence.Interfaces`)

## Purpose

Provides CRUD operations for user and plugin settings stored in the database.

## Interface

```csharp
public interface ISettingsRepository
{
    /// Retrieves a single setting value by category and name.
    /// Returns null if the setting does not exist.
    Task<string?> GetSettingAsync(string category, string name, CancellationToken ct = default);

    /// Retrieves all settings within a category (e.g. all "Theme" settings).
    Task<IReadOnlyList<SettingsRecord>> GetSettingsByCategoryAsync(string category, CancellationToken ct = default);

    /// Retrieves all settings for a specific plugin.
    Task<IReadOnlyList<SettingsRecord>> GetPluginSettingsAsync(string pluginId, CancellationToken ct = default);

    /// Creates or updates a setting. If a setting with the same (Category, Name, PluginId)
    /// exists, updates the value; otherwise inserts a new record.
    Task SetSettingAsync(string category, string name, string value, string? pluginId = null, CancellationToken ct = default);

    /// Creates or updates multiple settings in a single transaction.
    Task SetSettingsBulkAsync(IReadOnlyList<(string Category, string Name, string Value, string? PluginId)> settings, CancellationToken ct = default);

    /// Deletes a specific setting.
    Task DeleteSettingAsync(string category, string name, string? pluginId = null, CancellationToken ct = default);

    /// Deletes all settings for a given plugin (used during plugin uninstall).
    Task DeletePluginSettingsAsync(string pluginId, CancellationToken ct = default);

    /// Returns true if any setting exists for the given category.
    Task<bool> HasCategoryAsync(string category, CancellationToken ct = default);
}
```

## Notes

- Settings use upsert semantics (INSERT OR REPLACE) for atomic create-or-update.
- The `PluginId` parameter distinguishes platform settings (null) from plugin settings.
