# Tasks: Update Themes Manager

**Feature ID**: 002-themes-manager | **Branch**: `002-fluent-theme-engine` | **Date**: 2026-05-19

**Input**: Design documents from `/specs/002-themes-manager/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: Not explicitly requested in the feature specification. Testing is manual verification via `msbuild` + runtime checks in Excel VSTO host.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Single WPF project: `WpfApp2/` at repository root
- All paths relative to `WpfApp2/` unless prefixed with repo root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify project state and prepare for implementation

- [x] T001 Verify `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` compiles successfully before any changes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story window migration can begin. Includes ThemeManager fixes and new token definitions.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### ThemeManager Core Fixes

- [x] T002 Fix accent persistence in `Services/ThemeManager.cs` — ensure `ApplyTheme()` preserves `_currentAccentColor` when `accentColor` is null (FR-001)
- [x] T003 Fix early-return logic in `Services/ThemeManager.cs` — reorder `_currentTheme` update to prevent duplicate work on same-theme reselection (FR-002)
- [x] T004 Implement safe dictionary fallback in `Services/ThemeManager.cs` — wrap removal/addition in try/catch; preserve current theme on new-theme load failure (FR-003)
- [x] T005 Add `ThemeChanged` event thread safety in `Services/ThemeManager.cs` — dispatch via `Application.Current.Dispatcher.InvokeAsync()` (FR-004)
- [x] T006 Add theme switch debouncing in `Services/ThemeManager.cs` — coalesce rapid requests with ≈150ms window (FR-002, US-3)

### New Token Definitions

- [x] T007 [P] Define `Brush.Background.Root` (LinearGradientBrush) in `Theme/Base/Colors.xaml`
- [x] T008 [P] Define `Brush.Accent.ProgressFill` (LinearGradientBrush) in `Theme/Base/Colors.xaml`
- [x] T009 [P] Define `Brush.Stroke.Info` (SolidColorBrush) in `Theme/Base/Colors.xaml`
- [x] T010 [P] Define `Brush.Stroke.Status` (SolidColorBrush) in `Theme/Base/Colors.xaml`
- [x] T011 [P] Define `Brush.Fill.Info` (SolidColorBrush) in `Theme/Base/Colors.xaml`
- [x] T012 [P] Define `Brush.Fill.Status` (SolidColorBrush) in `Theme/Base/Colors.xaml`
- [x] T013 [P] Add dark overrides for all new tokens in `Theme/Dark/DarkColors.xaml`
- [x] T014 [P] Add light overrides for all new tokens in `Theme/Light/LightColors.xaml`
- [x] T015 [P] Add custom overrides for all new tokens in `Theme/Custom/CustomColors.xaml`
- [x] T016 Add `ProgressGlow` DropShadowEffect to `Theme/Effects/Shadows.xaml` with `Color="{DynamicResource AccentColorValue}"`

**Checkpoint**: Foundation ready — ThemeManager is bug-free and new tokens exist. User story implementation can now begin in parallel.

---

## Phase 3: User Story 3 — No Crashes on Startup or Theme Change (Priority: P1)

**Goal**: Eliminate P0 runtime crash risks from duplicate resource definitions, orphaned files, and redundant dictionary loads.

**Independent Test**: Open every window sequentially; switch themes rapidly 10 times; verify zero XAML parse exceptions.

### Implementation for User Story 3

- [x] T017 [P] [US3] Remove duplicate converters from `App.xaml` (keep in `ThemeResources.xaml`) (FR-005)
- [x] T018 [P] [US3] Remove duplicate dictionary loads (`Shadows.xaml`, `Glow.xaml`, `ThemeCardStyles.xaml`, `AccentSwatchStyles.xaml`) from `Views/SettingsWindow.xaml` (FR-006)
- [x] T019 [P] [US3] Remove orphaned `FluentWhite.xaml` reference from `Som3a_WPF_UI.csproj` (FR-013)
- [x] T020 [P] [US3] Remove orphaned `FluentEffects.xaml` file and its references from `App.xaml` and `Theme/ThemeResources.xaml` (FR-010)
- [x] T021 [P] [US3] Remove commented-out legacy theme imports from `App.xaml` (FR-014)

**Checkpoint**: At this point, opening any window or switching themes should produce zero XAML parse exceptions.

---

## Phase 4: User Story 1 — Theme Switching Works Reliably Across All Windows (Priority: P1) 🎯 MVP

**Goal**: Switch between Dark, Light, and Custom themes and see every window update immediately — root background, cards, controls, and progress bars.

**Independent Test**: Open Settings, click each theme card (Dark/Light/Custom), verify ALL open windows reflect the new theme instantly within 200ms.

### Implementation for User Story 1

- [x] T022 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `Fixpiecolors.xaml` (FR-007)
- [x] T023 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `Float_path.xaml` (FR-007)
- [x] T024 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `LinksManagerWindow.xaml` (FR-007)
- [x] T025 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `SubDailyReportWindow.xaml` (FR-007)
- [x] T026 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `UnmergeFillDownWindow.xaml` (FR-007)
- [x] T027 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `StyleSelectorWindow.xaml` (FR-007)
- [x] T028 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `XerEditorWindow.xaml` (FR-007)
- [x] T029 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `AssignTradeCodesWindow.xaml` (FR-007)
- [x] T030 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `ProjectAnalysisWindow.xaml` (FR-007)
- [x] T031 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `PrimaveraCompareWindow.xaml` (FR-007)
- [x] T032 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `PrimaveraResultsWindow.xaml` (FR-007)
- [x] T033 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` in `Views/SettingsWindow.xaml` (FR-007)

**Checkpoint**: At this point, switching themes should update all 12 window backgrounds correctly and within 200ms.

---

## Phase 5: User Story 2 — Accent Color Changes Reflect Everywhere (Priority: P1)

**Goal**: Change the accent color and see it applied to all accent-dependent UI — button fills, selection highlights, progress bar fills, and glow effects.

**Independent Test**: Select a new accent swatch in Settings; verify buttons, progress bars, and glow effects use the new color. Switch themes without changing accent — verify accent persists.

### Implementation for User Story 2

- [x] T034 [P] [US2] Replace progress bar gradient with `{DynamicResource Brush.Accent.ProgressFill}` in `LinksManagerWindow.xaml` (FR-008)
- [x] T035 [P] [US2] Replace progress bar gradient with `{DynamicResource Brush.Accent.ProgressFill}` in `SubDailyReportWindow.xaml` (FR-008)
- [x] T036 [P] [US2] Replace progress bar gradient with `{DynamicResource Brush.Accent.ProgressFill}` in `UnmergeFillDownWindow.xaml` (FR-008)
- [x] T037 [P] [US2] Replace progress bar gradient with `{DynamicResource Brush.Accent.ProgressFill}` in `AssignTradeCodesWindow.xaml` (FR-008)
- [x] T038 [P] [US2] Replace progress bar gradient with `{DynamicResource Brush.Accent.ProgressFill}` in `ProjectAnalysisWindow.xaml` (FR-008)
- [x] T039 [US2] Fix `Background="White"` and `Foreground="Black"` in `Theme/ModernWindow.xaml` to use `{DynamicResource BackgroundBrush}` and `{DynamicResource TextMainBrush}` (FR-011)
- [x] T040 [US2] Replace inline DropShadowEffect on WindowBorder in `Theme/ModernWindow.xaml` with `Effect="{DynamicResource Shadow.Window}"` (FR-012)
- [x] T041 [US2] Fix TreeView hover/selected colors in `Float_path.xaml` to use `{DynamicResource AccentBrush}` at reduced opacity (FR-017)

**Checkpoint**: At this point, accent color changes should reflect in progress bars, glow effects, and button hover states across all affected windows.

---

## Phase 6: User Story 4 — Progress Bars Display Correctly in All Themes (Priority: P2)

**Goal**: Progress bars always show visible, correctly-colored fill regardless of which theme is active.

**Independent Test**: Trigger a progress operation in Dark theme, then switch to Light — verify progress fill and percentage text are visible in both.

### Implementation for User Story 4

- [x] T042 [P] [US4] Replace `Foreground="White"` with `{DynamicResource TextOnAccentBrush}` on progress percent text in `LinksManagerWindow.xaml` (FR-009)
- [x] T043 [P] [US4] Replace `Foreground="White"` with `{DynamicResource TextOnAccentBrush}` on progress percent text in `SubDailyReportWindow.xaml` (FR-009)
- [x] T044 [P] [US4] Replace `Foreground="White"` with `{DynamicResource TextOnAccentBrush}` on progress percent text in `UnmergeFillDownWindow.xaml` (FR-009)
- [x] T045 [P] [US4] Replace `Foreground="White"` with `{DynamicResource TextOnAccentBrush}` on progress percent text in `ProjectAnalysisWindow.xaml` (FR-009)
- [x] T046 [P] [US4] Replace inline DropShadowEffect on progress bar in `SubDailyReportWindow.xaml` with `<Border.Effect><StaticResource ResourceKey="ProgressGlow"/></Border.Effect>` (FR-015)
- [x] T047 [P] [US4] Replace inline DropShadowEffect on progress bar in `ProjectAnalysisWindow.xaml` with `<Border.Effect><StaticResource ResourceKey="ProgressGlow"/></Border.Effect>` (FR-015)

**Checkpoint**: At this point, progress bars should display correctly in both Dark and Light themes with visible text and accent-colored fill.

---

## Phase 7: User Story 5 — Window Background Matches the Active Theme (Priority: P2)

**Goal**: Every window's background matches the selected theme — not stuck on the dark gradient. Also fix remaining hardcoded colors, missing foregrounds, and border tokens.

**Independent Test**: Switch to Light theme; verify every window's root background is light-colored and all text/borders are visible.

### Implementation for User Story 5

- [x] T048 [P] [US5] Add missing `Foreground="{DynamicResource TextMainBrush}"` to TextBlock in `PrimaveraResultsWindow.xaml` (FR-016)
- [x] T049 [P] [US5] Add missing `Foreground="{DynamicResource TextMainBrush}"` to TextBlock in `Views/SettingsWindow.xaml` (FR-016)
- [x] T050 [P] [US5] Replace `Foreground="LimeGreen"` with `{DynamicResource SuccessBrush}` in `SubDailyReportWindow.xaml` (FR-018)
- [x] T051 [P] [US5] Replace hardcoded close button hover background `#22FF4757` with `{DynamicResource DangerBrush}` at 13% opacity in `Theme/WindowAnimations.xaml` (FR-018)
- [x] T052 [P] [US5] Replace hardcoded `#12FFFFFF` border fills/strokes with `{DynamicResource Brush.Fill.Info}` / `{DynamicResource Brush.Stroke.Info}` across all affected windows (FR-019)
- [x] T053 [P] [US5] Replace hardcoded `#18FFFFFF` border fills/strokes with `{DynamicResource Brush.Fill.Status}` / `{DynamicResource Brush.Stroke.Status}` across all affected windows (FR-019)
- [x] T054 [P] [US5] Replace hardcoded `#1FFFFFFF` border fills/strokes with per-theme resource tokens across all affected windows (FR-019)
- [x] T055 [P] [US5] Replace hardcoded `#22FFFFFF` border fills/strokes with per-theme resource tokens across all affected windows (FR-019)

**Checkpoint**: At this point, all 12 windows should have theme-aware backgrounds, visible text, and no hardcoded border colors.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Verification, build validation, and constitutional compliance

- [x] T056 Build verification: run `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` and confirm zero errors
- [ ] T057 Runtime check: launch in Excel VSTO host, verify Dark → Light → Custom switching updates ALL open window backgrounds
- [ ] T058 Runtime check: verify accent color change updates glow effects and progress bar fills
- [ ] T059 Runtime check: rapidly switch themes 10 times, confirm no crash and only final theme applied
- [ ] T060 Regression: verify progress bar displays in all 7 windows with progress bars
- [ ] T061 Regression: verify TreeView hover/selected highlight works in `Float_path.xaml`
- [ ] T062 Regression: verify close button hover color is visible in `Theme/WindowAnimations.xaml`
- [x] T063 [P] Grep audit: verify zero hardcoded `#HEX` colors remain in any window `.xaml` file (SC-003)
- [x] T064 Constitution compliance review: verify DynamicResource-only usage, ThemeManager integration, no inline effects, and WindowChrome inheritance across all modified files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
  - ThemeManager fixes (T002–T006) can be worked on sequentially in the same file
  - Token definitions (T007–T016) can run in parallel with each other
- **User Stories (Phase 3–7)**: All depend on Foundational phase completion
  - User Story 3 (crash fixes) can start in parallel with others but should complete early
  - User Stories 1, 2, 4, 5 can proceed in parallel (if staffed) or sequentially
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 3 (P1)**: Can start after Foundational — No dependencies on other stories. Should be done early to remove crash risks.
- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories. 🎯 **MVP scope**.
- **User Story 2 (P1)**: Can start after Foundational — May integrate with US1 (theme switching triggers accent re-application) but is independently testable.
- **User Story 4 (P2)**: Can start after Foundational — Depends on US2 for progress bar fill tokens but is independently testable.
- **User Story 5 (P2)**: Can start after Foundational — Depends on US1 for root backgrounds but covers additional window polish.

### Within Each User Story

- Story 3: All tasks are [P] — can run in parallel (different files)
- Story 1: All tasks are [P] — can run in parallel (different windows)
- Story 2: T034–T038 are [P] (different windows); T039–T041 are sequential (same file or dependent)
- Story 4: All tasks are [P] — can run in parallel (different windows)
- Story 5: All tasks are [P] — can run in parallel (different windows/files)

### Parallel Opportunities

- All token definitions (T007–T016) can run in parallel
- All XAML cleanup tasks (T017–T021) can run in parallel
- All window root background replacements (T022–T033) can run in parallel
- All progress bar gradient replacements (T034–T038) can run in parallel
- All progress bar text fixes (T042–T045) can run in parallel
- All border color fixes (T052–T055) can run in parallel
- All runtime/regression checks (T056–T064) are sequential but quick

---

## Parallel Example: User Story 1

```powershell
# Launch all root background replacements in parallel:
Task: "Replace root background in Fixpiecolors.xaml"
Task: "Replace root background in Float_path.xaml"
Task: "Replace root background in LinksManagerWindow.xaml"
Task: "Replace root background in SubDailyReportWindow.xaml"
Task: "Replace root background in UnmergeFillDownWindow.xaml"
Task: "Replace root background in StyleSelectorWindow.xaml"
Task: "Replace root background in XerEditorWindow.xaml"
Task: "Replace root background in AssignTradeCodesWindow.xaml"
Task: "Replace root background in ProjectAnalysisWindow.xaml"
Task: "Replace root background in PrimaveraCompareWindow.xaml"
Task: "Replace root background in PrimaveraResultsWindow.xaml"
Task: "Replace root background in Views/SettingsWindow.xaml"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (ThemeManager fixes + tokens)
3. Complete Phase 3: User Story 3 (remove crash risks)
4. Complete Phase 4: User Story 1 (root backgrounds — core theme switching)
5. **STOP and VALIDATE**: Test theme switching across all 12 windows
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational + US3 → Crash-free baseline
2. Add User Story 1 → Test theme switching independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test accent color independently → Deploy/Demo
4. Add User Story 4 → Test progress bars independently → Deploy/Demo
5. Add User Story 5 → Test window polish independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 3 (crash fixes) + User Story 1 (root backgrounds)
   - Developer B: User Story 2 (accent color + progress fills + ModernWindow)
   - Developer C: User Story 4 (progress bar text/shadows) + User Story 5 (borders + missing foregrounds)
3. Stories complete and integrate independently
4. Team runs Phase 8 polish together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify `msbuild` after each major phase (especially after Phase 2 and Phase 3)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- **Token convention reminder**: All new tokens MUST use `Brush.*` semantic namespace (e.g., `Brush.Background.Root`). Legacy flat keys (e.g., `BackgroundBrush`) are retained for backward compatibility but MUST NOT be used for new work.
