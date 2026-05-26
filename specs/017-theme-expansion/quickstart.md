# Quickstart: Theme Expansion

**Feature**: 017-theme-expansion
**Date**: 2026-05-26

## Prerequisites

1. **Solution**: `WpfApp2\Som3a_WPF_UI.csproj` (.NET Framework 4.8)
2. **Existing dependencies**: ThemeManager singleton, ModernWindow, ThemeResources.xaml loading order
3. **NuGet package**: Install `MaterialDesignThemes` version 5.x
   ```
   Install-Package MaterialDesignThemes -Version 5.1.0
   ```
4. **Build**: MSBuild from command line (see AGENTS.md)

## Implementation Order

### Step 1: Material Design Integration (FR-001, FR-002, FR-002a)

1. Add `MaterialDesignThemes` NuGet to `WpfApp2`
2. Create `Theme/MaterialIntegration.xaml`:
   - Merge `MaterialDesignTheme.Defaults.xaml` (not Light/Dark — preserve Fluent colors)
   - Define bridging resources (e.g., `MaterialDesignBody` → `{DynamicResource Brush.Text.Primary}`)
3. Create `Theme/Controls/MaterialIcons.xaml`:
   - Implicit PackIcon style with `Foreground="{DynamicResource Brush.Accent.Primary}"`
4. Create `Theme/Controls/MaterialControls.xaml`:
   - Slider style: active track → `Brush.Accent.Primary`, inactive → `Brush.Surface.CardSubtle`
   - ToggleButton style: Material ripple + Fluent accent
   - Chip style: Material Chip template with Fluent surface/text tokens
5. Register new dictionaries in `ThemeResources.xaml` (after existing Controls, before ModernWindow)
6. Replace existing icon usage with PackIcon (sidebar, toolbar, ribbon)

### Step 2: Background Image & Blur (FR-003, FR-004)

1. Create `Services/DwmBlurService.cs`:
   - P/Invoke `DwmSetWindowCompositionAttribute`
   - `EnableBlur(IntPtr hwnd, double intensity)` — 0.0 to 1.0 mapping to accent policy
   - `IsBlurSupported` — check OS version, DWM composition, safe mode
2. Create `Theme/Effects/Backdrop.xaml`:
   - `ImageBrush` resources for background
   - Solid/gradient fallback brushes
3. Extend `Controls/ModernWindow.cs`:
   - `WindowBackdrop` DP already exists — wire it to DWM blur service
   - `BackgroundSettings` property binding to `Backdrop.xaml` resources
4. Add `WindowBackdropStyle`, `BackgroundImagePath`, `BackgroundBlurIntensity`, `BackgroundBlurEnabled` to `Properties/Settings.settings`
5. Create `Views/CustomThemeSettings.xaml`:
   - File picker button for image selection (with 10MB/4096px validation)
   - Blur intensity slider (0-100%)
   - Blur enable/disable toggle
6. Extend `ThemeManager.cs`:
   - `ApplyBackground(string imagePath, double blurIntensity)` method
   - Fallback logic: missing image → solid/gradient

### Step 3: Font System (FR-005, FR-006, FR-007)

1. Create `Services/FontEnumerator.cs`:
   - `GetSystemFonts()` → list of installed font families
   - `IsArabicCompatible(FontFamily)` → check for Arabic Unicode block glyphs
   - `GeneratePreview(FontFamily, string sample)` → 200x40px thumbnail
2. Create `Controls/FontPreview.cs`:
   - UserControl displaying font thumbnail + family name
   - Click selection with highlight border
3. Add `SelectedFontFamily` to `Properties/Settings.settings`
4. Add `FontFamily="{DynamicResource CustomFontFamily}"` to root control styles in control templates (or a global implicit style)
5. Extend `ThemeManager.cs`:
   - `ApplyFont(string fontFamilyName)` method
   - `SetResource("CustomFontFamily", new FontFamily(fontFamilyName))`
6. Extend `AppearancePanel.xaml` or `CustomThemeSettings.xaml`:
   - Font selection list with preview thumbnails
   - Arabic font section (Phase 24 readiness)

### Step 4: Accent Customization (FR-008, FR-009, FR-010)

1. Create `Controls/ColorWheel.cs`:
   - WriteableBitmap HSV wheel (256x256px)
   - Mouse drag → polar coords → HSV→RGB → SelectedColor DP
   - Preview circle showing selected color
2. Add hex input `TextBox` to `AppearancePanel.xaml`:
   - Regex validation: `^#[0-9A-Fa-f]{6}$`
   - Two-way binding to accent color
3. Extend `ThemeManager.cs`:
   - `GenerateAccentVariants(Color baseColor)` — 5 HSL-derived variants
   - `GenerateAccentVariants(Color baseColor)` → `GenerateAccentVariants(Color baseColor)` — 5 HSL-derived variants _[duplicate removed]_
   - Minimum ΔE check for perceptual distinctiveness
4. Add variant swatches to `AppearancePanel.xaml`:
   - 5 additional swatches below the 8 preset swatches
   - Each labeled: Hover, Pressed, Glow, Border, Subtle

### Step 5: Window Controls (FR-011, FR-012)

1. Refine `Theme/Controls/WindowButtonStyles.xaml`:
   - Unify sizing: Close=46x32, Min/Max=36x32
   - Ensure all buttons use theme tokens for backgrounds
   - 100ms CubicEase hover/pressed transitions
2. Update `Theme/ModernWindow.xaml`:
   - Confirm drag region: title Grid column 1 = draggable, column 2 (buttons) = not draggable via `WindowChrome.IsHitTestVisibleInChrome`
   - Remove any reference to the older `WindowStyles.xaml` alternative template
3. Verify `WindowRenderModeDetector` safe mode doesn't break button layout

### Step 6: Light & Dark Theme WCAG Compliance (FR-013)

1. Extend `ThemeManager.cs`:
   - `ValidateContrast()` — iterate token pairs, compute WCAG contrast ratio
   - Log warnings for non-compliant pairs (4.5:1 normal, 3:1 large)
2. Fix non-compliant tokens in `Light/LightColors.xaml` and `Dark/DarkColors.xaml`:
   - Adjust lightness values to meet AA thresholds
   - Common fix: darken `TextSecondary` in light theme, lighten `TextSecondary` in dark theme
3. Re-run contrast validation after fixes

### Step 7: Persistence & Settings UI (FR-007, FR-014, FR-015)

1. Add new keys to `Properties/Settings.settings`
2. Extend `ThemeManager.SaveCurrentTheme()` to persist background/font settings
3. Extend `ThemeManager.LoadThemeFromSettings()` to restore background/font settings
4. Create `ViewModels/CustomThemeViewModel.cs`:
   - Binds to `BackgroundSettings` and `FontSettings`
   - Commands: `SelectImageCommand`, `ClearImageCommand`, `ApplyFontCommand`
5. Wire `CustomThemeSettings.xaml` into `SettingsPage.xaml` navigation

## Build Verification

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## Excel VSTO Test

1. Build solution
2. Launch Excel, load the Som3a add-in
3. Verify theme switching works (Dark ↔ Light ↔ Custom)
4. Verify Material icons render in sidebar (safe mode and normal mode)
5. Verify background image applies to Shell workspace
6. Verify blur slider works (on supported Windows 10+ builds)
7. Verify font change propagates to all windows
8. Verify accent color wheel and hex input work
9. Verify window control buttons (min/max/close) render and respond correctly

## Key Files

| File | Action | Content |
|------|--------|---------|
| `Theme/MaterialIntegration.xaml` | Create | MaterialDesignThemes import + Fluent bridging |
| `Theme/Controls/MaterialIcons.xaml` | Create | PackIcon implicit style |
| `Theme/Controls/MaterialControls.xaml` | Create | Slider, ToggleButton, Chip styles |
| `Theme/Effects/Backdrop.xaml` | Create | Background image/blur resources |
| `Services/DwmBlurService.cs` | Create | Win32 DWM interop |
| `Services/FontEnumerator.cs` | Create | System font enumeration |
| `Controls/ColorWheel.cs` | Create | HSV color wheel custom control |
| `Controls/FontPreview.cs` | Create | Font thumbnail control |
| `Views/CustomThemeSettings.xaml` | Create | Background + font settings UI |
| `ViewModels/CustomThemeViewModel.cs` | Create | Background/font state management |
| `Theme/ModernWindow.xaml` | Modify | Refined drag region, button layout |
| `Theme/Controls/WindowButtonStyles.xaml` | Modify | Unified button sizing/hover states |
| `Theme/ThemeResources.xaml` | Modify | Add new dictionaries in order |
| `Services/ThemeManager.cs` | Modify | BackgroundApply, FontApply, AccentVariantGenerate, ValidateContrast |
| `Views/AppearancePanel.xaml` | Modify | Color wheel, hex input, variant swatches, font selector |
| `ViewModels/SettingsViewModel.cs` | Modify | Background/font/accent variant properties |
| `Properties/Settings.settings` | Modify | New persistence keys |
