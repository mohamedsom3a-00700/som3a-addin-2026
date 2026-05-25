# Quickstart: Diagnostics & Stability Platform

## Overview

4 new service classes, 1 new ViewModel, 1 new Models file, 1 modified Settings window — no new NuGet dependencies.

## Files to Create

| File | Type | Purpose |
|------|------|---------|
| `WpfApp2/Services/DiagnosticsService.cs` | New class | Aggregates render, theme, memory, popup data |
| `WpfApp2/Services/ValidationEngine.cs` | New class | Scans ResourceDictionaries for violations |
| `WpfApp2/Services/LoggingService.cs` | New class | File-based structured logging with rotation |
| `WpfApp2/ViewModels/DiagnosticsViewModel.cs` | New class | MVVM for diagnostics panel |
| `WpfApp2/Models/DiagnosticsModels.cs` | New file | DTOs: DiagnosticSnapshot, ValidationResult, LogEntry, FallbackManifest |
| `WpfApp2/Services/ThemeManager.cs` | Modify | Add IsFallbackActive, GetFallbackManifest, crash-safe try/catch |
| `WpfApp2/Services/RenderModeService.cs` | Modify | Add GetPopupDiagnostics(), ComboBox health checks |
| `WpfApp2/Views/SettingsWindow.xaml` | Modify | Add Diagnostics section/tab |
| `WpfApp2/Views/SettingsWindow.xaml.cs` | Modify | Wire DiagnosticsViewModel |

## Build & Test

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Manual test checklist (Excel VSTO)
# 1. Open Settings → Diagnostics section
# 2. Verify render mode, theme, GPU, memory display correctly
# 3. Click "Run Validation" — verify results appear within 3s
# 4. Trigger theme switch — verify diagnostics panel updates
# 5. Corrupt a resource dictionary — verify fallback activates
# 6. Open ComboBox — verify popup health reports OK
# 7. Check %LOCALAPPDATA%\Som3a\Logs\ for log files
```

## Key Architecture Decisions

- **No NuGet dependencies** — all code uses .NET Framework 4.8 built-in types
- **Read-only panel** — no recovery actions from diagnostics (per clarification)
- **Button disabled while validating** — no concurrency issues (per clarification)
- **Plain text structured logs** — simple to read, no parser needed (per clarification)
- **Auto-rotate logs at 5MB** — prevents disk bloat (per clarification)
