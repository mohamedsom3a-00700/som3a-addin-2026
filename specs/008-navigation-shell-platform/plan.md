# Implementation Plan: Navigation Shell Platform

**Branch**: `008-navigation-shell-platform` | **Date**: 2026-05-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/008-navigation-shell-platform/spec.md`

## Summary

Transform the Excel VSTO add-in into a unified workspace shell with sidebar navigation, page-based workspace hosting, command palette, and ribbon integration. The shell hosts new feature windows as pages inside a ModernWindow-based container. Existing standalone windows remain unchanged until Phase 11. Welcome/dashboard page shown on first open; error states shown inline on page load failure. Sidebar supports full keyboard navigation (Tab, arrow keys, Enter/Space, Home/End) and scales to up to 25 destinations.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8)

**Primary Dependencies**: Native WPF (no third-party UI frameworks per Constitution §XIV), existing Theme system (ThemeManager, ModernWindow)

**Storage**: None — ShellState is session-only (in-memory). No persistence layer needed.

**Testing**: Manual Excel VSTO host testing, `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` for build validation, CodeRabbit review for code quality gates

**Target Platform**: Windows — Excel VSTO Add-in (WPF)

**Project Type**: Desktop Add-in (WPF) — Shell is a container window within the add-in

**Performance Goals**: Page transitions ≤200ms (Constitution §IX), shell opens within 1.5s of ribbon click (SC-003), command palette responds within 500ms (SC-004), sidebar scrolls responsively with 25 items (SC-008)

**Constraints**: Excel VSTO rendering safety (Constitution §X), WindowChrome primary rendering (Constitution §XI), DynamicResource-only for themeable properties (Constitution §III), centralized effects only (Constitution §XII), navigation animations must be GPU-safe fade/opacity transitions (Constitution §IX), no inline DropShadowEffect

**Scale/Scope**: Up to 25 registered navigation destinations (sidebar), new windows only (existing windows standalone until Phase 11), session-only workspace restore

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Shell introduces no monolithic dictionaries; ShellResources.xaml (if created) follows modular pattern with pointers to existing theme resources.
- [x] **III. DynamicResource-Only** — Shell window, sidebar, and pages use {DynamicResource} exclusively for backgrounds, borders, foregrounds, and effects. No StaticResource or inline colors.
- [x] **IV. Runtime Theme Mutation Governance** — Shell subscribes to ThemeManager.ThemeChanged and refreshes page chrome/workspace styling through DynamicResource reevaluation. No direct brush mutation.
- [x] **IX. Animation Governance** — Sidebar transition animations, page entrance/exit fade animations ≤200ms. No layout thrashing. GPU-safe opacity transitions only.
- [x] **X. Excel Rendering Safety** — Shell container uses ModernWindow (already Excel-safe). WindowRenderModeDetector activates fallback mode. No AllowsTransparency issues.
- [x] **XI. WindowChrome Enforcement** — Shell inherits from ModernWindow. WindowChrome is the primary rendering strategy.
- [x] **XII. Centralized Effects** — No inline DropShadowEffect. Shadow.Popup and other effects sourced from Effects/Shadows.xaml and Effects/Glow.xaml.
- [x] **XV. Resource Loading Order** — Any new ShellResources.xaml dictionary added after existing control styles and before theme overrides in the loading order.

**Note**: All gates pass. No violations to track in Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/008-navigation-shell-platform/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 — navigation patterns research
├── data-model.md        # Phase 1 — entity definitions
├── quickstart.md        # Phase 1 — how to add a new page
├── contracts/           # Phase 1 — interface contracts
│   ├── INavigationService.md
│   ├── IPageHost.md
│   └── ISidebarModel.md
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
WpfApp2/
├── Controls/
│   ├── ModernWindow.cs              # Existing — extended with shell support
│   └── Shell/                        # NEW — shell components
│       ├── ShellWindow.xaml          # Shell container (sidebar + workspace + statusbar)
│       ├── ShellWindow.xaml.cs
│       ├── SidebarControl.xaml       # Navigation sidebar
│       ├── SidebarControl.xaml.cs
│       ├── WorkspaceHost.cs          # Frame/ContentControl for page hosting
│       ├── CommandPalette.xaml       # Quick navigation overlay
│       └── CommandPalette.xaml.cs
├── Services/
│   ├── NavigationService.cs          # NEW — page registration + navigation logic
│   ├── NavigationService.cs
│   └── ... (existing services)
├── Pages/                            # NEW — shell-hosted pages
│   ├── WelcomePage.xaml              # Default welcome/dashboard page
│   ├── WelcomePage.xaml.cs
│   └── PageBase.cs                   # Base class for shell pages
├── Theme/
│   └── ShellStyles.xaml              # NEW — sidebar, command palette, status bar styles
└── ... (existing project files)
```

**Structure Decision**: Standard WPF project layout extended with a `Shell/` subfolder under `Controls/`, a new `Pages/` directory for shell-hosted pages, and a new `NavigationService` in the existing `Services/` directory. Sidebar styles are added to `Theme/ShellStyles.xaml` following the existing pattern (`Theme/Controls/`).

## Complexity Tracking

*All Constitution gates pass. No complexity violations to justify.*
