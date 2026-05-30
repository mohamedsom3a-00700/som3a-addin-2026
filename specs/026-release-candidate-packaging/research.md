# Research: Release Candidate & Production Packaging

## MSI Installer for VSTO Add-in with .NET 8 Dependencies

**Decision**: Use WiX Toolset v4 for MSI packaging with a Burn bootstrapper for .NET 8 runtime chaining.

**Rationale**:
- VSTO add-in registration requires specific registry keys under `HKCU\Software\Microsoft\Office\Excel\Addins` with `Manifest`, `LoadBehavior`, `FriendlyName`, and `Description` values
- The `|vstolocal` postfix on the Manifest value is critical for MSI-deployed add-ins (avoids ClickOnce cache)
- .NET 8 cannot load in-process with .NET Framework 4.8 — the .NET 8 service binaries must run out-of-process, communicated via the existing Som3a.Bridge interop
- WiX Burn bootstrapper chains prerequisites: .NET Framework 4.8 redist → .NET 8 runtime → MSI package
- MajorUpgrade element handles version upgrades via stable UpgradeCode GUID

**Alternatives considered**:
- InstallShield LE: More expensive, less VSTO-specific community knowledge
- Manual setup.exe: Lacks MSI maintenance/repair/rollback capabilities
- Single MSI without runtime chaining: Requires separate runtime installer, worse UX

## Code Signing

**Decision**: Authenticode sign all executables and VSTO manifests with EV code signing certificate. Timestamp all signatures.

**Rationale**:
- VSTO requires both Authenticode on DLLs and ClickOnce manifest signing (.dll.manifest + .vsto)
- EV certificate provides immediate SmartScreen reputation for enterprise deployment
- Timestamping prevents expiration issues for long-lived enterprise installs
- Certificate push to Trusted Publishers via Group Policy for silent deployment

## Excel Interop Cleanup

**Decision**: Use bulk Range.Value read/write with Marshal.ReleaseComObject + two-pass GC cleanup pattern.

**Rationale**:
- Bulk read/write via `object[,]` arrays is 10-100x faster than cell-by-cell for 10K+ rows
- Two-dot chains (e.g., `app.Workbooks.Open`) create implicit COM references — assign intermediates to locals
- Two-pass `GC.Collect()` + `GC.WaitForPendingFinalizers()` after manual releases ensures Excel.exe termination
- Wrap all interop in `try/finally` with each release in its own try/catch to prevent cascading failures

## WPF Performance Optimization

**Decision**: DataGrid Recycling virtualization + lazy page loading + periodic GC for long sessions.

**Rationale**:
- VirtualizationMode.Recycling already configured globally in DataGridStyles.xaml — add EnableColumnVirtualization for wide grids
- No ScrollViewer around DataGrids (preserves virtualization)
- Lazy-load pages via TabControl ContentTemplate trigger on IsSelected
- Periodic optimized GC (GCCollectionMode.Optimized, Gen 2) on 5-minute timer during idle for 8+ hour sessions
- Weak Event pattern for all static event subscriptions (EventBus already uses WeakReference)

## Crash Diagnostics & Safe Logging

**Decision**: Capture diagnostics snapshot at crash (memory, active plugins, recent operations, error context) with no PII collection.

**Rationale**:
- Safe logging at error-level (100MB cap) prevents log flooding
- PII exclusion via assumption — no user data, file paths, or Excel content captured
- Diagnostics export as structured JSON for IT admin analysis
