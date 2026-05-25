# Implementation Plan: Settings & Personalization UX

**Branch**: `010-settings-personalization-ux` | **Date**: 2026-05-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-settings-personalization-ux/spec.md`

## Summary

Refactor the existing SettingsWindow into a categorized Windows 11-style settings experience with 6 category panels (Appearance, Performance, Accessibility, Diagnostics, Excel, Plugins), live theme preview with Apply/Cancel flow, accent color picker, background style selector, animation speed and density controls, and settings import/export. All new settings panels follow existing MVVM patterns, use DynamicResource for themeable properties, and route theme mutations through ThemeManager.

## Technical Context

**Language/Version**: .NET Framework 4.8 / C# 7.3

**Primary Dependencies**: WPF, Excel VSTO, ThemeManager (existing), RenderModeService (existing), ModernWindow (existing)

**Storage**: Properties.Settings.Default (ApplicationSettingsBase) extended with JSON serialization for import/export

**Testing**: Manual Excel host testing; visual verification of theme/preview rendering; import/export file format validation via unit tests

**Target Platform**: Excel VSTO (Windows 10/11)

**Project Type**: Desktop add-in (Excel VSTO)

**Performance Goals**: Panel switching <500ms, live preview render <1s, theme apply propagate <2s, settings persistance <500ms

**Constraints**: DynamicResource-only for themeable properties; Excel VSTO safe rendering; animations ≤200ms; centralized effects only; MVVM with no code-behind business logic; ThemeManager-only theme mutations

**Scale/Scope**: Single-user desktop add-in; 6 settings categories; ~15 setting values; up to 100KB export file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Settings panels follow existing modular pattern (Views/ + ViewModels/ + Services/); no monolithic ResourceDictionaries introduced.
- [x] **III. DynamicResource-Only** — All new or refactored settings UI uses DynamicResource for themeable properties; StaticResource prohibited.
- [x] **IV. Runtime Theme Mutation Governance** — All theme mutations routed through ThemeManager via the Apply button; no direct brush manipulation.
- [x] **IX. Animation Governance** — Animation speed controls tune existing animation infrastructure; no new heavy animations introduced.
- [x] **X. Excel Rendering Safety** — SettingsWindow inherits from ModernWindow; WindowRenderModeDetector handles fallback automatically.
- [x] **XI. WindowChrome Enforcement** — SettingsWindow already uses ModernWindow; no new windows introduced.
- [x] **XII. Centralized Effects** — No new inline effects; all effects sourced from existing Effects/Shadows.xaml and Effects/Glow.xaml.
- [x] **XV. Resource Loading Order** — No new ResourceDictionaries added to loading sequence; settings are data, not theme resources.

All gates pass. No violations to justify in Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-settings-personalization-ux/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
WpfApp2/
├── Views/
│   └── SettingsWindow.xaml           # Refactored: categorized panels + sidebar
│   └── SettingsWindow.xaml.cs        # Code-behind (UI-only)
├── ViewModels/
│   └── SettingsViewModel.cs          # NEW: settings state, commands, preview logic
├── Services/
│   └── SettingsPersistenceService.cs # NEW: read/write/import/export settings
├── Theme/
│   └── Controls/
│       └── SettingsPanelStyles.xaml  # NEW: panel-specific styles (sidebar, cards, controls)
├── Models/
│   └── UserSettings.cs              # NEW: settings data model
│   └── SettingsExport.cs            # NEW: serialization model with version metadata
```

## Complexity Tracking

No Constitution violations — Complexity Tracking not required.
