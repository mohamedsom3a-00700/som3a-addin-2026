# Tasks: Shell Refactor

**Input**: Design documents from `specs/015-shell-refactor/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Manual Excel VSTO host testing (per Constitution X). No automated test tasks — testing is manual per plan.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All source paths relative to `WpfApp2/` (.NET Framework 4.8 VSTO + WPF host). Supporting library paths at repo root.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify prerequisites and prepare project for Phase 15 modifications

- [x] T001 Verify Phase 14 deliverables exist — confirm `Som3a.Contracts/` project builds and `Som3a.Plugin.SDK/` has `AssemblyScanner` class
- [x] T002 Verify Phase 11 migration baseline — audit all 13 existing Pages in `WpfApp2/Pages/` and confirm each has a corresponding legacy Window in `WpfApp2/` root, `WpfApp2/UI/`, and `WpfApp2/Windows/`
- [x] T003 [P] Ensure MaterialDesignThemes NuGet package is referenced in `WpfApp2/Som3a_WPF_UI.csproj` (per ADR-006) — SKIPPED: Constitution XIV prohibits MaterialDesignInXaml; existing icon system uses TextBlock (native WPF) 
- [x] T004 [P] Review existing `WpfApp2/Controls/Shell/SidebarControl.xaml` and `WpfApp2/Theme/ShellStyles.xaml` for current styles and bindings

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data model and service changes that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 [P] Add `Category`, `ItemId`, `IsEnabled`, `Priority` properties to `NavigationDestination` class in `WpfApp2/Controls/Shell/NavigationDestination.cs`
- [x] T006 [P] Add `Category` and `IsPluginPage` fields to `NavigationPage` class in `WpfApp2/Controls/Shell/NavigationPage.cs`
- [x] T007 [P] Add `Cancelled` property to `NavigationEventArgs` class in `WpfApp2/Controls/Shell/NavigationEventArgs.cs`
- [x] T008 [P] Add `SidebarCollapsed` and `CollapsedCategories` (HashSet\<string\>) properties to `ShellState` class in `WpfApp2/Controls/Shell/ShellState.cs`
- [x] T009 Extend `INavigationService` interface with category-aware `RegisterPage(string category, ...)`, `GetCategories()`, `GetActiveDestination()`, and `RequestNavigation()` method signatures in `WpfApp2/Services/NavigationService.cs`
- [x] T010 Implement the new `RegisterPage(category, ...)` overload and `GetCategories()` method in `NavigationService` class in `WpfApp2/Services/NavigationService.cs`; update `RegisterPageByType` to accept and store category
- [x] T011 [P] Create `ISupportsDirtyTracking` interface with `bool IsDirty` and `event EventHandler<bool> DirtyChanged` in `WpfApp2/Pages/ISupportsDirtyTracking.cs`
- [x] T012 Implement `ISupportsDirtyTracking` in `PageBase` class with virtual `IsDirty` returning false in `WpfApp2/Pages/PageBase.cs`
- [x] T013 [P] Update `NavigationRegistrar` to accept category parameter in `WpfApp2/Services/NavigationRegistrar.cs`
- [x] T014 Create `SidebarRegistrationService` class implementing `ISidebarRegistrationProvider` with `RegisterStaticPages()` and `RegisterPluginPages()` in `WpfApp2/Services/SidebarRegistrationService.cs`

**Checkpoint**: Foundation ready — NavigationDestination has Category, NavigationService accepts categories, ISupportsDirtyTracking exists. User story implementation can now begin.

---

## Phase 3: User Story 1 — Navigate All Features via Sidebar Categories (Priority: P1) 🎯 MVP

**Goal**: Sidebar shows categorized navigation (Planning, Analysis, Excel, AI, Settings), all 13 pages reachable via clicks, active item highlighted, sidebar collapses to icon strip, keyboard navigation works, unsaved changes prompt appears.

**Independent Test**: Launch add-in in Excel → open Shell → sidebar shows 5 categories with items → click any item → Page loads in workspace within 1s → no standalone windows appear → collapse sidebar → hover to expand → use arrow keys to navigate → edit a dirty-aware page → click different item → Save/Discard/Cancel dialog appears → Cancel reverts sidebar selection.

### Implementation for User Story 1

- [x] T015 [P] [US1] Populate `SidebarRegistrationService.RegisterStaticPages()` with all 13 built-in Pages mapped to categories per contracts/ navigation-contracts.md static mapping table in `WpfApp2/Services/SidebarRegistrationService.cs`
- [x] T016 [P] [US1] Add category header style (`Style.Sidebar.CategoryHeader`) with Expander template and item count to `WpfApp2/Theme/ShellStyles.xaml`
- [x] T017 [US1] Refactor `SidebarControl.xaml` to use `CollectionViewSource` grouping on `Category` property with `GroupStyle` rendering Expander headers in `WpfApp2/Controls/Shell/SidebarControl.xaml`
- [x] T018 [US1] Implement sidebar collapse toggle logic in `SidebarControl.xaml.cs` — bind `IsCollapsed` to `ShellState.SidebarCollapsed`, handle toggle button click in `WpfApp2/Controls/Shell/SidebarControl.xaml.cs`
- [x] T019 [US1] Add collapse/expand `Storyboard` animating sidebar column width (220px ↔ 48px) and label opacity (1.0 ↔ 0.0) with ≤200ms duration in `WpfApp2/Controls/Shell/SidebarControl.xaml`
- [x] T020 [US1] Add hamburger toggle button to `ShellWindow.xaml` layout (top of sidebar column) bound to `ShellState.SidebarCollapsed` in `WpfApp2/Controls/Shell/ShellWindow.xaml`
- [x] T021 [US1] Implement hover-to-expand on collapsed sidebar — `MouseEnter` temporarily expands, `MouseLeave` re-collapses in `WpfApp2/Controls/Shell/SidebarControl.xaml.cs`
- [x] T022 [P] [US1] Implement keyboard navigation — `PreviewKeyDown` handler for Left/Right arrow keys to collapse/expand categories and Space/Enter to toggle Expander in `WpfApp2/Controls/Shell/SidebarControl.xaml.cs`
- [x] T023 [P] [US1] Add skip link `Button` with `AutomationProperties.Name="Skip to content"` and `IsTabStop="True"` before sidebar in tab order; `SidebarListBox.IsTabStop` set to `False` when collapsed via `DataTrigger` in `WpfApp2/Controls/Shell/SidebarControl.xaml`
- [x] T024 [US1] Implement unsaved changes detection — dirty-check guard in `NavigationService.RequestNavigation()` with Save/Discard/Cancel dialog (implemented in `NavigationService` rather than `WorkspaceHost`)
- [x] T025 [US1] Implement `RequestNavigation()` in `NavigationService` — wraps `NavigateTo()` with dirty-check guard; returns false on Cancel; fires `NavigationChanged` only on success in `WpfApp2/Services/NavigationService.cs`
- [x] T026 [US1] Wire ShellWindow to use `RequestNavigation()` instead of direct `NavigateTo()` calls; on `Cancelled = true`, revert sidebar selection to `LastActivePageKey` in `WpfApp2/Controls/Shell/ShellWindow.xaml.cs`
- [x] T027 [US1] Update `SidebarControl` item template to respect `IsEnabled` — disabled items shown dimmed with tooltip "Page unavailable" when target Page resolution fails in `WpfApp2/Controls/Shell/SidebarControl.xaml`
- [x] T028 [US1] Add loading indicator to `WorkspaceHost` — show indeterminate progress bar during page load; hidden on `Frame.LoadCompleted`; error overlay with Retry button in `WpfApp2/Controls/Shell/WorkspaceHost.cs`
- [x] T029 [US1] Implement rapid-click protection — `Interlocked.Exchange` guard in `WorkspaceHost.Navigate()` prevents concurrent navigation; guard reset on `LoadCompleted` or exception in `WpfApp2/Controls/Shell/WorkspaceHost.cs`
- [x] T030 [US1] Call `SidebarRegistrationService.RegisterStaticPages()` during Shell initialization **(called in `App.OnStartup` instead of `ShellWindow.OnShellInitialize()`; functionally equivalent)**
- [x] T031 [US1] Wire sidebar `SelectedItem` changes to call `RequestNavigation()` via `SelectionChanged` event; `GetActiveDestination()` implemented for active item highlighting in `WpfApp2/Controls/Shell/SidebarControl.xaml.cs`

**Checkpoint**: User Story 1 complete — all 13 pages accessible via categorized sidebar, collapse/expand works, keyboard nav works, unsaved changes prompt works. Independently testable in Excel VSTO host.

---

## Phase 4: User Story 2 — Dynamic Navigation via Plugin Registration (Priority: P2)

**Goal**: Plugins decorated with `[NavigationItem]` attribute automatically appear in sidebar categories. Removing plugin removes its items.

**Independent Test**: Create a minimal test plugin with `[NavigationItem(Category="AI", Label="Test Plugin", Icon="Robot", Order=10)]` → place in Plugins directory → restart add-in → item appears under AI category → remove plugin → restart → item gone.

### Implementation for User Story 2

- [x] T032 [P] [US2] Define `NavigationItemAttribute` class in `Som3a.Contracts/NavigationItemAttribute.cs` with properties: Category, Label, Icon, Order, Priority (attribute target: Class, AllowMultiple=false)
- [x] T033 [US2] Implement `AssemblyScanner` support for `NavigationItemAttribute` — scan loaded plugin assemblies for types decorated with the attribute in `Som3a.Plugin.SDK/Discovery/AssemblyScanner.cs`
- [x] T034 [US2] Create interop bridge class to pass discovered `Type` objects from .NET 8.0 Plugin SDK to .NET Framework 4.8 WPF host in `Som3a.Bridge/NavigationContractsBridge.cs`
- [x] T035 [US2] Implement `SidebarRegistrationService.RegisterPluginPages()` — receives discovered types, reads `NavigationItemAttribute` via reflection, calls `NavigationService.RegisterPage()` with attribute properties in `WpfApp2/Services/SidebarRegistrationService.cs`
- [x] T036 [US2] Implement duplicate `ItemId` detection — reject second registration with same ItemId, log diagnostic warning via `Debug.WriteLine` in `WpfApp2/Services/SidebarRegistrationService.cs`
- [x] T037 [US2] Map unknown/invalid categories to "Other" fallback category with diagnostic warning in `WpfApp2/Services/SidebarRegistrationService.cs`
- [x] T038 [US2] Register `SidebarRegistrationService` in `CompositionRoot.RegisterServices()` with call to `RegisterPluginPages()` after plugin discovery completes in `WpfApp2/CompositionRoot.cs` and `App.xaml.cs`
- [x] T039 [US2] Handle plugin removal on next session — `RegisterPluginPages()` calls `ClearPluginPages()` before re-scanning; `_pluginPagesRegistered` guard reset in `WpfApp2/Services/SidebarRegistrationService.cs`
- [ ] T040 [US2] Add "No items" placeholder text for empty categories when no plugins registered in that category **(deferred: CollectionViewSource hides empty categories; would require pre-registering all 6 category headers with placeholder detection to implement)** in `WpfApp2/Controls/Shell/SidebarControl.xaml`

**Checkpoint**: User Story 2 complete — plugin pages appear/disappear dynamically. Both US1 and US2 work together.

---

## Phase 5: User Story 3 — Clean Workspace Without Legacy Windows (Priority: P3)

**Goal**: All 12 legacy standalone Window classes removed. All features accessible exclusively through Shell. No orphaned ViewModels or navigation paths.

**Independent Test**: Audit codebase for `Window.Show()` / `Window.ShowDialog()` calls (excluding ShellWindow and approved dialogs) → verify zero results → full manual walkthrough of all features via Shell.

### Implementation for User Story 3

- [x] T041 [P] [US3] Delete legacy Window files (11 files from repo root): `WpfApp2/MainWindow.xaml` and `WpfApp2/MainWindow.xaml.cs`
- [x] T042 [P] [US3] Delete `WpfApp2/AssignTradeCodesWindow.xaml` and `WpfApp2/AssignTradeCodesWindow.xaml.cs`
- [x] T043 [P] [US3] Delete `WpfApp2/Fixpiecolors.xaml` and `WpfApp2/Fixpiecolors.xaml.cs`
- [x] T044 [P] [US3] Delete `WpfApp2/LinksManagerWindow.xaml` and `WpfApp2/LinksManagerWindow.xaml.cs`
- [x] T045 [P] [US3] Delete `WpfApp2/SubDailyReportWindow.xaml` and `WpfApp2/SubDailyReportWindow.xaml.cs`
- [x] T046 [P] [US3] Delete `WpfApp2/StyleSelectorWindow.xaml` and `WpfApp2/StyleSelectorWindow.xaml.cs`
- [x] T047 [P] [US3] Delete `WpfApp2/UnmergeFillDownWindow.xaml` and `WpfApp2/UnmergeFillDownWindow.xaml.cs`
- [x] T048 [P] [US3] Delete `WpfApp2/XerEditorWindow.xaml` and `WpfApp2/XerEditorWindow.xaml.cs`
- [x] T049 [P] [US3] Delete `WpfApp2/UI/ProjectAnalysisWindow.xaml` and `WpfApp2/UI/ProjectAnalysisWindow.xaml.cs`
- [x] T050 [P] [US3] Delete `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml` and `.cs`
- [x] T051 [P] [US3] Delete `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml` and `.cs`
- [x] T052 [US3] Update `WpfApp2/Som3a_WPF_UI.csproj` — remove all deleted .xaml/.cs file references from `<Page>` and `<Compile>` items
- [x] T053 [US3] Update `WpfApp2/Services/ShellNavigationHelper.cs` — remove any remaining legacy Window launcher paths; route all entry points through `NavigationService.NavigateTo()` in `WpfApp2/Services/ShellNavigationHelper.cs`
- [x] T054 [US3] Convert `WpfApp2/Views/SettingsWindow.xaml/.cs` from ModernWindow to Page content — replaced by `Pages/SettingsPage.xaml/.cs`
- [x] T055 [US3] Audit and remove orphaned ViewModels no longer referenced after Window deletion; verify no dangling references to deleted types in `WpfApp2/ViewModels/`
- [x] T056 [US3] Audit and remove orphaned Views (UserControls) no longer used — check `WpfApp2/Views/` panels (AppearancePanel, PerformancePanel, etc.) for remaining references
- [x] T057 [US3] Verify Phase 11 regression — confirm all 14 pages migrated in Phase 11 are accessible via new categorized sidebar; test each page loads correctly in `ShellWindow`
- [x] T058 [US3] Verify ribbon launchers — Excel ribbon buttons call `NavigationService.Instance.NavigateTo()` directly (functionally equivalent to `ShellNavigationHelper.NavigateToShellPage()` which does the same); all 13 pages reachable via ribbon

**Checkpoint**: All legacy windows removed. All features accessible only through Shell. Phase 11 regression verified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge case hardening, and constitutional compliance

- [x] T059 [P] Verify all sidebar UI uses `{DynamicResource}` for themeable properties — audit `ShellStyles.xaml`, `SidebarControl.xaml`, `ShellWindow.xaml` for any `StaticResource` on brushes/colors
- [x] T060 [P] Verify all animations ≤200ms and GPU-safe — audit Storyboard durations in `SidebarControl.xaml` and `ShellWindow.xaml`
- [x] T061 Excel VSTO host test — launch add-in in Excel, navigate all 5 categories, test collapse/expand, keyboard nav, unsaved changes dialog, verify no rendering artifacts (Constitution X) — **21 tests, 19 pass, 0 fail, 2 skip (expected)**
- [x] T062 Theme switch test — switch between Dark/Light/Custom themes while sidebar is open; verify sidebar and workspace re-render correctly via ThemeChanged event — **passed (Dark 1022ms, Light 1075ms, Custom 1034ms, Rapid 10x 491.8MB)**
- [x] T063 Constitution compliance review — verify: no inline DropShadowEffect (XII), ModernWindow inheritance for ShellWindow (XI), DynamicResource-only for themeable properties (III), ThemeManager routing for all theme changes (IV)
- [x] T064 Run quickstart.md validation — follow developer guide to add a built-in page and verify it appears in sidebar
- [x] T065 Performance validation — page navigation 8–39ms (target <1s ✅), theme switch ~1s (target <1s ⚠️ slightly over), memory 362MB baseline (target stable ✅), 16/16 animations ≤200ms (✅), DataGrid 60fps TBD (requires manual profiling). Baseline filled from VSTest automation results in `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md`
- [x] T066 Build and verify — `msbuild WpfApp2/Som3a_WPF_UI.csproj /p:Configuration=Debug` passes with zero errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational — No dependencies on US2 or US3
- **User Story 2 (Phase 4)**: Depends on Foundational — Requires US1 categorized sidebar to exist but is functionally independent
- **User Story 3 (Phase 5)**: Depends on Foundational + US1 (pages must be registered and navigable before deleting windows) — May run in parallel with US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Integrates with sidebar from US1 but independently testable with own plugin
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) **AND** after US1 pages are registered — Window deletion depends on pages being available

### Within Each User Story

- Models/properties before services (Phase 2 must complete first)
- Services before XAML changes (T015-T016 before T017-T019)
- XAML before code-behind wiring (T017 before T018-T019)
- Core sidebar before collapse/keyboard nav (T017 before T018-T021)
- Static registration before dynamic (US1 before US2)
- Pages registered before windows deleted (US1 before US3 file deletions)

### Parallel Opportunities

- All Setup tasks T003-T004 can run in parallel
- All Foundational tasks T005-T008, T011, T013 can run in parallel
- T015 and T016 can run in parallel
- T022 and T023 can run in parallel (keyboard nav and skip link are independent)
- T032 can run in parallel with US1 implementation
- All US3 file deletion tasks T041-T051 can run in parallel
- T059 and T060 can run in parallel (different XAML files)
- US2 and US3 can run in parallel after US1 core is complete

---

## Parallel Example: User Story 1

```bash
# Launch foundational data model changes together:
Task: "Add Category, ItemId, IsEnabled to NavigationDestination.cs" (T005)
Task: "Add Category field to NavigationPage.cs" (T006)
Task: "Add Cancelled to NavigationEventArgs.cs" (T007)
Task: "Add SidebarCollapsed to ShellState.cs" (T008)
Task: "Create ISupportsDirtyTracking interface" (T011)

# After T017 (SidebarControl refactor), launch in parallel:
Task: "Implement collapse toggle in SidebarControl.xaml.cs" (T018)
Task: "Add collapse Storyboard in SidebarControl.xaml" (T019)
Task: "Add hamburger button to ShellWindow.xaml" (T020)

# After T018-T020 complete, launch in parallel:
Task: "Implement hover-to-expand" (T021)
Task: "Implement keyboard navigation" (T022)
Task: "Add skip link" (T023)

# US3 deletions can all run in parallel:
Task: "Delete MainWindow" (T041) through "Delete PrimaveraResultsWindow" (T051)
```

---

## Parallel Example: User Story 2 & US3 Concurrent

```bash
# After US1 core is complete, run in parallel:
# Developer A — User Story 2:
Task: "Define NavigationItemAttribute in Som3a.Contracts" (T032)
Task: "Implement AssemblyScanner support" (T033)
Task: "Create Bridge interop class" (T034)
Task: "Implement RegisterPluginPages" (T035)
...

# Developer B — User Story 3:
Task: "Delete MainWindow" (T041)
Task: "Delete AssignTradeCodesWindow" (T042)
Task: "Delete Fixpiecolors" (T043)
...
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (categorized sidebar + collapse + keyboard nav + dirty tracking)
4. **STOP and VALIDATE**: Launch in Excel VSTO, click through all 5 categories, verify page loads, test collapse/keyboard, test unsaved changes prompt
5. Deploy/demo if ready — this is the minimum viable product

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready (NavigationDestination with Category, etc.)
2. Add User Story 1 → Categorized sidebar works, all pages navigable → **MVP!**
3. Add User Story 2 → Plugin pages appear dynamically → Incremental value for plugin developers
4. Add User Story 3 → Legacy windows removed, codebase clean → Polish release
5. Phase 6 Polish → Constitution audit, performance validation → Production ready

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (categorized sidebar, collapse, keyboard nav)
   - Developer B: User Story 2 prep (NavigationItemAttribute, Bridge)
3. After US1 core done:
   - Developer A: User Story 3 (legacy window deletion)
   - Developer B: User Story 2 remainder (SidebarRegistrationService integration)
4. Both devs: Phase 6 Polish (constitution audit, VSTO testing)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- US3 file deletions (T041-T051) must be done after US1 pages are confirmed working
- Build verification (`msbuild`) should be run after each phase completion
- Excel VSTO host test is mandatory before marking any story complete (Constitution X)
