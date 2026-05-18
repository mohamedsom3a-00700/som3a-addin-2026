# Phase 6: Animation & Motion

**Branch**: `feature/animation-system`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Build a comprehensive animation library for smooth, subtle motion throughout the application following Fluent Design principles. All animations MUST be subtle (≤200ms) per the Performance Budget Rules.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Theme/Effects/Animations.xaml` (enhance from Phase 2)
  - `WpfApp2/Theme/Effects/EasingFunctions.xaml` (new or consolidated)
  - All control templates referencing animations

---

## Tasks

### T025 Add slide animations
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

```xaml
<!-- Popup slide down (150ms) -->
<Storyboard x:Key="PopupSlideIn">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                    From="-10" To="0" Duration="0:0:0.15">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="0" To="1" Duration="0:0:0.15"/>
</Storyboard>

<Storyboard x:Key="PopupSlideOut">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                    From="0" To="-10" Duration="0:0:0.1"/>
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="1" To="0" Duration="0:0:0.1"/>
</Storyboard>

<!-- Page transitions -->
<Storyboard x:Key="PageSlideLeft">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                    From="50" To="0" Duration="0:0:0.2">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="0.5" To="1" Duration="0:0:0.2"/>
</Storyboard>

<Storyboard x:Key="PageSlideRight">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                    From="-50" To="0" Duration="0:0:0.2">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="0.5" To="1" Duration="0:0:0.2"/>
</Storyboard>
```

### T026 Add fade transitions
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

```xaml
<Storyboard x:Key="FadeIn">
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="0" To="1" Duration="0:0:0.2"/>
</Storyboard>

<Storyboard x:Key="FadeOut">
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                    From="1" To="0" Duration="0:0:0.15"/>
</Storyboard>
```

### T027 Add elevation transitions
**File**: `WpfApp2/Theme/Effects/Animations.xaml`

Smooth shadow/opacity transitions for focus states:
```xaml
<!-- Elevation up (focus, hover) -->
<Storyboard x:Key="ElevationUp">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.BlurRadius)"
                    To="16" Duration="0:0:0.15">
        <DoubleAnimation.EasingFunction>
            <QuadraticEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.Opacity)"
                    To="0.3" Duration="0:0:0.15"/>
</Storyboard>

<!-- Elevation down (unfocus) -->
<Storyboard x:Key="ElevationDown">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.BlurRadius)"
                    To="8" Duration="0:0:0.1"/>
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.Opacity)"
                    To="0.2" Duration="0:0:0.1"/>
</Storyboard>
```

### T028 Create easing functions library
**File**: `WpfApp2/Theme/Effects/EasingFunctions.xaml` (or consolidate into Animations.xaml)

```xaml
<CubicEase x:Key="EaseOut" EasingMode="EaseOut"/>
<CubicEase x:Key="EaseIn" EasingMode="EaseIn"/>
<QuadraticEase x:Key="EaseInOut" EasingMode="EaseInOut"/>
<BackEase x:Key="EaseBack" EasingMode="EaseOut" Amplitude="0.5"/>
```

---

## Dependency Order

T028 → T025, T026, T027 (all can be parallel after easing functions defined)

---

## Performance Budget Rules — Animation Restrictions

```text
- All animations ≤ 200ms (hard limit)
- AVOID animations on DataGrid rows
- AVOID BlurEffect on scrolling containers
- AVOID nested DropShadows
- Subtle only: Fade, Opacity, Translate, Glow transition, Border transition
- AVOID: Large scaling, Bouncy effects, Heavy blur animations
```

---

## Acceptance Criteria

- [ ] All animations ≤ 200ms duration (enforced)
- [ ] Consistent easing functions
- [ ] No bouncy or heavy effects
- [ ] No animations on DataGrid rows
- [ ] Reusable via StaticResource keys
- [ ] All animations documented with duration and easing

---

## Constitution Check

Per constitution:
- **Animation Rules**: Animations MUST be subtle. Allowed: Fade, Opacity, Translate, Glow transition, Border transition. Avoid: Large scaling, Bouncy effects, Heavy blur animations ✅
- **Performance Budget Rules**: AVOID BlurEffect on scrolling containers, no nested DropShadows ✅
- **Design Authority Rules**: OpenCode MUST NOT add heavy animations without performance validation ✅

(End of file — total 134 lines)