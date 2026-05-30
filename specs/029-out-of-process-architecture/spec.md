# Feature Specification: Out-of-Process Architecture

**Feature Branch**: `029-out-of-process-architecture`

**Created**: 2026-05-30

**Status**: Draft

**Input**: User description: "Phase 1B of Fluent UI & Modern Platform Migration — WPF UI runs as standalone .NET 8 process communicating with VSTO add-in via Som3a.Bridge named pipe. Includes cold start protocol, Excel crash watchdog, window ownership, Excel command protocol, and pipe performance baseline."

## Clarifications

### Session 2026-05-30

- Q: Cold start failure — what happens after error toast? → A: WPF stays open with periodic retry (every 5s, up to 60s) until handshake succeeds or timeout expires
- Q: Grace period for rapid Excel restart → A: 10-second grace window after detecting Excel disconnect before initiating WPF shutdown
- Q: Mid-session pipe reconnection strategy → A: Exponential backoff (1s, 2s, 4s, 8s, 16s), 5 retries (~31s total), then declare connection lost and enter offline mode

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Launch WPF UI from Excel Ribbon (Priority: P1)

An Excel user clicks a ribbon button in the Som3a add-in. The VSTO add-in starts the WPF application as a separate process, establishes a named pipe connection, and the WPF ShellWindow appears within 5 seconds, ready for interaction.

**Why this priority**: This is the foundational flow — without it, no other Phase 1B feature can function. The entire out-of-process architecture depends on successful cold start and pipe handshake.

**Independent Test**: Can be fully tested by clicking any ribbon button that launches the WPF UI and verifying the window appears and is responsive within 5 seconds.

**Acceptance Scenarios**:

1. **Given** Excel is running with the Som3a add-in loaded, **When** the user clicks a ribbon button that launches the WPF UI, **Then** the ShellWindow appears as a separate process within 5 seconds
2. **Given** the WPF window has opened, **When** the user tries to interact with the UI (click sidebar, navigate pages), **Then** all interactions respond normally
3. **Given** the VSTO add-in cannot establish a pipe handshake within 5 seconds, **When** the startup sequence completes, **Then** an error toast is displayed and the WPF process continues retrying every 5 seconds for up to 60 seconds
4. **Given** the handshake retries have all failed after 60 seconds, **When** the final timeout expires, **Then** the WPF process displays a permanent failure message and exits

---

### User Story 2 - Excel Shutdown Triggers WPF Auto-Close (Priority: P1)

The user closes Excel (either intentionally or due to a crash). The WPF UI detects the lost connection via missing heartbeats and auto-shuts-down within 15 seconds, leaving no orphan processes.

**Why this priority**: Preventing orphan processes is critical for system stability and resource management. A crash watchdog is essential for a robust out-of-process architecture.

**Independent Test**: Can be tested by launching the WPF UI from Excel, then closing Excel — verify the WPF window closes automatically and no WPF process remains in Task Manager.

**Acceptance Scenarios**:

1. **Given** the WPF UI is running connected to Excel via named pipe, **When** the user closes Excel normally, **Then** the WPF window shuts down within 15 seconds
2. **Given** the WPF UI is running connected to Excel, **When** Excel crashes or is killed via Task Manager, **Then** the WPF window shuts down within 45 seconds (3 missed heartbeats × 10s + 10s grace + overhead)
3. **Given** the WPF UI has shut down due to Excel closing, **When** the user checks Task Manager, **Then** no WPF-related orphan processes remain

---

### User Story 3 - Write Excel Cell from WPF UI (Priority: P2)

A user working in the WPF UI (e.g., on the BOQ Activity Generator page) triggers an action that writes data to an Excel cell. The command is sent via the named pipe protocol, and the cell value appears in the active Excel workbook.

**Why this priority**: Excel interop is the core value of the add-in. Users must be able to read/write Excel data from the WPF UI via the pipe bridge.

**Independent Test**: Can be tested by navigating to any page that writes to Excel (e.g., BOQ Generator), triggering a write action, and verifying the cell value appears in Excel.

**Acceptance Scenarios**:

1. **Given** the WPF UI is connected to Excel via named pipe, **When** the user triggers a "WriteCell" command from the WPF UI, **Then** the specified cell value appears in the correct sheet within 1 second
2. **Given** the pipe connection is active, **When** 100 sequential Excel commands are sent, **Then** all 100 complete in under 500ms total

---

### Edge Cases

- What happens if the user starts the WPF process manually (outside of VSTO) without Excel running? The WPF process should detect no pipe server and either retry or show a "Waiting for Excel..." state with a timeout.
- What happens if the pipe connection is lost mid-operation (e.g., during a multi-step Excel write)? The WPF should attempt to reconnect using exponential backoff (1s, 2s, 4s, 8s, 16s for 5 retries, ~31s total). If all retries fail, display an error and enter offline mode with Excel-dependent features disabled. The user can manually trigger a reconnect.
- What happens if the Excel process is busy or frozen and doesn't respond to the heartbeat? The crash watchdog should differentiate between a frozen process (no heartbeat) and a slow process (delayed heartbeat). After 3 consecutive missed heartbeats, treat as crash and initiate shutdown after a 10-second grace window.
- What happens on rapid restart (user closes Excel and immediately reopens)? The WPF process should wait for 10 seconds after detecting the disconnect before initiating shutdown, giving the new Excel instance time to establish a connection.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: VSTO add-in MUST launch the WPF application as a separate OS process during add-in startup
- **FR-002**: VSTO add-in MUST establish a named pipe connection with the WPF process within 5 seconds of launching it. On failure, the WPF process MUST display an error toast and retry the handshake every 5 seconds for up to 60 seconds. After 60 seconds, the WPF process MUST exit with a permanent failure message.
- **FR-003**: VSTO add-in MUST send a heartbeat ping to the WPF process every 10 seconds via the named pipe
- **FR-004**: WPF process MUST shut down automatically after detecting that Excel has stopped sending heartbeat signals for 30 consecutive seconds
- **FR-005**: WPF ShellWindow MUST behave as a child of the Excel main window so that all dialogs and popups remain on top of Excel
- **FR-006**: WPF process MUST be able to send structured Excel commands to the VSTO add-in via named pipe (e.g., `{ "cmd": "WriteCell", "sheet": "...", "row": N, "col": N, "value": "..." }`)
- **FR-007**: Named pipe message exchange MUST complete 100 sequential request-response cycles in under 500 milliseconds total (average < 5ms per message)
- **FR-008**: WPF process MUST log the results of the performance baseline test to the diagnostics system
- **FR-009**: WPF application entry point MUST support launching independently without requiring VSTO hosting

### Key Entities *(include if feature involves data)*

- **PipeMessage**: The structured message format exchanged over the named pipe. Contains command type (e.g., heartbeat, WriteCell, ReadCell), payload data, and correlation ID for request-response matching.
- **ExcelCommand**: A specific subtype of PipeMessage defining WPF→VSTO operations: target sheet, cell coordinates, value to write, formula to execute, or data range to read.
- **HeartbeatSignal**: A periodic ping message from VSTO→WPF used by the crash watchdog to monitor Excel process health.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users see the WPF ShellWindow within 5 seconds of clicking a ribbon button that launches it
- **SC-002**: When Excel is closed normally, the WPF process exits within 25 seconds (10s grace window + 15s detection); for crash, within 45 seconds (10s grace + 35s detection)
- **SC-003**: Zero orphan WPF processes remain after Excel exits (verified via system process list)
- **SC-004**: Users can write data to Excel cells from the WPF UI with commands completing in under 1 second
- **SC-005**: Inter-process communication supports at least 200 message exchanges per second (100 messages in under 500ms)
- **SC-006**: WPF dialog windows (e.g., file picker, confirmation dialogs) stay on top of the Excel window (modal ownership)

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The Som3a.Bridge named pipe infrastructure already exists and will be extended, not rewritten from scratch
- The existing VSTO add-in startup and shutdown lifecycle hooks are available to wire into
- The WPF application can be launched independently of the VSTO host (supporting both in-process VSTO hosting and standalone out-of-process modes)
- Heartbeat interval of 10 seconds with 3 consecutive failures (30 seconds total) plus a 10-second grace window is sufficient to detect Excel crashes without false positives from brief hangs
- The performance baseline threshold (< 5ms average per message) is feasible given the named pipe is a local inter-process communication channel
- Window ownership via HWND is compatible with the existing WindowChrome configuration
- The pipe message schema can use JSON serialization for simplicity and debuggability
