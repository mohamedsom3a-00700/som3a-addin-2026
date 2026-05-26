# Implementation Plan: Theme Expansion

**Branch**: `017-theme-expansion` | **Date**: 2026-05-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/017-theme-expansion/spec.md`

## Summary

Enhance the existing Fluent theme engine with Material Design icon and control integration (PackIcon, DialogHost, Slider, ToggleButton, Chip), Shell workspace background image with DWM blur, dynamic font family switching, unified window control buttons, expanded accent picker (color wheel + hex input + variant generation), and WCAG 2.1 AA contrast compliance for both dark and light themes.

## Technical Context

**Language/Version**: C# 7.3, .NET Framework 4.8 (WpfApp2 WPF host only — no new .NET 8.0 projects)

**Primary Dependencies**:
- **MaterialDesignThemes** (NuGet 5.x) — PackIcon, DialogHost, Slider, ToggleButton, Chip controls. Authorized per ADR-006.
- **Win32 DWM API** (`dwmapi.dll`) — `SetWindowCompositionAttribute` with `ACCENT_ENABLE_BLURBEHIND` for background blur
- **System.Windows.Media** — `Fonts.SystemFontFamilies` for font enumeration; `ColorHelper` for HSV↔RGB conversion
- **Existing**: ThemeManager (singleton), ModernWindow, WindowRenderModeDetector, ThemeResources.xaml loading order

**Storage**: `Properties.Settings.Default` — new keys for `BackgroundImagePath`, `BackgroundBlurIntensity`, `SelectedFontFamily`, `WindowBackdropStyle` (extends existing `SelectedTheme`, `AccentColor`)

**Testing**: Manual UI verification (theme switching, Excel VSTO host), WCAG contrast ratio checker tool, visual regression (screenshot comparison dark/light/Custom)

**Target Platform**: Windows x64, Excel VSTO Add-in host (WPF .NET Framework 4.8)

**Project Type**: Desktop application (Excel VSTO Add-in with WPF UI)

**Performance Goals**:
- Background image load + blur apply: <500ms
- Font change propagation across all windows: <500ms
- Accent color change across all windows: <500ms
- Theme switching (Dark↔Light↔Custom): <1s
- Window control hover/pressed animation: ≤200ms

**Constraints**:
- Must not break existing VSTO rendering safety (Constitution §X)
- All themeable properties use `{DynamicResource}` (Constitution §III)
- Theme mutations route through ThemeManager exclusively (Constitution §IV)
- No inline DropShadowEffect (Constitution §XII)
- WindowChrome remains primary rendering strategy (Constitution §XI)
- All animations ≤200ms (Constitution §IX)
- Material Design integration limited to icons, dialogs, sliders, toggles, chips per ADR-006

**Scale/Scope**: ~8 surfaces affected (Shell workspace, Settings page, Appearance panel, Custom theme settings, ModernWindow template, WindowButtonStyles, ThemeManager, Properties.Settings)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — New resource dictionaries for Material styles, background effects, and font tokens remain in isolated files under Theme/ directory. No monolithic dictionaries.
- [x] **III. DynamicResource-Only** — All new themeable resources (Material icon brushes, background image brush, font family resources) use `{DynamicResource}`. No StaticResource for brushes, colors, borders, or effects.
- [x] **IV. Runtime Theme Mutation Governance** — Background image, blur, font, and accent variant changes all route through ThemeManager. `BackgroundSettings` and `FontSettings` mutations call `ThemeManager.SetResource()`.
- [x] **IX. Animation Governance** — Window control hover transitions use existing 100ms CubicEase pattern. All new animations (accent preview, font update) stay within ≤200ms. Reduced motion respected.
- [x] **X. Excel Rendering Safety** — DWM blur is Win32-level and Excel-host-safe. Background image rendering uses WPF ImageBrush within Shell ContentControl (no transparency issues). Safe mode disables blur and falls back to solid backdrop.
- [x] **XI. WindowChrome Enforcement** — ModernWindow remains the base class. Window control button refinements are template changes within ModernWindow.xaml, not new window types.
- [x] **XII. Centralized Effects** — No new DropShadowEffect definitions needed. Accent variant glow already handled by Glow.xaml. Blur is Win32 DWM, not WPF BitmapEffect.
- [x] **XIV. No Third-Party UI Frameworks** — MaterialDesignThemes usage is limited to PackIcon, DialogHost, Slider, ToggleButton, Chip per ADR-006. No MaterialDesignInXaml theme takeover. Existing Fluent tokens preserved.
- [x] **XV. Resource Loading Order** — New Material icon dictionary inserted after Control Styles, before ModernWindow.xaml. New Material control style dictionary inserted within Controls/ section. Background/font resource dictionary inserted after ThemeResources aggregator.
- [x] **XVI. Theme Safety & Recovery** — Fallback recovery: missing background image → solid backdrop; missing font → Segoe UI; Material icons fail → text labels; DWM blur unavailable → sharp background.

## Project Structure

### Documentation (this feature)

```text
specs/017-theme-expansion/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
WpfApp2/
├── Theme/
│   ├── Base/
│   │   ├── Colors.xaml              # (modified) Add Material + Font token sections
│   │   └── ComponentTokens.xaml     # (modified) Add icon size, blur radius tokens
│   ├── Controls/
│   │   ├── WindowButtonStyles.xaml   # (modified) Unified button styles
│   │   ├── MaterialControls.xaml    # NEW — Material Slider, ToggleButton, Chip styles
│   │   └── MaterialIcons.xaml       # NEW — PackIcon style + brush bindings
│   ├── Effects/
│   │   └── Backdrop.xaml            # NEW — Background image, blur effect resources
│   ├── Typography.xaml              # (modified) Add DynamicResource font family
│   ├── ModernWindow.xaml            # (modified) Refined title bar drag region + button layout
│   ├── ThemeResources.xaml          # (modified) Add new dictionaries in loading order
│   └── MaterialIntegration.xaml     # NEW — MaterialDesignThemes resource import + theme bridging
├── Controls/
│   ├── ModernWindow.cs              # (modified) Add WindowBackdrop DP, new BackgroundSettings/FontSettings properties
│   ├── ColorWheel.cs                # NEW — HSV color wheel custom control
│   └── FontPreview.cs               # NEW — Font preview thumbnail user control
├── Services/
│   ├── ThemeManager.cs              # (modified) Add BackgroundApply, FontApply, AccentVariantGenerate methods
│   ├── DwmBlurService.cs            # NEW — Win32 DWM interop (SetWindowCompositionAttribute)
│   └── FontEnumerator.cs            # NEW — System font enumeration with Arabic font detection
├── Views/
│   ├── AppearancePanel.xaml         # (modified) Add color wheel, hex input, font selector sections
│   └── CustomThemeSettings.xaml     # NEW — Background image picker, blur slider, font preview
├── ViewModels/
│   ├── SettingsViewModel.cs         # (modified) Add background/font/accent variant properties
│   └── CustomThemeViewModel.cs      # NEW — Background image, blur, font selection state
└── Properties/
    └── Settings.settings            # (modified) Add BackgroundImagePath, BackgroundBlurIntensity, SelectedFontFamily, WindowBackdropStyle

```

**Structure Decision**: All work stays within the existing `WpfApp2` project. No new .NET 8.0 class libraries needed. The Theme/ directory gains new resource dictionary files following the existing modular pattern. New controls (ColorWheel, FontPreview) follow the Controls/ pattern of existing custom controls. New services (DwmBlurService, FontEnumerator) follow the Services/ pattern.

## Complexity Tracking

> No Constitution violations to justify. All checks passed.
