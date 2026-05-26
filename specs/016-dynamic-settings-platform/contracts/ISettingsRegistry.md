# Contract: ISettingsRegistry

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`
**Related**: `ISettingsModule` (Phase 14), `SettingDefinition`, `SettingsSection`, `ValidationRule`, `SettingValueType`

## Purpose

Defines the contract for the dynamic settings registry used by plugins to register their settings sections and individual settings. The registry exposes lookup methods for the settings page to dynamically build its UI and for persistence to save/load values.

## Interface

```csharp
using System.Collections.Generic;

namespace Som3a.Contracts
{
    public interface ISettingsRegistry
    {
        void RegisterSection(string pluginId, SettingsSection section);
        void RegisterSetting(string sectionId, SettingDefinition setting);
        void UpdateSettingValue(string sectionId, string key, object? value);
        IReadOnlyList<SettingsSection> GetSectionsByCategory(string category);
        IReadOnlyList<string> GetAllCategories();
        SettingDefinition? GetSetting(string sectionId, string key);
        object? GetSettingValue(string sectionId, string key);
        IReadOnlyList<SettingsSection> GetSectionsForPlugin(string pluginId);
        void PurgePluginSections(string pluginId);
        bool IsDirty { get; }
    }
}
```

## Supporting Types

```csharp
public class SettingsSection
{
    public string Id { get; set; } = string.Empty;
    public string PluginId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int Version { get; set; } = 1;
    public string? IconKey { get; set; }
    public List<SettingDefinition> Settings { get; set; } = new();
    public List<ValidationRule>? SectionValidationRules { get; set; }
}

public class SettingDefinition
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingValueType ValueType { get; set; }
    public object? DefaultValue { get; set; }
    public object? CurrentValue { get; set; }
    public List<string>? EnumValues { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public double? StepSize { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsReadOnly { get; set; }
    public List<ValidationRule>? ValidationRules { get; set; }
    public int UiOrder { get; set; }
}

public class ValidationRule
{
    public string RuleId { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
}

public enum ValidationSeverity
{
    Error,
    Warning
}

public enum SettingValueType
{
    String,
    Integer,
    Decimal,
    Boolean,
    Enum,
    Color,
    FilePath,
    Secret
}
```

## Event Types

```csharp
public class SettingsChangedEvent
{
    public string ModuleId { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

## Registration Flow

1. Plugin SDK discovers `[SettingsSection]`-decorated types implementing `ISettingsModule`
2. `PluginHost.Initialize()` calls `module.RegisterSettings(settingsRegistry)`
3. `RegisterSettings` creates `SettingsSection` instances and calls `registry.RegisterSection(pluginId, section)`
4. For each setting in the section, `registry.RegisterSetting(sectionId, setting)` is called
5. The settings page reads categories via `GetAllCategories()` and sections via `GetSectionsByCategory(category)`
6. On user save: `registry.UpdateSettingValue(sectionId, key, newValue)` → persistence writes JSON → EventBus emits `SettingsChangedEvent`

## Persistence Contract

```csharp
public interface ISettingsPersistence
{
    Task<Dictionary<string, object?>?> LoadPluginSettingsAsync(string pluginId);
    Task SavePluginSettingsAsync(string pluginId, Dictionary<string, object?> settings);
    Task<byte[]?> LoadEncryptedValueAsync(string pluginId, string key);
    Task SaveEncryptedValueAsync(string pluginId, string key, byte[] encryptedData);
    Task ExportSnapshotAsync(string exportPath, IReadOnlyList<string>? pluginIds = null);
    Task ImportSnapshotAsync(string importPath, IReadOnlyList<string>? pluginIds = null);
}
```
