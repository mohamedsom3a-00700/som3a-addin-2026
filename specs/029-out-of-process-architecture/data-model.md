# Data Model: Pipe Message Protocol

**Phase**: 1 | **Status**: Draft

## Entities

### PipeMessage

Base message exchanged over the named pipe between WPF and VSTO processes.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | string | yes | Message type discriminator — see MessageType enum |
| `correlationId` | string (GUID) | yes | Unique ID for request-response matching |
| `timestamp` | string (ISO 8601) | yes | UTC timestamp of message creation |
| `payload` | object | varies | Type-specific payload content |

### MessageType enum

| Value | Direction | Description |
|-------|-----------|-------------|
| `handshake` | VSTO→WPF | First message after pipe connect — contains Excel HWND and process info |
| `heartbeat` | VSTO→WPF | Periodic liveness signal (every 10s) |
| `heartbeat_ack` | WPF→VSTO | Acknowledgement of heartbeat receipt |
| `excel_command` | WPF→VSTO | Send an Excel operation command |
| `excel_response` | VSTO→WPF | Response to an Excel command (success data or error) |
| `shutdown` | VSTO→WPF | Graceful shutdown notification (Excel closing normally) |
| `shutdown_ack` | WPF→VSTO | Acknowledgement of shutdown notification |
| `performance_test` | WPF→VSTO | Initiate performance baseline test |
| `performance_result` | VSTO→WPF | Performance test results |
| `error` | either | Error notification (parse failure, unhandled exception) |

### HandshakePayload

First payload exchanged on pipe connection.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `excelHwnd` | int (HWND) | yes | Excel main window handle for window ownership |
| `processId` | int | yes | VSTO host process ID |
| `version` | string | yes | Protocol version (e.g., "1.0.0") |

### HeartbeatPayload

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `sequence` | int | yes | Heartbeat sequence number for gap detection |
| `status` | string | no | Optional status info ("ok", "busy") |

### ExcelCommandPayload

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `cmd` | string | yes | Command name — see ExcelCommandType |
| `sheet` | string | no | Target worksheet name |
| `row` | int | no | Target row (1-indexed) |
| `col` | int | no | Target column (1-indexed) |
| `value` | string | no | Value to write |
| `range` | string | no | Excel range address (e.g., "A1:B10") |

### ExcelCommandType enum

| Value | Description |
|-------|-------------|
| `WriteCell` | Write a single value to a cell |
| `ReadCell` | Read a single cell value |
| `WriteRange` | Write values to a range of cells |
| `ReadRange` | Read a range of cell values |
| `GetSheetNames` | Get list of worksheet names |
| `ActivateSheet` | Activate/select a worksheet |
| `RunFormula` | Set a cell formula |

### ErrorPayload

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `code` | string | yes | Error code (e.g., "PIPE_TIMEOUT", "UNKNOWN_COMMAND") |
| `message` | string | yes | Human-readable error description |
| `correlationId` | string (GUID) | no | Original correlation ID that triggered the error |

## State Transitions

### Pipe Connection Lifecycle

```
[DISCONNECTED] → (VSTO starts WPF process)
    ↓
[CONNECTING] → (WPF connects to named pipe server)
    ↓                    ↓ (failure) → retry every 5s, up to 60s
[CONNECTED] ← → → → → → ┘
    ↓ (handshake received)
[ACTIVE]
    ↓ (heartbeat every 10s)
[ACTIVE] ← (heartbeat_ack every 10s)
    ↓ (3 missed heartbeats)
[GRACE_WINDOW] → (10s timer)
    ↓ (new connection) → back to [ACTIVE]
    ↓ (timeout) → [SHUTTING_DOWN]
    ↓
[SHUTTING_DOWN] → Application.Shutdown()
```

### Command Lifecycle

```
WPF → Send excel_command → VSTO
WPF ← Receive excel_response ← VSTO
    ↓ (timeout 10s)
WPF → Send error (PIPE_TIMEOUT)
```

## Validation Rules

- All messages MUST have a valid `type` from MessageType enum
- `correlationId` MUST be a valid GUID
- `timestamp` MUST be in ISO 8601 format
- `handshake` MUST be the first message type sent after pipe connection
- `excel_command` messages MUST include a valid `cmd` from ExcelCommandType
- Unknown message types MUST be replied to with an `error` message
- Message size MUST NOT exceed 1MB
