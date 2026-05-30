using Som3a.Infrastructure.Persistence.Models;

namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface IPluginRepository
{
    Task RegisterPluginAsync(PluginRecord plugin, CancellationToken ct = default);

    Task<PluginRecord?> GetPluginAsync(string pluginId, CancellationToken ct = default);

    Task<IReadOnlyList<PluginRecord>> GetAllPluginsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PluginRecord>> GetEnabledPluginsAsync(CancellationToken ct = default);

    Task SetPluginEnabledAsync(string pluginId, bool isEnabled, CancellationToken ct = default);

    Task SetPluginHealthAsync(string pluginId, string healthStatus, string? message = null, CancellationToken ct = default);

    Task RecordVersionAsync(PluginVersionRecord versionRecord, CancellationToken ct = default);

    Task<IReadOnlyList<PluginVersionRecord>> GetVersionHistoryAsync(string pluginId, CancellationToken ct = default);

    Task UnregisterPluginAsync(string pluginId, CancellationToken ct = default);
}
