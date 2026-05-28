# Feature Specification: Duration Estimator Plugin

**Feature Branch**: `feature/phase-22-duration-estimator`

**Created**: 2026-05-28

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 22"

## Clarifications

### Session 2026-05-28

- Q: Where do calculated duration estimates go? → A: Export to Excel sheet AND feed into downstream scheduling pipeline (Primavera-compatible).
- Q: How should batch duration calculation behave? → A: Incremental/live — activities recalculated instantly as user modifies inputs (rates, crew, calendar), always reflects current data.
- Q: How should duration estimates be persisted? → A: Auto-save — every calculation result is automatically saved, no manual save action required.
- Q: What is the maximum number of activities the plugin must support? → A: Thousands (<10,000) — large project or multi-building scope.
- Q: Should the "planning engineer" vs "project manager" role distinction translate to feature gating? → A: Narrative only — personas are illustrative, no actual feature access control.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Duration Calculation from Activity Quantities (Priority: P1)

A planning engineer selects activities with known quantities (from BOQ/Activity Generator) and calculates their durations using productivity rates, crew sizes, and working hours per day. Results are exported to Excel and fed into the downstream scheduling pipeline.

**Why this priority**: Duration calculation is the core value proposition — without it, the plugin cannot produce schedule estimates. All other features (benchmarks, variance, AI) build on this foundation.

**Independent Test**: Can be fully tested by importing a list of activities with quantities, applying a productivity rate, crew size, and hours per day, and verifying the calculated duration matches `Quantity / (Productivity * Crew * Hours Per Day)`.

**Acceptance Scenarios**:

1. **Given** a list of activities each with a quantity value, **When** the user selects activities and provides productivity rate, crew size, and hours per day, **Then** the system calculates duration for each activity using the standard formula.
2. **Given** an activity with quantity 100, productivity rate 10, crew size 2, and 8 hours per day, **When** duration is calculated, **Then** the result is 0.625 working days (100 / (10 * 2 * 8)).
3. **Given** an activity with zero quantity, **When** duration calculation is attempted, **Then** the system flags the activity as invalid and does not produce a duration.
4. **Given** calculated durations are complete, **When** the user triggers export, **Then** results are written to an Excel sheet with activity ID, quantity, productivity rate, crew size, hours per day, and calculated duration columns.
5. **Given** an activity already has a calculated duration, **When** the user modifies the productivity rate, crew size, or hours per day, **Then** the duration updates instantly without requiring a manual recalculate action.

---

### User Story 2 - Productivity Benchmark Database with Trade Categories (Priority: P2)

A planning engineer browses a built-in library of productivity benchmarks organized by trade (concrete, steel, MEP, finishes, etc.) and applies relevant rates to their activities, with the ability to override or add custom benchmarks.

**Why this priority**: Productivity benchmarks significantly accelerate duration estimation by providing industry-standard starting points. Without benchmarks, engineers must manually research or guess productivity rates for every activity.

**Independent Test**: Can be fully tested by browsing the benchmark library, filtering by trade category, selecting a benchmark rate for an activity, and verifying the rate is applied correctly in the duration calculation.

**Acceptance Scenarios**:

1. **Given** the productivity benchmark library, **When** the user browses by trade category, **Then** relevant benchmarks are displayed with trade name, activity description, productivity rate, unit, and source.
2. **Given** a user selects a benchmark rate for an activity, **When** the duration is recalculated, **Then** the selected productivity rate replaces the default and the new duration is shown immediately.
3. **Given** the user has custom productivity data, **When** they enter a custom benchmark, **Then** it is saved and available for future use alongside built-in benchmarks.

---

### User Story 3 - Calendar-Aware Duration Scheduling (Priority: P2)

A planning engineer configures a project calendar (working days, holidays, working hours) and durations are automatically adjusted to calendar-aware working days instead of calendar days.

**Why this priority**: Construction schedules must account for non-working days (weekends, holidays). Calendar-unaware durations produce misleading schedules that cannot be used for planning.

**Independent Test**: Can be fully tested by configuring a calendar with specific work days and holidays, calculating a duration, and verifying the end date correctly skips non-working days.

**Acceptance Scenarios**:

1. **Given** a project calendar with Saturday-Sunday as non-working days, **When** a 10-working-day duration is calculated starting Monday, **Then** the resulting end date is 14 calendar days later (skipping 2 weekends).
2. **Given** a calendar with a public holiday, **When** a duration spans that holiday, **Then** the holiday is excluded from working day count and the end date shifts accordingly.
3. **Given** the user modifies working hours per day on the calendar, **When** duration is recalculated, **Then** the result updates to reflect the new hours.

---

### User Story 4 - Variance Analysis with 3-Point Estimates (Priority: P3)

A project manager (narrative role — no feature gating) generates Optimistic, Most Likely, and Pessimistic duration estimates with confidence intervals to understand schedule risk and uncertainty.

**Why this priority**: Variance analysis provides schedule risk visibility. It enables project managers to communicate confidence levels and make informed decisions about schedule buffers and contingencies.

**Independent Test**: Can be fully tested by providing optimistic and pessimistic productivity rates for an activity and verifying that the 3-point estimates (Optimistic, Most Likely, Pessimistic) are calculated with standard deviation and confidence intervals.

**Acceptance Scenarios**:

1. **Given** an activity with optimistic (15), most likely (10), and pessimistic (5) productivity rates, **When** variance analysis is run, **Then** the system shows 3 duration values with standard deviation and confidence interval.
2. **Given** only a single productivity rate is available, **When** variance analysis is run, **Then** the system uses the single rate for all 3 estimates and notes low confidence.
3. **Given** variance analysis results, **When** the user adjusts the confidence level (e.g., 80% vs 95%), **Then** the confidence interval range updates accordingly.

---

### User Story 5 - AI Productivity Suggestions and Anomaly Detection (Priority: P3)

A planning engineer requests AI-powered productivity rate suggestions based on the project type, and the system flags activities with unusual durations that may indicate data entry errors or unrealistic expectations.

**Why this priority**: AI suggestions reduce the time spent researching productivity rates and help catch errors early. While valuable, this feature depends on the AI infrastructure (Phase 18) and can be layered on after core functionality works.

**Independent Test**: Can be fully tested by comparing AI-suggested rates against known industry benchmarks for a given project type and verifying flagged anomalies are genuinely unusual relative to similar activities.

**Acceptance Scenarios**:

1. **Given** a set of activities in a project of known type (e.g., residential high-rise), **When** AI suggestions are requested, **Then** productivity rates are recommended per activity based on the project type.
2. **Given** an activity with a duration significantly longer or shorter than similar activities, **When** the anomaly detection runs, **Then** the activity is flagged with an explanation of why it appears unusual.
3. **Given** the user accepts an AI-suggested productivity rate, **When** duration is recalculated, **Then** the new rate is applied and the previous rate is logged for comparison.

---

### Edge Cases

- What happens when an activity has quantity but no unit of measure matches any productivity benchmark?
- How does the system handle activities with extremely large quantities that produce impractical durations (e.g., >5 years for a single activity)?
- What happens when the user modifies the calendar after durations have already been calculated?
- How does the system handle overlapping or conflicting productivity benchmarks for the same activity?
- What happens when an activity is deleted or its quantity changes after duration has been calculated?
- How does the system handle zero-crew-size or zero-hours-per-day inputs?
- What happens when AI provider is unavailable for productivity suggestions?
- How are durations affected when a calendar has zero working days configured?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to input or import activities with quantity values and units of measure for duration calculation.
- **FR-002**: System MUST calculate activity duration using the formula: Duration = Quantity / (Productivity Rate * Crew Size * Hours Per Day).
- **FR-002b**: Duration calculations MUST update instantly (within 500ms) whenever any input value (productivity rate, crew size, hours per day, quantity) changes — no manual recalculate action required.
- **FR-003**: System MUST provide a built-in productivity benchmark library organized by trade category (concrete, steel, MEP, finishes, earthwork, etc.) with editable rates.
- **FR-004**: Users MUST be able to add, edit, and delete custom productivity benchmarks for their own reference.
- **FR-005**: Users MUST be able to adjust crew size and hours per day per activity or globally.
- **FR-006**: System MUST support project calendar configuration including working days, non-working days, holidays, and working hours per day.
- **FR-007**: Duration calculations MUST be calendar-aware — expressed in working days, not calendar days.
- **FR-008**: System MUST support zone/phased construction adjustments as a percentage modifier on productivity.
- **FR-009**: System MUST support weather and site condition factors as configurable percentage modifiers on productivity.
- **FR-010**: System MUST produce three-point estimates (Optimistic, Most Likely, Pessimistic) with standard deviation and confidence intervals when multiple productivity rates are available.
- **FR-011**: System MUST provide risk-adjusted durations based on configurable confidence levels.
- **FR-012**: System MUST support AI-powered productivity rate suggestions based on project type when AI providers are available.
- **FR-013**: System MUST detect and flag activity durations that are statistical anomalies compared to similar activities in the same project.
- **FR-014**: System MUST log all productivity rate changes, duration recalculations, and user overrides for audit and comparison.
- **FR-015**: System MUST allow users to apply learning curve factors that reduce productivity over repeated cycles of the same activity type.
- **FR-016**: System MUST export calculated durations to an Excel sheet with each activity's ID, quantity, productivity rate, crew size, hours per day, and resulting duration.
- **FR-017**: System MUST expose calculated durations as consumable data for the downstream scheduling pipeline (Primavera-compatible format).
- **FR-018**: System MUST auto-save all duration calculation results immediately whenever inputs change — no explicit save action required from the user.

### Key Entities *(include if feature involves data)*

- **Activity**: A construction task with quantity, unit of measure, and associated productivity inputs. Relates to BOQ Activity Generator output (Phase 20) and WBS Structure (Phase 19).
- **ProductivityRate**: A numeric rate defining how much work can be completed per unit time (e.g., m²/day). Belongs to a trade category and has a unit of measure. May be built-in benchmark or user-defined custom entry.
- **Calendar**: A project schedule configuration defining working days, holidays, and working hours. Determines how working durations map to calendar durations.
- **DurationEstimate**: The calculated result for an activity including base duration (working days), calendar duration, and optionally 3-point estimates (optimistic, most likely, pessimistic) with confidence intervals.
- **ProductivityModifier**: Configurable factors (zone/phased construction, weather, site conditions, learning curve) that adjust the base productivity rate up or down by a percentage.
- **TradeCategory**: A classification grouping (concrete, steel, MEP, finishes, earthwork, etc.) that organizes productivity benchmarks and enables filtering.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Planning engineers can calculate durations for a project of up to 10,000 activities in under 5 minutes using the productivity benchmark library.
- **SC-002**: Calendar-aware duration calculations correctly skip weekends and holidays with 100% accuracy for any configured calendar.
- **SC-003**: Variance analysis produces Optimistic, Most Likely, and Pessimistic estimates with standard deviation <10% error compared to manual calculation using the same inputs.
- **SC-004**: AI productivity suggestions are within 20% of published industry benchmarks for at least 5 construction project types (residential, commercial, industrial, infrastructure, MEP).
- **SC-005**: Anomaly detection flags at least 90% of deliberately erroneous duration entries (e.g., quantity off by factor of 10) without generating false positives on normal entries.
- **SC-006**: Users can complete a full duration estimate workflow (import activities → apply benchmarks → configure calendar → calculate → review results) in under 10 minutes.
- **SC-007**: The productivity benchmark library contains at least 50 built-in rates across 5+ trade categories at initial release.
- **SC-008**: Calculated durations export to Excel with all required fields in under 30 seconds for 1,000 activities, under 3 minutes for 10,000 activities.
- **SC-009**: Downstream scheduling pipeline can consume duration output without data loss or transformation errors.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The Duration Estimator Plugin depends on the Som3a.Domain (Phase 14) for domain entities (Activity, Quantity, Calendar) and Som3a.AI (Phase 18) for AI-powered suggestions and anomaly detection.
- Activity input data (quantities, units, descriptions) will come from the BOQ Activity Generator (Phase 20) or direct Excel import via existing VSTO functionality.
- Productivity benchmarks are initialized from industry-standard construction productivity references but can be fully customized by end users.
- Weather and site condition factors are represented as simple percentage modifiers; complex multi-factor adjustment models are out of scope for v1.
- Learning curve factors follow a standard logarithmic model with user-configurable learning rate percentage per activity cycle.
- AI suggestions are advisory only — the final productivity rate decision always rests with the planning engineer.
- The plugin registers its pages in the Shell sidebar under the "Planning" category following Phase 15 conventions.
- Excel VSTO rendering safety rules apply to all plugin UI rendered inside the Excel host window.
- Project calendar configuration supports a single default calendar per project; multi-calendar scenarios are out of scope for v1.
- Duration estimates are auto-saved on every input change; no manual save or versioning mechanism is provided in v1.
- Maximum supported activity count is 10,000 per project; projects exceeding this count require batching into multiple project files.
- Role references (planning engineer, project manager) are narrative only — no feature access control or permission system is implemented.
