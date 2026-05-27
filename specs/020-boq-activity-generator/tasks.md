---

description: "Task list for Phase 20: BOQ Activity Generator"
---

# Tasks: BOQ Activity Generator

**Input**: Design documents from `specs/020-boq-activity-generator/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF Host**: `WpfApp2/`
- **Domain Library**: `Som3a.Domain/`
- **AI Library**: `Som3a.AI/`
- **Tests**: `Som3a.Domain.Tests/`, `Som3a.AI.Tests/`, `WpfApp2.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for the BOQ Activity Generator plugin

- [ ] T001 Create BOQ Activity Generator plugin page scaffolding: `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` and `WpfApp2/Pages/BOQActivityGeneratorPage.xaml.cs` following existing page patterns (ModernWindow, DynamicResource, Shell workspace)
- [ ] T002 [P] Create BOQ Activity Generator ViewModel: `WpfApp2/ViewModels/BOQActivityGeneratorViewModel.cs` inheriting from ViewModelBase with INotifyPropertyChanged
- [ ] T003 [P] Create Activities domain directory in `Som3a.Domain/Activities/` with Activity.cs and ActivityId.cs domain entities
- [ ] T004 [P] Create ActivityValidationResult entity in `Som3a.Domain/Activities/ActivityValidationResult.cs`
- [ ] T005 [P] Create BOQ-to-activity prompt template in `Som3a.AI/Prompts/BoqActivityPrompt.cs` registered with PromptTemplateRegistry
- [ ] T006 Register BOQ Activity Generator module in `WpfApp2/CompositionRoot.cs` via ModuleRegistry

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 [P] Implement BOQContextBuilder service in `WpfApp2/Services/BOQContextBuilder.cs` — reads BOQ from active Excel workbook, identifies BOQ sheet, parses items with quantities/units/classifications
- [ ] T008 [P] Implement data privacy consent dialog in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` — opt-in before first AI generation each session
- [ ] T009 [P] Create BOQContext model class in `Som3a.Domain/Activities/BOQContext.cs` with Items, ItemCount, TotalQuantity, IsTruncated fields
- [ ] T010 [P] Create ValidationIssue and ValidationStatus enums in `Som3a.Domain/Activities/`
- [ ] T011 Wire up BOQ preview display in BOQActivityGeneratorPage — show parsed BOQ items in read-only grid after reading workbook

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Generate Activities from BOQ Using AI (Priority: P1) 🎯 MVP

**Goal**: A planning engineer loads BOQ from Excel, triggers AI generation, and receives a structured list of construction activities with intelligent grouping, verb-noun naming, and BOQ references.

**Independent Test**: Load an Excel workbook with a valid BOQ sheet, trigger AI generation, verify output is a list of activities each with Activity ID, Name, Description, BOQ Reference, Quantity, and Unit.

### Implementation for User Story 1

- [ ] T012 [P] [US1] Create IActivityGenerationService interface in `WpfApp2/Services/IActivityGenerationService.cs`
- [ ] T013 [P] [US1] Implement ActivityGenerationService in `WpfApp2/Services/ActivityGenerationService.cs` — calls Phase 18 OrchestrationEngine with BOQ context prompt, parses activity list from AI response
- [ ] T014 [P] [US1] Create GeneratedActivity model class in `Som3a.Domain/Activities/GeneratedActivity.cs` with ActivityId, Name, Description, BoqReferences, Quantity, Unit, ValidationStatus, IsUserModified fields
- [ ] T015 [P] [US1] Implement activity mapping/grouping logic in `Som3a.Domain/Activities/ActivityGroupingRule.cs` — groups similar BOQ items into single activities
- [ ] T016 [P] [US1] Add non-blocking progress status display in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` — estimated time message during AI generation
- [ ] T017 [US1] Wire AI generation trigger in BOQActivityGeneratorViewModel — handle "Generate" command, call ActivityGenerationService, populate GeneratedActivity list
- [ ] T018 [US1] Implement 10-second cooldown between generations in BOQActivityGeneratorViewModel — warn user if triggered before cooldown expires
- [ ] T019 [US1] Implement user edit preservation across re-generation in ActivityGenerationService — match by BOQ reference, preserve user-modified fields
- [ ] T020 [US1] Display generated activities in review grid in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Review and Edit Generated Activities (Priority: P1)

**Goal**: After AI generation, the engineer is presented with an interactive validation grid showing all activities. Validation checks run automatically: duplicate detection, naming convention compliance, missing BOQ references, quantity consistency. Errors are highlighted inline with actionable messages.

**Independent Test**: Generate activities from a BOQ containing known issues (duplicate items, inconsistent quantities), verify the validation grid highlights each issue and the engineer can correct them inline.

### Implementation for User Story 2

- [ ] T021 [P] [US2] Create IActivityValidationService interface in `WpfApp2/Services/IActivityValidationService.cs`
- [ ] T022 [P] [US2] Implement ActivityValidationService in `WpfApp2/Services/ActivityValidationService.cs` — four-rule validation pipeline: duplicate detection, naming convention regex, missing BOQ reference, quantity consistency
- [ ] T023 [P] [US2] Add naming convention regex pattern in `Som3a.Domain/Activities/ActivityNamingRule.cs` — verb-noun format validation
- [ ] T024 [P] [US2] Add inline validation error/warning display in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` — per-row highlighting with actionable messages
- [ ] T025 [P] [US2] Add editable DataGrid columns for Name, Description, Quantity, Unit in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml`
- [ ] T026 [US2] Implement in-place cell editing in BOQActivityGeneratorViewModel — handle cell value changes, update GeneratedActivity properties
- [ ] T027 [US2] Implement re-run validation on edit in BOQActivityGeneratorViewModel — debounced validation on each cell edit
- [ ] T028 [US2] Implement merge/remove flow for duplicate activities in BOQActivityGeneratorViewModel

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Sequence Activities and Suggest Dependencies (Priority: P2)

**Goal**: The engineer sequences generated activities by WBS hierarchy and trade order. The system detects parallel vs sequential work and suggests basic dependencies with confidence indicators.

**Independent Test**: Generate activities from a multi-trade BOQ, verify the system suggests at least 5 logical dependencies and the engineer can accept/reject each one.

### Implementation for User Story 3

- [ ] T029 [P] [US3] Create IActivitySequencingService interface in `WpfApp2/Services/IActivitySequencingService.cs`
- [ ] T030 [P] [US3] Implement ActivitySequencingService in `WpfApp2/Services/ActivitySequencingService.cs` — order by WBS hierarchy + trade sequence heuristic
- [ ] T031 [P] [US3] Create ActivitySequenceOrder model in `Som3a.Domain/Activities/ActivitySequenceOrder.cs`
- [ ] T032 [P] [US3] Create ActivityDependency model in `Som3a.Domain/Activities/ActivityDependency.cs` — PredecessorId, SuccessorId, Type, Confidence, Rationale, IsAccepted
- [ ] T033 [US3] Implement dependency suggestion engine in ActivitySequencingService — trade-precedence knowledge base with confidence indicators
- [ ] T034 [US3] Add sequencing view in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` — show ordered activities with parallel groups
- [ ] T035 [US3] Add accept/modify/reject per-dependency UI in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml`
- [ ] T036 [US3] Implement dependency persistence across re-generation in ActivityGenerationService — preserve accepted dependencies for matched activities

**Checkpoint**: User Stories 1-3 should now be independently functional

---

## Phase 6: User Story 4 - Export Generated Activities to Excel (Priority: P2)

**Goal**: After review and sequencing, the engineer exports the final activity list to a dedicated Excel sheet within the same workbook, with theme-consistent styling.

**Independent Test**: Generate and sequence activities, export to Excel, verify the sheet contains all expected columns (ID, Name, Description, BOQ Reference, Quantity, Unit, Dependencies) with correct data and proper styling.

### Implementation for User Story 4

- [ ] T037 [P] [US4] Create IActivityExportService interface in `WpfApp2/Services/IActivityExportService.cs`
- [ ] T038 [P] [US4] Implement ActivityExportService in `WpfApp2/Services/ActivityExportService.cs` — export to Excel via existing interop, create "Generated Activities" sheet
- [ ] T039 [P] [US4] Create ActivityExportConfig model in `WpfApp2/Models/ActivityExportConfig.cs` — TargetSheetName, Columns, IncludeDependencies, OverwriteExisting, ThemeColors
- [ ] T040 [P] [US4] Add export button and configuration options in `WpfApp2/Pages/BOQActivityGeneratorPage.xaml`
- [ ] T041 [US4] Wire export trigger in BOQActivityGeneratorViewModel — call ActivityExportService with current activity list and config
- [ ] T042 [US4] Implement theme-consistent Excel styling in ActivityExportService — apply DynamicResource-derived colors to cell formatting
- [ ] T043 [US4] Implement overwrite confirmation when export sheet already exists in BOQActivityGeneratorViewModel

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T044 [P] Register all services (BOQContextBuilder, ActivityGenerationService, ActivityValidationService, ActivitySequencingService, ActivityExportService) in `WpfApp2/CompositionRoot.cs`
- [ ] T045 [P] Add error handling for all AI provider failures — user-friendly messages in BOQActivityGeneratorViewModel
- [ ] T046 [P] Add cancellation support for AI generation in BOQActivityGeneratorViewModel — cancel button during generation
- [ ] T047 [P] Add logging for all AI generation operations (request size, response time, activity count) via existing DiagnosticsService
- [ ] T048 [P] Add empty-state handling in BOQActivityGeneratorPage — guidance when no BOQ loaded or AI unavailable
- [ ] T049 [P] Run quickstart.md validation — verify all acceptance scenarios
- [ ] T050 Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration, Excel rendering safety, and WindowChrome inheritance
- [ ] T051 Build validation — full solution build with `MSBuild.exe WpfApp2/Som3a_WPF_UI.csproj /p:Configuration=Debug`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (P1) must be completed before US2 (P1), US3 (P2), US4 (P2)
  - US2 (P1) must be completed before US3 (P2) and US4 (P2)
  - US3 (P2) must be completed before US4 (P2) (dependencies in export column)
  - US4 (P2) has no downstream dependencies
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories (MVP scope)
- **User Story 2 (P1)**: Depends on US1 — needs generated activities to validate
- **User Story 3 (P2)**: Depends on US1 + US2 — needs validated activities to sequence
- **User Story 4 (P2)**: Depends on US1 + US2 + US3 — needs sequenced activities to export

### Within Each User Story

- Models before services
- Services before UI integration
- UI integration before story completion
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Models within a story marked [P] can run in parallel
- Multiple services within a story marked [P] can run in parallel
- Different user stories must be sequential (each depends on prior stories)

---

## Parallel Example: User Story 1

```bash
# Launch all models for User Story 1 together:
Task: "Create IActivityGenerationService interface in WpfApp2/Services/IActivityGenerationService.cs"
Task: "Create GeneratedActivity model in Som3a.Domain/Activities/GeneratedActivity.cs"
Task: "Create ActivityGroupingRule in Som3a.Domain/Activities/ActivityGroupingRule.cs"
```

## Parallel Example: User Story 2

```bash
# Launch all models and UI for User Story 2 together:
Task: "Create IActivityValidationService interface in WpfApp2/Services/IActivityValidationService.cs"
Task: "Create ActivityNamingRule in Som3a.Domain/Activities/ActivityNamingRule.cs"
Task: "Add inline validation error display in WpfApp2/Pages/BOQActivityGeneratorPage.xaml"
Task: "Add editable DataGrid columns in WpfApp2/Pages/BOQActivityGeneratorPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently (load BOQ, generate activities, display grid)
5. Deploy/demo if ready — AI-powered activity generation is the core value

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (AI generation) → Test independently → **MVP delivered**
3. Add User Story 2 (validation grid) → Test independently → Enhanced quality
4. Add User Story 3 (sequencing) → Test independently → Structured schedule
5. Add User Story 4 (Excel export) → Test independently → Complete workflow

### Sequential Team Strategy

With a single developer, the recommended order is:

1. Complete Setup + Foundational together (Phase 1 + 2)
2. User Story 1 (P1) — AI generation
3. User Story 2 (P1) — Validation and editing
4. User Story 3 (P2) — Sequencing
5. User Story 4 (P2) — Export
6. Polish (Phase 7)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- All new UI must use DynamicResource for themeable properties per Constitution §III
- All new services must be registered via ServiceContainer/CompositionRoot
- Excel operations must be safe within VSTO host — no blocking UI thread
