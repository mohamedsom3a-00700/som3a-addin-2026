# Feature Specification: Diagnostics & Stability Platform

**Feature Branch**: `feature/phase-08-diagnostics`

**Created**: 2026-05-25

**Status**: Draft

**Input**: User description: "implementation_plan.md phase 8 - Diagnostics & Stability Platform for preventing WPF/Excel rendering failures"

## Clarifications

### Session 2026-05-25

- Q: Should explicit out-of-scope boundaries be defined? → A: Yes, define specific out-of-scope items now. Added above as "Out of Scope (explicit)" section.
- Q: What is the log retention policy? → A: Auto-rotate logs with a configurable size limit. Logs roll over automatically when the size limit is reached; oldest entries are discarded.
- Q: What interactivity level should the diagnostics panel support? → A: Read-only display plus manual validation trigger only. No recovery actions or administrative controls in scope.
- Q: What happens when validation is triggered while already running? → A: The "Run Validation" button is disabled while a scan is active. No queuing, cancelling, or debouncing needed.
- Q: What log format should be used? → A: Plain text with structured fields. Each log entry contains a timestamp, severity level, category, and description message on a single line.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Support Staff Diagnose Rendering Issues (Priority: P1)

Support staff and power users can open a diagnostics panel to view the current rendering state of the application, including render mode, GPU availability, active theme, and memory usage. This enables rapid diagnosis of rendering problems without requiring developer tools.

**Why this priority**: Rendering failures (black windows, missing controls, popup clipping) are the most visible and disruptive issues in Excel VSTO hosting. Without diagnostics, support staff must guess at the root cause.

**Independent Test**: Can be fully tested by opening the diagnostics panel in all three render scenarios (FallbackSafe, WindowChrome, unknown host) and confirming each displays accurate information.

**Acceptance Scenarios**:

1. **Given** the application is running inside Excel, **When** a user opens the diagnostics panel, **Then** the current render mode (FallbackSafe or WindowChrome) is displayed
2. **Given** the diagnostics panel is open, **When** the user switches themes, **Then** the active theme name updates in real-time
3. **Given** the diagnostics panel is open, **When** memory usage changes, **Then** the displayed memory value refreshes within 5 seconds

---

### User Story 2 - Developers Validate Resource Integrity (Priority: P1)

Developers and support staff can run a validation scan that checks all loaded resource dictionaries for missing tokens, inline colors, invalid resources, and duplicate styles. Results are displayed in the diagnostics panel with actionable information.

**Why this priority**: Inline colors, missing tokens, and duplicate styles are common sources of rendering artifacts and theme breakage. Early detection prevents issues reaching end users.

**Independent Test**: Can be fully tested by introducing a known violation (e.g., hardcoding a color in a control template), running validation, and confirming the violation is reported.

**Acceptance Scenarios**:

1. **Given** the diagnostics panel is open, **When** a user clicks "Run Validation", **Then** all loaded resource dictionaries are scanned within 3 seconds
2. **Given** a resource dictionary contains an inline color value, **When** validation completes, **Then** the inline color is reported with its file location and recommended token replacement
3. **Given** a resource dictionary has a duplicate style definition, **When** validation completes, **Then** the duplicate is reported with file locations

---

### User Story 3 - Application Recovers Gracefully from Resource Failures (Priority: P2)

When a theme resource or resource dictionary fails to load (corrupted, missing, or invalid), the application falls back to a hardcoded safe default theme instead of crashing or displaying a black window.

**Why this priority**: Unrecoverable resource failures currently result in black windows or application crashes, which block users from completing their work in Excel.

**Independent Test**: Can be fully tested by deliberately corrupting a resource dictionary file, restarting the application, and confirming the application loads with the fallback theme instead of crashing.

**Acceptance Scenarios**:

1. **Given** a theme resource dictionary is corrupted, **When** the application starts, **Then** it loads with a hardcoded safe default theme and logs the failure
2. **Given** a resource failure occurred during startup, **When** the application is running, **Then** a notification is displayed informing the user that a fallback theme is active
3. **Given** the application is running under FallbackSafe mode, **When** a popup is opened, **Then** it renders correctly without transparency artifacts or clipping

---

### User Story 4 - Support Staff Review Application Logs (Priority: P3)

Support staff can access application logs that record theme switches, resource failures, render mode changes, and validation results. Logs are persisted to disk for post-mortem analysis.

**Why this priority**: Post-mortem analysis of rendering issues requires historical data that cannot be captured by live diagnostics alone.

**Independent Test**: Can be fully tested by performing actions (theme switch, validation run), navigating to the log viewer, and confirming actions are recorded with timestamps.

**Acceptance Scenarios**:

1. **Given** the user switches themes, **When** the log viewer is opened, **Then** the theme switch event is recorded with timestamp and theme name
2. **Given** a validation scan detects violations, **When** the log viewer is opened, **Then** each violation is recorded with severity, location, and description
3. **Given** a resource failure occurs, **When** the log file is inspected, **Then** the failure details are written to disk within 5 seconds

---

### Edge Cases

- What happens when the diagnostics panel is opened but no render mode service is available?
- How does the system handle corrupted log files (partial writes, disk full)?
- What happens when validation encounters a resource dictionary that cannot be read?
- How does crash-safe loading behave when ALL theme dictionaries are missing?
- What does the diagnostics panel show when memory usage cannot be read?
- How does the application behave when log file creation fails (permissions, disk full)?

### Out of Scope (explicit)

The following are explicitly NOT in scope for this feature:

- **Automated crash telemetry**: Sending diagnostics data to remote servers or external systems
- **Automated issue repair**: Automatically fixing validation violations or resource failures (detection and reporting only)
- **General performance profiling**: CPU profiling, frame rate analysis, or benchmark tools
- **Real-time memory leak detection**: Continuous memory growth tracking (snapshot-based estimation only)
- **Remote log export**: Sending or streaming logs over a network
- **Search/filter capabilities in log viewer**: Raw chronological access to log files only; advanced filtering is deferred

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a diagnostics panel accessible from the Settings window that displays current render mode, GPU availability, active theme name, and estimated memory usage
- **FR-002**: The diagnostics panel MUST update displayed values in real-time or within 5 seconds of state changes
- **FR-003**: Users MUST be able to manually trigger a resource validation scan from the diagnostics panel; the trigger control MUST be disabled while a scan is in progress
- **FR-004**: The validation system MUST scan all loaded resource dictionaries and report missing DynamicResource keys, hardcoded color values, duplicate style definitions, and unresolvable resource references
- **FR-005**: Each validation finding MUST include severity level (error/warning/info), file location, and a description of the issue
- **FR-006**: The system MUST provide a crash-safe loading mechanism that catches resource dictionary load failures and falls back to a hardcoded safe theme dictionary
- **FR-007**: When fallback mode is active, the system MUST display a user-visible notification indicating that a fallback theme is in use
- **FR-008**: The system MUST log significant events including theme switches, resource failures, render mode changes, and validation scan results
- **FR-009**: Logs MUST be persisted to disk and accessible from a log viewer within the diagnostics panel
- **FR-010**: Log files MUST use a plain text format with structured fields; each entry MUST include a timestamp, severity level, event category, and description, all on a single line
- **FR-011**: The system MUST detect when popup controls (ComboBox, context menus) render with transparency issues or clipping and report findings
- **FR-012**: The diagnostics panel MUST be renderable in both FallbackSafe and WindowChrome modes without causing rendering failures

### Key Entities *(include if feature involves data)*

- **DiagnosticSnapshot**: A point-in-time capture of render mode, theme, GPU state, and memory usage displayed in the diagnostics panel
- **ValidationResult**: A single finding from resource validation, containing severity, category, file location, description, and suggested remediation
- **LogEntry**: A timestamped record of a system event, including category, severity, source, and message text
- **FallbackManifest**: The set of resource dictionaries and settings used when primary theme resources fail to load

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Support staff can identify the current render mode, theme, and GPU state within 2 clicks from the Settings window
- **SC-002**: A full resource validation scan completes within 3 seconds for the current set of loaded resource dictionaries
- **SC-003**: Application startup does NOT fail when any single resource dictionary is corrupted, missing, or invalid
- **SC-004**: All significant rendering failures (black windows, clipping, missing controls) can be reproduced and diagnosed using the diagnostics panel without external debugging tools
- **SC-005**: Log files are written to disk within 5 seconds of any logged event and survive application restarts
- **SC-006**: The diagnostics panel itself renders correctly in all supported render modes (FallbackSafe, WindowChrome, unknown host)
- **SC-007**: Runtime overhead of the diagnostics and validation system is imperceptible to users during normal operation

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Diagnostics panel is located within the Settings window under a "Diagnostics" section, accessible to all users
- Validation runs at runtime against currently loaded ResourceDictionary instances, not against XAML files on disk
- Crash-safe loading applies to theme resource dictionaries only (not to application-wide initialization)
- Log files are stored in AppData/Som3a/Logs/ with auto-rotation when a size limit is reached; oldest entries are discarded automatically
- Memory usage is estimated from available .NET process metrics, not an external profiling tool
- GPU availability detection reuses the existing WindowRenderModeDetector infrastructure
- Popup diagnostics focus on ComboBox controls (the most commonly problematic popup type)
- The existing RenderModeService is extended rather than replaced
- Validation findings are suggestions, not blocking errors — the application continues running regardless
