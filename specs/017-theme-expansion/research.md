# Research: Theme Expansion

**Feature**: 017-theme-expansion
**Date**: 2026-05-26
**Status**: Complete

## R1: Material Design Integration Strategy

**Decision**: Integrate MaterialDesignThemes NuGet package as an additive resource layer — not a theme takeover. A new `MaterialIntegration.xaml` resource dictionary imports `MaterialDesignTheme.Defaults.xaml` and defines bridging resources that map Material control brushes to existing Fluent semantic tokens. Material controls (PackIcon, Slider, ToggleButton, Chip) are styled via a new `Controls/MaterialControls.xaml` dictionary using `BasedOn` and explicit template overrides.

**Rationale**: The project constitution (XIV) prohibits full third-party UI frameworks but ADR-006 explicitly authorizes Material Design for icons, dialogs, and selected controls. Importing MaterialDesignThemes as a resource dictionary (not a merged theme that replaces the application's resource hierarchy) preserves the Fluent token system while gaining PackIcon (6000+ icons), DialogHost (modal/non-modal dialogs), and Material-styled Slider/ToggleButton/Chip. The `MaterialControls.xaml` dictionary overrides Material control templates to use `{DynamicResource}` references to Fluent semantic tokens (Brush.Accent, Brush.Text, Brush.Surface), ensuring theme switching propagates to Material controls.

**Alternatives considered**:
- **Custom icon pack (XAML Paths)** — Time-prohibitive; the app needs 50+ icons for sidebar categories, toolbar, and ribbon. Creating each as a XAML Path with dynamic color support would be weeks of work. PackIcon provides 6000+ ready icons with built-in `Foreground` binding.
- **FontAwesome/other icon fonts** — Fewer icons than Material, less mature WPF support. PackIcon is more performant (single glyph rendering) and has better WPF integration than other icon font libraries.
- **Full MaterialDesignInXaml theme** — Violates Constitution XIV. Would replace all existing Fluent styles and break the established token hierarchy. Rejected outright.

**Implementation notes**:
- `MaterialIntegration.xaml` merges `MaterialDesignTheme.Defaults.xaml` (provides ControlTemplate defaults) but does NOT merge `MaterialDesignTheme.Light.xaml` or `MaterialDesignTheme.Dark.xaml` (which would override Fluent colors)
- PackIcon instances in XAML: `<materialDesign:PackIcon Kind="Home" Foreground="{DynamicResource Brush.Accent.Primary}"/>`
- DialogHost wraps window content: `<materialDesign:DialogHost Identifier="RootDialog">`
- Slider style in `MaterialControls.xaml` overrides Track, Thumb templates to use `Brush.Accent.Primary` for active track and `Brush.Surface.CardSubtle` for inactive
- ToggleButton style uses Material ripple effect but inherits accent color from `Brush.Accent.Primary`
- Chip style binds background to `Brush.Surface.CardSubtle` and text to `Brush.Text.Primary`

## R2: DWM Blur Implementation

**Decision**: Use Windows DWM API (`DwmSetWindowCompositionAttribute` via `dwmapi.dll`) with `ACCENT_ENABLE_BLURBEHIND` to apply Gaussian blur to Shell workspace background. Encapsulated in `DwmBlurService` with fallback detection. The blur is applied to the Shell window's glass region, with the background image rendered behind it in a WPF ImageBrush.

**Rationale**: WPF's `BlurEffect` (`System.Windows.Media.Effects.BlurEffect`) is software-rendered and performs poorly on large surfaces (the Shell workspace can be 1920x1080+). It causes GPU-accelerated rendering to degrade to software, violating performance budgets. DWM blur is hardware-accelerated at the compositor level (Desktop Window Manager), has zero CPU/GPU impact on the WPF render thread, and is Excel-VSTO-safe because it operates at the OS compositor layer, not the WPF visual tree.

**Alternatives considered**:
- **WPF BlurEffect on ImageBrush** — Unacceptable performance. A 1920x1080 BlurEffect with Radius=20 drops frame rate from 60fps to ~8fps. Violates SC-002 (500ms target) and Constitution IX (GPU-safe).
- **Pre-blurred image generation (offline)** — No real-time adjustment. The spec requires a live slider (0-100%). Pre-generating blurred images at every increment wastes memory.
- **AcrylicBrush (WinUI/UWP)** — Not available in .NET Framework 4.8 WPF without WinUI interop, which adds complexity and potential VSTO compatibility issues.

**Implementation notes**:
- `DwmBlurService.EnableBlur(WindowHandle, blurIntensity 0.0-1.0)` — maps intensity to DWM accent policy gradient color alpha
- Fallback detection: `DwmBlurService.IsBlurSupported` checks OS version (Windows 10 build 1709+), DWM composition state, and safe mode. If unsupported, background renders sharp.
- Glass region: applies to entire Shell window client area; content (pages, sidebar) renders on top with opaque backgrounds
- Safe mode (FallbackSafe rendering) disables DWM blur entirely — falls back to solid/gradient backdrop

## R3: Dynamic Font Switching

**Decision**: Store the selected font family name as a `FontFamily` resource in `Application.Current.Resources["CustomFontFamily"]`. All control styles bind `FontFamily` to `{DynamicResource CustomFontFamily}` with a fallback to `Segoe UI`. `FontEnumerator` provides font discovery, Arabic font detection, and preview thumbnail generation.

**Rationale**: WPF `FontFamily` is a dependency property on all `Control` subclasses and supports `DynamicResource` binding. By setting `FontFamily="{DynamicResource CustomFontFamily}"` on root-level styles (Window, Label, Button, TextBlock default styles), all controls inherit the font change automatically without per-control code. The fallback to Segoe UI ensures text always renders even if the custom font is unavailable.

**Alternatives considered**:
- **Per-window FontFamily property** — Requires setting the property on every window instance. Misses popups, tooltips, context menus, and dynamically created controls.
- **Implicit Style with DataTrigger** — Unnecessary complexity. A single `DynamicResource` key is simpler and equally effective.
- **ResourceDictionary swap per theme** — Doesn't work because font selection is per-user (Custom theme), not per-theme-mode (Dark/Light). A resource dictionary swap would require 3 copies of every control style with different font families, which is unmaintainable.

**Implementation notes**:
- `FontEnumerator.GetSystemFonts()` → `List<FontFamilyInfo>` with `Name`, `FamilyName`, `IsArabicCompatible`, `PreviewGlyphs`
- `FontEnumerator.GeneratePreview(FontFamily, sampleText)` → returns a `BitmapSource` thumbnail at 200x40px for the font selector UI
- Arabic font detection: checks if the font contains Arabic Unicode block glyphs (U+0600–U+06FF) via `GlyphTypeface.CharacterToGlyphMap`
- Font change propagation: `ThemeManager.SetResource("CustomFontFamily", new FontFamily(fontName))` — WPF's `DynamicResource` system handles propagation to all open windows automatically
- Persistence: `Settings.Default.SelectedFontFamily` stores the family name string

## R4: Color Wheel Implementation

**Decision**: Implement a custom WPF `ColorWheel` control as a `UserControl` that renders an HSV color wheel using a `WriteableBitmap` with pixel-level HSV→RGB conversion. Mouse/touch drag interaction returns the selected color. The existing `AccentSwatchItem` model is extended with a `CustomColor` variant.

**Rationale**: WPF has no built-in color wheel. Third-party color pickers (e.g., ColorPicker from Extended WPF Toolkit) violate Constitution XIV or add unnecessary dependencies. A custom UserControl with `WriteableBitmap` is performant (single bitmap render, no visual tree per pixel), gives full control over the HSV space, and integrates naturally with the existing accent picker UI (AppearancePanel). The bitmap is generated once on load and regenerated only when the wheel size changes (DPI change).

**Alternatives considered**:
- **Linear RGB sliders (R, G, B)** — Poor UX for color selection. Users think in hue/saturation terms, not RGB channel values. The spec explicitly requests a "color wheel."
- **Third-party ColorPicker** — Violates Constitution XIV (no third-party UI frameworks beyond ADR-006 exceptions). Adds dependency overhead.
- **HSV sliders (H, S, V separately)** — Better than RGB but still less intuitive than a 2D wheel. Wheel provides spatial color relationships that sliders can't convey.

**Implementation notes**:
- `ColorWheel` control: `DependencyProperty SelectedColor (Color)`, `DependencyProperty WheelRadius (double)`
- Bitmap generation: iterate over polar coordinates (angle = hue, radius = saturation), convert HSV→RGB, write pixel to `WriteableBitmap`
- Mouse interaction: `MouseDown`/`MouseMove` → convert mouse position to polar coordinates → HSV→RGB → update `SelectedColor`
- Hex input: `TextBox` with validation (regex `^#[0-9A-Fa-f]{6}$`), updates `SelectedColor` and syncs with wheel
- Real-time accent preview: `SelectedColor` binding triggers `ThemeManager.ApplyAccentColor()` with 100ms debounce via `DispatcherTimer`
- Wheel renders at 256x256px, scales with DPI

## R5: Accent Variant Generation Algorithm

**Decision**: Generate 5 accent variants (hover, pressed, glow, border, subtle) from the base accent color using HSL color space transformations. All variants are computed in `ThemeManager.GenerateAccentVariants(Color baseColor)` and stored as named resources.

**Rationale**: HSL (Hue, Saturation, Lightness) is perceptually intuitive for variant generation:
- **Hover**: Same hue/sat, lightness +10% — brighter for visual feedback
- **Pressed**: Same hue/sat, lightness -10% — darker for press state
- **Glow**: Same hue, saturation +15%, lightness +5% — more vivid for glow effects
- **Border**: Same hue, saturation -10%, lightness +20% — muted for subtle borders
- **Subtle**: Same hue, saturation -30%, lightness +30% — very light tint for subtle backgrounds

Using HSL avoids the perceptual non-uniformity of RGB. For example, darkening a color by reducing R/G/B equally shifts hue toward black; HSL preserves hue during lightness changes.

**Alternatives considered**:
- **RGB channel scaling (R±20, G±20, B±20)** — Produces hue shifts and unnatural colors. Darkening bright blue (#3A86FF) by subtracting 20 from each channel produces a muddy, desaturated result instead of a darker blue.
- **LAB color space** — More perceptually accurate but computationally heavier. Requires XYZ→LAB conversion. HSL is "good enough" for UI variants and significantly simpler to implement.
- **Predefined variant tables** — Limits to 8 preset accent colors. Spec FR-008 (color wheel) and FR-009 (hex input) require any color to generate variants. Precomputed tables don't scale.

**Implementation notes**:
- `ColorHelper.RgbToHsl(Color)` and `ColorHelper.HslToRgb(double h, double s, double l)` — standard conversion formulas
- Generated resources: `Accent.Color.Hover`, `Accent.Color.Pressed`, `Accent.Color.Glow`, `Accent.Color.Border`, `Accent.Color.Subtle` + corresponding `Accent.Brush.*` variants
- Minimum perceptual distance check: if any variant's ΔE (CIE76) from base is <5, apply a minimum adjustment to ensure visual difference
- Variants updated on every accent change via `ThemeManager.GenerateAccentVariants()` → `ThemeManager.SetResource()` for each variant

## R6: WCAG 2.1 AA Contrast Compliance

**Decision**: Validate all semantic token pairs (Text/Primary vs Surface/Primary, Text/Secondary vs Surface, Accent vs Surface, etc.) against WCAG 2.1 AA thresholds using relative luminance calculation. Fix non-compliant tokens by adjusting lightness values in dark and light color palettes. Add a contrast validation method to `ThemeManager`.

**Rationale**: WCAG 2.1 AA requires 4.5:1 for normal text and 3:1 for large text (≥18pt or ≥14pt bold). The existing dark theme likely already passes (dark background + light text has naturally high contrast), but the light theme's semantic tokens need verification. The constitution (VIII) requires contrast validation for editable themes. Adding a programmatic checker ensures compliance is maintained as tokens evolve.

**Alternatives considered**:
- **Manual visual check only** — Subjective and error-prone. Two designers may disagree on whether contrast is "good enough." Objective measurement is needed.
- **External contrast checker tool (e.g., Colour Contrast Analyser)** — Requires manual process per token. Doesn't integrate with CI/build validation. Tokens can drift between manual checks.
- **Only light theme validation** — The clarification session confirmed both dark and light themes must meet AA. Cannot limit to light only.

**Implementation notes**:
- `ThemeManager.ValidateContrast()` — iterates semantic token pairs, computes contrast ratio, logs warnings for non-compliant pairs
- Token pairs defined in a static list:
  - `Brush.Text.Primary` vs `Brush.Surface.Primary` → 4.5:1 (normal text)
  - `Brush.Text.Secondary` vs `Brush.Surface.Primary` → 4.5:1 (normal text)
  - `Brush.Text.Primary` vs `Brush.Surface.Card` → 4.5:1
  - `Brush.Accent.Primary` vs `Brush.Surface.Primary` → 3:1 (large text/UI elements)
  - `Brush.Text.OnAccent` vs `Brush.Accent.Primary` → 4.5:1 (text on accent buttons)
- Contrast ratio formula: `(L1 + 0.05) / (L2 + 0.05)` where L1 is lighter luminance
- Relative luminance: `0.2126 * R + 0.7152 * G + 0.0722 * B` with sRGB linearization
- Non-compliant tokens in light theme are adjusted (e.g., `Brush.Text.Secondary` darkened from `#94A3B8` to `#64748B`)

## R7: Window Control Unification

**Decision**: Refine the existing `WindowButtonStyles.xaml` button styles into a single unified set used by the ModernWindow template. All windows inherit the same `TitleBarButton`, `CloseButton`, `MaximizeButton`, `MinimizeButton`, and `RestoreButton` styles. The title bar drag region is enforced via `WindowChrome.IsHitTestVisibleInChrome` attached property.

**Rationale**: The existing ModernWindow.xaml already defines window controls with styles from `WindowButtonStyles.xaml`. The issue is that some windows may use the alternative `WindowStyles.xaml` template (simpler, only minimize+close). The fix is to:
1. Remove the alternative template from `WindowStyles.xaml`
2. Ensure all windows inherit from `ModernWindow` (Constitution XI already enforces this)
3. Refine the button styles for consistent sizing (36x32 for min/max, 46x32 for close) and hover/pressed color tokens
4. The `WindowChrome.IsHitTestVisibleInChrome` property on the button container ensures buttons are excluded from drag

**Alternatives considered**:
- **Custom title bar UserControl per window** — Violates WindowChrome enforcement (Constitution XI). Each window would duplicate template code. A single control template in ModernWindow.xaml is the correct WPF pattern.
- **OS-native title bar (WindowStyle.SingleBorderWindow)** — Loses custom theming. Window controls wouldn't use theme tokens. Hover/pressed states would use OS defaults rather than accent colors.

**Implementation notes**:
- Close button: hover → `Brush.CloseButton.HoverBackground` (semi-transparent red), pressed → `Brush.CloseButton.PressedBackground`
- Minimize/Maximize buttons: hover → `Brush.TitleBar.ButtonHover` (accent-tinted), pressed → `Brush.TitleBar.ButtonPressed`
- Button hit-test areas: `Width=46` (close), `Width=36` (min/max), `Height=32` (all), with 2px internal padding for click target
- Title bar drag region: Grid column 1 (title text) has `WindowChrome.IsHitTestVisibleInChrome="True"`; column 2 (button StackPanel) has `WindowChrome.IsHitTestVisibleInChrome="False"`
- DPI scaling: button sizes are in WPF device-independent pixels (96 DPI = 1:1); at 150% DPI, WPF scales automatically
