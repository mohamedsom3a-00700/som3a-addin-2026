# Quickstart: Dynamic Settings Platform

**Feature**: 016-dynamic-settings-platform
**Date**: 2026-05-26

## Overview

This guide helps a developer understand the dynamic settings system and how to add settings to a new plugin.

## Key Files

| File | Purpose |
|------|---------|
| `WpfApp2/Services/SettingsRegistry.cs` | Central registry for all plugin settings |
| `WpfApp2/Services/SettingsPersistenceService.cs` | Per-plugin JSON persistence + DPAPI encryption |
| `WpfApp2/Services/SettingsChangedEvent.cs` | EventBus event type for hot-reload |
| `WpfApp2/Services/DynamicSettingsRenderer.cs` | DataTemplateSelector mapping ValueType → control |
| `WpfApp2/Services/SettingsValidator.cs` | Validation rule engine (Required, Range, Regex, etc.) |
| `WpfApp2/Services/SettingsMigrationService.cs` | Incremental migration from legacy Properties.Settings.Default |
| `WpfApp2/ViewModels/SettingsSectionViewModel.cs` | ViewModel wrapping a registered section |
| `WpfApp2/ViewModels/SettingControlViewModel.cs` | ViewModel wrapping a single setting with validation state |
| `WpfApp2/Views/SettingControlTemplates.xaml` | DataTemplates for each SettingValueType |
| `WpfApp2/Views/SettingControlSelector.cs` | DataTemplateSelector by ValueType |
| `WpfApp2/Views/SettingsSectionView.xaml` | Renders one section with header + controls |
| `WpfApp2/Pages/SettingsPage.xaml` | Modified to read sections from registry |
| `Som3a.Contracts/ISettingsRegistry.cs` | Interface contract (Phase 14) |
| `Som3a.Plugin.SDK/Attributes/SettingsSectionAttribute.cs` | Attribute for plugin settings discovery (Phase 14) |

## Adding Plugin Settings

### 1. Define a settings module implementing ISettingsModule

```csharp
using Som3a.Contracts;

[SettingsSection(Category = "AI", DisplayName = "Schedule Reviewer", Order = 10)]
public class ScheduleReviewerSettings : ISettingsModule
{
    public string ModuleId => "schedulereviewer";
    public string ModuleName => "Schedule Reviewer Plugin";

    public void RegisterSettings(ISettingsRegistry registry)
    {
        var section = new SettingsSection
        {
            Id = "schedulereviewer.general",
            PluginId = ModuleId,
            Category = "AI",
            DisplayName = "General Settings",
            Description = "Configure the Schedule Reviewer AI behavior",
            Order = 1,
            Version = 1,
            IconKey = "Robot"
        };

        section.Settings.Add(new SettingDefinition
        {
            Key = "ai.provider",
            DisplayName = "AI Provider",
            Description = "Which AI provider to use for schedule analysis",
            ValueType = SettingValueType.Enum,
            EnumValues = new List<string> { "Claude", "GPT-4", "DeepSeek" },
            DefaultValue = "Claude",
            UiOrder = 1
        });

        section.Settings.Add(new SettingDefinition
        {
            Key = "api.key",
            DisplayName = "API Key",
            Description = "Your AI provider API key",
            ValueType = SettingValueType.Secret,
            IsEncrypted = true,
            UiOrder = 2,
            ValidationRules = new List<ValidationRule>
            {
                new() { RuleId = "Required", ErrorMessage = "API key is required" },
                new() { RuleId = "MinLength", Parameters = "{\"min\": 8}", ErrorMessage = "Key must be at least 8 characters" }
            }
        });

        registry.RegisterSection(ModuleId, section);
    }

    public Task<ValidationResult> ValidateAsync()
    {
        // Plugin-level validation across all sections
        return Task.FromResult(new ValidationResult { IsValid = true });
    }

    public Task ExportAsync(string filePath) => Task.CompletedTask;
    public Task ImportAsync(string filePath) => Task.CompletedTask;
}
```

### 2. The setting appears automatically

The settings page reads from the registry — no manual UI wiring needed.

### 3. Subscribe to hot-reload events (optional)

```csharp
public class ScheduleReviewerPlugin : IPlugin
{
    private SubscriptionToken? _settingsToken;

    public void Initialize(IServiceContainer container, IEventBus eventBus)
    {
        _settingsToken = eventBus.Subscribe<SettingsChangedEvent>(OnSettingChanged);
    }

    private void OnSettingChanged(SettingsChangedEvent evt)
    {
        if (evt.ModuleId != "schedulereviewer") return;
        // React to the change — e.g., reinitialize AI client
    }
}
```

## Reading Settings Programmatically

```csharp
// From any service with access to ISettingsRegistry:
var setting = registry.GetSetting("schedulereviewer.general", "ai.provider");
var currentValue = setting?.CurrentValue ?? setting?.DefaultValue;
```

## Migrating Existing Settings

Existing settings (Theme, Accessibility, Performance, Diagnostics, Excel) are migrated incrementally:

```csharp
// In CompositionRoot startup:
var migrationService = container.Resolve<SettingsMigrationService>();
await migrationService.MigrateCategoryAsync("Theme");       // Migrates SelectedTheme, AccentColor, etc.
await migrationService.MigrateCategoryAsync("Accessibility"); // HighContrastEnabled, FocusIndicatorEnabled, etc.
// ... one category at a time
```

## Build & Test

```powershell
# Build WPF host (all new services are in WpfApp2)
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Run settings registry unit tests (if added to test project)
dotnet test Som3a.Domain.Tests --filter "Category=SettingsRegistry"
```
