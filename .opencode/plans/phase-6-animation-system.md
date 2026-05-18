# Phase 6: Animation & Motion

**Branch**: `feature/animation-system`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Build a comprehensive animation library for smooth, subtle motion throughout the application following Fluent Design principles.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Theme/Effects/Animations.xaml` (enhance)
  - `WpfApp2/Theme/Effects/EasingFunctions.xaml` (new)

---

## Tasks

### T025 Add slide animations
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

```xaml
<!-- Popup slide down -->
<Storyboard x:Key="PopupSlideIn">...</Storyboard>
<Storyboard x:Key="PopupSlideOut">...</Storyboard>

<!-- Page transitions -->
<Storyboard x:Key="PageSlideLeft">...</Storyboard>
<Storyboard x:Key="PageSlideRight">...</Storyboard>
```

### T026 Add fade transitions
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

```xaml
<Storyboard x:Key="FadeIn">
    <DoubleAnimation Storyboard.TargetProperty="Opacity" From="0" To="1" Duration="0:0:0.2"/>
</Storyboard>

<Storyboard x:Key="FadeOut">
    <DoubleAnimation Storyboard.TargetProperty="Opacity" From="1" To="0" Duration="0:0:0.15"/>
</Storyboard>
```

### T027 Add elevation transitions
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

Smooth shadow/opacity transitions for focus states.

### T028 Create easing functions library
**File**: `WpfApp2/Theme/Effects/EasingFunctions.xaml`

```xaml
<CubicEase x:Key="EaseOut" EasingMode="EaseOut"/>
<CubicEase x:Key="EaseIn" EasingMode="EaseIn"/>
<QuadraticEase x:Key="EaseInOut" EasingMode="EaseInOut"/>
<BackEase x:Key="EaseBack" EasingMode="EaseOut" Amplitude="0.5"/>
```

---

## Dependency Order
T028 → T025, T026, T027 (all can be parallel after easing functions)

---

## Acceptance Criteria
- [ ] All animations subtle (<200ms duration)
- [ ] Consistent easing functions
- [ ] No bouncy or heavy effects
- [ ] Animations respect system settings (future: animation toggle)
- [ ] Reusable via StaticResource keys