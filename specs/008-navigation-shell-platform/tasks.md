---

description: "Task list for Navigation Shell Platform feature implementation"

---

# Tasks: Navigation Shell Platform

**Input**: Design documents from `/specs/008-navigation-shell-platform/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not included — no explicit test requirements in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All paths are relative to `WpfApp2/` (the WPF project directory under the repository root):

- `WpfApp2/Controls/Shell/` — Shell window, sidebar, workspace host, command palette
- `WpfApp2/Pages/` — Shell-hosted page base class and welcome page
- `WpfApp2/Services/` — NavigationService
- `WpfApp2/Theme/` — ShellStyles.xaml resource dictionary

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and integration points for the shell feature

- [X] T001 Create `Controls/Shell/` and `Pages/` directories under `WpfApp2/`
- [X] T002 Extend `ModernWindow.cs` with shell initialization lifecycle hook (virtual method `OnShellInitialize()`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared entities and infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 [P] Create `NavigationDestination` model class with `INotifyPropertyChanged` (Key, Label, Icon, Order, IsVisible, IsSelected) in `WpfApp2/Controls/Shell/`
- [X] T004 [P] Create `NavigationPage` data class (Key, DisplayName, Icon, Order, PageType) for page registry in `WpfApp2/Controls/Shell/`
- [X] T005 [P] Create `ShellState` runtime model (ActivePageKey, LastActivePageKey, SidebarVisible, CommandPaletteOpen, PreviousPageStack) in `WpfApp2/Controls/Shell/`
- [X] T006 [P] Create `PageBase` abstract base class in `WpfApp2/Pages/PageBase.cs` with DynamicResource theme support
- [X] T007 [P] Create `Theme/ShellStyles.xaml` resource dictionary with shared shell styles (sidebar item, workspace, status bar)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — Switch Between Feature Pages via Sidebar (Priority: P1) 🎯 MVP

**Goal**: User opens a new feature page in the shell workspace and navigates between pages via the sidebar. Welcome page displays on first open. Full keyboard navigation works.

**Independent Test**: Launch any two shell-hosted pages from the sidebar and verify each displays correctly in the workspace area with the sidebar visible and the active item highlighted.

- [X] T008 [P] [US1] Create `WorkspaceHost.cs` implementing `IPageHost` contract (`Navigate`, `ShowError`, `ShowWelcome`, `Clear`) in `WpfApp2/Controls/Shell/`
- [X] T009 [P] [US1] Create `ShellWindow.xaml` and `ShellWindow.xaml.cs` with sidebar + workspace + status bar layout, inheriting from `ModernWindow`
- [X] T010 [P] [US1] Create `SidebarControl.xaml` and `SidebarControl.xaml.cs` with `ListBox` bound to `ObservableCollection<NavigationDestination>`
- [X] T011 [US1] Create `WelcomePage.xaml` and `WelcomePage.xaml.cs` in `WpfApp2/Pages/` displaying available navigation destinations
- [X] T012 [US1] Wire sidebar `SelectedItem` change event → `WorkspaceHost.Navigate()` to switch displayed page
- [X] T013 [US1] Add sidebar full keyboard navigation (Tab enters/leaves sidebar, arrow keys move, Enter/Space activates, Home/End jumps)
- [X] T014 [US1] Implement first-run welcome page display and sidebar active item highlight (FR-003, FR-012)

**Checkpoint**: At this point, User Story 1 should be fully functional — sidebar navigation and workspace switching work independently

---

## Phase 4: User Story 2 — Launch Shell Page from Excel Ribbon (Priority: P2)

**Goal**: User clicks an Excel ribbon button and the corresponding feature opens as a page inside the shell (auto-opening the shell if needed). Existing standalone windows remain unchanged.

**Independent Test**: Click a ribbon button mapped to a shell page and confirm the shell opens (or activates if already open) and navigates to the correct page.

- [X] T015 [P] [US2] Create `NavigationEventArgs` class with `PreviousKey`, `NewKey`, `Success`, `Error` properties
- [X] T016 [US2] Implement `NavigationService` singleton with `INavigationService` contract (`RegisterPage<T>`, `NavigateTo`, `GoBack`, `Search`, `Destinations`, `NavigationChanged`) in `WpfApp2/Services/NavigationService.cs`
- [X] T017 [US2] Integrate ribbon callbacks to call `NavigationService.Instance.NavigateTo(key)` for shell-page features
- [X] T018 [US2] Add shell auto-open logic in `NavigationService` — create `ShellWindow` if not already open on `NavigateTo`
- [X] T019 [US2] Handle standalone window fallback for pre-Phase 11 features when ribbon maps to a non-shell feature

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — ribbon actions open shell pages correctly

---

## Phase 5: User Story 3 — Quick Navigate via Command Palette (Priority: P3)

**Goal**: Power user presses Ctrl+K to open an overlay, types a partial page name, selects a result, and navigates directly.

**Independent Test**: Open command palette via keyboard shortcut, type a partial page name, select a result, and confirm the workspace navigates to the correct page.

- [X] T020 [P] [US3] Create `CommandPalette.xaml` and `CommandPalette.xaml.cs` with `Popup` (AllowsTransparency=False), `TextBox`, and `ListBox` in `WpfApp2/Controls/Shell/`
- [X] T021 [P] [US3] Add command palette visual styles to `Theme/ShellStyles.xaml` (overlay background, input field, results list)
- [X] T022 [US3] Implement Ctrl+K keyboard shortcut binding in `ShellWindow` to toggle command palette open/close
- [X] T023 [US3] Implement search filtering in command palette — match typed text against `INavigationService.Destinations` display names
- [X] T024 [US3] Wire command palette result selection (Enter/click) → `NavigationService.NavigateTo(key)` and close palette

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final quality verification

- [X] T025 Add page entrance/exit fade transitions (≤200ms) referencing `Effects/Animations.xaml` storyboards
- [X] T026 Implement inline error state in workspace with retry button (IPageHost.ShowError) in `WorkspaceHost.cs`
- [X] T027 Verify DynamicResource-only usage across all new shell XAML files (`ShellWindow.xaml`, `SidebarControl.xaml`, `CommandPalette.xaml`, `WelcomePage.xaml`, `ShellStyles.xaml`)
- [X] T028 Verify Excel rendering safety — all shell windows inherit `ModernWindow`, no `AllowsTransparency="True"`, WindowChrome primary rendering strategy, Effect sources from centralized `Effects/Shadows.xaml`
- [X] T029 Verify responsive sidebar scrolling performance with up to 25 registered navigation items in `ListBox` virtualization

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - US1 (P1) → US2 (P2): US2 builds on NavigationService introduced in US2, but US1 independently testable
  - US2 (P2) → US3 (P3): US3 requires NavigationService Destinations collection from US2
  - Stories proceed sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 — Sidebar Navigation (P1)**: Can start after Foundational (Phase 2). Independent of US2 and US3.
- **US2 — Ribbon Integration (P2)**: Depends on US1 (ShellWindow must exist for ribbon to open). NavigationService adds ribbon integration layer.
- **US3 — Command Palette (P3)**: Depends on US2 (NavigationService.Destinations collection). Builds on existing shell and sidebar.

### Within Each User Story

- Starting tasks marked [P] can be done concurrently
- Core implementation before integration wiring
- Story complete before moving to next priority

---

## Parallel Opportunities

- **Phase 2 (Foundational)**: All 5 tasks marked [P] can run in parallel
- **Phase 3 (US1)**: T008 WorkspaceHost, T009 ShellWindow, T010 SidebarControl can run in parallel; T011-T014 sequential
- **Phase 4 (US2)**: T015 NavigationEventArgs can run in parallel with NavigationService design; T016-T019 sequential
- **Phase 5 (US3)**: T020 CommandPalette XAML and T021 ShellStyles can run in parallel; T022-T024 sequential

---

## Parallel Example: User Story 1

```text
# Launch all US1 parallel tasks together:
Task: "Create WorkspaceHost.cs implementing IPageHost contract"
Task: "Create ShellWindow.xaml with sidebar + workspace + status bar layout"
Task: "Create SidebarControl.xaml with ListBox bound to NavigationDestination collection"

# After parallel tasks complete, continue with:
Task: "Create WelcomePage.xaml and WelcomePage.xaml.cs"
Task: "Wire sidebar selection → workspace navigation"
Task: "Add keyboard navigation support"
Task: "First-run welcome page and active highlight"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 — Sidebar Navigation
4. **STOP and VALIDATE**: Test US1 independently — launch pages via sidebar, verify workspace switching, keyboard nav, welcome page
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Sidebar Navigation) → Test independently → **MVP!**
3. Add US2 (Ribbon Integration) → Test independently → Deploy/Demo
4. Add US3 (Command Palette) → Test independently → Deploy/Demo
5. Polish & Cross-Cutting → Final quality pass

### File Creation Order Per Phase

**Phase 2 (Foundational)**:
- Parallel: `NavigationDestination`, `NavigationPage`, `ShellState`, `PageBase`, `ShellStyles.xaml`

**Phase 3 (US1)**:
- Parallel start: `WorkspaceHost.cs`, `ShellWindow.xaml+c.s`, `SidebarControl.xaml+c.s`
- Then: `WelcomePage.xaml+c.s`
- Then: Wiring, keyboard nav, first-run display

**Phase 4 (US2)**:
- Start: `NavigationEventArgs.cs`
- Then: `NavigationService.cs`
- Then: Ribbon callbacks, auto-open logic, fallback handling

**Phase 5 (US3)**:
- Parallel start: `CommandPalette.xaml+c.s`, ShellStyles updates
- Then: Keyboard binding, search filtering, selection wiring

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All new XAML files MUST use `{DynamicResource}` for themeable properties (Constitution §III)
- All new effects MUST reference centralized Effects/Shadows.xaml (Constitution §XII)
- Page transitions MUST stay ≤200ms and use GPU-safe opacity fades (Constitution §IX)
