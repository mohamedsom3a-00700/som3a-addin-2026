# Phase 3: Theme Engine Architecture

**Branch**: `feature/theme-engine`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Create the complete theme dictionary structure with Dark, Light, and Custom themes, establishing the foundation for runtime theme switching. The token system MUST follow the two-tier Primitive + Semantic token architecture defined in the constitution.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Theme/Dark/` (new folder)
  - `WpfApp2/Theme/Light/` (new folder)
  - `WpfApp2/Theme/Custom/` (new folder)
  - `WpfApp2/Theme/ThemeManager.xaml` (new)
  - `WpfApp2/Theme/Base/Colors.xaml` (update — already has primitive + semantic tokens)

---

## Token Architecture

The Colors.xaml file already contains:

**Primitive Tokens** (raw palette colors):
```
Blue500, Slate900, Slate800, Slate700,
WhiteAlpha95, WhiteAlpha75, WhiteAlpha40, WhiteAlpha20, WhiteAlpha10, WhiteAlpha5,
SuccessGreen, WarningOrange, DangerRed, InfoBlue
```

**Semantic Tokens** (UI meaning aliases):
```
Brush.Background.Primary, Brush.Background.Secondary, Brush.Background.Card,
Brush.Text.Primary, Brush.Text.Secondary, Brush.Text.Disabled,
Brush.Accent.Primary, Brush.Accent.Success, Brush.Accent.Warning, Brush.Accent.Danger, Brush.Accent.Info,
Brush.Stroke.Card,
Brush.Control.Background, Brush.Control.Stroke
```

Each theme (Dark, Light, Custom) overrides the semantic token colors to create the visual identity.

---

## Resource Loading Order

Theme dictionaries MUST be merged in this exact order (per Resource Loading Order in constitution):

```
1. Base/Colors.xaml              (primitive + semantic tokens)
2. Base/Typography.xaml
3. Base/Spacing.xaml
4. Base/Radius.xaml
5. Effects/Shadows.xaml
6. Effects/Animations.xaml
7. Effects/Glow.xaml
8. Controls/ (all control styles)
9. ModernWindow.xaml
10. WindowAnimations.xaml
11. Dark/Light/Custom theme overrides  <-- Phase 3 creates these
```

---

## Tasks

### T011 Create Dark theme
**Files**: `WpfApp2/Theme/Dark/DarkColors.xaml`, `WpfApp2/Theme/Dark/DarkTheme.xaml`

DarkColors.xaml — override semantic tokens with dark palette values:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Dark Semantic Tokens -->
    <SolidColorBrush x:Key="Brush.Background.Primary" Color="#0E1720"/>
    <SolidColorBrush x:Key="Brush.Background.Secondary" Color="#1C2B3A"/>
    <SolidColorBrush x:Key="Brush.Background.Card" Color="#15202B"/>
    <SolidColorBrush x:Key="Brush.Text.Primary" Color="#F2FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Text.Secondary" Color="#BFFFFFFF"/>
    <SolidColorBrush x:Key="Brush.Text.Disabled" Color="#66FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Accent.Primary" Color="#3A86FF"/>
    <SolidColorBrush x:Key="Brush.Accent.Success" Color="#2ED573"/>
    <SolidColorBrush x:Key="Brush.Accent.Warning" Color="#FFA502"/>
    <SolidColorBrush x:Key="Brush.Accent.Danger" Color="#FF4757"/>
    <SolidColorBrush x:Key="Brush.Accent.Info" Color="#1E90FF"/>
    <SolidColorBrush x:Key="Brush.Stroke.Card" Color="#33FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Control.Background" Color="#330E1720"/>
    <SolidColorBrush x:Key="Brush.Control.Stroke" Color="#33FFFFFF"/>

</ResourceDictionary>
```

DarkTheme.xaml — merged ResourceDictionary:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Som3a_WPF_UI;component/Theme/Base/Colors.xaml"/>
        <ResourceDictionary Source="/Som3a_WPF_UI;component/Theme/Dark/DarkColors.xaml"/>
    </ResourceDictionary.MergedDictionaries>

</ResourceDictionary>
```

### T012 Create Light theme
**Files**: `WpfApp2/Theme/Light/LightColors.xaml`, `WpfApp2/Theme/Light/LightTheme.xaml`

Light palette (blue accent on white/light gray):
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Light Semantic Tokens -->
    <SolidColorBrush x:Key="Brush.Background.Primary" Color="#FAFAFA"/>
    <SolidColorBrush x:Key="Brush.Background.Secondary" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Background.Card" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Text.Primary" Color="#1A1A1A"/>
    <SolidColorBrush x:Key="Brush.Text.Secondary" Color="#666666"/>
    <SolidColorBrush x:Key="Brush.Text.Disabled" Color="#AAAAAA"/>
    <SolidColorBrush x:Key="Brush.Accent.Primary" Color="#3A86FF"/>
    <SolidColorBrush x:Key="Brush.Accent.Success" Color="#2ED573"/>
    <SolidColorBrush x:Key="Brush.Accent.Warning" Color="#FFA502"/>
    <SolidColorBrush x:Key="Brush.Accent.Danger" Color="#FF4757"/>
    <SolidColorBrush x:Key="Brush.Accent.Info" Color="#1E90FF"/>
    <SolidColorBrush x:Key="Brush.Stroke.Card" Color="#1A000000"/>
    <SolidColorBrush x:Key="Brush.Control.Background" Color="#F0F0F0"/>
    <SolidColorBrush x:Key="Brush.Control.Stroke" Color="#1A000000"/>

</ResourceDictionary>
```

### T013 Create Custom theme
**Files**: `WpfApp2/Theme/Custom/CustomColors.xaml`, `WpfApp2/Theme/Custom/CustomTheme.xaml`

Stub with accent color placeholders for future customization:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Custom Theme - accent color is customizable -->
    <!-- Default: Blue accent, dark background -->
    <SolidColorBrush x:Key="Brush.Background.Primary" Color="#0E1720"/>
    <SolidColorBrush x:Key="Brush.Background.Secondary" Color="#1C2B3A"/>
    <SolidColorBrush x:Key="Brush.Background.Card" Color="#15202B"/>
    <SolidColorBrush x:Key="Brush.Text.Primary" Color="#F2FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Text.Secondary" Color="#BFFFFFFF"/>
    <SolidColorBrush x:Key="Brush.Text.Disabled" Color="#66FFFFFF"/>

    <!-- Customizable Accent - user selects this -->
    <SolidColorBrush x:Key="Brush.Accent.Primary" Color="#3A86FF"/>
    <SolidColorBrush x:Key="Brush.Accent.Success" Color="#2ED573"/>
    <SolidColorBrush x:Key="Brush.Accent.Warning" Color="#FFA502"/>
    <SolidColorBrush x:Key="Brush.Accent.Danger" Color="#FF4757"/>
    <SolidColorBrush x:Key="Brush.Accent.Info" Color="#1E90FF"/>
    <SolidColorBrush x:Key="Brush.Stroke.Card" Color="#33FFFFFF"/>
    <SolidColorBrush x:Key="Brush.Control.Background" Color="#330E1720"/>
    <SolidColorBrush x:Key="Brush.Control.Stroke" Color="#33FFFFFF"/>

</ResourceDictionary>
```

### T014 Create ThemeManager.xaml
**File**: `WpfApp2/Theme/ThemeManager.xaml`

ResourceDictionary containing all theme variants:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <ResourceDictionary x:Key="DarkTheme"
        Source="/Som3a_WPF_UI;component/Theme/Dark/DarkTheme.xaml"/>
    <ResourceDictionary x:Key="LightTheme"
        Source="/Som3a_WPF_UI;component/Theme/Light/LightTheme.xaml"/>
    <ResourceDictionary x:Key="CustomTheme"
        Source="/Som3a_WPF_UI;component/Theme/Custom/CustomTheme.xaml"/>

</ResourceDictionary>
```

---

## Dependency Order

T011 → T012 → T013 → T014 (sequential for theme structure)

---

## Acceptance Criteria

- [ ] Dark theme complete with all semantic token overrides
- [ ] Light theme complete with all semantic token overrides (matching structure)
- [ ] Custom theme placeholder ready for future accent color customization
- [ ] ThemeManager.xaml defines all theme variants
- [ ] Token names consistent across all themes
- [ ] Resource loading order documented and enforced
- [ ] All themes pass Theme Validation Checklist before Phase 4

---

## Theme Validation Checklist

Before Phase 4 begins, verify each theme passes:
- [ ] Contrast ratio: Text on background ≥ 4.5:1 (WCAG AA)
- [ ] DataGrid readability: Row text, header text, selection visible
- [ ] Disabled states: Clearly dimmed, not invisible
- [ ] Hover visibility: Interactive elements show hover state
- [ ] Focus visibility: Keyboard focus ring clearly visible
- [ ] Popup readability: ComboBox, Tooltip, Popup content readable
- [ ] Accessibility: No text disappears on any background combination
- [ ] DPI readability: All text and UI elements scale correctly at 100%, 125%, 150%, 200%

---

## Constitution Check

Per constitution:
- **Principle IV (Runtime Theme Switching)**: Theme switching MUST update without restart
- **Resource Loading Order**: Correct merge order is enforced
- **Primitive & Semantic Token Architecture**: Two-tier separation required for future custom theme generation
- **Incremental Migration Rules**: Validate one prototype window before advancing

(End of file — total 160 lines)