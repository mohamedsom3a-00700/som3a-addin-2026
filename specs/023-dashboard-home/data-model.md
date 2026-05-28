# Data Model: Dashboard & Home (Phase 23)

**Feature**: 023-dashboard-home
**Date**: 2026-05-28
**Status**: Complete

---

## Entity Overview

```
RecentItem
  ├── ToolId / FilePath (identity)
  ├── DisplayName
  └── Timestamp

ChangelogEntry
  ├── Version
  ├── Date
  └── Changes (list of strings)

WidgetDefinition
  ├── WidgetType (enum)
  ├── Title
  ├── Icon
  └── DisplayOrder

AIProviderStatusInfo
  ├── ProviderId
  ├── ProviderName
  ├── IsOnline
  ├── LastChecked
  └── SessionTokenUsage

PluginStatusSummary
  ├── TotalCount
  ├── ActiveCount
  ├── FailedCount
  └── DegradedCount

PerformanceMetrics
  ├── StartupTimeMs
  └── LastNavigationTimeMs

RecentItemsStore (persistence root)
  ├── RecentTools (list of RecentItem)
  └── RecentProjects (list of RecentItem)
```

---

## Entity Definitions

### RecentItem

Represents a recently used tool or recently opened project.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| ToolId | string | Nullable, max 100 chars | Navigation key for tools (e.g., `"planning.wbs.generator"`) |
| FilePath | string | Nullable, max 500 chars | Full file path for projects |
| DisplayName | string | Required, max 100 chars | Human-readable name for display |
| Timestamp | DateTime (UTC) | Required | When the item was last used/opened |

**Identity**: `ToolId` for tools, `FilePath` for projects. One must be non-null.

**Validation rules**:
- Exactly one of `ToolId` or `FilePath` must be non-null
- `DisplayName` must not be empty
- `Timestamp` must not be in the future

**Lifecycle**: Created when a tool is used or project opened. Removed when list exceeds 5 items (oldest first).

---

### RecentItemsStore

Persistence root for recent items. Serialized to/from JSON.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| RecentTools | List\<RecentItem\> | Max 5 items | Recently used tools, ordered by timestamp descending |
| RecentProjects | List\<RecentItem\> | Max 5 items | Recently opened projects, ordered by timestamp descending |

**Persistence**: JSON file at `%AppData%/Som3a/recent-items.json`

**JSON schema**:
```json
{
  "recentTools": [
    {
      "toolId": "planning.wbs.generator",
      "displayName": "WBS Generator",
      "timestamp": "2026-05-28T10:30:00Z"
    }
  ],
  "recentProjects": [
    {
      "filePath": "C:\\Projects\\Building-A.xer",
      "displayName": "Building-A.xer",
      "timestamp": "2026-05-28T09:15:00Z"
    }
  ]
}
```

**Validation rules**:
- Lists must not exceed 5 items each
- Items ordered by `Timestamp` descending (most recent first)
- Duplicate `ToolId` or `FilePath` not allowed (update timestamp instead)

---

### ChangelogEntry

Represents a single version entry from the changelog.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| Version | string | Required, max 50 chars | Version string (e.g., "1.3.0") |
| Date | DateTime | Required | Release date |
| Changes | List\<string\> | Required, min 1 item | List of change descriptions |

**Source**: Parsed from `CHANGELOG.md` using line-based parser.

**Validation rules**:
- `Version` must not be empty
- `Changes` must contain at least one entry

---

### WidgetDefinition

Metadata for a dashboard widget. Used for registration and display.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| WidgetType | WidgetType (enum) | Required | Identifies the widget type |
| Title | string | Required, max 50 chars | Display title |
| Icon | string | Required, max 50 chars | Material Design icon name |
| DisplayOrder | int | Required | Position in the grid (0-8) |

**WidgetType enum values**:
| Value | Name | Title | Order |
|-------|------|-------|-------|
| 0 | Version | Current Version | 0 |
| 1 | Updates | Latest Updates | 1 |
| 2 | RecentTools | Recent Tools | 2 |
| 3 | RecentProjects | Recent Projects | 3 |
| 4 | DiagnosticsSummary | Diagnostics | 4 |
| 5 | AIProviderStatus | AI Providers | 5 |
| 6 | PerformanceSummary | Performance | 6 |
| 7 | QuickActions | Quick Actions | 7 |
| 8 | PluginStatus | Plugins | 8 |

---

### AIProviderStatusInfo

Represents the health state of a single AI provider for the dashboard widget.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| ProviderId | string | Required, max 50 chars | Unique provider identifier (e.g., "openai") |
| ProviderName | string | Required, max 100 chars | Display name (e.g., "OpenAI GPT-4") |
| IsOnline | bool | Required | Whether the provider passed the last health check |
| LastChecked | DateTime (UTC) | Required | When the health check was last performed |
| SessionTokenUsage | int | Required, >= 0 | Total tokens used by this provider in the current session |

**Source**: `IAIProvider.HealthCheckAsync()` (cached 30s) + `ITokenTracker.GetUsageByProvider()`

**State transitions**:
```
Unknown → Online (health check passes)
Unknown → Offline (health check fails)
Online → Online (health check passes)
Online → Offline (health check fails)
Offline → Online (health check passes)
Offline → Offline (health check fails)
```

---

### PluginStatusSummary

Aggregated plugin health summary for the dashboard widget.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| TotalCount | int | Required, >= 0 | Total number of registered plugins |
| ActiveCount | int | Required, >= 0 | Number of plugins in Active state |
| FailedCount | int | Required, >= 0 | Number of plugins in Failed state |
| DegradedCount | int | Required, >= 0 | Number of plugins in Degraded state |

**Source**: `Contracts.IModuleRegistry.GetAllModules()` grouped by `ModuleState`

**Health determination**:
- All active → "Healthy" (green)
- Any degraded → "Degraded" (yellow)
- Any failed → "Unhealthy" (red)

---

### PerformanceMetrics

Performance timing data for the Performance Summary widget.

| Attribute | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| StartupTimeMs | double | Required, >= 0 | Time from App.OnStartup to ShellWindow.Loaded |
| LastNavigationTimeMs | double | Required, >= 0 | Duration of the most recent page navigation |
| LastNavigationTarget | string | Nullable, max 100 chars | Page key of the most recent navigation |

**Source**: `IPerformanceMonitor` service

**Lifecycle**: Created at startup, updated on each navigation. In-memory only (not persisted).

---

## Relationships

```
HomeViewModel
  ├── has 9 → WidgetViewModel (1:many, composition)
  │     ├── VersionWidgetViewModel → reads Assembly info
  │     ├── UpdatesWidgetViewModel → reads ChangelogEntry
  │     ├── RecentToolsWidgetViewModel → reads RecentItemsStore.RecentTools
  │     ├── RecentProjectsWidgetViewModel → reads RecentItemsStore.RecentProjects
  │     ├── DiagnosticsSummaryWidgetViewModel → reads DiagnosticSnapshot
  │     ├── AIProviderStatusWidgetViewModel → reads List<AIProviderStatusInfo>
  │     ├── PerformanceSummaryWidgetViewModel → reads PerformanceMetrics
  │     ├── QuickActionsWidgetViewModel → uses INavigationService
  │     └── PluginStatusWidgetViewModel → reads PluginStatusSummary
  │
  └── navigates to → DiagnosticsPage (on Diagnostics widget click)
```

---

## Service Contracts

### IRecentItemsService

```
GetRecentTools() → List<RecentItem>
GetRecentProjects() → List<RecentItem>
AddRecentTool(toolId, displayName) → void
AddRecentProject(filePath, displayName) → void
ClearRecentTools() → void
ClearRecentProjects() → void
```

**Persistence**: Reads/writes `%AppData%/Som3a/recent-items.json`
**Thread safety**: File-level locking via `SemaphoreSlim(1, 1)`

### IPerformanceMonitor

```
StartupTimeMs → double (read-only)
LastNavigationTimeMs → double (read-only)
LastNavigationTarget → string (read-only)
RecordAppStart() → void
BeginNavigation(targetKey) → void
EndNavigation() → void
NavigationCompleted → event (PerformanceMetrics)
```

**Thread safety**: All operations on UI thread (DispatcherTimer)

### IChangelogService

```
GetLatestEntry() → ChangelogEntry
GetAllEntries() → List<ChangelogEntry>
```

**Source**: Reads `CHANGELOG.md` from app directory or embedded resource
**Caching**: Parses once on first call, caches result in memory

---

## State Diagrams

### Widget Loading State

```
Uninitialized → Loading (LoadDataAsync called)
Loading → Loaded (data fetched successfully)
Loading → Error (data fetch failed)
Loaded → Loading (RefreshAsync called)
Error → Loading (RefreshAsync called)
```

Each `WidgetViewModel` exposes:
- `IsLoading` (bool) — true during Loading state
- `ErrorMessage` (string) — non-null during Error state
- `IsLoaded` (bool) — true during Loaded state

### AI Provider Health Check Cache

```
Unknown → Cached(Online) (health check passes, cache for 30s)
Unknown → Cached(Offline) (health check fails, cache for 30s)
Cached(Online) → Expired (30s elapsed)
Cached(Offline) → Expired (30s elapsed)
Expired → Cached(Online) (health check passes)
Expired → Cached(Offline) (health check fails)
```
