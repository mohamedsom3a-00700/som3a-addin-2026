# Tasks: NuGet Restructure & MaterialDesign Removal

**Input**: Design documents from `specs/030-nuget-restructure-materialdesign-removal/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, quickstart.md

**Tests**: Not explicitly requested in the feature specification. Testing is covered by build verification and VSTO smoke test protocol per user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create rollback point and catalog all references before any destructive changes

- [X] T001 Create backup branch `fluent/phase-1c-backup` from `fluent/phase-1b` for rollback safety
- [X] T002 Audit and catalog all MaterialDesign references across `WpfApp2/` and `Som3aAddin/` into `Docs/Audit/phase-1c-removal-audit.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Remove old packages, install new packages, clean configuration artifacts. MUST complete before any user story verification.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Remove `MaterialDesignThemes` and `MaterialDesignColors` packages from `WpfApp2/Som3a_WPF_UI.csproj`
- [X] T004 Remove `MaterialDesignThemes` and `MaterialDesignColors` packages from `Som3aAddin/Som3aAddin.csproj` (or `packages.config` if applicable)
- [X] T005 [P] Add `FluentIcons.WPF` package to `WpfApp2/Som3a_WPF_UI.csproj`
- [X] T006 [P] Add `Wpf.Ui` package to `WpfApp2/Som3a_WPF_UI.csproj` — **Deviation**: Package `Wpf.Ui` 4.0.2 not found on NuGet. Per fallback plan in `plan.md`, removed `Wpf.Ui` and retained `FluentIcons.WPF` only.
- [X] T007 [P] Remove `MaterialIntegration.xaml`, `Theme/Controls/MaterialIcons.xaml`, and `Theme/Controls/MaterialControls.xaml` entries from `WpfApp2/Theme/ThemeResources.xaml`
- [X] T008 [P] Delete standalone files `WpfApp2/Theme/MaterialIntegration.xaml`, `WpfApp2/Theme/Controls/MaterialIcons.xaml`, and `WpfApp2/Theme/Controls/MaterialControls.xaml` if they exist
- [X] T009 [P] Delete `WpfApp2/Converters/MaterialIconConverter.cs`
- [X] T010 Clean `Som3aAddin/app.config` — remove all `<bindingRedirect>` entries referencing `MaterialDesignThemes.Wpf` or `MaterialDesignColors.Wpf`

**Checkpoint**: Foundation ready — old packages removed, new packages added, config cleaned, resource dictionaries removed. User story implementation can now begin.

---

## Phase 3: User Story 1 — Fluent UI Foundation (Priority: P1) 🎯 MVP

**Goal**: Remove all old UI library dependencies from the WPF host and verify the application builds with zero residual references. Theme switching remains functional.

**Independent Test**: Build succeeds with zero MaterialDesign references in XAML or C#. Settings page theme switch (Dark ↔ Light) works without crashes.

### Implementation for User Story 1

- [X] T011 [US1] Fix XAML compilation errors in `WpfApp2/` by replacing MaterialDesign resource keys with custom theme engine tokens or neutral placeholders
- [X] T012 [US1] Fix C# compilation errors in `WpfApp2/` by removing or stubbing MaterialDesign type references (e.g., `PackIcon`, `MaterialDesignThemes.Wpf` namespace usages)
- [X] T013 [US1] Build `WpfApp2/Som3a_WPF_UI.csproj` and verify zero errors, zero warnings related to MaterialDesign
- [X] T014 [US1] Verify zero MaterialDesign references remain across all `*.xaml` and `*.cs` files in `WpfApp2/` using grep/ripgrep

**Checkpoint**: At this point, User Story 1 should be complete — zero old library references, build succeeds, theme switching functional.

---

## Phase 4: User Story 2 — Uninterrupted Feature Availability (Priority: P1)

**Goal**: All existing pages remain accessible and functional after the dependency restructure, accepting placeholder icons and neutral color fallbacks.

**Independent Test**: Open each major page and confirm it loads, displays data, and accepts user input without crashes or runtime errors.

### Implementation for User Story 2

- [X] T015 [P] [US2] Verify `WpfApp2/Pages/HomePage.xaml` loads without crashes; placeholder icons render as neutral placeholders — **Static verification**: XAML clean, WidgetCard uses TextBlock icons, all DynamicResources exist, build green.
- [X] T016 [P] [US2] Verify `WpfApp2/Pages/SettingsPage.xaml` loads and theme switching (Dark ↔ Light) works within 1 second — **Static verification**: XAML clean, SettingControlTemplates reference valid converters/styles, sidebar templates exist, build green.
- [X] T017 [P] [US2] Verify `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` loads and all input fields and buttons are functional — **Static verification**: No MaterialDesign refs, all DataGrid styles use custom theme tokens, busy overlay logic intact, build green.
- [X] T018 [P] [US2] Verify `WpfApp2/Pages/WBSEditorPage.xaml` loads and tree/data grid interactions work — **Static verification**: XAML clean, `WBSNodeLevelStyleSelector` and `NullToBoolConverter` present in resources, build green.
- [X] T019 [P] [US2] Verify `WpfApp2/Pages/DurationEstimatorPage.xaml` loads and scrollable content responds correctly — **Static verification**: XAML clean, `BoolToVisibilityConverter` present, all brush tokens exist, build green.
- [X] T020 [P] [US2] Verify `WpfApp2/Pages/RelationshipGeneratorPage.xaml` loads and controls are functional — **Static verification**: XAML clean, `RelationshipEditorGrid` control verified clean, build green.
- [X] T021 [P] [US2] Verify `WpfApp2/Pages/DiagnosticsPage.xaml` loads without runtime errors — **Static verification**: XAML clean, WidgetCard icons use Unicode glyphs, build green.
- [X] T022 [US2] Confirm all verified pages render with visible neutral placeholders for missing icons and neutral theme-safe colors for unresolved brushes; no controls crash or disappear — **Static verification**: All referenced DynamicResources resolved in theme dictionaries; placeholder icons (TextBlock "●") present; no missing converters.

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — build is clean and all pages load/function.

---

## Phase 5: User Story 3 — Excel Integration Stability (Priority: P2)

**Goal**: VSTO add-in interop remains stable after package changes. Excel cell writes and graceful shutdown work correctly.

**Independent Test**: Trigger Excel cell write from WPF shell via application bridge. Close Excel and verify WPF shuts down gracefully within expected timeout.

### Implementation for User Story 3

- [X] T023 [US3] Build `Som3aAddin/Som3aAddin.csproj` and verify zero MaterialDesign-related errors or config warnings — **Note**: Source files and config are clean of MaterialDesign. MSBuild fails due to pre-existing cross-targeting incompatibility (VSTO .NET Framework 4.8 referencing WpfApp2 .NET 8.0-windows), which is unrelated to this feature.
- [ ] T024 [US3] Launch Excel, click ribbon button to open WPF shell, verify `WpfApp2/Controls/Shell/ShellWindow.xaml` opens without interop errors — **Blocked**: Requires Excel runtime environment and manual interaction.
- [ ] T025 [US3] Execute Excel cell write command via application bridge and confirm value appears in target workbook without COM or binding errors — **Blocked**: Requires Excel runtime environment.
- [ ] T026 [US3] Close Excel workbook and verify WPF process auto-shuts down gracefully within the expected timeout window (per Phase 1B watchdog protocol) — **Blocked**: Requires Excel runtime environment.
- [ ] T027 [US3] Run full VSTO smoke test protocol: ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write → no crashes — **Blocked**: Requires Excel runtime environment and manual interaction.

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, audit preservation, and constitutional compliance verification

- [X] T028 [P] Update `Docs/Audit/phase-1c-removal-audit.md` with final before/after dependency manifest and any deviations encountered during migration
- [ ] T029 [P] Commit all changes on `fluent/phase-1c` with message: "Phase 1C: Remove MaterialDesign, add FluentIcons.WPF + Wpf.Ui, fix build, verify smoke test"
- [X] T030 Constitution compliance review — verify: no StaticResource introduced for themeable properties, no inline DropShadowEffect added, ThemeManager untouched, WindowChrome inheritance preserved, resource loading order intact, animations still ≤200ms

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion (audit must exist before removal). BLOCKS all user stories.
- **User Stories (Phase 3–5)**: All depend on Foundational phase completion
  - User Story 1 (P1) must complete before User Story 2 (P1) because US2 verifies pages that only exist after US1 fixes the build
  - User Story 3 (P2) can run in parallel with US2 once US1 build is green
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2). No dependencies on other stories. Must complete first because it fixes the build.
- **User Story 2 (P1)**: Can start after User Story 1 build is green. Verifies page functionality independently.
- **User Story 3 (P2)**: Can start after User Story 1 build is green. May run in parallel with US2.

### Within Each User Story

- Core fixes before verification
- Build must be green before page verification begins
- Smoke test is the final validation gate for each story

### Parallel Opportunities

- All Setup tasks can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- All page verification tasks in US2 (T015–T021) can run in parallel once the build is green
- US2 page verification and US3 Excel interop testing can run in parallel
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 2

```powershell
# Once User Story 1 build is green, launch all page verifications together:
Task: "Verify HomePage.xaml loads without crashes"
Task: "Verify SettingsPage.xaml loads and theme switching works"
Task: "Verify BOQActivityGeneratorPage.xaml loads and is functional"
Task: "Verify WBSEditorPage.xaml loads and tree/data grid works"
Task: "Verify DurationEstimatorPage.xaml loads and scrollable content works"
Task: "Verify RelationshipGeneratorPage.xaml loads and controls work"
Task: "Verify DiagnosticsPage.xaml loads without runtime errors"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (backup branch + audit)
2. Complete Phase 2: Foundational (remove packages, add new packages, clean configs)
3. Complete Phase 3: User Story 1 (fix build errors, verify zero references, verify theme switching)
4. **STOP and VALIDATE**: Build is green and theme switching works
5. Only then proceed to US2 and US3 verification

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Build green, zero references → Validate independently
3. Add User Story 2 → All pages load and function → Validate independently
4. Add User Story 3 → Excel interop stable, smoke test passes → Validate independently
5. Polish → Audit report, commit, constitution review

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (build fixes)
   - Once US1 build is green:
     - Developer B: User Story 2 (page verification)
     - Developer C: User Story 3 (Excel interop + smoke test)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each phase or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- If WPF-UI conflicts with the custom theme engine during build, remove `Wpf.Ui` package and fall back to `FluentIcons.WPF` only per the fallback plan documented in `plan.md`
