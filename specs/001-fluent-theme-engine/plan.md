# Implementation Plan: WPF Fluent UI Theme Engine

**Branch**: `002-fluent-theme-engine` | **Date**: 2026-05-19 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-fluent-theme-engine/spec.md`

## Summary

Build a production-grade Fluent Runtime Theme Engine for Som3a Add-in 2026, enabling runtime theme switching (Dark/Light/Custom) with full semantic color editing, live synchronization across all windows, animated transitions ≤200ms, and Excel VSTO-safe rendering via WindowChrome with automatic fallback detection.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8

**Primary Dependencies**: 
- WPF (Windows Presentation Foundation)
- VSTO (Visual Studio Tools for Office) — Excel hosting
- Windows Shell (WindowChrome) — borderless window rendering
- ApplicationSettingsBase — settings persistence

**Storage**: 
- Primary: `Properties/Settings.settings` (SelectedTheme, AccentColor)
- Secondary: JSON file at `%AppData%/Som3a/custom-theme.json` (extended custom colors)

**Testing**: No formal testing framework detected. Build verification via `msbuild`.

**Target Platform**: Windows 10/11, Excel 2016+ VSTO host, .NET Framework 4.8

**Project Type**: Desktop application — WPF with VSTO Excel add-in

**Performance Goals**: 
- Theme switching <200ms
- All animations ≤200ms, GPU-safe
- DataGrid virtualized (500+ rows smooth)
- No frame drops during normal operation

**Constraints**: 
- Excel VSTO hosting — `AllowsTransparency=True` may cause black window rendering on some configurations
- No third-party UI frameworks (per constitution)
- DynamicResource-only for all themeable properties
- No inline DropShadowEffect — centralized in Effects/Shadows.xaml

**Scale/Scope**: 
- ~15 windows to migrate
- 3 built-in themes + Custom theme editor
- ~50 XAML resource files in Theme/ directory
- 8 accent swatch presets for Custom theme

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Theme resources organized into modular dictionaries (Base/, Dark/, Light/, Custom/, Controls/, Effects/). No monolithic dictionaries. Each concern isolated and independently replaceable.
- [x] **III. DynamicResource-Only** — All themeable properties use `{DynamicResource Brush.*}` or `{DynamicResource Key}`. No StaticResource for themeable brushes, colors, borders, effects. Confirmed: Colors.xaml, all control styles, theme dictionaries use DynamicResource.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation exclusively through `ThemeManager` singleton. No direct brush mutation from windows, controls, or viewmodels. Confirmed: ThemeManager.ApplyTheme() is the sole mutation path.
- [x] **IX. Animation Governance** — All animations ≤200ms. Confirmed: Animations.xaml, ButtonStyles.xaml, ThemeCardStyles.xaml use ≤150ms transitions. No heavy transform animations. CubicEase(EaseOut) for GPU-friendly easing.
- [x] **X. Excel Rendering Safety** — WindowRenderModeDetector auto-detects VSTO hosting and activates FallbackSafe mode. Confirmed: WindowRenderModeDetector checks `Helpers.WindowChromeHelper.IsVstoHosted` and extreme DPI (>3.0x) to return FallbackSafe mode.
- [x] **XI. WindowChrome Enforcement** — ModernWindow uses WindowChrome as primary rendering strategy. Confirmed: ModernWindow.cs applies WindowChromeHelper with fallback-safe option when VSTO detected.
- [x] **XII. Centralized Effects** — No inline DropShadowEffect. All effects sourced from Effects/Shadows.xaml and Effects/Glow.xaml. Confirmed: Shadow.Popup, Shadow.Card, Glow.Focus, Glow.ThemeCard.Selected are centralized.
- [x] **XV. Resource Loading Order** — ThemeResources.xaml follows prescribed sequence: Base/Colors → Base/Typography → Base/Spacing → Base/Radius → Effects → Controls → Window Styles.

## Project Structure

### Documentation (this feature)

```text
specs/001-fluent-theme-engine/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
WpfApp2/
├── Theme/
│   ├── Base/
│   │   ├── Colors.xaml           ✅ Primitive + Semantic tokens
│   │   ├── Typography.xaml
│   │   ├── Spacing.xaml
│   │   └── Radius.xaml
│   ├── Dark/
│   │   ├── DarkColors.xaml       ✅ Semantic token overrides
│   │   └── DarkTheme.xaml        ✅ Merged dictionary
│   ├── Light/
│   │   ├── LightColors.xaml       ✅ Semantic token overrides
│   │   └── LightTheme.xaml        ✅ Merged dictionary
│   ├── Custom/
│   │   ├── CustomColors.xaml      ✅ 8 accent swatch presets + AccentColorValue
│   │   └── CustomTheme.xaml       ✅ AccentDynamicResource
│   ├── Controls/
│   │   ├── ButtonStyles.xaml     ✅ Trigger-based, ≤150ms
│   │   ├── ComboBoxStyles.xaml   ✅ Popup: AllowsTransparency=False
│   │   ├── DataGridStyles.xaml   ✅ EnableRowVirtualization=True
│   │   ├── ThemeCardStyles.xaml  ✅ ToggleButton-based, scale animation
│   │   ├── AccentSwatchStyles.xaml ✅ Scale animation on hover/selected
│   │   ├── CheckBoxStyles.xaml
│   │   ├── RadioButtonStyles.xaml
│   │   ├── ToggleButtonStyles.xaml
│   │   ├── ScrollViewerStyles.xaml
│   │   ├── TextBoxStyles.xaml
│   │   ├── ScrollBarStyles.xaml
│   │   ├── ListViewStyles.xaml
│   │   └── ... (other controls)
│   ├── Effects/
│   │   ├── Shadows.xaml          ✅ Centralized DropShadowEffect
│   │   ├── Glow.xaml             ✅ DynamicResource AccentColorValue
│   │   └── Animations.xaml       ✅ All ≤200ms
│   ├── ModernWindow.xaml
│   └── ThemeResources.xaml       ✅ Aggregator with loading order documentation
├── Controls/
│   └── ModernWindow.cs           ✅ WindowRenderModeDetector integration
├── Services/
│   ├── ThemeManager.cs           ✅ Singleton, ApplyTheme(), ThemeChanged event
│   ├── WindowRenderModeDetector.cs ✅ VSTO/DPI detection, FallbackSafe mode
│   └── ThemeSettings.cs          ✅ Legacy JSON persistence
├── Views/
│   └── SettingsWindow.xaml       ✅ Theme cards + accent swatches
├── Views/SettingsWindow.xaml.cs  ✅ ThemeManager.ThemeChanged listener
├── App.xaml
└── Properties/Settings.settings  ✅ SelectedTheme, AccentColor
```

**Structure Decision**: Single WPF application (Som3a_WPF_UI.csproj) with modular ResourceDictionary libraries under Theme/. ThemeManager singleton orchestrates runtime switching. WindowRenderModeDetector provides VSTO-safe rendering path.

## Complexity Tracking

> N/A — No Constitution violations requiring justification. All gates pass.

## Research Phase Output

After Phase 0, research.md will document:
- VSM migration strategy for 4 high-priority controls (ComboBox, Button, ToggleButton, ThemeCards)
- Full custom theme editor UX design
- Custom theme persistence (JSON schema)
- Contrast validation approach
- Brush interpolation for animated transitions