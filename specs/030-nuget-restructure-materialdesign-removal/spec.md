# Feature Specification: NuGet Restructure & MaterialDesign Removal

**Feature Branch**: `[030-nuget-restructure-materialdesign-removal]`

**Created**: 2026-05-30

**Status**: Draft

**Input**: User description: "specs\\future-plan-fluent-ui-migration.md PHASE 1C"

## Clarifications

### Session 2026-05-30

- **Q1**: Scope Boundary — Visual Migration vs. Cleanup-Only → **A**: Zero visual changes. Remove old UI library dependencies, fix build errors only, use temporary brush stubs. No icon or control replacement. All visual migration deferred to Phase 3.
- **Q2**: Rollback Strategy — Recovery on Build or Runtime Failure → **A**: Backup branch revert. Maintain a parallel backup branch with the old library intact. If the smoke test fails after removal, revert the feature branch to the backup state and retry before merging to the main development line.
- **Q3**: Degraded State Tolerance — Placeholder Behavior → **A**: Visible neutral placeholders. Missing icons render as empty squares or default glyphs; broken styling tokens fall back to neutral theme-safe colors. The page remains navigable and all input fields and buttons work. Phase 3 fixes all visuals.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fluent UI Foundation (Priority: P1)

As a user, I want the application to transition away from the old UI library so that the interface aligns with the Windows Fluent design language and our custom theme engine, providing a modern, consistent visual experience.

**Why this priority**: This is the foundational dependency migration that enables all subsequent Fluent UI phases. Without removing the old UI library and establishing the new dependency foundation, higher-level UI modernization (icon migration, control replacement, settings redesign) cannot proceed safely.

**Independent Test**: Can be fully tested by verifying the application contains zero references to the old UI library and all existing pages launch, render, and respond to theme switches without crashes or missing controls.

**Acceptance Scenarios**:

1. **Given** the application is installed with the updated dependency foundation, **When** the user launches the shell from Excel, **Then** the window opens, the sidebar renders, and no icons or controls from the old library are visible.
2. **Given** the user navigates to the Settings page, **When** switching between Dark and Light themes, **Then** all controls render correctly using the custom theme engine without visual regressions or missing elements.

---

### User Story 2 - Uninterrupted Feature Availability (Priority: P1)

As a user, I want all existing features to remain fully accessible after the underlying UI library replacement so that my daily workflows (BOQ generation, WBS editing, duration estimation, Primavera integration) are not disrupted.

**Why this priority**: Dependency removal carries inherent risk of breaking existing functionality. Guaranteeing continuity ensures the migration is transparent to end users and does not degrade the current product experience.

**Independent Test**: Can be fully tested by opening each major page (Home, Settings, BOQ Generator, WBS Editor, Duration Estimator, Relationship Generator, Diagnostics) and confirming each loads, displays data, and accepts user input without errors.

**Acceptance Scenarios**:

1. **Given** the user opens the BOQ Activity Generator page, **When** viewing the page controls and layout, **Then** the page loads successfully and all input fields and buttons are functional.
2. **Given** the user is on any existing page, **When** interacting with scrollable content or progress indicators, **Then** the controls respond correctly and no runtime errors occur.

---

### User Story 3 - Excel Integration Stability (Priority: P2)

As a user working within Excel, I want the VSTO add-in interop to remain stable after the package changes so that data can still be written to and read from worksheets reliably.

**Why this priority**: The out-of-process architecture (Phase 1B) relies on clean interop. Removing stale configuration artifacts from the old library prevents communication failures that could break Excel integration.

**Independent Test**: Can be fully tested by triggering an Excel cell write operation from the WPF shell and confirming the value appears in the target workbook without interop errors.

**Acceptance Scenarios**:

1. **Given** the WPF shell is running alongside Excel, **When** the user executes a command that writes data to an Excel cell via the application bridge, **Then** the value is written successfully and no interop errors are raised.
2. **Given** the user closes the Excel workbook, **When** the WPF process detects the Excel session ending, **Then** it shuts down gracefully within the expected timeout window.

---

### Edge Cases

- **Hidden legacy references**: A user interface definition or code file contains an indirect reference to an old library resource key that is not caught by the initial audit. The system must fail the build process with a clear error rather than crashing at runtime.
- **Theme resource missing**: A removed brush or color token causes a control to render with a default system color instead of the intended theme color. The fallback must be a visible neutral theme-safe color, and the control must remain interactive, not crash or disappear.
- **VSTO configuration stale**: The VSTO configuration still contains a reference to the removed old library. The add-in must load without configuration warnings or runtime resolution errors.
- **Smoke test failure after removal**: If any page fails to load or the build breaks after the old library is removed, recovery is performed by reverting the feature branch to the backup branch containing the old library state, then retrying the migration with a corrected reference map. The main development line is never merged with a broken state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The WPF host MUST remove all dependencies on the old UI library.
- **FR-002**: The VSTO add-in MUST remove all dependencies on the old UI library.
- **FR-003**: The WPF host MUST integrate modern icon and control libraries that align with the Fluent design system. No existing page UI elements are replaced with new library controls in this phase; library integration is limited to package availability and compilation compatibility.
- **FR-004**: All theme resource files tied exclusively to the old UI library MUST be removed from the application's theme system.
- **FR-005**: The VSTO add-in configuration MUST be cleaned of any references to the removed old UI library.
- **FR-006**: The application MUST build successfully after the dependency changes with zero residual references to the old UI library in code or markup.
- **FR-007**: All existing pages and controls MUST continue to render and function correctly after the migration. Missing icons or unresolved styling tokens MUST fall back to visible neutral placeholders (empty squares or default glyphs for icons, neutral theme-safe colors for brushes) rather than crashing or hiding elements. Full visual migration is deferred to Phase 3.
- **FR-008**: The custom theme engine MUST remain the primary theming mechanism. If the new control libraries are not fully compatible, the application MUST fall back to existing custom controls without breaking theme switching or causing runtime errors.

### Key Entities *(include if feature involves data)*

- **Theme Resource Catalog**: The aggregated set of resource dictionaries that define colors, brushes, fonts, spacing, and control styles. After this migration, it no longer includes old library resource keys.
- **Icon Mapping Registry**: The conceptual mapping of application icon names to their Fluent equivalents. During this phase, the registry is prepared; full population and replacement occur in a subsequent phase.
- **Dependency Registry**: The list of third-party libraries the application relies on. Captures the before/after state of third-party integrations for audit and verification purposes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The application contains no references to the old UI library in its dependency list or configuration files.
- **SC-002**: Users can launch the application from Excel and open any existing page without crashes, runtime exceptions, or missing UI elements.
- **SC-003**: Users can switch between Dark and Light themes on the Settings page, and all visible controls render correctly without visual regressions within 1 second of the switch.
- **SC-004**: Excel interop operations (cell write via the application bridge) complete successfully with no interop errors.
- **SC-005**: 100% of existing application pages are accessible and functional after the UI library replacement, as verified by the standard VSTO smoke test protocol.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Phase 1A (Framework & Project Format Upgrade) and Phase 1B (Out-of-Process Architecture) are complete and stable; this phase builds directly on their verified state.
- The custom theme engine is capable of supporting the new control library; any incompatibilities will be identified and documented, with a fallback to custom controls if necessary.
- This phase is strictly dependency removal and compilation fix. No intentional visual changes are made. Placeholder icons and temporary styling tokens are acceptable; full icon mapping and control styling refinement are planned for Phase 3.
- If any page renders broken icons or missing controls after removal, this is acceptable as long as the page loads without crashes and functionality remains intact.
- All existing pages reference the old UI library only through direct, named declarations; no dynamic or reflection-based loading of old library types is present.
- The VSTO add-in does not directly host visual elements from the old library; its references are limited to non-visual configuration artifacts.
