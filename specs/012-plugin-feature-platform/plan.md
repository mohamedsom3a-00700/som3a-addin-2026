# Implementation Plan: Plugin & Feature Platform

**Branch**: `012-plugin-feature-platform` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/012-plugin-feature-platform/spec.md`

## Summary

Build a plugin platform enabling internal features to register as modules providing navigation pages, ribbon actions, and commands. Modules are lazily loaded on first access, operate in isolation (faults contained), support a defined lifecycle state machine, and have assembly integrity verification. Diagnostics UI provides module status, version, and resource usage. Builds on Phase 5 (Navigation Shell) and Phase 6 (MVVM Cleanup: ServiceContainer, EventBus, ModuleRegistry).

## Technical Context

**Language/Version**: C#, .NET Framework 4.8 (aligned with existing project)

**Primary Dependencies**: Existing ServiceContainer/EventBus/ModuleRegistry from Phase 6; existing Navigation Shell from Phase 5; existing Excel VSTO ribbon infrastructure

**Storage**: Module assemblies on filesystem under `Modules/` directory; metadata tracked in-memory by ModuleRegistry

**Testing**: MSTest (aligned with existing project convention) [NEEDS CLARIFICATION: confirm test framework if different]

**Target Platform**: Excel VSTO Add-in, Windows (x64)

**Project Type**: Desktop application (WPF) — infrastructure/plumbing extension

**Performance Goals**: Application startup with 5 registered-but-unloaded modules shows ≤5% increase vs baseline; diagnostics view renders module list within 1 second; supports 20 concurrent registered modules without navigation/ribbon degradation

**Constraints**: Must integrate with existing Phase 5 Navigation Shell and Phase 6 DI container/event bus; no external/third-party module sandboxing for v1; modules are internal team-developed assemblies

**Scale/Scope**: Up to 20 concurrently registered modules; each module provides some combination of pages, ribbon actions, and commands

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Plugin platform introduces no monolithic dictionaries; module system itself is modular (loader, registry, contracts, diagnostics as separate concerns).
- [x] **III. DynamicResource-Only** — Plugin platform does not introduce new themeable resources; modules that add UI will inherit existing DynamicResource conventions.
- [x] **IV. Runtime Theme Mutation Governance** — No theme mutation involved in plugin platform.
- [x] **IX. Animation Governance** — Loading spinner is a simple visibility toggle, not an animation subject to 200ms rule.
- [x] **X. Excel Rendering Safety** — Module pages render within the existing Navigation Shell (Phase 5) which already handles Excel safety via WindowRenderModeDetector.
- [x] **XI. WindowChrome Enforcement** — No new windows created by plugin platform; pages render inside existing shell.
- [x] **XII. Centralized Effects** — No effects introduced by plugin platform.
- [x] **XV. Resource Loading Order** — No new resource dictionaries introduced.

**Result**: All gates pass. No constitution violations.

## Project Structure

### Documentation (this feature)

```text
specs/012-plugin-feature-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Contracts/              # Module contract interfaces (NEW)
│   ├── IModule.cs
│   ├── IModuleRegistry.cs
│   ├── IPluginLoader.cs
│   ├── INavigationRegistration.cs
│   ├── IRibbonActionRegistration.cs
│   └── ICommandRegistration.cs
├── Services/               # Existing services directory
│   ├── ModuleRegistry.cs   # NEW — module tracking and lifecycle
│   ├── PluginLoader.cs     # NEW — assembly discovery and loading
│   ├── ModuleDiagnosticsService.cs # NEW — diagnostics data provider
│   └── ... (existing services)
├── Controls/               # Existing controls directory
│   └── ModuleLoadingOverlay.xaml # NEW — inline loading spinner
├── Views/                  # Existing views directory
│   └── (existing windows — no new views needed for platform)
├── Modules/                # NEW — deployed module assemblies directory
└── ... (existing project files)
```

**Structure Decision**: Contracts as interfaces in a `Contracts/` folder for clean separation; services follow existing `Services/` pattern; the `Modules/` directory is the deployment target for module assemblies.

## Complexity Tracking

> N/A — All constitution checks pass without violations.

## Phase 0: Outline & Research

### Research Tasks

1. **Module contract patterns** — Research best practices for .NET module/plugin contract interfaces (MEF, MAF, custom); evaluate against existing Phase 6 ModuleRegistry.
2. **Lazy loading patterns** — Research assembly loading strategies (Assembly.LoadFrom, Assembly.LoadContext, reflection-only) for .NET Framework 4.8; evaluate impact on Excel VSTO hosting.
3. **Assembly integrity validation** — Research hash/checksum approaches for .NET assemblies (SHA256, strong-name validation); determine practical approach for internal-only modules.
4. **Fault isolation in VSTO** — Research AppDomain isolation vs in-process fault containment for modules within Excel VSTO host; evaluate tradeoffs of separate AppDomains for .NET Framework 4.8.
5. **Ribbon registration patterns** — Research how existing VSTO ribbon infrastructure works; determine interface for modules to register ribbon actions.

## Phase 1: Design & Contracts

### Deliverables

- `data-model.md` — Module entity, lifecycle state machine, registry structure, diagnostics model
- `contracts/` — Interface definitions for IModule, IModuleRegistry, IPluginLoader, etc.
- `quickstart.md` — Step-by-step guide for creating a new module
- Update `AGENTS.md` — Add plan reference between SPECKIT markers
