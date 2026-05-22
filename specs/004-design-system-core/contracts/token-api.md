# Token API Contract: Design System Core

**Branch**: `004-design-system-core` | **Date**: 2026-05-22

## Overview

This contract defines the public token API — the set of named resources that control templates, window markup, and other consumers may reference. It establishes naming conventions, reference rules, and the dependency chain between token layers.

## Token Naming Convention

### Primitive Tokens

Pattern: `Primitive.<Color>.<Shade>`

| Namespace | Examples | Type |
|-----------|----------|------|
| `Primitive.Blue.*` | `Primitive.Blue.500`, `Primitive.Blue.600` | `Color` |
| `Primitive.Slate.*` | `Primitive.Slate.900`, `Primitive.Slate.800`, `Primitive.Slate.700` | `Color` |
| `Primitive.White.*` | `Primitive.White.95`, `Primitive.White.75`, `Primitive.White.40`, `Primitive.White.33`, `Primitive.White.20`, `Primitive.White.10`, `Primitive.White.5` | `Color` |
| `Primitive.Green.*` | `Primitive.Green.500` | `Color` |
| `Primitive.Orange.*` | `Primitive.Orange.500` | `Color` |
| `Primitive.Red.*` | `Primitive.Red.500` | `Color` |
| `Primitive.Black.*` | `Primitive.Black.13`, `Primitive.Black.8`, `Primitive.Black.53` | `Color` |
| `Primitive.Transparency.*` | `Primitive.Transparency.Subtle`, `Primitive.Transparency.Light`, `Primitive.Transparency.MediumLow`, `Primitive.Transparency.Medium`, `Primitive.Transparency.Strong` | `Color` |

**Rules**:
- Immutable at runtime (Constitution V)
- May only be defined in `Theme/Base/Colors.xaml`
- New primitive tokens may be added for accent color families in Phase 3

### Semantic Color Tokens

Pattern: `Color.<Category>.<Property>`

| Namespace | Examples | Type |
|-----------|----------|------|
| `Color.Background.*` | `Color.Background.CardSubtle`, `Color.Background.RootStart`, `Color.Background.RootEnd` | `Color` |
| `Color.Text.*` | `Color.Text.OnAccent` | `Color` |
| `Color.Stroke.*` | `Color.Stroke.Info`, `Color.Stroke.Status` | `Color` |
| `Color.Fill.*` | `Color.Fill.Info`, `Color.Fill.Status` | `Color` |
| `Color.Control.*` | `Color.Control.Background`, `Color.Control.HoverBackground`, `Color.Control.SelectedBackground` | `Color` |
| `Color.ComboBox.*` | `Color.ComboBox.HoverBorder`, `Color.ComboBox.HoverBackground`, `Color.ComboBoxPopup`, `Color.ComboBoxPopupBorder`, `Color.ComboBoxItemHighlight` | `Color` |
| `Color.Button.*` | `Color.Button.Background`, `Color.Button.HoverBackground`, `Color.Button.PressedBackground` | `Color` |
| `Color.DataGrid.*` | `Color.DataGrid.AlternatingRow`, `Color.DataGrid.HoverRow`, `Color.DataGrid.SelectedRow` | `Color` |
| `Color.Animation.*` | `Color.Animation.ThumbFade`, `Color.Animation.ButtonHoverDanger` | `Color` |

**Rules**:
- May reference `Primitive.*` tokens only — no direct hex, no `Brush.*` references (FR-004)
- Theme-overridable: Dark, Light, and Custom themes override these in their respective `*Colors.xaml`

### Semantic Brush Tokens

Pattern: `Brush.<Category>.<Property>`

| Namespace | Examples | Type |
|-----------|----------|------|
| `Brush.Background.*` | `Brush.Background.Primary`, `Brush.Background.Secondary`, `Brush.Background.Card`, `Brush.Background.CardSubtle`, `Brush.Background.Root` | `SolidColorBrush` / `LinearGradientBrush` |
| `Brush.Text.*` | `Brush.Text.Primary`, `Brush.Text.Secondary`, `Brush.Text.Disabled`, `Brush.Text.OnAccent` | `SolidColorBrush` |
| `Brush.Accent.*` | `Brush.Accent.Primary`, `Brush.Accent.Success`, `Brush.Accent.Warning`, `Brush.Accent.Danger`, `Brush.Accent.Info`, `Brush.Accent.ProgressFill` | `SolidColorBrush` / `LinearGradientBrush` |
| `Brush.Stroke.*` | `Brush.Stroke.Card`, `Brush.Stroke.Info`, `Brush.Stroke.Status` | `SolidColorBrush` |
| `Brush.Fill.*` | `Brush.Fill.Info`, `Brush.Fill.Status` | `SolidColorBrush` |
| `Brush.Control.*` | `Brush.Control.Background`, `Brush.Control.Stroke`, `Brush.Control.HoverBackground`, `Brush.Control.SelectedBackground` | `SolidColorBrush` |
| `Brush.ComboBox.*` | `Brush.ComboBox.HoverBorder`, `Brush.ComboBox.HoverBackground` | `SolidColorBrush` |
| `Brush.Button.*` | `Brush.Button.Background`, `Brush.Button.HoverBackground`, `Brush.Button.PressedBackground` | `SolidColorBrush` |
| `Brush.DataGrid.*` | `Brush.DataGrid.AlternatingRow`, `Brush.DataGrid.HoverRow`, `Brush.DataGrid.SelectedRow` | `SolidColorBrush` |
| `Brush.Progress.*` | `Brush.Progress.Background` | `SolidColorBrush` |
| `Brush.Swatch.*` | `Brush.Swatch.Stroke` | `SolidColorBrush` |

**Rules**:
- MUST reference `Color.*` or `Primitive.*` tokens via `{StaticResource}` — no direct hex (FR-004)
- MUST NOT reference another `Brush.*` token (semantic-to-semantic chains prohibited)
- Consumed by control templates via `{DynamicResource Brush.*}`
- Theme-overridable via Dark/Light/Custom `*Colors.xaml`

### Spacing Tokens

Pattern: `Spacing.<Size>` and `Padding.<Size>`

| Key | Value | Type |
|-----|-------|------|
| `Spacing.XSmall` | 4 | `sys:Double` |
| `Spacing.Small` | 8 | `sys:Double` |
| `Spacing.Medium` | 12 | `sys:Double` |
| `Spacing.Large` | 16 | `sys:Double` |
| `Spacing.XLarge` | 20 | `sys:Double` |
| `Spacing.XXLarge` | 24 | `sys:Double` |
| `Padding.XSmall` | 4 | `Thickness` |
| `Padding.Small` | 8,6 | `Thickness` |
| `Padding.Medium` | 12,8 | `Thickness` |
| `Padding.Large` | 16,12 | `Thickness` |
| `Padding.XLarge` | 20,16 | `Thickness` |

### Typography Tokens

| Key | Value | Type |
|-----|-------|------|
| `FontFamilyPrimary` | Segoe UI | `FontFamily` |
| `TitleFontSize` | 20 | `sys:Double` |
| `HeaderFontSize` | 16 | `sys:Double` |
| `SubHeaderFontSize` | 14 | `sys:Double` |
| `BodyFontSize` | 13 | `sys:Double` |
| `CaptionFontSize` | 11 | `sys:Double` |
| `FontWeightNormal` | Normal | `FontWeight` |
| `FontWeightMedium` | Medium | `FontWeight` |
| `FontWeightBold` | Bold | `FontWeight` |
| `Style.Text.Title` | (style) | `Style` |
| `Style.Text.Subtitle` | (style) | `Style` |
| `Style.Text.Body` | (style) | `Style` |
| `Style.Text.Caption` | (style) | `Style` |
| `Style.Text.Button` | (style) | `Style` |

### Radius Tokens

| Key | Value | Type |
|-----|-------|------|
| `SmallRadius` | 6,6,6,6 | `CornerRadius` |
| `MediumRadius` | 10,10,10,10 | `CornerRadius` |
| `LargeRadius` | 14,14,14,14 | `CornerRadius` |
| `CardRadius` | 12,12,12,12 | `CornerRadius` |
| `WindowRadius` | 18,18,18,18 | `CornerRadius` |
| `HeaderCornerRadius` | 18,18,0,0 | `CornerRadius` |
| `PillRadius` | 20,20,20,20 | `CornerRadius` |

### Elevation, Motion, ZIndex, Opacity Tokens

(Already present — see plan.md research for full listings)

### Component Tokens

Pattern: `Component.<Control>.<Property>`

| Key | Value | Type |
|-----|-------|------|
| `Component.Button.Padding` | 12,6 | `Thickness` |
| `Component.Button.Radius` | 10 | `CornerRadius` |
| `Component.Button.Height` | 32 | `sys:Double` |
| `Component.ComboBox.Radius` | 10 | `CornerRadius` |
| `Component.ComboBox.Height` | 30 | `sys:Double` |
| `Component.TextBox.Radius` | 10 | `CornerRadius` |
| `Component.TextBox.Padding` | 10,6 | `Thickness` |
| `Component.TextBox.Height` | 30 | `sys:Double` |
| `Component.Card.Padding` | 16,12 | `Thickness` |
| `Component.Card.Radius` | 12 | `CornerRadius` |
| `Component.DataGrid.CellPadding` | 8,4 | `Thickness` |
| `Component.DataGrid.HeaderPadding` | 8,6 | `Thickness` |
| `Component.ThemeCard.Width` | 140 | `sys:Double` |
| `Component.ThemeCard.Height` | 120 | `sys:Double` |
| `Component.ThemeCard.Radius` | 12 | `CornerRadius` |
| `Component.AccentSwatch.Size` | 28 | `sys:Double` |
| `Component.ProgressBar.Radius` | 10 | `CornerRadius` |

## Reference Rules Summary

1. **Primitive** → raw hex only, never references other tokens
2. **Semantic Color** → references Primitive or direct hex (in base only)
3. **Semantic Brush** → references Semantic Color or Primitive, never direct hex, never other Brush tokens
4. **Component** → references Spacing, Padding, Radius, or Brush tokens; may have hardcoded defaults where DynamicResource unsupported
5. **Control Template** → references Brush, Component, Spacing, Radius, Motion, Opacity, Elevation, or Effect tokens via DynamicResource
6. **Legacy Flat Key** → references Primitive or Brush token, annotated as deprecated for Phase 6 removal

## Deprecation Contract

Legacy flat keys (e.g., `Blue500`, `Slate900`, `BackgroundBrush`, `TextMainBrush`) are preserved with deprecation comments:

```xml
<!-- DEPRECATED: Use Brush.Accent.Primary instead. Removal: Phase 6 -->
<SolidColorBrush x:Key="Blue500" Color="{StaticResource Primitive.Blue.500}"/>
```

New code MUST NOT reference legacy flat keys. All new references MUST use `Brush.*` or `Primitive.*` tokens.