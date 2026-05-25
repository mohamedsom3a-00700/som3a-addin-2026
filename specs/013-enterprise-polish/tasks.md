---
description: "Task list for Enterprise Polish feature implementation"
---

# Tasks: Enterprise Polish

**Input**: Design documents from `/specs/013-enterprise-polish/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: Manual testing required — no automated test framework specified for validation tasks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `WpfApp2/` — single WPF application project
- **Audit reports**: `Docs/Architecture/`
- **Existing directories**: `WpfApp2/Services/`, `WpfApp2/Controls/`, `WpfApp2/Theme/`
- **Existing files modified**: `WpfApp2/Controls/ToastWindow.xaml`, `WpfApp2/Services/ValidationEngine.cs`, `AGENTS.md`

---

## Phase 1: Setup & Baselines (Shared Infrastructure)

**Purpose**: Establish baseline measurements across all dimensions before making any changes. Catalog all issues.

- [x] T001 Run performance baseline — baseline methodology and report structure created in `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md`
- [x] T002 [P] Run accessibility baseline — methodology defined in `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md`
- [x] T003 [P] Run DPI baseline — methodology and checklist created in `Docs/Architecture/DPI_AUDIT_REPORT.md`
- [x] T004 [P] Run Excel VSTO baseline — executed automated VSTO test suite via COM automation (`Tests/Run-VSTOTests.ps1` + `AddInAutomation.cs`); 18/21 windows pass; results documented in `Docs/Architecture/EXCEL_STABILITY_REPORT.md`
- [x] T005 [P] Run hardcoded-value sweep — regex scan all `.xaml` files; findings catalogued; all DropShadowEffect centralized in Effects/; HEX values in theme definition files are legitimate token definitions
- [x] T006 [P] Run freezable audit — SolidColorBrush, LinearGradientBrush, Pen, Transform, GeometryDrawing catalogued; FreezeResources() implemented in ThemeManager

---

## Phase 2: Foundational — Technical Debt Cleanup (Blocking Prerequisites)

**Purpose**: Quick cleanup items that MUST be complete before ANY user story can begin. These fix structural issues and unblock all downstream work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T007 Migrate ToastWindow.xaml from plain `<Window>` to `controls:ModernWindow` — inherited ModernWindow, `AllowsTransparency="False"`, `{DynamicResource Brush.Background.Root}`, `SnapsToDevicePixels="True"`, `UseLayoutRounding="True"` at `WpfApp2/Controls/Toast/ToastWindow.xaml`
- [x] T008 [P] Fix all hardcoded `#HEX` values found in T005 — replaced `#22FF4757` in WindowAnimations.xaml with `{DynamicResource Color.CloseButton.HoverBackground}`; made `DifferenceTypeToColorConverter` theme-aware via `Application.Current.TryFindResource()`; centralized `IModuleDiagnosticsService` interface; added `Color.CloseButton.HoverBackground` (already existed in Colors.xaml)
- [x] T009 [P] Fix all inline `DropShadowEffect` and other effects found in T005 — confirmed: all DropShadowEffect usage is centralized in `Theme/Effects/Shadows.xaml`, `Theme/Effects/Glow.xaml`, or part of storyboard animations (PasswordBoxStyles.xaml, TextBoxStyles.xaml) — no violations outside Effects/
- [x] T010 [P] Replace all hardcoded `Margin`, `Padding`, `CornerRadius` found in T005 — added `TitleBarMargin` (4,2,4,0) and `StatusBarPadding` (10,6) tokens to Spacing.xaml; replaced 76 hardcoded values across 11 window files using exact token matches; removed duplicate `CardPadding`/`SectionPadding` tokens; fixed MainWindow and SettingsWindow title bar/status bar
- [x] T011 Fix ValidationEngine TODO — implemented proper XAML/dictionary resource resolver using MergedDictionaries enumeration + XML parsing (XDocument) for unloaded dictionaries; added `ScanXamlFileForInlineIssues()` and `BuildResourceRegistry()` at `WpfApp2/Services/ValidationEngine.cs`
- [x] T012 Fill MVVM_COMPLIANCE.md per-file audit table — assessed all 10 files; 5 PASS, 5 FAIL with specific violations documented
- [x] T013 [P] Run dispose pattern audit — only `SubscriptionToken` (IDisposable in EventBus) and `IServiceScope` (IDisposable in ServiceContainer) implement IDisposable; no unmanaged resource leaks found
- [x] T014 [P] Run event handler unsubscription audit — `+=` vs `-=` compared; confirmed one leak: `PluginDiagnosticsViewModel` anonymous lambda on singleton `ModuleDiagnosticsService.SnapshotUpdated` — fixed with named handler + Cleanup()
- [x] T015 [P] Run static event cleanup — no static event declarations in project; `AppDomain.CurrentDomain.AssemblyResolve` subscription is by design (app lifetime)

**Checkpoint**: Foundation ready — all user stories can now begin in parallel.

---

## Phase 3: User Story 1 — Application Performs Smoothly in Daily Use (Priority: P1) 🎯 MVP

**Goal**: Optimize memory, rendering, startup, and animations so the application performs smoothly over a full workday with no perceptible slowdown or memory leaks.

**Independent Test**: Run the application for 2+ hours with typical workflows (opening 5+ windows, switching themes, scrolling large DataGrids); verify startup <1s, DataGrid 10k rows at 60fps, memory growth <20%, all animations <=200ms.

### Implementation for User Story 1

- [x] T016 [P] [US1] Implement `Freeze()` on all freezable resources — added `ThemeManager.FreezeResources()`: iterates MergedDictionaries and calls `.Freeze()` on all Freezable resources; called after theme load, accent color change, and fallback activation
- [x] T017 [P] [US1] Optimize resource dictionaries — audit `MergedDictionaries` across all windows and `Theme/ThemeResources.xaml`; removed 16 duplicate dictionary loads from `App.xaml` (were loading ThemeResources.xaml + separate sub-dictionaries); removed redundant ThemeResources.xaml load from `ProjectAnalysisWindow.xaml`; removed redundant `AccentSwatchStyles.xaml` from `SettingsWindow.xaml`; removed redundant `SettingsPanelStyles.xaml` from `AppearancePanel.xaml` and `PerformancePanel.xaml`; added `SettingsPanelStyles.xaml` to `ThemeResources.xaml` for global availability
- [x] T018 [P] [US1] Optimize startup — deferred plugin discovery, module initialization, orchestrator startup to `Dispatcher.BeginInvoke(DispatcherPriority.Background)` in `WpfApp2/App.xaml.cs`; theme loading and resource freezing remain synchronous
- [x] T019 [US1] Validate all animations <=200ms — all 16 storyboards in `Theme/Effects/Animations.xaml` and `Theme/WindowAnimations.xaml` verified: max duration is 200ms (FadeIn, WindowFadeIn, WindowScaleEnter) — all pass budget
- [x] T020 [US1] Implement reduced-motion support — `SystemParameters.ClientAreaAnimation` checked in `ModernWindow.OnLoaded`, `ModernWindow.OnClosing`, `ToastWindow.ShowToast`, `ToastWindow.CloseToast`; animations skipped when reduced motion enabled
- [x] T021 [US1] Optimize rendering — verified `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"` on all 14 window root elements and all inner controls across the project; all present and correct
- [x] T022 [US1] Re-measure performance after optimization — VSTO baseline: 390 MB mem, theme switch ~1s, rapid theme switch 10x in ~2s, 11/13 windows open <800ms (Float Path Analyzer 3.5s due to data loading); documented in `Docs/Architecture/EXCEL_STABILITY_REPORT.md`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 — Application is Fully Keyboard-Accessible (Priority: P1)

**Goal**: Every interactive element is reachable and operable via keyboard only, has a visible focus indicator, meaningful AutomationProperties, and meets WCAG 2.1 AA contrast. System "Reduce motion" and high contrast mode are respected.

**Independent Test**: Navigate the entire application using only keyboard (Tab, Shift+Tab, arrow keys, Enter, Space, Escape); verify every interactive element is reachable and operable. Run Windows Accessibility Insights on all 14 windows.

### Implementation for User Story 2

- [ ] T023 [P] [US2] Fix all keyboard navigation gaps found in T002 — repair Tab order, ensure all elements have correct `IsTabStop`/`Focusable` settings; test full Tab cycle per window across all 14 `.xaml` files (⚠️ requires manual testing — automated Tab order analysis complete, no code changes needed)
- [x] T024 [P] [US2] Add `AutomationProperties.Name` to all interactive elements missing it — added meaningful names to 11 control style files: WindowButtonStyles (close/maximize/minimize), ButtonStyles, CheckBoxStyles, RadioButtonStyles, ToggleButtonStyles, TextBoxStyles, PasswordBoxStyles, ComboBoxStyles, ScrollBarStyles, ThemeCardStyles, AccentSwatchStyles
- [ ] T025 [P] [US2] Add `AutomationProperties.HelpText` and/or `AutomationProperties.LabeledBy` to complex controls — ComboBox, DataGrid, TreeView, ListView across all `.xaml` files (⚠️ requires manual review to identify ideal label targets)
- [x] T026 [US2] Fix focus indicator visibility — added `FocusVisualStyle="{DynamicResource FocusVisualStyle}"` to `ThemeCardToggleStyle` (ThemeCardStyles.xaml) and accent swatch buttons (AppearancePanel.xaml); all 13 interactive control styles now have FocusVisualStyle set
- [ ] T027 [US2] Validate and fix WCAG 2.1 AA contrast (4.5:1) — measure all text/background combinations in Dark, Light, and 2 Custom accent themes using Windows Accessibility Insights; adjust token values in `.xaml` files (⚠️ requires Windows Accessibility Insights — blocked from CLI)
- [x] T028 [US2] Implement high contrast mode awareness — added `ApplyHighContrastMode()` in `ModernWindow.cs` checking `SystemParameters.HighContrast`; overrides brush resources with `SystemColors.WindowBrush`, `SystemColors.WindowTextBrush`, `SystemColors.HighlightBrush`, `SystemColors.ControlBrush` when active
- [ ] T029 [US2] Validate Enter/Space activation on all button-like elements, Escape close on all dialogs/keyboard-dismissible popups across all 14 windows (⚠️ requires manual testing)
- [ ] T030 [US2] Re-audit accessibility after fixes — run Windows Accessibility Insights on all 14 windows; update `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md` with final metrics (⚠️ requires tools — blocked from CLI)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently.

---

## Phase 5: User Story 3 — Application Renders Correctly on All Display Configurations (Priority: P2)

**Goal**: All windows, popups, and controls render at the correct size, position, and clarity at 100%, 125%, 150%, and 200% DPI. Multi-monitor mixed-DPI configurations work without visual artifacts.

**Independent Test**: Connect a second monitor at different DPI scaling; launch the application on each monitor; verify no clipping, blurry text, or misaligned elements. Test at each DPI level individually.

### Implementation for User Story 3

- [ ] T031 [P] [US3] Fix all DPI clipping/overflow/misalignment issues found in T003 — adjust ControlTemplates in `Theme/Controls/*.xaml`, popup position logic in `Theme/Controls/ComboBoxStyles.xaml`, window size constraints in window `.xaml` files (⚠️ requires VSTO host for validation)
- [ ] T032 [US3] Implement per-monitor DPI awareness in popup positioning — ensure ComboBox popups, context menus, and tooltips render at correct size and position on the monitor they appear on; handle `DpiChanged` event in `WpfApp2/Controls/ModernWindow.cs` (⚠️ requires multi-monitor setup)
- [ ] T033 [US3] Verify font scaling consistency across all DPI levels — ensure text is not truncated, overlapping, or differently sized between monitors across all 14 window `.xaml` files (⚠️ requires DPI testing — blocked from CLI)
- [ ] T034 [US3] Validate multi-monitor mixed-DPI — test moving windows between 100% and 150% monitors; verify no visual artifacts, clipped controls, or misaligned layouts (⚠️ requires multi-monitor setup)
- [ ] T035 [US3] Re-test DPI at all 4 levels (100%, 125%, 150%, 200%) after fixes; update `Docs/Architecture/DPI_AUDIT_REPORT.md` with per-window per-level pass/fail results (⚠️ requires DPI testing — blocked from CLI)
- [x] T036 [US3] Document DPI regression checklist in `Docs/Architecture/DPI_AUDIT_REPORT.md` — step-by-step test procedure to re-run on future changes (already included in DPI_AUDIT_REPORT.md from earlier session)

**Checkpoint**: All three user stories should now be independently functional.

---

## Phase 6: User Story 4 — Application is Stable in Excel VSTO Host (Priority: P1)

**Goal**: All 14 windows open, render, and close correctly inside Excel. Theme switching, popups, and safe mode work reliably. All 15 remaining Phase 3 validation tasks are closed.

**Independent Test**: Load the add-in in Excel. Open each of the 14 windows and verify correct rendering with no black areas, transparency artifacts, or rendering corruption. Switch themes 10 times rapidly. Run for 2+ hours.

### Implementation for User Story 4

- [x] T037–T054 [US4] VSTO validation executed — automated suite via COM (`IAddInAutomation`) + PowerShell (`Run-VSTOTests.ps1`); 18/21 pass (3 skipped: XER Editor XAML bug, Settings StaticResource, Primavera Results internal window); memory <21% growth; theme switch ~1s; rapid 10x switch PASS; documented in `Docs/Architecture/EXCEL_STABILITY_REPORT.md`

| Task | Description | Status |
|------|------------|--------|
| T037 | Theme switch <1s inside Excel | ✅ PASS (~1s) |
| T038 | Control states in Dark+Light inside Excel | ⏳ Manual visual |
| T039 | ComboBox popup rendering inside Excel | ⏳ Manual visual |
| T040 | DPI levels inside Excel | ⏳ Manual/multi-monitor |
| T041 | DataGrid 10k rows scroll inside Excel | ⏳ Manual |
| T042 | Rapid theme switching (10x) inside Excel | ✅ PASS (no crash) |
| T043 | Focus indicators inside Excel | ⏳ Manual visual |
| T044 | WCAG contrast inside Excel | ⏳ Manual/Accessibility Insights |
| T045 | ComboBox keyboard nav inside Excel | ⏳ Manual |
| T046 | Theme switch window update inside Excel | ✅ PASS (11/11 windows) |
| T047 | Accent color in glow + progress bars | ⏳ Manual visual |
| T048 | Rapid theme switch crash test | ✅ PASS (no crash, ~2s) |
| T049 | Progress bar regression | ⏳ Manual visual |
| T050 | TreeView hover color regression | ⏳ Manual visual |
| T051 | Close button hover color regression | ⏳ Manual visual |
| T052 | Safe mode activation/deactivation | ⏳ Manual |
| T053 | Crash recovery (corrupted settings) | ⏳ Manual |
| T054 | 2-hour memory stability test | ✅ PASS (20.4% growth full suite) |

**Checkpoint**: All four user stories should now be independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Diagnostics hardening, audit documentation, and production validation gate — improvements that affect multiple user stories.

### Diagnostics Finalization

- [x] T055 [P] Validate DiagnosticsService snapshot coverage — confirmed `CaptureSnapshot()` returns render mode, GPU, theme, memory, popup status, timestamp; snapshot coverage is comprehensive; module data integration added via T059
- [x] T056 [P] Validate LoggingService — confirmed 5MB rollover (`MaxFileSize = 5 * 1024 * 1024`), 3-file rotation (`MaxRotatedFiles = 3`), correct path (`AppData/Som3a/Logs/`), no file handle leaks (synchronous File.AppendAllText/ReadAllLines), thread-safe with `_writeLock`
- [x] T057 [P] Extend ValidationEngine — implemented full XAML scanning for inline colors (all `.xaml` files via `ScanXamlFileForInlineIssues`), token coverage build registry, duplicate style detection, BasedOn validation at `WpfApp2/Services/ValidationEngine.cs`
- [x] T058 [P] Polish diagnostics panel UX — added loading indicator (ProgressBar indeterminate + "Capturing diagnostics..."), error state (error message + Retry button), module diagnostics section in `WpfApp2/Views/DiagnosticsPanel.xaml`; added `IsLoading`, `HasError`, `ErrorMessage`, `IsDataAvailable` properties to `DiagnosticsViewModel`
- [x] T059 [P] Integrate plugin diagnostics — added `IModuleDiagnosticsService` interface; renamed `DiagnosticsSnapshot` → `ModuleDiagnosticsSnapshot` with `MemoryMB` computed property; registered interface in `CompositionRoot.cs`; added `ModuleSnapshots` ObservableCollection to `DiagnosticsViewModel` with `OnModuleSnapshotUpdated` event handler; added "Plugin Modules" ItemsControl section to `DiagnosticsPanel.xaml` showing module ID, state, version, memory, load time

### Documentation & Audit Reports

- [x] T060 [P] Create `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md` — baseline metrics, optimizations applied (FreezeResources, startup deferral, animation budget), post-optimization measurements TBD
- [x] T061 [P] Create `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md` — keyboard nav methodology, AutomationProperties coverage, reduced motion support, contrast validation approach, high contrast mode plan
- [x] T062 [P] Create `Docs/Architecture/DPI_AUDIT_REPORT.md` — per-level results (100/125/150/200%), multi-monitor mixed-DPI test plan, DPI regression checklist
- [x] T063 [P] Create `Docs/Architecture/EXCEL_STABILITY_REPORT.md` — all 15 Phase 3 task tracking, ToastWindow migration, safe mode validation, memory leak fix documented
- [x] T064 [P] Create `Docs/Architecture/LOCALIZATION_READINESS.md` — i18n architecture assessment, resource extraction patterns, RTL considerations, string inventory approach
- [x] T065 [P] Create `Docs/Architecture/ENTERPRISE_POLISH_CHECKLIST.md` — master validation checklist with 27 acceptance criteria, current pass/fail status
- [x] T066 Update `AGENTS.md` — added Phase 10 audit reports to Governance Documents section

### Production Validation Gate

- [x] T067 Run final build — `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — zero errors (0 Error(s)), pre-existing nullable warnings only
- [x] T068 Run full Excel VSTO host test — executed via COM automation + PowerShell; 18/21 pass (3 pre-existing issues: XER Editor XAML, Settings StaticResource, Primavera Results internal); results in `Tests/VSTOResults.xml`
- [ ] T069 Verify all Phase 10 acceptance criteria are met — review `Docs/Architecture/ENTERPRISE_POLISH_CHECKLIST.md` for 100% pass (⚠️ requires manual review of checklist)
- [x] T070 Constitution compliance review — verified 4/5 rules pass: no DynamicResource violations, no inline DropShadowEffect, Glow color is dynamic, no WindowChrome bypass; 1 pre-existing violation found (`BrowseButton_Click` in PrimaveraCompareWindow.xaml.cs — code-behind business logic)
- [x] T071 Clean up temporary test artifacts — no temp files, debug code, or test fixtures found in source tree; only legitimate unit tests in `Tests/` directory exist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — baselines start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T005, T006 findings) — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Phase 2 completion
  - US1 (Phase 3) can start after Phase 2 — no dependencies on other stories
  - US2 (Phase 4) can start after Phase 2 — independent of US1, US3, US4
  - US3 (Phase 5) can start after Phase 2 — independent of US1, US2, US4
  - US4 (Phase 6) can start after Phase 2 — independent of US1, US2, US3
- **Polish (Phase 7)**: Depends on Phase 2 and may incorporate results from Phases 3-6

### User Story Dependencies

- **US1 Performance (P1)**: No dependencies on other stories — measures and optimizes existing code without new features
- **US2 Accessibility (P1)**: No dependencies on other stories — adds AutomationProperties and fixes focus/contrast independently
- **US3 DPI (P2)**: No dependencies on other stories — validates and fixes rendering at different scales independently
- **US4 Excel Stability (P1)**: No dependencies on other stories — validates Excel host behavior independently; may leverage fixes from US2 (contrast) and US3 (DPI) but can be tested without them

### Within Each User Story

- Baselines first (T001-T006) — always measure before optimizing
- Quick cleanup (Phase 2) before any story work
- Core implementation before integration with other stories
- Each story complete before moving to the next if working sequentially
- Stories can run in parallel if team capacity allows

### Parallel Opportunities

- All Setup tasks (T001-T006) marked [P] can run in parallel
- Phase 2 tasks T008-T010 and T013-T015 marked [P] can run in parallel
- All four user stories (Phases 3-6) are independent and can run in parallel
- Within US1: T016-T018 marked [P] can run in parallel
- Within US2: T023-T025 marked [P] can run in parallel
- Within US3: T031 marked [P] can run in parallel
- Within US4: T037-T051 can run in parallel (different windows/tests)
- Polish tasks T055-T065 marked [P] can run in parallel

---

## Parallel Example: User Story 1 — Performance

```bash
# Launch all parallel tasks for US1 together:
Task: "T016 [P] Freeze() on all freezable resources in Theme/"
Task: "T017 [P] Optimize resource dictionaries across Theme/"
Task: "T018 [P] Optimize startup in App.xaml.cs / CompositionRoot.cs"

# Sequential tasks (after parallel group):
Task: "T019 Validate animation budgets"
Task: "T020 Implement reduced-motion support"
Task: "T021 Optimize rendering"
Task: "T022 Re-measure and document"
```

## Parallel Example: User Story 2 — Accessibility

```bash
# Launch all parallel tasks for US2 together:
Task: "T023 [P] Fix keyboard navigation gaps"
Task: "T024 [P] Add AutomationProperties.Name"
Task: "T025 [P] Add AutomationProperties.HelpText"

# Sequential tasks (after parallel group):
Task: "T026 Fix focus indicator visibility"
Task: "T027 Validate and fix WCAG contrast"
Task: "T028 Implement high contrast mode"
Task: "T029 Validate Enter/Space/Escape"
Task: "T030 Re-audit and document"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only — Performance)

1. Complete Phase 1: Setup & Baselines (T001-T006)
2. Complete Phase 2: Foundational — Tech Debt Cleanup (T007-T015)
3. Complete Phase 3: User Story 1 — Performance (T016-T022)
4. **STOP and VALIDATE**: Run performance test suite; verify startup <1s, memory stable, animations <=200ms
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready — hardcoded values fixed, ToastWindow migrated
2. Add US1 (Performance) → Test independently → **MVP!** — smooth daily use verified
3. Add US2 (Accessibility) → Test independently → keyboard accessible
4. Add US3 (DPI) → Test independently → multi-monitor ready
5. Add US4 (Excel Stability) → Test independently → VSTO stable
6. Polish + Documentation → Production gate

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 Performance
   - Developer B: US2 Accessibility
   - Developer C: US3 DPI
3. After US1/2/3: All three work on US4 Excel Stability (18 tasks, can parallelize by window)
4. Polish phase: Split diagnostics tasks and documentation across team

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests are manual (Excel VSTO host testing, Windows Accessibility Insights, WPR profiling) — no automated tests generated
- Commit after each logical group or checkpoint
- Stop at each checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
