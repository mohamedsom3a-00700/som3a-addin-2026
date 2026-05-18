# Phase 5: Control Standardization

**Branch**: `feature/control-standardization`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Unify all controls under the Fluent design language with proper visual states, DynamicResource usage, and consistent styling.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Theme/Controls/TextBoxStyles.xaml`
  - `WpfApp2/Theme/Controls/CheckBoxStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/RadioButtonStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/DataGridStyles.xaml`
  - `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` (new)

---

## Tasks

### T019 Refactor TextBox Styles
**File**: `WpfApp2/Theme/Controls/TextBoxStyles.xaml`

Check existing implementation, ensure:
- Hover state with accent border
- Focus state with focus glow
- Disabled state with reduced opacity
- No hardcoded colors
- Smooth transition animations

### T020 Create CheckBox Styles
**File**: `WpfApp2/Theme/Controls/CheckBoxStyles.xaml`

Create fluent CheckBox:
```xaml
<Style x:Key="FluentCheckBox" TargetType="{x:Type CheckBox}">
    <!-- Custom check mark design -->
    <!-- Accent highlight on check -->
    <!-- Proper spacing and sizing -->
</Style>
```

Required states: Normal, Hover, Checked, Disabled

### T021 Create RadioButton Styles
**File**: `WpfApp2/Theme/Controls/RadioButtonStyles.xaml`

Fluent RadioButton with custom dot indicator.
Required states: Normal, Hover, Selected, Disabled

### T022 Create ToggleButton Styles
**File**: `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml`

On/Off toggle with smooth slide animation.

### T023 Improve DataGrid
**File**: `WpfApp2/Theme/Controls/DataGridStyles.xaml`

Enhancements:
- Row virtualization enabled
- Better hover/selection states
- Theme-aware column headers
- Alternating row colors via DynamicResource
- Custom ScrollViewer integration

### T024 Create ScrollViewer Styles
**File**: `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml`

Modern thin scrollbar design with hover expansion.

---

## Dependency Order
T019 → T020, T021, T022 (can be parallel) → T023 → T024

---

## Acceptance Criteria
- [ ] All input controls have Hover, Focus, Pressed, Disabled states
- [ ] All colors use DynamicResource
- [ ] Animation transitions smooth (<200ms)
- [ ] DataGrid has virtualization
- [ ] Consistent visual language across all controls