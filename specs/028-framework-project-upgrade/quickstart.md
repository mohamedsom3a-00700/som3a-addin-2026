# Quickstart: Framework & Project Format Upgrade

**Phase**: 1 — Design
**Date**: 2026-05-30

## Prerequisites

- .NET 8.0 SDK installed (`dotnet --list-sdks`)
- Visual Studio 2022 (or MSBuild 17.8+) with .NET 8.0 workload
- Git branch: `028-framework-project-upgrade`

---

## Step 1: Convert Project to SDK-Style

**File**: `WpfApp2/Som3a_WPF_UI.csproj`

Replace the entire contents with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <OutputType>WinExe</OutputType>
    <AssemblyName>Som3a_WPF_UI</AssemblyName>
    <RootNamespace>Som3a_WPF_UI</RootNamespace>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <!-- Preserve existing assembly info from AssemblyInfo.cs -->
  </PropertyGroup>
</Project>
```

Key changes:
- `Sdk="Microsoft.NET.Sdk"` instead of legacy `ToolsVersion`
- `net8.0-windows` target instead of `net48`
- `<UseWPF>true</UseWPF>` enables WPF references
- Remove all assembly references that were in legacy format (WPF assemblies are implicit)

---

## Step 2: Migrate NuGet to PackageReference

**Delete**: `WpfApp2/packages.config`

**Add to .csproj** (inside `<Project>`):

```xml
  <ItemGroup>
    <PackageReference Include="MaterialDesignColors" Version="3.1.0" />
    <PackageReference Include="MaterialDesignThemes" Version="5.2.0" />
    <PackageReference Include="Microsoft.Web.WebView2" Version="1.0.3912.50" />
    <PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.39" />
    <PackageReference Include="System.Text.Json" Version="8.0.5" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  </ItemGroup>
```

**Version notes**:
- `MaterialDesignThemes` 5.1.0 targets `net48` only; 5.2.0+ adds `net8.0-windows` support
- `System.Text.Json` 6.0.0 works but upgrading to 8.0.x is recommended for performance
- All other packages work at or near current versions

---

## Step 3: Fix Compile Errors

```powershell
dotnet build WpfApp2/Som3a_WPF_UI.csproj
```

Collect errors and fix with minimum changes:

| Common Error | Fix |
|-------------|-----|
| `AppDomain.CreateDomain` | Remove or replace with `AppDomain.CurrentDomain` |
| `BinaryFormatter` | Add `<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>` temporarily, or switch to JSON |
| `System.Configuration` missing | Add `<PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />` |
| Missing WinForms interop | Add `<PackageReference Include="System.Windows.Forms" Version="8.0.0" />` or refactor |

**IMPORTANT**: Fix only what fails to compile. Do not refactor, rename, or restructure.

---

## Step 4: Run Tests

```powershell
dotnet test Tests\Som3a_WPF_UI.Tests\Som3a_WPF_UI.Tests.csproj
dotnet test Tests\Som3a.Infrastructure.Tests\Som3a.Infrastructure.Tests.csproj
```

All tests must pass without any modifications.

---

## Step 5: Audit VSTO Add-in Configuration

Check the VSTO add-in project's `app.config` for binding redirects referencing WPF assemblies. Update redirect version ranges if they reference .NET Framework 4.8 versions that conflict.

---

## Step 6: Visual Regression Check

Compare screenshots of 3-5 key pages (Dark + Light themes) against pre-upgrade captures.

---

## Step 7: Capture Build Baseline

```powershell
Measure-Command { dotnet build WpfApp2/Som3a_WPF_UI.csproj -c Debug }
```

Compare against pre-migration build time. Acceptable: ≤150% of original.

---

## Verification Checklist

- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet restore` resolves all packages without conflicts
- [ ] `dotnet test` passes for both test projects
- [ ] Git diff shows only project format + API fixes (no logic changes)
- [ ] Pre-upgrade vs post-upgrade screenshots show no visual regressions
- [ ] Build time ≤150% of pre-migration baseline
- [ ] VSTO add-in configuration updated (if needed)
