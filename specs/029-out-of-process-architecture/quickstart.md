# Quickstart: Out-of-Process Architecture

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 with Office/SharePoint development workload
- Som3a solution cloned and restored

## Key Files

| File | Purpose |
|------|---------|
| `Som3a.Bridge/PipeMessageSchema.cs` | Message type enums, serialization helpers |
| `Som3a.Bridge/ExcelCommandProtocol.cs` | Excel command constants and builders |
| `WpfApp2/Services/PipeClientService.cs` | Named pipe client: connect, send, receive, retry |
| `WpfApp2/Services/CrashWatchdogService.cs` | Heartbeat subscriber, grace timer, shutdown trigger |
| `WpfApp2/App.xaml.cs` | Entry point: detect startup mode (standalone vs VSTO) |
| `WpfApp2/Controls/Shell/ShellWindow.xaml.cs` | Accept Excel HWND, set window owner |
| `Som3a.Shared/ThisAddIn.cs` | Cold start logic: launch WPF, establish pipe server |
| `Som3a.Shared/Som3aAddinBridge.cs` | Route pipe commands to Excel interop calls |

## Build & Test

```powershell
# Build WPF host
dotnet build WpfApp2\Som3a_WPF_UI.csproj

# Build VSTO add-in (requires MSBuild, .NET Framework 4.8)
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Som3a.Shared\Som3a.Shared.csproj /p:Configuration=Debug

# Run tests
dotnet test Tests\Som3a_WPF_UI.Tests.csproj
```

## VSTO Smoke Test Checklist

1. Open Excel → verify ribbon buttons appear
2. Click a ribbon button → WPF ShellWindow opens as separate process (verify in Task Manager)
3. Navigate to 3 pages → sidebar renders, pages load
4. Switch theme (Dark/Light) → both render correctly
5. Trigger Excel cell write → verify value appears in correct sheet
6. Close Excel → WPF window closes within 15s (check Task Manager for zero orphan processes)

## Error Scenarios

| Scenario | Expected Behavior |
|----------|-------------------|
| WPF launched without Excel | "Waiting for Excel..." state, retries every 5s for 60s |
| Excel crashes while WPF open | Grace window 10s → WPF shutdown within 45s total |
| Pipe disconnected mid-session | Exponential backoff reconnect (31s total), then offline mode |
| Performance threshold not met | Log to diagnostics; investigate pipe buffer settings |
