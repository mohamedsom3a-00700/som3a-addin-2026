# Data Model: Update Themes Manager

**Feature**: 002-themes-manager  
**Date**: 2026-05-19

---

## Overview

This feature does not introduce traditional database entities. Its "data model" is the **Resource Token Taxonomy** — the set of named XAML resources (brushes, colors, effects) that define the application's visual state. The runtime state is managed by the `ThemeManager` singleton.

---

## Runtime State (ThemeManager)

```csharp
public sealed class ThemeManager
{
    // Current theme selection (Dark / Light / Custom)
    private AppTheme _currentTheme = AppTheme.Dark;
    
    // Independent user preference — persists across theme switches
    private string _currentAccentColor = "#3A86FF";
    
    // Debounce timer for rapid switches (≈150ms)
    // Implementation detail: DispatcherTimer or Task.Delay cancellation token
}
```

### State Transitions

```
[Initial Load] --LoadThemeFromSettings()--> [Dark + #3A86FF] (default)
     │                                                    │
     │ ApplyTheme("Light", null)                          │ ApplyTheme("Dark", "#FF0000")
     ▼                                                    ▼
[Light + #3A86FF]                                    [Dark + #FF0000]
     │                                                    │
     │ ApplyTheme("Custom", "#00FF00")                    │ ApplyTheme("Custom", null)
     ▼                                                    ▼
[Custom + #00FF00]                                   [Custom + #FF0000]
```

**Invariant**: `_currentAccentColor` NEVER changes unless an explicit `accentColor` is provided to `ApplyTheme()`.

---

## Resource Token Taxonomy

### Primitive Tokens (immutable at runtime)

Defined in `Theme/Base/Colors.xaml`:

| Token | Type | Value | Usage |
|-------|------|-------|-------|
| `Blue500` | SolidColorBrush | `#3A86FF` | Base accent primitive |
| `Slate900` | SolidColorBrush | `#0E1720` | Deepest dark |
| `Slate800` | SolidColorBrush | `#1C2B3A` | Dark surface |
| `WhiteAlpha95` | SolidColorBrush | `#F2FFFFFF` | Primary text (dark bg) |
| `WhiteAlpha75` | SolidColorBrush | `#BFFFFFFF` | Secondary text |

### Semantic Tokens (themeable, per-theme overrides)

**Namespace convention**: `Brush.<Category>.<Role>`

| Token | Category | Dark Value | Light Value | Custom Value | Usage |
|-------|----------|------------|-------------|--------------|-------|
| `Brush.Background.Primary` | Background | `#0E1720` | `#F8F9FA` | `#0E1720` | Main app background |
| `Brush.Background.Secondary` | Background | `#1C2B3A` | `#FFFFFF` | `#1C2B3A` | Cards, surfaces |
| `Brush.Background.Card` | Background | `#15202B` | `#FFFFFF` | `#15202B` | Card surfaces |
| `Brush.Background.Root` | Background | Navy-teal gradient | Light gray gradient | Navy-teal gradient | **NEW** Window root bg |
| `Brush.Text.Primary` | Text | `#F2FFFFFF` | `#1A1A1A` | `#F2FFFFFF` | Main text |
| `Brush.Text.Secondary` | Text | `#BFFFFFFF` | `#6B7280` | `#BFFFFFFF` | Secondary text |
| `Brush.Text.Disabled` | Text | `#66FFFFFF` | `#9CA3AF` | `#66FFFFFF` | Disabled text |
| `Brush.Accent.Primary` | Accent | `#3A86FF` | `#3A86FF` | `#3A86FF` | Primary accent |
| `Brush.Accent.Success` | Accent | `#2ED573` | `#2ED573` | `#2ED573` | Success states |
| `Brush.Accent.Danger` | Accent | `#FF4757` | `#FF4757` | `#FF4757` | Danger/error |
| `Brush.Accent.ProgressFill` | Accent | Accent gradient | Accent gradient | Accent gradient | **NEW** Progress fill |
| `Brush.Stroke.Card` | Stroke | `#33FFFFFF` | `#E5E7EB` | `#33FFFFFF` | Card borders |
| `Brush.Stroke.Info` | Stroke | `#12FFFFFF` | `#1E3A5F` | `#12FFFFFF` | **NEW** Info borders |
| `Brush.Stroke.Status` | Stroke | `#1FFFFFFF` | `#E5E7EB` | `#1FFFFFFF` | **NEW** Status borders |
| `Brush.Control.Background` | Control | `#330E1720` | `#F3F4F6` | `#330E1720` | Control bg |
| `Brush.Control.Stroke` | Control | `#33FFFFFF` | `#D1D5DB` | `#33FFFFFF` | Control borders |

### Runtime Mutable Resources (set by ThemeManager)

| Resource Key | Type | Set By | Consumers |
|--------------|------|--------|-----------|
| `AccentColor` | Color | `ApplyAccentColor()` | Glow effects, brushes |
| `AccentBrush` | SolidColorBrush | `ApplyAccentColor()` | Buttons, selections |
| `AccentColorBrush` | SolidColorBrush | `ApplyAccentColor()` | Legacy compatibility |
| `AccentColorValue` | Color | `ApplyAccentColor()` | `Glow.xaml` DynamicResource |

### Centralized Effects

Defined in `Theme/Effects/Shadows.xaml` and `Theme/Effects/Glow.xaml`:

| Effect Key | Location | Color Source | Usage |
|------------|----------|--------------|-------|
| `Shadow.Window` | Shadows.xaml | `#000000` | Window chrome shadow |
| `Shadow.Card` | Shadows.xaml | `#000000` | Card elevation |
| `Shadow.Popup` | Shadows.xaml | `#000000` | Popup menus |
| `Glow.Focus` | Glow.xaml | `AccentColorValue` | Focus ring glow |
| `Glow.ButtonHover` | Glow.xaml | `AccentColorValue` | Button hover glow |
| `Glow.Primary` | Glow.xaml | `AccentColorValue` | Primary button glow |
| `Glow.Selection` | Glow.xaml | `AccentColorValue` | Selection highlight |
| `ProgressGlow` | **Shadows.xaml** (new) | `AccentColorValue` | Progress bar glow |

---

## Validation Rules

1. **No duplicate keys**: Every `x:Key` must be unique across all merged dictionaries in `ThemeResources.xaml`.
2. **DynamicResource only**: All consumer references to themeable tokens MUST use `{DynamicResource}`.
3. **Semantic namespace for new tokens**: New tokens MUST use `Brush.<Category>.<Role>` naming.
4. **Accent immutability by theme**: Theme dictionaries (DarkColors.xaml, etc.) MUST NOT define `AccentColorValue` or `AccentBrush`; these are runtime-mutable only.
5. **Effects centralization**: No `<DropShadowEffect>` may appear inline in any window `.xaml`. All effects MUST be defined in `Effects/Shadows.xaml` or `Effects/Glow.xaml`.

---

## Relationships

```
ThemeResources.xaml (aggregator)
├── Base/Colors.xaml          (primitive + semantic defaults)
├── Base/Typography.xaml
├── Base/Radius.xaml
├── Base/Spacing.xaml
├── Effects/Shadows.xaml      (shadow effects)
├── Effects/Glow.xaml         (accent-driven glow effects)
├── Controls/*.xaml           (control styles referencing semantic tokens)
├── Dark/DarkColors.xaml      (overrides for Dark theme)
├── Light/LightColors.xaml    (overrides for Light theme)
└── Custom/CustomColors.xaml  (overrides for Custom theme)

ThemeManager (singleton runtime service)
├── Reads: Properties.Settings.Default (SelectedTheme, AccentColor)
├── Writes: Application.Current.Resources.MergedDictionaries
├── Mutates: AccentColorValue, AccentBrush, Glow effect colors
└── Notifies: ThemeChanged event → all subscriber windows
```
