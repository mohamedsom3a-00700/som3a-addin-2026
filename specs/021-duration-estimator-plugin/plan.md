# Implementation Plan: Duration Estimator Plugin

**Branch**: `022-duration-estimator-plugin` | **Date**: 2026-05-28 | **Spec**: spec.md

**Input**: Feature specification from `specs/021-duration-estimator-plugin/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Productivity analysis engine (Phase 22 of Enterprise Planning Platform) that calculates construction activity durations from quantities and productivity rates. Features a benchmark library organized by trade category, calendar-aware scheduling, three-point variance analysis, and AI-powered productivity suggestions with anomaly detection. Exports results to Excel and feeds the downstream scheduling pipeline (Primavera-compatible). Supports up to 10,000 activities with live incremental recalculation and auto-save.

## Technical Context

**Language/Version**: C# (.NET 8.0 class library for plugin logic, .NET Framework 4.8 for VSTO WPF host Shell pages)

**Primary Dependencies**:
- Som3a.Domain (Phase 14) — Activity, Calendar, Quantity domain entities
- Som3a.Contracts (Phase 14) — IPlugin, ISettingsModule, IExportEngine interfaces
- Som3a.AI (Phase 18) — AIOrchestrator, ContextBuilder for AI productivity suggestions and anomaly detection
- Som3a.Plugin.SDK (Phase 14) — [Plugin], [NavigationItem], [SettingsSection] attributes
- Som3a.Exporting (Phase 14) — ExcelExportEngine for duration sheet generation
- Som3a.Infrastructure (Phase 14) — Secure storage for AI provider keys (if AI features used)
- System.Text.Json — Productivity benchmark serialization

**Storage**: Productivity benchmarks stored as built-in resources + user-custom JSON at AppData/Som3a/DurationEstimator/benchmarks.json; plugin settings via Dynamic Settings Platform (Phase 16)

**Testing**: Unit tests (ProductivityEngine, DurationCalculator, CalendarEngine, VarianceAnalyzer); Integration tests (Excel export round-trip, AI provider integration); VSTO host tests (plugin loads and functions inside Excel)

**Target Platform**: Windows x64 — Excel VSTO Add-in host — Plugin runs as .NET 8.0 library loaded by Som3a.Plugin.SDK host

**Project Type**: Desktop application plugin (VSTO add-in plugin within Som3a Enterprise Planning Platform)

**Performance Goals**:
- 10,000 activity durations calculated within 5 minutes on initial import (batch)
- Live recalculation completes within 500ms per single input change
- Excel export of 1,000 durations in under 30 seconds; 10,000 in under 3 minutes
- UI remains responsive during calculations (async/await pattern)
- Benchmark library loads within 1 second at plugin startup

**Constraints**:
- Must not break existing VSTO functionality or other plugins
- Must follow DynamicResource-only, centralized-effects, MVVM rules (Constitution) for any UI pages
- Excel VSTO rendering safety must be maintained (WindowRenderModeDetector fallback)
- Maximum 10,000 activities per project
- Single calendar per project (multi-calendar out of scope for v1)
- AI suggestions are advisory only; engineer always approves before use

**Scale/Scope**: Single .NET 8.0 plugin assembly within a multi-plugin platform; 50+ built-in productivity benchmarks across 5+ trade categories; 10,000 max activities; registers 1-2 Shell pages under "Planning" sidebar category

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Plugin follows the established plugin pattern (Som3a.Plugin.SDK). Resources (benchmarks, settings) are isolated within the plugin assembly. No monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — Any UI pages registered by this plugin will follow the existing Shell/Theme conventions using DynamicResource; no StaticResource usage.
- [x] **IV. Runtime Theme Mutation Governance** — Plugin does not mutate themes. Theme mutations remain routed through ThemeManager exclusively.
- [x] **IX. Animation Governance** — Plugin UI (if any) uses existing Shell animation conventions (≤200ms, GPU-safe, Excel-host-safe).
- [x] **X. Excel Rendering Safety** — Plugin pages run inside the existing Shell which already implements WindowRenderModeDetector and fallback mode. No new windowing introduced.
- [x] **XI. WindowChrome Enforcement** — Plugin uses existing Shell Pages; no new standalone windows. No WindowChrome changes.
- [x] **XII. Centralized Effects** — Plugin UI uses existing centralized effects (Effects/Shadows.xaml, Effects/Glow.xaml). No inline effects.
- [x] **XV. Resource Loading Order** — The plugin does not introduce new ResourceDictionaries; it uses the existing ThemeResources.xaml loading order for any UI needs.

**Gate status**: ALL PASS — No constitutional violations. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/021-duration-estimator-plugin/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Plugins/DurationEstimator/
├── src/
│   ├── DurationEstimator.csproj
│   ├── Engine/
│   │   ├── ProductivityEngine.cs        # Duration = Qty / (Productivity * Crew * Hours)
│   │   ├── DurationCalculator.cs        # Calendar-aware duration computation
│   │   └── CrewSizeFactor.cs            # Crew size adjustments
│   ├── Benchmarks/
│   │   ├── ProductivityBenchmarkLibrary.cs  # Built-in + custom benchmark registry
│   │   ├── TradeCategoryRegistry.cs     # Trade category management
│   │   └── BenchmarkData.json           # Built-in productivity rates (embedded resource)
│   ├── Variance/
│   │   ├── VarianceAnalyzer.cs          # 3-point estimates, standard deviation, CI
│   │   └── RiskAdjuster.cs              # Risk-adjusted duration engine
│   ├── AI/
│   │   ├── AIProductivitySuggestor.cs   # AI-driven rate recommendations
│   │   └── AnomalyDetector.cs           # Statistical anomaly flagging
│   ├── Export/
│   │   ├── DurationExporter.cs          # Excel export + scheduling pipeline data
│   │   └── SchedulingPipelineWriter.cs  # Primavera-compatible format output
│   └── Calendar/
│       ├── CalendarEngine.cs           # Work day / holiday / hours logic
│       └── CalendarConfig.cs           # Calendar configuration model
├── tests/
│   ├── DurationEstimator.UnitTests.csproj
│   ├── Unit/
│   │   ├── ProductivityEngineTests.cs
│   │   ├── DurationCalculatorTests.cs
│   │   ├── CalendarEngineTests.cs
│   │   ├── VarianceAnalyzerTests.cs
│   │   └── AnomalyDetectorTests.cs
│   └── Integration/
│       ├── ExcelExportIntegrationTests.cs
│       └── AIIntegrationTests.cs
└── README.md
```

**Structure Decision**: Option 1 (Single project with modular namespaces) — Selected. The Duration Estimator is a single .NET 8.0 class library that follows the standard plugin structure established by Phases 14 and 20-21. Business logic is organized by domain concern (Engine, Benchmarks, Variance, AI, Export, Calendar) into separate namespaces/folders within one assembly. Unit tests mirror the source structure. The plugin registers Shell pages and settings via Som3a.Plugin.SDK attributes.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitutional violations identified. Complexity tracking is not required.
