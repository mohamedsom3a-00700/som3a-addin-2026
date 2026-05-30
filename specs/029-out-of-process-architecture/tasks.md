# Tasks: Out-of-Process Architecture (Phase 1B)

**Input**: Design documents from `specs/029-out-of-process-architecture/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Test tasks are included for critical pipe protocol paths.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify project structure and ensure existing code builds cleanly

- [X] T001 Create placeholder files for new source files (Som3a.Bridge/PipeMessageSchema.cs, Som3a.Bridge/ExcelCommandProtocol.cs, WpfApp2/Services/PipeClientService.cs, WpfApp2/Services/CrashWatchdogService.cs) and verify existing project builds with `dotnet build WpfApp2\Som3a_WPF_UI.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ All user stories depend on this phase**

- [X] T002 [P] Create `PipeMessageSchema.cs` in `Som3a.Bridge/` with `MessageType` enum (handshake, heartbeat, heartbeat_ack, excel_command, excel_response, shutdown, shutdown_ack, performance_test, performance_result, error), `PipeMessage` base class, `HandshakePayload`, `HeartbeatPayload`, `ExcelCommandPayload`, `ErrorPayload`, `ExcelCommandType` enum, and JSON serialization using System.Text.Json
- [X] T003 Create `PipeClientService.cs` in `WpfApp2/Services/` implementing named pipe client connect (to `\\.\pipe\Som3aBridge`), send/receive methods with JSON serialization, disconnect, and state tracking (DISCONNECTED, CONNECTING, CONNECTED, ACTIVE) (depends on T002 for message types)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Launch WPF UI from Excel Ribbon (Priority: P1) 🎯 MVP

**Goal**: User clicks ribbon button → VSTO launches WPF as separate process → pipe handshake within 5s → ShellWindow appears as Excel child window

**Independent Test**: Click any Som3a ribbon button, verify ShellWindow opens as separate process (check Task Manager) within 5 seconds, window is responsive, and window stays on top of Excel

- [X] T004 [P] [US1] Modify `App.xaml.cs` in `WpfApp2/` to add standalone launch mode detection (`-standalone` or `--pipe` arg), instantiate `PipeClientService`, connect to pipe, trigger handshake, and show ShellWindow on success; show error toast on failure with 5s retry loop up to 60s
- [X] T005 [US1] Modify `ShellWindow.xaml.cs` in `WpfApp2/Controls/Shell/` to add `SetOwner(nint excelHwnd)` method that calls `WindowInteropHelper(this).Owner = excelHwnd`, to be called after handshake receives Excel HWND (depends on T004 for HWND availability)
- [X] T006 [US1] Modify `ThisAddIn.cs` in `Som3a.Shared/` to add cold start logic in startup: read WPF binary path, call `Process.Start(path)`, create `NamedPipeServerStream` at `\\.\pipe\Som3aBridge`, accept client connection, send `handshake` message with Excel HWND and process ID, support 5s handshake timeout with retry

**Checkpoint**: US1 complete — ShellWindow opens from ribbon button, is responsive, and stays on top of Excel

---

## Phase 4: User Story 2 - Excel Shutdown Triggers WPF Auto-Close (Priority: P1)

**Goal**: When Excel closes or crashes, WPF auto-shuts down within 15-45 seconds, no orphan processes remain

**Independent Test**: Launch WPF from Excel, close Excel → verify WPF closes within 15s, check Task Manager for zero orphan WPF processes

- [X] T007 [P] [US2] Create `CrashWatchdogService.cs` in `WpfApp2/Services/` that subscribes to heartbeats from `PipeClientService`, tracks sequence numbers, counts consecutive missed heartbeats (3 misses = trigger), implements 10s grace window for rapid restart, logs state transitions, and calls `Application.Shutdown()` on timeout
- [X] T008 [US2] Wire `CrashWatchdogService` into `App.xaml.cs` startup flow; modify `ThisAddIn.cs` in `Som3a.Shared/` to send `heartbeat` every 10s via pipe server, handle graceful Excel shutdown by sending `shutdown` message, and support `shutdown_ack` response (depends on T007 for watchdog to consume heartbeats)

**Checkpoint**: US2 complete — closing Excel causes WPF to auto-close cleanly

---

## Phase 5: User Story 3 - Write Excel Cell from WPF UI (Priority: P2)

**Goal**: WPF sends `excel_command` via pipe → VSTO executes Excel interop → response returns within 1s; 100 sequential messages complete in <500ms

**Independent Test**: Trigger a WriteCell command from WPF (e.g., BOQ Generator page) → verify cell value appears in Excel; send 100 commands via performance test → verify <500ms total

- [X] T009 [P] [US3] Create `ExcelCommandProtocol.cs` in `Som3a.Bridge/` with `ExcelCommandType` enum constants, `ExcelCommandPayload` builders, `ExcelResponsePayload`, command serialization helpers, and correlation ID generation
- [X] T010 [P] [US3] Extend `PipeClientService.cs` in `WpfApp2/Services/` with async `SendCommand<T>(ExcelCommandPayload)` method that creates correlation ID, sends `excel_command` message, awaits matching `excel_response`, and returns result with 10s timeout (depends on T009 for command types)
- [X] T011 [US3] Modify `Som3aAddinBridge.cs` in `Som3a.Shared/` to parse incoming `excel_command` messages, dispatch to Excel interop based on command type (WriteCell, ReadCell, etc.), wrap results in `excel_response` messages, and handle errors with `error` responses (depends on T009 for command parsing)
- [X] T012 [US3] Add `PerformanceBaselineTest()` method in `Tests/Som3a_WPF_UI.Tests.csproj` that sends 100 sequential `performance_test` commands, measures total elapsed time with `Stopwatch`, fails if >500ms total, and logs results to diagnostics system using `DiagnosticsChannel` from Som3a.Bridge

**Checkpoint**: US3 complete — Excel cells can be written from WPF UI; performance baseline passes

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements, validation, and compliance checks

- [X] T013 [P] Update documentation: `quickstart.md` with final file list, build/test commands; `contracts/` with any schema changes discovered during implementation
- [X] T014 Validate full build (`dotnet build WpfApp2\Som3a_WPF_UI.csproj`), run tests (`dotnet test Tests\Som3a_WPF_UI.Tests.csproj`), verify VSTO smoke test checklist from quickstart.md, confirm zero orphan processes after Excel exit
- [X] T015 Constitution compliance review: verify `DynamicResource`-only theme usage, no inline `DropShadowEffect`, WindowChrome enforcement, Excel rendering safety, animation governance ≤200ms

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 - Cold Start (Phase 3)**: Depends on Foundational — No deps on other stories
- **US2 - Crash Watchdog (Phase 4)**: Depends on Foundational (T002, T003) + US1 (T004, T006 for startup context)
- **US3 - Excel Commands (Phase 5)**: Depends on Foundational (T002, T003) + US1 (T004 for pipe context)
- **Polish (Phase 6)**: Depends on all desired user stories

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational — standalone independent story
- **US2 (P1)**: Starts after Foundational + US1 basics (needs PipeClientService + ThisAddIn heartbeat sender) — independently testable by closing Excel
- **US3 (P2)**: Starts after Foundational + US1 basics (needs PipeClientService + App launch routing) — independently testable by sending WriteCell

### Within Each User Story

- Models/services before integration
- Core implementation before wiring
- Story complete before moving to next priority

### Parallel Opportunities

- **Phase 2**: T002 (PipeMessageSchema) and T003 (PipeClientService) — sequential (T002→T003)
- **Phase 3**: T004 (App.xaml.cs), T006 (ThisAddIn.cs) — parallel [P]; T005 depends on T004
- **Phase 4**: T007 (CrashWatchdogService) and T008 (wiring) — sequential (T007→T008)
- **Phase 5**: T009 (ExcelCommandProtocol) and T010 (PipeClientService extension) — parallel [P]; T011 depends on T009

---

## Parallel Example: Phase 3 (US1) Launch

```powershell
# All three can run in parallel:
Task: "T004 - Modify App.xaml.cs in WpfApp2/"
Task: "T006 - Modify ThisAddIn.cs in Som3a.Shared/"
# T005 starts after T004 completes
Task: "T005 - Modify ShellWindow.xaml.cs in WpfApp2/Controls/Shell/"
```

## Parallel Example: Phase 5 (US3) Excel Commands

```powershell
# Both run in parallel:
Task: "T009 - Create ExcelCommandProtocol.cs in Som3a.Bridge/"
Task: "T010 - Extend PipeClientService.cs in WpfApp2/Services/"
# T011 starts after T009 completes
Task: "T011 - Modify Som3aAddinBridge.cs in Som3a.Shared/"
```

---

## Implementation Strategy

### MVP First (US1 + US2 — both P1)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (PipeMessageSchema + PipeClientService)
3. Complete Phase 3: US1 (Cold Start — ShellWindow opens from ribbon)
4. Complete Phase 4: US2 (Crash Watchdog — auto-close on Excel exit)
5. **STOP and VALIDATE**: Test US1 + US2 together
   - ShellWindow opens in 5s
   - Window stays on top of Excel
   - Closing Excel closes WPF in 15s
   - Zero orphan processes
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 + US2 → Test together → Deploy/Demo (MVP!)
3. Add US3 → Test independently → Deploy/Demo (full Phase 1B)
4. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
