# Phase 2: Shadow System

**Branch**: `feature/shadow-system`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Centralize and organize all shadow effects with proper naming convention, making them theme-aware and reusable across all controls.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Theme/Effects/Shadows.xaml` (new)
  - `WpfApp2/Theme/Effects/Glow.xaml` (new)
  - `WpfApp2/Theme/Effects/Animations.xaml` (new)
  - `WpfApp2/Theme/Fluent/FluentEffects.xaml` (migrate from)

---

## Tasks

### T007 [P] Create Shadows.xaml
**File**: `WpfApp2/Theme/Effects/Shadows.xaml`

Organize shadow definitions:
```xaml
<!-- Window Shadow -->
<DropShadowEffect x:Key="Shadow.Window" BlurRadius="30" ShadowDepth="8" Opacity="0.4"/>

<!-- Popup Shadow -->
<DropShadowEffect x:Key="Shadow.Popup" BlurRadius="15" ShadowDepth="3" Opacity="0.3"/>

<!-- Card Shadow -->
<DropShadowEffect x:Key="Shadow.Card" BlurRadius="16" ShadowDepth="4" Opacity="0.25"/>

<!-- Small Shadow -->
<DropShadowEffect x:Key="Shadow.Small" BlurRadius="8" ShadowDepth="2" Opacity="0.2"/>

<!-- Medium Shadow -->
<DropShadowEffect x:Key="Shadow.Medium" BlurRadius="12" ShadowDepth="3" Opacity="0.25"/>

<!-- Large Shadow -->
<DropShadowEffect x:Key="Shadow.Large" BlurRadius="20" ShadowDepth="5" Opacity="0.3"/>
```

### T008 [P] Create Glow.xaml
**File**: `WpfApp2/Theme/Effects/Glow.xaml`

Centralize glow effects:
```xaml
<!-- Focus Glow (keyboard focus indicator) -->
<DropShadowEffect x:Key="Glow.Focus" Color="#3A86FF" BlurRadius="10" ShadowDepth="0" Opacity="0.35"/>

<!-- Button Hover Glow -->
<DropShadowEffect x:Key="Glow.ButtonHover" Color="#3A86FF" BlurRadius="8" ShadowDepth="0" Opacity="0.3"/>

<!-- Primary Button Glow -->
<DropShadowEffect x:Key="Glow.Primary" Color="#3A86FF" BlurRadius="12" ShadowDepth="0" Opacity="0.4"/>

<!-- Selection Glow -->
<DropShadowEffect x:Key="Glow.Selection" Color="#3A86FF" BlurRadius="6" ShadowDepth="0" Opacity="0.5"/>

<!-- Accent Glow -->
<DropShadowEffect x:Key="Glow.Accent" Color="#3A86FF" BlurRadius="8" ShadowDepth="0" Opacity="0.4"/>
```

### T009 [P] Migrate FluentEffects to new structure
**File**: `WpfApp2/Theme/Fluent/FluentEffects.xaml`

Import new Effects and remove duplicates.

### T010 [P] Create Animations.xaml (or enhance existing)
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

Create comprehensive animation library:
```xaml
<!-- Easing Functions -->
<CubicEase x:Key="AnimEase" EasingMode="EaseOut"/>
<QuadraticEase x:Key="AnimPressEase" EasingMode="EaseOut"/>

<!-- Hover Transitions -->
<Storyboard x:Key="HoverEnter">...</Storyboard>
<Storyboard x:Key="HoverExit">...</Storyboard>

<!-- Press Transitions -->
<Storyboard x:Key="PressEnter">...</Storyboard>
<Storyboard x:Key="PressExit">...</Storyboard>

<!-- Focus Transitions -->
<Storyboard x:Key="FocusEnter">...</Storyboard>
<Storyboard x:Key="FocusExit">...</Storyboard>

<!-- Popup Open/Close -->
<Storyboard x:Key="PopupOpen">...</Storyboard>
<Storyboard x:Key="PopupClose">...</Storyboard>
```

---

## Dependency Order
T007, T008, T010 → T009 (migrate after structures defined)

---

## Acceptance Criteria
- [ ] All shadow effects in centralized Effects/ folder
- [ ] Proper naming: Shadow.* and Glow.*
- [ ] All effects use DynamicResource where applicable
- [ ] Animation library complete and reusable
- [ ] No duplicate shadow definitions across files