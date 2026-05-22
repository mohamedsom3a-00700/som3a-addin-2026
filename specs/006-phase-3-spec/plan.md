# Implementation Plan: Theme Engine 2.0

**Branch**: `006-phase-3-spec` | **Date**: 2026-05-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/006-phase-3-spec/spec.md`

## Summary

Build a production-grade Fluent Runtime Theme Engine enabling runtime Dark/Light/Custom theme switching with accent color support, Excel VSTO-safe rendering, centralized effects, keyboard accessibility, and DPI-aware controls across all 14 application windows.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8

**Primary Dependencies**: Native WPF, Excel VSTO, ApplicationSettingsBase

**Storage**: `Properties/Settings.settings` (ApplicationSettingsBase) — SelectedTheme + AccentColor

**Testing**: Manual verification via `msbuild` + runtime checks inside Excel VSTO host

**Target Platform**: Windows 10/11, Excel 2016+ VSTO host

**Project Type**: Desktop application — WPF with VSTO Excel add-in

**Performance Goals**: Theme switching <1s, all animations ≤200ms, DataGrid virtualization for 1000+ rows

**Constraints**: DynamicResource-only for themeable properties; no inline DropShadowEffect; all animations ≤200ms; Excel VSTO rendering safety; no third-party UI frameworks

**Scale/Scope**: 14 windows, 3 built-in themes + Custom with 8 accent presets, ~50 XAML resource files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Resources organized into modular dictionaries (Base/, Dark/, Light/, Custom/, Controls/, Effects/). No monolithic dictionaries.
- [x] **III. DynamicResource-Only** — All themeable properties use `{DynamicResource}`. No StaticResource for brushes, colors, borders, effects.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation exclusively through ThemeManager singleton. No direct brush mutation from windows or controls.
- [x] **IX. Animation Governance** — All animations ≤200ms, GPU-safe, CubicEase for easing.
- [x] **X. Excel Rendering Safety** — WindowRenderModeDetector auto-detects VSTO hosting; safe fallback mode for problematic systems.
- [x] **XI. WindowChrome Enforcement** — All windows inherit ModernWindow with WindowChrome as primary strategy.
- [x] **XII. Centralized Effects** — Zero inline DropShadowEffect. All effects centralized in Effects/Shadows.xaml and Effects/Glow.xaml.
- [x] **XV. Resource Loading Order** — ThemeResources.xaml follows prescribed sequence: Base → Effects → Controls → Window Styles → Theme Overrides.

## Project Structure

### Documentation (this feature)

```text
specs/006-phase-3-spec/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
├── checklists/
│   ├── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Theme/
│   ├── Base/
│   │   ├── Colors.xaml           # Primitive + Semantic tokens
│   │   ├── Typography.xaml
│   │   ├── Spacing.xaml
│   │   ├── Radius.xaml
│   │   ├── Elevation.xaml
│   │   ├── Motion.xaml
│   │   ├── ZIndex.xaml
│   │   ├── Opacity.xaml
│   │   └── ComponentTokens.xaml
│   ├── Dark/
│   │   ├── DarkColors.xaml
│   │   └── DarkTheme.xaml
│   ├── Light/
│   │   ├── LightColors.xaml
│   │   └── LightTheme.xaml
│   ├── Custom/
│   │   ├── CustomColors.xaml
│   │   └── CustomTheme.xaml
│   ├── Controls/
│   │   ├── ButtonStyles.xaml
│   │   ├── ComboBoxStyles.xaml
│   │   ├── DataGridStyles.xaml
│   │   ├── ThemeCardStyles.xaml
│   │   ├── AccentSwatchStyles.xaml
│   │   ├── CheckBoxStyles.xaml
│   │   ├── RadioButtonStyles.xaml
│   │   ├── ToggleButtonStyles.xaml
│   │   ├── ScrollViewerStyles.xaml
│   │   ├── TextBoxStyles.xaml
│   │   └── ... (other controls)
│   ├── Effects/
│   │   ├── Shadows.xaml
│   │   ├── Glow.xaml
│   │   └── Animations.xaml
│   ├── ModernWindow.xaml
│   └── ThemeResources.xaml
├── Services/
│   ├── ThemeManager.cs
│   ├── WindowRenderModeDetector.cs
│   ├── RenderModeService.cs
│   └── ThemeSettings.cs
├── Controls/
│   └── ModernWindow.cs
├── Views/
│   └── SettingsWindow.xaml
├── Properties/
│   └── Settings.settings
└── App.xaml
```

**Structure Decision**: Single WPF project (Som3a_WPF_UI.csproj) with modular ResourceDictionary libraries under Theme/. ThemeManager singleton orchestrates runtime switching. WindowRenderModeDetector provides VSTO-safe rendering path.

## Complexity Tracking

No constitutional violations. All gates pass without justification needed.
