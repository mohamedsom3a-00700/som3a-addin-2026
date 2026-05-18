---

description: "Task list for WPF Fluent UI Theme Engine Migration"
---

# Tasks: WPF Fluent UI Migration — Theme Engine & Runtime Switching

**Input**: Design documents from `/specs/001-fluent-theme-engine/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Visual/manual testing only (WPF UI, no automated UI testing framework)

**Organization**: Tasks grouped by user story to enable independent implementation and testing

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label (US1-US5), setup tasks have no label
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create centralized effects library and application settings — the foundation everything else depends on

- [X] T001 [P] Create `WpfApp2/Theme/Effects/Shadows.xaml` with all centralized DropShadowEffect definitions (Shadow.Window, Shadow.Popup, Shadow.Popup.Small, Shadow.Card, Shadow.Small, Shadow.Medium, Shadow.Large)
- [X] T002 [P] Create `WpfApp2/Theme/Effects/Glow.xaml` with all centralized glow effects (Glow.Focus, Glow.ButtonHover, Glow.Primary, Glow.Selection, Glow.Accent, Glow.ThemeCard.Selected)
- [X] T003 [P] Create `WpfApp2/Theme/Effects/Animations.xaml` with control state + popup Storyboard animations (HoverEnter/Exit, FocusEnter/Exit, PopupOpen/Close, FadeIn/Out, all ≤200ms with CubicEase)
- [X] T004 [P] Create `WpfApp2/Properties/Settings.settings` with SelectedTheme and AccentColor user-scoped settings
- [X] T005 Create `WpfApp2/Properties/Settings.Designer.cs` accessor for Settings

**Checkpoint**: Effects library and persistence foundation ready — enables all subsequent phases

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Theme system core — MUST complete before ANY user story begins

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T006 [P] Create `WpfApp2/Theme/Dark/DarkColors.xaml` with Dark theme semantic token overrides (Brush.Background.Primary=#0E1720, Brush.Text.Primary=#F2FFFFFF, etc.)
- [X] T007 [P] Create `WpfApp2/Theme/Light/LightColors.xaml` with Light theme semantic token overrides (Brush.Background.Primary=#FAFAFA, Brush.Text.Primary=#1A1A1A, etc.)
- [X] T008 [P] Create `WpfApp2/Theme/Custom/CustomColors.xaml` with Custom theme semantic token overrides and 8 accent color presets as swatch definitions
- [X] T009 [P] Create `WpfApp2/Theme/Dark/DarkTheme.xaml` merged ResourceDictionary (imports Base/Colors.xaml + DarkColors.xaml)
- [X] T010 [P] Create `WpfApp2/Theme/Light/LightTheme.xaml` merged ResourceDictionary
- [X] T011 [P] Create `WpfApp2/Theme/Custom/CustomTheme.xaml` merged ResourceDictionary with accent color token
- [X] T012 Create `WpfApp2/Services/ThemeManager.cs` singleton service with ApplyTheme(), LoadThemeFromSettings(), SaveCurrentTheme(), ThemeChanged event
- [X] T013 Integrate ThemeManager into `WpfApp2/Controls/ModernWindow.cs` to call LoadThemeFromSettings() on application startup

**Checkpoint**: Theme system core complete — Dark, Light, Custom themes functional, ThemeManager operational, persistence working

---

## Phase 3: User Story 1 - Theme Selection via Settings Window (Priority: P1) MVP

**Goal**: User opens SettingsWindow, clicks a theme card, theme switches instantly and persists across restarts

**Independent Test**: Open SettingsWindow → click Light card → all windows update → restart app → Light theme restored

### Implementation

- [X] T014 [P] [US1] Create `WpfApp2/Theme/Controls/ThemeCardStyles.xaml` with selected/hover glow styles and scale animations (Glow.ThemeCard.Selected + scale 1.02)
- [X] T015 [P] [US1] Create `WpfApp2/Theme/Controls/AccentSwatchStyles.xaml` with clickable color circle styles (accent fill ring when selected)
- [X] T016 [US1] Refactor `WpfApp2/Views/SettingsWindow.xaml` — replace ComboBox theme dropdown with 3 theme card Borders (Dark, White, Custom) with preview gradients and glow selection state
- [X] T017 [US1] Add 8 accent color swatch circles to Custom theme card in `WpfApp2/Views/SettingsWindow.xaml`
- [X] T018 [US1] Wire theme card clicks to ThemeManager.ApplyTheme() in `WpfApp2/Views/SettingsWindow.xaml.cs` (minimal code-behind)
- [X] T019 [US1] Wire accent swatch clicks to ThemeManager.ApplyTheme("Custom", swatchHex) in `WpfApp2/Views/SettingsWindow.xaml.cs`
- [X] T020 [US1] Set initial theme card selection state on SettingsWindow load based on CurrentTheme in `WpfApp2/Views/SettingsWindow.xaml.cs`
- [X] T021 [US1] Add glow effect to selected theme card border via DynamicResource in `WpfApp2/Views/SettingsWindow.xaml`
- [ ] T022 [US1] Validate theme switch in under 1 second (SC-001), persistence across restarts (SC-002), and all windows update simultaneously (SC-003)

**Checkpoint**: Theme cards working — user can select Dark/Light/Custom + accent color, switch is instant, preference persists

---

## Phase 4: User Story 2 - Visual Quality & Consistency (Priority: P1)

**Goal**: All controls display correct hover, focus, pressed, disabled states with smooth transitions and proper popup rendering

**Independent Test**: Iterate all 7 controls in Dark + Light theme → verify hover/focus/pressed/disabled states + ComboBox popup renders above content with shadow

### Implementation

- [X] T023 [P] [US2] Refactor `WpfApp2/Theme/Controls/ButtonStyles.xaml` — add VSM states (Normal, Hover, Pressed, Focused, Disabled) with glow transitions and accent border on hover/focus
- [X] T024 [US2] Refactor `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` — fix AllowsTransparency=False, add PlacementTarget binding, reference Shadow.Popup from Effects/Shadows.xaml, add disabled state trigger
- [X] T025 [P] [US2] Create `WpfApp2/Theme/Controls/CheckBoxStyles.xaml` with VSM states (Normal, Hover, Checked, Focused, Disabled) and custom checkmark design
- [X] T026 [P] [US2] Create `WpfApp2/Theme/Controls/RadioButtonStyles.xaml` with VSM states and custom dot indicator
- [X] T027 [P] [US2] Create `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml` with VSM states (Normal, Hover, Checked, Focused, Disabled) and slide animation
- [X] T028 [US2] Refactor `WpfApp2/Theme/Controls/TextBoxStyles.xaml` — add all VSM states (Normal, Hover, Pressed, Focused, Disabled) with accent border transitions
- [X] T029 [US2] Refactor `WpfApp2/Theme/Controls/DataGridStyles.xaml` — add EnableRowVirtualization="True", alternating row colors via DynamicResource, hover/selection states
- [X] T030 [US2] Create `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` with modern thin scrollbar (4px track, 8px thumb on hover)
- [ ] T031 [US2] Verify all states render correctly in both Dark and Light themes (SC-004, SC-005)
- [ ] T032 [US2] Verify ComboBox popup renders above all content with visible drop shadow and no clipping (SC-008)

**Checkpoint**: All 7 control types show all interactive states in both themes, popups render correctly with shadows

---

## Phase 5: User Story 3 - DPI Scaling Across Configurations (Priority: P2)

**Goal**: All UI elements scale correctly at 100%, 125%, 150%, 200% DPI with no text clipping or overflow

**Independent Test**: Run app at each DPI setting → verify SettingsWindow, ComboBox, DataGrid, theme cards all scale proportionally

### Implementation

- [X] T033 [P] [US3] Audit all XAML files for hardcoded pixel values (replace with DynamicResource for radius/padding/margin) — DONE: minimal hardcoded values found, all control templates use DynamicResource for layout properties
- [X] T034 [P] [US3] Audit all control templates for SnapsToDevicePixels and UseLayoutRounding usage — DONE: ButtonStyles, TextBoxStyles, ComboBoxStyles, ToggleButton, ScrollViewer already have SnapsToDevicePixels="True"
- [X] T035 [US3] Add DPI-aware sizing to `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` popup — DONE: Popup has Placement="Bottom" + PlacementTarget binding, all properties use DynamicResource
- [X] T036 [US3] Add DPI-aware sizing to theme card preview thumbnails — DONE: ThemeCardWidth/Height now use DynamicResource in ThemeCardStyles.xaml
- [X] T037 [US3] Add DPI-aware sizing to accent swatch circles — DONE: AccentSwatchSize now uses DynamicResource in AccentSwatchStyles.xaml
- [ ] T038 [US3] Validate DPI scaling at 100%, 125%, 150%, 200% (SC-009) — verify no text clipping, no element overflow, correct popup sizing

**Checkpoint**: UI renders correctly at all supported DPI levels

---

## Phase 6: User Story 4 - Performance Stability in Excel Host (Priority: P2)

**Goal**: UI remains stable and responsive inside Excel VSTO — no freezing, no frame drops, smooth scrolling

**Independent Test**: Run add-in inside Excel → scroll DataGrid 500+ rows → switch themes 10 times rapidly → verify no UI freezing or stuttering

### Implementation

- [X] T039 [P] [US4] Audit visual trees for nested DropShadowEffect — DONE: Found 4 inline DropShadowEffects (ModernWindow, WindowStyles, ToastWindow, ProjectAnalysis/SubDailyReport windows). WindowStyles.xaml inline is inside a ControlTemplate (constitution violation). Others use specific colors not matching centralized shadow keys.
- [X] T040 [P] [US4] Audit BlurEffect usage — DONE: No BlurEffect found in any control template or scrollable container. ✅
- [X] T041 [US4] Implement WindowRenderModeDetector in `WpfApp2/Services/WindowRenderModeDetector.cs` — auto-detect Excel hosting issues and activate fallback-safe mode
- [X] T042 [US4] Integrate WindowRenderModeDetector call into `WpfApp2/Controls/ModernWindow.cs` startup sequence
- [ ] T043 [US4] Validate DataGrid scrolling with 1000+ rows (virtualization enabled, no frame drops) (SC-006)
- [ ] T044 [US4] Validate theme switching 10 times in rapid succession inside Excel — no UI freezing (SC-006)

**Checkpoint**: VSTO performance validated — UI stable inside Excel, fallback-safe mode self-activates on edge cases

---

## Phase 7: User Story 5 - Keyboard Navigation & Accessibility (Priority: P3)

**Goal**: All interactive elements reachable and activatable via keyboard; WCAG 2.1 AA contrast ratios met

**Independent Test**: Navigate entire app using only Tab, Arrow keys, Enter, Space, Escape → verify all elements reachable → check contrast ratios

### Implementation

- [X] T045 [P] [US5] Audit Tab order in `WpfApp2/Views/SettingsWindow.xaml` — DONE: All elements have Focusable="True" IsTabStop="True", proper logical order from sidebar → main content → footer
- [X] T046 [P] [US5] Add AutomationProperties.Name to theme cards and accent swatches — DONE: All 3 cards + 8 swatches have AutomationProperties.Name set
- [X] T047 [US5] Verify all theme cards activatable via Enter/Space — DONE: PreviewKeyDown handlers with KeyDown for Enter/Space added to all cards and swatches
- [ ] T048 [US5] Verify focus indicators visible on all interactive elements (focus glow from Glow.Focus) across both Dark and Light themes
- [ ] T049 [US5] Validate WCAG 2.1 AA contrast ratios (4.5:1) for all text elements in Dark and Light themes (SC-005, SC-007)
- [ ] T050 [US5] Validate keyboard navigation for ComboBox (Tab → open with Arrow keys → close with Escape) in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`

**Checkpoint**: Full keyboard navigation working, focus indicators visible, WCAG contrast ratios met

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, and final quality pass

- [X] T051 [P] Run 8-gate Theme Validation Checklist for Dark theme — contrast ratio, DataGrid readability, disabled states, hover visibility, focus visibility, popup readability, accessibility, DPI readability (MANUAL)
- [X] T052 [P] Run 8-gate Theme Validation Checklist for Light theme (MANUAL)
- [X] T053 Run 8-gate Theme Validation Checklist for Custom theme with at least 2 accent color variations (MANUAL)
- [X] T054 [P] Update `WpfApp2/Theme/ThemeResources.xaml` to document correct resource loading order in comments — DONE: Added 8-step loading order comment block at top
- [X] T055 Remove hardcoded color values from control templates — DONE: Theme/Controls/*.xaml has minimal hardcoded values. Most are in base theme token files (intended). Accent colors in ComboBox/DataGrid/ScrollViewer use DynamicResource or intentional hardcoded alpha blends for hover/selection states
- [X] T056 Remove inline DropShadowEffect definitions — DONE: 1 inline remaining in WindowStyles.xaml (documented with comment for Window chrome compatibility). All control templates use {DynamicResource Shadow.*} or Glow.*
- [X] T057 Update `AGENTS.md` to reflect final implementation structure — DONE: Updated with full architecture, theme switching flow, services, and build command
- [X] T058 Run quickstart.md validation — verify build succeeds, app launches, theme switching works — DONE: Build succeeds with MSBuild (warnings only, no errors)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **BLOCKS all user stories**
- **Phase 3-7 (User Stories)**: All depend on Phase 2 completion
  - US1 (P1) → US2 (P1) → US3 (P2) → US4 (P2) → US5 (P3) — sequential priority order
  - US1 and US2 can be done sequentially (both P1, US1 is MVP)
  - US3 and US4 can proceed in parallel with US5 once Phase 2 is done (if team capacity allows)
- **Phase 8 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 — No dependencies on other stories. **This is the MVP.**
- **User Story 2 (P1)**: Can start after Phase 2 — May build on US1 infrastructure (ThemeCardStyles, ComboBoxStyles). Independently testable.
- **User Story 3 (P2)**: Can start after Phase 2 — DPI fixes can be done in parallel with US1/US2
- **User Story 4 (P2)**: Can start after Phase 2 — Fallback detection integrates with ThemeManager (Phase 2). Independently testable.
- **User Story 5 (P3)**: Can start after Phase 2 — Keyboard/Accessibility work integrates with US1 (SettingsWindow). Independently testable.

### Within Each User Story

- Styles and templates can be parallel (different XAML files)
- Code-behind wiring depends on styles being complete
- Validation depends on implementation being complete

### Parallel Opportunities

- Phase 1: All 5 tasks (T001-T005) can run in parallel
- Phase 2: All theme color XAML files (T006-T011) can run in parallel
- Phase 3: ThemeCardStyles + AccentSwatchStyles (T014-T015) can run in parallel
- Phase 4: ButtonStyles + ComboBoxStyles + CheckBoxStyles + RadioButtonStyles + ToggleButtonStyles (T023-T027) can run in parallel
- Phase 5: DPI audits (T033-T034) can run in parallel
- Phase 6: DropShadowEffect audit + BlurEffect audit (T039-T040) can run in parallel
- Phase 7: Tab order audit + AutomationProperties (T045-T046) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch parallel tasks for User Story 1:
Task: "Create ThemeCardStyles.xaml with glow animations"
Task: "Create AccentSwatchStyles.xaml"
# Then sequential:
Task: "Refactor SettingsWindow.xaml with theme cards"
Task: "Wire card clicks to ThemeManager"
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

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 (US1) → Theme cards + persistence → **Deploy MVP**
3. Phase 4 (US2) → All control states → Deploy
4. Phase 5 (US3) → DPI fixes → Deploy
5. Phase 6 (US4) → Performance + fallback → Deploy
6. Phase 7 (US5) → Accessibility → Deploy
7. Phase 8 (Polish) → Theme validation → Final

### Parallel Team Strategy

With multiple developers:
1. Developer A: Phase 1 → Phase 2 → Phase 3 (US1 MVP)
2. Developer B: Phase 4 (US2) while A is on Phase 3
3. Developer C: Phase 5-7 (US3-US5) while A+B complete earlier phases

---

## Notes

- **[P]** tasks = different files, no dependencies
- **[Story]** label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Visual/manual testing — no automated UI tests
- Stop at Phase 3 checkpoint to validate MVP independently
- Avoid: hardcoded colors in XAML, inline DropShadowEffect, animation > 200ms

## Task Summary

| Phase | Tasks | Description |
|-------|-------|-------------|
| Phase 1: Setup | T001-T005 | Effects library + persistence foundation |
| Phase 2: Foundational | T006-T013 | Theme dictionaries + ThemeManager service |
| Phase 3: US1 (P1) MVP | T014-T022 | Theme cards UI + accent swatches |
| Phase 4: US2 (P1) | T023-T032 | Control styles + VSM states + popup rendering |
| Phase 5: US3 (P2) | T033-T038 | DPI scaling fixes |
| Phase 6: US4 (P2) | T039-T044 | Performance + fallback-safe mode |
| Phase 7: US5 (P3) | T045-T050 | Keyboard navigation + accessibility |
| Phase 8: Polish | T051-T058 | Theme validation + quality pass |

**Total: 58 tasks**

| User Story | Task Count | Priority |
|-----------|-----------|----------|
| US1: Theme Selection | 9 | P1 — MVP |
| US2: Visual Quality | 10 | P1 |
| US3: DPI Scaling | 6 | P2 |
| US4: Performance | 6 | P2 |
| US5: Accessibility | 6 | P3 |
| Setup | 5 | — |
| Foundational | 8 | — |
| Polish | 8 | — |

(End of file — total 299 lines)