# Quickstart: Release Candidate & Production Packaging

## Prerequisites

- Windows 10/11 x64 build machine
- Visual Studio 2022 with MSBuild
- WiX Toolset v4 installed
- EV Code Signing certificate (PFX or Azure Key Vault)
- .NET Framework 4.8 SDK
- .NET 8.0 SDK
- All Phase 14–25 features implemented and passing

## Release Pipeline Steps

### 1. Run Final Validation

```powershell
# Execute all 4 validation suites
.\scripts\validate-ui.ps1
.\scripts\validate-plugins.ps1
.\scripts\validate-ai.ps1
.\scripts\validate-excel.ps1
```

Check validation report at `build/validation-report.json`. All 4 areas must pass with zero failures.

### 2. Apply Performance Optimizations

- Enable DataGrid column virtualization in DataGridStyles.xaml
- Audit COM object cleanup across all Excel interop pages
- Add periodic GC on 5-minute timer for long sessions
- Implement lazy page loading in Shell navigation

### 3. Build Release Candidate

```powershell
# Build with release configuration
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Release

# Build .NET 8 libraries
dotnet build Som3a.Domain\Som3a.Domain.csproj -c Release
dotnet build Som3a.AI\Som3a.AI.csproj -c Release
# ... repeat for all .NET 8 projects
```

### 4. Sign Executables

```powershell
# Sign add-in assemblies
signtool sign /fd SHA256 /a /f cert.pfx /p password /tr http://timestamp.digicert.com /td SHA256 WpfApp2\bin\Release\*.dll

# Sign VSTO manifests
mage -Sign WpfApp2\bin\Release\Som3a_WPF_UI.dll.manifest -CertFile cert.pfx -Password password
mage -Update WpfApp2\bin\Release\Som3a_WPF_UI.vsto -AppManifest Som3a_WPF_UI.dll.manifest -CertFile cert.pfx
```

### 5. Create MSI Installer

Build WiX project to produce MSI with:
- Desktop shortcut
- Start Menu integration
- VSTO registry keys (HKCU\Software\Microsoft\Office\Excel\Addins)
- .NET 8 runtime prerequisite via Burn bootstrapper
- MajorUpgrade support for future versions

### 6. Export Documentation

Export all 6 guides:
- User Guide
- Admin Guide
- Plugin SDK Guide
- Architecture Guide
- AI Provider Setup Guide
- Troubleshooting Guide

Each document versioned with SemVer matching the release.

### 7. Release Candidate Checklist

- [ ] Clean build with zero warnings
- [ ] All validation suites pass (UI, plugins, AI, Excel)
- [ ] MSI installs cleanly on Windows 10/11 x64
- [ ] Executables signed with valid certificates
- [ ] Crash recovery captures diagnostics without data loss
- [ ] AI provider fallback activates within 5 seconds
- [ ] Excel export of 10K rows completes under 10 seconds
- [ ] Documentation set complete and versioned
- [ ] Production configuration has error-level logging, 100MB cap
