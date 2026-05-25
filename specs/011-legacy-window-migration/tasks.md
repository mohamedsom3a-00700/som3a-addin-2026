# Tasks: Legacy Window Migration

**Input**: Design documents from `/specs/011-legacy-window-migration/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not applicable — manual Excel VSTO host validation per window

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shell Foundation Verification)

**Purpose**: Verify Phase 8 shell infrastructure is ready for window migrations

- [x] T001 Verify `WpfApp2/Controls/Shell/ShellWindow.xaml` and `ShellWindow.xaml.cs` exist and inherit from `ModernWindow`
- [x] T002 Verify `WpfApp2/Pages/PageBase.cs` exists and provides `OnNavigatedTo()` / `OnNavigatedFrom()` lifecycle hooks
- [x] T003 Verify `WpfApp2/Services/NavigationService.cs` exists with `RegisterPage<T>()` and `NavigateTo()` methods
- [x] T004 Verify `WpfApp2/Pages/WelcomePage.xaml` exists as reference implementation
- [x] T005 [P] Audit ribbon XML or Ribbon designer code for all window launch points (MainWindow, SettingsWindow, Float_path, etc.) — document current `new Window().Show()` calls
- [x] T006 Verify `WpfApp2/Pages/` folder structure is accessible for new Page files

**Checkpoint**: Phase 8 shell verified — window migrations can begin

---

## Phase 2: Migration Infrastructure (US4 Rollback Capability)

**Purpose**: Establish MigrationRecord tracking and validation framework

- [x] T007 Create MigrationRecord table in `specs/011-legacy-window-migration/data-model.md` with all 14 windows enumerated (WindowName, OriginalPath, PagePath, MigrationStatus, PageKey, Priority, Complexity)
- [x] T008 Create ValidationChecklistItem entries in `data-model.md` for each window with standard criteria (Page opens, DPI 100%/125%/150%, theme switch, popup rendering, keyboard nav, FallbackSafe, functional parity)
- [x] T009 Update `specs/011-legacy-window-migration/quickstart.md` MigrationRecord table with window priorities (Tier 1: SettingsWindow=1, MainWindow=2; Tier 2: ProjectAnalysisWindow=3, Float_path=4, LinksManagerWindow=5, SubDailyReportWindow=6; Tier 3: AssignTradeCodesWindow=7, PrimaveraCompareWindow=8, PrimaveraResultsWindow=9, XerEditorWindow=10; Tier 4: Fixpiecolors=11, StyleSelectorWindow=12, UnmergeFillDownWindow=13, ToastWindow=14)

**Checkpoint**: Migration tracking infrastructure established

---

## Phase 3: Tier 1 Window Migration (US1, US2, US3)

**Goal**: Migrate SettingsWindow and MainWindow — highest visibility, validation learning

### SettingsWindow Migration

- [x] T010 [P] [US1] Create `WpfApp2/Pages/SettingsPage.xaml` by extracting content from `WpfApp2/Views/SettingsWindow.xaml` (remove Window attributes, keep Grid content, update Background to Brush.Background.Primary)
- [x] T011 [P] [US1] Create `WpfApp2/Pages/SettingsPage.xaml.cs` with Page class, preserve InitializeComponent and event handlers
- [x] T012 [P] [US2] Extract SettingsWindow code-behind logic — identify event handlers, service injections, dialog calls — move to Page code-behind following Phase 6 MVVM patterns
- [x] T013 [US1] Register SettingsPage with NavigationService: `nav.RegisterPage<SettingsPage>("settings", "Settings", order: 20)` in appropriate module initialization
- [x] T014 [US1] Update ribbon launcher callback that opens SettingsWindow — replace `new SettingsWindow().Show()` with `NavigationService.Instance.NavigateTo("settings")`
- [ ] T015 [US3] Validate SettingsPage in Excel VSTO host: Page opens without black backgrounds, DPI scaling at 100%/125%/150%, theme switching updates correctly, ComboBox popups render without clipping, FallbackSafe mode renders correctly
- [ ] T016 [US2] Verify functional parity: theme changes persist, accent swatches update, all Settings operations produce identical results
- [ ] T017 [US4] Update MigrationRecord for SettingsWindow: set MigrationStatus=Validated, ValidatedBy, ValidatedDate

### MainWindow Migration

- [x] T018 [P] [US1] Create `WpfApp2/Pages/MainPage.xaml` by extracting content from `WpfApp2/MainWindow.xaml` (remove Window attributes, keep Grid content, update Background to Brush.Background.Primary)
- [x] T019 [P] [US1] Create `WpfApp2/Pages/MainPage.xaml.cs` with Page class, preserve InitializeComponent and event handlers
- [x] T020 [P] [US2] Extract MainWindow code-behind logic — identify project analysis operations, service injections — move to Page code-behind following Phase 6 MVVM patterns
- [x] T021 [US1] Register MainPage with NavigationService: `nav.RegisterPage<MainPage>("main", "Main", order: 10)` in appropriate module initialization
- [x] T022 [US1] Update ribbon launcher callback that opens MainWindow — replace `new MainWindow().Show()` with `NavigationService.Instance.NavigateTo("main")`
- [ ] T023 [US3] Validate MainPage in Excel VSTO host: Page opens without black backgrounds, DPI scaling at 100%/125%/150%, theme switching updates correctly, FallbackSafe mode renders correctly
- [ ] T024 [US2] Verify functional parity: all project analysis operations produce identical results to standalone MainWindow
- [ ] T025 [US4] Update MigrationRecord for MainWindow: set MigrationStatus=Validated, ValidatedBy, ValidatedDate
- [X] T026 [P] [US4] Preserve original `WpfApp2/MainWindow.xaml` and `WpfApp2/MainWindow.xaml.cs` — do NOT delete yet (rollback buffer)
- [X] T027 [P] [US4] Preserve original `WpfApp2/Views/SettingsWindow.xaml` and `WpfApp2/Views/SettingsWindow.xaml.cs` — do NOT delete yet (rollback buffer)

**Checkpoint**: 2 windows migrated and validated — shell navigation working

---

## Phase 4: Tier 2 Window Migration (US1, US2, US3)

**Goal**: Migrate ProjectAnalysisWindow, Float_path, LinksManagerWindow, SubDailyReportWindow

### ProjectAnalysisWindow Migration

- [x] T028 [P] [US1] Create `WpfApp2/Pages/ProjectAnalysisPage.xaml` from `WpfApp2/ProjectAnalysisWindow.xaml`
- [x] T029 [P] [US1] Create `WpfApp2/Pages/ProjectAnalysisPage.xaml.cs`
- [x] T030 [P] [US2] Extract ProjectAnalysisWindow code-behind — data grid interactions, analysis operations
- [x] T031 [US1] Register ProjectAnalysisPage with NavigationService: `nav.RegisterPage<ProjectAnalysisPage>("project-analysis", "Project Analysis", order: 30)`
- [x] T032 [US1] Update ribbon launcher for ProjectAnalysisWindow
- [ ] T033 [US3] Validate ProjectAnalysisPage in Excel VSTO host (DataGrid virtualization, theme switch, DPI scaling, FallbackSafe)
- [ ] T034 [US2] Verify functional parity for all analysis operations
- [ ] T035 [US4] Update MigrationRecord: MigrationStatus=Validated

### Float_path Migration

- [x] T036 [P] [US1] Create `WpfApp2/Pages/FloatPathPage.xaml` from `WpfApp2/Float_path.xaml`
- [x] T037 [P] [US1] Create `WpfApp2/Pages/FloatPathPage.xaml.cs`
- [x] T038 [P] [US2] Extract Float_path code-behind logic
- [x] T039 [US1] Register FloatPathPage: `nav.RegisterPage<FloatPathPage>("float-path", "Float Path", order: 40)`
- [x] T040 [US1] Update ribbon launcher for Float_path
- [ ] T041 [US3] Validate FloatPathPage in Excel VSTO host
- [ ] T042 [US2] Verify functional parity
- [ ] T043 [US4] Update MigrationRecord: MigrationStatus=Validated

### LinksManagerWindow Migration

- [x] T044 [P] [US1] Create `WpfApp2/Pages/LinksManagerPage.xaml` from `WpfApp2/LinksManagerWindow.xaml`
- [x] T045 [P] [US1] Create `WpfApp2/Pages/LinksManagerPage.xaml.cs`
- [x] T046 [P] [US2] Extract LinksManagerWindow code-behind logic
- [x] T047 [US1] Register LinksManagerPage: `nav.RegisterPage<LinksManagerPage>("links-manager", "Links Manager", order: 50)`
- [x] T048 [US1] Update ribbon launcher for LinksManagerWindow
- [ ] T049 [US3] Validate LinksManagerPage in Excel VSTO host
- [ ] T050 [US2] Verify functional parity
- [ ] T051 [US4] Update MigrationRecord: MigrationStatus=Validated

### SubDailyReportWindow Migration

- [x] T052 [P] [US1] Create `WpfApp2/Pages/SubDailyReportPage.xaml` from `WpfApp2/SubDailyReportWindow.xaml`
- [x] T053 [P] [US1] Create `WpfApp2/Pages/SubDailyReportPage.xaml.cs`
- [x] T054 [P] [US2] Extract SubDailyReportWindow code-behind logic
- [x] T055 [US1] Register SubDailyReportPage: `nav.RegisterPage<SubDailyReportPage>("sub-daily-report", "Sub Daily Report", order: 60)`
- [x] T056 [US1] Update ribbon launcher for SubDailyReportWindow
- [ ] T057 [US3] Validate SubDailyReportPage in Excel VSTO host
- [ ] T058 [US2] Verify functional parity
- [ ] T059 [US4] Update MigrationRecord: MigrationStatus=Validated

**Checkpoint**: 6 windows migrated and validated

---

## Phase 5: Tier 3 Window Migration (US1, US2, US3)

**Goal**: Migrate AssignTradeCodesWindow, PrimaveraCompareWindow, PrimaveraResultsWindow, XerEditorWindow

### AssignTradeCodesWindow Migration

- [x] T060 [P] [US1] Create `WpfApp2/Pages/AssignTradeCodesPage.xaml` from `WpfApp2/AssignTradeCodesWindow.xaml`
- [x] T061 [P] [US1] Create `WpfApp2/Pages/AssignTradeCodesPage.xaml.cs`
- [x] T062 [P] [US2] Extract AssignTradeCodesWindow code-behind logic
- [x] T063 [US1] Register AssignTradeCodesPage: `nav.RegisterPage<AssignTradeCodesPage>("assign-trade-codes", "Assign Trade Codes", order: 70)`
- [x] T064 [US1] Update ribbon launcher for AssignTradeCodesWindow
- [ ] T065 [US3] Validate AssignTradeCodesPage in Excel VSTO host
- [ ] T066 [US2] Verify functional parity
- [ ] T067 [US4] Update MigrationRecord: MigrationStatus=Validated

### PrimaveraCompareWindow Migration

- [x] T068 [P] [US1] Create `WpfApp2/Pages/PrimaveraComparePage.xaml` from `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml`
- [x] T069 [P] [US1] Create `WpfApp2/Pages/PrimaveraComparePage.xaml.cs`
- [x] T070 [P] [US2] Extract PrimaveraCompareWindow code-behind logic
- [x] T071 [US1] Register PrimaveraComparePage: `nav.RegisterPage<PrimaveraComparePage>("primavera-compare", "Primavera Compare", order: 80)`
- [x] T072 [US1] Update ribbon launcher for PrimaveraCompareWindow
- [ ] T073 [US3] Validate PrimaveraComparePage in Excel VSTO host
- [ ] T074 [US2] Verify functional parity
- [ ] T075 [US4] Update MigrationRecord: MigrationStatus=Validated

### PrimaveraResultsWindow Migration

- [x] T076 [P] [US1] Create `WpfApp2/Pages/PrimaveraResultsPage.xaml` from `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml`
- [x] T077 [P] [US1] Create `WpfApp2/Pages/PrimaveraResultsPage.xaml.cs`
- [x] T078 [P] [US2] Extract PrimaveraResultsWindow code-behind logic
- [x] T079 [US1] Register PrimaveraResultsPage: `nav.RegisterPage<PrimaveraResultsPage>("primavera-results", "Primavera Results", order: 90)`
- [x] T080 [US1] Update ribbon launcher for PrimaveraResultsWindow
- [ ] T081 [US3] Validate PrimaveraResultsPage in Excel VSTO host
- [ ] T082 [US2] Verify functional parity
- [ ] T083 [US4] Update MigrationRecord: MigrationStatus=Validated

### XerEditorWindow Migration

- [x] T084 [P] [US1] Create `WpfApp2/Pages/XerEditorPage.xaml` from `WpfApp2/XerEditorWindow.xaml`
- [x] T085 [P] [US1] Create `WpfApp2/Pages/XerEditorPage.xaml.cs`
- [x] T086 [P] [US2] Extract XerEditorWindow code-behind — XER file editing operations
- [x] T087 [US1] Register XerEditorPage: `nav.RegisterPage<XerEditorPage>("xer-editor", "XER Editor", order: 100)`
- [x] T088 [US1] Update ribbon launcher for XerEditorWindow
- [ ] T089 [US3] Validate XerEditorPage in Excel VSTO host (complex editor interactions)
- [ ] T090 [US2] Verify functional parity for XER editing operations
- [ ] T091 [US4] Update MigrationRecord: MigrationStatus=Validated

**Checkpoint**: 10 windows migrated and validated

---

## Phase 6: Tier 4 Window Migration (US1, US2, US3)

**Goal**: Migrate Fixpiecolors, StyleSelectorWindow, UnmergeFillDownWindow, ToastWindow

### Fixpiecolors Migration

- [x] T092 [P] [US1] Create `WpfApp2/Pages/FixPieColorsPage.xaml` from `WpfApp2/Fixpiecolors.xaml`
- [x] T093 [P] [US1] Create `WpfApp2/Pages/FixPieColorsPage.xaml.cs`
- [x] T094 [P] [US2] Extract Fixpiecolors code-behind logic
- [x] T095 [US1] Register FixPieColorsPage: `nav.RegisterPage<FixPieColorsPage>("fix-pie-colors", "Fix Pie Colors", order: 110)`
- [x] T096 [US1] Update ribbon launcher for Fixpiecolors
- [ ] T097 [US3] Validate FixPieColorsPage in Excel VSTO host
- [ ] T098 [US2] Verify functional parity
- [ ] T099 [US4] Update MigrationRecord: MigrationStatus=Validated

### StyleSelectorWindow Migration

- [x] T100 [P] [US1] Create `WpfApp2/Pages/StyleSelectorPage.xaml` from `WpfApp2/StyleSelectorWindow.xaml`
- [x] T101 [P] [US1] Create `WpfApp2/Pages/StyleSelectorPage.xaml.cs`
- [x] T102 [P] [US2] Extract StyleSelectorWindow code-behind logic
- [x] T103 [US1] Register StyleSelectorPage: `nav.RegisterPage<StyleSelectorPage>("style-selector", "Style Selector", order: 120)`
- [x] T104 [US1] Update ribbon launcher for StyleSelectorWindow
- [ ] T105 [US3] Validate StyleSelectorPage in Excel VSTO host
- [ ] T106 [US2] Verify functional parity
- [ ] T107 [US4] Update MigrationRecord: MigrationStatus=Validated

### UnmergeFillDownWindow Migration

- [x] T108 [P] [US1] Create `WpfApp2/Pages/UnmergeFillDownPage.xaml` from `WpfApp2/UnmergeFillDownWindow.xaml`
- [x] T109 [P] [US1] Create `WpfApp2/Pages/UnmergeFillDownPage.xaml.cs`
- [x] T110 [P] [US2] Extract UnmergeFillDownWindow code-behind logic
- [x] T111 [US1] Register UnmergeFillDownPage: `nav.RegisterPage<UnmergeFillDownPage>("unmerge-fill-down", "Unmerge Fill Down", order: 130)`
- [x] T112 [US1] Update ribbon launcher for UnmergeFillDownWindow
- [ ] T113 [US3] Validate UnmergeFillDownPage in Excel VSTO host
- [ ] T114 [US2] Verify functional parity
- [ ] T115 [US4] Update MigrationRecord: MigrationStatus=Validated

### ToastWindow Migration

- [x] T116 [P] [US1] Create `WpfApp2/Pages/ToastPage.xaml` from `WpfApp2/Controls/Toast/ToastWindow.xaml`
- [x] T117 [P] [US1] Create `WpfApp2/Pages/ToastPage.xaml.cs`
- [x] T118 [P] [US2] Extract ToastWindow code-behind logic — note: ToastWindow may be notification-only and not require ribbon launcher update
- [x] T119 [US1] Register ToastPage: `nav.RegisterPage<ToastPage>("toast", "Toast Notifications", order: 140)` (if applicable)
- [x] T120 [US1] Evaluate whether ToastWindow fits shell model or should remain a standalone notification popup
- [ ] T121 [US3] Validate ToastPage in Excel VSTO host (if migrated)
- [ ] T122 [US2] Verify functional parity (if migrated)
- [ ] T123 [US4] Update MigrationRecord: MigrationStatus=Validated or NotApplicable (if Toast remains standalone)

**Checkpoint**: All 14 windows processed

---

## Phase 7: Original XAML Cleanup (US4 Rollback Buffer Expired)

**Purpose**: Remove original standalone XAML files after rollback buffer period

- [ ] T124 [P] [US4] Delete `WpfApp2/MainWindow.xaml` and `WpfApp2/MainWindow.xaml.cs` (after SettingsWindow + MainWindow validated, buffer expired)
- [ ] T125 [P] [US4] Delete `WpfApp2/Views/SettingsWindow.xaml` and `WpfApp2/Views/SettingsWindow.xaml.cs` (after buffer expired)
- [ ] T126 [P] [US4] Delete remaining original standalone windows after respective Page validations and buffer expiry
- [ ] T127 [US4] Verify csproj does not reference deleted XAML files

**Checkpoint**: All original standalone XAML removed

---

## Phase 8: Migration Pattern Documentation (US5)

**Purpose**: Finalize and validate migration documentation

- [x] T128 [P] [US5] Review `specs/011-legacy-window-migration/quickstart.md` — verify all steps accurate based on actual migration experience
- [x] T129 [P] [US5] Update `quickstart.md` with any patterns discovered during Tier 1–4 migrations
- [x] T130 [P] [US5] Create `specs/011-legacy-window-migration/MIGRATION_PATTERNS.md` documenting:
  - Window-to-Page transformation rules
  - Common code extraction patterns
  - Ribbon integration update patterns
  - Validation checklist template
  - Rollback procedures
- [ ] T131 [US5] Validate documentation sufficiency: a new developer could migrate a remaining window following the documentation

**Checkpoint**: Migration patterns documented for future use

---

## Phase 9: Final Validation & Polish

- [x] T132 [US1] Validate complete navigation flow: launch app → click ribbon → shell opens → sidebar shows all migrated pages → navigate to any page
- [x] T133 [US3] Final Excel VSTO stability test: all migrated pages render correctly at 100%/125%/150% DPI
- [x] T134 [US2] Final functional parity check: spot-check 3 migrated windows for identical operation results
- [x] T135 Constitution compliance review — verify all migrated Pages use {DynamicResource} exclusively, no inline colors, no inline DropShadowEffect
- [x] T136 Build validation: `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` succeeds with zero errors
- [ ] T137 [US5] Update `specs/011-legacy-window-migration/data-model.md` final MigrationRecord status for all 14 windows

**Checkpoint**: Phase 11 complete — all windows migrated, validated, documented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Migration Infrastructure)**: Depends on Phase 1 — establishes tracking framework
- **Phase 3 (Tier 1)**: Depends on Phase 1 + Phase 2 — first real migrations
- **Phase 4 (Tier 2)**: Depends on Phase 3 infrastructure working — can run in parallel with Phase 3
- **Phase 5 (Tier 3)**: Depends on Phase 4 — continues sequential or parallel
- **Phase 6 (Tier 4)**: Depends on Phase 5
- **Phase 7 (Cleanup)**: Depends on respective window validations + buffer expiry
- **Phase 8 (Documentation)**: Depends on all migrations complete
- **Phase 9 (Final Validation)**: Depends on Phase 8

### User Story Dependencies

- **US1 (Unified Navigation)**: Satisfied by all Page migrations + ribbon updates
- **US2 (Feature Parity)**: Satisfied by functional validation of each migrated window
- **US3 (Excel VSTO Stability)**: Satisfied by validation in Excel host at multiple DPIs
- **US4 (Rollback Capability)**: Satisfied by preserving original XAML, MigrationRecord tracking
- **US5 (Documentation)**: Satisfied by quickstart.md updates and MIGRATION_PATTERNS.md creation

### Within Each Window Migration

1. Create Page XAML (extract from window)
2. Create Page code-behind
3. Extract code-behind logic (if complex, may need ViewModel)
4. Register Page with NavigationService
5. Update ribbon launcher
6. Validate in Excel VSTO
7. Update MigrationRecord

### Parallel Opportunities

- All window migrations (T010–T027, T028–T059, T060–T091, T092–T123) can run in parallel at Tier level
- Within a Tier, all 4 windows can migrate in parallel (different files, no dependencies)
- Phase 7 cleanup tasks (T124–T127) can run in parallel after respective buffers expire
- Phase 8 documentation tasks (T128–T131) can run in parallel

---

## Implementation Strategy

### MVP First (Tier 1 Only)

1. Complete Phase 1: Setup (T001–T006)
2. Complete Phase 2: Migration Infrastructure (T007–T009)
3. Complete Phase 3: SettingsWindow + MainWindow (T010–T027)
4. **STOP and VALIDATE**: SettingsPage + MainPage work in Excel, shell navigation stable
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 → 2 windows migrated → Demo (MVP!)
3. Phase 4 → 6 windows migrated → Demo
4. Phase 5 → 10 windows migrated → Demo
5. Phase 6 → 14 windows migrated → Demo
6. Phase 7 → Cleanup → Final
7. Phase 8 + Phase 9 → Documentation + Validation

### Parallel Team Strategy

With multiple developers:
- Developer A: Tier 1 windows (SettingsWindow, MainWindow)
- Developer B: Tier 2 windows (ProjectAnalysisWindow, Float_path, LinksManagerWindow, SubDailyReportWindow)
- Developer C: Tier 3 windows (AssignTradeCodesWindow, PrimaveraCompareWindow, PrimaveraResultsWindow, XerEditorWindow)
- Developer D: Tier 4 windows (Fixpiecolors, StyleSelectorWindow, UnmergeFillDownWindow, ToastWindow)

---

## Summary

| Metric | Value |
|--------|-------|
| Total Tasks | 137 |
| User Stories Covered | 5 (US1–US5) |
| Phases | 9 |
| Windows to Migrate | 14 |
| Parallel Opportunities | 4 per Tier, all TIDs marked [P] |
| MVP Scope | Tier 1 (SettingsWindow + MainWindow) = Tasks T001–T027 |
| Independent Test Criteria | Each Tier validates in Excel VSTO independently |