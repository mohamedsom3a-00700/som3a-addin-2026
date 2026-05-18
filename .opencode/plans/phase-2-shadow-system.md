# Phase 2: Shadow System

**Branch**: `feature/shadow-system`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Centralize and organize all shadow effects with proper naming convention, making them theme-aware and reusable across all controls. All DropShadowEffect definitions MUST be centralized — no inline shadows allowed.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Theme/Effects/Shadows.xaml` (new — centralized shadow definitions)
  - `WpfApp2/Theme/Effects/Glow.xaml` (new — centralized glow effects)
  - `WpfApp2/Theme/Effects/Animations.xaml` (new or enhanced)
  - `WpfApp2/Theme/Fluent/FluentEffects.xaml` (migrate from)
  - All control templates (remove inline shadows)

---

## Tasks

### T007 [P] Create Shadows.xaml
**File**: `WpfApp2/Theme/Effects/Shadows.xaml`

Centralize all DropShadowEffect definitions:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Window Shadow - large, soft -->
    <DropShadowEffect x:Key="Shadow.Window"
                      BlurRadius="30" ShadowDepth="8"
                      Opacity="0.4" Color="#000000"/>

    <!-- Popup Shadow - small, subtle -->
    <DropShadowEffect x:Key="Shadow.Popup.Small"
                      BlurRadius="12" ShadowDepth="2"
                      Opacity="0.25" Color="#000000"/>

    <!-- Popup Shadow - large (for dropdowns) -->
    <DropShadowEffect x:Key="Shadow.Popup"
                      BlurRadius="15" ShadowDepth="3"
                      Opacity="0.3" Color="#000000"/>

    <!-- Card Shadow -->
    <DropShadowEffect x:Key="Shadow.Card"
                      BlurRadius="16" ShadowDepth="4"
                      Opacity="0.25" Color="#000000"/>

    <!-- Small Shadow -->
    <DropShadowEffect x:Key="Shadow.Small"
                      BlurRadius="8" ShadowDepth="2"
                      Opacity="0.2" Color="#000000"/>

    <!-- Medium Shadow -->
    <DropShadowEffect x:Key="Shadow.Medium"
                      BlurRadius="12" ShadowDepth="3"
                      Opacity="0.25" Color="#000000"/>

    <!-- Large Shadow -->
    <DropShadowEffect x:Key="Shadow.Large"
                      BlurRadius="20" ShadowDepth="5"
                      Opacity="0.3" Color="#000000"/>

</ResourceDictionary>
```

### T008 [P] Create Glow.xaml
**File**: `WpfApp2/Theme/Effects/Glow.xaml`

Centralize glow effects for selection, focus, and accent states:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Focus Glow - keyboard focus indicator -->
    <DropShadowEffect x:Key="Glow.Focus"
                      Color="#3A86FF" BlurRadius="10"
                      ShadowDepth="0" Opacity="0.35"/>

    <!-- Button Hover Glow -->
    <DropShadowEffect x:Key="Glow.ButtonHover"
                      Color="#3A86FF" BlurRadius="8"
                      ShadowDepth="0" Opacity="0.3"/>

    <!-- Primary Button Glow -->
    <DropShadowEffect x:Key="Glow.Primary"
                      Color="#3A86FF" BlurRadius="12"
                      ShadowDepth="0" Opacity="0.4"/>

    <!-- Selection Glow -->
    <DropShadowEffect x:Key="Glow.Selection"
                      Color="#3A86FF" BlurRadius="6"
                      ShadowDepth="0" Opacity="0.5"/>

    <!-- Accent Glow -->
    <DropShadowEffect x:Key="Glow.Accent"
                      Color="#3A86FF" BlurRadius="8"
                      ShadowDepth="0" Opacity="0.4"/>

    <!-- Theme Card Selection Glow -->
    <DropShadowEffect x:Key="Glow.ThemeCard.Selected"
                      Color="#3A86FF" BlurRadius="14"
                      ShadowDepth="0" Opacity="0.45"/>

</ResourceDictionary>
```

### T009 [P] Migrate FluentEffects to new structure
**File**: `WpfApp2/Theme/Fluent/FluentEffects.xaml`

Import new Effects dictionaries and remove duplicate inline definitions from control templates:
```xaml
<!-- After: FluentEffects.xaml should just import centralized effects -->
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/Som3a_WPF_UI;component/Theme/Effects/Shadows.xaml"/>
    <ResourceDictionary Source="/Som3a_WPF_UI;component/Theme/Effects/Glow.xaml"/>
    <ResourceDictionary Source="/Som3a_WPF_UI;component/Theme/Effects/Animations.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

Audit all control templates and remove any inline `DropShadowEffect` definitions, replacing with DynamicResource references to centralized shadows.

### T010 [P] Create Animations.xaml
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

Create comprehensive animation library:
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Easing Functions -->
    <CubicEase x:Key="AnimEase" EasingMode="EaseOut"/>
    <QuadraticEase x:Key="AnimPressEase" EasingMode="EaseOut"/>
    <CubicEase x:Key="AnimFadeEase" EasingMode="EaseInOut"/>

    <!-- Hover Transitions (150ms) -->
    <Storyboard x:Key="HoverEnter">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         To="0.8" Duration="0:0:0.15"/>
    </Storyboard>
    <Storyboard x:Key="HoverExit">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         To="1.0" Duration="0:0:0.15"/>
    </Storyboard>

    <!-- Focus Transitions -->
    <Storyboard x:Key="FocusEnter">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         To="1.0" Duration="0:0:0.1"/>
    </Storyboard>
    <Storyboard x:Key="FocusExit">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         To="0.7" Duration="0:0:0.1"/>
    </Storyboard>

    <!-- Popup Open/Close (150ms - per performance budget) -->
    <Storyboard x:Key="PopupOpen">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                        From="0" To="1" Duration="0:0:0.15">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    <Storyboard x:Key="PopupClose">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                        From="1" To="0" Duration="0:0:0.1"/>
    </Storyboard>

    <!-- Fade Transitions (200ms max - per performance budget) -->
    <Storyboard x:Key="FadeIn">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                        From="0" To="1" Duration="0:0:0.2"/>
    </Storyboard>
    <Storyboard x:Key="FadeOut">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                        From="1" To="0" Duration="0:0:0.15"/>
    </Storyboard>

    <!-- Scale Transitions (for theme card selection) -->
    <Storyboard x:Key="ScaleIn">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                        To="1.02" Duration="0:0:0.15">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                        To="1.02" Duration="0:0:0.15">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>

    <!-- Border Color Transitions -->
    <Storyboard x:Key="BorderFocusEnter">
        <ColorAnimation Storyboard.TargetProperty="(Border.BorderBrush).(SolidColorBrush.Color)"
                        To="#3A86FF" Duration="0:0:0.1"/>
    </Storyboard>

</ResourceDictionary>
```

---

## Dependency Order

T007, T008, T010 → T009 (migrate FluentEffects after structures defined)

---

## Acceptance Criteria

- [ ] All DropShadowEffect definitions centralized in Effects/Shadows.xaml
- [ ] All glow effects centralized in Effects/Glow.xaml
- [ ] All animations centralized in Effects/Animations.xaml
- [ ] Proper naming: Shadow.* and Glow.*
- [ ] No inline DropShadowEffect definitions in any control template
- [ ] All effects use DynamicResource where applicable
- [ ] Animation library complete and reusable
- [ ] No duplicate shadow definitions across files
- [ ] All animations ≤ 200ms (per Performance Budget Rules)

---

## Constitution Check

Per constitution:
- **Performance Budget Rules**: No nested DropShadows, no BlurEffect on scrollable containers, all animations ≤ 200ms
- **Design Authority Rules**: OpenCode MUST NOT add inline DropShadowEffect definitions
- **Incremental Migration Rules**: DO NOT migrate multiple windows before Excel-host validation succeeds

(End of file — total 157 lines)