# Feature Specification: Legacy Window Migration

**Feature Branch**: `011-legacy-window-migration`

**Created**: 2026-05-25

**Status**: Draft

**Input**: User description: "implementation_plan.md phase 11"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Unified Workspace Navigation (Priority: P1)

Users access application features through a consistent workspace shell with sidebar navigation, enabling seamless movement between features without managing separate windows.

**Why this priority**: The primary value proposition is eliminating window management overhead. Users currently juggle multiple standalone windows, which breaks workflow continuity. This creates a unified workspace that reduces context-switching friction.

**Independent Test**: Can be fully tested by launching the application, verifying the shell loads with sidebar navigation, and navigating to at least one migrated feature page within the shell.

**Acceptance Scenarios**:

1. **Given** the application is launched, **When** the shell loads, **Then** a sidebar navigation panel is visible with all migrated feature entries listed
2. **Given** the shell is loaded, **When** a user clicks a feature entry in the sidebar, **Then** the corresponding Page loads in the workspace area without opening a new window
3. **Given** a Page is displayed in the workspace, **When** the user clicks a different feature entry, **Then** the workspace switches to the new Page while the shell remains stable

---

### User Story 2 - Feature Parity After Migration (Priority: P1)

All functionality available in standalone windows remains fully functional after migration to Pages within the shell.

**Why this priority**: Migration must not degrade user capability. Each migrated window must deliver identical functionality with the same controls, data entry, and output behavior users expect.

**Independent Test**: Can be tested by performing the same operations in a migrated Page that were performed in the original standalone window, comparing results.

**Acceptance Scenarios**:

1. **Given** MainWindow has been migrated to MainPage, **When** a user performs project analysis operations, **Then** results match the original standalone window output exactly
2. **Given** SettingsWindow has been migrated to SettingsPage, **When** a user changes theme or accent color, **Then** the change persists and renders correctly
3. **Given** any feature window is migrated, **When** all native operations (data entry, button clicks, grid interactions) are performed, **Then** all operations produce identical results to the standalone version

---

### User Story 3 - Excel VSTO Host Stability (Priority: P1)

Migrated windows render correctly within the Excel VSTO hosting environment, maintaining the same stability and rendering behavior as the original standalone windows.

**Why this priority**: Excel VSTO stability is the project's primary constraint. Any rendering regression in the host environment blocks the entire migration.

**Independent Test**: Can be tested by launching the migrated application from Excel VSTO, opening each migrated Page, and verifying no black windows, no clipping, no Excel freezing, and proper popup behavior.

**Acceptance Scenarios**:

1. **Given** the add-in is hosted in Excel, **When** a migrated Page opens within the shell, **Then** the window renders without black backgrounds or clipping artifacts
2. **Given** the add-in is hosted in Excel at 125% DPI, **When** a migrated Page displays, **Then** all UI elements scale correctly without overflow or truncation
3. **Given** the add-in is hosted in Excel with FallbackSafe mode active, **When** a migrated Page opens, **Then** safe-mode rendering (no transparency, no fade animations) applies correctly

---

### User Story 4 - Gradual Migration with Rollback Capability (Priority: P2)

Migration proceeds window-by-window, with original standalone XAML preserved until each migration is validated, enabling rollback if issues emerge.

**Why this priority**: Risk mitigation through incremental delivery. If a migrated Page fails validation, the original window remains available while issues are resolved.

**Independent Test**: Can be tested by attempting to launch the original standalone window after its migration is complete but before original XAML removal.

**Acceptance Scenarios**:

1. **Given** MainWindow has been migrated and its Page validated, **When** the ribbon launcher for MainWindow is invoked, **Then** the Page opens in shell OR the original standalone window opens (depending on configuration)
2. **Given** a migration validation fails, **When** the team rolls back the migration, **Then** original standalone XAML files remain intact and functional

---

### User Story 5 - Migration Pattern Documentation (Priority: P3)

The team establishes documented migration patterns that future developers can follow for remaining or new windows, ensuring consistency and reducing onboarding time.

**Why this priority**: Knowledge capture for maintainability. Without documented patterns, each subsequent migration would require relearning the approach.

**Independent Test**: Can be tested by providing the migration pattern documentation to a new team member and having them successfully migrate a minor window without additional guidance.

**Acceptance Scenarios**:

1. **Given** migration patterns are documented, **When** a developer reviews the documentation, **Then** they can understand the migration steps, folder conventions, ribbon update process, and validation checklist
2. **Given** a developer follows the documented pattern, **When** they migrate a standalone window, **Then** the result matches the established pattern for folder structure, naming, and integration

---

### Edge Cases

- What happens when a migrated Page depends on services that are not yet initialized in the shell startup sequence?
- How does the system handle a user attempting to open both a standalone window and its migrated Page counterpart simultaneously?
- What occurs when a migration is in progress (Pages/ created) but not yet validated when a critical Excel VSTO stability issue is discovered in another window?
- How does reduced-motion setting affect shell navigation animations?
- What happens when the shell is closed while a Page is performing a long-running operation?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a Shell with sidebar navigation that hosts all migrated Pages
- **FR-002**: System MUST create a `Pages/` folder hierarchy under the project root for all migrated window content
- **FR-003**: System MUST preserve original standalone `.xaml` files until migration validation passes
- **FR-004**: System MUST update ribbon launcher logic to open Pages within the Shell instead of standalone windows
- **FR-005**: System MUST validate each migrated Page in Excel VSTO host before removing original standalone XAML
- **FR-006**: System MUST maintain all existing functionality in migrated Pages (no feature loss)
- **FR-007**: System MUST support FallbackSafe rendering mode in the Shell and all Pages
- **FR-008**: System MUST document migration patterns for remaining windows
- **FR-009**: System MUST apply DynamicResource theme tokens consistently in all migrated Pages
- **FR-010**: System MUST inherit from ModernWindow base class for Shell window implementation
- **FR-011**: System MUST implement per-window navigation state (last visited Page) within a session
- **FR-012**: System MUST support keyboard navigation through sidebar (Tab, Arrow keys, Enter activation)

### Key Entities

- **Shell**: The unified workspace container window with sidebar navigation and Page content area
- **Page**: A migrated window content unit that lives within the Shell's content area (replaces standalone Window)
- **Sidebar**: The navigation panel listing all available feature Pages
- **Ribbon Launcher**: The ribbon button or control that triggers Page navigation within the Shell
- **Migration Record**: Documentation of each window's migration status, validation results, and original-to-Page mapping

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate to any migrated feature Page within 2 clicks from application launch
- **SC-002**: Shell with sidebar loads within 1 second on standard enterprise PC (Intel i5, 16GB RAM, SSD)
- **SC-003**: Page navigation transitions complete within 200ms (animation) or immediately (reduced-motion enabled)
- **SC-004**: Zero rendering regressions in Excel VSTO host across all migrated Pages (verified at 100%, 125%, 150% DPI)
- **SC-005**: Migration maintains 100% functional parity — all operations produce identical results before and after migration
- **SC-006**: All 13 migrated windows are validated within Phase 11 scope; ToastWindow remains standalone by design (notification popup pattern, not a candidate for Shell hosting)
- **SC-007**: Original standalone XAML files (excluding ToastWindow's standalone file) are removed only after corresponding Pages pass validation
- **SC-008**: Migration patterns are documented with sufficient detail for a new developer to complete a window migration independently

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).
- Shell applies to NEW windows only; existing windows remain standalone until Phase 11 (Constitution — Phase 5 scope restriction override).

## Assumptions

- Users have completed Phase 0–10 prerequisites; the Shell architecture from Phase 5 is stable and available for extension
- Ribbon XML or Ribbon designer is accessible for modifying launcher behavior (assumed to exist in project)
- The most-used windows can be determined via existing usage telemetry, user feedback, or ribbon click frequency data
- All 14 existing windows are candidates for migration; no window is permanently excluded
- Migration validation in Excel VSTO host can be performed manually per window with documented checklist
- Page content extracted from windows will follow the same ViewModel patterns established in Phase 6 (MVVM Cleanup)
- No window requires cross-Page state sharing beyond what the existing service layer provides
- The `Pages/` folder structure mirrors the existing `Views/` folder pattern but contains only migrated Page content