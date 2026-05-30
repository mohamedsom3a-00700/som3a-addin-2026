using Som3a.Infrastructure.Persistence.Models;

namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface ISettingsRepository
{
    Task<string?> GetSettingAsync(string category, string name, CancellationToken ct = default);

    Task<IReadOnlyList<SettingsRecord>> GetSettingsByCategoryAsync(string category, CancellationToken ct = default);

    Task<IReadOnlyList<SettingsRecord>> GetPluginSettingsAsync(string pluginId, CancellationToken ct = default);

    Task SetSettingAsync(string category, string name, string value, string? pluginId = null, CancellationToken ct = default);

    Task SetSettingsBulkAsync(IReadOnlyList<(string Category, string Name, string Value, string? PluginId)> settings, CancellationToken ct = default);

    Task DeleteSettingAsync(string category, string name, string? pluginId = null, CancellationToken ct = default);

    Task DeletePluginSettingsAsync(string pluginId, CancellationToken ct = default);

    Task<bool> HasCategoryAsync(string category, CancellationToken ct = default);
}
