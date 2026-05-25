# Data Model: Diagnostics & Stability Platform

## Entities

### DiagnosticSnapshot

A point-in-time capture of application rendering state displayed in the diagnostics panel.

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| `RenderMode` | string | Current mode: `FallbackSafe`, `WindowChrome`, or `Unknown` | RenderModeService |
| `RenderModeSource` | string | Detected by: host type, GPU availability, transparency test | RenderModeService |
| `GpuAvailable` | bool | Whether GPU acceleration is detected | RenderModeService |
| `GpuName` | string | GPU adapter description (if available) | RenderModeService |
| `ActiveTheme` | string | Current theme name (Dark, Light, Custom) | ThemeManager |
| `AccentColor` | string | Current accent color hex value | ThemeManager |
| `IsFallbackMode` | bool | Whether crash-safe fallback is active | ThemeManager |
| `MemoryWorkingSetMB` | double | Current process working set in MB | Process.WorkingSet64 |
| `MemoryManagedMB` | double | Managed heap size in MB | GC.GetTotalMemory |
| `PopupStatus` | string[] | Per-popup health status (ComboBox controls) | PopupDiagnostics |
| `Timestamp` | DateTime | When the snapshot was taken | System clock |

**Validation Rules:**
- All numeric values MUST be non-negative
- `RenderMode` MUST be one of: `FallbackSafe`, `WindowChrome`, `Unknown`
- `Timestamp` MUST use UTC
- Missing values MUST display as "N/A" not as empty or error text

### ValidationResult

A single finding from a resource validation scan.

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| `Id` | string | Unique identifier (e.g., `VR-001`) | Generated |
| `Severity` | string | `error`, `warning`, or `info` | ValidationEngine |
| `Category` | string | `missing-token`, `inline-color`, `duplicate-style`, `invalid-resource` | ValidationEngine |
| `DictionaryName` | string | Name of the ResourceDictionary containing the issue | ValidationEngine |
| `Location` | string | Key name or TargetType of the offending resource | ValidationEngine |
| `Description` | string | Human-readable description of the issue | ValidationEngine |
| `SuggestedFix` | string | Recommended token replacement or remediation | ValidationEngine |
| `Timestamp` | DateTime | When the finding was recorded | System clock |

**Validation Rules:**
- `Severity` MUST be one of: `error`, `warning`, `info`
- `Category` MUST be one of: `missing-token`, `inline-color`, `duplicate-style`, `invalid-resource`
- `Id` MUST be unique per scan session
- `Description` MUST NOT be empty

**Severity Guidelines:**
| Category | Default Severity | Reasoning |
|----------|-----------------|-----------|
| `inline-color` | `error` | Breaks theme switching, high impact |
| `missing-token` | `error` | Causes rendering artifacts, high impact |
| `duplicate-style` | `warning` | Causes unexpected styling, medium impact |
| `invalid-resource` | `warning` | May or may not affect rendering, medium impact |

### LogEntry

A single event recorded to the application log.

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| `Timestamp` | DateTime | When the event occurred (local time) | System clock |
| `Severity` | string | `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL` | Caller |
| `Category` | string | Event category: `Theme`, `Render`, `Validation`, `Diagnostics`, `System` | Caller |
| `Message` | string | Human-readable event description | Caller |
| `Source` | string | Originating component (e.g., `ThemeManager`, `ValidationEngine`) | Caller |
| `Exception` | string | Exception details (if applicable), truncated to 1000 chars | Caller |

**File Format (per FR-010):**
```text
[2026-05-25 14:30:00] [ERROR] [ThemeManager] Failed to load DarkColors.xaml | Source: ApplyTheme
[2026-05-25 14:30:05] [INFO] [Validation] Scan completed: 3 issues found | Source: ValidationEngine
```

**Validation Rules:**
- `Severity` MUST be one of: `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`
- `Category` MUST be one of: `Theme`, `Render`, `Validation`, `Diagnostics`, `System`
- `Message` MUST NOT exceed 500 characters
- `Exception` MUST NOT exceed 1000 characters (truncated if longer)

### FallbackManifest

The set of resource dictionaries and state used when primary theme resources fail to load.

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| `IsActive` | bool | Whether fallback mode is currently active | ThemeManager |
| `FailedDictionaries` | string[] | List of ResourceDictionary file paths that failed to load | ThemeManager |
| `FailureReasons` | string[] | Error messages corresponding to each failed dictionary | ThemeManager |
| `ActivatedAt` | DateTime | When fallback mode was activated | System clock |
| `HardcodedResourcesCount` | int | Number of inline fallback resources loaded | ThemeManager |

**Validation Rules:**
- `FailedDictionaries.Count` MUST equal `FailureReasons.Count`
- `IsActive` MUST be `false` if `FailedDictionaries` is empty

## Relationships

```text
ThemeManager (applies theme)
    │
    ├── FallbackManifest (tracks fallback state)
    └── DiagnosticSnapshot (reads theme state)
    
RenderModeService (detects host)
    │
    └── DiagnosticSnapshot (reads render mode)
    
ValidationEngine (scans resources)
    │
    ├── ValidationResult[] (produces findings)
    └── LogEntry[] (logs findings)
    
LoggingService (writes logs)
    │
    ├── LogEntry[] (persisted to disk)
    └── DiagnosticSnapshot (reads recent entries)
    
DiagnosticsService (aggregates)
    │
    ├── DiagnosticSnapshot (produced on demand)
    ├── ValidationResult[] (from ValidationEngine)
    └── LogEntry[] (from LoggingService)
    
DiagnosticsViewModel (consumes)
    │
    └── DiagnosticSnapshot, ValidationResult[], LogEntry[] (displayed in UI)
```
