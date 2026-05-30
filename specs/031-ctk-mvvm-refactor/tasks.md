# Tasks: CommunityToolkit.Mvvm ViewModel Refactor

**Input**: Design documents from `/specs/031-ctk-mvvm-refactor/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: Tests are NOT explicitly requested for this mechanical refactor. Validation is via build checks and VSTO smoke test.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create automated verification tooling before migration begins

- [x] T001 [P] Create `WpfApp2/Scripts/Verify-NoManualMvvmPatterns.ps1` automated verification script that scans ViewModels for forbidden manual patterns

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Migrate the base ViewModel class so all derived ViewModels can use source generation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T002 Migrate `WpfApp2/ViewModels/ViewModelBase.cs` to inherit `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` and remove manual `INotifyPropertyChanged` implementation

**Checkpoint**: Foundation ready — `ViewModelBase` compiles and all derived classes can now be migrated

---

## Phase 3: User Story 1 - Eliminate Manual UI Boilerplate (Priority: P1) 🎯 MVP

**Goal**: Replace all manual property change notification boilerplate with `[ObservableProperty]` source generation across all 20+ ViewModels, migrated in incremental batches

**Independent Test**: Build succeeds after each batch; `*.g.cs` artifacts present in `obj/`; zero manual `OnPropertyChanged` calls remain in migrated files

### Batch 1 — Core Infrastructure (Low Risk)

- [x] T003 [P] [US1] Migrate `WpfApp2/ViewModels/HomeViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`; refactor any custom setter side effects into `OnPropertyChanged` partial method hooks
- [x] T004 [P] [US1] Migrate `WpfApp2/ViewModels/ToastViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`; refactor any custom setter side effects into partial method hooks
- [x] T005 [US1] Build validation after Batch 1 — run `dotnet build WpfApp2/Som3a_WPF_UI.csproj` and verify `*.g.cs` files appear in `obj/Debug/net8.0-windows/`

### Batch 2 — Dashboard Widgets (Isolated, Testable)

- [x] T006 [P] [US1] Migrate all `WpfApp2/ViewModels/Dashboard/*WidgetViewModel.cs` files to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T007 [US1] Build validation after Batch 2 — verify compilation and `*.g.cs` generation for all Dashboard ViewModels

### Batch 3 — Settings-Related ViewModels

- [x] T008 [P] [US1] Migrate `WpfApp2/ViewModels/SettingsViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T009 [P] [US1] Migrate `WpfApp2/ViewModels/LanguagePageViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T010 [P] [US1] Migrate `WpfApp2/ViewModels/DiagnosticsViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T011 [US1] Build validation after Batch 3 — verify compilation and `*.g.cs` generation

### Batch 4 — Planning Page ViewModels

- [x] T012 [P] [US1] Migrate `WpfApp2/ViewModels/BOQActivityGeneratorViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T013 [P] [US1] Migrate `WpfApp2/ViewModels/DurationEstimatorPageViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T014 [P] [US1] Migrate `WpfApp2/ViewModels/WBSEditorViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T015 [US1] Build validation after Batch 4 — verify compilation and `*.g.cs` generation

### Batch 5 — Remaining ViewModels

- [x] T016 [P] [US1] Migrate `WpfApp2/ViewModels/WBSGeneratorViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T017 [P] [US1] Migrate `WpfApp2/ViewModels/RelationshipGeneratorViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T018 [P] [US1] Migrate `WpfApp2/ViewModels/ShellViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T019 [P] [US1] Migrate `WpfApp2/ViewModels/CommandPaletteViewModel.cs` to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T020 [P] [US1] Migrate all remaining `WpfApp2/ViewModels/Primavera/*ViewModel.cs` files to `partial class` with `[ObservableProperty]` and `[RelayCommand]`
- [x] T021 [US1] Build validation after Batch 5 — verify all remaining ViewModels compile and produce `*.g.cs` artifacts

**Checkpoint**: At this point, all ViewModels use source-generated properties and commands. User Story 1 is complete.

---

## Phase 4: User Story 2 - Standardize Command Binding Generation (Priority: P2)

**Goal**: Remove all obsolete manual command helper classes from the codebase

**Independent Test**: `Models/RelayCommand.cs` and `Helpers/AsyncRelayCommand.cs` are deleted; build succeeds; zero references to old command classes remain

- [x] T022 [P] [US2] Delete `WpfApp2/Models/RelayCommand.cs`
- [x] T023 [P] [US2] Delete `WpfApp2/Helpers/AsyncRelayCommand.cs`
- [x] T024 [US2] Fix any remaining references to `RelayCommand` or `AsyncRelayCommand` from `Models/` or `Helpers/` namespaces across the entire codebase
- [x] T025 [US2] Build validation after command cleanup — verify zero compilation errors and all commands still functional

**Checkpoint**: All manual command helper classes removed. User Story 2 is complete.

---

## Phase 5: User Story 3 - Preserve Application Behavior (Priority: P3)

**Goal**: Confirm zero behavioral regressions after the refactor

**Independent Test**: VSTO smoke test passes — ribbon visible, ShellWindow opens, sidebar renders, 3 pages navigable, theme switches, Excel cell write, no crashes

- [ ] T026 [US3] Run `WpfApp2/Scripts/Verify-NoManualMvvmPatterns.ps1` and confirm zero forbidden patterns (manual `OnPropertyChanged`, old `RelayCommand` references, non-partial ViewModels)
- [ ] T027 [US3] Run standard VSTO smoke test protocol: ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes

**Checkpoint**: Zero regressions confirmed. User Story 3 is complete.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation updates and final compliance checks

- [ ] T028 [P] Update `Docs/Architecture/MVVM_RULES.md` to document source-generated `[ObservableProperty]` and `[RelayCommand]` as the mandated pattern
- [ ] T029 [P] Update `Docs/Architecture/MVVM_COMPLIANCE.md` with new checklist items for partial classes, `ObservableObject` inheritance, and absence of manual command helpers
- [ ] T030 Constitution compliance review — verify no XAML changes introduced, no theme mutations, no window/WindowChrome changes, no inline effects
- [ ] T031 Run `quickstart.md` validation — confirm the migration guide accurately reflects the implemented patterns and file paths

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion (T002). Batches are sequential (build validation gates each batch), but individual ViewModels within a batch can be parallelized.
- **User Story 2 (Phase 4)**: Depends on User Story 1 completion (all ViewModels migrated). Can start as soon as T021 passes.
- **User Story 3 (Phase 5)**: Depends on User Story 2 completion (obsolete files deleted). Can start as soon as T025 passes.
- **Polish (Phase 6)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2). No dependencies on other stories. Is the MVP.
- **User Story 2 (P2)**: Can start after User Story 1. Requires all ViewModels migrated before deleting shared helpers.
- **User Story 3 (P3)**: Can start after User Story 2. Smoke test validates the entire refactor.

### Within Each Batch (User Story 1)

- Individual ViewModel migrations marked [P] can run in parallel (different files, no interdependencies)
- Build validation MUST run after all migrations in the batch complete
- Next batch cannot start until previous batch build validation passes

### Parallel Opportunities

- T001 (Setup) can run in parallel with any other task until it is needed for T026
- All ViewModel migrations within a batch marked [P] can run in parallel
- T022 and T023 (file deletions) can run in parallel
- T028 and T029 (documentation updates) can run in parallel
- With a team of 3+ developers: assign Batch 1-2 to Dev A, Batch 3-4 to Dev B, Batch 5 to Dev C, all after T002 completes

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each batch or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- If a batch build fails, fix on that branch before proceeding to next batch
