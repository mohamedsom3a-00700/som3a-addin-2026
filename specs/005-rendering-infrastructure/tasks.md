# Tasks: Rendering & Window Infrastructure

**Input**: Design documents from `/specs/005-rendering-infrastructure/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **WPF project**: `WpfApp2/` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and build verification

- [X] T001 Create Docs/Architecture/EXCEL_TEST_CHECKLIST.md with 8+ manual test scenarios covering window opening, DPI scaling, popup rendering, theme switching, and safe mode activation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: RenderModeService — ALL user stories depend on render mode detection

- [X] T002 Create RenderModeService in WpfApp2/Services/RenderModeService.cs with: Initialize(), GetCurrentMode(), IsSafeModeRequired(), IsGpuAvailable(), IsTransparencySupported(), RenderModeChanged event
- [X] T003 [P] Integrate WindowRenderModeDetector (existing) as detection backend for RenderModeService — GPU availability, transparency support, VSTO detection
- [X] T004 [P] Add RenderMode enum (WindowChrome, FallbackSafe) in RenderModeService.cs
- [X] T005 Implement fallback logic: on RenderModeService initialization failure, default to FallbackSafe mode

**Checkpoint**: RenderModeService ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Excel-Safe Window Rendering (Priority: P1) 🎯 MVP

**Goal**: Every window renders correctly inside Excel VSTO host with automatic FallbackSafe detection — no black windows, no transparency artifacts, no freezes.

**Independent Test**: Open each window from Excel VSTO host; verify correct rendering (no black windows, no artifacts); confirm move, minimize, maximize, close without freezing Excel.

### Implementation for User Story 1

- [X] T006 [US1] Audit all 14 window files — document ModernWindow inheritance, AllowsTransparency, inline effects, SnapsToDevicePixels, UseLayoutRounding
- [X] T007 [P] [US1] Add RenderMode DP (RenderMode) to ModernWindow in WpfApp2/Controls/ModernWindow.cs
- [X] T008 [P] [US1] Add IsSafeMode DP (read-only, bound to RenderModeService) to ModernWindow
- [X] T009 [P] [US1] Add WindowBackdrop DP (solid/gradient/image) to ModernWindow
- [X] T010 [P] [US1] Add DpiScale DP to ModernWindow with DefaultValue matching current behavior
- [X] T011 [P] [US1] Refactor InitializeWindow() in ModernWindow to use RenderModeService.IsSafeModeRequired() for fallback rendering
- [X] T012 [US1] Apply per-window changes to MainWindow.xaml: ensure WindowStyle="None", DynamicResource backgrounds, SnapsToDevicePixels, UseLayoutRounding
- [X] T013 [P] [US1] Apply per-window changes to Float_path.xaml
- [X] T014 [P] [US1] Apply per-window changes to AssignTradeCodesWindow.xaml
- [X] T015 [P] [US1] Apply per-window changes to Fixpiecolors.xaml
- [X] T016 [P] [US1] Apply per-window changes to LinksManagerWindow.xaml
- [X] T017 [P] [US1] Apply per-window changes to StyleSelectorWindow.xaml
- [X] T018 [P] [US1] Apply per-window changes to SubDailyReportWindow.xaml
- [X] T019 [P] [US1] Apply per-window changes to UnmergeFillDownWindow.xaml
- [X] T020 [P] [US1] Apply per-window changes to XerEditorWindow.xaml
- [X] T021 [P] [US1] Apply per-window changes to ProjectAnalysisWindow.xaml
- [X] T022 [P] [US1] Apply per-window changes to Views/SettingsWindow.xaml
- [X] T023 [P] [US1] Apply per-window changes to Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml
- [X] T024 [P] [US1] Apply per-window changes to Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml
- [X] T025 [P] [US1] Apply per-window changes to Toast/ToastWindow.xaml

**Checkpoint**: At this point, User Story 1 should be fully functional — all windows render safely in Excel VSTO host with automatic mode detection

---

## Phase 4: User Story 2 - Unified Shadow & Effect System (Priority: P1)

**Goal**: Single centralized shadow system with safe-mode fallback variants. Zero inline DropShadowEffect outside Effects/ directory.

**Independent Test**: Grep for `<DropShadowEffect` outside Effects/ — zero results. Safe-mode variants exist and activate when RenderModeService reports FallbackSafe.

### Implementation for User Story 2

- [X] T026 [P] [US2] Add Shadow.Window.Safe to WpfApp2/Theme/Effects/Shadows.xaml (50% blur, 75% opacity)
- [X] T027 [P] [US2] Add Shadow.Card.Safe to WpfApp2/Theme/Effects/Shadows.xaml
- [X] T028 [P] [US2] Add Shadow.Popup.Safe to WpfApp2/Theme/Effects/Shadows.xaml
- [X] T029 [US2] Document safe-mode variants in Docs/Architecture/SHADOW_SYSTEM.md
- [X] T030 [US2] Update ModernWindow fallback to apply Shadow.*.Safe variants when IsSafeModeRequired() is true
- [X] T031 [US2] Run grep across all .xaml files for inline DropShadowEffect outside Effects/ — fix any violations found

**Checkpoint**: All shadows centralized. Safe variants active in FallbackSafe mode. Zero inline DropShadowEffect violations.

---

## Phase 5: User Story 3 - DPI-Aware Window Scaling (Priority: P2)

**Goal**: All windows scale correctly at 100%, 125%, and 150% DPI settings.

**Independent Test**: Open every window at 100%, 125%, and 150% DPI scaling. Verify proportional scaling with no clipping or overlap.

### Implementation for User Story 3

- [X] T032 [P] [US3] Implement GetCurrentDpiScale() in WpfApp2/Helpers/DpiHelper.cs — returns current scale factor
- [X] T033 [P] [US3] Implement ScaleValue(double) in DpiHelper.cs — scales a value by current DPI
- [X] T034 [P] [US3] Implement IsHighDpi() in DpiHelper.cs — returns true when scale >= 1.5
- [X] T035 [US3] Integrate DpiHelper with ModernWindow — bind DpiScale DP to DpiHelper.GetCurrentDpiScale()
- [X] T036 [US3] Apply DpiHelper.ScaleValue() to layout-affected elements in refactored windows (verify no clipping at 125% and 150%)

**Checkpoint**: All windows pass visual inspection at 100%, 125%, and 150% DPI with no clipping or overlap.

---

## Phase 6: User Story 4 - Keyboard & Accessibility Support (Priority: P3)

**Goal**: All windows fully navigable via keyboard with visible focus indicators and screen reader preparation.

**Independent Test**: Navigate every window using Tab, Enter, Escape, arrow keys. Verify focus indicators visible on all interactive elements.

### Implementation for User Story 4

- [X] T037 [P] [US4] Add FocusVisualStyle using Glow.Focus to all interactive controls (Button, ComboBox, CheckBox, RadioButton, etc.)
- [X] T038 [P] [US4] Add AutomationProperties.Name to all interactive elements across all 14 windows
- [X] T039 [P] [US4] Add AutomationProperties.HelpText on complex controls (DataGrid, ListView)
- [X] T040 [P] [US4] Verify keyboard navigation (Tab order, Enter/Space activation) across all windows
- [X] T041 [US4] Add high contrast awareness — use system colors when high contrast mode is active
- [X] T042 [US4] Verify Escape key closes windows that have ModernWindow.CloseOnEscape

**Checkpoint**: All controls accessible via keyboard. Focus indicators visible. Screen reader metadata present.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T043 [P] Standardize window open animations in WpfApp2/Theme/WindowAnimations.xaml — ensure ≤200ms open, ≤150ms close using Motion.* tokens
- [X] T044 [P] Add safe-mode gating to WindowAnimations.xaml — skip all animations when IsSafeModeRequired() is true (FR-005, FR-009)
- [X] T045 [P] Run build: msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug — fix any compilation errors
- [X] T046 Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration, Excel rendering safety, WindowChrome inheritance across all modified files
- [X] T047 Run quickstart.md validation — confirm all tasks in quickstart.md execution order are complete

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Phase 2 (RenderModeService)
  - US1 (P1) and US2 (P1) are independent of each other — can run in parallel
  - US3 (P2) depends on ModernWindow DpiScale DP from US1
  - US4 (P3) depends on window refactoring from US1
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (RenderModeService) — No dependencies on other stories
- **User Story 2 (P1)**: Depends on Foundational (RenderModeService) — Independent of US1
- **User Story 3 (P2)**: Depends on US1 (ModernWindow DpiScale DP, window refactoring)
- **User Story 4 (P3)**: Depends on US1 (window refactoring creates the canvases for accessibility)

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- T003/T004 — Foundational tasks can run in parallel
- T007-T010 — ModernWindow DPs can all run in parallel
- T012-T025 — Window refactoring tasks all run in parallel (different files)
- T026-T028 — Shadow variants can run in parallel
- T032-T034 — DPI helper methods can run in parallel
- T037-T040 — Accessibility tasks can run in parallel
- T043-T047 — Polish tasks can run in parallel

---

## Parallel Example: User Story 1

```powershell
# Launch all ModernWindow DP additions together:
Task: "Add RenderMode DP to WpfApp2/Controls/ModernWindow.cs"
Task: "Add IsSafeMode DP to WpfApp2/Controls/ModernWindow.cs"
Task: "Add WindowBackdrop DP to WpfApp2/Controls/ModernWindow.cs"
Task: "Add DpiScale DP to WpfApp2/Controls/ModernWindow.cs"

# Launch all window refactoring tasks in parallel:
Task: "Refactor MainWindow.xaml for safe-mode rendering"
Task: "Refactor SettingsWindow.xaml for safe-mode rendering"
Task: "Refactor PrimaveraCompareWindow.xaml for safe-mode rendering"
# ... (all 14 windows in parallel)
```

## Parallel Example: User Story 2

```powershell
# Launch all safe shadow variant additions in parallel:
Task: "Add Shadow.Window.Safe to Effects/Shadows.xaml"
Task: "Add Shadow.Card.Safe to Effects/Shadows.xaml"
Task: "Add Shadow.Popup.Safe to Effects/Shadows.xaml"
```

## Parallel Example: User Story 3

```powershell
# Launch all DPI helper methods in parallel:
Task: "Implement GetCurrentDpiScale() in DpiHelper.cs"
Task: "Implement ScaleValue() in DpiHelper.cs"
Task: "Implement IsHighDpi() in DpiHelper.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup (T001 — EXCEL_TEST_CHECKLIST skeleton)
2. Phase 2: Foundational (T002-T005 — RenderModeService)
3. Phase 3: User Story 1 (T006-T025 — Window audit + ModernWindow DPs + window refactoring)
4. **STOP and VALIDATE**: Test all windows in Excel VSTO host
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → RenderModeService ready
2. Add US1 (Excel-Safe Rendering) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Shadow System) → Test independently → Deploy/Demo
4. Add US3 (DPI Scaling) → Test independently → Deploy/Demo
5. Add US4 (Accessibility) → Test independently → Deploy/Demo
6. Polish → Final validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Developer A: User Story 1 (window audit + ModernWindow DPs + window refactoring)
3. Developer B: User Story 2 (safe shadow variants in parallel)
4. Once US1 done: Developer C: User Story 3, Developer D: User Story 4
5. All stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total tasks: 47 across 7 phases
