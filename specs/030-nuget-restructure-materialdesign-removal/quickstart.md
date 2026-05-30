# Quickstart: NuGet Restructure & MaterialDesign Removal

**Date**: 2026-05-30
**Feature**: NuGet Restructure & MaterialDesign Removal (Phase 1C)

## Prerequisites

- Phase 1A (Framework Upgrade) and Phase 1B (Out-of-Process Architecture) are complete and stable.
- Branch `fluent/phase-1b` contains the verified state before MaterialDesign removal.
- A new branch `fluent/phase-1c` has been created from `fluent/phase-1b`.
- Visual Studio 2022 or `dotnet` CLI is available.

## Developer Setup

### Step 1: Create Backup

Ensure the `fluent/phase-1b` branch remains untouched as the rollback point:

```powershell
git branch fluent/phase-1c-backup fluent/phase-1b
```

### Step 2: Audit Existing References

Before modifying any files, produce a complete checklist of all MaterialDesign references:

```powershell
# Search XAML files
grep -rn "MaterialDesign\|materialDesign" --include="*.xaml" WpfApp2/

# Search C# files
grep -rn "MaterialDesign" --include="*.cs" WpfApp2/

# Search VSTO project
grep -rn "MaterialDesign" --include="*.config" Som3aAddin/
```

Save the output as `Docs/Audit/phase-1c-removal-audit.md` for reference during fixes.

### Step 3: Remove Packages

In Visual Studio Package Manager or via CLI:

```powershell
# WPF host
dotnet remove WpfApp2/Som3a_WPF_UI.csproj package MaterialDesignThemes
dotnet remove WpfApp2/Som3a_WPF_UI.csproj package MaterialDesignColors

# VSTO add-in (if packages are referenced)
# Note: VSTO may use packages.config; remove via NuGet UI or edit packages.config manually
```

### Step 4: Add New Packages

```powershell
dotnet add WpfApp2/Som3a_WPF_UI.csproj package FluentIcons.WPF
dotnet add WpfApp2/Som3a_WPF_UI.csproj package Wpf.Ui
```

### Step 5: Clean Resource Dictionaries

Remove the following entries from `WpfApp2/Theme/ThemeResources.xaml`:

- `MaterialIntegration.xaml`
- `Theme/Controls/MaterialIcons.xaml`
- `Theme/Controls/MaterialControls.xaml`

Delete these files if they exist as standalone files.

### Step 6: Fix Compilation Errors

Build the project:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

For each build error related to missing MaterialDesign types or resources:

1. Identify the broken reference (e.g., `MaterialDesignBody`, `MaterialDesignPaper`).
2. Replace with the equivalent custom theme engine token:
   - `MaterialDesignBody` → `{DynamicResource TextPrimaryBrush}`
   - `MaterialDesignPaper` → `{DynamicResource WindowBackgroundBrush}`
   - `MaterialDesignCardBackground` → `{DynamicResource CardBackgroundBrush}`
   - `MaterialDesignTextBoxBorder` → `{DynamicResource BorderBrush}`
3. For `MaterialIconConverter` or `MaterialDesign:PackIcon`, remove the reference. Do not replace with Fluent icons in this phase.

### Step 7: Clean VSTO Configuration

Open `Som3aAddin/app.config` and remove any `<bindingRedirect>` entries referencing `MaterialDesignThemes.Wpf` or `MaterialDesignColors.Wpf`.

### Step 8: Verify Build

Run the full build:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

Confirm zero errors and zero warnings related to MaterialDesign.

### Step 9: Smoke Test

Follow the VSTO Smoke Test protocol:

1. Open Excel.
2. Click the ribbon button to launch the WPF shell.
3. Verify the ShellWindow opens and the sidebar renders.
4. Navigate to Home, Settings, and Diagnostics pages.
5. Switch themes (Dark → Light → Dark).
6. Verify no crashes and all pages load.
7. Trigger an Excel cell write via the application bridge.
8. Close Excel and verify the WPF process shuts down gracefully.

### Step 10: Commit

```powershell
git add .
git commit -m "Phase 1C: Remove MaterialDesign, add FluentIcons.WPF + Wpf.Ui"
```

## Troubleshooting

| Issue | Resolution |
|-------|------------|
| Build error: missing `MaterialDesignBody` | Replace with `{DynamicResource TextPrimaryBrush}` |
| Build error: missing `MaterialDesign:PackIcon` | Remove the element or replace with a plain `TextBlock` placeholder |
| Runtime: missing icon shows as blank | Acceptable for this phase; Phase 3 will replace with Fluent icons |
| VSTO load warning about binding redirect | Remove the stale `<bindingRedirect>` from `app.config` |
| WPF-UI conflicts with custom theme | Remove WPF-UI package and fall back to FluentIcons.WPF only |
