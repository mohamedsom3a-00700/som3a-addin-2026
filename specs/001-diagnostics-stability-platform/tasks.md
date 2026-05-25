# Tasks: Diagnostics & Stability Platform

**Input**: Design documents from `/specs/001-diagnostics-stability-platform/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: No TDD requested — test tasks are not included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- All paths relative to `WpfApp2/` within repository root
- NuGet-free: all code uses .NET Framework 4.8 built-in types only

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create shared DTOs and model files used by all user stories

- [X] T001 Create DiagnosticsModels.cs with DiagnosticSnapshot, ValidationResult, LogEntry, and FallbackManifest DTOs in WpfApp2/Models/DiagnosticsModels.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Logging infrastructure shared across all user stories

- [X] T002 [P] Create ILoggingService interface and LoggingService implementation in WpfApp2/Services/LoggingService.cs — file-based structured logging with auto-rotation (5 MB max, 3 rotated files)
- [X] T003 [P] Register LoggingService in CompositionRoot.cs service container
- [X] T004 Add logging calls to ThemeManager.cs (log theme switches and resource failures via LoggingService)

**Checkpoint**: Foundation ready — logging works and ThemeManager logs events

---

## Phase 3: User Story 1 — Support Staff Diagnose Rendering Issues (Priority: P1)

**Goal**: Diagnostics panel displaying render mode, GPU, theme, memory, and popup health

**Independent Test**: Open diagnostics panel in three scenarios (FallbackSafe, WindowChrome, unknown host) and confirm accurate display

### Implementation for User Story 1

- [X] T005 [P] [US1] Create IDiagnosticsService interface and DiagnosticsService implementation in WpfApp2/Services/DiagnosticsService.cs — aggregates render mode, GPU, theme, memory, popup status into DiagnosticSnapshot
- [X] T006 [P] [US1] Extend IRenderModeService with GetPopupDiagnostics() in WpfApp2/Services/RenderModeService.cs — subscribe to ComboBox.DropDownOpened, verify AllowsTransparency=False, detect clipping
- [X] T007 [P] [US1] Create DiagnosticsViewModel in WpfApp2/ViewModels/DiagnosticsViewModel.cs — exposes DiagnosticSnapshot, auto-refresh timer (≤5s interval), RunValidationCommand placeholder
- [X] T008 [US1] Add Diagnostics section/tab to SettingsWindow.xaml — displays render mode, GPU, theme, memory, popup health from DiagnosticsViewModel
- [X] T009 [US1] Wire DiagnosticsViewModel to SettingsWindow.xaml.cs — constructor injection via ServiceContainer, data binding
- [X] T010 [US1] Register IDiagnosticsService, DiagnosticsViewModel in CompositionRoot.cs

**Checkpoint**: User Story 1 complete — diagnostics panel shows live rendering state

---

## Phase 4: User Story 2 — Developers Validate Resource Integrity (Priority: P1)

**Goal**: Run validation scans that detect missing tokens, inline colors, duplicate styles, and invalid resources

**Independent Test**: Introduce a known violation (hardcoded color in a control template), run validation, confirm it is reported

### Implementation for User Story 2

- [X] T011 [P] [US2] Create IValidationEngine interface and ValidationEngine implementation in WpfApp2/Services/ValidationEngine.cs — scan Application.Current.Resources.MergedDictionaries for: missing DynamicResource keys, hardcoded colors, duplicate styles, invalid resource references
- [X] T012 [P] [US2] Register IValidationEngine in CompositionRoot.cs
- [X] T013 [P] [US2] Wire RunValidationCommand to DiagnosticsViewModel — connects to ValidationEngine.RunValidation(), disables button while scanning
- [X] T014 [US2] Add validation results display to SettingsWindow.xaml Diagnostics section — list of ValidationResult items with severity, category, location, description shown in a DataGrid or ListView
- [X] T015 [US2] Log all validation results via ILoggingService when scan completes

**Checkpoint**: User Story 2 complete — validation scan runs and results display in diagnostics panel

---

## Phase 5: User Story 3 — Application Recovers Gracefully from Resource Failures (Priority: P2)

**Goal**: Crash-safe theme loading with fallback to hardcoded safe defaults when resource dictionaries fail

**Independent Test**: Corrupt a resource dictionary file, restart application, confirm fallback theme loads without crash

### Implementation for User Story 3

- [X] T016 [P] [US3] Add IsFallbackActive property and FallbackManifest to ThemeManager.cs — track fallback state, failed dictionaries, failure reasons
- [X] T017 [US3] Add crash-safe try/catch around ResourceDictionary loading in ThemeManager.ApplyTheme() — on single dictionary failure, skip and log; on all failures, load hardcoded inline fallback theme
- [X] T018 [US3] Create hardcoded inline fallback resource dictionary in ThemeManager.cs — minimal set of essential brushes (Background, Text, Accent) using black/white values, no external file dependency
- [X] T019 [P] [US3] Add user-visible notification when fallback mode activates — use existing ToastWindow or a StatusBar message
- [X] T020 [P] [US3] Integrate IsFallbackMode into DiagnosticsViewModel — display fallback status in diagnostics panel
- [X] T021 [US3] Log all resource failures via ILoggingService during crash-safe loading

**Checkpoint**: User Story 3 complete — corrupted dictionaries trigger fallback without crash

---

## Phase 6: User Story 4 — Support Staff Review Application Logs (Priority: P3)

**Goal**: Access persisted log entries from within the diagnostics panel

**Independent Test**: Perform actions (theme switch, validation run), open log viewer, confirm events recorded with timestamps

### Implementation for User Story 4

- [X] T022 [P] [US4] Add GetRecentEntries() to ILoggingService/LoggingService — reads last N entries from current log file
- [X] T023 [US4] Add log viewer section to SettingsWindow.xaml Diagnostics tab — displays recent LogEntry items (timestamp, severity, category, message) in a read-only DataGrid or ListView
- [X] T024 [US4] Wire LoggingService.GetRecentEntries() to DiagnosticsViewModel — exposes RecentLogs collection
- [X] T025 [US4] Add "View Logs" toggle or tab within diagnostics panel to switch between live diagnostics view and log viewer

**Checkpoint**: User Story 4 complete — logs accessible from diagnostics panel

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge case handling, and documentation

- [X] T026 [P] Handle edge case: diagnostics panel opens but RenderModeService unavailable — display "N/A" for all values
- [X] T027 [P] Handle edge case: corrupted log files or disk full — LoggingService silently swallows write failures (never crash)
- [X] T028 [P] Handle edge case: validation encounters unreadable ResourceDictionary — skip and log, continue scanning
- [X] T029 [P] Handle edge case: ALL theme dictionaries missing — fallback theme activates, notification displayed
- [X] T030 [P] Handle edge case: memory usage query fails — display "N/A" in diagnostics panel
- [X] T031 Run quickstart.md validation — build passes, all diagnostic scenarios verified in Excel VSTO
- [X] T032 Constitution compliance review — verify DynamicResource-only usage, no inline effects, ThemeManager routing, Excel rendering safety

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — provides LoggingService used by US2, US3, US4
- **US1 (Phase 3)**: Depends on Setup — no dependency on Foundational (no logging needed)
- **US2 (Phase 4)**: Depends on US1 (shares diagnostics panel UI) + Foundational (logging)
- **US3 (Phase 5)**: Depends on Foundational (logging for resource failures)
- **US4 (Phase 6)**: Depends on Foundational (LoggingService) + US1 (diagnostics panel UI)
- **Polish (Phase 7)**: Depends on all desired user stories complete

### User Story Dependencies

- **US1 (P1)**: After Setup — independent, no dependencies on other stories
- **US2 (P1)**: After US1 (uses diagnostics panel) + Foundational (logging)
- **US3 (P2)**: After Foundational (logging) — independent of US1/US2
- **US4 (P3)**: After US1 (diagnostics panel) + Foundational (LoggingService)

### Parallel Opportunities

- T002, T003 can run in parallel (LoggingService, CompositionRoot registration)
- T005, T006, T007 can run in parallel (DiagnosticsService, RenderModeService, DiagnosticsViewModel)
- T011, T012 can run in parallel (ValidationEngine, CompositionRoot registration)
- T016, T019, T020 can run in parallel (ThemeManager extensions, notification, ViewModel integration)
- T022 can start before US1 if LoggingService is done
- All edge case handlers (T026-T030) can run in parallel

---

## Parallel Example: User Story 1

```powershell
# Launch all service/model tasks for User Story 1 together:
Task: "Create IDiagnosticsService + DiagnosticsService in WpfApp2/Services/DiagnosticsService.cs"
Task: "Extend RenderModeService with GetPopupDiagnostics() in WpfApp2/Services/RenderModeService.cs"
Task: "Create DiagnosticsViewModel in WpfApp2/ViewModels/DiagnosticsViewModel.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 Only — Both P1)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (optional for US1 — skip if USA1 needed standalone)
3. Complete Phase 3: User Story 1 (diagnostics panel)
4. Complete Phase 4: User Story 2 (validation engine)
5. **STOP and VALIDATE**: Test US1 + US2 independently
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (diagnostics panel) → Test independently → Deploy/Demo (MVP cut 1)
3. Add US2 (validation engine) → Test independently → Deploy/Demo (MVP cut 2)
4. Add US3 (crash-safe loading) → Test independently → Deploy/Demo
5. Add US4 (log viewer) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- No NuGet dependencies — all code uses .NET Framework 4.8 built-in types
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
