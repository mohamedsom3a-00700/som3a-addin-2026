# Quickstart: Design System Core

**Branch**: `004-design-system-core` | **Date**: 2026-05-22

## Prerequisites

- .NET Framework 4.8 SDK
- Visual Studio 2019+ with WPF workload
- Git on `004-design-system-core` branch
- Excel 2016+ for VSTO host testing

## Build & Verify

```powershell
# Build the project
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Run token validation lint (after implementing FR-010)
powershell -File scripts\Validate-Tokens.ps1
```

## Token Reference Quick Guide

### Using Color Tokens in Controls

```xml
<!-- CORRECT: Semantic brush via DynamicResource -->
<Border Background="{DynamicResource Brush.Background.Primary}" />

<!-- CORRECT: Semantic brush for interactive states -->
<Button Background="{DynamicResource Brush.Button.Background}" />

<!-- INCORRECT: Direct hex color (violation!) -->
<Border Background="#0E1720" />

<!-- INCORRECT: StaticResource on themeable property (violation!) -->
<Border Background="{StaticResource Brush.Background.Primary}" />
```

### Using Spacing Tokens

```xml
<!-- CORRECT: Named padding token -->
<Button Padding="{DynamicResource Padding.Medium}" />

<!-- CORRECT: Named spacing token in Thickness -->
<Grid Margin="{DynamicResource Padding.Large}" />

<!-- INCORRECT: Hardcoded margin (violation!) -->
<Button Margin="12,8" />
```

### Using Typography Styles

```xml
<!-- CORRECT: Named text style -->
<TextBlock Style="{DynamicResource Style.Text.Body}" />

<!-- INCORRECT: Hardcoded font properties (violation!) -->
<TextBlock FontSize="13" FontWeight="Normal" Foreground="#F2FFFFFF" />
```

### Using Effects

```xml
<!-- CORRECT: Centralized shadow via DynamicResource -->
<Border Effect="{DynamicResource Shadow.Card}" />

<!-- CORRECT: Centralized glow via DynamicResource -->
<Border Effect="{DynamicResource Glow.Focus}" />

<!-- INCORRECT: Inline DropShadowEffect (violation!) -->
<Border>
  <Border.Effect>
    <DropShadowEffect BlurRadius="16" ShadowDepth="4" Opacity="0.25" />
  </Border.Effect>
</Border>
```

### Using Elevation Tokens

```xml
<!-- CORRECT: Elevation abstraction (references Shadow.* behind the scenes) -->
<Border Effect="{DynamicResource Elevation.Card}" />

<!-- Note: Elevation.Card maps to Shadow.Card in Effects/Shadows.xaml -->
```

## Adding New Tokens

### Adding a Primitive Color

```xml
<!-- In Theme/Base/Colors.xaml, Primitive section -->
<Color x:Key="Primitive.Teal.500">#14B8A6</Color>
```

Then reference it from a semantic token:

```xml
<!-- In Theme/Base/Colors.xaml, Semantic section -->
<SolidColorBrush x:Key="Brush.Accent.Teal" Color="{StaticResource Primitive.Teal.500}" />
```

### Adding a Semantic Brush for a Derived State

Derived state tokens compose from Primitive base color + Primitive transparency:

```xml
<!-- The Color intermediate (references Primitives) -->
<Color x:Key="Color.Button.HoverBackground">#3FFFFFFF</Color>
<!-- This is documented as: Primitive.White.25 on Slate.700 background -->

<!-- The Brush (references the Color) -->
<SolidColorBrush x:Key="Brush.Button.HoverBackground" Color="{StaticResource Color.Button.HoverBackground}" />
```

### Adding a Component Token

```xml
<!-- In Theme/Base/ComponentTokens.xaml -->
<Thickness x:Key="Component.CheckBox.Padding">8,6</Thickness>
```

## Deprecated Tokens

Legacy flat keys (e.g., `Blue500`, `Slate900`, `BackgroundBrush`) still work but are deprecated. When writing new code, always use the `Brush.*` or `Primitive.*` namespace:

```xml
<!-- DEPRECATED (will be removed in Phase 6): -->
<Border Background="{DynamicResource BackgroundBrush}" />

<!-- CORRECT replacement: -->
<Border Background="{DynamicResource Brush.Background.Primary}" />
```

## Testing Theme Switching

After making token changes, verify theme switching still works:

1. Launch the add-in inside Excel
2. Open SettingsWindow
3. Click each theme card (Dark, White/Light, Custom)
4. Verify all controls update immediately
5. Verify no XAML parse exceptions in Output window
6. Verify accent color swatches still work in Custom theme

## Token Validation

The build-time lint script (`scripts/Validate-Tokens.ps1`) checks for:

- Inline hex colors (`Color="#..."`) outside token definition files
- StaticResource on themeable DP properties
- Inline DropShadowEffect outside Effects/
- Hardcoded Margin/Padding/FontSize/FontWeight in controls and windows

Run it before committing token changes.