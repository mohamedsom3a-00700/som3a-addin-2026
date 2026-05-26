# Tasks: Dynamic Settings Platform

**Input**: Design documents from `specs/016-dynamic-settings-platform/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in spec — test tasks omitted by default

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new service files, register in CompositionRoot, initialize infrastructure

- [X] T001 Create SettingsRegistry service class in WpfApp2/Services/SettingsRegistry.cs
- [X] T002 [P] Create SettingsChangedEvent event type in WpfApp2/Services/SettingsChangedEvent.cs
- [X] T003 [P] Create SettingsPersistenceService for per-plugin JSON in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T004 [P] Create SettingsValidator validation rule engine in WpfApp2/Services/SettingsValidator.cs
- [X] T005 Create SettingsMigrationService for incremental legacy migration in WpfApp2/Services/SettingsMigrationService.cs
- [X] T006 [P] Create DynamicSettingsRenderer DataTemplateSelector in WpfApp2/Views/SettingControlSelector.cs
- [X] T007 [P] Create SettingControlTemplates.xaml resource dictionary in WpfApp2/Views/SettingControlTemplates.xaml
- [X] T008 Register all new services (SettingsRegistry singleton, SettingsPersistenceService, SettingsValidator, SettingsMigrationService, EventBus event types) in WpfApp2/CompositionRoot.cs
- [X] T009 Create contracts directory and write ISettingsRegistry.md contract spec at specs/016-dynamic-settings-platform/contracts/ISettingsRegistry.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T010 Implement SettingsRegistry class — implement ISettingsRegistry interface with RegisterSection(), RegisterSetting(), GetSectionsByCategory(), GetAllCategories(), GetSetting(), UpdateSettingValue(), PurgePluginSections() in WpfApp2/Services/SettingsRegistry.cs
- [X] T011 [P] Implement SettingsPersistenceService — per-plugin JSON load/save at AppData/Som3a/Plugins/{ModuleId}/settings.json with version tracking in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T012 [P] Implement SettingsValidator — validation rule engine supporting Required, Range, Regex, MinLength, MaxLength, FilePathExists rule types in WpfApp2/Services/SettingsValidator.cs
- [X] T013 [P] Implement SettingsChangedEvent class with ModuleId, SectionId, SettingKey, OldValue, NewValue, ChangedAt properties in WpfApp2/Services/SettingsChangedEvent.cs
- [X] T014 [P] Create SettingsSectionViewModel wrapping SettingsSection with bindable properties in WpfApp2/ViewModels/SettingsSectionViewModel.cs
- [X] T015 [P] Create SettingControlViewModel wrapping SettingDefinition with CurrentValue, ValidationMessage, IsValid bindable properties in WpfApp2/ViewModels/SettingControlViewModel.cs
- [X] T016 [P] Create SettingControlTemplates.xaml — define DataTemplates keyed by SettingValueType (String→TextBox, Boolean→CheckBox, Enum→ComboBox, Integer/Decimal→NumericUpDown, Color→ColorPicker, FilePath→BrowseButton+TextBox, Secret→PasswordBox) with {DynamicResource} for themeable properties in WpfApp2/Views/SettingControlTemplates.xaml
- [X] T017 Implement SettingControlSelector (DataTemplateSelector) — SelectTemplate() maps SettingValueType enum to matching DataTemplate resource key in WpfApp2/Views/SettingControlSelector.cs
- [X] T018 Implement SettingsPersistenceService.LoadPluginSettingsAsync() — deserialize JSON, apply defaults for missing keys (FR-010), handle corrupt file fallback in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T019 Implement SettingsPersistenceService.SavePluginSettingsAsync() — serialize section values to JSON, write atomically to per-plugin file in WpfApp2/Services/SettingsPersistenceService.cs

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Plugin Developer Registers Settings (Priority: P1) 🎯 MVP

**Goal**: A plugin developer creates a plugin, registers settings sections via [SettingsSection] attribute + ISettingsModule, and they appear in the registry automatically

**Independent Test**: Create a test plugin that registers 1 section with 1 text control; verify section appears in SettingsRegistry.GetSectionsByCategory() with expected properties and the control is rendered in the settings page

### Implementation for User Story 1

- [X] T020 [P] [US1] Implement ISettingsModule integration — PluginHost.Initialize() calls module.RegisterSettings(settingsRegistry) during plugin load in WpfApp2/Services/SettingsRegistry.cs
- [X] T021 [P] [US1] Map [SettingsSection] attribute properties (Category, DisplayName, Order, IconKey) to SettingsSection creation in plugin discovery pipeline referencing Som3a.Plugin.SDK/Attributes/SettingsSectionAttribute.cs
- [X] T022 [US1] Implement duplicate detection — reject section ID duplicates within the same plugin with diagnostic log; merge same-name categories in WpfApp2/Services/SettingsRegistry.cs
- [X] T023 [US1] Add GetSectionsForPlugin(pluginId) method to SettingsRegistry for plugin-level section retrieval in WpfApp2/Services/SettingsRegistry.cs
- [X] T024 [US1] Implement PluginSettingsState tracking — track Registered/Active/Orphaned state per plugin in SettingsRegistry in WpfApp2/Services/SettingsRegistry.cs

**Checkpoint**: Plugin developer can register settings sections and they appear in the registry

---

## Phase 4: User Story 2 - User Manages Plugin Settings (Priority: P1)

**Goal**: A planning engineer opens Settings page, sees all plugin settings organized by category, modifies values, sees validation feedback, and saves

**Independent Test**: Open Settings page, modify a text field, observe validation feedback on invalid input (red message below control), click Save, restart app, verify value restored

### Implementation for User Story 2

- [X] T025 [US2] Modify SettingsViewModel — replace hardcoded panel list with dynamic section loading from SettingsRegistry.GetAllCategories() and GetSectionsByCategory() in WpfApp2/ViewModels/SettingsViewModel.cs
- [X] T026 [P] [US2] Create SettingsSectionView.xaml — user control that renders a section header (IconKey + DisplayName + Description) and iterates SettingControlViewModels using SettingControlSelector in WpfApp2/Views/SettingsSectionView.xaml
- [X] T027 [P] [US2] Create SettingsSectionView.xaml.cs with ViewModel binding in WpfApp2/Views/SettingsSectionView.xaml.cs
- [X] T028 [US2] Modify SettingsPage.xaml — load dynamic sections grouped by category from SettingsViewModel, each category rendered as a group/expander with SettingsSectionView children in WpfApp2/Pages/SettingsPage.xaml
- [X] T029 [US2] Modify SettingsPage.xaml.cs — bind SettingsViewModel, handle Save to persist via SettingsPersistenceService and emit SettingsChangedEvent via EventBus in WpfApp2/Pages/SettingsPage.xaml.cs
- [X] T030 [P] [US2] Wire SettingsChangedEvent publish after successful save in SettingsRegistry.UpdateSettingValue() — include OldValue, NewValue, ModuleId, SectionId, SettingKey in WpfApp2/Services/SettingsRegistry.cs
- [X] T031 [P] [US2] Implement SettingControlViewModel validation — on value change, run validation rules from SettingDefinition.ValidationRules using SettingsValidator, update ValidationMessage/IsValid in WpfApp2/ViewModels/SettingControlViewModel.cs
- [X] T032 [US2] Wire real-time validation feedback — SettingControlViewModel.IsValid changes trigger visual indicator in SettingControlTemplates.xaml (red border/icon for error, yellow for warning) in WpfApp2/Views/SettingControlTemplates.xaml
- [X] T033 [US2] Add IsDirty tracking to SettingsViewModel — track which settings have been modified; display unsaved changes indicator in WpfApp2/ViewModels/SettingsViewModel.cs
- [X] T034 [US2] Build and verify — MSBuild the solution and test settings page renders with at least 2 registered sections in WpfApp2/Som3a_WPF_UI.csproj

**Checkpoint**: User can browse, modify, validate, and save plugin settings through a dynamic UI

---

## Phase 5: User Story 3 - Import/Export Settings (Priority: P2)

**Goal**: User exports all plugin settings to a portable file for backup/transfer and imports them on another installation

**Independent Test**: Export settings to file, modify a setting value, import the saved file, verify original value restored

### Implementation for User Story 3

- [X] T035 [P] [US3] Implement ExportSnapshotAsync — serialize all plugin sections and current values to portable JSON bundle (FR-005) in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T036 [P] [US3] Implement ImportSnapshotAsync — deserialize portable JSON bundle, merge overwriting matching section keys (FR-006) in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T037 [US3] Implement per-plugin export — optional pluginId filter on ExportSnapshotAsync exports only that plugin's settings in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T038 [US3] Add import/export UI controls to SettingsPage — Export All button, Export Per-Plugin dropdown, Import button with file picker dialog in WpfApp2/Pages/SettingsPage.xaml
- [X] T039 [US3] Wire import confirmation dialog — warn user before overwriting current settings, show import summary (plugins/sections restored) in WpfApp2/Pages/SettingsPage.xaml.cs
- [X] T040 [US3] Add import validation — validate JSON structure before merge, reject files with invalid schema, show error details in WpfApp2/Services/SettingsPersistenceService.cs

**Checkpoint**: User can export and import settings per-plugin or as a full snapshot

---

## Phase 6: User Story 4 - API Key Management (Priority: P2)

**Goal**: User configures AI provider API keys through settings UI. Keys encrypted at rest and masked in UI

**Independent Test**: Save an API key, observe masked input in UI, restart app, confirm key is usable, verify plaintext cannot be read from files using standard file viewers

### Implementation for User Story 4

- [X] T041 [P] [US4] Implement LoadEncryptedValueAsync — decrypt and load secret via DPAPI (System.Security.Cryptography.ProtectedData) from AppData/Som3a/Plugins/{ModuleId}/secrets.json in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T042 [P] [US4] Implement SaveEncryptedValueAsync — encrypt and store secret via DPAPI to per-plugin secrets.json in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T043 [US4] Add Secret/PasswordBox DataTemplate to SettingControlTemplates.xaml — masked input (PasswordBoxChar=●) for SettingValueType.Secret, bind to SettingControlViewModel.CurrentValue securely in WpfApp2/Views/SettingControlTemplates.xaml
- [X] T044 [US4] Implement SettingDefinition.IsEncrypted routing — when IsEncrypted=true, route value through encrypted path (secrets.json) instead of main settings.json in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T045 [US4] Wire ApiKeyEncryption from Som3a.Infrastructure — reuse existing ApiKeyEncryption service for DPAPI encryption inside SettingsPersistenceService in WpfApp2/Services/SettingsPersistenceService.cs
- [X] T046 [US4] Ensure OldValue/NewValue in SettingsChangedEvent exclude raw secret values — log metadata only, not plaintext secrets in WpfApp2/Services/SettingsRegistry.cs

**Checkpoint**: User can save, view (masked), and persist API keys securely with encryption

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Migrate existing settings, verify all flows, final validation

- [X] T047 [P] Implement SettingsMigrationService.MigrateCategoryAsync("Theme") — read SelectedTheme, AccentColor, AnimationSpeed, UiDensity, BackgroundStyle from Properties.Settings.Default and write to SettingsRegistry with proper section structure in WpfApp2/Services/SettingsMigrationService.cs
- [X] T048 [P] Implement SettingsMigrationService.MigrateCategoryAsync("Accessibility") — migrate HighContrastEnabled, FocusIndicatorEnabled to registry in WpfApp2/Services/SettingsMigrationService.cs
- [X] T049 [P] Implement SettingsMigrationService.MigrateCategoryAsync("Performance") — migrate RenderMode, SafeModeEnabled to registry in WpfApp2/Services/SettingsMigrationService.cs
- [X] T050 [P] Implement SettingsMigrationService.MigrateCategoryAsync("Diagnostics") — migrate diagnostics settings to registry in WpfApp2/Services/SettingsMigrationService.cs
- [X] T051 [P] Implement SettingsMigrationService.MigrateCategoryAsync("Excel") — migrate Excel settings to registry in WpfApp2/Services/SettingsMigrationService.cs
- [X] T052 Implement automatic background migration on startup — check migration tracker, migrate any unmigrated categories in WpfApp2/CompositionRoot.cs
- [X] T053 [P] Add orphaned settings auto-purge logic — purge plugin sections after configurable grace period (default 30 days) in WpfApp2/Services/SettingsRegistry.cs
- [X] T054 [P] Add FR-014 diagnostics logging — log every settings change (timestamp, plugin ID, setting key, before/after values) using existing diagnostics infrastructure in WpfApp2/Services/SettingsRegistry.cs
- [X] T055 Run MSBuild — verify solution compiles with zero errors
- [X] T056 Manual Excel VSTO host test — navigate Settings page, add/edit/save settings, verify no black window or rendering issues

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P1 → P2 → P2)
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational — Must be complete before US3 test (import/export depends on registered sections being modifiable)
- **User Story 3 (P2)**: Depends on US2 (import/export manipulates same persisted data)
- **User Story 4 (P2)**: Depends on US2 (encrypted settings are a subclass of the dynamic settings UI) but can be done in parallel with US3

### Within Each User Story

- ViewModels before Views
- Services before Pages
- Models before services
- Core implementation before integration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- US1 and US2 can start in parallel after Foundational
- US3 and US4 can start in parallel (after US2 is complete)
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 2

```bash
# Launch all ViewModels for User Story 2 together:
Task: "Modify SettingsViewModel in WpfApp2/ViewModels/SettingsViewModel.cs"
Task: "Create SettingsSectionView.xaml in WpfApp2/Views/SettingsSectionView.xaml"
Task: "Create SettingsSectionView.xaml.cs in WpfApp2/Views/SettingsSectionView.xaml.cs"

# Then after ViewModels complete:
Task: "Modify SettingsPage.xaml in WpfApp2/Pages/SettingsPage.xaml"
Task: "Modify SettingsPage.xaml.cs in WpfApp2/Pages/SettingsPage.xaml.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Plugin Developer Registration)
4. Complete Phase 4: User Story 2 (User Manages Plugin Settings)
5. **STOP and VALIDATE**: Test both stories independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 + User Story 2 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 3 (Import/Export) → Test independently → Deploy/Demo
4. Add User Story 4 (API Key Security) → Test independently → Deploy/Demo
5. Add Polish (Migration) → Final validation → Deploy

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Plugin registration)
   - Developer B: User Story 2 (Dynamic settings UI)
3. After US2 completes:
   - Developer A: User Story 3 (Import/Export)
   - Developer B: User Story 4 (API keys)
4. All developers: Phase 7 (Polish & Migration)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All UI controls use {DynamicResource} for themeable properties (Constitution III)
- SettingControlTemplates.xaml is loaded by SettingsPage only, not added to ThemeResources.xaml (avoids XV ordering concerns)
