# Contract: ISettingsModule

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the contract for the dynamic settings registry. Every plugin registers its settings through this contract, enabling the platform to dynamically build the settings UI without knowing plugin-specific details.

## Interface

```csharp
public interface ISettingsModule
{
    string ModuleId { get; }
    string ModuleName { get; }
    
    void RegisterSettings(ISettingsRegistry registry);
    Task<ValidationResult> ValidateAsync();
    Task ExportAsync(string filePath);
    Task ImportAsync(string filePath);
}
```

## Registry Interface

```csharp
public interface ISettingsRegistry
{
    void RegisterSection(SettingsSection section);
    void RegisterSetting(string sectionId, SettingDefinition setting);
}

public class SettingsSection
{
    public string Id { get; set; }
    public string Category { get; set; }          // "Appearance", "AI", "Performance", etc.
    public string DisplayName { get; set; }
    public int Order { get; set; }
    public string? IconKey { get; set; }
}

public class SettingDefinition
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public SettingValueType ValueType { get; set; }
    public object? DefaultValue { get; set; }
    public object? CurrentValue { get; set; }
    public List<ValidationRule>? ValidationRules { get; set; }
    public bool IsEncrypted { get; set; }         // If true, stored via DPAPI
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
    Secret            // API keys — always IsEncrypted = true
}

public class ValidationRule
{
    public string RuleId { get; set; }             // "Required", "Range", "Regex", "MinLength", "MaxLength"
    public string? Parameters { get; set; }         // JSON-encoded rule parameters
    public string ErrorMessage { get; set; }
}
```

## Persistence

- Settings stored as per-plugin JSON files at `AppData/Som3a/Plugins/{ModuleId}/settings.json`.
- Fields marked `IsEncrypted = true` stored via DPAPI-encrypted separate file `secrets.json`.
- `ExportAsync` produces a portable JSON bundle for sharing/backup.
- `ImportAsync` merges settings from a previously exported bundle.

## Dynamic UI

The platform settings page reads registered sections and dynamically builds:
- Category tabs (based on `Category` field)
- Section groups within each tab
- UI controls based on `ValueType` (textbox for String, checkbox for Boolean, color picker for Color, etc.)
- Inline validation using `ValidationRules`
