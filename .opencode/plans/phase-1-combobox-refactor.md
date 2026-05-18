# Phase 1: ComboBox Refactor

**Branch**: `feature/combobox-fix`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Fix the custom WPF ComboBox to become a production-ready Fluent UI control with proper popup rendering, shadow support, all visual states, and DynamicResource-only theming.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
  - `WpfApp2/Theme/Controls/ComboBoxItemStyles.xaml`
  - `WpfApp2/Theme/Effects/Shadows.xaml` (new)

---

## Tasks

### T001 [P] Create Shadows.xaml with popup shadow definition
**File**: `WpfApp2/Theme/Effects/Shadows.xaml`
Create centralized shadow effects:
```xaml
<DropShadowEffect x:Key="Shadow.Popup" BlurRadius="15" ShadowDepth="3" Opacity="0.3"/>
```

### T002 [P] Refactor ComboBoxStyles.xaml - Template Structure
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
- Add `PopupOpen` state with accent border
- Add `Pressed` state handling
- Add drop shadow to popup via DynamicResource
- Ensure `AllowsTransparency="True"` with proper popup stacking
- Fix popup placement for transparent windows

### T003 [P] Replace hardcoded colors with DynamicResource
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
Replace:
- `#15FFFFFF` → `{DynamicResource ComboBoxItemHoverBrush}`
- `#33FFFFFF` → `{DynamicResource ComboBoxItemSelectedBrush}`

### T004 [P] Add keyboard accessibility states
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
Add focus ring with 2px accent border on keyboard focus.

### T005 Refactor ComboBoxItemStyles.xaml - Selection & Hover
**File**: `WpfApp2/Theme/Controls/ComboBoxItemStyles.xaml`
- Hover: Use DynamicResource brush
- Selected: Use Accent brush with opacity
- Add focus visual indicator
- Add disabled state
- Add smooth transition animation

### T006 Add Popup animations
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
Update Popup animation with slide-down entry and fade-in effect (150ms).

---

## Dependency Order
1. T001 → T002 (shadow reference)
2. T002, T003, T004 → T005 (parallel - same file region)
3. T006 → T002 (animation in popup)

---

## Acceptance Criteria
- [ ] Popup renders above all containers in transparent borderless windows
- [ ] Popup has visible drop shadow
- [ ] All states work: Normal, Hover, Focused, Pressed, Disabled, PopupOpen
- [ ] No hardcoded color values
- [ ] All brushes use DynamicResource
- [ ] Keyboard navigation works (Tab, Arrow keys, Escape to close)
- [ ] Dark/Light theme compatible (via DynamicResource)

---

## Constitution Check
Per constitution Principle III (DynamicResource Only) and Principle V (Feature Completeness):
- All color values MUST use DynamicResource
- Control MUST be theme-aware and runtime switchable