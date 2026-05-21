# Popup Architecture

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Target**: Excel VSTO-hosted WPF windows

---

## 1. Core Popup Rules

Every Popup MUST follow these rules:

### Transparency

```xml
AllowsTransparency="False"
```

`True` is allowed ONLY when transparent blending is explicitly required AND tested in Excel host.

### Placement

```xml
Placement="Bottom"
PlacementTarget="{Binding RelativeSource={RelativeSource TemplatedParent}}"
```

### Shadow

```xml
Effect="{DynamicResource Shadow.Popup}"
```

Never inline `DropShadowEffect` in popup templates.

### Keyboard Navigation

- Arrow keys navigate items
- Escape closes popup
- Tab moves to next focusable element
- Enter/Space selects item

---

## 2. ComboBox Popup Architecture

### Template Requirements

```xml
<Popup x:Name="PART_Popup"
       Placement="Bottom"
       PlacementTarget="{Binding RelativeSource={RelativeSource TemplatedParent}}"
       AllowsTransparency="False"
       Focusable="False"
       PopupAnimation="Fade"
       IsOpen="{TemplateBinding IsDropDownOpen}">
    <Border Margin="0,6,0,0"
            Background="{DynamicResource ComboBoxPopupBackground}"
            BorderBrush="{DynamicResource ComboBoxPopupBorder}"
            BorderThickness="1"
            CornerRadius="{DynamicResource MediumRadius}"
            Effect="{DynamicResource Shadow.Popup}">
        <ScrollViewer MaxHeight="250"
                      VerticalScrollBarVisibility="Auto"
                      HorizontalScrollBarVisibility="Disabled"
                      CanContentScroll="False">
            <ItemsPresenter/>
        </ScrollViewer>
    </Border>
</Popup>
```

### Critical Requirements

| Requirement | Why |
|-------------|-----|
| `AllowsTransparency="False"` | Prevents rendering artifacts in Excel host |
| `PlacementTarget` binding | Ensures correct positioning relative to parent |
| `Margin="0,6,0,0"` | Visual gap between ComboBox and popup |
| `MaxHeight="250"` | Prevents popup from exceeding screen bounds |
| `CornerRadius` from token | Consistent rounding with design system |
| Centralized shadow effect | Consistent shadow appearance |

---

## 3. Width Synchronization

Popup width MUST match or exceed the ComboBox width to prevent content clipping.

```xml
<Popup MinWidth="{TemplateBinding ActualWidth}" ...>
```

---

## 4. Excel Host Considerations

### Z-Order

Popups in Excel-hosted WPF may appear behind Excel window if not handled correctly.

Mitigations:
- Ensure parent window is `Topmost="True"` during popup display
- Use `WindowInteropHelper` to verify HWND z-order if issues arise

### DPI Scaling

- No hardcoded pixel values
- Use `DynamicResource` for all sizes
- Test at 100%, 125%, 150% scaling

### Clipping

- Ensure popup is not clipped by parent `ClipToBounds="True"` containers
- Use `AdornerLayer` or separate window if necessary for complex scenarios

---

## 5. Validation Checklist

- [ ] `AllowsTransparency="False"` on all popups
- [ ] `PlacementTarget` correctly bound
- [ ] Centralized shadow effect used
- [ ] Keyboard navigation works (arrows, escape, tab, enter)
- [ ] Width synchronized with parent
- [ ] Tested in Excel VSTO host
- [ ] No rendering artifacts at any DPI scale

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Performance and rendering budget |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
