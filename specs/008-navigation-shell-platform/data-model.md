# Data Model: Navigation Shell Platform

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-23

## Entities

### NavigationPage

A feature page hosted inside the shell workspace. Represents the content displayed when a user selects a sidebar item or launches from ribbon.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Key | String | Unique identifier used for navigation requests | Must be unique across all registered pages. Alphanumeric + hyphens. |
| DisplayName | String | Human-readable label shown in sidebar and command palette | Max 50 characters. |
| Icon | String | Icon key or glyph reference for sidebar item | Optional. References existing icon system. |
| Order | Integer | Display order in sidebar (ascending) | Lower values appear first. Must be ≥ 0. |
| PageType | Type | CLR type of the Page subclass | Must inherit from `System.Windows.Controls.Page`. |

**Relationships**:
- A `NavigationPage` is referenced by one or more `NavigationDestination` entries.
- A `NavigationPage` is loaded lazily — instantiated on first navigation request.

**Validation Rules**:
- Duplicate keys MUST be rejected at registration time.
- PageType MUST be a valid `Page` subclass with a parameterless constructor (for lazy loading).
- Order values need not be contiguous but MUST be unique.

---

### NavigationDestination

A sidebar or command palette entry that points to a NavigationPage.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Key | String | Navigation key (matches NavigationPage.Key) | Foreign key to NavigationPage. |
| Label | String | Display text in sidebar or command palette | Max 50 characters. |
| Icon | String | Icon reference | Optional. |
| IsSelected | Boolean | Whether this destination is currently active | Managed by NavigationService. |
| IsVisible | Boolean | Whether this item appears in sidebar | Default: true. Allows runtime filtering. |

**Relationships**:
- References `NavigationPage` via `Key`.
- Collected in an `ObservableCollection<NavigationDestination>` for sidebar binding.

**Validation Rules**:
- Key MUST reference a registered NavigationPage.
- At least one destination MUST have `IsVisible = true` for sidebar to be non-empty.

---

### ShellState

The runtime state of the shell instance.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| ActivePageKey | String | Key of the currently displayed page | Null on first open (welcome page shown). |
| LastActivePageKey | String | Key of the previously displayed page | For back-navigation support. Null if no history. |
| SidebarVisible | Boolean | Whether sidebar is expanded/collapsed | Default: true. |
| CommandPaletteOpen | Boolean | Whether command palette overlay is visible | Only one overlay at a time. |
| PreviousPageStack | Stack<String> | Navigation history stack for back navigation | Max depth: 10. FIFO eviction policy. |

**Lifecycle**:
- Created when shell opens.
- Destroyed when shell closes (session-only; no persistence).
- Reset on Excel session end (add-in unload).

## State Transitions

### Navigation Flow

```text
[Welcome Page] ──→ [Page A] ──→ [Page B] ──→ [Page A]
                      ↑                          │
                      └──────────────────────────┘
                           (sidebar click)
```

### Page Lifecycle

```text
Registered → (first navigation) → Loaded → (navigate away) → Unloaded
                ↓ (error)                           ↓
             Error State ← (retry) → Loaded   (stay in memory or dispose)
```

- Pages MAY be kept in memory or disposed on navigation away (implementation choice; `research.md` recommends keeping alive for performance).
- Error state is an inline UI state within the workspace, not a separate page.
