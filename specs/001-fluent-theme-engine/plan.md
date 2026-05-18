# Implementation Plan: WPF Fluent UI Migration — Theme Engine & Runtime Switching

**Branch**: `[002-fluent-theme-engine]` | **Date**: 2026-05-18 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-fluent-theme-engine/spec.md`

## Summary

Migrate the WPF UI layer of an Excel-hosted VSTO add-in to a production-grade Fluent Design system with runtime theme switching (Dark/Light/Custom), theme card UI in SettingsWindow, centralized shadow/glow/animation library, VisualStateManager for four high-priority controls, and DPI-aware rendering. The system uses WindowChrome for borderless windows with automatic fallback-safe mode detection for Excel hosting edge cases. The token system follows a two-tier Primitive + Semantic architecture enabling custom accent color selection via preset swatches. All changes follow the Incremental Migration Rules with validation gates before advancing phases.

## Technical Context

**Language/Version**: C# 8.0 / .NET Framework 4.8

**Primary Dependencies**: WPF (native), WindowChrome, WebView2 (conditional), Microsoft Office Interop (VSTO)

**Storage**: Application Settings (Properties.Settings.Default for theme preference + accent color)

**Testing**: Visual/manual testing (WPF UI), no automated UI testing framework present

**Target Platform**: Windows Desktop (Excel 2016+ VSTO Add-in)

**Project Type**: Desktop application (WPF + VSTO)

**Performance Goals**: Theme switch < 1s, UI response in Excel host with 1000-row DataGrid, smooth scrolling, no frame drops, animations ≤ 200ms

**Constraints**: VSTO add-in performance overhead, Excel hosting rendering constraints, no third-party UI frameworks, MVVM architecture, no inline color values in templates

**Scale/Scope**: 9-phase incremental migration, 3 built-in themes + 1 custom, 7 control types, 3 accent color presets (Custom theme), 1 settings window, multiple content windows

## Constitution Check

GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Library-First Modular Architecture** | ✅ PASS | Theme system follows folder structure (Base/, Dark/, Light/, Custom/, Controls/, Effects/). ResourceDictionaries independently testable. |
| **II. MVVM Architecture** | ✅ PASS | Business logic in ViewModels; code-behind minimal. ThemeManager is a service (not ViewModel), controls bind via DynamicResource. |
| **III. DynamicResource Only** | ✅ PASS | All control templates use DynamicResource for theme colors. No StaticResource for themeable properties. |
| **IV. Runtime Theme Switching** | ✅ PASS | ThemeManager replaces merged dictionaries dynamically. User preferences preserved across sessions. |
| **V. Feature Completeness Standard** | ✅ PASS | All controls theme-aware, runtime switchable, keyboard accessible, DPI safe. |
| **VI. Performance & Efficiency** | ✅ PASS | Brushes reused globally, no nested DropShadows, virtualized DataGrid rows, ≤200ms animations. |
| **Resource Loading Order** | ✅ PASS | 8-step explicit order defined in constitution. ThemeManager maintains correct merge order. |
| **Primitive & Semantic Token Architecture** | ✅ PASS | Two-tier separation in Colors.xaml. Semantic tokens alias primitives. |
| **Popup Architecture Rules** | ✅ PASS | ComboBox popup uses AllowsTransparency=False, correct Placement/PlacementTarget, centralized shadow. |
| **VisualStateManager Strategy** | ✅ PASS | VSM scope: ComboBox, Button, ToggleButton, ThemeCards only. Incremental migration one control at a time. |
| **Incremental Migration Rules** | ✅ PASS | Excel host validation gate before multi-window migration. |
| **Performance Budget Rules** | ✅ PASS | No nested DropShadows, no BlurEffect on scrollable containers, animations ≤200ms. |
| **WindowChrome Enforcement** | ✅ PASS | WindowChrome preferred with automatic runtime fallback detection. |
| **Design Authority Rules** | ✅ PASS | No third-party UI frameworks, no inline DropShadowEffect, no architecture replacement without approval. |
| **Theme Validation Checklist** | ✅ PASS | 8-gate validation before each theme is considered complete. |

**Constitution Check Result**: ✅ ALL GATES PASS — Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/001-fluent-theme-engine/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── theme-api.md     # ThemeManager service contract
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Theme/
│   ├── Base/
│   │   └── Colors.xaml              # Primitive + semantic tokens (existing, enhanced)
│   ├── Dark/
│   │   ├── DarkColors.xaml           # Dark theme semantic token overrides (NEW)
│   │   └── DarkTheme.xaml            # Dark merged dictionary (NEW)
│   ├── Light/
│   │   ├── LightColors.xaml           # Light theme semantic token overrides (NEW)
│   │   └── LightTheme.xaml           # Light merged dictionary (NEW)
│   ├── Custom/
│   │   ├── CustomColors.xaml          # Custom theme with accent color tokens (NEW)
│   │   └── CustomTheme.xaml          # Custom merged dictionary (NEW)
│   ├── Controls/
│   │   ├── ThemeCardStyles.xaml       # Theme card styles (NEW)
│   │   ├── CheckBoxStyles.xaml        # Fluent CheckBox (NEW)
│   │   ├── RadioButtonStyles.xaml     # Fluent RadioButton (NEW)
│   │   ├── ToggleButtonStyles.xaml    # Fluent ToggleButton (NEW)
│   │   ├── ScrollViewerStyles.xaml    # Fluent ScrollViewer (NEW)
│   │   ├── ComboBoxStyles.xaml        # ComboBox with VSM (existing, enhanced)
│   │   └── ButtonStyles.xaml         # Button with VSM (existing, enhanced)
│   ├── Effects/
│   │   ├── Shadows.xaml               # Centralized DropShadowEffect definitions (NEW)
│   │   ├── Glow.xaml                  # Centralized glow effects (NEW)
│   │   └── Animations.xaml            # Control state + popup animations (NEW)
│   ├── ModernWindow.xaml               # ModernWindow base (existing)
│   ├── WindowStyles.xaml               # Window styles (existing)
│   ├── WindowAnimations.xaml           # Window animations (existing)
│   └── ThemeResources.xaml             # Aggregator ResourceDictionary (existing, ordered)
├── Controls/
│   └── ModernWindow.xaml.cs           # ModernWindow code-behind (existing)
├── Services/
│   └── ThemeManager.cs                # Runtime theme switching service (NEW)
├── ViewModels/
│   ├── SettingsViewModel.cs           # Settings window VM (existing, enhanced)
│   └── MainViewModel.cs               # Main window VM (existing)
├── Views/
│   ├── SettingsWindow.xaml            # Settings with theme cards (existing, enhanced)
│   └── SettingsWindow.xaml.cs         # Settings code-behind (existing, minimal)
└── Properties/
    ├── Settings.settings               # Theme + accent preference storage (NEW)
    └── Settings.Designer.cs           # Settings accessor (NEW)
```

**Structure Decision**: Single WpfApp2 project with Theme/ folder following the modular folder structure defined in the constitution. Services/ for ThemeManager, ViewModels/ for MVVM, Controls/ for custom controls. The existing WpfApp2 already follows this structure.

## Complexity Tracking

No constitution violations requiring justification. All principles are satisfied with the planned implementation approach.