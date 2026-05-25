# Implementation Plan: Diagnostics & Stability Platform

**Branch**: `011-diagnostics-stability-platform` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-diagnostics-stability-platform/spec.md`

## Summary

Prevent WPF/Excel rendering failures by providing a diagnostics panel (render mode, GPU, theme, memory), a resource validation engine (missing tokens, inline colors, duplicates), crash-safe theme loading with fallback recovery, and application logging. All new services extend existing infrastructure (ThemeManager, RenderModeService, WindowRenderModeDetector).

## Technical Context

**Language/Version**: C# / .NET Framework 4.8

**Primary Dependencies**: WPF, existing ThemeManager, RenderModeService, WindowRenderModeDetector, System.Diagnostics (logging), System.Management (memory estimation)

**Storage**: File system — log files stored at AppData/Som3a/Logs/ with auto-rotation

**Testing**: MSBuild project build + manual Excel VSTO host testing. Existing patterns: window-by-window validation via EXCEL_TEST_CHECKLIST.md.

**Target Platform**: Windows (Excel VSTO Add-in)

**Project Type**: Desktop application (VSTO Add-in), WPF UI

**Performance Goals**: Validation scan completes in <3s for ~30 resource dictionaries; log writes complete within 5s; diagnostics panel opens in <1s; runtime overhead imperceptible during normal operation

**Constraints**: Must not crash under Excel VSTO hosting; must render correctly in both FallbackSafe and WindowChrome modes; no remote data transmission; no automated repair of detected issues

**Scale/Scope**: ~15 windows, ~30 resource dictionaries, single-process VSTO add-in

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — New services are independent C# classes; no monolithic resource dictionaries introduced.
- [x] **III. DynamicResource-Only** — Diagnostics panel consumes existing theme resources via DynamicResource; no StaticResource needed.
- [x] **IV. Runtime Theme Mutation Governance** — Diagnostics panel is read-only (per clarification). No theme mutation path from diagnostics.
- [x] **IX. Animation Governance** — No new animations in scope. Existing animations unchanged.
- [x] **X. Excel Rendering Safety** — Core goal. Diagnostics panel explicitly required to render in both FallbackSafe and WindowChrome modes (FR-012).
- [x] **XI. WindowChrome Enforcement** — Diagnostics panel is embedded in SettingsWindow which inherits ModernWindow.
- [x] **XII. Centralized Effects** — Diagnostics panel reuses existing centralized effects only.
- [x] **XV. Resource Loading Order** — No new resource dictionaries introduced by this feature.

**Gate**: ✅ PASS — All constitutional checks pass. No violations to justify.

**Post-Design Re-check (Phase 1)**: ✅ PASS — Design introduces no new resource dictionaries, no StaticResource usage, no inline effects. All services follow MVVM pattern. No animation changes. No constitutional violations found.

## Project Structure

### Documentation (this feature)

```text
specs/001-diagnostics-stability-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (internal service interfaces)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
WpfApp2/
├── Services/
│   ├── DiagnosticsService.cs       # NEW  — aggregate diagnostics data (render mode, GPU, theme, memory)
│   ├── ValidationEngine.cs         # NEW  — runtime resource dictionary scanner
│   └── LoggingService.cs           # NEW  — file-based structured logging
├── Views/
│   ├── SettingsWindow.xaml         # MODIFY — add Diagnostics section/tab
│   └── SettingsWindow.xaml.cs      # MODIFY — wire diagnostics view model
├── ViewModels/
│   └── DiagnosticsViewModel.cs     # NEW  — MVVM for diagnostics panel
├── Services/
│   ├── ThemeManager.cs             # MODIFY — add theme validation hooks, crash-safe loading
│   └── RenderModeService.cs        # EXTEND — add popup diagnostics
├── Theme/
│   └── ...                         # MODIFY — add fallback-safe theme dictionary if needed
├── Models/
│   └── DiagnosticsModels.cs        # NEW  — DiagnosticSnapshot, ValidationResult, LogEntry DTOs
```

## Complexity Tracking

No constitutional violations. Not applicable.

## Phase 0: Research

See [research.md](research.md).

## Phase 1: Design & Contracts

See [data-model.md](data-model.md), [contracts/](contracts/), [quickstart.md](quickstart.md).
