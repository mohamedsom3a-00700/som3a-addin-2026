# Implementation Plan: Control Standardization

**Branch**: `feature/phase-04-control-standardization` | **Date**: 2026-05-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/007-control-standardization/spec.md`

## Summary

Standardize all WPF controls across the Excel VSTO add-in to ensure consistent appearance, behavior, keyboard accessibility, and Excel-safe rendering. The primary workstreams are: (1) audit and deduplicate control styles, (2) fix ComboBox popup architecture (width, clipping, shadow, Excel rendering), (3) standardize TextBox, DataGrid, ListView, TreeView, PasswordBox templates, (4) add virtualization and smooth scrolling, (5) enforce keyboard navigation and accessibility states, and (6) remove all inline colors/shadows in favor of centralized tokens.

## Technical Context

**Language/Version**: .NET Framework 4.8, C#, XAML (WPF)

**Primary Dependencies**: Native WPF (no third-party UI frameworks per Constitution §XIV), Theme engine (ThemeManager, ThemeResources.xaml, Effects/*.xaml from Phase 3)

**Storage**: N/A — no data storage required for control styling

**Testing**: msbuild build verification + manual Excel VSTO visual testing across Dark/Light/Custom themes at 100%/150% DPI

**Target Platform**: Windows 10/11, Excel VSTO Add-in host

**Project Type**: Desktop application (WPF + Excel VSTO Add-in)

**Performance Goals**: DataGrid scrolling at 30+ FPS with 1000+ rows; control animations ≤200ms

**Constraints**: DynamicResource-only for themeable properties; no inline colors/shadows; centralized effects only; Excel safe-mode rendering support; all controls inherit ModernWindow

**Scale/Scope**: 10 control types (Button, TextBox, ComboBox, CheckBox, RadioButton, ToggleButton, DataGrid, ListView, TreeView, PasswordBox); 14 existing windows; ~12 existing control style XAML files in Theme/Controls/

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — This feature extends existing modular dictionaries in Theme/Controls/. No monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — FR-011, FR-012 mandate DynamicResource-only for all themeable properties. No StaticResource violations permitted.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation continues through ThemeManager. Control styles consume existing tokens; no direct mutation.
- [x] **IX. Animation Governance** — FR-010 mandates ≤200ms animations. FR-006 uses centralized Glow.Focus. Reduced motion support included.
- [x] **X. Excel Rendering Safety** — FR-002, FR-003 mandate ComboBox safe-mode variants. All changes must work in FallbackSafe mode.
- [x] **XI. WindowChrome Enforcement** — No new windows created. All existing windows already inherit ModernWindow.
- [x] **XII. Centralized Effects** — FR-012 prohibits inline DropShadowEffect. All shadows sourced from Effects/Shadows.xaml.
- [x] **XV. Resource Loading Order** — New/modified control styles follow existing loading order in ThemeResources.xaml (Control Styles loaded after Effects, before Theme Overrides).

All checks pass. No violations to justify.

## Project Structure

```text
specs/007-control-standardization/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (internal project — no external interfaces to document)
└── tasks.md             # Phase 2 output (/speckit.tasks command)

WpfApp2/
├── Theme/
│   ├── Controls/        # Existing + modified control style files
│   │   ├── ButtonStyles.xaml
│   │   ├── ComboBoxStyles.xaml      # Primary refactor target (popup architecture)
│   │   ├── ComboBoxItemStyles.xaml  # Review for consistency
│   │   ├── TextBoxStyles.xaml       # Standardize
│   │   ├── DataGridStyles.xaml      # Standardize + virtualization
│   │   ├── ListViewStyles.xaml      # Standardize + virtualization
│   │   ├── ListViewItemStyles.xaml  # Review for consistency
│   │   ├── TreeViewStyles.xaml      # Create if missing; keyboard nav
│   │   ├── PasswordBoxStyles.xaml   # Create if missing
│   │   └── ... (existing: CheckBox, RadioButton, ToggleButton, ScrollViewer, etc.)
│   ├── Base/
│   ├── Effects/
│   ├── Dark/
│   ├── Light/
│   ├── Custom/
│   └── ThemeResources.xaml
├── Services/
│   └── RenderModeService.cs
├── Controls/
│   └── ModernWindow.cs
└── Views/               # 14 window files — each may need minor template fixes
```

**Structure Decision**: Single WPF project (`WpfApp2/`) with existing modular Theme/ directory structure. No new projects or folders required. All changes are contained within `Theme/Controls/` directory and minor window-level fixes in `Views/`.

## Complexity Tracking

No Constitution Check violations. Complexity tracking not required.
