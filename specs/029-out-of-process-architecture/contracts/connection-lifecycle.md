# Connection Lifecycle Contract

## States

```
DISCONNECTED → CONNECTING → CONNECTED → ACTIVE → GRACE_WINDOW → SHUTTING_DOWN
                                  ↓                    ↑
                             (retry loop)         (reconnect)
```

## Transitions

### Cold Start

1. VSTO `ThisAddIn_Startup` reads WPF binary path from registry/config
2. VSTO calls `Process.Start(path)` — WPF process launches
3. VSTO opens `NamedPipeServerStream` at `\\.\pipe\Som3aBridge`
4. WPF opens `NamedPipeClientStream` at same pipe name
5. WPF sends `handshake` message → VSTO responds with Excel HWND + PID
6. WPF sets `WindowInteropHelper.Owner = excelHwnd` on ShellWindow
7. State → `ACTIVE`

**Timeout**: 5s for initial handshake; retry every 5s for up to 60s

### Heartbeat

1. VSTO sends `heartbeat` every 10s
2. WPF replies with `heartbeat_ack` within 1s
3. WPF tracks heartbeat sequence numbers
4. If 3 consecutive heartbeats missed → state `GRACE_WINDOW`

### Grace Window

1. After 3 missed heartbeats, WPF enters 10s grace window
2. WPF shows "Reconnecting..." indicator in UI
3. If new `handshake` received within 10s → return to `ACTIVE`
4. If 10s expires → state `SHUTTING_DOWN`

### Graceful Shutdown

1. VSTO sends `shutdown` message when Excel closes normally
2. WPF acks with `shutdown_ack`
3. WPF enters 10s grace window (for rapid restart)
4. If no new handshake within 10s → WPF calls `Application.Shutdown()`

### Mid-Session Pipe Disconnect

1. WPF detects pipe disconnect (read returns 0 bytes)
2. WPF attempts to reconnect with exponential backoff:
   - Retry 1: wait 1s
   - Retry 2: wait 2s
   - Retry 3: wait 4s
   - Retry 4: wait 8s
   - Retry 5: wait 16s
   - Total retry window: ~31s
3. If any retry succeeds → return to `ACTIVE`
4. If all retries fail → enter offline mode, disable Excel-dependent features, show error
5. User can click "Reconnect" button to trigger manual retry
