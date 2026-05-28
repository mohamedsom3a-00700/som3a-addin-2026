# Data Model: Duration Estimator Plugin

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-28
**Feature**: Duration Estimator Plugin (Phase 22)

---

## Entity Model

### ProductivityRate

A numeric rate defining how much work a crew can complete per unit time.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | string | Unique identifier | Required, max 64 chars |
| TradeCategoryId | string | References TradeCategory | Required, must exist |
| ActivityDescription | string | Description of the work type | Required, max 200 chars |
| ProductivityValue | decimal | Quantity per crew-day | Required, > 0 |
| UnitOfMeasure | string | Unit (m², m³, ton, each, etc.) | Required, max 20 chars |
| CrewSize | int | Default crew size for this rate | Required, >= 1, <= 50 |
| Source | string | Origin of the benchmark | Optional, max 100 chars |
| IsBuiltIn | bool | True if built-in, false if user-custom | Required, default false |
| IsActive | bool | Soft-delete flag | Required, default true |
| Version | int | Optimistic concurrency | Required, >= 1 |

**Identity**: (TradeCategoryId, ActivityDescription, UnitOfMeasure) — unique combination

**Validation Rules**:
- ProductivityValue must be > 0 and <= 1,000,000
- CrewSize must be between 1 and 50
- ActivityDescription must not be empty
- UnitOfMeasure must be from the approved unit list

---

### TradeCategory

A classification grouping that organizes productivity benchmarks.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | string | Unique identifier | Required, max 40 chars |
| Name | string | Display name | Required, max 100 chars |
| DisplayOrder | int | Sort order in UI | Required, >= 0 |
| IsBuiltIn | bool | True if built-in | Required, default false |

**Built-in Categories**: concrete, steel, masonry, MEP, finishes, earthwork, roofing, insulation, glazing, painting, flooring, tiling, landscaping, roadwork, piling, waterproofing

---

### DurationEstimate

The calculated result for an activity.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| ActivityId | string | References external Activity entity | Required |
| Quantity | decimal | Activity quantity | Required, > 0 |
| AppliedProductivityRate | decimal | Rate used in calculation | Required, > 0 |
| CrewSize | int | Crew size used | Required, >= 1 |
| HoursPerDay | decimal | Working hours per day | Required, > 0, <= 24 |
| DurationWorkingDays | decimal | Result in working days | Required, >= 0 |
| CalendarDurationDays | int | Calendar days including non-working | Required, >= 0 |
| StartDate | DateTime? | Scheduled start date | Optional |
| EndDate | DateTime? | Calculated end date | Optional |
| OptimisticDuration | decimal? | 3-point optimistic estimate | Optional |
| MostLikelyDuration | decimal? | 3-point most likely estimate | Optional |
| PessimisticDuration | decimal? | 3-point pessimistic estimate | Optional |
| StandardDeviation | decimal? | Standard deviation of estimate | Optional |
| ConfidenceInterval95Lower | decimal? | 95% CI lower bound | Optional |
| ConfidenceInterval95Upper | decimal? | 95% CI upper bound | Optional |
| AppliedModifiers | List<ProductivityModifier> | Modifiers applied | Optional |
| IsAnomaly | bool | Flagged by anomaly detection | Default false |
| AnomalyReason | string | Explanation if flagged | Optional |
| LastCalculated | DateTime | Timestamp of calculation | Required |
| CalculationVersion | string | Version of calculation engine | Required |

**Validation Rules**:
- DurationWorkingDays must equal Quantity / (AppliedProductivityRate × CrewSize × HoursPerDay) within rounding tolerance
- CalendarDurationDays must be >= DurationWorkingDays
- If OptimisticDuration provided, PessimisticDuration must also be provided
- StandardDeviation = (Pessimistic - Optimistic) / 6 when both provided

---

### ProductivityModifier

A configurable factor that adjusts the base productivity rate.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | string | Unique modifier identifier | Required, max 40 chars |
| Name | string | Display name (e.g., "Weather", "Zone") | Required, max 100 chars |
| ModifierType | enum | Adjustment type | Required |
| Percentage | decimal | Adjustment percentage | Required, -50 to +50 |
| Description | string | Explanation of the modifier | Optional, max 500 chars |

**ModifierType values**: ZonePhased, Weather, SiteCondition, LearningCurve, Custom

**Behavior**:
- Positive percentage = productivity reduction (duration increases)
- Negative percentage = productivity improvement (duration decreases)
- Multiple modifiers are additive: EffectiveRate = BaseRate × (1 + Sum(Percentages) / 100)
- If EffectiveRate <= 0, calculation is invalid and flagged

---

### CalendarConfig

Project calendar configuration.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| WorkingDays | List<DayOfWeek> | Days considered working | Required, at least 1 |
| Holidays | List<DateTime> | Non-working holidays | Optional |
| HoursPerDay | decimal | Hours per working day | Required, > 0, <= 24 |
| StartDate | DateTime | Project start date | Required |

**Default**: Monday-Friday working days, 8 hours/day, no holidays

---

## Entity Relationships

```text
TradeCategory (1) ──── (*) ProductivityRate
     │                          │
     │                          │ has many
     │                          ▼
     │                   DurationEstimate
     │                          │
     │                          │ calculated for
     │                          ▼
     │                    Activity (Som3a.Domain)
     │
     └── groups rates by trade

ProductivityModifier (*) ── applied to ── DurationEstimate

CalendarConfig (1) ── determines ── DurationEstimate.EndDate
```

---

## State Transitions

### DurationEstimate Lifecycle

```text
[Pending] ── calculate ──▶ [Calculated] ── export ──▶ [Exported]
                              │
                              │ input changes
                              ▼
                          [Pending]  (recalculation triggered)
```

- **Pending**: Activity loaded but no duration calculated yet
- **Calculated**: Duration computed; values are current
- **Exported**: Duration has been written to Excel or scheduling pipeline
- Input changes (rate, crew, calendar, quantity) reset Calculated → Pending for affected activities only
- AI suggestions and anomaly detection run on Calculated state as optional overlays

---

## Data Volume Estimates

| Entity | Expected Count | Growth Rate |
|--------|---------------|-------------|
| ProductivityRate (built-in) | 50-200 | By user additions only |
| ProductivityRate (custom) | 0-500 per user | As users add benchmarks |
| TradeCategory | 15-20 built-in | Rarely added |
| DurationEstimate | 10-10,000 per project | One per activity per session |
| ProductivityModifier | 0-10 per project | Configuration-driven |

---

## Serialization

- Built-in productivity benchmarks: Embedded JSON resource in assembly
- Custom productivity benchmarks: JSON file at `%AppData%/Som3a/DurationEstimator/benchmarks.json`
- Duration estimates: In-memory during session; exported to Excel/scheduling pipeline
- Calendar configuration: Plugin settings via Som3a.Infrastructure (Phase 16 dynamic settings)
- All JSON uses `System.Text.Json` with source-generated serialization context
