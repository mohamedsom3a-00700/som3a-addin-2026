# Tasks: Control Standardization

**Input**: Design documents from `specs/007-control-standardization/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No test tasks generated — tests not requested in feature specification for this UI standardization phase. Validation is performed via visual inspection, grep audits, and msbuild verification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single WPF project**: `WpfApp2/` at repository root
- **Control styles**: `WpfApp2/Theme/Controls/`
- **Effect files**: `WpfApp2/Theme/Effects/`
- **Window files**: `WpfApp2/Views/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Review existing control styles and load current state

- [X] T001 Review the spec at `specs/007-control-standardization/spec.md` and research decisions at `specs/007-control-standardization/research.md`
- [X] T002 [P] Review plan.md at `specs/007-control-standardization/plan.md` and data-model.md at `specs/007-control-standardization/data-model.md`
- [X] T003 Familiarize with existing control styles under `WpfApp2/Theme/Controls/` and their current VSM state coverage

---

## Phase 2: Foundational — Audit All Control Templates (Blocking Prerequisites)

**Purpose**: Blocking audit that identifies all issues before any user story work begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Run grep audit for hardcoded colors in `WpfApp2/Theme/Controls/*.xaml` — pattern: `Color="#[0-9A-Fa-f]+"` — document results
- [X] T005 [P] Run grep audit for inline DropShadowEffect in `WpfApp2/Theme/Controls/*.xaml` — pattern: `DropShadowEffect` — document results
- [X] T006 [P] Run grep audit for duplicate style definitions across all control XAML files — pattern: `TargetType="(Button|TextBox|ComboBox|CheckBox|RadioButton|ToggleButton|DataGrid|ListView|TreeView|PasswordBox)"` — count occurrences per type
- [X] T007 [P] Run grep audit for inline styles `<Style ` inside window files at `WpfApp2/Views/*.xaml` — document which windows have inline styles
- [X] T008 [P] Run grep audit for hardcoded margins/padding in control styles — pattern: `Margin="`, `Padding="` — verify they use `{DynamicResource Spacing.*}` or `{DynamicResource Padding.*}`
- [X] T009 Run grep audit for StaticResource usage on themeable properties in `WpfApp2/Theme/Controls/*.xaml` — verify DynamicResource-only compliance
- [X] T010 Compile audit results into consolidated report — list all hardcoded colors, inline shadows, duplicate styles, inline window styles, hardcoded spacing, and StaticResource violations by file and line

**Checkpoint**: Foundation audit complete — all violations documented and ready for remediation in user story phases

---

## Phase 3: User Story 2 — ComboBox Dropdowns Work Reliably Inside Excel (Priority: P1) 🎯 MVP

**Goal**: Fix ComboBox popup architecture — width matching, clipping prevention, consistent shadow, Excel safe-mode rendering

**Independent Test**: Open every window that contains a ComboBox, click each dropdown, verify popup opens with correct width, visible shadow (normal or safe variant), no clipping. Test in Dark, Light, and Custom themes at 100% and 150% DPI.

- [X] T011 [US2] Refactor ComboBox popup in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — verify `AllowsTransparency="False"`, `Placement="Bottom"` with smart direction detection (opens upward near screen bottom), `PlacementTarget` bound to owning ComboBox
- [X] T012 [P] [US2] Synchronize popup width with parent ComboBox in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — ensure popup width matches the ComboBox control width (or wider if content exceeds)
- [X] T013 [P] [US2] Ensure safe-mode shadow variant in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — use `{DynamicResource Shadow.Popup}` for normal mode, `{DynamicResource Shadow.Popup.Safe}` for safe mode; remove any inline shadow
- [X] T014 [P] [US2] Add empty-item handling for ComboBox popup in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — suppress popup display or show empty list indicator when no items
- [X] T015 [US2] Review and update `WpfApp2/Theme/Controls/ComboBoxItemStyles.xaml` for consistency — ensure item hover/selection colors use `{DynamicResource Brush.*}` tokens
- [ ] T016 [US2] Validate ComboBox popup rendering across all windows — open each window with ComboBox, verify width, shadow, no clipping at 100% and 150% DPI, in all three themes and both rendering modes

**Checkpoint**: ComboBox popups render correctly in all windows, all themes, all rendering modes

---

## Phase 4: User Story 1 — Consistent Control Appearance Across All Windows (Priority: P1)

**Goal**: Standardize TextBox, DataGrid, ListView, PasswordBox control styles so every instance looks and behaves identically across all windows

**Independent Test**: Open any three different windows side by side, compare Button, TextBox, ComboBox, CheckBox, DataGrid, ListView appearance — each control type must look identical in all windows. Switch themes and verify all windows update consistently.

- [X] T017 [P] [US1] Standardize TextBox styles in `WpfApp2/Theme/Controls/TextBoxStyles.xaml` — add complete VSM states: Normal, MouseOver (accent border), Focused (accent border glow), Disabled (reduced opacity). All colors via `{DynamicResource Brush.*}`
- [X] T018 [P] [US1] Standardize DataGrid styles in `WpfApp2/Theme/Controls/DataGridStyles.xaml` — replace any remaining hardcoded colors with `{DynamicResource Brush.DataGrid.*}` tokens. Verify hover/selection/alternating row colors use semantic tokens
- [X] T019 [P] [US1] Standardize ListView styles in `WpfApp2/Theme/Controls/ListViewStyles.xaml` — ensure selection colors use `{DynamicResource Brush.*}` tokens, consistent with DataGrid pattern
- [X] T020 [P] [US1] Create PasswordBox styles in `WpfApp2/Theme/Controls/PasswordBoxStyles.xaml` — follow TextBox VSM pattern (Normal, MouseOver, Focused, Disabled) with same border/background styling
- [X] T021 [P] [US1] Create TreeView styles if missing in `WpfApp2/Theme/Controls/` — standardize item height, hover/selection colors, expand/collapse arrow styling
- [X] T022 [P] [US1] Add empty-state placeholder for DataGrid in `WpfApp2/Theme/Controls/DataGridStyles.xaml` — centered "No data available" text when `Items.Count == 0` (FR-014)
- [X] T023 [P] [US1] Add empty-state placeholder for ListView in `WpfApp2/Theme/Controls/ListViewStyles.xaml` — centered "No data available" text (FR-014)
- [X] T024 [P] [US1] Add empty-state placeholder for TreeView in `WpfApp2/Theme/Controls/TreeViewStyles.xaml` — centered "No data available" text (FR-014)
- [ ] T025 [US1] Verify all standardized controls render correctly across three different windows — compare appearance side by side. Repeat in Dark, Light, and Custom themes

**Checkpoint**: All standardized controls look and behave identically across all windows and themes

---

## Phase 5: User Story 3 — Smooth Scrolling in Large Data Sets (Priority: P2)

**Goal**: Add virtualization and smooth scrolling to DataGrid, ListView, and TreeView for responsive performance with 1000+ rows

**Independent Test**: Open a DataGrid with 1000+ rows, scroll using scroll bar, mouse wheel, and arrow keys — verify smooth scrolling at 30+ FPS with no stuttering. Verify ListView with 500+ items scrolls responsively.

- [X] T026 [P] [US3] Add virtualization to DataGrid in `WpfApp2/Theme/Controls/DataGridStyles.xaml` — set `VirtualizingStackPanel.VirtualizationMode="Recycling"`, `EnableRowVirtualization="True"`, `ScrollUnit="Item"`
- [X] T027 [P] [US3] Add virtualization to ListView in `WpfApp2/Theme/Controls/ListViewStyles.xaml` — set `VirtualizingStackPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`
- [X] T028 [P] [US3] Add virtualization to TreeView in `WpfApp2/Theme/Controls/TreeViewStyles.xaml` — set `VirtualizingStackPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`
- [X] T029 [P] [US3] Ensure all scrollable containers use thin scrollbar from `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` — apply implicit style targeting `ScrollViewer` with 4px track, 8px thumb
- [ ] T030 [US3] Verify smooth scrolling — load 1000+ rows in DataGrid, scroll via bar/mouse wheel/arrow keys; verify 30+ FPS. Repeat with 500+ items in ListView

**Checkpoint**: All scrollable containers handle 1000+ rows smoothly at 30+ FPS

---

## Phase 6: User Story 4 — Keyboard Navigation Across All Controls (Priority: P2)

**Goal**: Ensure every interactive control is reachable and activatable via keyboard with visible focus indicators

**Independent Test**: Navigate through every interactive element in three most complex windows using only Tab, Enter, Space, Arrow keys, and Escape — verify every element is reachable, activatable, and focus rings visible at all times in both Dark and Light themes.

- [X] T031 [P] [US4] Apply `FocusVisualStyle="{DynamicResource Glow.Focus}"` to all interactive controls in `WpfApp2/Theme/Controls/ButtonStyles.xaml`, `TextBoxStyles.xaml`, `ComboBoxStyles.xaml`, `CheckBoxStyles.xaml`, `RadioButtonStyles.xaml`, `ToggleButtonStyles.xaml`, `DataGridStyles.xaml`, `ListViewStyles.xaml`, `TreeViewStyles.xaml`, `PasswordBoxStyles.xaml`
- [X] T032 [P] [US4] Verify `IsTabStop="True"` on all interactive controls — check each control style has `IsTabStop` correctly configured; document any controls that need fixing
- [X] T033 [P] [US4] Add TreeView keyboard navigation in `WpfApp2/Theme/Controls/TreeViewStyles.xaml` — Arrow Up/Down for item selection, Arrow Left/Right for expand/collapse, consistent item height
- [X] T034 [P] [US4] Add dialog escape-to-close behavior — verify `<Window>` style in `WpfApp2/Theme/Controls/WindowStyles.xaml` binds `Closing` command or keyboard gesture for Escape key
- [ ] T035 [P] [US4] Add reduced-motion support per FR-010 — wrap all control animations with trigger on `SystemParameters.ClientAreaAnimation` or `UIAnimation` flag; disable transitions when reduced motion active
- [ ] T036 [US4] Verify complete keyboard traversal — navigate three most complex windows using only Tab, Enter, Space, Arrow keys, Escape. Verify focus rings visible in Dark and Light themes

**Checkpoint**: Every interactive control is keyboard-accessible with visible focus in all themes

---

## Phase 7: User Story 5 — No Duplicate or Conflicting Control Styles (Priority: P3)

**Goal**: Remove all duplicate and inline control styles, ensuring exactly one authoritative style per control type

**Independent Test**: Search all XAML files for duplicate `TargetType` definitions — verify no control type has more than one style. Verify zero inline `<Style>` in window files.

- [ ] T037 [US5] For each duplicate style found in T006 audit — compare property setters between duplicate and canonical style; if identical, remove duplicate and update all references; if divergent, flag for manual review per clarification decision
- [ ] T038 [P] [US5] Migrate all inline styles found in `WpfApp2/Views/*.xaml` (from T007 audit) to their respective centralized style files in `WpfApp2/Theme/Controls/` — ensure windows reference canonical styles instead of defining inline
- [X] T039 [P] [US5] Run grep audit to confirm zero remaining hardcoded colors in `WpfApp2/Theme/Controls/*.xaml` (re-run T004 pattern) — all values must use `{DynamicResource Brush.*}`
- [X] T040 [P] [US5] Run grep audit to confirm zero remaining inline DropShadowEffect in `WpfApp2/Theme/Controls/*.xaml` (re-run T005 pattern) — all shadows must source from `Effects/Shadows.xaml`
- [X] T041 [P] [US5] Run grep audit to confirm zero remaining StaticResource on themeable properties in `WpfApp2/Theme/Controls/*.xaml` (re-run T009 pattern)

**Checkpoint**: Zero duplicates, zero inline styles in windows, zero hardcoded colors/shadows in controls, zero StaticResource violations

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, build verification, and remediation

- [X] T042 Run `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — verify build passes with zero errors
- [X] T043 [P] Update `WpfApp2/Theme/ThemeResources.xaml` if new control style files were created (PasswordBoxStyles.xaml, TreeViewStyles.xaml) — ensure they are included in the MergedDictionaries in correct loading order
- [ ] T044 [P] Add loading indicator for data-bound controls per FR-014 — ensure DataGrid, ListView, TreeView show a consistent loading indicator during data population
- [ ] T045 [P] Add error logging for style load failures per FR-013 — verify `System.Diagnostics.Trace` or existing logger is called when style resolution fails; ensure graceful fallback to default WPF styling
- [ ] T046 Final comprehensive validation — run all quickstart.md success criteria checks:
  - SC-001: grep audit — zero duplicate styles
  - SC-002: visual check — all ComboBox popups render correctly
  - SC-003: performance check — DataGrid 1000+ rows at 30+ FPS
  - SC-004: keyboard check — full traversal of every window
  - SC-005: grep audit — zero hardcoded colors/shadows in controls
  - SC-006: timing check — all animations ≤200ms; reduced motion disables them
  - SC-007: grep audit — zero inline DropShadowEffect outside Effects/
- [ ] T047 Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration (no direct brush mutation), Excel rendering safety (safe-mode variants), WindowChrome inheritance, centralized effects only, resource loading order

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — review documents first
- **Foundational — Audit (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational audit completion
  - Phase 3 (US2) and Phase 4 (US1) are both P1 and can proceed in parallel
  - Phase 5 (US3) depends on Phase 4 (US1) — virtualization requires standardized controls
  - Phase 6 (US4) depends on Phase 3+4 — keyboard nav requires refactored controls
  - Phase 7 (US5) depends on Phase 3+4 — deduplication requires final canonical styles
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US2 — ComboBox Popup (P1)**: Can start after Foundational Phase 2 — No dependencies on other stories
- **US1 — Consistent Appearance (P1)**: Can start after Foundational Phase 2 — No dependencies on other stories
- **US3 — Smooth Scrolling (P2)**: Best done after US1 (standardized controls) to avoid rework
- **US4 — Keyboard Navigation (P2)**: Requires standardized controls from US1 and US2 for consistent focus behavior
- **US5 — Remove Duplicates (P3)**: Should be final pass after all other changes to ensure no new duplicates introduced

### Within Each User Story

- Audit findings from Phase 2 guide each story's implementation
- Models/templates before validation
- Implementation before grep verification
- Story complete before moving to next

### Parallel Opportunities

- Phase 3 (US2 — ComboBox) and Phase 4 (US1 — Standardization) can run in parallel as both target different files
- T012/T013/T014 within US2 can run in parallel (different property changes in same file, or different files)
- T017/T018/T019/T020/T021 within US1 can run in parallel (different control style files)
- T026/T027/T028 within US3 can run in parallel (DataGrid, ListView, TreeView are separate files)
- T031/T032/T033/T034/T035 within US4 can run in parallel (different control files)
- T038/T039/T040/T041 within US5 can run in parallel (file-level independence)

---

## Parallel Example: Phase 3 — User Story 2 (ComboBox Popup)

```bash
# Launch independent file changes together:
Task: "T011 Refactor ComboBox popup structure in ComboBoxStyles.xaml"
Task: "T012 Add width synchronization in ComboBoxStyles.xaml"
Task: "T013 Add safe-mode shadow variant in ComboBoxStyles.xaml"
Task: "T014 Add empty-item handling in ComboBoxStyles.xaml"
```

## Parallel Example: Phase 4 — User Story 1 (Consistent Appearance)

```bash
# Launch all control standardizations together (separate files):
Task: "T017 Standardize TextBoxStyles.xaml"
Task: "T018 Standardize DataGridStyles.xaml"
Task: "T019 Standardize ListViewStyles.xaml"
Task: "T020 Create PasswordBoxStyles.xaml"
Task: "T021 Create TreeViewStyles.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 2 — ComboBox Popup)

1. Complete Phase 1: Setup — review all documents
2. Complete Phase 2: Foundational — audit all templates (CRITICAL — blocks all stories)
3. Complete Phase 3: US2 — ComboBox popup fix
4. **STOP and VALIDATE**: Test every ComboBox in every window across all themes and rendering modes
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Audit ready
2. Add US2 (ComboBox) → Test independently → Deploy/Demo
3. Add US1 (Consistent appearance) → Test independently → Deploy/Demo
4. Add US3 (Smooth scrolling) → Test independently → Deploy/Demo
5. Add US4 (Keyboard navigation) → Test independently → Deploy/Demo
6. Add US5 (Deduplication) → Test independently → Final polish

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once audit is done:
   - Developer A: Phase 3 US2 (ComboBox popup)
   - Developer B: Phase 4 US1 (TextBox, DataGrid, ListView standardization)
   - Developer C: Phase 5 US3 (Virtualization) — can start after US1 standardized controls
3. Stories complete and integrate independently
4. Polish phase done together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests not requested per spec — validation via visual inspection, grep audit, msbuild
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- All Brush/Color values must use `{DynamicResource}` — no hardcoded hex values anywhere in control styles
- All DropShadowEffect must source from `Effects/Shadows.xaml` — no inline definitions
