# Implementation Plan: Legacy Window Migration

**Branch**: `011-legacy-window-migration` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/011-legacy-window-migration/spec.md`

## Summary

Migrate 14 existing standalone windows to Pages within the unified Shell (built in Phase 8), enabling seamless navigation without window management overhead. Migration is incremental — original XAML preserved until each Page validates in Excel VSTO host. Ribbon launchers updated to open Pages instead of standalone windows. Phase 8 Shell and NavigationService provide the foundation; Phase 11 applies this to existing windows.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8), WPF

**Primary Dependencies**: Phase 8 Shell (`ShellWindow`, `NavigationService`), Phase 6 MVVM infrastructure (`ServiceContainer`, `EventBus`), Phase 3 Theme system (`ThemeManager`, `ModernWindow`), existing Ribbon XML/designer

**Storage**: None — migration is presentation-layer transformation; existing data storage unchanged

**Testing**: Manual Excel VSTO host testing per window, `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` for build validation, CodeRabbit review for code quality gates

**Target Platform**: Windows — Excel VSTO Add-in (WPF)

**Project Type**: Desktop Add-in — window-to-page migration

**Performance Goals**: Shell loads within 1s (SC-002), page navigation transitions ≤200ms (SC-003), zero rendering regressions at 100%/125%/150% DPI (SC-004)

**Constraints**: Excel VSTO rendering safety (Constitution §X), WindowChrome primary (Constitution §XI), DynamicResource-only (Constitution §III), centralized effects only (Constitution §XII), animations ≤200ms (Constitution §IX), original XAML preserved until validation passes

**Scale/Scope**: 14 windows to migrate (MainWindow, SettingsWindow, Float_path, AssignTradeCodesWindow, Fixpiecolors, LinksManagerWindow, StyleSelectorWindow, SubDailyReportWindow, UnmergeFillDownWindow, XerEditorWindow, ProjectAnalysisWindow, PrimaveraCompareWindow, PrimaveraResultsWindow, ToastWindow), 1 Page per window, incremental validation per window

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Migration adds Pages to existing `Pages/` folder; no monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — All migrated Pages use `{DynamicResource}` for backgrounds, borders, foregrounds, effects. No inline colors.
- [x] **IV. Runtime Theme Mutation Governance** — Pages consume theme through DynamicResource reevaluation; no direct brush mutation.
- [x] **IX. Animation Governance** — Page navigation transitions ≤200ms using existing fade/opacity animations from Effects/Animations.xaml.
- [x] **X. Excel Rendering Safety** — Pages inherit from `Page` (not `Window`); shell uses ModernWindow with WindowRenderModeDetector fallback.
- [x] **XI. WindowChrome Enforcement** — Shell container inherits ModernWindow; WindowChrome is primary rendering strategy.
- [x] **XII. Centralized Effects** — No inline DropShadowEffect; effects sourced from Effects/Shadows.xaml and Effects/Glow.xaml.
- [x] **XV. Resource Loading Order** — Migrated Pages follow existing loading order; no new dictionaries added outside theme structure.

**Note**: All gates pass. No violations.

## Project Structure

### Documentation (this feature)

```text
specs/011-legacy-window-migration/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 — window inventory and migration approach research
├── data-model.md        # Phase 1 — MigrationRecord entity and validation schema
├── quickstart.md        # Phase 1 — step-by-step window migration guide
├── contracts/           # Phase 1 — (none needed; reuses Phase 8 INavigationService)
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 — created by /speckit.tasks
```

### Source Code (repository root)

```text
WpfApp2/
├── Controls/Shell/              # Existing Phase 8 — shell container
│   ├── ShellWindow.xaml
│   ├── ShellWindow.xaml.cs
│   └── ...
├── Pages/                       # Existing Phase 8 + Phase 11 additions
│   ├── WelcomePage.xaml         # Existing Phase 8
│   ├── WelcomePage.xaml.cs
│   ├── PageBase.cs              # Existing Phase 8
│   ├── MainPage.xaml            # NEW — migrated from MainWindow.xaml
│   ├── MainPage.xaml.cs
│   ├── SettingsPage.xaml        # NEW — migrated from Views/SettingsWindow.xaml
│   ├── SettingsPage.xaml.cs
│   ├── ProjectAnalysisPage.xaml # NEW — migrated from ProjectAnalysisWindow.xaml
│   ├── ProjectAnalysisPage.xaml.cs
│   └── ... (remaining 11 windows)
├── Services/
│   └── NavigationService.cs     # Existing Phase 8 — reused for page registration
├── Views/                       # Existing — Settings panels, Diagnostics panels
├── Windows/                     # Existing — original standalone XAML (preserved until validation)
│   ├── MainWindow.xaml         # PRESERVED — removed after MainPage validation
│   ├── MainWindow.xaml.cs
│   └── ... (original standalone windows)
└── ... (existing project files)
```

**Structure Decision**: Pages live alongside existing Phase 8 Pages; original standalone XAML remains in `Windows/` (or root) until each migration validates. Ribbon launchers updated to call `NavigationService.Instance.NavigateTo()` instead of `new Window().Show()`.

## Phase 0: Outline & Research

### Research Tasks

1. **Window inventory audit** — Enumerate all 14 windows, their current location (root `WpfApp2/` or `Windows/`), dependencies (services consumed, ViewModels), and complexity rating (simple dialog vs. complex data grid).

2. **Migration priority assessment** — Score each window by: usage frequency (ribbon click count), complexity (controls count), dependency depth (services, shared state), and business criticality. Produce ranked list.

3. **PageBase pattern review** — Examine existing `PageBase.cs` from Phase 8; confirm it provides necessary lifecycle hooks, theming, and MVVM integration.

4. **Ribbon integration mapping** — Identify ribbon XML entries or Ribbon designer code that launches each window; document the change needed (window instantiation → navigation service call).

5. **ViewModel extraction analysis** — For each window, identify existing code-behind logic that must move to ViewModel; confirm MVVM patterns from Phase 6 are available.

6. **Validation checklist design** — Create per-window validation checklist covering: theme consistency, DPI scaling, popup behavior, keyboard navigation, FallbackSafe mode, and functional parity.

## Phase 1: Design & Contracts

### Deliverables

- `data-model.md` — `MigrationRecord` entity with fields: WindowName, OriginalPath, PagePath, MigrationStatus (NotStarted/InProgress/Validated/RolledBack), ValidatedBy, ValidatedDate, Notes
- `quickstart.md` — 6-step migration guide: (1) Create Page class, (2) Extract content XAML, (3) Move code-behind logic to Page code-behind or ViewModel, (4) Register with NavigationService, (5) Update ribbon launcher, (6) Validate in Excel VSTO
- `contracts/` — No new interfaces; reuses `INavigationService` from Phase 8
- Update `AGENTS.md` — Add plan reference between SPECKIT markers

### Workstream Execution Order

```text
Phase 0 research (P11-T001 through P11-T005)
    ↓
Phase 1 design artifacts (data-model.md, quickstart.md)
    ↓
Phase 2 tasks generation via /speckit.tasks
```

## Complexity Tracking

*All Constitution gates pass. No complexity violations to justify.*