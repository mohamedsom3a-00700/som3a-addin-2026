# Icon Audit: Fluent Icon & Control Migration

**Date**: 2026-05-31
**Branch**: `032-fluent-icon-control-migration`

## Summary

| Category | Count | Files | Severity |
|----------|-------|-------|----------|
| Segoe MDL2 Assets font usage | 2 | 1 | Medium |
| Fluent codepoints in XAML (`&#xF...`) | 9 | 2 | High |
| C# Fluent codepoints (`\U000F...`) | 9 | 9 | High |
| C# Fluent icon name strings | 28 | 2 | High |
| Bullet characters (●) | 9 | 3 | Medium |
| Path geometry icons (window chrome) | 10 | 4 | None |
| Toast emoji icons | 4 | 1 | Low |
| Shell emoji icons | 2 | 1 | Low |
| FluentIcons.WPF package | 1 | 1 | Unused |

## Category 1: Segoe MDL2 Assets Font Usage

| File | Line | Content |
|------|------|---------|
| `Theme/Controls/SettingsPanelStyles.xaml` | 75 | `FontFamily="Segoe MDL2 Assets"` — SidebarIconTemplate |
| `Theme/Controls/SettingsPanelStyles.xaml` | 94 | `FontFamily="Segoe MDL2 Assets"` — SidebarFullTemplate |

## Category 2: Fluent Codepoints in XAML

### WidgetCard error icon (wrong font — Segoe UI doesn't contain Fluent codepoints)

| File | Line | Codepoint |
|------|------|-----------|
| `Controls/WidgetCardStyles.xaml` | 89 | `&#xF026;` (Warning) |

### DiagnosticsPage widget card icons

| File | Line | Codepoint |
|------|------|-----------|
| `Pages/DiagnosticsPage.xaml` | 32 | `&#xF0440;` |
| `Pages/DiagnosticsPage.xaml` | 41 | `&#xF0509;` |
| `Pages/DiagnosticsPage.xaml` | 54 | `&#xF035B;` |
| `Pages/DiagnosticsPage.xaml` | 71 | `&#xF0219;` |
| `Pages/DiagnosticsPage.xaml` | 91 | `&#xF0417;` |
| `Pages/DiagnosticsPage.xaml` | 101 | `&#xF0189;` |

### Shell window emoji icons

| File | Line | Codepoint |
|------|------|-----------|
| `Controls/Shell/ShellWindow.xaml` | 55 | `&#x1F310;` (Globe) |
| `Controls/Shell/ShellWindow.xaml` | 61 | `&#x2600;` (Sun) |

## Category 3: Bullet Character Icons

### Sidebar navigation items

| File | Line | Content |
|------|------|---------|
| `Controls/Shell/SidebarControl.xaml` | 86 | `Text="&#x25CF;"` — sidebar item bullet |

### SettingsPage category indicators

| File | Line | Content |
|------|------|---------|
| `Pages/SettingsPage.xaml` | 89 | `Text="●"` — settings category indicator |

### ShellStyles category icon triggers

| File | Line | Content |
|------|------|---------|
| `Theme/ShellStyles.xaml` | 87 | `Text="●"` — default CategoryIcon |
| `Theme/ShellStyles.xaml` | 115-130 | 6 category trigger setters — all `Value="●"` |

## Category 4: C# Unicode Codepoint Icon Assignments

### Dashboard WidgetViewModels (9 files)

| File | Line | Class | Codepoint |
|------|------|-------|-----------|
| `ViewModels/Dashboard/VersionWidgetViewModel.cs` | 22 | VersionWidgetViewModel | `\U000F05D2` |
| `ViewModels/Dashboard/AIProviderStatusWidgetViewModel.cs` | 28 | AIProviderStatusWidgetViewModel | `\U000F0068` |
| `ViewModels/Dashboard/QuickActionsWidgetViewModel.cs` | 16 | QuickActionsWidgetViewModel | `\U000F0417` |
| `ViewModels/Dashboard/PluginStatusWidgetViewModel.cs` | 35 | PluginStatusWidgetViewModel | `\U000F0445` |
| `ViewModels/Dashboard/PerformanceSummaryWidgetViewModel.cs` | 25 | PerformanceSummaryWidgetViewModel | `\U000F0520` |
| `ViewModels/Dashboard/DiagnosticsSummaryWidgetViewModel.cs` | 32 | DiagnosticsSummaryWidgetViewModel | `\U000F0209` |
| `ViewModels/Dashboard/RecentProjectsWidgetViewModel.cs` | 19 | RecentProjectsWidgetViewModel | `\U000F0214` |
| `ViewModels/Dashboard/RecentToolsWidgetViewModel.cs` | 23 | RecentToolsWidgetViewModel | `\U000F024B` |
| `ViewModels/Dashboard/UpdatesWidgetViewModel.cs` | 26 | UpdatesWidgetViewModel | `\U000F0117` |

### SettingsViewModel icon name strings

| File | Line | Icon Value |
|------|------|------------|
| `ViewModels/SettingsViewModel.cs` | 685 | `"Palette"` |
| `ViewModels/SettingsViewModel.cs` | 693 | `"Speedometer"` |
| `ViewModels/SettingsViewModel.cs` | 701 | `"Human"` |
| `ViewModels/SettingsViewModel.cs` | 709 | `"ChartBar"` |
| `ViewModels/SettingsViewModel.cs` | 717 | `"FileExcel"` |
| `ViewModels/SettingsViewModel.cs` | 725 | `"Puzzle"` |
| `ViewModels/SettingsViewModel.cs` | 733 | `"Sitemap"` |
| `ViewModels/SettingsViewModel.cs` | 741 | `"Robot"` |

### SidebarRegistrationService icon name strings (21 unique)

| File | Line | Icon Value |
|------|------|------------|
| `Services/SidebarRegistrationService.cs` | 27 | `"ChartTimelineVariant"` |
| `Services/SidebarRegistrationService.cs` | 28 | `"Compare"` |
| `Services/SidebarRegistrationService.cs` | 29 | `"FileDocument"` |
| `Services/SidebarRegistrationService.cs` | 30 | `"FileCode"` |
| `Services/SidebarRegistrationService.cs` | 31 | `"FileTree"` |
| `Services/SidebarRegistrationService.cs` | 32 | `"Creation"` |
| `Services/SidebarRegistrationService.cs` | 33 | `"PencilRuler"` |
| `Services/SidebarRegistrationService.cs` | 34 | `"Creation"` (duplicate) |
| `Services/SidebarRegistrationService.cs` | 35 | `"Connection"` |
| `Services/SidebarRegistrationService.cs` | 36 | `"Timer"` |
| `Services/SidebarRegistrationService.cs` | 37 | `"Routes"` |
| `Services/SidebarRegistrationService.cs` | 38 | `"LinkVariant"` |
| `Services/SidebarRegistrationService.cs` | 39 | `"FileTable"` |
| `Services/SidebarRegistrationService.cs` | 40 | `"Tag"` |
| `Services/SidebarRegistrationService.cs` | 41 | `"Palette"` |
| `Services/SidebarRegistrationService.cs` | 42 | `"FormatPaint"` |
| `Services/SidebarRegistrationService.cs` | 43 | `"TableMergeCells"` |
| `Services/SidebarRegistrationService.cs` | 44 | `"Home"` |
| `Services/SidebarRegistrationService.cs` | 45 | `"Cog"` |
| `Services/SidebarRegistrationService.cs` | 46 | `"Translate"` |
| `Services/SidebarRegistrationService.cs` | 47 | `"MonitorDashboard"` |

## Key Finding: Systemic Font/Codepoint Mismatch

The codebase has 3 different icon identity systems coexisting:

1. **Fluent icon enum name strings** (28 instances) — Rendered through Segoe MDL2 Assets (wrong font)
2. **Fluent icon Unicode codepoints** (18 instances) — Rendered through Segoe UI (wrong font)
3. **Legacy Unicode characters** (22 instances) — Render correctly but inconsistent

The `FluentIcons.WPF` NuGet package is installed but never used anywhere.
