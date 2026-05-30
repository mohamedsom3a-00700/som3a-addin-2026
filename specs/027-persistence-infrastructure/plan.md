# Implementation Plan: Persistence & Platform Database Infrastructure

**Branch**: `027-persistence-infrastructure` | **Date**: 2026-05-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/027-persistence-infrastructure/spec.md`

## Summary

Introduce a SQLite-based enterprise persistence layer for the Planova platform covering settings, AI metadata (execution history, token usage), plugin metadata and states, diagnostics (logs, crash reports, export history), and templates (WBS, activity, productivity benchmarks). The layer uses repository pattern with async operations, Unit of Work for transaction safety, automatic migrations with rollback, configurable per-category data retention, DPAPI encryption for secrets, and backup/recovery support.

## Technical Context

**Language/Version**: C# (.NET 8.0 for persistence library in Som3a.Infrastructure/Planova.Infrastructure)

**Primary Dependencies**: Microsoft.Data.Sqlite, System.Text.Json (for structured content serialization)

**Storage**: SQLite single file at AppData/Som3a/platform.db; WAL mode for concurrent read/write

**Testing**: xUnit for unit tests (repository contracts, migration engine, validation); integration tests with in-memory SQLite for migration and backup scenarios

**Target Platform**: Windows x64 — Excel VSTO Add-in host (.NET Framework 4.8 shell consuming .NET 8.0 persistence library via bridge)

**Project Type**: Class library (.NET 8.0) within the existing solution

**Performance Goals**: DB startup <100ms, settings load <50ms, AI history query <300ms for 10k records, plugin metadata load <100ms

**Constraints**:
- All DB operations async with cancellation tokens
- Sensitive data encrypted via Windows DPAPI at rest
- SQLite WAL mode for concurrent read/write
- GUID primary keys for all entities (future sync readiness)
- Per-category data retention with automated cleanup (AI: 1yr, diagnostics: 90d, crash reports: 2yr)
- Migration version tracking with automatic rollback on failure
- Full database backup with integrity verification

**Scale/Scope**: Single-user local desktop database; target <500MB over 12 months with retention defaults

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design — PASS.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Persistence is implemented as a class library within Som3a.Infrastructure; isolated, testable, replaceable. **PASS**
- [x] **III. DynamicResource-Only** — N/A (no UI resources in persistence library). **NO VIOLATION**
- [x] **IV. Runtime Theme Mutation Governance** — N/A (no theme mutation in persistence library). **NO VIOLATION**
- [x] **IX. Animation Governance** — N/A (no animations in persistence library). **NO VIOLATION**
- [x] **X. Excel Rendering Safety** — N/A (no new windows or UI). **NO VIOLATION**
- [x] **XI. WindowChrome Enforcement** — N/A (no new windows). **NO VIOLATION**
- [x] **XII. Centralized Effects** — N/A (no visual effects in persistence library). **NO VIOLATION**
- [x] **XV. Resource Loading Order** — N/A (no resource dictionaries in persistence library). **NO VIOLATION**

**Result: Constitution Gate PASSES.** No violations. This is a non-UI backend persistence library — all UI-specific constitutional principles are not applicable.

## Project Structure

### Documentation (this feature)

```text
specs/027-persistence-infrastructure/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Som3a.Infrastructure/  (or Planova.Infrastructure after rebranding)
└── Persistence/
    ├── SQLite/
    │   ├── DatabaseContext.cs
    │   ├── DatabaseFactory.cs
    │   ├── ConnectionManager.cs
    │   └── SQLiteConfiguration.cs
    ├── Repositories/
    │   ├── SettingsRepository.cs
    │   ├── AIRepository.cs
    │   ├── PluginRepository.cs
    │   ├── DiagnosticsRepository.cs
    │   └── TemplateRepository.cs
    ├── Interfaces/
    │   ├── ISettingsRepository.cs
    │   ├── IAIRepository.cs
    │   ├── IPluginRepository.cs
    │   ├── ILogRepository.cs
    │   └── ITemplateRepository.cs
    ├── Migrations/
    ├── Seeders/
    ├── UnitOfWork/
    └── Backup/

tests/
└── Som3a.Infrastructure.Tests/
    ├── Repositories/
    ├── Migration/
    ├── Backup/
    └── Validation/
```

**Structure Decision**: Single project (Som3a.Infrastructure) with Persistence/ namespace, following the repository + Unit of Work pattern defined in the enterprise master plan. Test project mirrors the source structure.

## Complexity Tracking

> **No Constitution violations — all UI-specific principles are N/A for this non-UI persistence library.**
