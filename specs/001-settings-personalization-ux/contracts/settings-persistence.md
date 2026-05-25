# Settings Persistence Contract

## Service Interface

The `SettingsPersistenceService` exposes the following contract:

### Read

Load settings from persistent store (Properties.Settings.Default).

```csharp
UserSettings LoadSettings();
```

**Returns**: Current UserSettings with stored values, or defaults if no saved state exists.

### Save

Persist current settings to Properties.Settings.Default.

```csharp
void SaveSettings(UserSettings settings);
```

**Errors**: On failure, throws `SettingsPersistenceException`. Caller (ViewModel) handles via toast notification.

### Export

Serialize settings to a portable JSON file.

```csharp
void ExportSettings(UserSettings settings, string filePath);
```

**Output**: JSON file with schema defined below. File under 100KB.

### Import

Deserialize settings from a JSON file and return parsed values.

```csharp
ImportResult ImportSettings(string filePath);
```

**Returns**: `ImportResult` with `UserSettings Settings` and `List<string> Warnings` (e.g., unknown keys ignored).

**Errors**: 
- File not found → `FileNotFoundException`
- Invalid JSON → `SettingsImportException("Invalid file format")`
- Missing required fields → `SettingsImportException("Missing required field: ...")`
- On any error → no settings are modified

## JSON Import/Export Schema

```json
{
  "version": "1.0.0",
  "exportedAt": "2026-05-25T10:00:00Z",
  "appVersion": "2026.1.0.0",
  "settings": {
    "selectedTheme": "Dark",
    "accentColor": "#3A86FF",
    "animationSpeed": "Full",
    "uiDensity": "Normal",
    "backgroundStyle": "Gradient",
    "highContrastEnabled": false,
    "focusIndicatorEnabled": true,
    "renderMode": "Auto",
    "safeModeEnabled": false
  }
}
```

### Versioning Rules

- `version` field follows semver. On import: exact match in major.minor is accepted; unknown fields silently ignored.
- Unknown top-level keys in settings object are ignored (forward compatibility).
- Missing optional fields default to current value (no change).
- Missing required fields (version, settings) trigger import failure.

## ViewModel Public Contract

The `SettingsViewModel` exposes:

```csharp
// Navigation
ObservableCollection<SettingsCategory> Categories { get; }
SettingsCategory SelectedCategory { get; set; }

// Commands
ICommand ApplyThemeCommand { get; }       // Commits live preview to main app
ICommand CancelPreviewCommand { get; }     // Reverts live preview
ICommand ExportSettingsCommand { get; }    // Opens save dialog, exports
ICommand ImportSettingsCommand { get; }    // Opens open dialog, imports

// State
UserSettings CurrentSettings { get; }
UserSettings PreviewSettings { get; }      // Live preview working copy
bool IsDirty { get; }                      // Unsaved changes exist
bool IsPreviewActive { get; }              // Preview is showing
```
