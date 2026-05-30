# Research: Out-of-Process Architecture

**Phase**: 0 | **Status**: Complete

## Decisions

### Named Pipe Communication

- **Decision**: Use `System.IO.Pipes` (`NamedPipeServerStream` / `NamedPipeClientStream`) with duplex mode for bidirectional communication
- **Rationale**: Already established via Som3a.Bridge; zero additional dependencies; built-in .NET support
- **Alternatives considered**: TCP loopback (unnecessary overhead for local IPC), Memory-mapped files (more complex synchronization), WCF (deprecated in .NET 8)

### Pipe Message Serialization

- **Decision**: Use `System.Text.Json` for JSON message serialization
- **Rationale**: Already referenced in WpfApp2 project; faster than Newtonsoft.Json; no additional NuGet dependency
- **Alternatives considered**: Newtonsoft.Json (already in some projects but heavier), Binary serialization (not version-tolerant), MessagePack (faster but extra dependency)

### Window Ownership Pattern

- **Decision**: Use `WindowInteropHelper.Owner = excelHwnd` to set Excel as the WPF ShellWindow owner
- **Rationale**: Standard WPF pattern for setting window owner from a native HWND; compatible with WindowChrome as it operates below the Chrome layer
- **Alternatives considered**: Setting `Topmost = true` (incorrect — doesn't provide true modal ownership), Manual Win32 `SetParent` (too aggressive — would clip chrome)

### Heartbeat Protocol

- **Decision**: VSTO sends heartbeat every 10s; WPF watchdog counts consecutive misses; 3 misses (30s) triggers shutdown initiation; 10s grace window for rapid restart
- **Rationale**: 10s interval balances network traffic with timely detection; 3-miss rule avoids false positives from brief hangs; 10s grace allows new Excel instance to connect on restart
- **Alternatives considered**: 5s heartbeat (too chatty), 30s heartbeat (too slow detection), 1-miss immediate shutdown (too aggressive — false positives on slow Excel operations)

### Cold Start Protocol

- **Decision**: VSTO calls `Process.Start()` on WPF binary path; WPF process starts, connects as pipe client; VSTO accepts as pipe server; handshake within 5s; WPF retries every 5s for up to 60s before permanent failure
- **Rationale**: WPF as pipe client ensures VSTO server is ready before WPF attempts connection; 60s retry covers slow-start or compilation scenarios
- **Alternatives considered**: WPF as pipe server (VSTO fails to connect if WPF starts slowly), VSTO launching WPF with `/wait` flag (blocks VSTO startup)

### Excel Command Protocol

- **Decision**: JSON messages with structure `{ "cmd": "CommandName", "params": {...}, "correlationId": "guid" }`
- **Rationale**: Simple, extensible, debuggable; correlationId enables request-response matching for async operations
- **Alternatives considered**: Fixed-position binary protocol (not extensible, hard to debug), Custom delimited format (prone to parsing errors)

## Performance Baseline

- Target: <5ms avg per message, 100 msg in <500ms
- Method: Send 100 sequential request-response messages, measure elapsed time with `Stopwatch`, log result to diagnostics
- Risk mitigation: If threshold not met, investigate pipe buffer tuning (`PipeOptions.WriteThrough`, buffer sizes) before changing protocol
