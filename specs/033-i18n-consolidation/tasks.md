# Tasks: i18n Consolidation & Language Support

**Input**: Design documents from `/specs/033-i18n-consolidation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF App**: `WpfApp2/` — main application
- **Library**: `Som3a.Localization/` — localization library
- **Tests**: `Tests/Som3a_WPF_UI.Tests/` — existing test project

---

## Phase 1: Setup (Documentation)

**Purpose**: Initialize spec directory structure for implementation tracking

- [ ] T001 Create implementation directory structure under `specs/033-i18n-consolidation/` with `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, and `tasks.md` in place

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Translation pre-work and service wiring that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 [P] Merge all English strings from `WpfApp2/Resources/Strings.resx` into `Som3a.Localization/Resources/Strings.en-US.resx` (consolidate ~400+ existing keys, resolve brand differences "Planova" → "Som3a")
- [X] T003 [P] Merge existing Arabic strings from `Som3a.Localization/Resources/Strings.ar-SA.resx` and generate ~740 missing Arabic translations into `Som3a.Localization/Resources/Strings.ar-SA.resx` (complete all 1800+ keys)
- [X] T004 Register `LocalizationService` (from `Som3a.Localization.Services`) in `WpfApp2/CompositionRoot.cs` and set up dependency injection via `IServiceContainer`
- [X] T005 Create `LocExtension` markup extension in `WpfApp2/MarkupExtensions/LocExtension.cs` that wraps `ILocalizationService.GetString()` and auto-refreshes on `LanguageChanged` event
- [X] T006 [P] Migrate language preference persistence from `%APPDATA%/Som3a/language-preference.txt` to `Properties.Settings.Default.SelectedLanguage` in `WpfApp2/Properties/Settings.settings`
- [X] T007 [P] Update `ILocalizationService` interface in `Som3a.Localization/Contracts/ILocalizationService.cs` to add `FontScalingFactor` property and `GetFontFamily(string locale)` methods if not already present

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Switch Language and See Translated UI (Priority: P1) 🎯 MVP

**Goal**: Users can switch between English and Arabic and see all UI text translated immediately without restart

**Independent Test**: Switch language from the existing shell toggle and verify all UI text changes on all pages

### Implementation for User Story 1

- [ ] T008 [P] [US1] Update `WpfApp2/App.xaml.cs` to initialize `LocalizationService` at startup, load saved language preference, and wire the `LanguageChanged` event
- [ ] T009 [US1] Migrate all `TranslationSource` bindings in `WpfApp2/Views/` and subdirectories to use `LocExtension` or ViewModel-injected `ILocalizationService` (~495 XAML binding sites)
- [ ] T010 [US1] Update all ViewModels in `WpfApp2/ViewModels/` to accept `ILocalizationService` via constructor injection and expose string properties that refresh on `LanguageChanged`
- [ ] T011 [US1] Remove `WpfApp2/Services/LocalizationBridgeService.cs` after confirming no remaining references
- [ ] T012 [US1] Remove `WpfApp2/Services/TranslationSource.cs` after confirming no remaining XAML bindings reference it
- [ ] T013 [US1] Add missing-key diagnostic logging to `Som3a.Localization/Services/LocalizationService.cs` in the `GetString` fallback path (per FR-009 clarification)

**Checkpoint**: At this point, US1 should be fully functional — language switching works and all UI text updates

---

## Phase 4: User Story 2 — Correct RTL Layout in Arabic Mode (Priority: P1)

**Goal**: When user switches to Arabic, the entire layout mirrors for RTL — text alignment, scrollbar placement, DataGrid columns, navigation sidebar, keyboard Tab order

**Independent Test**: Switch to Arabic and verify layout reversal on each page, DataGrid column ordering, scrollbar placement, and keyboard Tab flow

### Implementation for User Story 2

- [ ] T014 [US2] Add declarative `FlowDirection` binding to all Page and Window root elements in `WpfApp2/Pages/` and `WpfApp2/Controls/` that reads from `ILocalizationService.IsRTL`
- [ ] T015 [P] [US2] Fix `DataGrid` column order and header alignment in RTL mode across all DataGrid instances in `WpfApp2/Pages/`
- [ ] T016 [P] [US2] Fix `ScrollBar` placement in RTL mode (left side for Arabic) across all scrollable containers in `WpfApp2/Pages/` and `WpfApp2/Controls/`
- [ ] T017 [US2] Update `WpfApp2/Controls/Shell/ShellRTLManager.cs` to also apply keyboard Tab order mirroring and focus direction per RTL state (per clarification decision)
- [ ] T018 [US2] Test and fix mixed English-Arabic bidirectional text alignment in DataGrid cells and text blocks
- [ ] T019 [US2] Remove imperative `FlowDirection` assignments from `WpfApp2/Controls/Shell/WorkspaceHost.cs` — replace with declarative XAML binding from US1/T014

**Checkpoint**: At this point, US2 should be functional — RTL layout works correctly across all pages

---

## Phase 5: User Story 3 — Automatic Font Switching per Locale (Priority: P2)

**Goal**: Font families switch automatically per locale when language changes; users can customize fonts per locale

**Independent Test**: Switch languages and verify font family changes on text elements; open font settings and change the Arabic font

### Implementation for User Story 3

- [ ] T020 [P] [US3] Create `FontService` in `WpfApp2/Services/FontService.cs` that manages per-locale font mappings and wraps font resource updates (decouple from `ThemeManager`)
- [ ] T021 [US3] Register `FontService` in `WpfApp2/CompositionRoot.cs` and wire it to subscribe to `LanguageChanged` event for automatic font switching
- [ ] T022 [US3] Update `WpfApp2/App.xaml.cs` language change handler to delegate font switching to `FontService` instead of `ArabicFontManager`
- [ ] T023 [US3] Decouple font resources from `WpfApp2/Services/ThemeManager.cs` — remove `CustomFontFamily` and `FontFamily.Active` overrides from theme font logic
- [ ] T024 [US3] Remove `WpfApp2/Services/ArabicFontManager.cs` after confirming all font logic is handled by `FontService`

**Checkpoint**: At this point, US3 should be functional — fonts switch automatically per locale

---

## Phase 6: User Story 4 — Manage Language & Font Settings (Priority: P2)

**Goal**: Users access a consolidated Language & Font settings page with language selector, font picker per locale, RTL preview toggle, and font size scaling

**Independent Test**: Open Settings → Language & Font and verify all controls function: language switch, font change, scaling adjustment, RTL preview

### Implementation for User Story 4

- [ ] T025 [P] [US4] Create `WpfApp2/Pages/Settings/LanguagePage.xaml` with language selector (EN/AR radio or dropdown), font picker per locale (dropdown with font family list), font size scaling slider (0.8x–1.5x), and RTL preview toggle card
- [ ] T026 [US4] Create `WpfApp2/ViewModels/Settings/LanguagePageViewModel.cs` with properties for selected language, selected fonts per locale, font scaling factor, and RTL preview state; inject `ILocalizationService` and `FontService`
- [ ] T027 [US4] Register `LanguagePage` in the Settings navigation sidebar in `WpfApp2/Pages/Settings/SettingsPage.xaml` or its navigation configuration
- [ ] T028 [US4] Implement RTL preview toggle behavior: when activated, a sample card renders in RTL to demonstrate layout effect without committing the language switch
- [ ] T029 [US4] Wire font size scaling from the slider to `FontService` and verify all text elements respect the scaling factor

**Checkpoint**: At this point, US4 should be functional — full settings page with language, fonts, scaling

---

## Phase 7: User Story 5 — Quick Language Toggle from Shell (Priority: P3)

**Goal**: Shell window has a language toggle button with a Fluent 2 icon that switches language instantly

**Independent Test**: Click language toggle in shell window and verify language switches instantly; icon indicates current locale

### Implementation for User Story 5

- [ ] T030 [P] [US5] Replace 🌐 emoji `TextBlock` in `WpfApp2/Controls/Shell/ShellWindow.xaml` with a Fluent 2 icon for the language toggle button
- [ ] T031 [US5] Wire the language toggle `Click` handler in `WpfApp2/Controls/Shell/ShellWindow.xaml.cs` to call `ILocalizationService.SetLanguage()` instead of `LocalizationBridgeService`
- [ ] T032 [US5] Update shell toggle icon to reflect current language state (show English flag/text when in Arabic, Arabic when in English, or use a generic globe icon with tooltip indicating current language)
- [ ] T033 [US5] Update toggle `ToolTip` binding to use `LocExtension` instead of `TranslationSource`

**Checkpoint**: At this point, all user stories should be independently functional

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements, verification, and cleanup that affect multiple user stories

- [ ] T034 [P] Run VSTO smoke test: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → switch theme (Dark/Light) → switch language (EN→AR→EN) → Excel cell write via interop → no crashes
- [ ] T035 [P] Verify all 1800+ string keys have complete translations in both `Strings.en-US.resx` and `Strings.ar-SA.resx` — run diagnostic log scan for missing key warnings
- [ ] T036 [P] Verify language switch performance meets <500ms target across all pages
- [ ] T037 [P] Verify language preference persists across application restart (EN→restart→EN, AR→restart→AR)
- [ ] T038 [P] Run full test suite: `dotnet test Tests/Som3a_WPF_UI.Tests.csproj`
- [ ] T039 [P] Update `Docs/Architecture/LOCALIZATION_READINESS.md` with new localization architecture details
- [ ] T040 Constitution compliance review — verify DynamicResource-only usage, no inline effects, WindowChrome inheritance, and Excel rendering safety for all new/modified pages

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user stories**
- **US1 — Switch Language (Phase 3)**: Depends on Foundational
- **US2 — RTL Layout (Phase 4)**: Depends on US1 (needs working language switch to test RTL)
- **US3 — Font Switching (Phase 5)**: Depends on US1 (needs working language switch to trigger font change)
- **US4 — Language Settings Page (Phase 6)**: Depends on US3 (font picker), US1 (language switch), US2 (RTL preview)
- **US5 — Shell Toggle (Phase 7)**: Depends on US1 (needs `ILocalizationService` wired)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational — no dependencies on other stories
- **US2 (P1)**: Starts after US1 — independently testable once US1 complete
- **US3 (P2)**: Starts after US1 — independently testable once US1 complete
- **US4 (P2)**: Starts after US1, US2, US3 — integrates all prior stories into settings UI
- **US5 (P3)**: Starts after US1 — independently testable once US1 complete

### Parallel Opportunities

- T002 and T003 (Foundational) can run in parallel
- T008 can run in parallel with T006, T007 (different files)
- T015, T016 (US2 RTL fixes) can run in parallel
- T020 (US3 FontService) can run in parallel with T014 (US2 RTL)
- T025 (US4 XAML) can run in parallel with T026 (US4 ViewModel)
- T030 (US5 icon) can run in parallel with T031, T032, T033
- All Polish tasks (T034–T040) can run in parallel

---

## Parallel Example: User Story 1

```powershell
# Launch model + screen reader support together:
Task: "Update App.xaml.cs to initialize LocalizationService in WpfApp2/App.xaml.cs"
Task: "Update LocExtension to auto-refresh in WpfApp2/MarkupExtensions/LocExtension.cs"
```

## Parallel Example: User Story 2

```powershell
# Launch DataGrid fixes + ScrollBar fixes together:
Task: "Fix DataGrid column order in RTL across WpfApp2/Pages/"
Task: "Fix ScrollBar placement in RTL across WpfApp2/Pages/ and WpfApp2/Controls/"
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 — language switching works via existing shell toggle
4. **STOP and VALIDATE**: Test US1 independently — switch language, verify all text changes
5. Verify build: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug`
6. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + 2 → Foundation: translation pre-work, service wiring
2. Add US1 → Language switching works (MVP!) → Deploy/Demo
3. Add US2 + US3 (parallel) → RTL layout + font switching → Deploy/Demo
4. Add US4 → Consolidated settings page → Deploy/Demo
5. Add US5 → Polished shell toggle → Deploy/Demo

### Parallel Team Strategy

With multiple developers:
1. Complete Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (language switch + binding migration)
   - Developer B: US2 (RTL layout)
3. After US1 done:
   - Developer A: US3 (font switching)
   - Developer B: US4 (language page)
4. Developer A: US5 (shell toggle)
5. Team: Polish pass together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- T001–T007 are blocking prerequisites for ALL stories
- US1 is the MVP — once complete, core value is delivered
- Arabic encoding corruption in existing `StringsArabic.resx` means all ~1062 existing values must be re-generated in T003
- ~495 XAML binding sites need migration in T009 — use regex search to find all `{Binding Source={x:Static services:TranslationSource.Instance}, Path=[...]}` patterns
- Verify build after each phase before proceeding
- Run VSTO smoke test (T034) as final validation before merge
