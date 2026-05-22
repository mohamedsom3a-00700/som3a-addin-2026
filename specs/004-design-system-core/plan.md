# Implementation Plan: Design System Core

**Branch**: `004-design-system-core` | **Date**: 2026-05-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-design-system-core/spec.md`

## Summary

Phase 1 converts the current WPF styling into a three-tier enterprise token architecture (Primitive → Semantic → Component), eliminates all inline violations (hex colors, hardcoded margins/padding, inline DropShadowEffects), standardizes spacing and typography tokens, creates missing token files (Elevation, Motion, ZIndex, Opacity are already present but need audit), updates the ThemeResources.xaml aggregator, and validates token compliance via build-time lint and runtime ThemeManager checks.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8, WPF XAML

**Primary Dependencies**: WPF (System.Xaml, PresentationFramework), Microsoft.Extensions.DependencyInjection (future Phase 6, not this phase)

**Storage**: XAML ResourceDictionary files for tokens, Properties/Settings for theme persistence

**Testing**: msbuild compile verification + grep-based lint scripts + manual Excel VSTO runtime testing

**Target Platform**: Windows Desktop (Excel VSTO Add-in host)

**Project Type**: Desktop application (WPF add-in)

**Performance Goals**: Theme switch <200ms, no UI freeze, Excel-host-safe rendering

**Constraints**: No third-party UI frameworks (Constitution XIV), DynamicResource-only for themeable properties (Constitution III), no inline effects (Constitution XII), all tokens immutable at runtime (Constitution V)

**Scale/Scope**: 67 existing resource files across Theme/, ~15 Primitive tokens, ~25 Semantic Color tokens, ~27 Semantic Brush tokens, ~35 Legacy Flat Keys, ~9 inline hex violations in controls/windows

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Feature introduces ComponentTokens.xaml as a new modular dictionary; all resources remain isolated and testable.
- [x] **III. DynamicResource-Only** — Feature explicitly eliminates StaticResource on themeable properties and inline hex colors per FR-006, FR-010.
- [x] **IV. Runtime Theme Mutation Governance** — No direct brush mutation; all theme changes route through ThemeManager. FR-013 ensures theme switching regression.
- [x] **V. Primitive & Semantic Token Architecture** — Feature IS the Primitive/Semantic/Component architecture. Semantic-to-semantic chains prohibited per clarification.
- [x] **IX. Animation Governance** — Motion tokens already exist; no animation changes in this phase beyond Motion.xaml audit.
- [x] **X. Excel Rendering Safety** — WindowRenderModeDetector not modified; existing fallback mode preserved.
- [x] **XI. WindowChrome Enforcement** — No window chrome changes in this phase.
- [x] **XII. Centralized Effects** — Elevation.xaml DropShadowEffects in Base/ need resolution (V8 violation documented); controls reference centralized effects.
- [x] **XV. Resource Loading Order** — Feature updates ThemeResources.xaml to documented order per FR-012.

**Violations to resolve:**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Elevation.xaml contains DropShadowEffect definitions in Base/ instead of Effects/ | .NET Framework 4.8 compat per in-file comment | Relocating would require controls to choose between `Shadow.*` and `Elevation.*` keys — resolve by having Elevation.* reference Shadow.* keys or move effects to Effects/ and keep Elevation.xaml as abstraction-only |

## Project Structure

### Documentation (this feature)

```text
specs/004-design-system-core/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── token-api.md      # Token contract definitions
└── checklists/
    └── requirements.md   # Spec quality checklist
```

### Source Code (repository root)

```text
WpfApp2/Theme/
├── Base/
│   ├── Colors.xaml              # MODIFY: add Primitive.* tokens, consolidate Semantic tokens
│   ├── Typography.xaml          # MODIFY: audit, ensure Style.Text.* styles present
│   ├── Spacing.xaml             # MODIFY: audit, ensure full token set present
│   ├── Radius.xaml              # AUDIT: verify completeness
│   ├── Elevation.xaml           # MODIFY: resolve DropShadowEffect placement
│   ├── Motion.xaml              # AUDIT: verify completeness
│   ├── ZIndex.xaml              # AUDIT: verify completeness
│   ├── Opacity.xaml             # AUDIT: verify completeness
│   └── ComponentTokens.xaml     # MODIFY: audit, ensure all Component.* tokens present
├── Effects/
│   ├── Shadows.xaml             # AUDIT: verify no duplication with Elevation.xaml
│   ├── Glow.xaml                # AUDIT: verify completeness
│   └── Animations.xaml          # AUDIT: verify all animations ≤200ms
├── Controls/
│   ├── ButtonStyles.xaml       # MODIFY: replace inline colors
│   ├── ComboBoxStyles.xaml     # MODIFY: replace CornerRadius="10"
│   ├── DataGridStyles.xaml     # MODIFY: replace inline colors
│   ├── ScrollBarStyles.xaml    # MODIFY: replace inline hex
│   ├── GroupBoxStyles.xaml     # MODIFY: replace inline hex
│   └── ... (other controls — audit only)
├── Dark/
│   └── DarkColors.xaml          # AUDIT: verify semantic overrides present
├── Light/
│   └── LightColors.xaml         # AUDIT: verify semantic overrides present
├── Custom/
│   └── CustomColors.xaml        # AUDIT: verify accent swatch overrides present
├── ThemeResources.xaml          # MODIFY: update loading order if needed
└── WindowAnimations.xaml       # MODIFY: replace inline hex

WpfApp2/Views/
└── SettingsWindow.xaml          # MODIFY: replace inline GradientStop colors

WpfApp2/Controls/
├── LoadingOverlay.xaml          # MODIFY: replace inline hex background
└── Toast/
    └── ToastWindow.xaml          # MODIFY: replace named color Foreground

WpfApp2/MainWindow.xaml          # MODIFY: replace inline hex (if any remaining)
WpfApp2/Services/
└── ThemeManager.cs              # MODIFY: add token integrity validation (FR-010)
```

**Structure Decision**: Existing Theme/ directory structure is preserved. New files are not needed (Elevation, Motion, ZIndex, Opacity, ComponentTokens already exist). Changes are modifications to existing files and additions to Colors.xaml token definitions.

## Existing Token Inventory (Research Results)

### Already Present (No New Files Needed)

| File | Tokens | Status |
|------|--------|--------|
| `Base/Colors.xaml` | 15 Primitive.*, 25 Color.*, 27 Brush.*, 35 legacy flat keys | **Needs**: deprecation comments on legacy keys, Primitive.Transparency.* tokens for derived states |
| `Base/Typography.xaml` | 9 value keys + 5 Style.Text.* styles | **Complete** — ensure all 5 styles reference DynamicResource |
| `Base/Spacing.xaml` | 6 Spacing.*, 9 Padding.*, 7 size keys | **Needs**: Padding.XSmall addition |
| `Base/Radius.xaml` | 7 CornerRadius keys | **Complete** |
| `Base/Elevation.xaml` | 9 DropShadowEffect keys | **Needs**: relocate effects to Effects/, keep Elevation.xaml as abstraction mapping |
| `Base/Motion.xaml` | 8 duration + 4 easing keys | **Complete** |
| `Base/ZIndex.xaml` | 8 z-index keys | **Complete** |
| `Base/Opacity.xaml` | 6 opacity keys | **Complete** |
| `Base/ComponentTokens.xaml` | 17 component tokens | **Needs**: ensure all reference Brush.*/Spacing.* tokens, add hardcoded value replacements |
| `Effects/Shadows.xaml` | 12 shadow effects (incl. safe variants) | **Needs**: verify no overlap with Elevation.xaml |
| `Effects/Glow.xaml` | 6 glow effects | **Complete** |
| `Effects/Animations.xaml` | 8 storyboards + 2 easings | **Complete** |

### Inline Violations to Fix

| # | Type | File | Detail |
|---|------|------|--------|
| V1 | Inline hex GradientStop | SettingsWindow.xaml:206-258 | 6 GradientStop Color values |
| V2 | Inline hex Background | ScrollBarStyles.xaml:22 | `#55FFFFFF` |
| V3 | Inline hex Background | GroupBoxStyles.xaml:13 | `#14000000` |
| V4 | Inline hex Background | LoadingOverlay.xaml:13 | `#88000000` |
| V5 | Named color Foreground | ToastWindow.xaml:21 | `White` |
| V6 | Inline hex ColorAnimation | WindowAnimations.xaml:82 | `#22FF4757` |
| V7 | Inline hex ColorAnimation | ScrollViewerStyles.xaml:78 | `#66FFFFFF` |
| V8 | DropShadowEffect in Base/ | Elevation.xaml | 9 DropShadowEffect definitions not in Effects/ |