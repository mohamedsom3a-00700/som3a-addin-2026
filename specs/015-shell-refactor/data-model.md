# Data Model: Shell Refactor

**Feature**: 015-shell-refactor
**Date**: 2026-05-26

## Entity: NavigationDestination *(modified)*

Represents a single clickable navigation item in the sidebar.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Key | string | Yes | Unique navigation key (e.g., "planning.boq", "settings.appearance") |
| ItemId | string | Yes | Stable plugin-scoped identifier for duplicate detection |
| Label | string | Yes | Display text shown in the sidebar |
| Icon | string | No | Material Design icon name (e.g., "PackageVariant", "Cog") |
| Category | string | Yes | Category group name — one of: "Planning", "Analysis", "Excel", "AI", "Settings", or "Other" |
| Order | int | Yes | Sort order within the category (lower = first) |
| Priority | int | No | Plugin registration priority (for conflict resolution; higher wins) |
| IsVisible | bool | Yes | Whether the item is shown in the sidebar |
| IsEnabled | bool | Yes | Whether the item is clickable (disabled = dimmed with tooltip if Page unavailable) |
| IsSelected | bool | Yes | Whether this item is the currently active page |

**Validation rules**:
- `Key` must be non-empty and unique across all registrations
- `ItemId` must be unique within a Category (plugin-level uniqueness)
- `Category` must match a known category or "Other"
- `Order` must be ≥ 0
- `Icon` if null defaults to "HelpCircleOutline"

**State transitions**:
- `IsVisible`: true → false when plugin unloaded; false → true when plugin loaded
- `IsEnabled`: true → false when target Page resolution fails; false → true never (requires re-registration)
- `IsSelected`: true → false when user navigates to different item; false → true on click

**Relationships**:
- One NavigationDestination references one NavigationPage (via Key in the registry)
- Many NavigationDestinations belong to one Category group

## Entity: NavigationPage *(modified)*

Internal registry entry that stores page type and factory logic.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Key | string | Yes | Maps to NavigationDestination.Key |
| DisplayName | string | Yes | Human-readable name for error messages and window title |
| Icon | string | No | Icon name |
| Category | string | Yes | Category this page belongs to |
| Order | int | Yes | Sort order |
| PageType | Type | Yes | The `System.Type` of the Page to instantiate |
| IsPluginPage | bool | No | True if registered dynamically from a plugin |

**Methods**:
- `CreateInstance()` → Page: Activator.CreateInstance or DI-resolved instance

## Entity: ShellState *(modified)*

Tracks runtime state of the Shell workspace.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| ActivePageKey | string | No | Key of the currently displayed page |
| LastActivePageKey | string | No | Key of the previously displayed page (for revert on Cancel) |
| SidebarVisible | bool | Yes | Whether the sidebar is visible at all |
| SidebarCollapsed | bool | Yes | Whether the sidebar is in collapsed (icon-strip) state |
| CollapsedCategories | HashSet<string> | No | Set of category names that are currently collapsed (header only) |
| CommandPaletteOpen | bool | Yes | Whether the command palette overlay is open |
| PreviousPageStack | Stack<string> | No | Navigation history (max depth 10) |

**Validation rules**:
- `SidebarCollapsed` implies `SidebarVisible` (collapsed sidebar is still visible as icon strip)
- If `SidebarCollapsed` = true, keyboard focus must not enter sidebar items
- `CollapsedCategories` persists per-session only; resets on restart (all categories expanded)

## Interface: ISupportsDirtyTracking *(new)*

Implemented by pages that have editable state.

| Member | Type | Description |
|--------|------|-------------|
| IsDirty | bool (get) | True if the page has unsaved modifications |
| DirtyChanged | event EventHandler\<bool\> | Raised when IsDirty changes |

## Entity: SidebarCategory *(new, runtime only)*

A named grouping for navigation items. Not persisted — derived from NavigationDestination.Category.

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Category identifier ("Planning", "Analysis", etc.) |
| DisplayName | string | Human-readable category label |
| Order | int | Display order in the sidebar (0 = top) |
| IsCollapsed | bool | Whether this category's items are hidden in the sidebar |
| Items | IEnumerable\<NavigationDestination\> | Navigation items in this category, ordered by Order |

**Predefined categories** (static, built-in):

| Name | DisplayName | Order |
|------|-------------|-------|
| Planning | Planning | 1 |
| Analysis | Analysis | 2 |
| Excel | Excel | 3 |
| AI | AI | 4 |
| Settings | Settings | 5 |
| Other | Other | 99 |

## Entity: PageLoadState *(new, runtime only)*

Tracks the loading state of a page being navigated to.

| Field | Type | Description |
|-------|------|-------------|
| PageKey | string | The Key being loaded |
| Status | enum { Loading, Loaded, Error } | Current load status |
| ErrorMessage | string | Error details if Status = Error |
| StartTime | DateTime | When loading began (for timeout detection) |

**State transitions**:
- Loading → Loaded (page created and displayed successfully)
- Loading → Error (exception or timeout ≥ 5 seconds)
- Error → Loading (user clicks Retry)
