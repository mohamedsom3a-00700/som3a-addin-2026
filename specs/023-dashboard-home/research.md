# Research: Dashboard & Home (Phase 23)

**Feature**: 023-dashboard-home
**Date**: 2026-05-28
**Status**: Complete

---

## R1: Existing Shell Navigation Integration

### Question
How does the current Shell workspace handle default page display, and what is the best way to replace the WelcomePage with a Home dashboard?

### Findings

**Current architecture**:
- `ShellWindow` sets `WelcomePageType = typeof(WelcomePage)` in its constructor
- `WorkspaceHost.ShowWelcome()` creates an instance of `WelcomePageType` and navigates to it
- `WelcomePage` is registered with key `"welcome"`, category `"Other"`, and `IsVisible = false`
- The sidebar uses `NavigationService.CategoryOrder` to sort categories: Planning (1), Analysis (2), Excel (3), AI (4), Settings (5), Other (99)

**Integration approach**:
- Register `HomePage` with key `"home"` and a new `"Home"` category with order 0 (appears first)
- Modify `ShellWindow` to set `WelcomePageType = typeof(HomePage)` — this is the minimal change
- Alternatively, call `NavigationService.Instance.NavigateTo("home")` in `ShellWindow.Loaded` after checking no pending navigation

**Decision**: Use the `WelcomePageType` approach — it's the existing pattern and requires minimal code change. The `HomePage` replaces `WelcomePage` as the default landing page.

### Alternatives Considered
- **Modify `WorkspaceHost.ShowWelcome()`** — rejected as it couples the host to a specific page type
- **Add a new `DefaultPageKey` property** — rejected as over-engineering when `WelcomePageType` already exists

---

## R2: Widget Data Source Availability

### Question
Which existing services provide the data needed for each of the 9 widgets, and what gaps exist?

### Findings

| Widget | Existing Service | API Available | Gap |
|--------|-----------------|---------------|-----|
| Current Version | `System.Reflection.Assembly` | `GetName().Version` | None — direct API |
| Latest Updates | None | N/A | Need `IChangelogService` to parse CHANGELOG.md |
| Recent Tools | None | N/A | Need `IRecentItemsService` for persistence |
| Recent Projects | None | N/A | Need `IRecentItemsService` for persistence |
| Diagnostics Summary | `IDiagnosticsService` | `CaptureSnapshot()` returns `DiagnosticSnapshot` | None — full data available |
| AI Provider Status | `IAIProvider.HealthCheckAsync()` + `ITokenTracker` | Health check per provider, token totals | None — both registered as singletons |
| Performance Summary | None | N/A | Need `IPerformanceMonitor` for timing |
| Quick Actions | `INavigationService` | `NavigateTo(key)` | None — direct API |
| Plugin Status | `Contracts.IModuleRegistry` | `GetAllModules()`, `GetModulesByState()` | None — full state tracking available |

**Gaps identified**: 3 new services needed (`IChangelogService`, `IRecentItemsService`, `IPerformanceMonitor`).

### Decision
Create 3 lightweight services in WpfApp2/Services/. All are simple implementations:
- `RecentItemsService` — JSON file read/write with `List<RecentItem>`
- `PerformanceMonitor` — `Stopwatch` wrapper with startup timestamp and last navigation duration
- `ChangelogService` — Read CHANGELOG.md, parse first `## [version]` section

### Alternatives Considered
- **Embed changelog as resource** — rejected as it requires rebuild for updates
- **Use Windows MRU (Most Recently Used) list** — rejected as it's shell-level, not app-level
- **Use `System.Diagnostics.PerformanceCounter`** — rejected as over-engineering for simple timing

---

## R3: AI Provider Health Check Strategy

### Question
How should the AI Provider Status widget determine online/offline state without blocking the UI or making excessive API calls?

### Findings

**Existing infrastructure**:
- `IAIProvider.HealthCheckAsync()` sends a minimal "Ping" prompt (MaxTokens=10, Temperature=0)
- 7 providers exist: OpenAI, Claude, DeepSeek, GLM, Kimi, Codex, Ollama
- `OrchestrationEngine` holds references to all registered providers
- `TokenTracker` is a singleton that records usage on every successful AI response

**Health check cost**: Each health check makes an actual API call with token cost. Running health checks on all 7 providers every 5 seconds would be expensive and rate-limited.

**Decision**: 
- Cache health check results with a 30-second TTL (time-to-live)
- Run health checks on a background thread with 30-second interval
- Display "Unknown" state for providers not yet checked
- Show last known state while refresh is in progress
- Use `TokenTracker.TotalTokens` for session token display (no additional API calls needed)

### Alternatives Considered
- **Ping-based health check (no API call)** — rejected as providers don't expose a ping endpoint separate from the API
- **Assume all providers online** — rejected as it defeats the purpose of the widget
- **Check on widget display only** — rejected as it causes delay when viewing the dashboard

---

## R4: Recent Items Persistence Strategy

### Question
What is the best persistence mechanism for Recent Tools and Recent Projects lists?

### Findings

**Existing patterns in the codebase**:
- `sidebar-state.json` at `%AppData%/Som3a/` — stores sidebar expand/collapse state
- `theme.json` at `%AppData%/Som3a/` — stores theme selection
- `Properties.Settings.Default` — stores SelectedTheme and AccentColor

**Data characteristics**:
- Recent Tools: List of 5 items (toolId + timestamp)
- Recent Projects: List of 5 items (filePath + timestamp)
- Total data size: <1KB
- Read on dashboard load, write on tool/project usage
- No concurrent access (single-user desktop app)

**Decision**: JSON file at `%AppData%/Som3a/recent-items.json` using `System.Text.Json`.

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

### Alternatives Considered
- **SQLite database** — rejected as over-engineering for 10 items (Phase 27 will add SQLite later)
- **Windows Registry** — rejected due to permission issues and lack of portability
- **`Properties.Settings.Default`** — rejected as it's strongly-typed and not suitable for dynamic lists
- **In-memory only (no persistence)** — rejected as spec requires persistence across sessions (FR-013)

---

## R5: Performance Monitoring Strategy

### Question
How should startup time and navigation duration be measured without adding overhead?

### Findings

**Startup time**:
- The application startup is handled by `App.xaml.cs` → `CompositionRoot` → `ShellWindow`
- The time from `App.OnStartup` to `ShellWindow.Loaded` is the effective startup time
- This can be measured by recording `DateTime.UtcNow` in `App.OnStartup` and comparing in `ShellWindow.Loaded`

**Navigation duration**:
- `WorkspaceHost.Navigate(Page)` is the single navigation entry point
- It already shows a `ProgressBar` loading indicator during navigation
- Navigation duration = time from `Navigate()` call to `Page.Loaded` event

**Decision**: 
- `PerformanceMonitor` records `AppStartTime` in `App.OnStartup`
- `PerformanceMonitor.CalculateStartupTime()` called in `ShellWindow.Loaded` — computes elapsed time
- `PerformanceMonitor.BeginNavigation()` called at start of `WorkspaceHost.Navigate()`
- `PerformanceMonitor.EndNavigation()` called on `Page.Loaded` — computes elapsed time
- Values stored in-memory (no persistence needed — only current session)

### Alternatives Considered
- **`System.Diagnostics.Stopwatch` per page** — rejected as it requires modifying every page
- **`PerformanceCounter` category** — rejected as over-engineering and requires admin privileges
- **ETW (Event Tracing for Windows)** — rejected as over-engineering for 2 metrics

---

## R6: Widget Layout and Responsive Design

### Question
What WPF layout strategy best supports 9 widgets across different screen sizes and DPI scales?

### Findings

**Target displays**:
- Primary: 1920x1080 (Full HD) — most common for planning engineers
- Secondary: 1366x768 (laptop) — minimum supported
- DPI: 100% to 200% (per constitution §X and existing DPI audit)

**Widget size requirements**:
- Each widget needs approximately 300x200 pixels at 100% DPI
- 3 columns × 3 rows = 9 widgets at 900x600 minimum content area
- Shell sidebar takes 220px, leaving 1700px on 1920-wide display

**Decision**: 
- Use `UniformGrid` with `Columns="3"` and `Rows="3"` as the primary layout
- Each widget card has `MinWidth="280"` and `MinHeight="180"`
- Wrap in `ScrollViewer` for smaller screens (1366x768 at 150% DPI)
- Use `DynamicResource` for all spacing tokens (per constitution §III)

### Alternatives Considered
- **`WrapPanel`** — rejected as it doesn't enforce equal sizing, leading to uneven layouts
- **`Grid` with explicit columns/rows** — rejected as it requires manual column/row assignment for each widget
- **`ItemsControl` with `UniformGrid` ItemsPanel** — selected approach: allows binding to `ObservableCollection<WidgetViewModel>` while maintaining uniform layout

---

## R7: Changelog Parsing Strategy

### Question
How should the Latest Updates widget extract changelog data without adding a markdown parsing library?

### Findings

**Changelog format**: Standard Keep a Changelog format (`## [version] - date` sections with bullet lists)

**Existing dependencies**: The project does not use any markdown parsing library (Markdig, CommonMark, etc.)

**Data needed**: Only the latest version section (version number, date, bullet list of changes)

**Decision**: Simple line-based parser:
1. Read `CHANGELOG.md` from app directory (or embedded resource as fallback)
2. Find first line matching `## [` pattern
3. Extract version and date from that line
4. Collect all subsequent `- ` lines until next `## [` or end of file
5. Return as `ChangelogEntry` (Version, Date, List of changes)

**Fallback**: If `CHANGELOG.md` not found, display "No updates available" message.

### Alternatives Considered
- **Add Markdig NuGet package** — rejected as adding a dependency for a simple parse task
- **Embed changelog as resource** — rejected as it requires rebuild for updates
- **Fetch from GitHub releases API** — rejected as it requires network access and API token
- **Hardcode latest version** — rejected as it requires code change for each release

---

## R8: Real-Time Diagnostics Refresh

### Question
What is the optimal refresh strategy for real-time diagnostics data?

### Findings

**Existing pattern**: `DiagnosticsViewModel` uses `DispatcherTimer` with 5-second interval to call `IDiagnosticsService.CaptureSnapshot()`.

**Data sources and their update frequency**:
- Memory usage: Changes continuously (GC cycles)
- Render mode: Changes rarely (only on window creation/DPI change)
- Theme: Changes on user action (theme switch)
- GPU status: Changes rarely (only on driver issues)
- Error logs: Changes on error occurrence

**Decision**: 
- Use `DispatcherTimer` with 5-second interval (matches existing pattern)
- On each tick: call `CaptureSnapshot()` and update all diagnostic properties
- Subscribe to `ThemeManager.ThemeChanged` event for immediate theme update (don't wait for timer)
- Subscribe to `IDiagnosticsService.SnapshotUpdated` event if available

### Alternatives Considered
- **1-second interval** — rejected as excessive CPU overhead for non-critical data
- **Event-driven only** — rejected as not all data sources fire events (e.g., memory usage)
- **10-second interval** — rejected as too slow for memory monitoring during troubleshooting

---

## Summary of Decisions

| # | Decision | Rationale |
|---|----------|-----------|
| R1 | Replace WelcomePage via `WelcomePageType` | Minimal change, existing pattern |
| R2 | 3 new lightweight services | Fill data gaps for changelog, recent items, performance |
| R3 | Cached health checks with 30s TTL | Balance freshness vs API cost |
| R4 | JSON file at AppData | Matches existing patterns, simple, portable |
| R5 | In-memory timing via PerformanceMonitor | Lightweight, no persistence needed |
| R6 | UniformGrid 3×3 with ScrollViewer fallback | Equal sizing, responsive, DPI-aware |
| R7 | Simple line-based changelog parser | No new dependencies, sufficient for widget needs |
| R8 | 5-second DispatcherTimer | Matches existing pattern, balances freshness and performance |
