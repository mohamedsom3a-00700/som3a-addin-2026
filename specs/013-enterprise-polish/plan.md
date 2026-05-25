# Implementation Plan: Enterprise Polish

**Branch**: `013-enterprise-polish` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/013-enterprise-polish/spec.md`

## Summary

Production-grade optimization of the entire application — validate, harden, and polish every aspect for enterprise deployment. Seven coordinated workstreams: Performance Hardening (WS-A), Accessibility Compliance (WS-B), DPI & Multi-Monitor Validation (WS-C), Excel Host Stability (WS-D), Technical Debt Cleanup (WS-E), Diagnostics Finalization (WS-F), and Documentation & Audit Deliverables (WS-G). Closes all 15 remaining Phase 3 validation tasks.

## Technical Context

**Language/Version**: C#, .NET Framework 4.8 (aligned with existing project)

**Primary Dependencies**: Existing theme engine, ThemeManager, WindowRenderModeDetector, DiagnosticsService, ValidationEngine, LoggingService, all 13 ModernWindow windows, ToastWindow (to migrate)

**Storage**: Diagnostics logs at `AppData/Som3a/Logs/` (existed); audit reports under `Docs/Architecture/` (new); enterprise polish checklist (new)

**Testing**: Manual Excel VSTO host testing; Windows Accessibility Insights; Windows Performance Recorder; Windows PerfView / .NET Memory Profiler; manual DPI validation

**Target Platform**: Excel VSTO Add-in, Windows (x64)

**Project Type**: Desktop application (WPF) — optimization and validation phase

**Performance Goals**: Startup <1s, DataGrid 10k rows at 60fps, memory growth <20% over 2h, all animations <=200ms, theme switch <1s for all windows

**Constraints**: Must not break existing functionality; all optimizations must preserve or improve Excel VSTO rendering stability; must respect existing DynamicResource-only and centralized-effects rules; no code-behind business logic in new code

**Scale/Scope**: 14 windows, 3 themes (Dark/Light/Custom), 5+ DPI levels (100–200%), multi-monitor mixed DPI, all 7 diagnostic services, all 15 Phase 3 validation tasks

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Enterprise polish introduces no monolithic dictionaries; audit reports are documentation, not resources.
- [x] **III. DynamicResource-Only** — No new themeable resources introduced; existing DynamicResource usage is validated and hardened.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation continues through ThemeManager; polish adds no new mutation paths.
- [x] **IX. Animation Governance** — All animations validated against <=200ms rule; reduced-motion support added where missing.
- [x] **X. Excel Rendering Safety** — Primary focus of WS-D; all window changes validated inside Excel VSTO host.
- [x] **XI. WindowChrome Enforcement** — ToastWindow migration adds the final ModernWindow conformance.
- [x] **XII. Centralized Effects** — Hardcoded-value sweep enforces zero inline effects; ValidationEngine detects violations.
- [x] **XV. Resource Loading Order** — ToastWindow migration preserves the prescribed loading sequence.

**Result**: All gates pass. No constitution violations.

## Project Structure

### Documentation (this feature)

```text
specs/013-enterprise-polish/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
├── checklists/          # Validation checklists
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Controls/
│   └── ToastWindow.xaml          # MODIFIED — migration from plain <Window>
├── Services/
│   ├── ValidationEngine.cs       # MODIFIED — resolve TODO, extend scan
│   ├── DiagnosticsService.cs     # VALIDATED — snapshot coverage
│   ├── LoggingService.cs         # VALIDATED — rotation, path
│   └── ... (existing services)
├── Theme/
│   └── ... (existing — no new files, but validated for Freeze(), hardcoded values)
├── Docs/Architecture/
│   ├── PERFORMANCE_AUDIT_REPORT.md       # NEW
│   ├── ACCESSIBILITY_AUDIT_REPORT.md     # NEW
│   ├── DPI_AUDIT_REPORT.md               # NEW
│   ├── EXCEL_STABILITY_REPORT.md         # NEW
│   ├── LOCALIZATION_READINESS.md         # NEW
│   └── ENTERPRISE_POLISH_CHECKLIST.md    # NEW
└── AGENTS.md                      # MODIFIED — Phase 10 references
```

**Structure Decision**: No new services or controls introduced — this phase modifies, validates, and hardens existing code. Audit reports are documentation artifacts under `Docs/Architecture/`. ToastWindow migration is the only source-level structural change.

## Complexity Tracking

> N/A — All constitution checks pass without violations. ToastWindow migration and Freeze() optimization follow existing patterns.

## Phase 0: Outline & Research

### Research Tasks

1. **Performance baseline** — Establish startup time, memory usage, DataGrid scroll fps, theme switch time, animation timings on reference enterprise PC (i5, 16GB, SSD).
2. **Accessibility baseline** — Run Windows Accessibility Insights on all 14 windows to catalog keyboard navigation gaps, missing AutomationProperties, contrast violations.
3. **DPI baseline** — Test all 14 windows at 100%, 125%, 150%, 200% DPI. Test multi-monitor mixed DPI if hardware available.
4. **Excel VSTO baseline** — Execute Phase 3 T057–T062 validation; document current failures.
5. **Hardcoded-value sweep** — Run regex scan across all `.xaml` files for `#HEX` colors, inline effects, hardcoded margins; catalog findings.
6. **ValidationEngine review** — Understand current TODO and design the proper XAML/dictionary resolver.
7. **ToastWindow analysis** — Review current ToastWindow.xaml to plan migration to ModernWindow.
8. **Freezable audit** — Search for all `SolidColorBrush`, `LinearGradientBrush`, `Pen`, `Transform` resources that can be frozen.

## Phase 1: Design & Contracts

### Deliverables

- `data-model.md` — Performance metrics schema, accessibility violation catalog format, DPI test matrix, audit report templates
- `contracts/` — Interfaces for audit report generation (if new abstraction needed); ValidationEngine extension API
- `quickstart.md` — Step-by-step guide for running Phase 10 validation tasks
- Update `AGENTS.md` — Add Phase 10 paths between SPECKIT markers

## Workstream Execution Order

```text
WS-E (Tech Debt Cleanup) starts immediately (ToastWindow, hardcoded sweep)
    ↓
WS-A (Perf) + WS-B (Accessibility) + WS-C (DPI) in parallel after baselines
    ↓
WS-D (Excel Stability) after WS-E cleanup
    ↓
WS-F (Diagnostics Finalization) after WS-D
    ↓
WS-G (Documentation) final — depends on all audit results
```
