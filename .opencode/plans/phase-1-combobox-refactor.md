# Phase 1: ComboBox Refactor

**Branch**: `feature/combobox-fix`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Fix the custom WPF ComboBox to become a production-ready Fluent UI control with proper popup rendering, shadow support, all visual states, and DynamicResource-only theming. This phase follows the Incremental Migration Rules from the constitution.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
  - `WpfApp2/Theme/Controls/ComboBoxItemStyles.xaml`
  - `WpfApp2/Theme/Effects/Shadows.xaml` (may be created in Phase 2 or referenced from existing)

---

## Tasks

### T001 [P] Create Shadows.xaml with popup shadow definition
**File**: `WpfApp2/Theme/Effects/Shadows.xaml`
Create centralized shadow effects:
```xaml
<!-- Popup Shadow - small, subtle -->
<DropShadowEffect x:Key="Shadow.Popup.Small" BlurRadius="12" ShadowDepth="2" Opacity="0.25" Color="#000000"/>

<!-- Window Shadow -->
<DropShadowEffect x:Key="Shadow.Window" BlurRadius="30" ShadowDepth="8" Opacity="0.4" Color="#000000"/>

<!-- Card Shadow -->
<DropShadowEffect x:Key="Shadow.Card" BlurRadius="16" ShadowDepth="4" Opacity="0.25" Color="#000000"/>
```

### T002 [P] Refactor ComboBoxStyles.xaml - Popup Architecture
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`

Per the Popup Architecture Rules (Constitution):
- Popup uses `AllowsTransparency="False"` (or minimal usage)
- Popup uses correct `Placement="Bottom"`
- Popup uses correct `PlacementTarget`
- Popup renders in top visual layer without clipping
- Popup shadow from `Effects/Shadows.xaml` (not inline)
- Popup supports keyboard navigation

Refactor the popup section:
```xaml
<Popup x:Name="PART_Popup"
       AllowsTransparency="False"
       IsOpen="{TemplateBinding IsDropDownOpen}"
       Placement="Bottom"
       PlacementTarget="{Binding RelativeSource={RelativeSource TemplatedParent}}"
       PopupAnimation="Fade">
    <Border Margin="0,6,0,0"
            Background="{DynamicResource CardBrush}"
            BorderBrush="{DynamicResource ControlStrokeBrush}"
            BorderThickness="1"
            CornerRadius="{DynamicResource MediumRadius}"
            Effect="{DynamicResource Shadow.Popup.Small}">
        <ScrollViewer MaxHeight="200">
            <ItemsPresenter/>
        </ScrollViewer>
    </Border>
</Popup>
```

### T003 [P] Replace hardcoded colors with DynamicResource
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`

Replace:
- `#15FFFFFF` → `{DynamicResource WhiteAlpha10}` ✅ Already done
- `#33FFFFFF` → `{DynamicResource WhiteAlpha20}` ✅ Already done

Verify no other hardcoded colors remain.

### T004 [P] Add keyboard accessibility states
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
Add focus ring with 2px accent border on keyboard focus.

**Note**: VisualStateManager migration is deferred to Phase 5 per Incremental Migration Rules. For now, use Trigger-based states:
- Hover: Accent border
- Keyboard focus: Accent border + focus indicator
- Disabled: Opacity 0.55

### T005 Refactor ComboBoxItemStyles.xaml - Selection & Hover
**File**: `WpfApp2/Theme/Controls/ComboBoxItemStyles.xaml`
- Hover: Use `DynamicResource WhiteAlpha10` ✅ Already done in ComboBoxStyles.xaml
- Selected: Use accent brush with opacity
- Add focus visual indicator
- Add disabled state ✅ Already added
- Smooth transition animation (defer to Phase 6)

### T006 Add Popup animations
**File**: `WpfApp2/Theme/Controls/ComboBoxStyles.xaml`
Update Popup animation with slide-down entry and fade-in effect (150ms).

Reference animations from `Effects/Animations.xaml` if available, otherwise define inline with easing:
```xaml
<CubicEase x:Key="PopupEase" EasingMode="EaseOut"/>
```

---

## Dependency Order

1. T001 → T002 (shadow reference, may use existing shadow if Shadows.xaml not yet created)
2. T002, T003, T004, T005 → T006 (parallel - popup structure first)
3. T006 → T002 (animation in popup)

---

## Acceptance Criteria

- [ ] Popup renders in top layer without clipping from parent containers
- [ ] Popup has visible drop shadow (from centralized Effects/Shadows.xaml)
- [ ] All states work: Normal, Hover, Focused, Pressed, Disabled, PopupOpen
- [ ] No hardcoded color values
- [ ] All brushes use DynamicResource
- [ ] Keyboard navigation works (Tab, Arrow keys, Escape to close)
- [ ] Dark/Light theme compatible (via DynamicResource)
- [ ] Popup respects WindowChrome hosting (no AllowsTransparency conflicts)
- [ ] DPI scaling correct (no hardcoded pixel values in popup)

---

## Incremental Migration Gate

This phase represents the **first window migration**. Before advancing to Phase 2:
1. Build succeeds
2. ComboBox validates inside Excel (VSTO host)
3. DPI behavior validated at 100%, 125%, 150%, 200%
4. Popup renders correctly over all containers
5. Keyboard navigation tested

---

## Constitution Check

Per constitution:
- **Principle III (DynamicResource Only)**: All color values MUST use DynamicResource ✅
- **Principle V (Feature Completeness)**: Control MUST be theme-aware, runtime switchable, keyboard accessible, DPI safe
- **Popup Architecture Rules**: No inline DropShadowEffect, correct Placement/PlacementTarget
- **Performance Budget Rules**: No nested DropShadows, minimal visual tree
- **Incremental Migration Rules**: DO NOT migrate multiple windows before Excel-host validation succeeds

(End of file — total 136 lines)