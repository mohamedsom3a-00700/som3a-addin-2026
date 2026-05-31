# Icon Mapping: Fluent Icon & Control Migration

**Date**: 2026-05-31
**Branch**: `032-fluent-icon-control-migration`

## SidebarRegistrationService Icon Names

All 21 unique icon names from `SidebarRegistrationService.cs` mapped to `FluentIcons.Common.Symbol`:

| Current Icon Name | FluentIcons Symbol | Valid? | Notes |
|-------------------|-------------------|--------|-------|
| `ChartTimelineVariant` | `Symbol.ChartTimelineVariant` | Yes | Planning category |
| `Compare` | `Symbol.Compare` | Yes | Primavera Compare |
| `FileDocument` | `Symbol.Document` | Yes | Renamed from FileDocument |
| `FileCode` | `Symbol.Code` | Yes | Renamed from FileCode |
| `FileTree` | `Symbol.ListTree` | Yes | Renamed from FileTree |
| `Creation` | `Symbol.Add` | Yes | Used twice (duplicate) |
| `PencilRuler` | `Symbol.Drawer` | Yes | Renamed from PencilRuler |
| `Connection` | `Symbol.Link` | Yes | Renamed from Connection |
| `Timer` | `Symbol.History` | Yes | Renamed from Timer |
| `Routes` | `Symbol.Routes` | Yes | Direct match |
| `LinkVariant` | `Symbol.LinkMultiple` | Yes | Renamed from LinkVariant |
| `FileTable` | `Symbol.Table` | Yes | Renamed from FileTable |
| `Tag` | `Symbol.Tag` | Yes | Direct match |
| `Palette` | `Symbol.Colors` | Yes | Renamed from Palette |
| `FormatPaint` | `Symbol.PaintBrush` | Yes | Renamed from FormatPaint |
| `TableMergeCells` | `Symbol.TableMergeCells` | Yes | Direct match |
| `Home` | `Symbol.Home` | Yes | Direct match |
| `Cog` | `Symbol.Settings` | Yes | Renamed from Cog |
| `Translate` | `Symbol.Translate` | Yes | Direct match |
| `MonitorDashboard` | `Symbol.System` | Yes | Renamed from MonitorDashboard |

## Dashboard WidgetViewModel Codepoints

All 9 widget codepoints mapped to `FluentIcons.Common.Symbol`:

| Current Codepoint | FluentIcons Symbol | Widget Class |
|-------------------|-------------------|--------------|
| `\U000F05D2` | `Symbol.Info` | VersionWidgetViewModel |
| `\U000F0068` | `Symbol.Bot` | AIProviderStatusWidgetViewModel |
| `\U000F0417` | `Symbol.Lightning` | QuickActionsWidgetViewModel |
| `\U000F0445` | `Symbol.Puzzle` | PluginStatusWidgetViewModel |
| `\U000F0520` | `Symbol.Gauge` | PerformanceSummaryWidgetViewModel |
| `\U000F0209` | `Symbol.Bug` | DiagnosticsSummaryWidgetViewModel |
| `\U000F0214` | `Symbol.Folder` | RecentProjectsWidgetViewModel |
| `\U000F024B` | `Symbol.Wrench` | RecentToolsWidgetViewModel |
| `\U000F0117` | `Symbol.ArrowSync` | UpdatesWidgetViewModel |

## SettingsViewModel Icon Names

| Current Icon Name | FluentIcons Symbol | Valid? |
|-------------------|-------------------|--------|
| `Palette` | `Symbol.Colors` | Yes |
| `Speedometer` | `Symbol.Gauge` | Yes |
| `Human` | `Symbol.Person` | Yes |
| `ChartBar` | `Symbol.ChartBar` | Yes |
| `FileExcel` | `Symbol.Table` | Yes |
| `Puzzle` | `Symbol.Puzzle` | Yes |
| `Sitemap` | `Symbol.Org` | Yes |
| `Robot` | `Symbol.Bot` | Yes |

## DiagnosticsPage XAML Codepoints

| Current Codepoint | FluentIcons Symbol |
|-------------------|-------------------|
| `&#xF0440;` | `Symbol.ClipboardCheck` |
| `&#xF0509;` | `Symbol.Settings` |
| `&#xF035B;` | `Symbol.Desktop` |
| `&#xF0219;` | `Symbol.FolderOpen` |
| `&#xF0417;` | `Symbol.Lightning` |
| `&#xF0189;` | `Symbol.ChartMultiple` |

## WidgetCard Error Icon

| Current Codepoint | FluentIcons Symbol |
|-------------------|-------------------|
| `&#xF026;` | `Symbol.Warning` |
