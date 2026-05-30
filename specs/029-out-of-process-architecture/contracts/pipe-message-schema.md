# Pipe Message Contract

**Protocol Version**: 1.0.0
**Transport**: Named Pipe (Som3a.Bridge)
**Serialization**: JSON (UTF-8)

## Base Message Format

All messages follow this structure:

```json
{
  "type": "handshake|heartbeat|heartbeat_ack|excel_command|excel_response|shutdown|shutdown_ack|performance_test|performance_result|error",
  "correlationId": "a1b2c3d4-...",
  "timestamp": "2026-05-30T12:00:00.000Z",
  "payload": { }
}
```

## Message Schemas

### Handshake (VSTO→WPF, first message)

```json
{
  "type": "handshake",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "excelHwnd": 123456,
    "processId": 7890,
    "version": "1.0.0"
  }
}
```

### Heartbeat (VSTO→WPF, every 10s)

```json
{
  "type": "heartbeat",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "sequence": 1,
    "status": "ok"
  }
}
```

### Heartbeat Ack (WPF→VSTO)

```json
{
  "type": "heartbeat_ack",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "sequence": 1
  }
}
```

### Excel Command (WPF→VSTO)

```json
{
  "type": "excel_command",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "cmd": "WriteCell",
    "sheet": "Sheet1",
    "row": 1,
    "col": 1,
    "value": "Hello"
  }
}
```

### Excel Response (VSTO→WPF)

```json
{
  "type": "excel_response",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "success": true,
    "data": null
  }
}
```

### Shutdown (VSTO→WPF)

```json
{
  "type": "shutdown",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "reason": "ExcelClosing"
  }
}
```

### Error (either direction)

```json
{
  "type": "error",
  "correlationId": "guid",
  "timestamp": "iso8601",
  "payload": {
    "code": "UNKNOWN_COMMAND",
    "message": "Received unknown command type: invalid_cmd",
    "correlationId": "original-guid"
  }
}
```

## Error Codes

| Code | Description |
|------|-------------|
| `HANDSHAKE_TIMEOUT` | Pipe handshake not completed within timeout |
| `PIPE_DISCONNECTED` | Pipe connection lost unexpectedly |
| `UNKNOWN_COMMAND` | Received unrecognized command type |
| `EXCEL_ERROR` | Excel COM operation failed |
| `MESSAGE_TOO_LARGE` | Message exceeds 1MB limit |
| `PARSE_ERROR` | JSON deserialization failure |
| `INVALID_STATE` | Message received in wrong connection state |
