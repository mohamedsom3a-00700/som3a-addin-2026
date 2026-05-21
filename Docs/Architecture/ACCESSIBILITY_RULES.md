# Accessibility Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Goal**: Keyboard navigation + screen reader preparation

---

## 1. Keyboard Navigation

### Tab Order

All interactive controls MUST be reachable via Tab key in logical order.

```xml
<!-- Set TabIndex explicitly if default order is wrong -->
<TextBox TabIndex="0"/>
<ComboBox TabIndex="1"/>
<Button TabIndex="2"/>
```

### Focus Visual

All focusable controls MUST show a visible focus indicator.

```xml
<Setter Property="FocusVisualStyle" Value="{DynamicResource FocusVisualStyle}"/>
```

Focus visual SHOULD use `Glow.Focus` effect for consistency.

### Activation Keys

| Control | Activation |
|---------|-----------|
| Button | Enter, Space |
| CheckBox | Space |
| RadioButton | Space, Arrow keys (grouped) |
| ComboBox | Alt+Down, Enter to select |
| ListBox | Arrow keys, Enter to select |
| MenuItem | Enter, Arrow keys |

### Escape Behavior

`ModernWindow` already implements `CloseOnEscape`. Popups MUST close on Escape.

---

## 2. Automation Properties

### Name

Every interactive element MUST have a descriptive name.

```xml
<Button Content="Save"
        AutomationProperties.Name="Save changes"/>

<Ellipse Style="{DynamicResource AccentSwatch}"
         AutomationProperties.Name="Blue accent color"/>
```

### HelpText

Complex controls SHOULD provide additional context.

```xml
<ComboBox AutomationProperties.HelpText="Select the sheet containing last update data"/>
```

### LabeledBy

Form inputs SHOULD reference their label.

```xml
<TextBlock x:Name="lblOldSheet" Text="Last Update Sheet"/>
<ComboBox AutomationProperties.LabeledBy="{Binding ElementName=lblOldSheet}"/>
```

---

## 3. Screen Reader Preparation

### LiveRegions

Status updates MUST use `LiveRegion` so screen readers announce them.

```xml
<TextBlock x:Name="StatusText"
           AutomationProperties.LiveSetting="Polite"
           Text="{Binding StatusText}"/>
```

### Hidden Decorative Elements

Purely decorative elements MUST be hidden from screen readers.

```xml
<Border AutomationProperties.IsOffscreenBehavior="FromRenderTransform"
        ... />
```

For icons that are decorative:

```xml
<Image Source="icon.png"
       AutomationProperties.IsOffscreenBehavior="FromRenderTransform"/>
```

---

## 4. High Contrast

### System Colors

When high contrast mode is active, use system colors:

```xml
<SolidColorBrush x:Key="Brush.HighContrast.Background"
                 Color="{DynamicResource {x:Static SystemColors.WindowColorKey}}"/>
```

### Detection

```csharp
var highContrast = SystemParameters.HighContrast;
```

**Note:** Full high contrast support is architecture-ready for future implementation. Current focus is preparation (token structure, detection hook).

---

## 5. Reduced Motion

### Detection

```csharp
var reduceMotion = SystemParameters.IsAnimationEnabled == false;
```

### Behavior

When reduced motion is preferred:

- Skip fade/scale animations
- Use instant state changes
- Disable window open/close animations

---

## 6. Focus Visibility

### Minimum Requirements

- Focus ring MUST be at least 2px thick
- Focus color MUST have 3:1 contrast against background
- Focus indicator MUST be visible on ALL interactive elements

### Glow Effect Focus

```xml
<Style x:Key="FocusVisualStyle">
    <Setter Property="Control.Template">
        <Setter.Value>
            <ControlTemplate>
                <Border BorderBrush="{DynamicResource AccentBrush}"
                        BorderThickness="2"
                        CornerRadius="{DynamicResource MediumRadius}"
                        Margin="-2"
                        Effect="{DynamicResource Glow.Focus}"/>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## 7. Validation Checklist

- [ ] All interactive elements reachable by Tab
- [ ] Focus visual visible on all focusable controls
- [ ] `AutomationProperties.Name` on all interactive elements
- [ ] `AutomationProperties.HelpText` on complex controls
- [ ] Escape closes popups and dialogs
- [ ] Enter/Space activate buttons and toggles
- [ ] Arrow keys navigate lists, menus, radio groups
- [ ] Status updates use `LiveRegion`
- [ ] Decorative elements hidden from screen readers

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [UI_GUIDELINES.md](UI_GUIDELINES.md) | UI design guidelines |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
