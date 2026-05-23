# Feature Specification: Navigation Shell Platform

**Feature Branch**: `008-navigation-shell-platform`

**Created**: 2026-05-23

**Status**: Draft

**Input**: User description: "Phase 5 — Navigation Shell Platform from implementation_plan.md"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Switch Between Feature Pages via Sidebar (Priority: P1)

A user opens a new feature from the Excel ribbon and sees it displayed within the shell workspace. The sidebar shows available navigation destinations. The user clicks a different destination in the sidebar and the workspace content switches to the new page without opening a new window.

**Why this priority**: Sidebar navigation and workspace switching are the core value of the shell. Without this, there is no unified workspace — users would still juggle separate windows.

**Independent Test**: Can be fully tested by launching any two new feature pages from the sidebar and verifying each displays correctly in the workspace area with the sidebar visible.

**Acceptance Scenarios**:

1. **Given** the shell is open with a sidebar listing navigation destinations, **When** the user clicks a destination, **Then** the workspace displays the corresponding page content
2. **Given** a page is displayed in the workspace, **When** the user clicks a different sidebar destination, **Then** the workspace transitions to the new page
3. **Given** the shell opens for the first time, **When** the workspace loads, **Then** a welcome/dashboard page is displayed with available features
4. **Given** the welcome page is displayed, **When** the user clicks a sidebar destination, **Then** the workspace navigates to the selected page

---

### User Story 2 - Launch Shell Page from Excel Ribbon (Priority: P2)

A user clicks a ribbon button in Excel that triggers a new feature. Instead of opening a standalone window, the feature opens as a page inside the shell workspace. The ribbon action identifies the target page and instructs the shell to navigate to it.

**Why this priority**: Integration with the Excel ribbon is the primary entry point for all add-in features. Ensuring ribbon actions route into the shell correctly is essential for adoption.

**Independent Test**: Can be tested by clicking a ribbon button mapped to a shell page and confirming the shell opens (or activates) and navigates to the correct page.

**Acceptance Scenarios**:

1. **Given** the shell is not yet open, **When** the user clicks a ribbon button mapped to a shell page, **Then** the shell opens and displays the requested page
2. **Given** the shell is already open, **When** the user clicks a ribbon button mapped to a different page, **Then** the shell navigates to that page without opening a second window
3. **Given** the ribbon button maps to a standalone-only feature (pre-Phase 11), **When** the user clicks it, **Then** it opens as a standalone window (existing behavior preserved)

---

### User Story 3 - Quick Navigate via Command Palette (Priority: P3)

A power user presses a keyboard shortcut (Ctrl+K) to open the command palette overlay. They type a few characters and see matching navigation destinations. They select one and the shell navigates directly to that page.

**Why this priority**: The command palette provides keyboard-driven power-user navigation but is not required for the shell to deliver value. It can be built after sidebar navigation and ribbon integration are stable.

**Independent Test**: Can be tested by opening the command palette, typing a partial page name, selecting a result, and confirming the workspace navigates to the correct page.

**Acceptance Scenarios**:

1. **Given** the shell is open, **When** the user presses the command palette shortcut, **Then** an overlay appears centered in the workspace with a text input focused
2. **Given** the command palette is open, **When** the user types characters matching a page name, **Then** matching results appear in a list below the input
3. **Given** results are displayed, **When** the user selects one, **Then** the palette closes and the workspace navigates to that page

---

### Edge Cases

- What happens when a shell page is already displayed and the user clicks its sidebar entry again? No-op — nothing happens; the page remains as-is
- How does the shell handle a ribbon action that opens a feature while a modal dialog is open inside a page?
- What happens when all sidebar destinations are removed or none are registered?
- How does the shell behave when Excel is in full-screen mode or the VSTO pane is resized to very small dimensions?
- What happens to in-progress work in a page when the user navigates away via sidebar or command palette?
- What happens when a page fails to load? An error state is shown within the workspace with a retry option — the shell remains usable

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Shell MUST provide a container layout with a sidebar navigation panel, a workspace area for hosting pages, and a status bar
- **FR-002**: Users MUST be able to select navigation items from the sidebar to switch the content displayed in the workspace
- **FR-003**: Sidebar MUST visually indicate which navigation item corresponds to the currently displayed page
- **FR-004**: Sidebar MUST support full keyboard navigation — Tab enters/leaves the sidebar, arrow keys move between items, Enter/Space activates, Home/End jumps to first/last item
- **FR-005**: Navigation items in the sidebar MUST be dynamically registered — adding a new page should not require editing the sidebar markup
- **FR-006**: System MUST provide a command palette that allows keyboard-driven navigation to any registered page
- **FR-007**: Command palette MUST filter results as the user types, showing only matching navigation destinations
- **FR-008**: New feature windows MUST be convertible to pages that can be hosted inside the shell workspace without modification to the shell itself
- **FR-009**: Excel ribbon actions MUST be able to launch a specific page inside the shell, with the shell opening automatically if not already displayed
- **FR-010**: Shell MUST support navigation transitions that provide visual feedback when switching pages
- **FR-011**: Shell MUST restore the last active page on next open within the same Excel session
- **FR-012**: Shell MUST display a welcome/dashboard page in the workspace when no feature page is selected or when the shell first opens
- **FR-013**: Existing standalone windows (pre-Phase 5) MUST continue to open as separate windows — the shell does not host them

### Key Entities

- **NavigationPage**: A feature page hosted inside the shell workspace. Represents the content displayed when a user selects a sidebar item or launches from ribbon. Key attributes: unique identifier, display name, icon, order priority, and whether it can be displayed in the shell.
- **NavigationDestination**: A sidebar or command palette entry that points to a NavigationPage. Contains the label, icon, and the target page identifier that the workspace should load.
- **ShellState**: The runtime state of the shell including which page is currently active, which page was last active (for restore), sidebar visibility, and command palette open/closed state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate between any two pages via the sidebar in under 2 seconds from click to fully rendered content
- **SC-002**: New features can be added as shell pages by a developer in under 30 minutes, requiring only page registration and no shell modifications
- **SC-003**: The shell opens and displays the requested page within 1.5 seconds from ribbon button click
- **SC-004**: Command palette displays matching results within 500ms of the user stopping typing
- **SC-005**: 100% of Excel ribbon actions for new features route through the shell without standalone window fallback
- **SC-006**: Shell workspace correctly applies the current theme and updates when the theme changes without page reload
- **SC-007**: Sidebar navigation requires at most 2 clicks to reach any registered page from any starting page
- **SC-008**: Sidebar maintains responsive scrolling performance with up to 25 registered navigation items

## Clarifications

### Session 2026-05-23

- Q: What should the workspace display when no page is selected? → A: Show a welcome/dashboard page with available features
- Q: What happens when a page fails to load? → A: Show an error state within the workspace with a retry option
- Q: Should sidebar support full keyboard navigation? → A: Yes — arrow keys, Tab, Enter/Space, Home/End
- Q: What is the expected maximum number of navigation destinations? → A: Up to 25
- Q: What happens when clicking an already-active sidebar item? → A: No-op — nothing happens

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The shell applies to NEW feature windows only. Existing windows (MainWindow, SettingsWindow, Float_path, etc.) remain standalone until Phase 11.
- No existing windows are converted to pages in this phase — the shell framework and page infrastructure are the deliverable.
- The existing ModernWindow, ThemeManager, and Theme system are fully functional and will be used by the shell container.
- Navigation pages are loaded on demand — the shell does not pre-load all pages at startup.
- The command palette indexes all registered navigation destinations; no full-text search of page content is required.
- Workspace restore lasts only for the current Excel session (no persistence between sessions in this phase).
- The ribbon integration assumes ribbon buttons are already configured in the add-in and only need to be mapped to shell navigation.
