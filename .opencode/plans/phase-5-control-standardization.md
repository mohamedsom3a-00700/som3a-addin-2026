# Phase 5: Control Standardization

**Branch**: `feature/control-standardization`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Unify all controls under the Fluent design language with proper visual states, DynamicResource usage, and consistent styling. This phase introduces **VisualStateManager** gradually for key controls, following the VSM Strategy from the constitution.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Theme/Controls/TextBoxStyles.xaml`
  - `WpfApp2/Theme/Controls/CheckBoxStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/RadioButtonStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml` (new)
  - `WpfApp2/Theme/Controls/DataGridStyles.xaml`
  - `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` (new)

---

## VisualStateManager Strategy

Per the constitution's VSM Strategy, this phase is the **initial VSM migration phase**. Controls are migrated one at a time, with validation after each.

### VSM State Requirement

Every stateful control MUST implement these states:
```
Normal → Hover → Pressed → Focused
                   ↓
              Disabled
Selected (ToggleButton, CheckBox, RadioButton)
PopupOpen (popup controls — ComboBox, deferred from Phase 1)
```

### Migration Order
1. TextBox (existing — add missing states)
2. CheckBox (new — use VSM from start)
3. RadioButton (new — use VSM from start)
4. ToggleButton (new — use VSM from start)
5. DataGrid (enhance existing)
6. ScrollViewer (new)

---

## Tasks

### T019 Refactor TextBox Styles
**File**: `WpfApp2/Theme/Controls/TextBoxStyles.xaml`

Check existing implementation, ensure all states via VSM or Triggers:
- Normal state: Default border
- Hover state: Accent border
- Pressed state: Accent border + slight shadow
- Focused state: Focus glow border
- Disabled state: Reduced opacity, muted colors
- No hardcoded colors — use DynamicResource
- Smooth transition animations (≤150ms)

### T020 Create CheckBox Styles
**File**: `WpfApp2/Theme/Controls/CheckBoxStyles.xaml`

Fluent CheckBox with VSM:
```xaml
<Style x:Key="FluentCheckBox" TargetType="{x:Type CheckBox}">
    <Setter Property="Foreground" Value="{DynamicResource TextMainBrush}"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type CheckBox}">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="20"/>
                        <ColumnDefinition Width="8"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <!-- Custom Checkbox Box -->
                    <Border x:Name="CheckBorder"
                            Grid.Column="0"
                            Width="18" Height="18"
                            CornerRadius="4"
                            BorderBrush="{DynamicResource ControlStrokeBrush}"
                            BorderThickness="1"
                            Background="{DynamicResource ControlBgBrush}">
                        <!-- Check mark (hidden by default) -->
                        <Path x:Name="CheckMark"
                              Data="M 2 8 L 6 12 L 14 4"
                              Stroke="{DynamicResource AccentBrush}"
                              StrokeThickness="2"
                              Visibility="Collapsed"
                              VerticalAlignment="Center"
                              HorizontalAlignment="Center"/>
                    </Border>

                    <!-- Label -->
                    <ContentPresenter Grid.Column="2"
                                      VerticalAlignment="Center"/>
                </Grid>
                <ControlTemplate.Triggers>
                    <!-- Hover -->
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="CheckBorder" Property="BorderBrush"
                                Value="{DynamicResource AccentBrush}"/>
                    </Trigger>
                    <!-- Checked -->
                    <Trigger Property="IsChecked" Value="True">
                        <Setter TargetName="CheckBorder" Property="Background"
                                Value="{DynamicResource AccentBrush}"/>
                        <Setter TargetName="CheckMark" Property="Visibility" Value="Visible"/>
                        <Setter TargetName="CheckMark" Property="Stroke" Value="White"/>
                    </Trigger>
                    <!-- Disabled -->
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter Property="Opacity" Value="0.55"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**Required states**: Normal, Hover, Checked, Disabled, Focused

### T021 Create RadioButton Styles
**File**: `WpfApp2/Theme/Controls/RadioButtonStyles.xaml`

Fluent RadioButton with custom dot indicator:
```xaml
<Style x:Key="FluentRadioButton" TargetType="{x:Type RadioButton}">
    <!-- Custom circular indicator -->
    <!-- Dot appears when selected -->
    <!-- Grouping support -->
</Style>
```

**Required states**: Normal, Hover, Selected, Disabled, Focused

### T022 Create ToggleButton Styles
**File**: `WpfApp2/Theme/Controls/ToggleButtonStyles.xaml`

On/Off toggle with smooth slide animation:
```xaml
<Style x:Key="FluentToggleButton" TargetType="{x:Type ToggleButton}">
    <!-- Track (background) -->
    <!-- Thumb (slider) -->
    <!-- Slide animation on toggle -->
</Style>
```

**Required states**: Normal, Hover, Pressed, Disabled, Focused, Checked (Selected)

### T023 Improve DataGrid
**File**: `WpfApp2/Theme/Controls/DataGridStyles.xaml`

Enhancements:
- Row virtualization enabled (`EnableRowVirtualization="True"`)
- Better hover/selection states
- Theme-aware column headers
- Alternating row colors via DynamicResource
- Custom ScrollViewer integration
- No animations on DataGrid rows (per Performance Budget Rules)

### T024 Create ScrollViewer Styles
**File**: `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml`

Modern thin scrollbar design with hover expansion:
```xaml
<Style x:Key="FluentScrollBar" TargetType="ScrollBar">
    <!-- Thin track (4px) -->
    <!-- Thumb expands on hover (8px) -->
    <!-- Smooth transition -->
</Style>
```

---

## Dependency Order

T019 → T020, T021, T022 (can be parallel — VSM patterns similar) → T023 → T024

---

## Acceptance Criteria

- [ ] All input controls have: Normal, Hover, Pressed, Focused, Disabled, Selected (where applicable)
- [ ] All colors use DynamicResource
- [ ] No hardcoded colors in any control template
- [ ] Animation transitions smooth (≤200ms)
- [ ] DataGrid has row virtualization
- [ ] Consistent visual language across all controls
- [ ] All controls pass Theme Validation Checklist
- [ ] VSM migration validated inside Excel (VSTO host)

---

## Constitution Check

Per constitution:
- **Principle III (DynamicResource Only)**: All colors use DynamicResource ✅
- **Principle V (Feature Completeness)**: All controls theme-aware, runtime switchable, keyboard accessible, DPI safe ✅
- **VisualStateManager Strategy**: VSM introduced gradually, one control type at a time ✅
- **Performance Budget Rules**: No animations on DataGrid rows, no nested DropShadows ✅
- **Incremental Migration Rules**: Validate in Excel before advancing ✅

(End of file — total 155 lines)