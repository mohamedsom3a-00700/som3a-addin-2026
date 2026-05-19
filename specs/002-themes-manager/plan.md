# Implementation Plan: Update Themes Manager

**Feature ID**: 002-themes-manager | **Branch**: `002-fluent-theme-engine` | **Date**: 2026-05-19

**Input**: [Feature specification](spec.md) | [Research](research.md)

**Prerequisite**: [001-fluent-theme-engine](../001-fluent-theme-engine/plan.md) — existing theme infrastructure

---

## Summary

Fix bugs in `ThemeManager.cs`, eliminate ~78 hardcoded color violations across 12 windows, resolve 3 P0 runtime crash risks, and remove dead/duplicate theme resources — making the theme engine production-ready without a full re-write.

---

## Technical Context

**Language/Version**: C# / .NET Framework 4.8

**Primary Dependencies**: Native WPF only (Constitution §XIV prohibits third-party UI frameworks)

**Storage**: `Properties.Settings.settings` (ApplicationSettingsBase) for theme persistence; legacy JSON at `AppData/Som3a/theme.json` retained but not modified

**Testing**: Manual verification via `msbuild` + runtime checks in Excel VSTO host

**Target Platform**: Excel VSTO Add-in (Windows desktop)

**Project Type**: Desktop WPF library (VSTO)

**Performance Goals**: Theme switch <200ms, accent change <200ms (SC-005)

**Constraints**: Animations ≤200ms (Constitution §IX); Excel rendering safety (Constitution §X); no inline DropShadowEffect (Constitution §XII); DynamicResource-only for themeable properties (Constitution §III)

**Scale/Scope**: 12 windows, ~78 hardcoded color violations, 3 P0 crash risks

---

## Constitution Check

*GATE: Passed. Re-check after design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), this plan verifies:

- [x] **I. Library-First Modular Architecture** — All changes remain within existing modular dictionary structure (Base, Dark, Light, Custom, Effects, Controls). No monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — All new and migrated window properties use `{DynamicResource}`. No StaticResource for themeable items.
- [x] **IV. Runtime Theme Mutation Governance** — All mutations route through `ThemeManager.ApplyTheme()` and `ThemeManager.ApplyAccentColor()`. No direct brush mutation from windows or controls.
- [x] **IX. Animation Governance** — No new animations introduced. Existing animations already ≤200ms.
- [x] **X. Excel Rendering Safety** — Window background changes are static `DynamicResource` swaps; they do not interfere with `WindowRenderModeDetector`. FallbackSafe mode remains respected.
- [x] **XI. WindowChrome Enforcement** — All 12 existing windows already inherit from `ModernWindow`. No new windows introduced.
- [x] **XII. Centralized Effects** — Inline DropShadowEffect instances are replaced with centralized `Effects/Shadows.xaml` references. `FluentEffects.xaml` (dead file) is removed entirely.
- [x] **XV. Resource Loading Order** — New tokens added to `Theme/Base/Colors.xaml` and theme overrides (`DarkColors.xaml`, `LightColors.xaml`, `CustomColors.xaml`). `ThemeResources.xaml` aggregator already loads dictionaries in the correct sequence.

**Re-evaluated after design**: All checks still pass.

---

## Project Structure

### Documentation (this feature)

```text
specs/002-themes-manager/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── spec.md              # Feature specification
```

### Source Code (repository root)

```text
WpfApp2/
├── Services/
│   └── ThemeManager.cs              # Singleton service fixes
├── Theme/
│   ├── Base/
│   │   └── Colors.xaml              # New semantic tokens (Brush.*)
│   ├── Dark/
│   │   └── DarkColors.xaml          # Dark overrides for new tokens
│   ├── Light/
│   │   └── LightColors.xaml         # Light overrides for new tokens
│   ├── Custom/
│   │   └── CustomColors.xaml        # Custom overrides for new tokens
│   ├── Effects/
│   │   ├── Shadows.xaml             # Add ProgressGlow
│   │   └── Glow.xaml                # Already dynamic (no changes)
│   ├── Controls/
│   │   └── ... (existing styles)
│   ├── Fluent/
│   │   ├── FluentEffects.xaml       # REMOVE
│   │   └── FluentWhite.xaml         # REMOVE
│   ├── ModernWindow.xaml            # Fix hardcoded White/Black + inline shadow
│   ├── WindowAnimations.xaml        # Fix hardcoded close-button red
│   └── ThemeResources.xaml          # Remove FluentEffects reference
├── App.xaml                          # Remove duplicate converters + dead comments
├── Views/
│   └── SettingsWindow.xaml          # Remove duplicate dict loads, fix Foreground
├── [12 window .xaml files]          # Replace hardcoded colors with DynamicResource
│   ├── Fixpiecolors.xaml
│   ├── Float_path.xaml              # TreeView hover/selected colors
│   ├── LinksManagerWindow.xaml      # Foreground="White" on progress
│   ├── SubDailyReportWindow.xaml    # LimeGreen + inline shadow
│   ├── UnmergeFillDownWindow.xaml   # Foreground="White" on progress
│   ├── StyleSelectorWindow.xaml
│   ├── XerEditorWindow.xaml
│   ├── AssignTradeCodesWindow.xaml
│   ├── ProjectAnalysisWindow.xaml   # Inline shadow + Foreground="White"
│   ├── PrimaveraCompareWindow.xaml
│   ├── PrimaveraResultsWindow.xaml  # Missing Foreground
│   └── SettingsWindow.xaml
└── Som3a_WPF_UI.csproj              # Remove FluentWhite.xaml + FluentEffects.xaml references
```

---

## Complexity Tracking

No constitutional violations. No complexity justifications required.

---

## Phase 1 — ThemeManager Core Fixes

**Goal**: Fix all bugs in the singleton and make dictionary replacement reliable.

### TASK-1001 — Fix accent persistence on theme switch
- **File**: `Services/ThemeManager.cs`
- **Issue**: The linked plan incorrectly described resetting `_currentAccentColor` to the theme default as a bug fix. The spec requires accent persistence.
- **Fix**: Ensure `ApplyTheme()` preserves `_currentAccentColor` when `accentColor` is null. Do NOT read any default accent from the new theme dictionary.

### TASK-1002 — Fix early-return logic
- **File**: `Services/ThemeManager.cs`
- **Issue**: Early return checks `theme == _currentTheme && string.IsNullOrEmpty(accentColor)`, but `_currentTheme` is updated AFTER this check. Second call with same theme proceeds unnecessarily.
- **Fix**: Reorder — save `_currentTheme` immediately, then use a proper guard.

### TASK-1003 — Implement dictionary replacement with safe fallback
- **File**: `Services/ThemeManager.cs`
- **Current**: Creates `ResourceDictionary` with `pack://` URI and immediately adds to `Application.Current.Resources.MergedDictionaries`
- **Fix**: Wrap removal + addition in try/catch. If the requested new theme fails to load, preserve the currently-active theme dictionary. Only fallback to Dark theme if the current dictionary itself is invalidated. Log errors.

### TASK-1004 — Add `ThemeChanged` event thread safety
- **File**: `Services/ThemeManager.cs`
- **Issue**: `ThemeChanged` event is fired on the calling thread (may be background).
- **Fix**: Use `System.Windows.Application.Current.Dispatcher.InvokeAsync()` for event dispatch.

### TASK-1005 — Add theme switch debouncing
- **File**: `Services/ThemeManager.cs`
- **Issue**: Rapid sequential clicks on different theme cards can cause transient visual states and overlapping dictionary operations.
- **Fix**: Debounce/coalesce theme change requests with a ≈150ms window so only the final selection is applied.

---

## Phase 2 — XAML Architecture Cleanup

**Goal**: Eliminate duplicate resource definitions, orphaned files, and loading inconsistencies.

### TASK-2001 — Remove duplicate converters from App.xaml
- **Files**: `App.xaml` (lines 35-42), `Theme/ThemeResources.xaml` (lines 66-76)
- **Action**: Delete 7 converter definitions from `App.xaml` (keep in `ThemeResources.xaml` which is loaded per-window). Remove the `<ResourceDictionary.MergedDictionaries>` wrapper from `App.xaml` if it becomes empty.

### TASK-2002 — Remove duplicate dictionary loads from SettingsWindow
- **File**: `Views/SettingsWindow.xaml` (lines 19-25)
- **Action**: Remove `Shadows.xaml`, `Glow.xaml`, `ThemeCardStyles.xaml`, `AccentSwatchStyles.xaml` from SettingsWindow's `MergedDictionaries` — these are already inside `ThemeResources.xaml`.

### TASK-2003 — Remove orphaned FluentWhite.xaml
- **File**: `Som3a_WPF_UI.csproj` (line 332-335)
- **Action**: Remove `<Page Include="Theme\Fluent\FluentWhite.xaml" />` from `csproj`.

### TASK-2004 — Remove orphaned FluentEffects.xaml
- **File**: `Theme/Fluent/FluentEffects.xaml`
- **Action**: Delete `FluentEffects.xaml` entirely. Remove its `<ResourceDictionary Source="..."/>` entries from `App.xaml` and `ThemeResources.xaml`.
- **Rationale**: Its effect keys (`FocusGlow`, `ButtonHoverGlow`, `PrimaryButtonGlow`, `CardShadow`, `WindowShadow`) are never referenced by any control. All active effects already live in centralized `Effects/Shadows.xaml` and `Effects/Glow.xaml`.

### TASK-2005 — Remove commented-out legacy theme imports from App.xaml
- **File**: `App.xaml` (lines 28-31)
- **Action**: Delete dead commented lines referencing `DarkTheme.xaml` and `ModernDashboard.xaml`.

---

## Phase 3 — Window Migration (Hardcoded Colors → DynamicResource)

**Goal**: Eliminate all ~78 hardcoded color violations across 12 window files.

**Token Convention**: All newly introduced tokens MUST use the semantic `Brush.*` namespace (e.g., `Brush.Background.Root`, `Brush.Stroke.Info`). Legacy flat keys are retained for backward compatibility but MUST NOT be used for new work (FR-000).

### TASK-3001 — Extract root background gradient → per-theme token
- **Files**: All 12 windows (`Fixpiecolors.xaml`, `Float_path.xaml`, `LinksManagerWindow.xaml`, `SubDailyReportWindow.xaml`, `UnmergeFillDownWindow.xaml`, `StyleSelectorWindow.xaml`, `XerEditorWindow.xaml`, `AssignTradeCodesWindow.xaml`, `ProjectAnalysisWindow.xaml`, `PrimaveraCompareWindow.xaml`, `PrimaveraResultsWindow.xaml`, `SettingsWindow.xaml`)
- **Action**: Define `Brush.Background.Root` in `DarkColors.xaml` (navy-teal gradient), `LightColors.xaml` (light gradient), `CustomColors.xaml` (navy-teal gradient). Replace inline `LinearGradientBrush` in every window with `<Border Background="{DynamicResource Brush.Background.Root}"/>`.

### TASK-3002 — Extract progress bar gradient → per-theme token
- **Files**: `LinksManagerWindow.xaml`, `SubDailyReportWindow.xaml`, `UnmergeFillDownWindow.xaml`, `AssignTradeCodesWindow.xaml`, `ProjectAnalysisWindow.xaml`
- **Action**: Define `Brush.Accent.ProgressFill` in `DarkColors.xaml` / `LightColors.xaml` / `CustomColors.xaml`. Replace inline gradient with `{DynamicResource Brush.Accent.ProgressFill}`.

### TASK-3003 — Fix `Foreground="White"` on progress percent
- **Files**: `LinksManagerWindow.xaml:521`, `SubDailyReportWindow.xaml:556`, `UnmergeFillDownWindow.xaml:329`, `ProjectAnalysisWindow.xaml:543`
- **Action**: Replace `Foreground="White"` with `Foreground="{DynamicResource TextOnAccentBrush}"`.

### TASK-3004 — Fix missing Foreground on TextBlocks
- **Files**: `PrimaveraResultsWindow.xaml:122`, `SettingsWindow.xaml:308`
- **Action**: Add `Foreground="{DynamicResource TextMainBrush}"`.

### TASK-3005 — Replace inline DropShadowEffect on progress bars
- **Files**: `SubDailyReportWindow.xaml:527-531`, `ProjectAnalysisWindow.xaml:512-516`
- **Action**: Define `ProgressGlow` DropShadowEffect in `Effects/Shadows.xaml` with `Color="{DynamicResource AccentColorValue}"`. Replace inline effect with `<Border.Effect><StaticResource ResourceKey="ProgressGlow"/></Border.Effect>`.

### TASK-3006 — Fix ModernWindow.xaml hardcoded Background/Foreground
- **File**: `Theme/ModernWindow.xaml:8-9`
- **Action**: Replace `Background="White"` → `Background="{DynamicResource BackgroundBrush}"`, `Foreground="Black"` → `Foreground="{DynamicResource TextMainBrush}"`. Replace inline DropShadowEffect on WindowBorder (line 27) with `Effect="{DynamicResource Shadow.Window}"`.

### TASK-3007 — Fix TreeView hover/selected colors
- **File**: `Float_path.xaml:411-422`
- **Action**: Replace `#22007ACC` and `#44007ACC` with `{DynamicResource AccentBrush}` at reduced opacity, or define `Brush.Control.HoverBackground` / `Brush.Control.SelectedBackground` as theme resources.

### TASK-3008 — Fix named color + close button red
- **Files**: `SubDailyReportWindow.xaml:209` (`Foreground="LimeGreen"`), `Theme/WindowAnimations.xaml:82` (`To="#22FF4757"`)
- **Action**: Replace `LimeGreen` with `{DynamicResource SuccessBrush}`. Replace close button hover background with `{DynamicResource DangerBrush}` at 13% opacity.

### TASK-3009 — Fix info/status border colors
- **Files**: All windows with `#12FFFFFF`, `#18FFFFFF`, `#1FFFFFFF`, `#22FFFFFF` border fills/strokes
- **Action**: Create `Brush.Stroke.Info`, `Brush.Stroke.Status`, `Brush.Fill.Info`, `Brush.Fill.Status` as per-theme resources. Apply via DynamicResource.

---

## Phase 4 — Verification

**Goal**: Validate the build compiles and theme switching works across themes.

### TASK-4001 — Build
```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

### TASK-4002 — Runtime checks
- Launch in Excel VSTO host
- Verify Dark → Light → Custom switching updates ALL window backgrounds
- Verify accent color change updates glow effects
- Verify no XAML parse exceptions on SettingsWindow open

### TASK-4003 — Regression
- Verify progress bar displays in all 7 windows
- Verify TreeView hover works in Float_path
- Verify close button hover color is visible

---

## Migration Rules

- **DynamicResource only** for all themeable properties
- **No hardcoded #HEX** in any window `.xaml`
- **No inline DropShadowEffect** — use Effects/Shadows.xaml
- **No duplicate resource loading** — each resource key defined once
- **All new brush tokens use `Brush.*` semantic namespace** (e.g., `Brush.Background.Root`, `Brush.Stroke.Info`)
- **Legacy flat keys retained for backward compatibility** but not used for new work

## Files Modified

| File | Changes |
|------|---------|
| `Services/ThemeManager.cs` | Fix 4 bugs (accent persistence, early return, safe fallback, thread-safe event, debounce) |
| `Theme/Base/Colors.xaml` | Add new semantic tokens (Brush.Background.Root, Brush.Accent.ProgressFill, Brush.Stroke.Info, etc.) |
| `Theme/Dark/DarkColors.xaml` | Dark overrides for new tokens |
| `Theme/Light/LightColors.xaml` | Light overrides for new tokens |
| `Theme/Custom/CustomColors.xaml` | Custom overrides for new tokens |
| `Theme/Effects/Shadows.xaml` | Add ProgressGlow DropShadowEffect |
| `Theme/Fluent/FluentEffects.xaml` | **REMOVE** (orphaned dead file) |
| `Theme/ModernWindow.xaml` | Fix hardcoded White/Black + inline shadow |
| `Theme/WindowAnimations.xaml` | Fix close button hardcoded red |
| `App.xaml` | Remove duplicate converters + dead comments + FluentEffects reference |
| `Views/SettingsWindow.xaml` | Remove duplicate dictionary loads, fix missing Foreground |
| `Som3a_WPF_UI.csproj` | Remove FluentWhite.xaml + FluentEffects.xaml references |
| All 12 window `.xaml` files | Replace hardcoded colors with DynamicResource |
| `Float_path.xaml` | TreeView hover/selected colors |
| `SubDailyReportWindow.xaml` | Inline shadow + LimeGreen |
| `ProjectAnalysisWindow.xaml` | Inline shadow |
| `PrimaveraResultsWindow.xaml` | Missing Foreground |
| `LinksManagerWindow.xaml` | Foreground="White" |
| `UnmergeFillDownWindow.xaml` | Foreground="White" |
