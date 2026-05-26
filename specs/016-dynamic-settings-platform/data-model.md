# Data Model: Dynamic Settings Platform

**Feature**: 016-dynamic-settings-platform
**Date**: 2026-05-26

## Entity: SettingsRegistry *(new)*

Central registry that holds all registered settings sections across all plugins. Singleton service registered in CompositionRoot.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| PluginSections | `Dictionary<string, List<SettingsSection>>` | Yes | Plugin ID → list of registered sections |
| AllByCategory | `Dictionary<string, List<SettingsSection>>` | Yes | Category → list of sections (for UI grouping) |
| SectionIndex | `Dictionary<string, SettingsSection>` | Yes | Section ID → section (for fast lookup) |
| PluginStates | `Dictionary<string, PluginSettingsState>` | Yes | Plugin ID → registration state (Registered/Active/Orphaned) |
| DefaultValues | `Dictionary<string, object>` | Yes | Setting key → default value (for fallback on FR-010) |

**Methods**:
- `RegisterSection(string pluginId, SettingsSection section)` — Register a section, checks for duplicate IDs within plugin
- `RegisterSetting(string sectionId, SettingDefinition setting)` — Register a setting within a section
- `GetSections(string category)` → `IReadOnlyList<SettingsSection>` — Get sections for a category
- `GetAllCategories()` → `IReadOnlyList<string>` — Get all unique category names
- `GetSetting(string sectionId, string key)` → `SettingDefinition` — Look up a setting by section + key
- `UpdateSettingValue(string sectionId, string key, object value)` — Persist and emit SettingsChangedEvent
- `PurgePlugin(string pluginId)` — Remove all sections for a plugin (on uninstall)
- `GetOrphanedSections()` → `IReadOnlyList<SettingsSection>` — Sections whose plugin is no longer loaded

**Validation rules**:
- `pluginId` must be non-empty
- `section.Id` must be unique per plugin (duplicates rejected with warning)
- `category` must be non-empty (defaults to "Other" if empty)
- `Order` must be ≥ 0

**State transitions**:
- Plugin loaded → sections registered with state `Active`
- Plugin unloaded → sections state `Orphaned`; marked for auto-purge after grace period (default: 30 days)
- User triggers manual purge → `Orphaned` sections removed from registry and persistence

**Relationships**:
- One SettingsRegistry holds many SettingsSection (1:N)
- One SettingsSection holds many SettingDefinition (1:N)
- One SettingsRegistry maps to one SettingsPersistenceService (1:1)

## Entity: SettingsSection *(new)*

A named group of settings within a plugin, rendered as a collapsible group in the settings UI.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | string | Yes | Unique section ID within the plugin (e.g., "theme.appearance") |
| PluginId | string | Yes | Owning plugin's module ID |
| Category | string | Yes | Display category (e.g., "Appearance", "AI", "Performance") |
| DisplayName | string | Yes | Human-readable section name |
| Description | string | No | Help text shown below the section header |
| Order | int | Yes | Sort order within the category (lower = first) |
| Version | int | Yes | Schema version; migration handler runs if stored version differs |
| IconKey | string | No | Material Design icon name (e.g., "Palette", "Cog") |
| Settings | `List<SettingDefinition>` | Yes | The settings in this section |
| ValidationRules | `List<ValidationRule>` | No | Section-level validation rules |

**Validation rules**:
- `Id` must be unique per `(PluginId, Category)` pair
- `Settings` must contain at least one entry
- `Version` must be ≥ 1

## Entity: SettingDefinition *(new, extends Phase 14 contract)*

A single configurable value with metadata for rendering, validation, and persistence.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Key | string | Yes | Unique key within the section (e.g., "theme.mode") |
| DisplayName | string | Yes | Display label shown beside the control |
| Description | string | No | Help text/tooltip for the control |
| ValueType | SettingValueType | Yes | Enum: String, Integer, Decimal, Boolean, Enum, Color, FilePath, Secret |
| DefaultValue | object | No | Default value used when no persisted value exists (FR-010) |
| CurrentValue | object | No | Runtime value — loaded from persistence, updated on save |
| EnumValues | `List<string>` | No | Allowed values when `ValueType == Enum` |
| MinValue | double? | No | Minimum when `ValueType == Integer || Decimal` |
| MaxValue | double? | No | Maximum when `ValueType == Integer || Decimal` |
| StepSize | double? | No | Increment step for spinner/slider when `ValueType == Integer || Decimal` |
| IsEncrypted | bool | Yes | If true, value is stored via DPAPI-encrypted file (secrets.json) |
| IsReadOnly | bool | Yes | If true, control is displayed as read-only text |
| ValidationRules | `List<ValidationRule>` | No | Per-setting validation rules |
| UiOrder | int | Yes | Display order within the section |

**Validation rules**:
- `Key` must be unique within the section
- `ValueType == Secret` implies `IsEncrypted = true` (enforced)
- `ValueType == Secret` must not have `DefaultValue` stored in plaintext
- `EnumValues` must be non-empty when `ValueType == Enum`

## Enum: SettingValueType *(extends Phase 14 contract)*

| Value | Renderer | Description |
|-------|----------|-------------|
| String | TextBox | Free-form text input |
| Integer | NumericUpDown-style spinner | Whole number input |
| Decimal | NumericUpDown-style spinner | Decimal number input |
| Boolean | Toggle switch / CheckBox | True/false toggle |
| Enum | ComboBox / RadioButton group | Selection from predefined list |
| Color | Color picker | Color swatch with picker dialog |
| FilePath | File/folder path picker | Path input with Browse button |
| Secret | PasswordBox (masked) | API keys and other secrets |

## Entity: ValidationRule *(new)*

A validation constraint with pluggable rule types.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| RuleId | string | Yes | Rule type identifier (e.g., "Required", "Range", "Regex", "MinLength", "MaxLength") |
| Parameters | string | No | JSON-encoded rule parameters (e.g., `{"min": 1, "max": 100}`) |
| ErrorMessage | string | Yes | User-facing error message when validation fails |
| Severity | ValidationSeverity | Yes | Error (blocks save) or Warning (allows save with indicator) |

**Supported RuleId types**:
- `Required` — Value must not be null/empty/whitespace
- `Range` — Numeric value must be within `{min}` and `{max}` (for Integer/Decimal)
- `Regex` — String value must match `{pattern}` regex
- `MinLength` — String value must be at least `{min}` characters
- `MaxLength` — String value must be at most `{max}` characters
- `FilePathExists` — File path must exist on disk
- `Custom` — Plugin-provided validation via `ISettingsModule.ValidateAsync()`

## Enum: ValidationSeverity *(new)*

| Value | Behavior |
|-------|----------|
| Error | Blocks save; shows red indicator |
| Warning | Allows save; shows yellow indicator with message |

## Event: SettingsChangedEvent *(new)*

Event published via EventBus when a setting value changes.

| Field | Type | Description |
|-------|------|-------------|
| ModuleId | string | The plugin whose setting changed |
| SectionId | string | The section containing the changed setting |
| SettingKey | string | The key of the changed setting |
| OldValue | object | Value before the change |
| NewValue | object | Value after the change |
| ChangedAt | DateTime | UTC timestamp when the change was committed |

## Entity: SettingsPersistenceManifest *(new)*

Metadata stored alongside per-plugin settings files for version tracking and migration.

| Field | Type | Description |
|-------|------|-------------|
| Version | int | Persistence schema version (for future migration) |
| PluginVersion | string | Plugin version that wrote this file (semver) |
| LastModified | DateTime | Last write timestamp |
| SectionKeys | `List<string>` | List of section IDs included in this file |
| MigratedFrom | string | Pre-migration source ("Properties.Settings.Default" or null) |

## Pre-Migration Entities (kept during incremental cutover)

These existing entities are kept during the incremental migration but removed after all 5 categories are migrated.

### Entity: UserSettings *(pre-migration, kept for backward compatibility)*

| Field | Type | Current Persistence |
|-------|------|---------------------|
| SelectedTheme | string | `Properties.Settings.Default` |
| AccentColor | string | `Properties.Settings.Default` |
| AnimationSpeed | string | `Properties.Settings.Default` |
| UiDensity | string | `Properties.Settings.Default` |
| BackgroundStyle | string | `Properties.Settings.Default` |
| HighContrastEnabled | bool | `Properties.Settings.Default` |
| FocusIndicatorEnabled | bool | `Properties.Settings.Default` |
| RenderMode | string | `Properties.Settings.Default` |
| SafeModeEnabled | bool | `Properties.Settings.Default` |

### Entity: SettingsCategory *(pre-migration, kept for backward compatibility)*

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Category identifier |
| DisplayName | string | Human-readable name |
| Icon | string | Icon key |
| PanelType | Type | UserControl type for the panel view |
| Order | int | Display order |
