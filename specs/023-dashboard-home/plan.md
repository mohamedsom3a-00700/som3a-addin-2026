# Implementation Plan: Dashboard & Home (Phase 23)

**Feature Branch**: `023-dashboard-home`
**Created**: 2026-05-28
**Status**: Planning Complete
**Spec**: [spec.md](./spec.md)
**Research**: [research.md](./research.md)
**Data Model**: [data-model.md](./data-model.md)

---

## Overview

Implement the Home dashboard as the default landing page for the Shell workspace. The dashboard displays 9 fixed widgets providing platform status, recent activity, and quick actions. Also implement a dedicated Diagnostics page with real-time system metrics.

**Goal**: Replace the placeholder `WelcomePage` with a functional dashboard that gives planning engineers immediate visibility into platform health, AI provider status, plugin state, and quick access to recent tools and projects.

---

## Architecture Decisions

### 1. Page Structure

**Decision**: Create two new pages:
- `HomePage` — replaces `WelcomePage` as the default landing page
- `DiagnosticsPage` — detailed diagnostics view accessible from Home widget

**Rationale**: Separates overview (Home) from deep-dive (Diagnostics). Home remains lightweight; Diagnostics can use heavier real-time updates.

**Alternatives considered**:
- Single page with expandable sections — rejected due to performance concerns with 9 widgets + real-time diagnostics on one page
- Tab-based layout — rejected as it hides information requiring scrolling

### 2. Widget Architecture

**Decision**: Composite ViewModel pattern with reusable `WidgetCard` control.

**Structure**:
```
HomeViewModel
  └─ ObservableCollection<WidgetViewModel> Widgets
       ├─ VersionWidgetViewModel
       ├─ UpdatesWidgetViewModel
       ├─ RecentToolsWidgetViewModel
       ├─ RecentProjectsWidgetViewModel
       ├─ DiagnosticsSummaryWidgetViewModel
       ├─ AIProviderStatusWidgetViewModel
       ├─ PerformanceSummaryWidgetViewModel
       ├─ QuickActionsWidgetViewModel
       └─ PluginStatusWidgetViewModel
```

**Rationale**: Each widget has independent data sources and refresh logic. Composite pattern allows:
- Independent testing of each widget
- Graceful degradation (one widget failure doesn't block others)
- Future customization (hide/show widgets) without refactoring

**Alternatives considered**:
- Single monolithic ViewModel — rejected due to complexity and tight coupling
- UserControl per widget — rejected as over-engineering for simple data display

### 3. Data Source Integration

**Decision**: Leverage existing services via constructor injection:

| Widget | Service | Data |
|--------|---------|------|
| Version | `Assembly.GetExecutingAssembly()` | App version, .NET version |
| Updates | `IFileSystemService` | Read `CHANGELOG.md` from app directory |
| Recent Tools | `IRecentItemsService` (new) | JSON file in AppData |
| Recent Projects | `IRecentItemsService` (new) | JSON file in AppData |
| Diagnostics Summary | `IDiagnosticsService` | `DiagnosticSnapshot` |
| AI Provider Status | `IAIOrchestrator`, `ITokenTracker` | Provider health, token usage |
| Performance Summary | `IPerformanceMonitor` (new) | Startup time, navigation time |
| Quick Actions | `INavigationService` | Navigate to pages |
| Plugin Status | `IModuleRegistry` | Module states |

**Rationale**: Reuses existing infrastructure. Only 2 new services needed (`IRecentItemsService`, `IPerformanceMonitor`).

### 4. Real-Time Updates

**Decision**: Use `DispatcherTimer` for periodic refresh (5-second interval) on Diagnostics widgets.

**Rationale**: 
- Matches existing `DiagnosticsViewModel` pattern
- Avoids complex event-driven updates for non-critical data
- 5-second interval balances freshness with performance

**Alternatives considered**:
- Event-driven updates via `EventBus` — rejected as over-engineering; diagnostics don't need sub-second latency
- Continuous polling (1-second) — rejected due to CPU overhead

### 5. Layout Strategy

**Decision**: Fixed 3-column grid layout with responsive wrapping.

**Structure**:
```
Row 0: [Version] [Updates] [Quick Actions]
Row 1: [Recent Tools] [Recent Projects] [AI Status]
Row 2: [Diagnostics Summary] [Performance] [Plugin Status]
```

**Rationale**: 
- Fixed layout (per spec clarification) eliminates customization complexity
- 3-column grid fits 1920x1080 displays without scrolling
- `WrapPanel` fallback for smaller screens

**Alternatives considered**:
- UniformGrid — rejected as it forces equal sizing for widgets with different content density
- StackPanel — rejected as it requires vertical scrolling on 1080p displays

### 6. Navigation Integration

**Decision**: 
- Register `HomePage` with key `"home"` and category `"Home"` (new category, order 0)
- Register `DiagnosticsPage` with key `"diagnostics"` and category `"Settings"` (order 99)
- Modify `ShellWindow` to navigate to `"home"` instead of showing `WelcomePage`

**Rationale**: 
- `"Home"` category appears first in sidebar (order 0)
- Diagnostics under Settings groups related configuration tools
- Replaces `WelcomePage` cleanly without breaking existing navigation

### 7. Persistence Strategy

**Decision**: JSON file at `%AppData%/Som3a/recent-items.json` for Recent Tools and Recent Projects.

**Schema**:
```json
{
  "recentTools": [
    { "toolId": "planning.wbs.generator", "timestamp": "2026-05-28T10:30:00Z" }
  ],
  "recentProjects": [
    { "filePath": "C:\\Projects\\Building-A.xer", "timestamp": "2026-05-28T09:15:00Z" }
  ]
}
```

**Rationale**: 
- Matches existing `sidebar-state.json` pattern
- Simple, human-readable, easy to debug
- No database overhead for simple list

**Alternatives considered**:
- SQLite database — rejected as over-engineering for 2 lists of 5 items
- Windows Registry — rejected due to permission issues and lack of portability

---

## File Structure

### New Files

```
WpfApp2/
├── Pages/
│   ├── HomePage.xaml                          # Home dashboard page
│   ├── HomePage.xaml.cs                       # Code-behind (minimal)
│   ├── DiagnosticsPage.xaml                   # Detailed diagnostics page
│   └── DiagnosticsPage.xaml.cs                # Code-behind (minimal)
├── ViewModels/
│   ├── HomeViewModel.cs                       # Main dashboard ViewModel
│   ├── Dashboard/
│   │   ├── WidgetViewModel.cs                 # Base widget ViewModel
│   │   ├── VersionWidgetViewModel.cs          # Version info widget
│   │   ├── UpdatesWidgetViewModel.cs          # Changelog widget
│   │   ├── RecentToolsWidgetViewModel.cs      # Recent tools widget
│   │   ├── RecentProjectsWidgetViewModel.cs   # Recent projects widget
│   │   ├── DiagnosticsSummaryWidgetViewModel.cs  # Diagnostics summary
│   │   ├── AIProviderStatusWidgetViewModel.cs # AI provider status
│   │   ├── PerformanceSummaryWidgetViewModel.cs  # Performance metrics
│   │   ├── QuickActionsWidgetViewModel.cs     # Quick action buttons
│   │   └── PluginStatusWidgetViewModel.cs     # Plugin health status
│   └── DashboardViewModel.cs                  # Detailed diagnostics ViewModel
├── Controls/
│   ├── WidgetCard.cs                          # Reusable widget container
│   └── WidgetCardStyles.xaml                  # Widget card styles
├── Services/
│   ├── RecentItemsService.cs                  # Recent tools/projects persistence
│   ├── PerformanceMonitor.cs                  # Startup/navigation timing
│   └── ChangelogService.cs                    # Parse CHANGELOG.md
└── Models/
    ├── RecentItem.cs                         # Recent item data model
    └── WidgetDefinition.cs                    # Widget metadata

Som3a.Contracts/
├── IRecentItemsService.cs                     # Recent items service contract
├── IPerformanceMonitor.cs                     # Performance monitor contract
└── IChangelogService.cs                       # Changelog service contract
```

### Modified Files

```
WpfApp2/
├── Controls/Shell/
│   └── ShellWindow.xaml.cs                    # Navigate to HomePage instead of WelcomePage
├── Services/
│   ├── NavigationService.cs                   # Add "Home" category (order 0)
│   └── SidebarRegistrationService.cs          # Register HomePage and DiagnosticsPage
└── CompositionRoot.cs                         # Register new services

WpfApp2/Theme/
└── ThemeResources.xaml                        # Merge WidgetCardStyles.xaml
```

---

## Implementation Tasks

### Phase 1: Foundation (Tasks 1-5)

**T1: Create data models and contracts**
- `Som3a.Contracts/IRecentItemsService.cs`
- `Som3a.Contracts/IPerformanceMonitor.cs`
- `Som3a.Contracts/IChangelogService.cs`
- `WpfApp2/Models/RecentItem.cs`
- `WpfApp2/Models/WidgetDefinition.cs`

**T2: Implement core services**
- `WpfApp2/Services/RecentItemsService.cs` — JSON persistence, add/get/clear methods
- `WpfApp2/Services/PerformanceMonitor.cs` — Track startup time, navigation duration
- `WpfApp2/Services/ChangelogService.cs` — Parse markdown, extract latest version

**T3: Create WidgetCard control**
- `WpfApp2/Controls/WidgetCard.cs` — Custom control with Title, Icon, Content properties
- `WpfApp2/Controls/WidgetCardStyles.xaml` — Styles using DynamicResource tokens
- Merge into `ThemeResources.xaml`

**T4: Implement base WidgetViewModel**
- `WpfApp2/ViewModels/Dashboard/WidgetViewModel.cs`
- Properties: `Title`, `Icon`, `IsLoading`, `ErrorMessage`
- Methods: `LoadDataAsync()`, `RefreshAsync()`

**T5: Register services in CompositionRoot**
- Register `IRecentItemsService`, `IPerformanceMonitor`, `IChangelogService` as singletons
- Verify dependency injection works

### Phase 2: Widget ViewModels (Tasks 6-14)

**T6: VersionWidgetViewModel**
- Read assembly version, .NET version, Windows version
- Static data (no refresh needed)

**T7: UpdatesWidgetViewModel**
- Inject `IChangelogService`
- Display latest version entry from CHANGELOG.md
- Handle missing file gracefully

**T8: RecentToolsWidgetViewModel**
- Inject `IRecentItemsService`, `INavigationService`
- Display last 5 tools with click-to-navigate
- Track tool usage via `NavigationService.NavigationChanged` event

**T9: RecentProjectsWidgetViewModel**
- Inject `IRecentItemsService`
- Display last 5 projects with file path and timestamp
- Open project on click (future enhancement)

**T10: DiagnosticsSummaryWidgetViewModel**
- Inject `IDiagnosticsService`
- Display render mode, theme, memory usage
- Refresh every 5 seconds via `DispatcherTimer`

**T11: AIProviderStatusWidgetViewModel**
- Inject `IAIOrchestrator`, `ITokenTracker`
- Display provider health (green/red indicator)
- Display session token usage
- Refresh every 10 seconds

**T12: PerformanceSummaryWidgetViewModel**
- Inject `IPerformanceMonitor`
- Display startup time, last navigation duration
- Update on navigation events

**T13: QuickActionsWidgetViewModel**
- Inject `INavigationService`
- Display 3 buttons: "New WBS", "Import XER", "Open Settings"
- Navigate on click

**T14: PluginStatusWidgetViewModel**
- Inject `IModuleRegistry`
- Display total plugins, healthy count, error count
- Color-code based on health status
- Refresh on `ModuleStateChanged` event

### Phase 3: Home Page (Tasks 15-17)

**T15: Create HomeViewModel**
- Inject all 9 widget ViewModels
- Expose `ObservableCollection<WidgetViewModel> Widgets`
- Implement `LoadAsync()` to initialize all widgets in parallel

**T16: Create HomePage XAML**
- 3-column grid layout with `WrapPanel` fallback
- Bind to `HomeViewModel.Widgets`
- Use `WidgetCard` control for each widget
- Apply DynamicResource styles

**T17: Implement HomePage code-behind**
- Call `HomeViewModel.LoadAsync()` on `Page.Loaded`
- Handle navigation cleanup on `Page.Unloaded`

### Phase 4: Diagnostics Page (Tasks 18-20)

**T18: Create DiagnosticsViewModel**
- Extend existing `DiagnosticsViewModel` pattern
- Add detailed sections: Render Mode, Theme, DPI, Memory, GPU, Popups, Logs
- Real-time refresh every 5 seconds

**T19: Create DiagnosticsPage XAML**
- Vertical stack of detailed diagnostic cards
- Use existing `DiagnosticsViewModel` structure as reference
- Add "Copy to Clipboard" button for support tickets

**T20: Implement DiagnosticsPage code-behind**
- Start/stop refresh timer on page load/unload
- Handle clipboard copy command

### Phase 5: Navigation Integration (Tasks 21-23)

**T21: Register pages in SidebarRegistrationService**
- Add `HomePage` to "Home" category (order 0)
- Add `DiagnosticsPage` to "Settings" category (order 99)

**T22: Update NavigationService**
- Add "Home" to `CategoryOrder` dictionary with order 0
- Ensure "Home" appears first in sidebar

**T23: Modify ShellWindow**
- Replace `ShowWelcomePage()` with `NavigateTo("home")`
- Update `WelcomePageType` to `typeof(HomePage)`
- Test default landing page behavior

### Phase 6: Testing & Polish (Tasks 24-26)

**T24: Unit tests**
- Test each widget ViewModel in isolation
- Mock services, verify data binding
- Test error handling (service unavailable)

**T25: Integration tests**
- Test HomePage loads all 9 widgets
- Test navigation from Quick Actions and Recent Tools
- Test Diagnostics page real-time updates

**T26: Excel VSTO host test**
- Verify HomePage renders in FallbackSafe mode
- Verify no GPU-dependent effects break rendering
- Verify performance (<1s load time)

---

## Dependencies

### External Dependencies (Existing)

- `IDiagnosticsService` — Provides `DiagnosticSnapshot` for diagnostics widgets
- `IAIOrchestrator` — Provides provider health status
- `ITokenTracker` — Provides session token usage
- `IModuleRegistry` — Provides plugin/module state
- `INavigationService` — Provides navigation and page registration
- `IEventBus` — Provides event subscriptions (optional)

### New Services (Phase 1)

- `IRecentItemsService` — Recent tools/projects persistence
- `IPerformanceMonitor` — Startup and navigation timing
- `IChangelogService` — Parse CHANGELOG.md

### Build Dependencies

- .NET Framework 4.8 (WpfApp2)
- .NET 8.0 (Som3a.Contracts)
- WPF (PresentationFramework, PresentationCore)

---

## Testing Strategy

### Unit Tests

**Scope**: Each widget ViewModel, each new service

**Framework**: xUnit + Moq (matches existing test projects)

**Key test cases**:
- `RecentItemsService` — Add, get, persist, reload
- `PerformanceMonitor` — Track startup, track navigation
- `ChangelogService` — Parse valid markdown, handle missing file
- `VersionWidgetViewModel` — Display correct versions
- `AIProviderStatusWidgetViewModel` — Handle provider offline, handle no providers
- `PluginStatusWidgetViewModel` — Display healthy/degraded/error counts

### Integration Tests

**Scope**: HomePage loads all widgets, navigation works

**Key test cases**:
- HomePage initializes all 9 widgets without errors
- Click Quick Action navigates to correct page
- Click Recent Tool navigates to correct page
- Diagnostics page updates every 5 seconds
- Widget graceful degradation when service unavailable

### Excel VSTO Host Tests

**Scope**: Rendering safety, performance

**Key test cases**:
- HomePage renders in FallbackSafe mode without black screen
- No inline `DropShadowEffect` violations
- HomePage loads in <1 second
- Diagnostics page refresh doesn't cause flicker
- All widgets use `DynamicResource` (no hardcoded colors)

---

## Risk Mitigation

### Risk 1: Service Unavailability

**Mitigation**: Each widget ViewModel implements graceful degradation:
- `IsLoading` property shows spinner during data fetch
- `ErrorMessage` property shows user-friendly message on failure
- Widget remains visible but displays "Data unavailable"

### Risk 2: Performance Overhead

**Mitigation**:
- Parallel widget initialization in `HomeViewModel.LoadAsync()`
- Lazy-load widget data (only when widget is visible)
- 5-second refresh interval (not continuous)
- Dispose timers on page unload

### Risk 3: Excel Rendering Issues

**Mitigation**:
- Use only `DynamicResource` for colors/brushes
- No `DropShadowEffect` on widget cards (use border instead)
- Test in FallbackSafe mode early in development
- Follow existing `WidgetCardStyles.xaml` pattern

### Risk 4: Navigation Conflicts

**Mitigation**:
- Register HomePage before modifying ShellWindow
- Test navigation in isolation before integration
- Keep `WelcomePage` as fallback during development
- Use `NavigationService.NavigateTo("home")` instead of direct page instantiation

---

## Acceptance Criteria

From [spec.md](./spec.md):

- [ ] Home page renders on shell open
- [ ] All 9 widget types functional
- [ ] Diagnostics dashboard updates in real-time
- [ ] Quick actions launch correctly
- [ ] Plugin status reflects loaded/healthy/unhealthy
- [ ] Build passes
- [ ] Excel VSTO host test passes

---

## Next Steps

1. Review this plan with stakeholders
2. Generate tasks.md using `/speckit.tasks`
3. Begin implementation with Task 1 (data models and contracts)

---

## References

- [Spec](./spec.md) — Feature specification
- [Research](./research.md) — Technology research and decisions
- [Data Model](./data-model.md) — Entity definitions and relationships
- [Constitution](../../.specify/memory/constitution.md) — Architectural constraints
