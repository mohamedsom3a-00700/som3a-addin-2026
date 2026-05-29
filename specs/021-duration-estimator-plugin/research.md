# Research: Duration Estimator Plugin

**Phase**: 0 — Outline & Research
**Date**: 2026-05-28
**Feature**: Duration Estimator Plugin (Phase 22)

## Overview

All Technical Context items were immediately resolvable from the existing enterprise platform architecture. No NEEDS CLARIFICATION markers were present. This document captures architectural decisions, best practices review, and integration patterns.

---

## Architectural Decisions

### Decision 1: Plugin Structure — Single .NET 8.0 Class Library

**Decision**: Implement Duration Estimator as a single .NET 8.0 class library at `Plugins/DurationEstimator/` following the pattern established by Phase 21 (RelationshipGenerator).

**Rationale**: The existing Som3a.Plugin.SDK (Phase 14) provides the standard plugin lifecycle — discovery via AssemblyScanner, registration via attributes ([Plugin], [NavigationItem]), lifecycle management via PluginHost. A single assembly is sufficient because the plugin domain is well-bounded (productivity analysis only).

**Alternatives considered**:
- Multi-assembly (core + UI + export) — Over-engineered for a single-domain plugin; would increase load time and complexity
- Inline within WpfApp2 — Violates the plugin-first architecture (ADR-002)

### Decision 2: Duration Formula Implementation

**Decision**: Implement the standard construction productivity formula: `Duration (working days) = Quantity / (Productivity Rate × Crew Size × Hours Per Day)`, where Productivity Rate is expressed in **quantity per worker-hour**.

**Rationale**: This is the industry-standard formula used by construction planning software (Primavera P6, MS Project). It separates each input factor independently, allowing users to adjust crew size and hours without modifying productivity rates.

**Alternatives considered**:
- Single composite rate (durations per unit already including crew) — Less flexible; prevents crew-size what-if analysis
- Machine learning regression — Over-engineered for deterministic calculation; data requirements unrealistic for v1

### Decision 3: Productivity Benchmark Storage

**Decision**: Built-in benchmarks embedded as JSON resource in the assembly; user-custom benchmarks stored as a JSON file at `AppData/Som3a/DurationEstimator/benchmarks.json`.

**Rationale**: JSON provides human-readable format for advanced users to edit. Built-in set ensures every installation starts with useful defaults. Separate user file prevents upgrade conflicts.

**Alternatives considered**:
- SQLite (Phase 27) — Not yet available; planned for later standardization
- XML — Heavier parsing; JSON is the project convention (System.Text.Json)

### Decision 4: Calendar Implementation

**Decision**: Single calendar per project with configurable working days, holidays, and hours per day. Calendar is an in-memory model managed by CalendarEngine.

**Rationale**: The spec explicitly scopes single-calendar for v1. The Calendar entity already exists in Som3a.Domain (Phase 14), so reuse is preferred.

**Alternatives considered**:
- Multiple calendars per project — Out of scope per spec clarification
- Calendar-less (always 5-day week with 8-hour days) — Insufficient for real construction schedules
- Reuse of existing Excel/VSTO calendar — Couples the plugin to Excel interop; breaks the domain model purity

### Decision 5: AI Integration Pattern

**Decision**: AI features (productivity suggestions, anomaly detection) use the existing AIOrchestrator from Som3a.AI (Phase 18) with structured output parsers.

**Rationale**: AIOrchestrator already handles provider routing, retry logic, context building, and structured output parsing. The Duration Estimator only needs to provide domain-specific prompt templates and output schemas.

**Alternatives considered**:
- Direct AI provider calls — Duplicates infrastructure already provided by Som3a.AI
- Local heuristic-only (no AI) — Loses the value of AI productivity benchmarking against similar projects

### Decision 6: Export Strategy

**Decision**: Excel export via Som3a.Exporting ExcelExportEngine for the Excel sheet output; a dedicated SchedulingPipelineWriter for Primavera-compatible data feed.

**Rationale**: The ExcelExportEngine already provides theme-consistent styling and multi-sheet support. The scheduling pipeline requires a distinct format (activity ID + duration fields) consumed by downstream scheduling tools.

**Alternatives considered**:
- Single exporter for both targets — Different consumers have different field requirements; separation keeps each clean
- CSV-only for pipeline — Less structured; Primavera-compatible implies a structured data contract

---

## Best Practices Review

### .NET 8.0 Class Library Patterns
- Use primary constructors for simple dependency injection
- Use records for immutable domain models (ProductivityRate, DurationEstimate)
- Use `JsonSerializerContext` source generation for benchmark serialization (AOT-friendly)
- All public interfaces belong in the plugin namespace; consumers reference through contracts
- Async all the way for AI calls and Excel export

### Construction Productivity Standards
- Productivity rates expressed as `quantity per crew-day` (e.g., 50 m²/day for a crew of 4)
- Crew sizes range from 1-10 depending on trade and activity complexity
- Learning curve follows Wright's model: T(n) = T(1) × n^(-log₂(learning rate))
- Weather impact: 5-20% productivity reduction depending on conditions
- Standard work week: 5 days × 8 hours = 40 hours (configurable)

### Calendar Rules
- Working days: Monday-Friday by default (configurable per project)
- Holidays: Configurable list; weekends excluded automatically
- Working hours: 8 hours/day default (configurable)
- Duration formula always produces working days; calendar-aware end date calculation
- Working days to calendar days: EndDate = StartDate + (Duration - 1) working days, skipping non-working days

### Three-Point Estimation (PERT)
- Optimistic (O): Best-case scenario (highest productivity)
- Most Likely (M): Expected scenario
- Pessimistic (P): Worst-case scenario (lowest productivity)
- Expected Duration (E) = (O + 4M + P) / 6
- Standard Deviation (SD) = (P - O) / 6
- Confidence interval: E ± (Z × SD) where Z = 1.645 for 90%, 1.96 for 95%, 2.576 for 99%

---

## Integration Patterns

### With Som3a.Domain
- Reuse `Activity` entity from Som3a.Domain/Activities/ — provides Quantity, Unit, Name
- Reuse `Calendar` entity from Som3a.Domain/Calendars/ — provides WorkDay, Holiday, WorkingHours
- Extend with plugin-specific value objects: `ProductivityRate`, `DurationEstimate`, `ProductivityModifier`

### With Som3a.Plugin.SDK
- `[Plugin(Id = "duration-estimator", Name = "Duration Estimator", Version = "1.0.0")]` for auto-discovery
- `[NavigationItem(Category = "Planning", Order = 4)]` for Shell sidebar registration
- `[SettingsSection(Category = "Plugins", Order = 3)]` for plugin configuration

### With Som3a.AI
- AIOrchestrator.ExecutePromptAsync with prompt template "duration-productivity-suggest"
- AIOrchestrator.ExecutePromptAsync with prompt template "duration-anomaly-detect"
- StructuredOutputParser to parse AI responses into `ProductivitySuggestion[]` and `AnomalyReport[]`

### With Som3a.Exporting
- ExcelExportEngine.GenerateSheet for the duration export worksheet
- Custom SchedulingPipelineWriter for targeted downstream format

### With Som3a.Contracts
- IPlugin lifecycle: Initialize → Register → LoadUI → RegisterSettings
- ISettingsModule for plugin configuration (default productivity rates, crew defaults, calendar presets)
- IExportEngine for the export pipeline integration

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| AI suggestions inaccurate for niche trades | Medium | Medium | Suggestions are advisory; engineer always approves |
| Calendar calculations fail edge cases (leap years, multi-year spans) | Low | High | Comprehensive unit test coverage for CalendarEngine |
| Productivity benchmark data incomplete | Medium | Medium | User-extensible; built-in set covers 80% of common trades |
| Excel export corruption for very large datasets (10k activities) | Low | Medium | Batch write with progress; test at scale |
| Plugin conflicts with other plugins (settings, navigation) | Low | High | Plugin SDK isolation; unique plugin ID registration |
