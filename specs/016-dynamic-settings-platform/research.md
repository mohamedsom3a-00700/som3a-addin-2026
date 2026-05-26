# Research: Dynamic Settings Platform

**Feature**: 016-dynamic-settings-platform
**Date**: 2026-05-26
**Status**: Complete

## R1: Dynamic UI Control Rendering

**Decision**: Use a WPF `DataTemplateSelector` keyed by `SettingValueType` enum to map each setting to its appropriate UI control. Control data templates are defined in a dedicated `SettingControlTemplates.xaml` resource dictionary.

**Rationale**: The `SettingValueType` enum already exists in `ISettingsRegistry` (defined in Phase 14 `ISettingsModule.md` contract) with values `String`, `Integer`, `Decimal`, `Boolean`, `Enum`, `Color`, `FilePath`, `Secret`. A `DataTemplateSelector` (`SettingControlSelector`) reads each setting's `ValueType`, matches it to a pre-defined `DataTemplate` key (e.g., `SettingTemplate_Boolean` → `CheckBox`, `SettingTemplate_Color` → color picker), and returns the template. This pattern is idiomatic WPF, fully MVVM-compatible, and requires no third-party controls (Constitution XIV).

**Alternatives considered**:
- **Code-behind switch statement building controls programmatically** — Works but breaks MVVM (business logic in code-behind), difficult to style consistently with theme tokens.
- **Reflection-based control factory** — Adds unnecessary complexity. The 7 enumerated types (plus `Secret` which renders as password-masked `String`) are known at design time.
- **Third-party property grid** (e.g., PropertyGrid/Fluent Ribbon) — Violates Constitution XIV (no third-party UI frameworks). Not needed for the defined scope.

**Implementation notes**:
- `SettingControlSelector` extends `DataTemplateSelector`, overrides `SelectTemplate` — reads `SettingValueType` from bound `SettingControlViewModel`
- DataTemplates in `SettingControlTemplates.xaml` reference `{DynamicResource}` tokens for theme consistency
- `Secret` type renders as `PasswordBox`-style masked input (FR-008)
- `Enum` type renders as `ComboBox` with items from `SettingDefinition.EnumValues`
- Control validation state is bound to `SettingControlViewModel.ValidationMessage` (shown inline below the control)

## R2: Settings Persistence Architecture

**Decision**: Per-plugin JSON settings files at `AppData/Som3a/Plugins/{ModuleId}/settings.json`. Encrypted secrets (field with `IsEncrypted = true`) stored separately at `AppData/Som3a/Plugins/{ModuleId}/secrets.json` encrypted via Windows DPAPI (`System.Security.Cryptography.ProtectedData`). The existing `SettingsPersistenceService` is extended to support both the legacy `Properties.Settings.Default` and the new per-plugin file structure.

**Rationale**: Per-plugin isolation provides clean boundaries: uninstalling a plugin removes its directory, one plugin's corrupt JSON doesn't affect others, and file-locking scope is minimal. DPAPI encryption is OS-native, user-scoped, and requires no external key management — appropriate for a single-user desktop application. The existing `SettingsPersistenceService` already handles JSON serialization for import/export; extending it minimizes new infrastructure.

**Alternatives considered**:
- **Single monolithic JSON** (`settings.json` for all plugins) — Creates contention and coupling. One plugin's corrupt data affects all. Export/import granularity is harder. Merge conflicts on concurrent saves.
- **Registry storage** (`HKCU\Software\Som3a`) — Not compatible with per-plugin settings lifecycle. Plugin uninstall would leave orphaned registry keys. User-scoped DPAPI encryption is more complex with registry.
- **SQLite database** — Overkill for key-value settings. Requires additional NuGet dependency. Adds query complexity for simple read/write operations.

**Implementation notes**:
- `SettingsPersistenceService.LoadPluginSettings<T>(string moduleId)` → deserializes JSON to `Dictionary<string, object?>`
- `SettingsPersistenceService.SavePluginSettings(string moduleId, Dictionary<string, object?> settings)`
- Encrypted fields: `ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser)` on write; `ProtectedData.Unprotect(...)` on read
- File format: JSON object with `version` (int), `sections` (array), `lastModified` (ISO 8601), `pluginVersion` (semver)
- Default values defined by `SettingDefinition.DefaultValue` — written to file on first load or when key missing (FR-010)

## R3: Settings Discovery & Registration

**Decision**: Plugin settings are auto-discovered via the existing `AssemblyScanner` from `Som3a.Plugin.SDK` (Phase 14). Each plugin assembly is scanned for types decorated with `[SettingsSection]` attribute that implement `ISettingsModule`. Discovered modules call `RegisterSettings(ISettingsRegistry)` during PluginHost initialization, which populates the registry.

**Rationale**: The Plugin SDK's `AssemblyScanner` and `PluginDiscoverer` already provide the infrastructure for assembly scanning, attribute discovery, and contract validation (from Phase 14's Plugin SDK design). Reusing this eliminates duplicate scanning logic and ensures consistent module lifecycle management. The `[SettingsSection]` attribute (defined in `Som3a.Plugin.SDK.Attributes`) maps to the `ISettingsModule` interface that Phase 14 already defined in `Som3a.Contracts`.

**Alternatives considered**:
- **WPF-only reflection in SettingsPage** — Duplicates scanning logic that already exists in Plugin SDK. Doesn't integrate with plugin lifecycle (unload/reload).
- **JSON manifest file per plugin** — Adds manual step for plugin developers. The `[SettingsSection]` attribute approach is compile-time safe and IDE-friendly.
- **Convention-based naming** (e.g., `*SettingsModule.cs`) — Fragile. Refactoring a class name would break discovery silently.

**Implementation notes**:
- `PluginHost.Initialize()` calls `module.RegisterSettings(SettingsRegistry.Instance)` for each plugin
- `SettingsRegistry` is a singleton registered in `CompositionRoot`
- Static (built-in) settings registered first, followed by plugin-discovered settings
- Duplicate section `Id` within same category: second registration is rejected with diagnostic warning
- Order is maintained by `Section.Order` property within each category

## R4: Hot-Reload via EventBus

**Decision**: Use the existing `EventBus` pattern with a typed `SettingsChangedEvent` class. When a setting value is saved, `SettingsRegistry` publishes `SettingsChangedEvent` with the changed setting's module ID, section ID, setting key, old value, and new value. Plugins subscribe to this event to hot-reload their behavior.

**Rationale**: The EventBus is already established in the codebase (`Services/EventBus.cs`), used by ModuleRegistry and other services. Adding a `SettingsChangedEvent` type is the minimal, consistent approach. Typed events with weak references prevent memory leaks from long-lived plugin subscribers. The existing subscriber isolation (one subscriber failure doesn't block others) is appropriate for plugin settings where one plugin's handler crash shouldn't prevent others from updating.

**Alternatives considered**:
- **Direct delegate/callback on ISettingsModule** — Tightly couples the registry to each module. More complex to implement for dynamic add/remove of plugins.
- **INotifyPropertyChanged on settings models** — Requires each setting to be a bindable property. Overkill for potentially hundreds of settings. Works against the generic key-value nature of the registry.
- **Polling** — Inefficient. 100ms polling for 50 plugins with 10 settings each creates 500 checks per cycle.

**Implementation notes**:
- `SettingsChangedEvent`: `ModuleId` (string), `SectionId` (string), `SettingKey` (string), `OldValue` (object?), `NewValue` (object?), `ChangedAt` (DateTime)
- Published via `EventBus.Instance.Publish(new SettingsChangedEvent { ... })` after persistence writes succeed
- Published even if encrypted — encrypted values are logged before/after as metadata only (not raw secrets)
- Plugins subscribe with `EventBus.Instance.Subscribe<SettingsChangedEvent>(OnSettingChanged)`
- Subscription tokens stored per plugin for cleanup on plugin unload

## R5: Existing Settings Migration Strategy

**Decision**: Incremental migration with fallback. Each existing settings category (Theme, Accessibility, Performance, Diagnostics, Excel) is migrated one at a time. During migration, the registry acts as primary storage while the legacy `Properties.Settings.Default` is kept as a read-through cache for backward compatibility. The legacy panels remain functional until all settings are migrated, at which point the old panels and models are removed.

**Rationale**: The existing `UserSettings` model (`SelectedTheme`, `AccentColor`, etc.) and panel views (`AppearancePanel.xaml`, etc.) are hardcoded references throughout the codebase. An all-at-once migration would require updating every reference simultaneously and risk regression. Incremental migration allows each category to be migrated independently, tested, and stabilized before the next, reducing risk. The fallback ensures that if a setting isn't found in the registry (pre-migration state), the system reads from the legacy store.

**Alternatives considered**:
- **Big-bang migration** — High risk: all settings pages break if migration has issues. Long QA cycle. Hard to bisect bugs.
- **Dual-write then cutover** — Write to both registry and legacy store during migration window. Simpler than read-through cache but increases write latency. Acceptable but adds complexity.
- **Flag-based routing** — Feature flag per category to switch between old and new. Adds flag management overhead. Useful for rollback but over-engineered for a settings migration.

**Implementation notes**:
- Migration progress tracked in `Som3aAddinSettings.json` at `AppData/Som3a/` with `migratedCategories` array
- `SettingsMigrationService` handles one category per method: `MigrateThemeSettings()`, `MigrateAccessibilitySettings()`, etc.
- Each migration method reads from `Properties.Settings.Default`, writes to registry JSON, then updates the migration tracker
- On startup, any unmigrated categories are migrated automatically in background (async, non-blocking)
- Legacy panels removed in a follow-up cleanup pass after all 5 categories are confirmed working from registry