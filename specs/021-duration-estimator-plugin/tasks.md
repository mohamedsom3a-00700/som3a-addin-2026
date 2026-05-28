# Tasks: Duration Estimator Plugin

**Input**: Design documents from `specs/021-duration-estimator-plugin/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL — only include if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Plugin source**: `Plugins/DurationEstimator/src/`
- **Plugin tests**: `Plugins/DurationEstimator/tests/`
- **Shell UI**: `WpfApp2/Pages/` (if Shell page needed)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create `Plugins/DurationEstimator/` directory structure per plan.md
- [x] T002 [P] Create `Plugins/DurationEstimator/src/DurationEstimator.csproj` with .NET 8.0 class library target and project references (Som3a.Domain, Som3a.Contracts, Som3a.Plugin.SDK, Som3a.AI, Som3a.Exporting, Som3a.Infrastructure)
- [x] T003 [P] Create `Plugins/DurationEstimator/tests/DurationEstimator.UnitTests.csproj` with xunit and project reference to DurationEstimator
- [x] T004 [P] Add both projects to the existing solution via `dotnet sln add`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core engine that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Create `ProductivityEngine` in `Plugins/DurationEstimator/src/Engine/ProductivityEngine.cs` with `CalculateWorkingDays(decimal quantity, decimal productivityRate, int crewSize, decimal hoursPerDay)` implementing the core formula
- [x] T006 [P] Create `CrewSizeFactor` in `Plugins/DurationEstimator/src/Engine/CrewSizeFactor.cs` with crew size adjustment logic
- [x] T007 Create plugin entry point class `DurationEstimatorPlugin` in `Plugins/DurationEstimator/src/DurationEstimatorPlugin.cs` with `[Plugin]`, `[NavigationItem]`, `[SettingsSection]` attributes implementing IPlugin lifecycle (Initialize, Register, LoadUI, RegisterSettings)
- [x] T008 Create `CalendarConfig` model in `Plugins/DurationEstimator/src/Calendar/CalendarConfig.cs` with WorkingDays, Holidays, HoursPerDay, StartDate fields — needed by all calendar-aware calculations

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Basic Duration Calculation (Priority: P1) 🎯 MVP

**Goal**: Planning engineer imports/enters activities with quantities, provides productivity rate/crew/hours, and gets instant working-day duration results that export to Excel and feed the scheduling pipeline.

**Independent Test**: Import a list of activities with quantities, apply productivity rate (10), crew size (2), and hours per day (8). Verify duration = Quantity / (10 * 2 * 8) working days. Change any input and verify duration updates instantly (<500ms). Export to Excel and verify columns.

### Implementation for User Story 1

- [x] T009 [P] [US1] Create `DurationCalculator` in `Plugins/DurationEstimator/src/Engine/DurationCalculator.cs` that combines ProductivityEngine with activity inputs and produces DurationEstimate records
- [x] T010 [P] [US1] Create `DurationExporter` in `Plugins/DurationEstimator/src/Export/DurationExporter.cs` — Excel sheet export via Som3a.Exporting ExcelExportEngine with activity ID, quantity, productivity rate, crew size, hours per day, duration columns
- [x] T011 [P] [US1] Create `SchedulingPipelineWriter` in `Plugins/DurationEstimator/src/Export/SchedulingPipelineWriter.cs` — produces Primavera-compatible JSON payload per contracts/scheduling-pipeline-contract.md
- [x] T012 [US1] Implement `IDurationEstimatorService` in `Plugins/DurationEstimator/src/DurationEstimatorService.cs` orchestrating import → calculate → export flow with live recalculation on input changes
- [x] T013 [US1] Implement `DurationEstimatorPageViewModel` in `WpfApp2/ViewModels/DurationEstimatorPageViewModel.cs` — commands for import, rate/crew/hours input, live recalculation binding
- [x] T014 [US1] Implement `DurationEstimatorPage` in `WpfApp2/Pages/DurationEstimatorPage.xaml` + code-behind — WPF Shell page with activity grid, input fields, export buttons, binding to ViewModel
- [x] T015 [US1] Add FR-014 logging in `DurationEstimatorService` — log all productivity rate changes, recalculations, and user overrides
- [x] T016 [US1] Handle edge cases: zero quantity (flag invalid), zero crew size/hours (flag invalid), extremely large quantities (warn >5yr duration)

**Checkpoint**: User Story 1 is fully functional — user can import activities, input rates/crew/hours, see instant durations, and export to Excel and scheduling pipeline.

---

## Phase 4: User Story 2 - Productivity Benchmark Database (Priority: P2)

**Goal**: Planning engineer browses built-in productivity benchmarks by trade category, selects one to auto-fill rate/crew, and creates custom benchmarks.

**Independent Test**: Browse benchmark library, filter by "concrete" category, select a benchmark — verify rate and crew auto-fill into the activity. Create a custom benchmark, save it, then select it from the library. Verify built-in and custom benchmarks coexist.

### Implementation for User Story 2

- [x] T017 [P] [US2] Create `TradeCategoryRegistry` in `Plugins/DurationEstimator/src/Benchmarks/TradeCategoryRegistry.cs` with built-in categories (concrete, steel, masonry, MEP, finishes, earthwork, etc.)
- [x] T018 [P] [US2] Create `BenchmarkData.json` embedded resource in `Plugins/DurationEstimator/src/Benchmarks/BenchmarkData.json` with 50+ built-in productivity rates across 5+ trade categories
- [x] T019 [US2] Create `ProductivityBenchmarkLibrary` in `Plugins/DurationEstimator/src/Benchmarks/ProductivityBenchmarkLibrary.cs` — loads built-in benchmarks from embedded JSON, manages user-custom benchmarks, persists to `%AppData%/Som3a/DurationEstimator/benchmarks.json`
- [x] T020 [US2] Add benchmark selection UI to `DurationEstimatorPageViewModel` — browse, filter by trade category, select benchmark → auto-fills rate and crew size
- [x] T021 [US2] Add custom benchmark management UI to `DurationEstimatorPage` — add, edit, delete custom benchmarks dialog
- [x] T022 [US2] Handle edge cases: overlapping benchmarks (show all matching, let user choose), benchmark not found for unit of measure (fall back to manual input)

**Checkpoint**: Users can select benchmarks from the library and create custom ones. US1 integration: selecting a benchmark auto-fills the rate and triggers live recalculation.

---

## Phase 5: User Story 3 - Calendar-Aware Duration Scheduling (Priority: P2)

**Goal**: Planning engineer configures a project calendar and durations display as calendar dates skipping non-working days and holidays.

**Independent Test**: Configure Monday-Friday 8h/day with a holiday on July 4th. Set start date to Monday June 29th. Calculate a 10-working-day duration — verify end date is Tuesday July 14th (skipping July 4th holiday and 2 weekends).

### Implementation for User Story 3

- [x] T023 [P] [US3] Create `CalendarEngine` in `Plugins/DurationEstimator/src/Calendar/CalendarEngine.cs` — `CalculateEndDate(startDate, workingDays)`, `IsWorkingDay(date)`, `CountWorkingDays(start, end)` using CalendarConfig
- [x] T024 [US3] Integrate `CalendarEngine` into `DurationCalculator` — duration result now includes `CalendarDurationDays` and `EndDate` fields
- [x] T025 [US3] Add calendar configuration UI to Shell Settings (via `[SettingsSection]`) — configure working days, holidays, hours per day
- [x] T026 [US3] Update `DurationEstimatorPage` to show start date input, calendar-aware end date, and working days vs. calendar days
- [x] T027 [US3] Handle edge cases: calendar with zero working days (invalid config, show error), holiday on weekend (skip once, not twice), modified calendar recalculates all durations

**Checkpoint**: Durations now show both working days and calendar-aware end dates. Calendar config is available via plugin settings.

---

## Phase 6: User Story 4 - Variance Analysis with 3-Point Estimates (Priority: P3)

**Goal**: Project manager generates Optimistic, Most Likely, and Pessimistic durations with standard deviation and confidence intervals.

**Independent Test**: For activity with optimistic rate (15), most likely (10), pessimistic (5), quantity=100, crew=2, hours=8 — verify Optimistic=0.417d, MostLikely=0.625d, Pessimistic=1.25d, Expected≈0.69d, SD≈0.14d, 95%CI≈[0.42, 0.96].

### Implementation for User Story 4

- [x] T028 [P] [US4] Create `VarianceAnalyzer` in `Plugins/DurationEstimator/src/Variance/VarianceAnalyzer.cs` — 3-point estimation with PERT formula, standard deviation, confidence intervals (90%, 95%, 99%)
- [x] T029 [P] [US4] Create `RiskAdjuster` in `Plugins/DurationEstimator/src/Variance/RiskAdjuster.cs` — risk-adjusted duration based on configurable confidence level
- [x] T030 [US4] Add variance fields (optimistic rate, most likely rate, pessimistic rate) to `DurationEstimatorPageViewModel` — only visible when variance mode is enabled
- [x] T031 [US4] Update `DurationEstimatorPage` to show variance results panel — 3 values, expected duration, SD, confidence interval bar
- [x] T032 [US4] Handle edge cases: single rate only (use same for all 3, note low confidence), confidence level selector (90/95/99%), invalid rates (optimistic < pessimistic is impossible, show error)

**Checkpoint**: Variance analysis available as optional enhancement. Users can toggle 3-point mode and see risk metrics alongside base durations.

---

## Phase 7: User Story 5 - AI Productivity Suggestions and Anomaly Detection (Priority: P3)

**Goal**: AI suggests productivity rates based on project type and flags unusual durations that may be data entry errors.

**Independent Test**: For a residential high-rise project with 50 activities, request AI suggestions — verify suggestions return within 30s and rates are within 20% of known industry benchmarks. Intentionally set one activity's quantity 10x too high — verify anomaly detection flags it.

### Implementation for User Story 5

- [x] T033 [P] [US5] Create `AIProductivitySuggestor` in `Plugins/DurationEstimator/src/AI/AIProductivitySuggestor.cs` — implements IAIProductivitySuggestor contract, uses AIOrchestrator with prompt template "duration-productivity-suggest"
- [x] T034 [P] [US5] Create `AnomalyDetector` in `Plugins/DurationEstimator/src/AI/AnomalyDetector.cs` — statistical anomaly detection comparing each DurationEstimate against peer activities in same trade category
- [x] T035 [P] [US5] Create AI prompt template resources for "duration-productivity-suggest" and "duration-anomaly-detect" in Som3a.AI PromptTemplates or as embedded resources
- [x] T036 [US5] Add AI suggestion button to `DurationEstimatorPage` — sends activity list + project type to AIProductivitySuggestor, displays results with accept/reject per suggestion
- [x] T037 [US5] Add anomaly indicators to activity grid rows — flagged activities show icon and tooltip with explanation
- [x] T038 [US5] Handle edge cases: AI provider unavailable (show graceful message, no crash), partial suggestions (apply accepted, keep existing for rejected), all activities anomalous (warn of possible systemic data issue)

**Checkpoint**: AI features are available as optional enhancements. Suggestions are advisory only; anomaly flags help catch errors.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T039 [P] `Quickstart.md` validation — verify all paths and commands match actual project structure
- [x] T040 Verify built-in `BenchmarkData.json` contains 50+ rates across 5+ trade categories
- [x] T041 Performance profiling — verify 10,000 activities calculated within 5 minutes, live recalculation within 500ms, export 1,000 in <30s
- [x] T042 [P] Add `ProductivityModifier` support (zone/phased, weather, site conditions, learning curve) in `Plugins/DurationEstimator/src/Engine/ProductivityEngine.cs` — ApplyModifiers() with additive percentage model
- [x] T043 [P] Constitution compliance review — verify DynamicResource-only in Shell pages, no inline effects, WindowChrome inheritance, Excel rendering safety
- [x] T044 Code cleanup: remove debug logging, standardize exception handling, ensure consistent naming across all files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories 1-5 can proceed sequentially (P1 → P2 → P2 → P3 → P3)
  - US2 and US3 can proceed in parallel (both P2, different domains)
  - US4 and US5 can proceed in parallel (both P3, different domains)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — No dependencies on other stories (manual rate input works without benchmarks)
- **US2 (P2)**: Can start after Foundational — Integrates with US1 (auto-fills rate/crew) but independently testable via benchmark CRUD alone
- **US3 (P2)**: Can start after Foundational — Integrates with US1 (calendar-aware dates) but independently testable via calendar config + test activities
- **US4 (P3)**: Can start after US1 (uses DurationCalculator) — Independently testable at calculation level
- **US5 (P3)**: Can start after US1 + US2 (needs activities + benchmarks for context) — AI suggestions testable with mock AI provider

### Within Each User Story

- Models before services
- Services before UI
- Core implementation before edge cases
- Story complete before moving to next priority

### Parallel Opportunities

- T002, T003, T004 (Setup) can run in parallel
- T006, T008 (Foundational) can run in parallel
- All [P]-marked tasks within each story can run in parallel
- US2 and US3 can be implemented in parallel by different developers
- US4 and US5 can be implemented in parallel by different developers

---

## Parallel Example: User Story 1

```powershell
# Launch all [P] tasks for User Story 1 together:
Task: "Create DurationCalculator in Plugins/DurationEstimator/src/Engine/DurationCalculator.cs"
Task: "Create DurationExporter in Plugins/DurationEstimator/src/Export/DurationExporter.cs"
Task: "Create SchedulingPipelineWriter in Plugins/DurationEstimator/src/Export/SchedulingPipelineWriter.cs"
```

## Parallel Example: User Stories 4 & 5

```powershell
# Launch US4 and US5 foundational [P] tasks in parallel:
Task: "Create VarianceAnalyzer in Plugins/DurationEstimator/src/Variance/VarianceAnalyzer.cs"
Task: "Create AIProductivitySuggestor in Plugins/DurationEstimator/src/AI/AIProductivitySuggestor.cs"
Task: "Create AnomalyDetector in Plugins/DurationEstimator/src/AI/AnomalyDetector.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (P1)
4. **STOP and VALIDATE**: Test User Story 1 independently — manual rate input → calculate → show working days → export to Excel
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (MVP) → Import activities, manual rates, instant calc, Excel export
3. Add User Story 2 (P2) → Benchmark library, rate selection, custom benchmarks
4. Add User Story 3 (P2) → Calendar-aware dates, holiday support, end dates
5. Add User Story 4 (P3) → Variance analysis, 3-point estimates, risk metrics
6. Add User Story 5 (P3) → AI suggestions, anomaly detection
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (MVP)
   - Developer B: User Story 2 (Benchmarks)
   - Developer C: User Story 3 (Calendar)
3. After US1 completes:
   - Developer A: User Story 4 (Variance)
   - Developer B: User Story 5 (AI)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
