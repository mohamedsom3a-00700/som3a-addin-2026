# Tasks: Localization & RTL

**Input**: Design documents from `/specs/024-localization-rtl/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths correspond to the existing Som3a.Localization (.NET 8.0) and WpfApp2 (.NET Framework 4.8) projects.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new directories and file scaffolds

- [X] T001 Create `WpfApp2/Theme/Fonts/` directory for Arabic font resource dictionaries
- [X] T002 [P] Create `WpfApp2/Controls/Shell/` directory for ShellRTLManager
- [X] T003 [P] Create `WpfApp2/Converters/` directory for CultureAwareFormattingConverter (if not exists)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core localization infrastructure that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Extend `LocalizationService` in `Som3a.Localization/Services/LocalizationService.cs` with `SetLanguage(string)`, `GetString(string)`, `GetSupportedLanguages()`, `SaveLanguagePreference()`, `LoadLanguagePreference()` and `LanguageChanged` event
- [X] T005 [P] Create English resource file `Som3a.Localization/Resources/Strings.en-US.resx` with all Shell/Settings/Dashboard/error keys
- [X] T006 [P] Create Arabic resource file `Som3a.Localization/Resources/Strings.ar-SA.resx` with Arabic translations for all keys from T005
- [X] T007 Implement `LanguageInfo` model and `LanguageChangedEventArgs` in `Som3a.Localization/Services/LocalizationService.cs`
- [X] T008 Implement `LanguageBridgeService` in `WpfApp2/Services/LocalizationBridgeService.cs` to bridge .NET 8.0 LocalizationService to WPF .NET Framework 4.8 host
- [X] T009 Configure `Som3a.Localization` project references in `WpfApp2` solution to expose localization services via the bridge

**Checkpoint**: Foundation ready — language resources exist and LocalizationService can load/switch languages

---

## Phase 3: User Story 1 - Switch Application Language (Priority: P1) 🎯 MVP

**Goal**: User can switch between English and Arabic in Settings and see all Shell UI text translate instantly without restart

**Independent Test**: Open app → Settings → Language → select Arabic → all Shell text switches to Arabic

### Implementation for User Story 1

- [X] T010 [P] [US1] Implement `ILocalizationService` interface in `Som3a.Localization/Contracts/ILocalizationService.cs`
- [X] T011 [P] [US1] Implement language persistence in `LocalizationService.SaveLanguagePreference()` using `Properties.Settings.Default`
- [X] T012 [US1] Create `LanguagePage.xaml` in `WpfApp2/Pages/Settings/LanguagePage.xaml` with language picker dropdown
- [X] T013 [US1] Create `LanguagePageViewModel.cs` in `WpfApp2/ViewModels/Settings/LanguagePageViewModel.cs` with `SetLanguageCommand`
- [X] T014 [US1] Wire language picker dropdown to `LocalizationService.SetLanguage()` in `WpfApp2/Pages/Settings/LanguagePage.xaml.cs`
- [X] T015 [US1] Add language preference load at startup in `WpfApp2/App.xaml.cs` (call `LoadLanguagePreference()` after `OnStartup`)
- [X] T016 [US1] Register `LanguagePage` in `NavigationService` sidebar under Settings category
- [X] T017 [US1] Hook `LocalizationService.LanguageChanged` event to refresh all Shell UI text bindings in `WpfApp2/Controls/Shell/ShellWindow.xaml.cs`
- [X] T018 [US1] Implement English fallback in `LocalizationService.GetString()` when Arabic translation missing for a key

**Checkpoint**: English ↔ Arabic language switch works across all Shell text. No restart needed.

---

## Phase 4: User Story 2 - Right-to-Left Layout (Priority: P1)

**Goal**: When Arabic is selected, the Shell workspace mirrors to RTL — sidebar on right, text right-aligned, navigation reversed

**Independent Test**: Switch to Arabic → sidebar moves to right side → switch to English → sidebar returns to left

### Implementation for User Story 2

- [X] T019 [P] [US2] Implement `IRTLManager` interface in `specs/024-localization-rtl/contracts/IRTLManager.cs`
- [X] T020 [US2] Create `ShellRTLManager` in `WpfApp2/Controls/Shell/ShellRTLManager.cs` with `ApplyLayout(bool isRTL)`, `RegisterFlowElement()`, `UnregisterFlowElement()`
- [X] T021 [US2] Wire `LocalizationService.LanguageChanged` to `ShellRTLManager.ApplyLayout()` in `WpfApp2/App.xaml.cs`
- [X] T022 [US2] Apply `FlowDirection="RightToLeft"` to Shell `Grid` root container in `WpfApp2/Controls/Shell/ShellWindow.xaml` when RTL
- [X] T023 [US2] Mirror sidebar to right side in `WpfApp2/Controls/Shell/SidebarControl.xaml` when `ShellRTLManager.IsRTL` is true
- [X] T024 [US2] Register primary Shell containers (sidebar, content area, header) with `ShellRTLManager.RegisterFlowElement()` in `WpfApp2/Controls/Shell/ShellWindow.xaml.cs`
- [X] T025 [US2] Ensure all DataGrid and ListView controls in Shell pages respect FlowDirection in `WpfApp2/Controls/Shell/ShellWindow.xaml.cs`
- [X] T026 [US2] Ensure all ComboBox dropdowns, popups, and flyouts respect FlowDirection in RTL mode

**Checkpoint**: RTL layout mirrors correctly. Sidebar on right, text right-aligned, navigation reversed. LTR restores fully.

---

## Phase 5: User Story 3 - Culture-Aware Formatting (Priority: P2)

**Goal**: Numbers, dates, and currencies format according to the selected language's culture conventions

**Independent Test**: Switch to Arabic → enter quantity 123456.78 → displays with Arabic-Indic digits and separators → switch to English → displays with Western digits

### Implementation for User Story 3

- [X] T027 [P] [US3] Implement `ICultureFormattingService` interface in `specs/024-localization-rtl/contracts/ICultureFormattingService.cs`
- [X] T028 [US3] Create `CultureAwareFormattingConverter` in `WpfApp2/Converters/CultureAwareFormattingConverter.cs` with `FormatNumber()`, `FormatDate()`, `FormatCurrency()` using `CultureInfo.CreateSpecificContext()`
- [X] T029 [US3] Create `CultureAwareFormattingService` in `WpfApp2/Services/CultureAwareFormattingService.cs` wrapping the converter for DI
- [X] T030 [US3] Register `CultureAwareFormattingConverter` as a XAML static resource in `WpfApp2/App.xaml`
- [X] T031 [US3] Apply `CultureNumberConverter`, `CultureDateConverter`, `CultureCurrencyConverter` bindings in Shell pages that display quantities, dates, and costs
- [X] T032 [US3] Wire `LocalizationService.LanguageChanged` to refresh culture formatting in `CultureAwareFormattingService`

**Checkpoint**: Number, date, and currency formatting matches the selected culture's conventions.

---

## Phase 6: User Story 4 - Arabic Typography (Priority: P2)

**Goal**: Arabic text renders in an Arabic-optimized font with proper shaping, ligatures, and size scaling; font can be configured in settings

**Independent Test**: Switch to Arabic → text renders in Cairo font with correct Arabic shaping → switch to English → text renders in Segoe UI

### Implementation for User Story 4

- [X] T033 [P] [US4] Implement `IArabicFontManager` interface in `specs/024-localization-rtl/contracts/IArabicFontManager.cs`
- [X] T034 [P] [US4] Create `ArabicFonts.xaml` in `WpfApp2/Theme/Fonts/ArabicFonts.xaml` defining Arabic font family resources (Cairo, IBM Plex Sans Arabic)
- [X] T035 [P] [US4] Create `FontFallback.xaml` in `WpfApp2/Theme/Fonts/FontFallback.xaml` defining Arabic→Latin fallback chain resources
- [X] T036 [P] [US4] Register `ArabicFonts.xaml` and `FontFallback.xaml` in `WpfApp2/Theme/ThemeResources.xaml` aggregator
- [X] T037 [US4] Implement `ArabicFontManager` in `WpfApp2/Services/ArabicFontManager.cs` with `SetArabicFont()`, `GetFontFallbackChain()`
- [X] T038 [US4] Wire `LocalizationService.LanguageChanged` to switch font family based on language in `WpfApp2/App.xaml.cs`
- [X] T039 [US4] Add Arabic font selection UI (dropdown with Cairo, IBM Plex Sans Arabic) to `LanguagePage.xaml`
- [X] T040 [US4] Add Arabic font preview thumbnails in `LanguagePage.xaml` showing sample Arabic text

**Checkpoint**: Arabic text renders with correct font, shaping, and ligatures. Font can be changed in Settings.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T041 [P] Run Excel VSTO host validation — verify language switch and RTL mode do not cause Excel instability or black window rendering
- [ ] T042 [P] Handle edge case: language switch while a modal dialog is open — queue switch or apply after dialog closes
- [ ] T043 [P] Handle edge case: configured Arabic font not installed on system — fall back to bundled font or Segoe UI
- [ ] T044 [P] Handle edge case: plugin with no Arabic translations displays English fallback text (not empty/broken UI)
- [ ] T045 [P] Add missing translation key logging to diagnostics system in `LocalizationService.GetString()`
- [ ] T046 [P] Run quickstart.md validation — verify all test scenarios pass
- [ ] T047 [P] Constitution compliance review — verify DynamicResource-only usage, Excel rendering safety, WindowChrome inheritance, and no theme mutation through LocalizationService

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3–6)**: All depend on Foundational phase completion
  - US1 (Phase 3) must complete before US2 (Phase 4)
  - US3 (Phase 5) and US4 (Phase 6) have no dependency on US2 — can proceed after US1
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Language switching — foundational, no dependencies on other stories
- **US2 (P1)**: RTL layout — depends on US1 completing (language switch triggers RTL)
- **US3 (P2)**: Culture formatting — depends on US1 (formatting tied to language), independent of US2
- **US4 (P2)**: Arabic typography — depends on US1 (font tied to language), independent of US2, US3

### Within Each User Story

- Interface contracts before implementation
- Core service logic before UI integration
- Story complete before moving to next

### Parallel Opportunities

- T002, T003 can run in parallel (Setup phase)
- T005, T006 can run in parallel (different .resx files)
- T010, T011 can run in parallel (US1 — different files)
- T019 can run in parallel with some US1 tasks
- US3 and US4 can proceed in parallel after US1 completes
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```powershell
# Launch ILocalizationService interface + language persistence together:
Task: "Implement ILocalizationService interface in Som3a.Localization/Contracts/ILocalizationService.cs"
Task: "Implement language persistence in LocalizationService.SaveLanguagePreference() using Properties.Settings.Default"
```

## Parallel Example: User Story 4

```powershell
# Launch font resource dictionaries + font manager together:
Task: "Create ArabicFonts.xaml in WpfApp2/Theme/Fonts/ArabicFonts.xaml"
Task: "Create FontFallback.xaml in WpfApp2/Theme/Fonts/FontFallback.xaml"
Task: "Implement ArabicFontManager in WpfApp2/Services/ArabicFontManager.cs"
```

---

## Implementation Strategy

### MVP First (Phase 3 — User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (language switching)
4. **STOP and VALIDATE**: Test language switch independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Language switching) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (RTL layout) → Test independently → Deploy/Demo
4. Add US3 (Culture formatting) → Test independently → Deploy/Demo
5. Add US4 (Arabic typography) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 → US2
   - Developer B: US3
   - Developer C: US4
3. Stories complete and integrate independently
