# Contract: Project Format (Build Interface)

**Phase**: 1 — Design
**Date**: 2026-05-30

## Build Entry Point

| Property | Value |
|----------|-------|
| Project file | `WpfApp2/Som3a_WPF_UI.csproj` |
| Build command | `dotnet build WpfApp2/Som3a_WPF_UI.csproj -c Debug` |
| Release build | `dotnet build WpfApp2/Som3a_WPF_UI.csproj -c Release` |
| Restore command | `dotnet restore WpfApp2/Som3a_WPF_UI.csproj` |

## Build Output Contract

```text
Output directory: WpfApp2/bin/<Configuration>/net8.0-windows/
├── Som3a_WPF_UI.exe      — WPF host application executable
├── Som3a_WPF_UI.dll      — WPF host application assembly
├── Som3a_WPF_UI.pdb      — Debug symbols
└── *.dll                  — All dependency assemblies copied locally
```

## Test Entry Points

| Test Project | Command |
|-------------|---------|
| Som3a_WPF_UI.Tests | `dotnet test Tests/Som3a_WPF_UI.Tests/Som3a_WPF_UI.Tests.csproj` |
| Som3a.Infrastructure.Tests | `dotnet test Tests/Som3a.Infrastructure.Tests/Som3a.Infrastructure.Tests.csproj` |

## NuGet Package Contract

The project exposes 6 NuGet dependencies via PackageReference. Post-migration, all must resolve without conflicts:

- MaterialDesignColors
- MaterialDesignThemes
- Microsoft.Web.WebView2
- Microsoft.Xaml.Behaviors.Wpf
- System.Text.Json
- Newtonsoft.Json

## Framework Contract

- **Runtime**: .NET 8.0 (supports Windows 10+)
- **UI Framework**: WPF (built-in)
- **COM Interop**: Via Som3a.Bridge (.NET Standard 2.0) — external to this project
