# Tasks: Dashboard & Home (Phase 23)

**Input**: Design documents from `/specs/023-dashboard-home/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: Not explicitly requested in spec — test tasks are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Contracts**: `Som3a.Contracts/` (.NET 8.0 class library)
- **WPF Host**: `WpfApp2/` (.NET Framework 4.8)
- **ViewModels**: `WpfApp2/ViewModels/Dashboard/`
- **Services**: `WpfApp2/Services/`
- **Pages**: `WpfApp2/Pages/`
- **Controls**: `WpfApp2/Controls/`
- **Models**: `WpfApp2/Models/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and directory structure

- [x] T001 Create `WpfApp2/ViewModels/Dashboard/` directory for widget ViewModels
- [x] T002 Verify `Som3a.Contracts/Som3a.Contracts.csproj` builds successfully with `dotnet build`
- [x] T003 [P] Create `CHANGELOG.md` at repository root if not already present (placeholder for Updates widget)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core contracts, models, services, and controls that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

### Contracts (Som3a.Contracts)

- [x] T004 [P] Create `IRecentItemsService` interface in `Som3a.Contracts/IRecentItemsService.cs` with methods: `GetRecentTools()`, `GetRecentProjects()`, `AddRecentTool(string toolId, string displayName)`, `AddRecentProject(string filePath, string displayName)`, `ClearRecentTools()`, `ClearRecentProjects()`
- [x] T005 [P] Create `IPerformanceMonitor` interface in `Som3a.Contracts/IPerformanceMonitor.cs` with properties: `StartupTimeMs`, `LastNavigationTimeMs`, `LastNavigationTarget`; methods: `RecordAppStart()`, `BeginNavigation(string targetKey)`, `EndNavigation()`; event: `NavigationCompleted`
- [x] T006 [P] Create `IChangelogService` interface in `Som3a.Contracts/IChangelogService.cs` with methods: `GetLatestEntry()` returning `ChangelogEntry`, `GetAllEntries()` returning `List<ChangelogEntry>`

### Models (WpfApp2)

- [x] T007 [P] Create `RecentItem` model in `WpfApp2/Models/RecentItem.cs` with properties: `ToolId` (string, nullable), `FilePath` (string, nullable), `DisplayName` (string), `Timestamp` (DateTime UTC). Include JSON serialization attributes for `System.Text.Json`.
- [x] T008 [P] Create `WidgetDefinition` model in `WpfApp2/Models/WidgetDefinition.cs` with `WidgetType` enum (Version=0, Updates=1, RecentTools=2, RecentProjects=3, DiagnosticsSummary=4, AIProviderStatus=5, PerformanceSummary=6, QuickActions=7, PluginStatus=8), and properties: `Title`, `Icon`, `DisplayOrder`

### Services (WpfApp2)

- [x] T009 Implement `RecentItemsService` in `WpfApp2/Services/RecentItemsService.cs` — JSON persistence at `%AppData%/Som3a/recent-items.json`, max 5 items per list, ordered by timestamp descending, duplicate detection (update timestamp instead of adding), `SemaphoreSlim` for thread safety. Implement `IRecentItemsService`.
- [x] T010 Implement `PerformanceMonitor` in `WpfApp2/Services/PerformanceMonitor.cs` — Record `AppStartTime` via `DateTime.UtcNow`, track navigation duration via `Stopwatch`, fire `NavigationCompleted` event. Implement `IPerformanceMonitor`.
- [x] T011 Implement `ChangelogService` in `WpfApp2/Services/ChangelogService.cs` — Line-based parser for `CHANGELOG.md`, find first `## [` line, extract version/date, collect `- ` bullet lines. Cache result in memory after first parse. Fallback: return empty entry if file not found. Implement `IChangelogService`.

### Controls (WpfApp2)

- [x] T012 [P] Create `WidgetCard` custom control in `WpfApp2/Controls/WidgetCard.cs` — Extend `ContentControl`, add dependency properties: `Title` (string), `Icon` (string), `IsLoading` (bool), `ErrorMessage` (string). Override `DefaultStyleKeyProperty`.
- [x] T013 [P] Create `WidgetCardStyles.xaml` in `WpfApp2/Controls/WidgetCardStyles.xaml` — Style for `WidgetCard` using `DynamicResource` tokens only (Brush.Surface, Brush.TextPrimary, Brush.TextSecondary, Brush.Border, Shadow.Card). Include loading spinner template, error state template, and empty state template. No inline `DropShadowEffect`.
- [x] T014 Merge `WidgetCardStyles.xaml` into `WpfApp2/Theme/ThemeResources.xaml` — Add `<ResourceDictionary Source="Controls/WidgetCardStyles.xaml"/>` after control styles in the loading order.

### Base ViewModel

- [x] T015 Create base `WidgetViewModel` in `WpfApp2/ViewModels/Dashboard/WidgetViewModel.cs` — Extend `ViewModelBase`, add properties: `Title` (string), `Icon` (string), `IsLoading` (bool), `ErrorMessage` (string), `IsLoaded` (bool). Add methods: `Task LoadDataAsync()`, `Task RefreshAsync()`. Implement loading state machine (Uninitialized → Loading → Loaded/Error).

### Service Registration

- [x] T016 Register new services in `WpfApp2/CompositionRoot.cs` — Add singleton registrations: `IRecentItemsService` → `RecentItemsService`, `IPerformanceMonitor` → `PerformanceMonitor`, `IChangelogService` → `ChangelogService`. Place after existing service registrations.

**Checkpoint**: Foundation ready — all contracts, models, services, and controls are in place. User story implementation can now begin.

---

## Phase 3: User Story 1 - Home Dashboard Landing Page (Priority: P1) MVP

**Goal**: Home dashboard renders as default Shell landing page with all 9 widgets displaying live data. Users can navigate via Quick Actions and Recent Tools.

**Independent Test**: Launch Shell workspace → Home page renders within 1 second → All 9 widgets visible with data → Click Quick Action navigates correctly → Click Recent Tool navigates correctly.

### Widget ViewModels

- [x] T017 [P] [US1] Create `VersionWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/VersionWidgetViewModel.cs` — Read `Assembly.GetExecutingAssembly().GetName().Version` for app version, `Environment.Version` for .NET version, `Environment.OSVersion` for Windows version. Static data, no refresh needed. Extend `WidgetViewModel`.
- [x] T018 [P] [US1] Create `UpdatesWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/UpdatesWidgetViewModel.cs` — Inject `IChangelogService`, call `GetLatestEntry()`, expose `Version`, `Date`, `Changes` (list of strings). Handle missing file: set `ErrorMessage` to "No updates available". Extend `WidgetViewModel`.
- [x] T019 [P] [US1] Create `RecentToolsWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/RecentToolsWidgetViewModel.cs` — Inject `IRecentItemsService` and `INavigationService`. Expose `ObservableCollection<RecentItem> RecentTools`. Add `NavigateToTool(string toolId)` command. Subscribe to `NavigationService.NavigationChanged` to track tool usage via `AddRecentTool()`. Empty state: "Your recently used tools will appear here". Extend `WidgetViewModel`.
- [x] T020 [P] [US1] Create `RecentProjectsWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/RecentProjectsWidgetViewModel.cs` — Inject `IRecentItemsService`. Expose `ObservableCollection<RecentItem> RecentProjects`. Empty state: "Your recently opened projects will appear here". Extend `WidgetViewModel`.
- [x] T021 [P] [US1] Create `DiagnosticsSummaryWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/DiagnosticsSummaryWidgetViewModel.cs` — Inject `IDiagnosticsService`. Call `CaptureSnapshot()` to get `DiagnosticSnapshot`. Expose `RenderMode`, `ActiveTheme`, `MemoryUsageMB`, `GpuAvailable`. Refresh every 5 seconds via `DispatcherTimer`. Subscribe to `ThemeManager.ThemeChanged` for immediate theme update. Extend `WidgetViewModel`.
- [x] T022 [P] [US1] Create `AIProviderStatusWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/AIProviderStatusWidgetViewModel.cs` — Inject `IAIOrchestrator` (or resolve providers from `ServiceContainer`) and `ITokenTracker`. Expose `ObservableCollection<AIProviderStatusInfo> Providers` and `TotalSessionTokens`. Call `HealthCheckAsync()` per provider with 30-second cache. Refresh every 30 seconds via background `Task`. Empty state: "No AI providers configured" with settings link. Extend `WidgetViewModel`.
- [x] T023 [P] [US1] Create `PerformanceSummaryWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/PerformanceSummaryWidgetViewModel.cs` — Inject `IPerformanceMonitor`. Expose `StartupTimeMs`, `LastNavigationTimeMs`, `LastNavigationTarget`. Subscribe to `PerformanceMonitor.NavigationCompleted` event for live updates. Extend `WidgetViewModel`.
- [x] T024 [P] [US1] Create `QuickActionsWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/QuickActionsWidgetViewModel.cs` — Inject `INavigationService`. Expose `ICommand NavigateToWBSCommand`, `NavigateToBOQCommand`, `NavigateToSettingsCommand`. Each command calls `NavigationService.NavigateTo()` with appropriate page key. Extend `WidgetViewModel`.
- [x] T025 [P] [US1] Create `PluginStatusWidgetViewModel` in `WpfApp2/ViewModels/Dashboard/PluginStatusWidgetViewModel.cs` — Inject `Contracts.IModuleRegistry`. Call `GetAllModules()` and group by `ModuleState`. Expose `TotalCount`, `ActiveCount`, `FailedCount`, `DegradedCount`, `HealthStatus` (Healthy/Degraded/Unhealthy). Subscribe to `ModuleStateChanged` event for live updates. Extend `WidgetViewModel`.

### Home Page

- [x] T026 [US1] Create `HomeViewModel` in `WpfApp2/ViewModels/HomeViewModel.cs` — Inject all 9 widget ViewModels via `IServiceContainer`. Expose `ObservableCollection<WidgetViewModel> Widgets` in fixed order (Version, Updates, RecentTools, RecentProjects, DiagnosticsSummary, AIProviderStatus, PerformanceSummary, QuickActions, PluginStatus). Implement `LoadAsync()` to initialize all widgets in parallel via `Task.WhenAll()`. Add `NavigateToDiagnosticsCommand`.
- [x] T027 [US1] Create `HomePage.xaml` in `WpfApp2/Pages/HomePage.xaml` — Use `ItemsControl` with `UniformGrid` (Columns="3", Rows="3") as `ItemsPanel`. Bind `ItemsSource` to `HomeViewModel.Widgets`. Use `WidgetCard` control in `ItemTemplate` with `DataTemplate` per widget type. Wrap in `ScrollViewer` for smaller screens. All colors use `DynamicResource`. Apply `MinWidth="280"` and `MinHeight="180"` per widget card.
- [x] T028 [US1] Create `HomePage.xaml.cs` in `WpfApp2/Pages/HomePage.xaml.cs` — Extend `PageBase`. Resolve `HomeViewModel` from `App.Container` in constructor. On `Page.Loaded`: call `ViewModel.LoadAsync()`. On `Page.Unloaded`: dispose timers and unsubscribe events.

### Navigation Integration

- [x] T029 [US1] Register `HomePage` in `WpfApp2/Services/SidebarRegistrationService.cs` — Add `NavigationService.Instance.RegisterPage<HomePage>("home", "Home", icon: "Home", order: 0)` under new "Home" category.
- [x] T030 [US1] Add "Home" category to `WpfApp2/Services/NavigationService.cs` — Add `"Home"` to `CategoryOrder` dictionary with order `0` (appears before Planning=1).
- [x] T031 [US1] Modify `WpfApp2/Controls/Shell/ShellWindow.xaml.cs` — Change `WelcomePageType = typeof(WelcomePage)` to `WelcomePageType = typeof(HomePage)`. Verify `WorkspaceHost.ShowWelcome()` navigates to HomePage on shell launch.

**Checkpoint**: Home dashboard is fully functional as default landing page with all 9 widgets. MVP deliverable.

---

## Phase 4: User Story 2 - Diagnostics Dashboard (Priority: P2)

**Goal**: Dedicated Diagnostics page with detailed real-time system metrics accessible from the Home dashboard.

**Independent Test**: Navigate to Diagnostics page from Home → Render mode indicator shows correct value → Memory gauge updates every 5 seconds → Theme change reflects immediately → Last error log entry displayed with timestamp.

### Implementation for User Story 2

- [x] T032 [US2] Create `DashboardViewModel` in `WpfApp2/ViewModels/DashboardViewModel.cs` — Extend `ViewModelBase`. Inject `IDiagnosticsService`, `ILoggingService`, `RenderModeService`. Expose detailed properties: `RenderMode`, `RenderModeSource`, `ActiveTheme`, `AccentColor`, `DPIScale`, `MemoryWorkingSetMB`, `MemoryManagedMB`, `GpuAvailable`, `GpuName`, `PopupStatus` (string[]), `LastErrorEntry` (LogEntry), `ValidationResults`. Use `DispatcherTimer` with 5-second interval to call `CaptureSnapshot()`. Subscribe to `ThemeManager.ThemeChanged` for immediate theme update. Add `CopyToClipboardCommand` and `RefreshCommand`.
- [x] T033 [US2] Create `DiagnosticsPage.xaml` in `WpfApp2/Pages/DiagnosticsPage.xaml` — Vertical `ScrollViewer` with `StackPanel` containing detailed diagnostic cards: Render Mode card, Theme card (with accent swatch preview), DPI card, Memory gauge card (progress bar), GPU card, Popup Diagnostics card (list), Last Error card (timestamp + message), Validation Results card. Add "Copy to Clipboard" button and "Refresh" button in header. All colors use `DynamicResource`.
- [x] T034 [US2] Create `DiagnosticsPage.xaml.cs` in `WpfApp2/Pages/DiagnosticsPage.xaml.cs` — Extend `PageBase`. Resolve `DashboardViewModel` from `App.Container`. On `Page.Loaded`: start refresh timer. On `Page.Unloaded`: stop refresh timer. Handle clipboard copy command.
- [x] T035 [US2] Register `DiagnosticsPage` in `WpfApp2/Services/SidebarRegistrationService.cs` — Add `NavigationService.Instance.RegisterPage<DiagnosticsPage>("diagnostics", "Diagnostics", icon: "Diagnostics", order: 99)` under "Settings" category.

**Checkpoint**: Diagnostics page accessible from Home widget, updates in real-time, all diagnostic sections functional.

---

## Phase 5: User Story 3 - AI Provider Status Monitoring (Priority: P2)

**Goal**: AI Provider Status widget on Home dashboard shows accurate online/offline state per provider with cached health checks and session token usage.

**Independent Test**: View AI Provider Status widget → Online providers show green indicator → Offline providers show red/gray indicator → Token usage displayed for current session → If no providers configured, message with settings link appears.

### Implementation for User Story 3

- [x] T036 [US3] Implement health check caching in `WpfApp2/ViewModels/Dashboard/AIProviderStatusWidgetViewModel.cs` — Add `Dictionary<string, (bool IsOnline, DateTime LastChecked)> _healthCache` with 30-second TTL. On refresh: skip health check if cache is valid. Run health checks on background thread via `Task.Run()`. Display "Unknown" state for providers not yet checked. Show last known state while refresh is in progress.
- [x] T037 [US3] Implement "No providers configured" empty state in `WpfApp2/ViewModels/Dashboard/AIProviderStatusWidgetViewModel.cs` — When `IAIOrchestrator` has no registered providers, set `ErrorMessage` to "No AI providers configured" and expose `NavigateToSettingsCommand` that navigates to `"settings.general"` page. Update `HomePage.xaml` DataTemplate to show settings link when `ErrorMessage` contains "No AI providers".

**Checkpoint**: AI Provider Status widget meets all US3 acceptance scenarios — online/offline indicators, token usage, empty state with settings link.

---

## Phase 6: User Story 4 - Plugin Status Overview (Priority: P3)

**Goal**: Plugin Status widget on Home dashboard shows accurate plugin health counts and navigates to existing plugin registry view on click.

**Independent Test**: View Plugin Status widget → Healthy plugins show green count → Failed/degraded plugins highlighted in red/yellow → Click widget navigates to existing plugin registry view.

### Implementation for User Story 4

- [x] T038 [US4] Implement click-to-navigate in `WpfApp2/ViewModels/Dashboard/PluginStatusWidgetViewModel.cs` — Add `NavigateToPluginRegistryCommand` that calls `NavigationService.NavigateTo()` with the existing plugin registry page key. Inject `INavigationService` via constructor. Update `HomePage.xaml` DataTemplate to make Plugin Status widget clickable (bind to command). Verify the existing plugin registry page key is used (check `SidebarRegistrationService` for the registered key).

**Checkpoint**: Plugin Status widget meets all US4 acceptance scenarios — health counts, color-coded indicators, click navigates to existing plugin registry view.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Constitution compliance, performance validation, and Excel VSTO host testing

- [x] T039 Constitution compliance review — Verify all new XAML files use `DynamicResource` only (no `StaticResource` for themeable properties). Verify no inline `DropShadowEffect`. Verify `WidgetCardStyles.xaml` follows resource loading order. Verify `HomePage` and `DiagnosticsPage` are Pages within Shell (no standalone windows). Verify MVVM pattern (no business logic in code-behind).
- [x] T040 Performance validation — Measure HomePage load time (must be <1 second). Verify `HomeViewModel.LoadAsync()` initializes widgets in parallel. Verify `DispatcherTimer` intervals are correct (5s for diagnostics, 30s for AI health checks). Verify timers are disposed on page unload (no memory leaks).
- [x] T041 Excel VSTO host test — Launch Shell in Excel VSTO host. Verify HomePage renders in FallbackSafe mode without black screen. Verify all 9 widgets display correctly. Verify no GPU-dependent effects break rendering. Verify Diagnostics page renders correctly. Verify navigation works from Quick Actions and Recent Tools.
- [x] T042 Build validation — Run `dotnet build Som3a.Contracts/Som3a.Contracts.csproj` with zero errors. Run `MSBuild.exe WpfApp2/Som3a_WPF_UI.csproj /p:Configuration=Debug` with zero errors. Verify no compiler warnings in new files.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational phase — MVP deliverable
- **US2 (Phase 4)**: Depends on Foundational phase — can start in parallel with US1
- **US3 (Phase 5)**: Depends on US1 (AI widget created in T022)
- **US4 (Phase 6)**: Depends on US1 (Plugin widget created in T025)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **US2 (P2)**: Can start after Foundational (Phase 2) — Independent of US1 (separate page)
- **US3 (P2)**: Depends on US1 task T022 (AIProviderStatusWidgetViewModel must exist)
- **US4 (P3)**: Depends on US1 task T025 (PluginStatusWidgetViewModel must exist)

### Within Each Phase

- **Phase 2**: Contracts (T004-T006) can run in parallel. Models (T007-T008) can run in parallel. Services (T009-T011) depend on their respective contracts. Controls (T012-T013) can run in parallel.
- **Phase 3**: All 9 widget ViewModels (T017-T025) can run in parallel. HomeViewModel (T026) depends on all widgets. HomePage (T027-T028) depends on HomeViewModel. Navigation (T029-T031) depends on HomePage.

### Parallel Opportunities

- T004, T005, T006 (contracts) — 3 parallel tasks
- T007, T008 (models) — 2 parallel tasks
- T012, T013 (controls) — 2 parallel tasks
- T017-T025 (widget ViewModels) — 9 parallel tasks
- US1 and US2 can run in parallel (different pages, no shared files)

---

## Parallel Example: Phase 2 Foundational

```text
# Launch all contracts in parallel:
Task T004: "Create IRecentItemsService in Som3a.Contracts/IRecentItemsService.cs"
Task T005: "Create IPerformanceMonitor in Som3a.Contracts/IPerformanceMonitor.cs"
Task T006: "Create IChangelogService in Som3a.Contracts/IChangelogService.cs"

# Launch all models in parallel:
Task T007: "Create RecentItem model in WpfApp2/Models/RecentItem.cs"
Task T008: "Create WidgetDefinition model in WpfApp2/Models/WidgetDefinition.cs"

# Launch controls in parallel:
Task T012: "Create WidgetCard control in WpfApp2/Controls/WidgetCard.cs"
Task T013: "Create WidgetCardStyles.xaml in WpfApp2/Controls/WidgetCardStyles.xaml"
```

## Parallel Example: Phase 3 Widget ViewModels

```text
# Launch all 9 widget ViewModels in parallel:
Task T017: "Create VersionWidgetViewModel"
Task T018: "Create UpdatesWidgetViewModel"
Task T019: "Create RecentToolsWidgetViewModel"
Task T020: "Create RecentProjectsWidgetViewModel"
Task T021: "Create DiagnosticsSummaryWidgetViewModel"
Task T022: "Create AIProviderStatusWidgetViewModel"
Task T023: "Create PerformanceSummaryWidgetViewModel"
Task T024: "Create QuickActionsWidgetViewModel"
Task T025: "Create PluginStatusWidgetViewModel"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Home Dashboard)
4. **STOP and VALIDATE**: Launch Shell → Home page renders → All 9 widgets visible → Quick Actions work → Recent Tools navigate
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Home Dashboard) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Diagnostics Page) → Test independently → Deploy/Demo
4. Add US3 (AI Provider Status) → Test independently → Deploy/Demo
5. Add US4 (Plugin Status) → Test independently → Deploy/Demo
6. Polish → Constitution compliance → VSTO test → Build validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Home Dashboard — all widgets + page + navigation)
   - Developer B: US2 (Diagnostics Page — separate page, no conflicts)
3. Once US1 is done:
   - Developer A: US3 (AI Provider enhancements)
   - Developer B: US4 (Plugin Status enhancements)
4. Stories complete and integrate independently

---

## Summary

| Metric | Value |
|--------|-------|
| Total tasks | 42 |
| Phase 1 (Setup) | 3 tasks |
| Phase 2 (Foundational) | 13 tasks |
| Phase 3 (US1 - Home Dashboard) | 15 tasks |
| Phase 4 (US2 - Diagnostics) | 4 tasks |
| Phase 5 (US3 - AI Status) | 2 tasks |
| Phase 6 (US4 - Plugin Status) | 1 task |
| Phase 7 (Polish) | 4 tasks |
| Parallel opportunities | 16 tasks marked [P] |
| MVP scope | US1 (Phase 3) — 31 tasks total (Setup + Foundational + US1) |

### Independent Test Criteria

| Story | Test |
|-------|------|
| US1 | Launch Shell → Home page renders <1s → 9 widgets visible → Quick Actions navigate → Recent Tools navigate |
| US2 | Navigate to Diagnostics → Render mode correct → Memory updates every 5s → Theme change reflects immediately |
| US3 | View AI widget → Online=green, Offline=red → Token usage shown → "No providers" message with settings link |
| US4 | View Plugin widget → Health counts accurate → Click navigates to plugin registry |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All XAML must use `DynamicResource` — no `StaticResource` for themeable properties
- No inline `DropShadowEffect` — use centralized effects from `Effects/Shadows.xaml`
- All new features are Pages within Shell — no standalone windows
