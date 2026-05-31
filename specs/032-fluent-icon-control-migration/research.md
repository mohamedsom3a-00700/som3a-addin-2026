# Research: Fluent Icon & Control Migration

**Date**: 2026-05-31
**Branch**: `032-fluent-icon-control-migration`

## Critical Discovery: MaterialDesign Already Removed

The original Phase 3 plan assumed MaterialDesign was still present. **Exhaustive search of the entire `WpfApp2/` codebase confirms zero MaterialDesign references** of any kind:

- `materialDesign:PackIcon` → **0 matches**
- `MaterialDesignFlatButton` → **0 matches**
- `MaterialDesignScrollViewer` → **0 matches**
- `MaterialDesignLinearProgressBar` → **0 matches**
- `MaterialDesignFont` → **0 matches**
- `materialDesign:Chip` → **0 matches**
- `MaterialIconConverter.cs` → **File does not exist**
- `MaterialDesignBody/Paper/CardBackground/TextBoxBorder` → **0 matches**
- `MaterialDesignInXamlToolkit` NuGet package → **Not referenced**

**Conclusion**: The MaterialDesign migration was completed in a prior phase (likely Phase 1C or earlier). FR-001 through FR-009, FR-014 are already satisfied.

## Current Icon System Architecture

The codebase has **three distinct icon patterns**, none using `FluentIcons.WPF`:

### Pattern 1: Unicode Codepoints as TextBlock

Used in `WidgetCardStyles.xaml` and `DiagnosticsPage.xaml`:

```xml
<TextBlock Text="{TemplateBinding Icon}" FontFamily="Segoe UI" FontSize="18" />
```

ViewModels set icons as Unicode escape strings:
```csharp
Icon = "\U000F05D2";  // Fluent 2 codepoint
```

**Files affected**: `Controls/WidgetCardStyles.xaml`, `Views/DiagnosticsPanel.xaml`, all `WidgetViewModel` subclasses.

### Pattern 2: String Icon Names via NavigationService (Ignored)

`SidebarRegistrationService.cs` registers pages with Fluent 2 icon kind names:
```csharp
RegisterPage("Planning", typeof(ProjectAnalysisPage), "project-analysis", "Project Analysis", "ChartTimelineVariant", 10);
```

**Problem**: `SidebarControl.xaml` ignores the `Icon` property entirely and renders a hardcoded bullet:
```xml
<TextBlock Text="●" FontSize="14" />
```

### Pattern 3: Segoe MDL2 Assets Font

Used in `SettingsPanelStyles.xaml` for the settings panel's own sidebar:
```xml
<TextBlock Text="{Binding Icon}" FontFamily="Segoe MDL2 Assets" FontSize="14" />
```

## Package Status

| Package | Version | Status |
|---------|---------|--------|
| `FluentIcons.WPF` | 1.1.293 | Installed but **completely unused** (dead dependency) |
| `Wpf.Ui` | — | **Not installed** |
| `CommunityToolkit.Mvvm` | 8.4.0 | Installed and in use |

## Decision: Revised Plan Scope

Since MaterialDesign is already gone, Phase 3 should focus on:

1. **Activating FluentIcons.WPF** — Replace Unicode codepoints and hardcoded bullets with proper `FluentIcon` controls
2. **Creating FluentIconConverter** — String → FluentIcon resolution for sidebar and dynamic icon scenarios
3. **Fixing SidebarControl** — Replace `Text="●"` with actual `FluentIcon` bound to registered icon names
4. **WPF-UI decision** — Defer to Phase 8 (Settings Redesign) since no MaterialDesign controls need replacing. The custom theme engine is mature and functional.

## Alternatives Considered

| Alternative | Rationale for Rejection |
|-------------|------------------------|
| Add WPF-UI controls now | No MaterialDesign controls to replace; custom templates are functional. Better suited for Phase 8 settings redesign. |
| Keep Unicode codepoints | Works but bypasses the FluentIcons.WPF package entirely; no icon metadata, sizing control, or theme-aware coloring. |
| Use Segoe MDL2 Assets everywhere | Platform-specific, limited icon set, not theme-aware. |

## Recommendations

1. **FR-010 (WPF-UI pilot)**: Defer to Phase 8. No compatibility risk to assess since no MaterialDesign controls exist.
2. **FR-011 (fallback documentation)**: Not needed — no WPF-UI integration attempted in this phase.
3. **FR-012 (icon mapping)**: The SidebarRegistrationService already uses Fluent 2 icon kind names. The mapping is already done.
4. **FR-013 (all pages render)**: Still applicable — verify all pages use FluentIcons consistently after migration.
5. **FR-015 (VSTO smoke test)**: Still applicable — verify no regressions from icon changes.
