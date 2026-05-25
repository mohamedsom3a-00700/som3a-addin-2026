# Tasks: Settings & Personalization UX

**Input**: Design documents from `/specs/001-settings-personalization-ux/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks included only where explicitly stated (import/export file format validation).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Single project: `WpfApp2/` at repository root
- New code under `WpfApp2/Models/`, `WpfApp2/Services/`, `WpfApp2/ViewModels/`, `WpfApp2/Theme/Controls/`
- Existing `WpfApp2/Views/SettingsWindow.xaml` refactored

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization — no setup needed, project already exists.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data models, services, and styles that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T001 [P] Create UserSettings data model in WpfApp2/Models/UserSettings.cs with all fields (SelectedTheme, AccentColor, AnimationSpeed, UiDensity, BackgroundStyle, HighContrastEnabled, FocusIndicatorEnabled, RenderMode, SafeModeEnabled)
- [X] T002 [P] Create SettingsExport data model in WpfApp2/Models/SettingsExport.cs with Version, ExportedAt, Settings, AppVersion fields + JSON serialization attributes
- [X] T003 [P] Create SettingsCategory model in WpfApp2/Models/SettingsCategory.cs with Id, DisplayName, Icon, PanelType, Order properties
- [X] T004 Create SettingsPersistenceService in WpfApp2/Services/SettingsPersistenceService.cs with LoadSettings(), SaveSettings(), ExportSettings(), ImportSettings() methods
- [X] T005 Create SettingsViewModel base structure in WpfApp2/ViewModels/SettingsViewModel.cs with ObservableCollection\<SettingsCategory\> Categories, SelectedCategory, IsDirty, IsPreviewActive properties
- [X] T006 [P] Create SettingsPanelStyles.xaml in WpfApp2/Theme/Controls/SettingsPanelStyles.xaml with sidebar styles, panel container styles, and shared control templates
- [X] T007 Register SettingsPersistenceService and SettingsViewModel in App.xaml.cs / CompositionRoot.cs service container

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Categorized Settings Navigation (Priority: P1) 🎯 MVP

**Goal**: Users can open Settings, see a sidebar with 6 categories, click any category to see its panel.

**Independent Test**: Open Settings → verify sidebar shows 6 categories → click each → verify correct panel loads → verify keyboard Tab + Up/Down navigation works.

### Implementation for User Story 1

- [X] T008 [P] [US1] Create AppearancePanel UserControl in WpfApp2/Views/AppearancePanel.xaml with theme cards, accent swatches, live preview
- [X] T009 [P] [US1] Create PerformancePanel UserControl in WpfApp2/Views/PerformancePanel.xaml with animation speed and density controls
- [X] T010 [P] [US1] Create AccessibilityPanel UserControl in WpfApp2/Views/AccessibilityPanel.xaml with high contrast and focus indicator toggles
- [X] T011 [P] [US1] Create DiagnosticsPanel UserControl in WpfApp2/Views/DiagnosticsPanel.xaml with export/import buttons and system info
- [X] T012 [P] [US1] Create ExcelPanel UserControl in WpfApp2/Views/ExcelPanel.xaml with render mode, safe mode, and DPI info
- [X] T013 [P] [US1] Create PluginsPanel UserControl in WpfApp2/Views/PluginsPanel.xaml with "No plugins installed" stub content
- [X] T014 [US1] Refactor SettingsWindow.xaml in WpfApp2/Views/SettingsWindow.xaml — add sidebar ListBox with 6 categories + ContentControl for panel switching
- [X] T015 [US1] Wire category selection in SettingsViewModel — SelectedCategory changes load corresponding UserControl via CurrentPanel property
- [X] T016 [US1] Implement keyboard navigation for sidebar: Tab to focus sidebar, Up/Down arrows to switch categories (FR-018)
- [X] T017 [US1] Implement sidebar collapse behavior: when window is too narrow, show icons with tooltips instead of labels (US1-AS4)

**Checkpoint**: At this point, the Settings window has a fully functional categorized sidebar. Users can navigate between all 6 panels, each showing its placeholder content. Keyboard navigation works.

---

## Phase 4: User Story 2 - Theme & Accent Live Preview (Priority: P1)

**Goal**: Users can preview theme and accent changes in a live preview area, then Apply or Cancel.

**Independent Test**: Open Settings → Appearance → click different theme card → verify preview updates → click accent swatch → verify accent changes in preview → click Apply → verify main app theme changes → navigate away → verify preview discarded.

### Implementation for User Story 2

- [X] T018 [US2] Implement live theme preview mechanism in SettingsViewModel — store PreviewSettings separate from CurrentSettings; apply preview resources via ThemeManager
- [X] T019 [US2] Add theme card selection to AppearancePanel.xaml — 3 cards (Dark/Light/Custom) with selection glow; wire to ViewModel
- [X] T020 [US2] Add accent swatch picker to AppearancePanel.xaml — 8 predefined swatches; wire to ViewModel
- [X] T021 [US2] Add background style selector to AppearancePanel.xaml — Solid/Gradient radio buttons; wire to ViewModel
- [X] T022 [US2] Implement live preview rendering in AppearancePanel.xaml — dedicated preview area (Border/Grid) with sample themed content
- [X] T023 [US2] Implement ApplyThemeCommand in SettingsViewModel — commits PreviewSettings to CurrentSettings, calls ThemeManager, calls SettingsPersistenceService.SaveSettings()
- [X] T024 [US2] Implement CancelPreviewCommand in SettingsViewModel — discards PreviewSettings, reverts to original theme
- [X] T025 [US2] Implement auto-cancel on category switch — when SelectedCategory changes while IsPreviewActive, calls CancelPreviewCommand automatically

**Checkpoint**: At this point, the Appearance panel has a fully functional live preview. Users can try themes and accents risk-free, then Apply to commit or Cancel/switch category to discard.

---

## Phase 5: User Story 3 - Animation & Density Controls (Priority: P2)

**Goal**: Users can control animation speed (Off/Reduced/Full) and UI density (Compact/Normal/Spacious) from the Performance panel.

**Independent Test**: Open Settings → Performance → set animation to Off → verify no animations play → set density to Compact → verify spacing reduces → restart app → verify settings persist.

### Implementation for User Story 3

- [ ] T026 [P] [US3] Implement AnimationScale attached property infrastructure in WpfApp2/Theme/Controls/AnimationProperties.cs
- [ ] T027 [P] [US3] Create density override dictionary — transient ResourceDictionary with Spacing.* token overrides
- [X] T028 [US3] Build PerformancePanel.xaml UI — animation speed radio buttons (Off/Reduced/Full) + density radio buttons (Compact/Normal/Spacious)
- [X] T029 [US3] Wire animation speed setting in SettingsViewModel — AnimationSpeedCommand; persist via SettingsPersistenceService
- [X] T030 [US3] Wire density setting in SettingsViewModel — DensityCommand; persist via SettingsPersistenceService
- [X] T031 [US3] Load animation speed and density from saved settings on startup via SettingsPersistenceService.LoadSettings()

**Checkpoint**: At this point, users can control animation intensity and UI density. Both settings persist across restarts.

---

## Phase 6: User Story 4 - Settings Import & Export (Priority: P3)

**Goal**: Users can export settings to a JSON file and import them back, with validation.

**Independent Test**: Open Settings → Diagnostics → Export → save file → change several settings → Import the file → verify all settings restored to exported values → try importing a corrupt file → verify error displayed.

### Tests for User Story 4

- [ ] T032 [P] [US4] Unit test for SettingsExport JSON serialization/deserialization roundtrip
- [ ] T033 [P] [US4] Unit test for import with corrupt/invalid JSON → SettingsImportException with no settings modified
- [ ] T034 [P] [US4] Unit test for import with unknown keys → keys silently ignored, warnings returned
- [ ] T035 [P] [US4] Unit test for import with missing required fields → SettingsImportException

### Implementation for User Story 4

- [X] T036 [US4] Implement ExportSettings in SettingsPersistenceService — serialize to JSON using Newtonsoft.Json
- [X] T037 [US4] Implement ImportSettings in SettingsPersistenceService — deserialize JSON, validate schema, apply validation rules
- [X] T038 [US4] Build DiagnosticsPanel.xaml UI — Export Settings button, Import Settings button, system info
- [X] T039 [US4] Wire ExportSettingsCommand in SettingsViewModel — show SaveFileDialog → call ExportSettings
- [X] T040 [US4] Wire ImportSettingsCommand in SettingsViewModel — show OpenFileDialog → call ImportSettings → apply imported settings
- [X] T041 [US4] Integrate import/export with save failure toast notification per FR-017

**Checkpoint**: At this point, all 4 user stories are complete. Users can navigate, personalize, control performance, and backup/restore settings.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [X] T042 [P] Update SettingsPanelStyles.xaml with consistent visual polish across all panels (margins, spacing, colors via DynamicResource)
- [X] T043 [P] Add AutomationProperties.Name to all sidebar categories and panel controls for screen reader support
- [X] T044 [P] Add edge case handling — closing SettingsWindow while preview is active reverts preview (per spec edge case #3)
- [X] T045 [P] Add edge case handling — accent picker gracefully shows only available swatches (per spec edge case #5)
- [X] T046 Run quickstart.md validation checklist
- [X] T047 Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration, Excel rendering safety, and WindowChrome inheritance

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: Can start immediately — blocks ALL user stories
- **US1 - Navigation (Phase 3)**: Depends on Foundational complete — can proceed independently of US2/US3/US4
- **US2 - Live Preview (Phase 4)**: Depends on Foundational complete + US1 navigation framework (reuses AppearancePanel structure)
- **US3 - Animation/Density (Phase 5)**: Depends on Foundational complete — can proceed in parallel with US2 once Foundational done
- **US4 - Import/Export (Phase 6)**: Depends on Foundational complete (SettingsPersistenceService) — can proceed in parallel with US2/US3 once Foundational done
- **Polish (Phase 7)**: Depends on all desired stories complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories — full MVP
- **User Story 2 (P1)**: Requires US1 sidebar+panel infrastructure (AppearancePanel exists)
- **User Story 3 (P2)**: No dependencies on US1/US2 — independently implementable
- **User Story 4 (P3)**: No dependencies on US1/US2/US3 — independently implementable

### Within Each User Story

- Models before services
- Services before UI
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Phase 2 tasks marked [P] can run in parallel (T001-T003)
- US3 (Phase 5) and US4 (Phase 6) can run in parallel with each other
- US4 test tasks T032-T035 can run in parallel
- Within US4, implementation tasks T036-T037 can run in parallel since they affect different methods

---

## Parallel Example: User Story 4

```powershell
# Launch all test tasks for US4 together:
Task: "Unit test for SettingsExport JSON serialization/deserialization roundtrip" (T032)
Task: "Unit test for import with corrupt/invalid JSON" (T033)
Task: "Unit test for import with unknown keys" (T034)
Task: "Unit test for import with missing required fields" (T035)

# Launch all models/services for US4 together:
Task: "Implement ExportSettings in SettingsPersistenceService" (T036)
Task: "Implement ImportSettings in SettingsPersistenceService" (T037)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational
2. Complete Phase 3: User Story 1 (Navigation + 6 panels with placeholders)
3. **STOP and VALIDATE**: Test User Story 1 independently
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Foundational → Foundation ready
2. Add US1 (Navigation) → Test independently → Demo (MVP!)
3. Add US2 (Live Preview) → Test independently → Demo
4. Add US3 (Animation/Density) → Test independently → Demo
5. Add US4 (Import/Export) → Test independently → Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Foundational together
2. Once Foundational is done:
   - Developer A: US3 (Animation/Density) — fully independent
   - Developer B: US4 (Import/Export) — fully independent
   - Developer C: US1 + US2 (Navigation + Preview) — sequential dependency
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- All new XAML files MUST use DynamicResource for themeable properties (Constitution §III)
- Theme mutations MUST go through ThemeManager exclusively (Constitution §IV)
- Animations MUST complete within 200ms (Constitution §IX)
- SettingsWindow MUST inherit from ModernWindow (Constitution §XI)
- No inline DropShadowEffect — use centralized Effects/Shadows.xaml (Constitution §XII)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
