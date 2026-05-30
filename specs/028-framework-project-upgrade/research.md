# Research: Framework & Project Format Upgrade (.NET 4.8 → .NET 8.0)

**Phase**: 0 — Research
**Date**: 2026-05-30
**Spec**: [spec.md](./spec.md)

## 1. SDK-Style Project Format

### Decision
Convert `Som3a_WPF_UI.csproj` from legacy .NET Framework format to SDK-style.

### Rationale
SDK-style is the modern .NET project format, required for `net8.0-windows` targeting. It provides implicit framework references (WPF, Windows Forms), built-in NuGet PackageReference support, and simplifies the project file from ~200 lines to ~30-50 lines.

### Key Properties Required

```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <OutputType>WinExe</OutputType>
  </PropertyGroup>
</Project>
```

- `Microsoft.NET.Sdk.WindowsDesktop` Sdk is required for WPF applications targeting .NET 8.0 (or use `Microsoft.NET.Sdk` with `<UseWPF>true</UseWPF>` which auto-imports the WPF SDK).
- `.NET 8.0` uses `<UseWPF>true</UseWPF>` inside `Microsoft.NET.Sdk` (the WPF/WindowsDesktop SDK is merged into the base SDK starting from .NET 6).

### Alternatives Considered
- Keeping legacy format with `net48` target — rejected because the feature goal is to upgrade to .NET 8.0.
- Using `Microsoft.NET.Sdk.WindowsDesktop` explicitly — compatible but unnecessary; `Microsoft.NET.Sdk` with `<UseWPF>true</UseWPF>` is preferred for .NET 8.0.

---

## 2. .NET Framework 4.8 → 8.0 API Compatibility

### Decision
Fix compile errors with minimum-change API replacements; use .NET 8.0 compatibility analyzers.

### Known Breaking Changes Affecting WPF Applications

| Area | .NET Framework 4.8 API | .NET 8.0 Replacement | Impact |
|------|----------------------|---------------------|--------|
| AppDomain | `AppDomain.CreateDomain()` | Removed — use `AppDomain.CurrentDomain` only | Low — rarely used in WPF apps |
| BinaryFormatter | `BinaryFormatter.Deserialize()` | Disabled by default — throws `NotSupportedException` | Medium — used for settings serialization |
| CAS | `System.Security.Permissions.*` | Completely removed | Low — not commonly used |
| Remoting | `System.Runtime.Remoting` | Removed | Low — not used in WPF apps |
| Configuration | `ConfigurationManager.AppSettings` | Still available via `System.Configuration` package | Low — NuGet package available |
| DataTable | `DataTable.DefaultView.Sort` | Behavior preserved but `DataTable` serialization changed | Low |
| WPF WebBrowser | Legacy IE-based `WebBrowser` | Still IE-based on .NET 8.0; no change needed | None |
| Windows Forms interop | `System.Windows.Forms.Integration.*` | Moved to `System.Windows.Forms.Integration` NuGet | Low |
| Security: AllowPartiallyTrustedCallers | `AllowPartiallyTrustedCallersAttribute` | Removed — no effect | Low |
| System.Drawing | `System.Drawing.Image`, `System.Drawing.Bitmap` | Still available via `System.Drawing.Common` NuGet | Low — may need explicit package reference |

### Strategy
1. Run `dotnet build` and collect all compile errors
2. For each error, determine the .NET 8.0 equivalent API
3. Apply the minimum change to restore compilation
4. Use `#if NETFRAMEWORK` / `#if NET` if a single code path must support both targets (not needed for Phase 1A since WpfApp2 fully moves to .NET 8.0)

### Tools Available
- .NET Portability Analyzer (VS extension)
- `dotnet upgrade-assistant` (Microsoft tool)
- `Try .NET` global analysis

---

## 3. NuGet Package Compatibility

### Decision
Upgrade each package to the minimum version that supports .NET 8.0.

### Package Compatibility Assessment

| Package | Current Version | Minimum .NET 8.0 Compatible Version | Notes |
|---------|----------------|--------------------------------------|-------|
| MaterialDesignColors | 3.1.0 | 3.1.0+ (latest stable) | MDIX (Material Design Icons) supports net8.0-windows from version 3.x |
| MaterialDesignThemes | 5.1.0 | 5.2.0+ | MaterialDesignThemes 5.1.0 targets net48 only; 5.2+ added net8.0-windows support |
| Microsoft.Web.WebView2 | 1.0.3912.50 | Same version | WebView2 NuGet supports net8.0-windows from a wide range |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.39 | Same version | Xaml.Behaviors supports net8.0-windows from version 1.1.x |
| System.Text.Json | 6.0.0 | 6.0.0+ (recommend 8.0.x) | System.Text.Json 6.0.0 was built for .NET 6 and supports .NET 8.0 via compat mode; upgrading to 8.0.x preferred |
| Newtonsoft.Json | 13.0.4 | 13.0.4+ | Newtonsoft.Json 13.0.4 supports .NET 8.0 — no upgrade needed |

### Verification
After updating, run `dotnet restore` and verify no NuGet package version conflicts.

---

## 4. VSTO Add-in Configuration

### Decision
Audit and update the VSTO add-in project's app.config for binding redirects.

### Context
The VSTO add-in project (separate from WpfApp2) remains on .NET Framework 4.8. Its `app.config` may contain binding redirects for WPF assemblies (`PresentationFramework`, `PresentationCore`, `WindowsBase`, `System.Xaml`) that reference the .NET Framework 4.8 versions.

After the upgrade, the WpfApp2 project generates a .NET 8.0 assembly. The VSTO add-in may need:
1. Updated binding redirects for any shared assemblies
2. Adjusted assembly references if the VSTO add-in directly references WpfApp2 output
3. Updated `codeBase` elements for assemblies that changed version

### Risk
If the VSTO add-in directly loads the WpfApp2 assembly (in-process), this will fail because a .NET 8.0 assembly cannot be loaded by a .NET Framework 4.8 process. This architectural concern is addressed in **Phase 1B** (out-of-process architecture). Phase 1A focuses on compile-time success and build verification.

### Mitigation for Phase 1A
- Document that runtime integration testing (VSTO smoke test) may require Phase 1B changes
- Verify `MSBuild.exe WpfApp2\Som3a_WPF_UI.csproj` succeeds
- Verify existing test projects pass against the upgraded WpfApp2

---

## 5. Test Project Compatibility

### Decision
All existing test projects must pass unchanged after the upgrade.

### Test Projects

| Project | Framework | Target | Dependencies |
|---------|-----------|--------|-------------|
| Som3a_WPF_UI.Tests | MSTest (3.1.1) | .NET 8.0 | WpfApp2 output |
| Som3a.Infrastructure.Tests | xUnit (2.6.0) | .NET 8.0 | Infrastructure libraries |

Both test projects already use SDK-style format and PackageReference. They should continue to work with the upgraded WpfApp2 assembly.

### Verification
```powershell
dotnet test Tests\Som3a_WPF_UI.Tests\Som3a_WPF_UI.Tests.csproj
dotnet test Tests\Som3a.Infrastructure.Tests\Som3a.Infrastructure.Tests.csproj
```

---

## 6. Visual Regression Baseline

### Decision
Capture pre-upgrade screenshots of 3-5 key pages in both Dark and Light themes for comparison.

### Recommended Pages for Screenshots
1. ShellWindow (sidebar visible, navigation loaded) — Dark + Light
2. HomePage/Dashboard — both themes
3. SettingsPage — both themes
4. One AI-enabled page (BOQ Activity Generator or Duration Estimator) — both themes
5. DiagnosticsPage — both themes

### Verification
After the upgrade, visually compare each page against its pre-upgrade screenshot. Any rendering differences (pixel offsets, color shifts, text clipping, layout breaks) must be investigated and resolved.

---

## 7. .NET 8.0 SDK Requirement

### Decision
Build environment must have .NET 8.0 SDK installed.

### Current Status
Check if CI/CD and development machines have .NET 8.0 SDK:
```powershell
dotnet --list-sdks
```

If not present, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Minimum Version
.NET 8.0 SDK 8.0.100 or later.
