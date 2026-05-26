# Implementation Plan: Dynamic Settings Platform

**Branch**: `feature/phase-16-dynamic-settings-platform` | **Date**: 2026-05-26 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/016-dynamic-settings-platform/spec.md`

## Summary

Replace the existing hardcoded SettingsPage (with static Appearance, Performance, Accessibility, Diagnostics, Excel panels) with a dynamic registry-based settings system. Every plugin registers its own settings sections, validation rules, UI controls, and persistence handlers via `ISettingsModule`. The settings page dynamically builds its UI from the registry, supports hot-reload via EventBus events, encrypted API key storage via `Som3a.Infrastructure.ApiKeyEncryption`, per-plugin JSON persistence, and import/export. Existing settings (Theme, Accessibility, etc.) are migrated incrementally.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for WpfApp2 WPF host; .NET 8.0 for Som3a.Contracts + Som3a.Plugin.SDK + Som3a.Infrastructure)

**Primary Dependencies**:
- **Existing**: SettingsPage.xaml/.cs, SettingsViewModel.cs, UserSettings.cs, SettingsCategory.cs, SettingsPersistenceService.cs, AppearancePanel/PerformancePanel/AccessibilityPanel/DiagnosticsPanel/ExcelPanel/PluginsPanel, ServiceContainer, EventBus, ModuleRegistry
- **From Phase 14**: Som3a.Contracts (`ISettingsModule`, `ISettingsRegistry`), Som3a.Plugin.SDK (`AssemblyScanner`, `PluginDiscoverer`), Som3a.Infrastructure (`ApiKeyEncryption`)
- **WPF**: DataTemplateSelector for dynamic control rendering, ContentControl with DataTemplate, WPF Expander/GroupBox for section grouping
- **Storage**: `System.Text.Json` for per-plugin settings files, `System.Security.Cryptography.ProtectedData` (DPAPI) for encrypted secrets

**Storage**:
- Per-plugin JSON settings at `AppData/Som3a/Plugins/{ModuleId}/settings.json`
- Encrypted secrets (API keys) at `AppData/Som3a/Plugins/{ModuleId}/secrets.json` via DPAPI
- Import/export produces portable JSON bundles with all plugins or per-plugin
- Existing `Properties.Settings.Default` (ApplicationSettingsBase) remains for pre-migration settings during incremental cutover

**Testing**:
- Unit tests (xUnit): SettingsRegistry registration/deduplication logic, validation rule engine, import/export format contract, encrypted field serialization
- Integration tests: Plugin discovery auto-registration, settings persistence round-trip, hot-reload EventBus event delivery, existing settings migration (Theme, Accessibility, etc.)
- Manual: Excel VSTO host test for settings page rendering, keyboard navigation through dynamic sections, validation feedback timing

**Target Platform**: Windows (x64) — Excel VSTO Add-in host (.NET Framework 4.8)

**Project Type**: WPF VSTO Add-in (desktop application hosted inside Excel)

**Performance Goals**:
- Settings page load within 1 second for 10 plugins with 50+ settings (SC-001)
- Validation feedback within 100ms of user finishing input (SC-003)
- Full export/import of 10+ plugins with 50+ settings completes in under 2 seconds (SC-004)
- No blocking of Excel UI thread during save/export/import

**Constraints**:
- All settings page UI MUST use `{DynamicResource}` for themeable properties (Constitution III)
- Theme mutations MUST route through ThemeManager (Constitution IV) — settings page does not mutate themes
- Animations for section expand/collapse ≤200ms, GPU-safe (Constitution IX)
- Excel VSTO rendering safety: settings page is a Shell Page, already VSTO-safe (Constitution X)
- No new standalone windows — settings page remains inside Shell (Constitution XI)
- No inline effects — centralized effects from Effects/Shadows.xaml (Constitution XII)
- Settings sections re-register on plugin load, not persisted across sessions in memory (FR-001)

**Scale/Scope**:
- ~5 existing settings categories to migrate (Theme, Accessibility, Performance, Diagnostics, Excel)
- 7 control types to implement: text input, toggle switch, dropdown, numeric spinner, slider, color picker, file/folder path picker (FR-013)
- Target: 10-50 plugins each registering 1-10 sections with 1-20 controls
- 5 key source files modified: SettingsPage, SettingsViewModel, SettingsPersistenceService, CompositionRoot
- 1 new service: SettingsRegistry (backed by concrete SettingsRegistry class)
- 1 new EventBus event type: `SettingsChangedEvent`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — The settings registry, persistence handlers, and dynamic UI rendering are isolated in Services/ and Pages/ respectively. The SettingsRegistry is a service class, not a monolithic dictionary. Control template selectors are localized to the SettingsPage. No new ResourceDictionaries beyond styles for new control types in existing theme files.
- [x] **III. DynamicResource-Only** — The SettingsPage already uses `{DynamicResource}` for themeable brushes, colors, borders, and effects. All new dynamic controls (slider, color picker, etc.) will use DynamicResource for themeable properties. No StaticResource introduced.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation continues through ThemeManager exclusively. The settings page reads/selects theme settings but does not mutate brushes directly. ApplyTheme calls route through ThemeManager.
- [x] **IX. Animation Governance** — Section expand/collapse animations (if any) ≤200ms and GPU-safe (opacity/height transitions only). Validation feedback is instantaneous (no animation).
- [x] **X. Excel Rendering Safety** — SettingsPage already exists as a Shell Page inside ShellWindow which uses WindowRenderModeDetector. No new transparency layers or popups. All new controls tested in Excel VSTO host.
- [x] **XI. WindowChrome Enforcement** — No new standalone windows. Settings page remains a Page within the Shell workspace (ModernWindow). No custom windows introduced.
- [x] **XII. Centralized Effects** — No inline DropShadowEffect. All effects sourced from Effects/Shadows.xaml. Settings controls use existing ButtonStyles, ComboBoxStyles, CheckBoxStyles etc. from Theme/Controls/.
- [x] **XV. Resource Loading Order** — No new resource dictionaries introduced beyond potential control styles for new control types (which follow loading order). ThemeResources.xaml unchanged.

## Project Structure

### Documentation (this feature)

```text
specs/016-dynamic-settings-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── ISettingsRegistry.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
WpfApp2/                                  # .NET Framework 4.8 VSTO + WPF host
├── Services/
│   ├── SettingsRegistry.cs               # NEW: ISettingsRegistry implementation
│   ├── SettingsPersistenceService.cs     # MODIFY: Add per-plugin JSON support
│   ├── SettingsChangedEvent.cs           # NEW: EventBus event type for hot-reload
│   ├── DynamicSettingsRenderer.cs        # NEW: Control type → WPF DataTemplate mapping
│   ├── SettingsValidator.cs              # NEW: Validation rule engine
│   └── SettingsMigrationService.cs       # NEW: Incremental migration from legacy storage
│
├── ViewModels/
│   ├── SettingsViewModel.cs              # MODIFY: Read from SettingsRegistry instead of hardcoded panels
│   ├── SettingsSectionViewModel.cs       # NEW: Wrap registered section for UI binding
│   ├── SettingControlViewModel.cs        # NEW: Wrap individual setting with value/validation state
│   └── PluginSettingsViewModel.cs        # NEW: Per-plugin settings view model
│
├── Views/
│   ├── SettingsSectionView.xaml/.cs      # NEW: Renders a single settings section with controls
│   ├── SettingControlSelector.cs         # NEW: DataTemplateSelector by SettingValueType
│   ├── SettingControlTemplates.xaml      # NEW: DataTemplates for each control type
│   └── [existing panels]                 # Pre-migration — kept as fallback during incremental cutover
│
├── Pages/
│   ├── SettingsPage.xaml/.cs             # MODIFY: Dynamic sections from registry
│   └── PageBase.cs                       # (unchanged)
│
├── Models/
│   ├── UserSettings.cs                   # Pre-migration — kept during incremental cutover
│   └── SettingsCategory.cs               # Pre-migration — kept during incremental cutover
│
├── CompositionRoot.cs                    # MODIFY: Register SettingsRegistry, SettingsPersistenceService
│
├── Som3a.Contracts/                      # .NET 8.0 (from Phase 14)
│   └── ISettingsRegistry.cs              # (interface only — implementation in WpfApp2.Services)
│
├── Som3a.Plugin.SDK/                     # .NET 8.0 (from Phase 14)
│   └── Attributes/
│       └── SettingsSectionAttribute.cs   # MODIFY: Extend if needed for new fields
│
├── Som3a.Infrastructure/                 # .NET 8.0 (from Phase 14)
│   └── Security/
│       └── ApiKeyEncryption.cs           # (existing, reused for encrypted settings)
│
└── Som3a.Bridge/                         # .NET Standard 2.0 interop
    └── SettingsContractsBridge.cs        # MODIFY: Bridge for ISettingsModule <-> WPF interop
```

**Structure Decision**: Single project (WpfApp2) with modifications plus existing Phase 14 library references. Key new code lives in `Services/` (registry, persistence, event) and `Views/` (dynamic control rendering). The existing `SettingsPage.xaml` is enhanced to read from the registry. The old panel system (`AppearancePanel`, `PerformancePanel`, etc.) is kept as fallback during incremental migration and removed when all settings categories have been migrated to the registry.

## Complexity Tracking

> No constitution violations to justify. All gates pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
