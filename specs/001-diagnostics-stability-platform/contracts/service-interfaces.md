# Service Interface Contracts: Diagnostics & Stability Platform

## IDiagnosticsService

Aggregates rendering, theme, memory, and popup diagnostics data for the diagnostics panel.

```csharp
public interface IDiagnosticsService
{
    /// <summary>
    /// Captures a point-in-time snapshot of all diagnostics data.
    /// Must complete within 500ms.
    /// </summary>
    DiagnosticSnapshot CaptureSnapshot();

    /// <summary>
    /// Fired when a new snapshot is available (for auto-refresh).
    /// </summary>
    event EventHandler<DiagnosticSnapshot> SnapshotUpdated;
}
```

**Dependencies**: IRenderModeService, IThemeManager, ILoggingService

---

## IValidationEngine

Scans loaded ResourceDictionaries for compliance violations.

```csharp
public interface IValidationEngine
{
    /// <summary>
    /// Runs a full validation scan against all currently loaded
    /// Application.Current.Resources.MergedDictionaries.
    /// Must complete within 3 seconds.
    /// The RunValidation button MUST be disabled while this executes.
    /// </summary>
    IReadOnlyList<ValidationResult> RunValidation();

    /// <summary>
    /// Fired when a validation scan completes.
    /// </summary>
    event EventHandler<IReadOnlyList<ValidationResult>> ValidationCompleted;
}
```

**Validation categories** (per ValidationResult.Category):
- `missing-token` — DynamicResource key not found in any loaded dictionary
- `inline-color` — Hardcoded Color/Brush value in resource definition
- `duplicate-style` — Same TargetType defined in multiple dictionaries
- `invalid-resource` — Resource reference (BasedOn, TargetType) resolves to null

---

## ILoggingService

Persists structured log entries to disk with auto-rotation.

```csharp
public interface ILoggingService
{
    /// <summary>
    /// Writes a log entry to disk. Must complete within 100ms.
    /// If file write fails, silently swallow — never crash the application.
    /// </summary>
    void Write(LogEntry entry);

    /// <summary>
    /// Convenience method for common log patterns.
    /// </summary>
    void Log(string severity, string category, string message, string source = null, string exception = null);

    /// <summary>
    /// Returns the N most recent log entries (reads from disk).
    /// Used by the diagnostics panel log viewer.
    /// </summary>
    IReadOnlyList<LogEntry> GetRecentEntries(int count = 50);

    /// <summary>
    /// Path to the current log file on disk.
    /// </summary>
    string CurrentLogFilePath { get; }
}
```

**Log rotation** (per clarification):
- Default max file size: 5 MB
- Max rotated files kept: 3 (`.log`, `.log.1`, `.log.2`, `.log.3`)
- Oldest file deleted when rotation limit exceeded
- File path: `%LOCALAPPDATA%\Som3a\Logs\diagnostics-YYYYMMDD.log`

---

## Extensions to Existing Interfaces

### IThemeManager (extended)

```csharp
// New members added to existing IThemeManager interface:

/// <summary>
/// Whether crash-safe fallback mode is currently active.
/// </summary>
bool IsFallbackActive { get; }

/// <summary>
/// Gets the fallback manifest describing what failed and when.
/// </summary>
FallbackManifest GetFallbackManifest();
```

### IRenderModeService (extended)

```csharp
// New members added to existing IRenderModeService interface:

/// <summary>
/// Returns health status for tracked popup controls.
/// </summary>
IReadOnlyList<string> GetPopupDiagnostics();
```
