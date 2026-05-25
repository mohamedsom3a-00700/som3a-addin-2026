# Implementation Plan: Enterprise Polish

**Branch**: `013-enterprise-polish` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/013-enterprise-polish/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

Production-grade optimization of the Som3a Add-in across 7 workstreams: Performance Hardening (WS-A), Accessibility Compliance (WS-B), DPI & Multi-Monitor Validation (WS-C), Excel Host Stability (WS-D), Technical Debt Cleanup (WS-E), Diagnostics Finalization (WS-F), and Documentation & Audit Deliverables (WS-G). Closes all 15 remaining Phase 3 validation tasks. Phase 4 (Control Standardization) is a hard prerequisite — no control standardization work is included in Phase 10 scope.

## Technical Context

**Language/Version**: C#, .NET Framework 4.8, WPF — aligned with existing project

**Primary Dependencies**: Existing Theme Engine, ThemeManager (singleton), WindowRenderModeDetector, DiagnosticsService (IDiagnosticsService), ValidationEngine (IValidationEngine), LoggingService (ILoggingService), all 13 ModernWindow-derived windows, ToastWindow (to migrate to ModernWindow)

**Storage**: Diagnostics logs at `AppData/Som3a/Logs/` (existing 5MB rollover, 3-file rotation); audit reports under `Docs/Architecture/` (new)

**Testing**: Manual Excel VSTO host testing across all 14 windows; Windows Accessibility Insights for accessibility audit; Windows Performance Recorder (WPR) / PerfView for performance baselines; manual DPI validation at 100%, 125%, 150%, 200%; .NET Memory Profiler (dotMemory or Visual Studio Diagnostic Tools) for memory analysis

**Target Platform**: Excel VSTO Add-in, Windows (x64), .NET Framework 4.8

**Project Type**: Desktop application (WPF) — optimization and validation phase

**Performance Goals**: All windows open within 1 second on standard enterprise PC (Intel i5, 16GB RAM, SSD); DataGrid with 10,000 rows scrolls at 60fps without stutter; memory growth <20% over 2-hour session; all animations <=200ms; theme switch completes within 1 second across all windows

**Constraints**: Must not break existing functionality; all optimizations must preserve or improve Excel VSTO rendering stability; must respect existing DynamicResource-only, centralized-effects, and no-code-behind-business-logic rules; ToastWindow migration is the only source-level structural change

**Scale/Scope**: 14 windows, 3 themes (Dark/Light/Custom), 5 DPI levels (100%–200%), multi-monitor mixed-DPI configurations, 7 diagnostic services, 15 Phase 3 validation tasks to close, 6 audit reports to produce

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Enterprise polish introduces no monolithic dictionaries; audit reports are documentation, not resources. All existing modular dictionary structure is preserved and validated.
- [x] **III. DynamicResource-Only** — No new themeable resources introduced; existing DynamicResource usage is validated via the hardcoded-value sweep and ValidationEngine extension. No StaticResource violations introduced.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation continues through ThemeManager exclusively. Polish adds no new mutation paths. Existing ThemeManager is validated for thread safety and debounce behavior.
- [x] **IX. Animation Governance** — All animations validated against <=200ms rule. Reduced-motion support added where missing. Animation budget compliance verified during performance audit.
- [x] **X. Excel Rendering Safety** — Primary focus of WS-D workstream. All window changes validated inside Excel VSTO host. Safe mode activation/deactivation tested. All 15 Phase 3 validation tasks closed.
- [x] **XI. WindowChrome Enforcement** — ToastWindow migration is the final ModernWindow conformance step. All 14 windows now inherit from ModernWindow. Fallback rendering path preserved.
- [x] **XII. Centralized Effects** — Hardcoded-value sweep enforces zero inline effects. ValidationEngine extended to detect violations across ALL .xaml files. All effects must originate from Effects/Shadows.xaml, Effects/Glow.xaml, or Effects/Animations.xaml.
- [x] **XV. Resource Loading Order** — ToastWindow migration and any new resource additions preserve the prescribed loading order in ThemeResources.xaml. No new resource dictionaries introduced outside the established order.

**Result**: All gates pass. No constitution violations.

## Project Structure

### Documentation (this feature)

```text
specs/013-enterprise-polish/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── spec.md              # Feature specification
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (empty — no new interfaces needed)
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Controls/
│   └── ToastWindow.xaml          # MODIFIED — migration from plain <Window> to controls:ModernWindow
├── Services/
│   ├── ValidationEngine.cs       # MODIFIED — resolve TODO, extend scan to all .xaml
│   ├── DiagnosticsService.cs     # VALIDATED — snapshot coverage across all windows/modes
│   ├── LoggingService.cs         # VALIDATED — rotation, path, rollover behavior
│   └── ... (existing services unchanged)
├── Theme/
│   ├── Base/                     # VALIDATED — Freeze() applied to all freezable resources
│   ├── Controls/                 # VALIDATED — hardcoded-value sweep, keyboard navigation
│   ├── Effects/                  # VALIDATED — animation budget compliance, reduced-motion
│   └── ... (existing theme files, no new additions)
└── Docs/Architecture/
    ├── PERFORMANCE_AUDIT_REPORT.md       # NEW
    ├── ACCESSIBILITY_AUDIT_REPORT.md     # NEW
    ├── DPI_AUDIT_REPORT.md               # NEW
    ├── EXCEL_STABILITY_REPORT.md         # NEW
    ├── LOCALIZATION_READINESS.md         # NEW
    └── ENTERPRISE_POLISH_CHECKLIST.md    # NEW
```

**Structure Decision**: No new services or controls introduced — this phase modifies, validates, and hardens existing code. ToastWindow migration is the only source-level structural change. Six audit reports are documentation artifacts under `Docs/Architecture/`. Contracts directory remains empty as no new interfaces are needed.

## Complexity Tracking

> N/A — All constitution checks pass without violations. ToastWindow migration follows established ModernWindow pattern. No new architectural complexity introduced.

## Phase 0: Outline & Research

### Research Tasks

1. **Performance baseline methodology** — Determine profiling tools (WPR, PerfView, dotMemory), define measurement protocol for startup time, memory, DataGrid scroll fps, theme switch time, animation durations on reference enterprise PC (i5, 16GB, SSD).
2. **Accessibility baseline methodology** — Determine tooling (Windows Accessibility Insights), define test protocol covering keyboard navigation, focus visibility, AutomationProperties, WCAG 2.1 AA contrast in all 3 themes.
3. **DPI baseline methodology** — Define test protocol for 100%, 125%, 150%, 200% DPI across all 14 windows; multi-monitor mixed-DPI test configuration.
4. **Excel VSTO baseline** — Execute Phase 3 T057–T062 validation tasks; document current pass/fail status for each.
5. **Hardcoded-value sweep methodology** — Define regex patterns for grep: `#[0-9A-Fa-f]{6,8}`, `DropShadowEffect`, `CornerRadius="[0-9]`, `Margin="[0-9]`, `Padding="[0-9]`; exclude `Theme/Base/` files.
6. **ValidationEngine review** — Analyze current TODO at line ~185; design proper XAML/dictionary resource resolver using `Application.Current.Resources.MergedDictionaries` enumeration + XML parsing for unloaded dictionaries.
7. **ToastWindow analysis** — Review current ToastWindow.xaml for dependencies; plan migration path to ModernWindow inheritance.
8. **Freezable audit methodology** — Search for `SolidColorBrush`, `LinearGradientBrush`, `Pen`, `Transform`, `GeometryDrawing` in all `.xaml` files; catalog which can be safely frozen.

## Phase 1: Design & Contracts

### Deliverables

- `data-model.md` — Performance metrics schema (PerformanceBaseline), AccessibilityViolation catalog, DpiTestResult, Phase3ValidationTask status, ValidationEngineExtension design, AuditReport templates, EnterprisePolishChecklist tracker (all created)
- `contracts/` — No new interfaces needed; ValidationEngine extension follows existing method signatures
- `quickstart.md` — 7-step execution guide with PowerShell commands for baselines, cleanup, parallel workstreams, and production gate (created)
- Update `AGENTS.md` — Add plan reference between SPECKIT markers

### Workstream Execution Order

```text
WS-E (Tech Debt Cleanup) starts immediately (ToastWindow, hardcoded sweep)
    ↓
WS-A (Perf) + WS-B (Accessibility) + WS-C (DPI) in parallel after baselines
    ↓
WS-D (Excel Stability) after WS-E cleanup + WS-A/B/C results available
    ↓
WS-F (Diagnostics Finalization) after WS-D
    ↓
WS-G (Documentation) final — depends on all audit results
```
