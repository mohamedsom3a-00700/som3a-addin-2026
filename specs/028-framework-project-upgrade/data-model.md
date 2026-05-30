# Data Model: Framework & Project Format Upgrade

**Phase**: 1 — Design
**Date**: 2026-05-30
**Spec**: [spec.md](./spec.md)

## Overview

This feature introduces no new runtime data entities (databases, records, or services). The "data model" for this infrastructure upgrade consists of the project configuration files, NuGet package references, and build output format.

---

## 1. Project Configuration Model

### 1.1 SDK-Style .csproj Schema

```text
Som3a_WPF_UI.csproj
├── SDK attribute: "Microsoft.NET.Sdk"
├── TargetFramework: "net8.0-windows"
├── UseWPF: true
├── OutputType: WinExe (for WPF application)
├── PackageReferences (6 packages)
│   ├── MaterialDesignColors           @ minimum .NET 8.0 compatible version
│   ├── MaterialDesignThemes           @ minimum .NET 8.0 compatible version
│   ├── Microsoft.Web.WebView2         @ minimum .NET 8.0 compatible version
│   ├── Microsoft.Xaml.Behaviors.Wpf   @ minimum .NET 8.0 compatible version
│   ├── System.Text.Json              @ minimum .NET 8.0 compatible version
│   └── Newtonsoft.Json               @ minimum .NET 8.0 compatible version
├── ProjectReferences
│   └── (existing references retained)
└── Removed: packages.config
```

### 1.2 PackageReference Schema

Each PackageReference entry:

```text
PackageReference
├── Include: PackageId (string, required)
├── Version: SemVer version (string, required)
└── (other attributes: PrivateAssets, IncludeAssets, ExcludeAssets — as needed)
```

### 1.3 Binding Redirect Model (VSTO Add-in app.config)

```text
VSTO Add-in app.config
└── runtime
    └── assemblyBinding
        └── dependentAssembly (per WPF assembly)
            ├── assemblyIdentity
            │   ├── name: AssemblyName (string)
            │   ├── publicKeyToken: Token (string)
            │   └── culture: "neutral"
            └── bindingRedirect
                ├── oldVersion: "old-range"
                └── newVersion: "new-version"
```

---

## 2. Build Output Model

```text
Build Output (bin\Release\net8.0-windows\)
├── Som3a_WPF_UI.exe      (WPF host executable)
├── Som3a_WPF_UI.dll      (WPF host assembly)
└── (supporting DLLs, .pdb files, config files)
```

---

## 3. Validation Rules

| Rule | Entity | Constraint |
|------|--------|-----------|
| VR-001 | .csproj SDK | MUST be `Microsoft.NET.Sdk` |
| VR-002 | TargetFramework | MUST be `net8.0-windows` |
| VR-003 | UseWPF | MUST be `true` |
| VR-004 | PackageReference versions | MUST be minimum compatible version for .NET 8.0, never lower |
| VR-005 | No dropped packages | All 6 existing packages in packages.config MUST have corresponding PackageReference |
| VR-006 | No new packages | No additional NuGet packages beyond the 6 existing ones |
| VR-007 | Binding redirects | VSTO add-in binding redirects MUST be audited and updated |
| VR-008 | Test compatibility | All existing tests MUST pass without modification |
| VR-009 | Zero logic changes | Git diff must show only project format + API compatibility fixes |

---

## 4. State Transitions

```text
Pre-migration state:
    Som3a_WPF_UI.csproj (legacy, net48, packages.config)
    → Build succeeds on .NET Framework 4.8 SDK
    → Tests pass against net48 output
    → VSTO loads WpfApp2 in-process

During migration:
    Modified .csproj (SDK-style, net8.0-windows, PackageReference)
    → dotnet restore: resolve all packages
    → dotnet build: resolve compile errors iteratively
    → dotnet test: verify test suite passes

Post-migration state:
    Som3a_WPF_UI.csproj (SDK-style, net8.0-windows, PackageReference)
    → Build succeeds on .NET 8.0 SDK
    → Tests pass against net8.0 output
    → VSTO loading behavior: see Phase 1B (out-of-process)
```
