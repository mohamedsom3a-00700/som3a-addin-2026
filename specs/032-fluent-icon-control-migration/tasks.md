# Tasks: Fluent Icon & Control Migration

**Input**: Design documents from `/specs/032-fluent-icon-control-migration/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: No test tasks requested. Verification via build success and manual visual inspection.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**Critical Note**: Research revealed MaterialDesign is already fully removed (zero references). The `FluentIcons.WPF` package is installed but unused. WPF-UI pilot (US4) is deferred to Phase 8. This plan focuses on activating FluentIcons.WPF.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify prerequisites and prepare the project for FluentIcons integration

- [X] T001 Verify `FluentIcons.WPF` v1.1.293 is referenced in `WpfApp2/Som3a_WPF_UI.csproj` and package restore succeeds
- [X] T002 Verify `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds before any changes (baseline)
- [X] T003 [P] Audit all 22 Pages and 12 Views for current icon rendering patterns (TextBlock, Segoe MDL2, hardcoded bullets) and document findings in `specs/032-fluent-icon-control-migration/icon-audit.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create `WpfApp2/Converters/FluentIconConverter.cs` implementing `IValueConverter` — parses string icon name → `FluentIcons.Common.Symbol` enum → returns `FluentIcon` control; fallback to `Symbol.ErrorCircleHalfFilled` for unknown names, `Symbol.Error` for null; diagnostic logging via `System.Diagnostics.Trace`
- [X] T005 Register `FluentIconConverter` as a resource in `WpfApp2/App.xaml` (or `WpfApp2/Theme/ThemeResources.xaml`) so all XAML files can reference it as `{StaticResource FluentIconConverter}`
- [X] T006 [P] Create `specs/032-fluent-icon-control-migration/icon-mapping.md` — map all 24 icon names from `WpfApp2/Services/SidebarRegistrationService.cs` to `FluentIcons.Common.Symbol` enum values; document any gaps or fallbacks
- [X] T007 [P] Audit all `WidgetViewModel` subclasses for Unicode codepoint icon assignments (e.g., `"\U000F05D2"`) and map each to the corresponding `FluentIcons.Common.Symbol` enum name

**Checkpoint**: Foundation ready — FluentIconConverter works, icon mapping complete, user story implementation can begin

---

## Phase 3: User Story 1 - Consistent Fluent Icons Across All Pages (Priority: P1) 🎯 MVP

**Goal**: Every icon in the application uses the Fluent 2 icon set for consistent appearance in both Dark and Light themes

**Independent Test**: Navigate to Sidebar, Shell titlebar, Settings page, and Diagnostics page. Verify all icons render as Fluent 2 icons in both themes. No missing or broken icons.

### Implementation for User Story 1

- [X] T008 [P] [US1] Add FluentIcons namespace to `WpfApp2/Theme/ThemeResources.xaml` — `xmlns:fluentIcons="clr-namespace:FluentIcons.WPF;assembly=FluentIcons.WPF"` at ResourceDictionary root level
- [X] T009 [P] [US1] Replace Unicode codepoint TextBlock icons with `fluentIcons:FluentIcon` in `WpfApp2/Controls/WidgetCardStyles.xaml` — use `Symbol="{Binding Icon, Converter={StaticResource FluentIconConverter}}"` with `IconSize="Medium"` and `Foreground="{DynamicResource Brush.TextPrimary}"`
- [X] T010 [US1] Update all `WidgetViewModel` icon assignments from Unicode codepoint strings (e.g., `"\U000F05D2"`) to FluentIcons Symbol enum names (e.g., `"Dashboard"`) across all files in `WpfApp2/ViewModels/Dashboard/`
- [X] T011 [P] [US1] Replace TextBlock icon renderings with `fluentIcons:FluentIcon` in `WpfApp2/Views/DiagnosticsPanel.xaml` — add FluentIcons namespace, use converter binding
- [X] T012 [US1] Build and verify: `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds; visually confirm widget icons and diagnostics icons render as Fluent 2 in both Dark and Light themes

**Checkpoint**: Widget cards and Diagnostics page show Fluent 2 icons in both themes

---

## Phase 4: User Story 2 - Fluent UI Control Rendering (Priority: P1)

**Goal**: All UI controls (scrollbars, progress bars, buttons, chips) use Fluent-styled controls with consistent Fluent design language

**Independent Test**: Scroll a page with a ScrollViewer, observe progress bars in widgets, click FlatButton-style buttons. All controls render with Fluent styling.

### Implementation for User Story 2

- [X] T013 [P] [US2] Audit `WpfApp2/Views/SettingsPanelStyles.xaml` for `Segoe MDL2 Assets` font usage — identify all TextBlock elements using this font for icon rendering
- [X] T014 [US2] Replace `FontFamily="Segoe MDL2 Assets"` TextBlock icons with `fluentIcons:FluentIcon` in `WpfApp2/Views/SettingsPanelStyles.xaml` — add FluentIcons namespace, use converter binding, set `IconSize="Small"`
- [X] T015 [P] [US2] Verify all custom control templates in `WpfApp2/Theme/Controls/` (ButtonStyles, ScrollBarStyles, ProgressBarStyles, etc.) use DynamicResource for themeable properties — no changes expected, verification only
- [X] T016 [US2] Build and verify: `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds; visually confirm settings panel sidebar shows Fluent 2 icons instead of Segoe MDL2 Assets glyphs

**Checkpoint**: Settings panel sidebar shows Fluent 2 icons; all custom control templates verified for DynamicResource compliance

---

## Phase 5: User Story 3 - Sidebar Icon System Migration (Priority: P2)

**Goal**: Sidebar dynamically loads Fluent 2 icons based on registered page metadata so any new page automatically gets the correct icon

**Independent Test**: Sidebar renders Fluent 2 icons for all registered pages. Register a test page with a new icon name — verify it resolves correctly.

### Implementation for User Story 3

- [X] T017 [US3] Add FluentIcons namespace to `WpfApp2/Controls/Shell/SidebarControl.xaml` — `xmlns:fluentIcons="clr-namespace:FluentIcons.WPF;assembly=FluentIcons.WPF"`
- [X] T018 [US3] Replace `<TextBlock Text="●" .../>` with `<fluentIcons:FluentIcon Symbol="{Binding Icon, Converter={StaticResource FluentIconConverter}}" IconSize="Small" Foreground="{DynamicResource Brush.TextPrimary}" />` in `WpfApp2/Controls/Shell/SidebarControl.xaml`
- [X] T019 [US3] Verify `WpfApp2/Services/SidebarRegistrationService.cs` icon names are all valid `FluentIcons.Common.Symbol` enum values — fix any invalid names using the mapping table from T006
- [X] T020 [US3] Build and verify: `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds; sidebar shows Fluent 2 icons for all 24 registered pages; navigating to each page works

**Checkpoint**: Sidebar shows Fluent 2 icons for all pages; dynamic icon resolution works via converter

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup, verification, and documentation

- [X] T021 [P] Search entire `WpfApp2/` codebase for remaining `FontFamily="Segoe MDL2 Assets"` references — fix any found in files not yet migrated
- [X] T022 [P] Search entire `WpfApp2/` codebase for remaining `TextBlock Text="` patterns that render icons as text — replace with `FluentIcon` controls where appropriate
- [X] T023 Verify Dark/Light theme switching preserves icon coloring across all pages — `Foreground` must use `{DynamicResource Brush.TextPrimary}` or equivalent theme token
- [X] T024 Run full VSTO smoke test per quickstart.md: Ribbon buttons visible → ShellWindow opens → sidebar renders with Fluent 2 icons → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes
- [X] T025 Update `specs/032-fluent-icon-control-migration/icon-mapping.md` with final verified mapping (add any discovered gaps)
- [X] T026 Constitution compliance review — verify DynamicResource-only usage across all modified files, ThemeManager integration preserved, Excel rendering safety maintained, WindowChrome inheritance unchanged

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational (Phase 2) — `FluentIconConverter` must exist
- **US2 (Phase 4)**: Depends on Foundational (Phase 2) — can run in parallel with US1 (different files)
- **US3 (Phase 5)**: Depends on Foundational (Phase 2) — can run in parallel with US1/US2 (different files)
- **Polish (Phase 6)**: Depends on US1 + US2 + US3 all complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — No dependencies on other stories
- **US2 (P1)**: Can start after Foundational — Independent of US1 (different files)
- **US3 (P2)**: Can start after Foundational — Independent of US1/US2 (different files)

### Within Each User Story

- Namespace additions before element replacements
- Converter registration before converter usage
- Build verification after each file modification group

### Parallel Opportunities

- **Phase 1**: T003 (icon audit) can run in parallel with T001-T002
- **Phase 2**: T006 (mapping table) and T007 (ViewModel audit) can run in parallel
- **Phase 3+4+5**: US1, US2, US3 modify different files and can be implemented in parallel
- **Phase 6**: T021 and T022 (codebase searches) can run in parallel

---

## Parallel Example: User Stories 1, 2, and 3

```text
# After Phase 2 completes, these can run simultaneously:

# Developer A — US1 (Widget icons + Diagnostics):
Task T009: Replace icons in WidgetCardStyles.xaml
Task T010: Update WidgetViewModel icon assignments
Task T011: Replace icons in DiagnosticsPanel.xaml

# Developer B — US2 (Settings panel):
Task T013: Audit SettingsPanelStyles.xaml
Task T014: Replace Segoe MDL2 Assets in SettingsPanelStyles.xaml

# Developer C — US3 (Sidebar):
Task T017: Add namespace to SidebarControl.xaml
Task T018: Replace bullet with FluentIcon in SidebarControl.xaml
Task T019: Verify icon names in SidebarRegistrationService.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Widget icons and Diagnostics icons render as Fluent 2 in both themes
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (icons across pages) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (control rendering) → Test independently → Deploy/Demo
4. Add US3 (sidebar icons) → Test independently → Deploy/Demo
5. Polish → Final verification → Ready for `/speckit.implement`

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Widget + Diagnostics icons)
   - Developer B: US2 (Settings panel)
   - Developer C: US3 (Sidebar)
3. Stories complete and integrate independently
4. Merge and run Polish phase together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **MaterialDesign is already removed** — no tasks needed for FR-001 through FR-009, FR-014
- **WPF-UI pilot (FR-010, FR-011) is deferred** to Phase 8 — no tasks in this plan
- **Icon mapping (FR-012) is partially done** — SidebarRegistrationService already uses Fluent 2 names
