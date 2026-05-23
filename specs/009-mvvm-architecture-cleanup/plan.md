# Implementation Plan: MVVM & Architecture Cleanup

**Branch**: `009-mvvm-architecture-cleanup` | **Date**: 2026-05-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/009-mvvm-architecture-cleanup/spec.md`

## Summary

Clean the add-in's application architecture by introducing a centralized service container for dependency injection, an event bus for decoupled cross-component communication, and a module registry for self-contained feature registration. Refactor existing code to remove business logic from code-behind files, relocate misplaced ViewModels, isolate services, and reduce coupling. Infrastructure is built first (container, event bus, module system), then applied incrementally to existing code. Scale target: 20-100 services, 3-10 modules, 10-50 event types. Observability hooks exposed through the container and event bus for downstream diagnostics.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8)

**Primary Dependencies**: Native WPF (no third-party UI frameworks per Constitution §XIV), existing ThemeManager/ModernWindow infrastructure (unchanged by this phase)

**Storage**: None — service container, event bus, and module registry are purely in-memory. No persistence layer needed.

**Testing**: `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` for build validation, manual Excel VSTO host testing for regression, CodeRabbit review for code quality gates

**Target Platform**: Windows — Excel VSTO Add-in (WPF)

**Project Type**: Desktop Add-in (WPF) — internal architecture cleanup within the existing add-in

**Performance Goals**: Service resolution and event bus combined add ≤50ms to application startup (SC-007), circular dependency detection completes within 1 second (SC-006)

**Constraints**: Excel VSTO rendering safety (Constitution §X) — no rendering changes, DynamicResource-only for all themeable properties (Constitution §III), no third-party UI frameworks (Constitution §XIV), business logic must move out of code-behind (Constitution §II)

**Scale/Scope**: Medium — 20-100 services, 3-10 modules, 10-50 event types, explicit registration at composition root (no auto-discovery until Phase 9)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Service container, event bus, and module registry are runtime services (not ResourceDictionaries). They follow modular separation: each is independently testable and replaceable. No monolithic registries.
- [x] **III. DynamicResource-Only** — Infrastructure changes introduce no themeable UI properties. No StaticResource or inline colors involved.
- [x] **IV. Runtime Theme Mutation Governance** — No theme mutations introduced. Existing ThemeManager usage remains unchanged.
- [x] **IX. Animation Governance** — No new animations introduced. Existing animation constraints unaffected by architecture cleanup.
- [x] **X. Excel Rendering Safety** — No UI rendering changes. Infrastructure is behind-the-scenes (ViewModels, services, DI). No impact on rendering pipeline.
- [x] **XI. WindowChrome Enforcement** — No new windows. No impact on window rendering strategy.
- [x] **XII. Centralized Effects** — No new effects. All existing effects remain centralized in Effects/Shadows.xaml and Effects/Glow.xaml.
- [x] **XV. Resource Loading Order** — No new ResourceDictionaries introduced that affect theme loading order. Infrastructure uses code-based service registration, not XAML dictionary merges.

**Note**: All gates pass. No violations to track in Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/009-mvvm-architecture-cleanup/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 — research and pattern decisions
├── data-model.md        # Phase 1 — entity definitions
├── quickstart.md        # Phase 1 — how to register services and modules
├── contracts/           # Phase 1 — interface contracts
│   ├── IServiceContainer.md
│   ├── IEventBus.md
│   └── IModule.md
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
WpfApp2/
├── Services/
│   ├── ServiceContainer.cs          # NEW — service registration and resolution
│   ├── EventBus.cs                  # NEW — publish/subscribe event system
│   ├── ModuleRegistry.cs            # NEW — module registration and initialization
│   └── ... (existing services, refactored to use container)
├── ViewModels/                       # NEW — centralized ViewModel location
│   └── ... (relocated ViewModels from other directories)
├── ... (existing project files — code-behind refactored in place)
```

**Structure Decision**: Clean WPF project layout with new infrastructure services in the existing `Services/` directory (matching the NavigationService pattern from Phase 5), a new centralized `ViewModels/` directory for relocated ViewModels, and code-behind files refactored in place. No new XAML dictionaries or theme files are introduced.

## Complexity Tracking

*All Constitution gates pass. No complexity violations to justify.*
