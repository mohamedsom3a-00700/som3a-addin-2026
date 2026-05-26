# Feature Specification: Shell Refactor

**Feature Branch**: `feature/phase-15-shell-refactor`

**Created**: 2026-05-26

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 15 — Complete the migration of all remaining standalone windows to Pages within the unified Shell workspace. Add full sidebar categories and dynamic navigation support."

## Clarifications

### Session 2026-05-26

- Q: Sidebar collapse/resize behavior — should the entire sidebar be collapsible? → A: Collapsible sidebar with auto-hide — sidebar collapses to an icon strip; hovering over it expands it temporarily over the workspace.
- Q: Keyboard navigation for the sidebar — how do keyboard-only users navigate? → A: Full keyboard navigation — Tab into sidebar, arrow keys up/down across items and categories, Enter to select, with a skip link to jump directly to workspace content.
- Q: Page state preservation during navigation — what happens to unsaved changes when navigating away? → A: Warn on unsaved changes — if the current page has unsaved modifications, show a confirmation dialog (Save / Discard / Cancel) before navigating. No dialog appears if nothing was changed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navigate All Features via Sidebar Categories (Priority: P1)

A planning engineer opens the Som3a add-in and needs to access any feature — BOQ analysis, activity generation, settings, or diagnostics. The Shell sidebar presents clearly organized categories (Planning, Analysis, Excel, AI, Settings) with labeled navigation items under each. Clicking any item loads the corresponding Page into the workspace. There are no standalone windows to manage, and the user never needs to hunt through Excel ribbons or separate dialogs to find functionality.

**Why this priority**: This is the core refactor deliverable. If features aren't accessible via the sidebar, the Shell-based platform doesn't work. Every subsequent phase depends on all features being navigable through a single, organized interface.

**Independent Test**: Can be fully tested by launching the add-in in Excel, verifying every feature from the original 14 migrated windows plus any additional pages is reachable by clicking a sidebar item, and confirming no standalone window appears.

**Acceptance Scenarios**:

1. **Given** the add-in is launched in Excel, **When** the user opens the Shell workspace, **Then** the sidebar shows categories Planning, Analysis, Excel, AI, and Settings, each with at least one navigation item.
2. **Given** the sidebar is displayed, **When** the user clicks a navigation item (e.g., "BOQ" under Planning), **Then** the corresponding Page loads in the workspace area within 1 second, replacing any previously displayed content.
3. **Given** any feature Page is displayed, **When** the user clicks a different sidebar item from a different category, **Then** the workspace switches to the new Page without opening any new window.
4. **Given** the user navigates through all sidebar items end-to-end, **When** they click every item across all categories, **Then** each one loads its correct Page and no orphaned or standalone windows appear anywhere.

---

### User Story 2 - Dynamic Navigation via Plugin Registration (Priority: P2)

A developer builds a new plugin (e.g., a Schedule Reviewer) and marks it with a `[NavigationItem]` attribute specifying its category ("AI"), label ("Schedule Reviewer"), icon, and priority order. When the add-in next starts, the sidebar automatically includes this new item in the AI category without any manual sidebar configuration changes. If the plugin is later removed, the item disappears automatically.

**Why this priority**: This enables the plugin ecosystem. Without dynamic registration, every new feature requires hard-coded sidebar updates, which breaks the plugin-first architecture (ADR-002). It's P2 because the sidebar must work first (P1), then become dynamic (P2).

**Independent Test**: Can be tested by creating a minimal test plugin with a `[NavigationItem]` attribute, starting the add-in, and confirming the item appears in the correct sidebar category; then removing the plugin and confirming the item disappears on next launch.

**Acceptance Scenarios**:

1. **Given** a plugin assembly with a `[NavigationItem]` attribute exists in the Plugins directory, **When** the Shell initializes, **Then** the sidebar includes a navigation item under the category and at the position specified by the attribute.
2. **Given** a plugin with a registered navigation item is removed from the Plugins directory, **When** the Shell next initializes, **Then** the corresponding navigation item no longer appears in the sidebar.
3. **Given** multiple plugins register items in the same category, **When** the sidebar renders, **Then** items are ordered by their declared priority within the category.
4. **Given** a plugin declares an invalid or unknown category, **When** the Shell initializes, **Then** the item is placed in a default or uncategorized section and a diagnostic warning is logged, without breaking the rest of the sidebar.

---

### User Story 3 - Clean Workspace Without Legacy Windows (Priority: P3)

A user who previously used the add-in before the Shell refactor opens it after the update. All the features they used — settings, diagnostics, BOQ tools, etc. — are now accessible only through the Shell sidebar. No old-style standalone windows pop up. The workspace feels unified and professional, with a single consistent window frame and navigation paradigm.

**Why this priority**: Cleanup and polish. While critical for long-term maintainability, this is validation and removal work that depends on P1 and P2 being complete first.

**Independent Test**: Can be tested by auditing the codebase for any remaining `Window.Show()` or `Window.ShowDialog()` calls (excluding the Shell's own ModernWindow), verifying no Window classes remain that aren't the Shell or dialogs, and doing a full manual walkthrough of all features.

**Acceptance Scenarios**:

1. **Given** the refactored add-in is running, **When** the user triggers any feature from any entry point (ribbon, sidebar, quick action), **Then** no standalone window is created; all content renders as Pages within the Shell workspace or as dialogs from the Shell.
2. **Given** legacy window classes existed in the codebase, **When** the refactor is complete, **Then** all legacy Window-derived classes that were migrated are removed, and all remaining windows are limited to the Shell (ModernWindow) and approved dialogs.
3. **Given** Phase 11 previously migrated 14 windows, **When** the refactor verification runs, **Then** all 14 migrated windows are confirmed accessible via the Shell sidebar with no regressions.

---

### Edge Cases

- What happens when a navigation item's target Page type cannot be resolved (e.g., missing assembly)? The sidebar should show the item as disabled or dimmed, with a tooltip indicating the page is unavailable, and log a diagnostic warning.
- How does the sidebar behave when no plugins are loaded (fresh install before any plugins exist)? The sidebar should display the built-in categories and items (Settings, Diagnostics) and show empty categories gracefully with a "No items" placeholder.
- What happens when a user rapidly clicks multiple sidebar items in quick succession? Each click should cancel any in-progress page load for the previous item and load the most recently requested Page. No memory leaks or state corruption should occur.
- How does the system handle a navigation item that takes longer than expected to load? A loading indicator should appear in the workspace area, and if the Page fails to load, an error message should display in the workspace with an option to retry.
- What happens if a `[NavigationItem]` attribute has a duplicate ItemId across plugins? The later-registered item should be rejected with a diagnostic warning, and the first registered item should remain active.
- How does the collapsed sidebar behave when a page requires urgent user attention (e.g., validation error on the current page)? The collapsed sidebar must not obscure or interfere with page-level notifications; the icon strip stays at the edge and does not block workspace content.
- How does keyboard focus work when the sidebar is collapsed? Tab key must skip the collapsed icon strip and move directly to workspace content (the skip link is implicit when collapsed). When the sidebar is expanded (via mouse hover), Tab enters the sidebar as normal.
- How does the system handle the user selecting "Cancel" on the unsaved-changes prompt? The navigation is aborted, the user remains on the current page with all unsaved changes intact, and the sidebar selection reverts to the previously active item.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Shell sidebar MUST organize navigation items into five standard categories: Planning, Analysis, Excel, AI, and Settings.
- **FR-002**: Users MUST be able to access every feature of the add-in through the Shell sidebar without any standalone windows.
- **FR-003**: The sidebar MUST support dynamic registration of navigation items via the `[NavigationItem]` attribute from the Plugin SDK (Som3a.Plugin.SDK).
- **FR-004**: Each navigation item MUST display an icon (Material Design icon where available, fallback default icon otherwise), a text label, and belong to a single category.
- **FR-005**: Navigation items within each category MUST be ordered by a numeric priority declared in their `[NavigationItem]` attribute.
- **FR-006**: Clicking a sidebar navigation item MUST load the corresponding Page into the Shell workspace, replacing the previously displayed content.
- **FR-007**: The currently active navigation item MUST be visually highlighted in the sidebar to indicate the user's current location.
- **FR-008**: The system MUST verify that all 14 windows previously migrated in Phase 11 are accessible via the Shell sidebar and that no regressions exist.
- **FR-009**: All legacy standalone Window classes that were migrated to Pages MUST be removed from the codebase (excluding the Shell's ModernWindow and approved modal dialogs).
- **FR-010**: Deprecated navigation paths and orphaned ViewModels/Views that are no longer reachable MUST be cleaned up.
- **FR-011**: The sidebar MUST handle plugins being added or removed between sessions, rebuilding the navigation tree accordingly on each Shell initialization.
- **FR-012**: If a navigation item's target Page fails to load, the system MUST display an error state in the workspace area with a retry option.
- **FR-013**: Navigation between Pages MUST complete in under 1 second under normal conditions to maintain a responsive user experience.
- **FR-014**: The sidebar MUST support a collapsed state where only category icons are visible as a narrow strip; hovering over the icon strip expands the sidebar temporarily over the workspace for navigation, and it auto-collapses when focus leaves the sidebar area.
- **FR-015**: The sidebar MUST support full keyboard navigation: Tab to enter the sidebar, arrow keys (up/down) to move between items and categories, Enter to activate the selected item, and Left/Right to collapse/expand categories. A skip link MUST be available to bypass the sidebar and jump directly to workspace content.
- **FR-016**: When navigating away from a page that has unsaved modifications, the system MUST prompt the user with a confirmation dialog offering Save, Discard, and Cancel options. If no modifications exist, navigation proceeds immediately without any prompt.

### Key Entities

- **Sidebar Category**: A named group of related navigation items (e.g., Planning, Analysis). Categories have a display order and are rendered as collapsible sections in the sidebar.
- **Navigation Item**: A clickable entry in the sidebar that loads a specific Page. Has a unique identifier, display label, icon reference, target Page type, parent category, and priority order. Registered dynamically via `[NavigationItem]` attribute or statically for built-in items.
- **Shell Workspace**: The unified container that hosts the sidebar (which can collapse to an icon strip), the content area where Pages render, and the ModernWindow chrome. The sole top-level window for the add-in.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate from any feature to any other feature in 2 clicks or fewer (select category, select item) without opening or closing any windows.
- **SC-002**: 100% of add-in features are accessible exclusively through the Shell sidebar — zero standalone windows remain.
- **SC-003**: Page transitions between sidebar items complete within 1 second for 95% of navigations under normal operating conditions.
- **SC-004**: The sidebar rebuilds its navigation tree on startup within 500 milliseconds even when 50+ navigation items are registered across multiple plugins.
- **SC-005**: All 14 windows migrated in Phase 11 pass regression testing via the new sidebar navigation — no functionality lost or navigation dead-ends encountered.
- **SC-006**: Adding a new plugin with a `[NavigationItem]` attribute results in that item appearing in the correct sidebar category on the next Shell initialization without any code changes to the Shell itself.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy — no new standalone windows (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Phase 14 (Platform Foundation) has been completed and the Som3a.Plugin.SDK with `[NavigationItem]` attribute is available as specified in the master plan.
- Phase 11 (Legacy Window Migration) successfully migrated 14 windows to Pages and updated their Ribbon launchers. This phase verifies and builds on that work.
- Phase 8 (Navigation Shell Platform) provides the existing Shell, sidebar, and navigation infrastructure that this phase refactors and extends.
- The Shell uses the ModernWindow from the existing theme engine as its window chrome — no new window type is introduced.
- Material Design icons are available via the approved MaterialDesignThemes package (ADR-006).
- All navigation happens within the Excel VSTO process — the Shell is hosted inside Excel, not as a separate process.
- The existing `INavigationService` contract can be extended or wrapped to support the dynamic registration model without breaking existing consumers.
- The five sidebar categories (Planning, Analysis, Excel, AI, Settings) are the initial set; plugins may introduce custom categories in future phases, but this phase supports only the standard five plus a fallback "Other" category.
