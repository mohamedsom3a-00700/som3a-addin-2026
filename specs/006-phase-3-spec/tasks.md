---
description: "Task list for Theme Engine 2.0 implementation"
---

# Tasks: Theme Engine 2.0

**Input**: Design documents from `/specs/006-phase-3-spec/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/theme-api.md, quickstart.md

**Tests**: Visual/manual testing only (WPF UI, no automated UI testing framework)

**Organization**: Tasks grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label (US1-US5), setup tasks have no label
- Include exact file paths in descriptions

## Path Conventions

- Single WPF project: `WpfApp2/` at repository root
- All paths relative to `WpfApp2/` unless prefixed with repo root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create centralized effects library and application settings — the foundation everything else depends on

- [x] T001 [P] Create `WpfApp2/Theme/Effects/Shadows.xaml` with all centralized DropShadowEffect definitions (Shadow.Window, Shadow.Popup, Shadow.Popup.Small, Shadow.Card, Shadow.Small, Shadow.Medium, Shadow.Large, Shadow.Window.Safe, Shadow.Card.Safe, Shadow.Popup.Safe)
- [x] T002 [P] Create `WpfApp2/Theme/Effects/Glow.xaml` with all centralized glow effects (Glow.Focus, Glow.ButtonHover, Glow.Primary, Glow.Selection, Glow.Accent, Glow.ThemeCard.Selected)
- [x] T003 [P] Create `WpfApp2/Theme/Effects/Animations.xaml` with control state + popup Storyboard animations (HoverEnter/Exit, FocusEnter/Exit, PopupOpen/Close, FadeIn/Out, all ≤200ms with CubicEase)
- [x] T004 [P] Create `WpfApp2/Properties/Settings.settings` with SelectedTheme and AccentColor user-scoped settings
- [x] T005 Create `WpfApp2/Properties/Settings.Designer.cs` accessor for Settings
- [x] T006 Create `Docs/Architecture/EXCEL_TEST_CHECKLIST.md` with 8+ manual test scenarios

**Checkpoint**: Effects library and persistence foundation ready — enables all subsequent phases

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Theme system core and render mode detection — MUST complete before ANY user story begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Theme Dictionaries & ThemeManager

- [x] T007 [P] Create `WpfApp2/Theme/Dark/DarkColors.xaml` with Dark theme semantic token overrides
- [x] T008 [P] Create `WpfApp2/Theme/Light/LightColors.xaml` with Light theme semantic token overrides
- [x] T009 [P] Create `WpfApp2/Theme/Custom/CustomColors.xaml` with Custom theme semantic overrides and 8 accent swatch definitions
- [x] T010 [P] Create `WpfApp2/Theme/Dark/DarkTheme.xaml` merged ResourceDictionary
- [x] T011 [P] Create `WpfApp2/Theme/Light/LightTheme.xaml` merged ResourceDictionary
- [x] T012 [P] Create `WpfApp2/Theme/Custom/CustomTheme.xaml` merged ResourceDictionary with accent token
- [x] T013 Create `WpfApp2/Services/ThemeManager.cs` singleton with ApplyTheme(), LoadThemeFromSettings(), SaveCurrentTheme(), ThemeChanged event
- [x] T014 Integrate ThemeManager into `WpfApp2/Controls/ModernWindow.cs` — call LoadThemeFromSettings() on startup

### ThemeManager Bug Fixes (002-themes-manager)

- [x] T015 Fix accent persistence in `Services/ThemeManager.cs` — preserve `_currentAccentColor` when accentColor is null
- [x] T016 Fix early-return logic — reorder `_currentTheme` update to prevent duplicate work
- [x] T017 Implement safe dictionary fallback — wrap removal/addition in try/catch
- [x] T018 Add `ThemeChanged` event thread safety — dispatch via `Dispatcher.InvokeAsync()`
- [x] T019 Add theme switch debouncing — ≈150ms coalesce window

### New Semantic Tokens

- [x] T020 [P] Define `Brush.Background.Root` (LinearGradientBrush) in `Theme/Base/Colors.xaml`
- [x] T021 [P] Define `Brush.Accent.ProgressFill` (LinearGradientBrush) in `Theme/Base/Colors.xaml`
- [x] T022 [P] Define `Brush.Stroke.Info`, `Brush.Stroke.Status`, `Brush.Fill.Info`, `Brush.Fill.Status` in `Theme/Base/Colors.xaml`
- [x] T023 [P] Add Dark/Light/Custom overrides for all new tokens
- [x] T024 Add `ProgressGlow` DropShadowEffect to `Theme/Effects/Shadows.xaml` with `Color="{DynamicResource AccentColorValue}"`

### RenderModeService

- [x] T025 Create `WpfApp2/Services/RenderModeService.cs` with Initialize(), GetCurrentMode(), IsSafeModeRequired(), RenderModeChanged event
- [x] T026 [P] Integrate WindowRenderModeDetector as detection backend
- [x] T027 [P] Add RenderMode enum (WindowChrome, FallbackSafe)
- [x] T028 Implement fallback logic: on initialization failure, default to FallbackSafe

**Checkpoint**: Foundation ready — ThemeManager operational, all 3 themes functional, persistence working, render mode detection in place. User story implementation can now begin.

---

## Phase 3: User Story 1 - Theme Selection and Switching (Priority: P1) 🎯 MVP

**Goal**: User opens SettingsWindow, clicks a theme card, theme switches instantly and persists across restarts. Accent color selection works for Custom theme.

**Independent Test**: Open SettingsWindow → click Light card → all windows update → restart app → Light theme restored. Select Custom + accent swatch → verify accent applied everywhere.

### Implementation for User Story 1

- [x] T029 [P] [US1] Create `WpfApp2/Theme/Controls/ThemeCardStyles.xaml` with selected/hover glow styles and scale animations (Glow.ThemeCard.Selected + scale 1.02)
- [x] T030 [P] [US1] Create `WpfApp2/Theme/Controls/AccentSwatchStyles.xaml` with clickable color circle styles (accent fill ring when selected)
- [x] T031 [US1] Refactor `WpfApp2/Views/SettingsWindow.xaml` — replace ComboBox theme dropdown with 3 theme card Borders (Dark, Light, Custom) with preview gradients and glow selection state
- [x] T032 [US1] Add 8 accent color swatch circles to Custom theme card in `WpfApp2/Views/SettingsWindow.xaml`
- [x] T033 [US1] Wire theme card clicks to ThemeManager.ApplyTheme() in `WpfApp2/Views/SettingsWindow.xaml.cs`
- [x] T034 [US1] Wire accent swatch clicks to ThemeManager.ApplyTheme("Custom", swatchHex) in `WpfApp2/Views/SettingsWindow.xaml.cs`
- [x] T035 [US1] Set initial theme card selection state on SettingsWindow load based on CurrentTheme
- [x] T036 [US1] Add glow effect to selected theme card border via DynamicResource

### Window Background Migration (12 windows)

- [x] T037–T048 [P] [US1] Replace root background gradient with `{DynamicResource Brush.Background.Root}` across all 12 window files (Fixpiecolors, Float_path, LinksManagerWindow, SubDailyReportWindow, UnmergeFillDownWindow, StyleSelectorWindow, XerEditorWindow, AssignTradeCodesWindow, ProjectAnalysisWindow, PrimaveraCompareWindow, PrimaveraResultsWindow, SettingsWindow)

### Crash-Risk Removal

- [x] T049 [P] [US1] Remove duplicate converters from `App.xaml` (keep in ThemeResources.xaml)
- [x] T050 [P] [US1] Remove duplicate dictionary loads from `Views/SettingsWindow.xaml`
- [x] T051 [P] [US1] Remove orphaned `FluentWhite.xaml` reference from `Som3a_WPF_UI.csproj`
- [x] T052 [P] [US1] Remove orphaned `FluentEffects.xaml` file and references from App.xaml and ThemeResources.xaml
- [x] T053 [P] [US1] Remove commented-out legacy theme imports from App.xaml

- [x] T054 [US1] Validate theme switch in under 1 second (SC-001), persistence across restarts (SC-002), and all windows update simultaneously (SC-003)

**Checkpoint**: Theme cards working — user can select Dark/Light/Custom + accent color, switch is instant, preference persists across restarts

---

## Phase 4: User Story 2 - Consistent Visual Quality Across Controls (Priority: P1)

**Goal**: All controls display correct hover, focus, pressed, disabled states with smooth transitions and proper popup rendering.

**Independent Test**: Iterate all 7 controls in Dark + Light theme → verify hover/focus/pressed/disabled states + ComboBox popup renders above content with shadow

### Implementation for User Story 2

- [x] T055 [P] [US2] Refactor `WpfApp2/Theme/Controls/ButtonStyles.xaml` — add VSM states (Normal, Hover, Pressed, Focused, Disabled) with glow transitions and accent border on hover/focus
- [x] T056 [P] [US2] Refactor `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — fix AllowsTransparency=False, add PlacementTarget binding, reference Shadow.Popup, add disabled state trigger
- [x] T057 [P] [US2] Create `WpfApp2/Theme/Controls/CheckBoxStyles.xaml` with VSM states (Normal, Hover, Checked, Focused, Disabled) and custom checkmark design
- [x] T058 [P] [US2] Create `WpfApp2/Theme/Controls/RadioButtonStyles.xaml` with VSM states and custom dot indicator
- [x] T059 [P] [US2] Create `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml` with VSM states (Normal, Hover, Checked, Focused, Disabled) and slide animation
- [x] T060 [P] [US2] Refactor `WpfApp2/Theme/Controls/TextBoxStyles.xaml` — add all VSM states with accent border transitions
- [x] T061 [P] [US2] Refactor `WpfApp2/Theme/Controls/DataGridStyles.xaml` — add EnableRowVirtualization, alternating row colors via DynamicResource, hover/selection states
- [x] T062 [P] [US2] Create `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` with modern thin scrollbar (4px track, 8px thumb on hover)

### Accent Color Application (5 windows)

- [x] T063–T067 [P] [US2] Replace progress bar gradients with `{DynamicResource Brush.Accent.ProgressFill}` across 5 windows (LinksManagerWindow, SubDailyReportWindow, UnmergeFillDownWindow, AssignTradeCodesWindow, ProjectAnalysisWindow)
- [x] T068 [US2] Fix ModernWindow.xaml hardcoded `Background="White"` / `Foreground="Black"` to use DynamicResource
- [x] T069 [US2] Replace inline DropShadowEffect on WindowBorder with `Shadow.Window`
- [x] T070 [US2] Fix TreeView hover/selected colors in Float_path.xaml to use DynamicResource

- [x] T071 [US2] Verify all states render correctly in both Dark and Light themes (SC-004, SC-005)
- [x] T072 [US2] Verify ComboBox popup renders above all content with visible shadow and no clipping (SC-008)

**Checkpoint**: All 7 control types show all interactive states in both themes, popups render correctly with shadows, accent color applied across all windows

---

## Phase 5: User Story 3 - DPI-Aware Rendering (Priority: P2)

**Goal**: All UI elements scale correctly at 100%, 125%, 150%, 200% DPI with no text clipping or overflow.

**Independent Test**: Run app at each DPI setting → verify SettingsWindow, ComboBox, DataGrid, theme cards all scale proportionally

### Implementation for User Story 3

- [x] T073 [P] [US3] Audit all XAML files for hardcoded pixel values — replace with DynamicResource for radius/padding/margin
- [x] T074 [P] [US3] Audit all control templates for SnapsToDevicePixels and UseLayoutRounding — ensure present on all controls
- [x] T075 [P] [US3] Implement GetCurrentDpiScale() in `WpfApp2/Helpers/DpiHelper.cs`
- [x] T076 [P] [US3] Implement ScaleValue(double) and IsHighDpi() in DpiHelper.cs
- [x] T077 [US3] Add DPI-aware sizing to ComboBox popup — Popup with Placement="Bottom" + PlacementTarget binding
- [x] T078 [US3] Add DPI-aware sizing to theme card preview thumbnails — DynamicResource themecard width/height
- [x] T079 [US3] Add DPI-aware sizing to accent swatch circles — DynamicResource swatch size
- [x] T080 [US3] Integrate DpiHelper with ModernWindow — bind DpiScale DP

- [x] T081 [US3] Validate DPI scaling at 100%, 125%, 150%, 200% (SC-009) — verify no text clipping, no element overflow, correct popup sizing

**Checkpoint**: UI renders correctly at all supported DPI levels

---

## Phase 6: User Story 4 - Stable Performance Inside Excel (Priority: P2)

**Goal**: UI remains stable and responsive inside Excel VSTO — no freezing, no frame drops, smooth scrolling.

**Independent Test**: Run add-in inside Excel → scroll DataGrid 500+ rows → switch themes 10 times rapidly → verify no UI freezing or stuttering

### Implementation for User Story 4

- [x] T082 [P] [US4] Audit visual trees for nested DropShadowEffect — fix any found outside Effects/
- [x] T083 [P] [US4] Audit BlurEffect usage — ensure zero BlurEffect on scrollable containers
- [x] T084 [US4] Add ModernWindow DPs: RenderMode, IsSafeMode, WindowBackdrop, DpiScale
- [x] T085 [US4] Refactor InitializeWindow() in ModernWindow to use RenderModeService.IsSafeModeRequired()
- [x] T086–T099 [P] [US4] Apply per-window safe-mode changes to all 14 windows (MainWindow, Float_path, AssignTradeCodesWindow, Fixpiecolors, LinksManagerWindow, StyleSelectorWindow, SubDailyReportWindow, UnmergeFillDownWindow, XerEditorWindow, ProjectAnalysisWindow, SettingsWindow, PrimaveraCompareWindow, PrimaveraResultsWindow, ToastWindow)
- [x] T100 [US4] Add safe-mode gating to WindowAnimations.xaml — skip animations in FallbackSafe mode

### Progress Bar Fixes

- [x] T101–T104 [P] [US4] Replace `Foreground="White"` with `TextOnAccentBrush` on progress percent text
- [x] T105–T106 [P] [US4] Replace inline DropShadowEffect on progress bars with `ProgressGlow`

- [x] T107 [US4] Validate DataGrid scrolling with 1000+ rows (virtualization enabled, no frame drops) (SC-006)
- [x] T108 [US4] Validate theme switching 10 times in rapid succession inside Excel — no UI freezing (SC-006)
- [x] T109 [US4] Runtime check: verify accent color change updates glow effects and progress bar fills
- [x] T110 [US4] Regression: verify progress bar displays in all 7 windows with progress bars
- [x] T111 [US4] Regression: verify TreeView hover/selected highlight in Float_path.xaml
- [x] T112 [US4] Regression: verify close button hover color visible

**Checkpoint**: VSTO performance validated — UI stable inside Excel, fallback-safe mode self-activates on edge cases

---

## Phase 7: User Story 5 - Keyboard Navigation and Accessibility (Priority: P3)

**Goal**: All interactive elements reachable and activatable via keyboard; WCAG 2.1 AA contrast ratios met.

**Independent Test**: Navigate entire app using only Tab, Arrow keys, Enter, Space, Escape → verify all elements reachable → check contrast ratios

### Implementation for User Story 5

- [x] T113 [P] [US5] Audit Tab order in `WpfApp2/Views/SettingsWindow.xaml` — ensure logical Tab order
- [x] T114 [P] [US5] Add AutomationProperties.Name to theme cards and accent swatches
- [x] T115 [US5] Verify all theme cards activatable via Enter/Space — add PreviewKeyDown handlers
- [x] T116 [P] [US5] Add FocusVisualStyle using Glow.Focus to all interactive controls
- [x] T117 [P] [US5] Add AutomationProperties.Name to all interactive elements across all 14 windows
- [x] T118 [P] [US5] Add AutomationProperties.HelpText on complex controls (DataGrid, ListView)
- [x] T119 [US5] Add high contrast awareness — use system colors when high contrast is active

### Window Polish (border/foreground fixes)

- [x] T120 [P] [US5] Add missing `Foreground` to PrimaveraResultsWindow TextBlock
- [x] T121 [P] [US5] Add missing `Foreground` to SettingsWindow TextBlock
- [x] T122 [P] [US5] Replace `Foreground="LimeGreen"` with `SuccessBrush`
- [x] T123 [P] [US5] Replace close button hover `#22FF4757` with `DangerBrush` (13% opacity)
- [x] T124–T127 [P] [US5] Replace hardcoded `#12FFFFFF`, `#18FFFFFF`, `#1FFFFFFF`, `#22FFFFFF` border fills/strokes with per-theme resource tokens

- [x] T128 [US5] Verify focus indicators visible on all interactive elements (focus glow) across both Dark and Light themes (SC-007)
- [x] T129 [US5] Validate WCAG 2.1 AA contrast ratios (4.5:1) for all text elements in Dark and Light themes (SC-005)
- [x] T130 [US5] Validate keyboard navigation for ComboBox (Tab → open with Arrow keys → close with Escape)

**Checkpoint**: Full keyboard navigation working, focus indicators visible, WCAG contrast ratios met

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, and final quality pass

- [x] T131 [P] Run 8-gate Theme Validation Checklist for Dark theme (MANUAL)
- [x] T132 [P] Run 8-gate Theme Validation Checklist for Light theme (MANUAL)
- [x] T133 Run 8-gate Theme Validation Checklist for Custom theme with at least 2 accent variations (MANUAL)
- [x] T134 [P] Update `WpfApp2/Theme/ThemeResources.xaml` to document correct resource loading order in comments
- [x] T135 Remove hardcoded color values from control templates — grep audit zero `#HEX` in window .xaml files
- [x] T136 Remove inline DropShadowEffect definitions — zero inline outside Effects/ (document exceptions)
- [x] T137 Update `AGENTS.md` to reflect final implementation structure
- [x] T138 Run quickstart.md validation — build succeeds, app launches, theme switching works
- [x] T139 Build verification — `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — zero errors
- [x] T140 Constitution compliance review — verify DynamicResource-only, ThemeManager integration, Excel rendering safety, WindowChrome inheritance

- [x] T141 Create build-time token validation script `scripts/Validate-Tokens.ps1`
- [x] T142 Run `scripts/Validate-Tokens.ps1` and fix violations until exit code is 0
- [x] T143 Run manual theme switching regression test — cycle Dark → Light → Custom, verify no visual regressions
- [x] T144 Verify ThemeManager token integrity validation produces zero warnings for all 3 themes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user stories**
- **User Stories (Phase 3-7)**: All depend on Foundational
  - US1 (P1) and US2 (P1) are independent — can run in parallel
  - US3 (P2) depends on US1 window refactoring (DPI-aware sizing on existing controls)
  - US4 (P2) depends on US2 shadow/effect work and US1 window refactoring
  - US5 (P3) depends on US1 and US2 (windows and controls must exist before accessibility can be added)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories. **This is the MVP.**
- **User Story 2 (P1)**: Can start after Foundational — Independent of US1 (different files/concerns). Independently testable.
- **User Story 3 (P2)**: Depends on US1 — DPI fixes apply to US1-created controls and refactored windows.
- **User Story 4 (P2)**: Depends on US2 (shadow system) + US1 (window refactoring) — Performance fixes build on both.
- **User Story 5 (P3)**: Depends on US1 (windows) + US2 (controls) — Accessibility is the final polish layer.

### Within Each User Story

- Styles and templates can be parallel (different XAML files)
- Code-behind wiring depends on styles being complete
- Validation depends on implementation being complete

### Parallel Opportunities

- Setup: All tasks (T001-T006) can run in parallel
- Foundational: Theme dictionaries and tokens (T007-T028) can run in parallel
- US1: ThemeCardStyles + AccentSwatchStyles (T029-T030) can run in parallel; all 12 window migrations (T037-T048) run in parallel
- US2: All 7 control style tasks (T055-T062) can run in parallel
- US3: DPI helper methods (T075-T076) can run in parallel with audit tasks
- US4: All 14 window refactoring tasks (T086-T099) can run in parallel
- US5: Tab order audit + AutomationProperties (T113-T118) can run in parallel

---

## Parallel Example: User Story 1

```powershell
# Launch parallel tasks:
Task: "Create ThemeCardStyles.xaml with glow animations"
Task: "Create AccentSwatchStyles.xaml"
# Then sequential:
Task: "Refactor SettingsWindow.xaml with theme cards"
Task: "Wire card clicks to ThemeManager"

# Launch all 12 window background replacements in parallel:
Task: "Fixpiecolors.xaml → Brush.Background.Root"
Task: "Float_path.xaml → Brush.Background.Root"
Task: "LinksManagerWindow.xaml → Brush.Background.Root"
# ... (all 12 windows)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational — **CRITICAL, blocks everything**
3. Complete Phase 3: User Story 1 — **This is the MVP**
4. **STOP and VALIDATE**: Theme cards working, theme switching instant, persistence across restarts
5. Deploy/demo MVP

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Theme Selection) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Visual Quality) → Test independently → Deploy/Demo
4. Add US3 (DPI Scaling) → Test independently → Deploy/Demo
5. Add US4 (Excel Stability) → Test independently → Deploy/Demo
6. Add US5 (Accessibility) → Test independently → Deploy/Demo
7. Polish → Final validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Theme Selection) + US3 (DPI)
   - Developer B: US2 (Visual Quality) + US4 (Excel Stability)
   - Developer C: US5 (Accessibility)
3. Stories complete and integrate independently
4. Team runs Phase 8 polish together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Visual/manual testing — no automated UI tests
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: hardcoded colors in XAML, inline DropShadowEffect, animation > 200ms

## Task Summary

| Phase | Tasks | Description | Status |
|-------|-------|-------------|--------|
| Phase 1: Setup | T001-T006 | Effects library + persistence + test checklist | ✅ Complete |
| Phase 2: Foundational | T007-T028 | Theme dictionaries + ThemeManager + RenderModeService | ✅ Complete |
| Phase 3: US1 (P1) MVP | T029-T054 | Theme cards + accent swatches + window backgrounds | ✅ 26/26 complete |
| Phase 4: US2 (P1) | T055-T072 | Control styles + VSM states + accent application | ✅ 19/19 complete |
| Phase 5: US3 (P2) | T073-T081 | DPI-aware sizing + helper methods | ✅ 9/9 complete |
| Phase 6: US4 (P2) | T082-T112 | Excel stability + safe mode + progress bars | ✅ 31/31 complete |
| Phase 7: US5 (P3) | T113-T130 | Keyboard nav + accessibility + window polish | ✅ 21/21 complete |
| Phase 8: Polish | T131-T144 | Validation + documentation + linting | ✅ 14/14 complete |

**Total: 144 tasks** | **✅ 144 complete** | **❌ 0 remaining**

All 144 tasks completed — Theme Engine 2.0 implementation is fully done.
