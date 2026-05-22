# Implementation Plan: Rendering & Window Infrastructure

**Branch**: `005-rendering-infrastructure` | **Date**: 2026-05-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-rendering-infrastructure/spec.md`

## Summary

Phase 2 creates an Excel-safe rendering architecture by extending ModernWindow with render mode, safe mode, backdrop, and DPI dependency properties; introducing RenderModeService for GPU/transparency detection with FallbackSafe defaults; adding safe-mode shadow variants; refactoring all 14 windows for safe rendering; extending DPI infrastructure; adding accessibility prep; and standardizing window animations with safe-mode gating.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8, WPF XAML

**Primary Dependencies**: WPF (System.Xaml, PresentationFramework), existing ModernWindow.cs, WindowRenderModeDetector.cs, ThemeManager.cs

**Storage**: XAML ResourceDictionary files for shadow/effect definitions; no new persistence needed beyond existing Properties/Settings

**Testing**: msbuild compile verification + grep-based lint scripts + manual Excel VSTO runtime testing per EXCEL_TEST_CHECKLIST.md

**Target Platform**: Windows Desktop (Excel VSTO Add-in host)

**Project Type**: Desktop application (WPF add-in)

**Performance Goals**: Window open animations ≤200ms, close ≤150ms, safe mode skips animations entirely, render mode detection at startup with no runtime overhead

**Constraints**: No third-party UI frameworks (Constitution XIV), DynamicResource-only for themeable properties (Constitution III), no inline effects (Constitution XII), WindowChrome primary (Constitution XI), all effects centralized (Constitution XII), render mode determined once at startup

**Scale/Scope**: 14 window files to audit, 4 new ModernWindow DPs, 3 safe-mode shadow variants, 1 new service (RenderModeService), 1 extended helper (DpiHelper), 1 new checklist (EXCEL_TEST_CHECKLIST.md), accessibility annotations across all interactive controls

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Feature introduces RenderModeService as a modular service; safe shadow variants extend existing Shadows.xaml; no monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — Feature replaces any remaining inline backgrounds/borders in windows with `{DynamicResource Brush.*}` tokens; FR-006 mandates DynamicResource for all window backgrounds and borders.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation remains through ThemeManager; FR-005 (FallbackSafe mode) is a render mode decision, not a theme mutation.
- [x] **IX. Animation Governance** — FR-009 mandates ≤200ms open, ≤150ms close animations using Motion.* tokens; safe mode skips all animations.
- [x] **X. Excel Rendering Safety** — Feature IS the Excel rendering safety infrastructure. FR-004, FR-005, FR-006 address all safety requirements.
- [x] **XI. WindowChrome Enforcement** — FR-002 and FR-006 require all windows to inherit ModernWindow; WindowChrome remains primary rendering strategy per FR-004.
- [x] **XII. Centralized Effects** — FR-003 adds safe shadow variants to centralized Shadows.xaml; zero inline DropShadowEffect outside Effects/.
- [x] **XV. Resource Loading Order** — Safe shadow variants are added to existing Effects/Shadows.xaml; no new ResourceDictionary loading order changes needed.

**No violations** — all constitutional requirements are satisfied by the feature specification.

## Project Structure

### Documentation (this feature)

```text
specs/005-rendering-infrastructure/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── contracts/           # Phase 1 output
    └── render-service.md  # RenderModeService contract
```

### Source Code (repository root)

```text
WpfApp2/Controls/
└── ModernWindow.cs              # MODIFY: add RenderMode, IsSafeMode, WindowBackdrop, DpiScale DPs

WpfApp2/Services/
├── RenderModeService.cs         # CREATE: wrapping WindowRenderModeDetector
├── WindowRenderModeDetector.cs  # EXTEND: ensure GPU/transparency detection integration
└── ThemeManager.cs              # AUDIT: ensure no regression with render mode

WpfApp2/Helpers/
└── DpiHelper.cs                 # CREATE/EXTEND: GetCurrentDpiScale, ScaleValue, IsHighDpi

WpfApp2/Theme/Effects/
└── Shadows.xaml                 # MODIFY: add Shadow.Window.Safe, Shadow.Card.Safe, Shadow.Popup.Safe

WpfApp2/Theme/WindowAnimations.xaml  # MODIFY: ensure ≤200ms/≤150ms, safe-mode skip

WpfApp2/                                 # MODIFY each: apply ModernWindow + safe-mode settings
├── MainWindow.xaml                     # Replace inline backgrounds/borders
├── Float_path.xaml                     # Add SnapsToDevicePixels, UseLayoutRounding
├── AssignTradeCodesWindow.xaml         # Ensure WindowStyle="None" + ModernWindow chrome
├── Fixpiecolors.xaml
├── LinksManagerWindow.xaml
├── StyleSelectorWindow.xaml
├── SubDailyReportWindow.xaml
├── UnmergeFillDownWindow.xaml
├── XerEditorWindow.xaml
├── ProjectAnalysisWindow.xaml
├── Views/SettingsWindow.xaml
└── Windows/PrimaveraComparison/
    ├── PrimaveraCompareWindow.xaml
    ├── PrimaveraResultsWindow.xaml
    └── Toast/ToastWindow.xaml

Docs/Architecture/
└── EXCEL_TEST_CHECKLIST.md     # CREATE: manual test checklist
```

**Structure Decision**: Existing WpfApp2 directory structure is preserved. One new service file (RenderModeService.cs), one new/extended helper (DpiHelper.cs), and one new checklist document are created. All other changes are modifications to existing files.

## Complexity Tracking

> No constitution violations to justify. All requirements are satisfied within the existing architecture boundaries.
