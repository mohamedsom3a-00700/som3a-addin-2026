# Contract: IPluginRepository

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces`

## Purpose

Manages plugin metadata, version history, enable/disable state, and health status.

## Interface

```csharp
public interface IPluginRepository
{
    /// Registers or updates a plugin record (upsert by PluginId).
    Task RegisterPluginAsync(PluginRecord plugin, CancellationToken ct = default);

    /// Retrieves a plugin record by its unique PluginId.
    Task<PluginRecord?> GetPluginAsync(string pluginId, CancellationToken ct = default);

    /// Returns all registered plugins.
    Task<IReadOnlyList<PluginRecord>> GetAllPluginsAsync(CancellationToken ct = default);

    /// Returns only enabled plugins.
    Task<IReadOnlyList<PluginRecord>> GetEnabledPluginsAsync(CancellationToken ct = default);

    /// Updates the enable/disable state of a plugin.
    Task SetPluginEnabledAsync(string pluginId, bool isEnabled, CancellationToken ct = default);

    /// Updates the health status of a plugin.
    Task SetPluginHealthAsync(string pluginId, string healthStatus, string? message = null, CancellationToken ct = default);

    /// Records a plugin version installation in the version history.
    Task RecordVersionAsync(PluginVersionRecord versionRecord, CancellationToken ct = default);

    /// Gets version history for a plugin.
    Task<IReadOnlyList<PluginVersionRecord>> GetVersionHistoryAsync(string pluginId, CancellationToken ct = default);

    /// Removes a plugin and its version history (used during plugin uninstall).
    Task UnregisterPluginAsync(string pluginId, CancellationToken ct = default);
}
```
