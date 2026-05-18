# Phase 8: Advanced Polish

**Branch**: `feature/advanced-polish`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Final polish phase for Fluent Design perfection: acrylic effects, glow system, keyboard navigation, and accessibility improvements.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**: Various

---

## Tasks

### T033 Add acrylic-inspired depth effects
**Files**: `WpfApp2/Theme/Fluent/FluentEffects.xaml` or new effects file

Create pseudo-acrylic backgrounds:
```xaml
<!-- Semi-transparent blur background -->
<SolidColorBrush x:Key="AcrylicSurface" Color="#E0102030"/>
```

Apply to:
- Title bars
- Sidebars
- Overlays

### T034 Enhance glow system for selection
**File**: `WpfApp2/Theme/Effects/Glow.xaml`

Add selection-specific glows for:
- Selected list items
- Active tabs
- Focused inputs
- Toggle states

### T035 Keyboard navigation audit
**Files**: All control styles

Ensure:
- Tab order logical
- Arrow keys work in lists
- Enter/Space activates buttons
- Escape closes popups
- Focus indicators visible

### T036 Accessibility improvements
**Files**: Base tokens, all controls

Improvements:
- Contrast ratio ≥ 4.5:1 for text
- Focus indicators meet WCAG 2.1
- Screen reader support (AutomationProperties)
- Reduced motion support (check animation toggle)

---

## Dependency Order
T033, T034, T035, T036 (can be parallel)

---

## Acceptance Criteria
- [ ] Acrylic-like depth achieved on surfaces
- [ ] Selection glows consistent
- [ ] Full keyboard navigation working
- [ ] Accessibility compliance improved
- [ ] All polish items feel cohesive