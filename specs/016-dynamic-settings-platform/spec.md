# Feature Specification: Dynamic Settings Platform

**Feature Branch**: `feature/phase-16-dynamic-settings-platform`

**Created**: 2026-05-26

**Status**: Draft

**Input**: User description: "Replace static settings pages with a dynamic registry-based settings system where every plugin registers its own settings sections, validation rules, UI controls, and persistence handlers."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plugin Developer Registers Settings (Priority: P1)

A plugin developer creates a new plugin and registers its settings sections, validation rules, and UI controls through the settings registry. The settings automatically appear in the Settings page without any manual wiring.

**Why this priority**: This is the foundational capability — all other features depend on plugins being able to register settings. Without this, nothing else works.

**Independent Test**: Can be fully tested by creating a test plugin that registers one settings section with one text control and a validation rule, then verifying the section appears in the Settings page.

**Acceptance Scenarios**:
1. **Given** a plugin implementing ISettingsModule, **When** the plugin is loaded, **Then** its settings sections are registered in the SettingsRegistry
2. **Given** a plugin with a `[SettingsSection]` attribute, **When** the plugin assembly is discovered, **Then** the settings section is auto-registered without manual configuration
3. **Given** a registered settings section, **When** the user opens the Settings page, **Then** the section renders with the defined UI controls

---

### User Story 2 - User Manages Plugin Settings (Priority: P1)

A planning engineer opens the Settings page, sees all plugin settings organized by category, modifies values, sees validation feedback in real-time, and saves changes.

**Why this priority**: This is the primary user-facing value — engineers need to configure plugins without navigating separate pages or files.

**Independent Test**: Can be fully tested by opening the Settings page, modifying a text field, observing validation feedback, and confirming the value persists after restart.

**Acceptance Scenarios**:
1. **Given** the Settings page with multiple plugin sections, **When** the user changes a setting, **Then** validation runs immediately and shows errors or success
2. **Given** invalid setting input, **When** the user tries to save, **Then** the system shows a validation error and prevents saving
3. **Given** valid settings changes, **When** the user saves, **Then** the settings persist and are restored on next application launch

---

### User Story 3 - Import/Export Settings (Priority: P2)

A user exports all plugin settings to a portable file for backup or transfer to another machine, then imports them on another installation.

**Why this priority**: This is important for enterprise deployment and disaster recovery but not needed for initial daily use.

**Independent Test**: Can be fully tested by exporting settings to a file, modifying a setting, importing the saved file, and verifying the original value is restored.

**Acceptance Scenarios**:
1. **Given** multiple plugins with registered settings, **When** the user triggers full export, **Then** all plugin settings are exported to a single JSON file
2. **Given** an exported settings file, **When** the user triggers import, **Then** all settings are restored to their exported values
3. **Given** the export/import UI, **When** the user selects per-plugin export, **Then** only that plugin's settings are exported

---

### User Story 4 - API Key Management (Priority: P2)

A user configures AI provider API keys through the settings UI. Keys are encrypted at rest and masked in the UI for security.

**Why this priority**: AI provider integration depends on secure API key storage, but the initial implementation can use unencrypted storage as a fallback.

**Independent Test**: Can be fully tested by saving an API key, verifying the UI shows it masked, restarting the application, and confirming the key is still usable by the AI provider.

**Acceptance Scenarios**:
1. **Given** the API key input field, **When** the user types a key, **Then** the input is masked (password-style)
2. **Given** a saved API key, **When** viewing the settings, **Then** the key appears masked
3. **Given** a saved API key, **When** written to disk, **Then** the key is encrypted and not in plaintext

---

### Edge Cases

- What happens when two plugins register settings sections with the same category name? — Sections are grouped under categories, same-name categories merge; duplicate section keys within a category are rejected with a validation error.
- How does the system handle a plugin that is disabled or uninstalled? — Its settings remain but are marked as orphaned; orphaned settings are automatically purged after a configurable grace period (default: 30 days), with a user option to manually purge earlier.
- What happens when the settings persistence file is corrupted? — System falls back to default settings for all plugins and logs a warning.
- How does the system handle settings version migration when a plugin updates its schema? — Each settings section carries a version number; the registry applies migration handlers if the stored version differs from the registered version.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `SettingsRegistry` that allows plugins to register settings sections, validation rules, UI controls, and persistence handlers via `ISettingsModule`
- **FR-002**: Plugins MUST be able to auto-register settings via the `[SettingsSection]` attribute on their assembly or class
- **FR-003**: The Settings page MUST dynamically build its UI from registered sections, rendering each section's defined controls in the specified order
- **FR-004**: Validation rules MUST run on settings change and display feedback without requiring a page reload
- **FR-005**: Users MUST be able to export settings per-plugin or as a full export to a portable file
- **FR-006**: Users MUST be able to import settings from an exported file, restoring all settings or selected plugin settings
- **FR-007**: API keys stored by the settings system MUST be encrypted at rest
- **FR-008**: API key input fields MUST mask the key value in the UI
- **FR-009**: The settings system MUST handle persistence versioning, applying migrations when a plugin registers a new schema version
- **FR-010**: If the persistence file is corrupted or missing, the system MUST fall back to default settings for all plugins without crashing
- **FR-011**: Existing settings (Theme, Accessibility, Performance, Diagnostics, Excel) MUST be migrated from their current persistence mechanism to the new registry
- **FR-012**: When a setting value is saved, the system MUST emit a settings-changed event so that affected plugins can hot-reload their behavior without restart
- **FR-013**: The Settings page MUST support rendering at minimum these control types: text input, toggle switch, dropdown, numeric spinner, slider, color picker, file/folder path picker
- **FR-014**: The settings system MUST log every settings change with timestamp, plugin ID, setting key, and before/after values using the existing diagnostics infrastructure

### Key Entities *(include if feature involves data)*

- **SettingsRegistry**: Central registry that holds all registered settings sections from all plugins. Supports lookup by plugin ID, category, and section key. Emits settings-changed events for hot-reload.
- **SettingsSection**: A named group of settings with a category, display order, version number, validation rules, and UI control definitions.
- **SettingsValue**: A key-value pair with metadata including type, validation constraints, default value, and encryption flag.
- **SettingsPersistenceHandler**: Interface that plugins implement to define how their settings are stored and loaded (file-based, encrypted, etc.).
- **SettingsExport**: A portable snapshot of settings from one or more plugins, structured for import/export operations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new plugin can register 3 settings sections with 10+ controls and have them appear in the Settings page within 1 second of plugin load
- **SC-002**: Users can navigate all plugin settings sections and modify values without page flicker or load delay exceeding 200ms per section
- **SC-003**: Validation feedback appears within 100ms of the user finishing input in a field
- **SC-004**: Full export/import of 10+ plugins with 50+ settings completes in under 2 seconds
- **SC-005**: Encrypted API keys cannot be read in plaintext from the persistence file using standard file viewers
- **SC-006**: If settings file is deleted, all 5 existing settings categories (Theme, Accessibility, Performance, Diagnostics, Excel) continue to work with their defaults — zero crashes

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Users have the Shell workspace available with sidebar navigation (Phase 15 prerequisite)
- The `[SettingsSection]` attribute is already defined in the Som3a.Contracts or Som3a.Plugin.SDK layer (Phase 14 prerequisite)
- `ISettingsModule` interface is already defined in Som3a.Contracts (Phase 14 prerequisite)
- Settings UI will live under the existing Shell/Settings navigation category
- Each plugin manages its own settings schema and migration logic — the registry only calls migration handlers, does not infer schema changes
- Encryption uses the platform's existing `ApiKeyEncryption` service from Som3a.Infrastructure (Phase 14 prerequisite)
- Existing settings (Theme, Accessibility, etc.) currently persist via `Properties/Settings.settings` (ApplicationSettingsBase) and `Services/ThemeSettings.cs` — these will be migrated incrementally, not all at once
- Orphaned settings auto-purge grace period defaults to 30 days unless configured otherwise per plugin

## Clarifications

### Session 2026-05-26

- Q: Should settings changes take effect immediately (hot-reload) or require restart? → A: Hot-reload via event notification — the registry emits a settings-changed event that plugins subscribe to for instant behavior updates
- Q: What UI control types should the dynamic settings system support? → A: Standard types — text input, toggle switch, dropdown, numeric spinner, slider, color picker, file/folder path picker
- Q: What level of diagnostics/logging should the settings system capture? → A: Changes only — log every settings change with timestamp, plugin ID, setting key, and before/after values
- Q: How should the system handle old settings when a plugin removes or renames them? → A: Orphan with automatic purge after a configurable migration grace period
