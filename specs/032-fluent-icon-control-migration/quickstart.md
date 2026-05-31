# Quickstart: Fluent Icon & Control Migration

**Branch**: `032-fluent-icon-control-migration`
**Date**: 2026-05-31

## What This Feature Does

Activates the `FluentIcons.WPF` package (already installed but unused) to replace the current ad-hoc icon system (Unicode codepoints, hardcoded bullets, Segoe MDL2 Assets) with proper Fluent 2 icon controls across the entire application.

## Prerequisites

- Phases 1A, 1B, 1C, and 2 are complete
- `FluentIcons.WPF` v1.1.293 is installed (verified in `Som3a_WPF_UI.csproj`)
- `CommunityToolkit.Mvvm` v8.4.0 is installed
- Build command: `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug`

## Key Changes

| Area | Before | After |
|------|--------|-------|
| Sidebar icons | Hardcoded `●` bullet | `FluentIcon` bound to registered icon names |
| Widget icons | Unicode codepoints in TextBlock | `FluentIcon` with Symbol enum |
| Settings sidebar | Segoe MDL2 Assets font | `FluentIcon` with Symbol enum |
| Icon resolution | No converter | `FluentIconConverter` (string → Symbol) |
| Fallback behavior | Missing/blank icons | `Symbol.Error` + diagnostic warning |

## Files Modified

### New Files
- `Converters/FluentIconConverter.cs` — String → FluentIcons.Symbol converter
- `specs/032-fluent-icon-control-migration/icon-mapping.md` — Complete icon mapping table

### Modified Files
- `Controls/Shell/SidebarControl.xaml` — Replace `Text="●"` with `FluentIcon`
- `Controls/WidgetCardStyles.xaml` — Replace TextBlock icon with `FluentIcon`
- `Views/DiagnosticsPanel.xaml` — Replace TextBlock icon with `FluentIcon`
- `Views/SettingsPanelStyles.xaml` — Replace Segoe MDL2 Assets with `FluentIcon`
- `Theme/ThemeResources.xaml` — Add FluentIcons namespace if needed

### Unchanged Files
- `Services/SidebarRegistrationService.cs` — Icon names already Fluent 2 compatible
- All ViewModel icon assignments — Unicode codepoints replaced with Symbol enum values

## Verification

1. `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds
2. All sidebar icons render as Fluent 2 icons (not bullets)
3. All widget icons render as Fluent 2 icons (not Unicode codepoints)
4. Theme switch (Dark/Light) preserves icon coloring
5. VSTO smoke test passes (ribbon → Shell → sidebar → navigate → theme switch → Excel write)
6. Zero "MaterialDesign" references remain (already satisfied)
7. Zero `Segoe MDL2 Assets` references remain after migration

## Risk: Low

- No MaterialDesign controls to replace (already removed)
- No new NuGet packages needed (FluentIcons.WPF already installed)
- Custom theme engine retained (no WPF-UI integration needed in this phase)
- Sidebar icon names already use Fluent 2 naming convention
