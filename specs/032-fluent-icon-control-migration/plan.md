# Implementation Plan: Fluent Icon & Control Migration

**Branch**: `032-fluent-icon-control-migration` | **Date**: 2026-05-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/032-fluent-icon-control-migration/spec.md`

## Summary

Activate the `FluentIcons.WPF` package (already installed but unused) to replace the current ad-hoc icon system with proper Fluent 2 icon controls. **MaterialDesign has already been fully removed** from the codebase — zero references remain. The actual work is: (1) create `FluentIconConverter`, (2) replace Unicode codepoints and hardcoded bullets with `FluentIcon` controls, (3) fix `SidebarControl.xaml` to use registered icon names, (4) replace Segoe MDL2 Assets font usage.

## Technical Context

**Language/Version**: C# 14.0, .NET 8.0-windows
**Primary Dependencies**: FluentIcons.WPF 1.1.293, CommunityToolkit.Mvvm 8.4.0, WPF (custom theme engine)
**Storage**: N/A (no persistence changes)
**Testing**: VSTO smoke test, manual visual verification, `dotnet build` success
**Target Platform**: Windows desktop (WPF), Excel VSTO host
**Project Type**: Desktop application (WPF + VSTO add-in)
**Performance Goals**: No measurable performance change (icon rendering is lightweight)
**Constraints**: Must pass VSTO smoke test; must not break Dark/Light theme switching; must use DynamicResource for themeable properties
**Scale/Scope**: 22 Pages, 12 Views, 1 SidebarControl, ~15 WidgetViewModels

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Feature introduces no monolithic dictionaries; FluentIcons namespace added to existing ThemeResources.xaml aggregator.
- [x] **III. DynamicResource-Only** — No StaticResource used for themeable brushes/colors. FluentIcon Foreground uses `{DynamicResource Brush.TextPrimary}`.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation path goes through ThemeManager exclusively. No direct brush mutation from icon controls.
- [x] **IX. Animation Governance** — No animations introduced. Icon rendering is static.
- [x] **X. Excel Rendering Safety** — WindowRenderModeDetector unchanged. Icon controls are lightweight and don't affect rendering pipeline.
- [x] **XI. WindowChrome Enforcement** — No new windows introduced. Existing ModernWindow inheritance preserved.
- [x] **XII. Centralized Effects** — No inline effects. Icon rendering uses no DropShadowEffect.
- [x] **XV. Resource Loading Order** — FluentIcons namespace added to ThemeResources.xaml without changing load order.

**All gates pass. No violations.**

## Project Structure

### Documentation (this feature)

```text
specs/032-fluent-icon-control-migration/
├── plan.md              # This file
├── research.md          # Phase 0 output (completed)
├── data-model.md        # Phase 1 output (completed)
├── quickstart.md        # Phase 1 output (completed)
├── contracts/           # Phase 1 output (completed)
│   └── fluent-icon-converter.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── spec.md              # Feature specification
```

### Source Code (repository root)

```text
WpfApp2/
├── Converters/
│   └── FluentIconConverter.cs          # NEW — String → Symbol converter
├── Controls/
│   └── Shell/
│       └── SidebarControl.xaml         # MODIFY — Replace "●" with FluentIcon
├── Views/
│   ├── DiagnosticsPanel.xaml           # MODIFY — Replace TextBlock icon with FluentIcon
│   └── SettingsPanelStyles.xaml        # MODIFY — Replace Segoe MDL2 Assets with FluentIcon
├── Controls/
│   └── WidgetCardStyles.xaml           # MODIFY — Replace TextBlock icon with FluentIcon
├── Theme/
│   └── ThemeResources.xaml             # MODIFY — Add FluentIcons namespace
└── Services/
    └── SidebarRegistrationService.cs   # UNCHANGED — Icon names already Fluent 2 compatible
```

**Structure Decision**: Single WPF project. No structural changes. New converter file added to existing `Converters/` directory.

## Implementation Steps

### Step 1: Create FluentIconConverter (30 min)

Create `WpfApp2/Converters/FluentIconConverter.cs`:

- Implement `IValueConverter`
- `Convert()`: Parse string → `FluentIcons.Common.Symbol` enum → return `FluentIcon` control
- Fallback: `Symbol.ErrorCircleHalfFilled` for unknown names, `Symbol.Error` for null
- Diagnostic logging via `System.Diagnostics.Trace` for unknown icon names
- **Test**: Build succeeds; converter resolves known icon names correctly

### Step 2: Fix SidebarControl.xaml (20 min)

Modify `WpfApp2/Controls/Shell/SidebarControl.xaml`:

- Add FluentIcons namespace: `xmlns:fluentIcons="clr-namespace:FluentIcons.WPF;assembly=FluentIcons.WPF"`
- Replace `<TextBlock Text="●" .../>` with `<fluentIcons:FluentIcon Symbol="{Binding Icon, Converter={StaticResource FluentIconConverter}}" IconSize="Small" />`
- Add `FluentIconConverter` to resource dictionary (App.xaml or SidebarControl resources)
- **Test**: Sidebar renders Fluent 2 icons for all registered pages

### Step 3: Fix WidgetCardStyles.xaml (20 min)

Modify `WpfApp2/Controls/WidgetCardStyles.xaml`:

- Add FluentIcons namespace
- Replace `<TextBlock Text="{TemplateBinding Icon}" FontFamily="Segoe UI" FontSize="18" />` with `<fluentIcons:FluentIcon Symbol="{TemplateBinding Icon, Converter={StaticResource FluentIconConverter}}" IconSize="Medium" />`
- Update `WidgetViewModel` icon assignments from Unicode codepoints to Symbol enum names
- **Test**: All dashboard widgets render Fluent 2 icons

### Step 4: Fix DiagnosticsPanel.xaml (15 min)

Modify `WpfApp2/Views/DiagnosticsPanel.xaml`:

- Add FluentIcons namespace
- Replace any TextBlock icon renderings with `FluentIcon` controls
- **Test**: Diagnostics page renders Fluent 2 icons

### Step 5: Fix SettingsPanelStyles.xaml (15 min)

Modify `WpfApp2/Views/SettingsPanelStyles.xaml`:

- Add FluentIcons namespace
- Replace `FontFamily="Segoe MDL2 Assets"` TextBlock icons with `FluentIcon` controls
- **Test**: Settings panel sidebar renders Fluent 2 icons

### Step 6: Update ThemeResources.xaml (5 min)

Modify `WpfApp2/Theme/ThemeResources.xaml`:

- Add FluentIcons namespace at root level for global access
- **Test**: All pages can reference FluentIcons without per-file namespace declarations

### Step 7: Generate Icon Mapping Table (15 min)

Create `specs/032-fluent-icon-control-migration/icon-mapping.md`:

- Audit all icon names in `SidebarRegistrationService.cs` (24 registrations)
- Audit all icon codepoints in WidgetViewModels
- Map each to FluentIcons.Common.Symbol enum value
- Document any gaps or fallbacks needed
- **Test**: Complete mapping table with zero unmapped icons

### Step 8: Full Verification (20 min)

- `dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug` succeeds
- All 22 Pages + 12 Views render without crashes
- Sidebar shows Fluent 2 icons (not bullets)
- Widget cards show Fluent 2 icons (not Unicode codepoints)
- Settings panel shows Fluent 2 icons (not Segoe MDL2 Assets)
- Dark/Light theme switch preserves icon coloring
- VSTO smoke test passes

## Complexity Tracking

> No Constitution Check violations. No complexity tracking needed.

## Estimated Total Time

~2.5 hours for implementation + verification

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| FluentIcons.WPF API differs from expected | Low | Medium | Check package docs before Step 1; fallback to Unicode codepoints if needed |
| Some icon names don't have FluentIcons equivalents | Low | Low | Use closest match; document in mapping table |
| SidebarControl XAML structure is complex | Medium | Low | Read existing XAML carefully; make minimal targeted changes |
| WidgetViewModel icon assignments use non-standard names | Medium | Low | Audit all ViewModels in Step 3; standardize to FluentIcons Symbol names |
