# Tasks: Relationship Generator Plugin

**Input**: Design documents from `specs/001-relationship-generator-plugin/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: NOT REQUESTED — spec.md does not request test tasks. Skip all test sections.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Domain layer**: `Som3a.Domain/Relationships/`
- **Validation layer**: `Som3a.Validation/Relationships/`
- **AI layer**: `Som3a.AI/Prompts/`, `Som3a.AI/Parsers/`
- **WPF layer**: `WpfApp2/Pages/`, `WpfApp2/ViewModels/`, `WpfApp2/Services/`, `WpfApp2/Controls/`
- **Tests**: `Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization — register plugin page and service interfaces in the shell

- [x] T001 Create Relationship Generator plugin page placeholder in `WpfApp2/Pages/RelationshipGeneratorPage.xaml` with empty grid layout, registered under "Planning" sidebar category via existing plugin registration mechanism
- [x] T002 [P] Register `IRelationshipGenerationService`, `IRelationshipValidationService`, `IRelationshipAnalysisService`, and `IRelationshipExportService` interfaces in `WpfApp2/CompositionRoot.cs` as singleton registrations
- [x] T003 [P] Register Relationship Generator plugin module with `IModuleRegistry` in `WpfApp2/CompositionRoot.cs` with priority order after Phase 20 module

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain entity enhancements and graph container — MUST be complete before any user story can begin

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 [P] Enhance `Relationship` entity in `Som3a.Domain/Relationships/Relationship.cs` — add `Rationale` (string), `Confidence` (RelationshipConfidence enum), `IsAccepted` (bool), `IsUserModified` (bool), `GeneratedAt` (DateTime?) fields; update `Validate()` method; create `RelationshipConfidence` enum (High/Medium/Low)
- [x] T005 [P] Create `RelationshipNetwork` graph container in `Som3a.Domain/Relationships/RelationshipNetwork.cs` with adjacency list, reverse adjacency list, `Build()`, `GetTopologicalOrder()`, `GetConnectedComponents()`, `HasCycles()` methods

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Generate Activity Relationships from AI (Priority: P1) 🎯 MVP

**Goal**: Generate predecessor/successor relationships from a list of construction activities using AI analysis of trade sequence, space constraints, and resource flow

**Independent Test**: Provide a known activity list (20 construction activities with clear trade sequences) and verify generated relationships match expected FS/SS/FF/SF patterns with at least 80% coverage

### Implementation for User Story 1

- [x] T006 [P] [US1] Create `RelationshipPrompt` template in `Som3a.AI/Prompts/RelationshipPrompt.cs` with structured instruction for AI to output JSON containing relationship type (FS/SS/FF/SF), lag, rationale, and confidence for each relationship pair
- [x] T007 [P] [US1] Enhance `RelationshipParser` in `Som3a.AI/Parsers/RelationshipParser.cs` to parse richer structured output including rationale, confidence, and lag values into `Relationship` domain entities
- [x] T008 [US1] Create `IRelationshipGenerationService` interface in `WpfApp2/Services/IRelationshipGenerationService.cs` with `GenerateRelationshipsAsync`, `RegenerateRelationshipsAsync`, `GetFallbackRelationshipsAsync` methods matching contracts specification
- [x] T009 [US1] Implement `RelationshipGenerationService` in `WpfApp2/Services/RelationshipGenerationService.cs` — AI prompt context building, first-attempt generation, auto-retry with simplified prompt on failure, fallback to empty manual mode on second failure
- [x] T010 [P] [US1] Create `RelationshipEditorGrid` user control in `WpfApp2/Controls/RelationshipEditorGrid.xaml` and `.xaml.cs` — DataGrid with ComboBox columns for predecessor/successor (dropdown of activities), type (FS/SS/FF/SF), lag (numeric spinner); per-row Accept/Reject toggle; Add/Remove row buttons; DynamicResource-only theming
- [x] T011 [US1] Create `RelationshipGeneratorViewModel` in `WpfApp2/ViewModels/RelationshipGeneratorViewModel.cs` — commands for Generate, Accept/Reject, Add/Remove rows; observable collection of relationships; progress status; AI availability check; implements `INotifyPropertyChanged`
- [x] T012 [US1] Create `RelationshipGeneratorPage` in `WpfApp2/Pages/RelationshipGeneratorPage.xaml` and `.xaml.cs` — plugin page integrating the editor grid, generate button, progress indicator; code-behind UI-only (binding to ViewModel)
- [x] T013 [US1] Implement edit preservation in `RelationshipGenerationService.RegenerateRelationshipsAsync` — match relationships by (predecessorId, successorId) pair across re-generation; preserve `IsAccepted`, `IsUserModified`, user-modified Type and Lag

**Checkpoint**: At this point, User Story 1 should be fully functional — engineer can load activities, trigger AI generation, view/edit/accep/reject relationships in the grid, and re-run generation without losing edits

---

## Phase 4: User Story 2 — Validate Relationships for Errors (Priority: P1)

**Goal**: Validate the logic network for cycles, open-ended activities, dangling links, and redundant relationships; produce a structured validation report

**Independent Test**: Introduce known errors (cycles, open ends, danglings, duplicates) into a relationship set and verify the validation report catches every type of error

### Implementation for User Story 2

- [x] T014 [P] [US2] Create `ValidationReport` and `NetworkValidationIssue` entities in `Som3a.Domain/Relationships/ValidationReport.cs` — report contains issues list, HasErrors, HasWarnings, TotalIssueCount, ValidatedAt; issue contains IssueType (NetworkIssueType), Severity, Message, AffectedActivityIds, AffectedRelationshipIds, Details
- [x] T015 [P] [US2] Create `NetworkIssueType` enum in `Som3a.Domain/Relationships/` — values: CircularDependency, OpenStart, OpenEnd, DanglingActivity, RedundantRelationship, LagOutOfRange
- [x] T016 [P] [US2] Create `OpenEndDetector` in `Som3a.Validation/Relationships/OpenEndDetector.cs` — identifies open-start (in-degree=0) and open-end (out-degree=0) activities using the `RelationshipNetwork` adjacency lists
- [x] T017 [P] [US2] Create `NetworkValidator` in `Som3a.Validation/Relationships/NetworkValidator.cs` — orchestrates DependencyValidator, LoopDetector, OpenEndDetector, plus inline redundant detection and dangling detection; produces `ValidationReport`; all rules run independently (comprehensive report)
- [x] T018 [US2] Create `IRelationshipValidationService` interface in `WpfApp2/Services/IRelationshipValidationService.cs` with `ValidateNetworkAsync` and `ValidateSingleRelationshipAsync` methods
- [x] T019 [US2] Implement `RelationshipValidationService` in `WpfApp2/Services/RelationshipValidationService.cs` — calls `NetworkValidator`, returns `ValidationReport`; debounced per-relationship validation for inline grid editing (300ms)
- [x] T020 [US2] Integrate validation results into `RelationshipGeneratorViewModel` — display validation report summary (error/warning counts), highlight invalid rows in the editor grid, show issue details per row

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — engineer can generate relationships AND validate them, seeing all issues in the validation report

---

## Phase 5: User Story 3 — Analyze Parallel Execution Paths (Priority: P2)

**Goal**: Identify independent work streams, parallel execution groups, critical path, and resource conflicts through the relationship network

**Independent Test**: Provide a known activity network with two independent chains (A→B→C, D→E→F) and verify the system correctly identifies independent work streams and the critical path

### Implementation for User Story 3

- [x] T021 [P] [US3] Create `ParallelExecutionGroup` entity in `Som3a.Domain/Relationships/ParallelExecutionGroup.cs` — GroupId, ActivityIds[], Reason, TopologicalLevel
- [x] T022 [P] [US3] Create `CriticalPathResult` entity in `Som3a.Domain/Relationships/CriticalPathResult.cs` — Path[], TotalDuration, CriticalActivities, TotalActivities, CriticalityIndex
- [x] T023 [P] [US3] Create `ResourceConflict` entity and `ConflictSeverity` enum in `Som3a.Domain/Relationships/ResourceConflict.cs` — ActivityIds[], ResourceType, Severity (Warning/Critical), Message
- [x] T024 [US3] Create `IRelationshipAnalysisService` interface in `WpfApp2/Services/IRelationshipAnalysisService.cs` with `AnalyzeParallelGroupsAsync`, `AnalyzeCriticalPathAsync`, `DetectResourceConflictsAsync` methods
- [x] T025 [US3] Implement `RelationshipAnalysisService` in `WpfApp2/Services/RelationshipAnalysisService.cs` — parallel groups by topological sort levels, critical path by DP longest-path on DAG, resource conflicts by shared ResourceType within parallel groups
- [x] T026 [US3] Integrate analysis results into `RelationshipGeneratorViewModel` — add "Analyze" command; display parallel groups, critical path activities, and resource conflicts in the UI (read-only summary sections below the editor grid)

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T027 [P] Create `IRelationshipExportService` interface and `RelationshipExportService` implementation in `WpfApp2/Services/` — export accepted relationships to active Excel sheet as new columns (Predecessor, Successor, Type, Lag) using existing VSTO interop
- [x] T028 [P] Add "Export to Excel" button and command to `RelationshipGeneratorViewModel` and `RelationshipGeneratorPage`
- [x] T029 Add user-facing status messages and error handling across all three services (AI failure notification, validation summary notification, analysis completion notification)
- [x] T030 Constitution compliance review — verify DynamicResource-only usage in `RelationshipEditorGrid.xaml`, no inline effects, no direct ThemeManager bypass, Excel save uses existing interop

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion — MVP (P1)
- **User Story 2 (Phase 4)**: Depends on Foundational completion — P1 (can run in parallel with US1 if staffed separately since validation uses different files: `Som3a.Validation/` vs `Som3a.AI/` + `WpfApp2/`)
- **User Story 3 (Phase 5)**: Depends on Foundational completion — P2 (can run in parallel with US1/US2 since it uses separate service interface and UI sections)
- **Polish (Phase 6)**: Depends on at least US1 being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational — Independently testable with synthetic error sets; integrates with US1's grid for inline validation display
- **User Story 3 (P2)**: Can start after Foundational — Independently testable with known network; integrates with US1's ViewModel for analysis display

### Within Each User Story

- Models/services before UI
- Interfaces before implementations
- Core implementation before integration into ViewModel/Page
- Story complete before moving to next priority

### Parallel Opportunities

- All [P] marked tasks within a phase can run in parallel (different files, no dependencies)
- US1 and US2 can start in parallel after Foundational (different files: AI prompts/parsers/services vs validation entities/detectors)
- US3 can start after Foundational independently

---

## Parallel Example: User Story 1

```bash
# Launch all [P] tasks for User Story 1 together:
Task: "Create RelationshipPrompt in Som3a.AI/Prompts/RelationshipPrompt.cs"
Task: "Enhance RelationshipParser in Som3a.AI/Parsers/RelationshipParser.cs"
Task: "Create RelationshipEditorGrid in WpfApp2/Controls/"

# Then sequential:
Task: "Create IRelationshipGenerationService interface"
Task: "Implement RelationshipGenerationService"
Task: "Create RelationshipGeneratorViewModel"
Task: "Create RelationshipGeneratorPage"
Task: "Implement edit preservation"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (3 tasks)
2. Complete Phase 2: Foundational (2 tasks — CRITICAL)
3. Complete Phase 3: User Story 1 (8 tasks)
4. **STOP and VALIDATE**: Load 20+ activities → Generate AI relationships → Review in grid → Accept/reject → Verify FS/SS/FF/SF types and lag values
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → **Deploy/Demo (MVP!)**
3. Add User Story 2 → Test independently with synthetic error sets → Deploy/Demo
4. Add User Story 3 → Test independently with known network → Deploy/Demo
5. Add Polish (Excel export) → Final validation

### Parallel Team Strategy

With multiple developers:

1. Complete Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (AI generation + grid editor)
   - Developer B: User Story 2 (validation detectors + service)
   - Developer C: User Story 3 (analysis service — can start later as P2)
3. Stories complete and integrate independently into shared ViewModel

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests NOT requested in spec — test sections omitted from all phases
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same-file conflicts, cross-story dependencies that break independence
