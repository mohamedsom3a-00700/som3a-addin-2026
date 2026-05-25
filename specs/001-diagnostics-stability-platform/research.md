# Phase 0: Research — Diagnostics & Stability Platform

## Research Tasks

### R001: Memory Estimation Approach

**Decision**: Use `System.Diagnostics.Process.WorkingSet64` for memory estimation.

**Rationale**: WPF applications hosted inside Excel VSTO cannot reliably use performance counters (may not be available in all environments). `Process.WorkingSet64` provides the current physical memory (working set) in bytes, is available in .NET Framework 4.8 without additional dependencies, and requires no elevated permissions. This aligns with the spec's "estimated" requirement — precision to the byte is unnecessary for diagnostics purposes.

**Alternatives considered**:
- `System.Management` WMI queries: More detailed but requires permissions, slower, and unreliable in VSTO hosting.
- `GC.GetTotalMemory()`: Only reports managed heap, not total process memory — misleading for diagnostics.
- `PerformanceCounter`: Requires admin privileges or specific configuration in VSTO hosts.

### R002: Resource Validation Algorithm

**Decision**: Validate ResourceDictionary instances at runtime by enumerating all keys and inspecting values programmatically.

**Rationale**: The spec requires runtime validation of loaded ResourceDictionaries (Assumptions section). The approach:
1. Enumerate all MergedDictionaries from `Application.Current.Resources.MergedDictionaries`
2. For each dictionary, enumerate all keys via `IDictionaryEnumerator`
3. Check for inline colors by inspecting values: any `Color` struct, `SolidColorBrush` with hardcoded `Color` property, or `string` values containing `#` followed by hex
4. Detect missing DynamicResource keys by attempting lookup of common expected keys against a known manifest of required tokens
5. Detect duplicate styles by tracking `Style.TargetType` collisions across dictionaries
6. Detect unresolvable references by scanning `BasedOn`, `TargetType` for null after lookup

**Alternatives considered**:
- Static XAML parsing: Safer but cannot detect runtime-resolved issues. Runtime is correct per spec.
- XAML schema validation: Too strict — flags valid DynamicResource lookups as errors.
- Hybrid approach (runtime + static): More thorough but increases scope beyond spec requirements.

### R003: Logging Implementation

**Decision**: Implement a dedicated `LoggingService` class using `StreamWriter` with synchronized writes.

**Rationale**: The project has no existing logging framework. Adding a third-party dependency (NLog, log4net, Serilog) contravenes the constitution's "No Third-Party UI Frameworks" spirit and adds deployment complexity for a VSTO add-in. A simple file-based logger:
- Writes plain text with structured fields: `[timestamp] [severity] [category] message`
- Uses `StreamWriter` with auto-flush and file-sharing for single-process safety
- Implements rolling file behavior: when file exceeds configured size (default 5MB), rename to `.1` and create new file; keep max 3 rotated files
- Stores files at `%LOCALAPPDATA%\Som3a\Logs\`

**Alternatives considered**:
- System.Diagnostics.Trace: Requires listener configuration, harder to control file location and rotation.
- NLog/Serilog: Powerful but adds NuGet dependency and deployment considerations for VSTO.
- Event Tracing for Windows (ETW): Overkill for this use case, requires admin privileges.

### R004: Popup Diagnostics Approach

**Decision**: Extend existing WindowRenderModeDetector with a popup health check.

**Rationale**: The spec says popup diagnostics focus on ComboBox controls (the most commonly problematic popup type). The approach:
1. Subscribe to ComboBox.DropDownOpened event on open windows
2. Capture popup `FrameworkElement` from ComboBox template
3. Verify `AllowsTransparency` is `False` (per architectural rule)
4. Compare popup `ActualWidth` to expected width (detect clipping)
5. Report pass/fail status with any anomalies found

**Alternatives considered**:
- Visual tree walker on all open windows: Too invasive, may cause rendering issues.
- WPF AutomationPeer-based inspection: More thorough but higher complexity for marginal benefit.
- User-initiated popup test (click to open): Simple but requires user cooperation.

### R005: Crash-Safe Theme Loading

**Decision**: Wrap all ResourceDictionary.MergedDictionary additions in try/catch blocks with fallback to hardcoded safe theme.

**Rationale**: The spec requires graceful recovery when a resource dictionary fails to load (FR-006). The approach extends existing ThemeManager patterns:
1. During `ApplyTheme()`, attempt to load each dictionary in try/catch
2. If a single dictionary fails, skip it, log the failure (FR-008), and continue loading remaining dictionaries
3. If ALL dictionaries fail, load a hardcoded fallback theme defined inline in code (not XAML) containing only essential system brushes in black/white — this cannot fail because it has no external file dependency
4. Set `IsFallbackMode` flag on ThemeManager
5. ThemeManager exposes `IsFallbackActive` property consumed by the diagnostics panel
6. User-visible notification via existing ToastWindow or StatusBar mechanism
7. Once in fallback mode, theme switching is disabled (cannot corrupt further)

**Alternatives considered**:
- Guard class with static fallback brushes: Simpler but harder to extend if more fallback resources needed.
- Separate fallback ResourceDictionary file: Could itself fail to load — defeats the purpose.
- AppDomain-level recovery: Too heavyweight; VSTO add-ins cannot safely use AppDomain isolation.

## Summary

All five research items resolve to concrete implementation strategies. No NEEDS CLARIFICATION items remain. The feature is ready for Phase 1 (Design & Contracts).
