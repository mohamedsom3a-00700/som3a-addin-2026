# UI Guidelines

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Style Target**: Windows 11 Fluent Design System

---

## 1. Visual Direction

The UI system must feel like:

- Premium enterprise software
- Modern Windows Fluent application
- High-end desktop productivity suite
- Consistent commercial-grade UI platform

The architecture optimizes for: long-term maintainability, runtime stability, consistency, scalability, and future theme extensibility.

---

## 2. Color Usage

### All colors MUST come from tokens

```xml
<!-- CORRECT -->
<Setter Property="Background" Value="{DynamicResource Brush.Background.Primary}"/>

<!-- INCORRECT -->
<Setter Property="Background" Value="#0E1720"/>
```

### DynamicResource is mandatory for themeable properties

- Background
- Foreground
- BorderBrush
- Fill (paths, shapes)
- Effect colors

### StaticResource is allowed ONLY for

- Primitive token definitions (inside Colors.xaml)
- Converters
- Non-themeable structural values (Grid row heights, Column widths)

---

## 3. Typography

### Font Family

```xml
<FontFamily x:Key="FontFamilyPrimary">Segoe UI</FontFamily>
```

### Font Sizes

| Token | Size | Usage |
|-------|------|-------|
| `TitleFontSize` | 20 | Window titles, major headings |
| `HeaderFontSize` | 16 | Section headers |
| `SubHeaderFontSize` | 14 | Sub-sections, card titles |
| `BodyFontSize` | 13 | Body text, labels, buttons |
| `CaptionFontSize` | 11 | Captions, helper text, timestamps |

### Typography Styles

Use the centralized `Style.Text.*` styles:

```xml
<TextBlock Style="{DynamicResource Style.Text.Title}"/>
<TextBlock Style="{DynamicResource Style.Text.Body}"/>
```

---

## 4. Spacing & Layout

### Spacing Scale

| Token | Value | Usage |
|-------|-------|-------|
| `Spacing.XSmall` | 4 | Tight gaps, icon padding |
| `Spacing.Small` | 8 | Compact element gaps |
| `Spacing.Medium` | 12 | Standard gaps |
| `Spacing.Large` | 16 | Section gaps |
| `Spacing.XLarge` | 20 | Major section separation |
| `Spacing.XXLarge` | 24 | Window edge padding |

### Padding Scale

| Token | Value | Usage |
|-------|-------|-------|
| `Padding.XSmall` | 4 | Minimal padding |
| `Padding.Small` | 8,6 | Buttons, small inputs |
| `Padding.Medium` | 12,8 | Inputs, list items |
| `Padding.Large` | 16,12 | Cards, panels |
| `Padding.XLarge` | 20,16 | Dialogs, major panels |

---

## 5. Corner Radius

| Token | Value | Usage |
|-------|-------|-------|
| `Radius.Small` | 6 | Small buttons, tags |
| `Radius.Medium` | 10 | Inputs, combo boxes |
| `Radius.Large` | 14 | Cards, panels |
| `Radius.Card` | 12 | Theme cards, info cards |
| `Radius.Window` | 18 | Window chrome |
| `Radius.Pill` | 20 | Tags, badges, pills |

---

## 6. Elevation & Shadows

### Shadow Usage

| Token | Effect | Usage |
|-------|--------|-------|
| `Shadow.Window` | Large, diffuse | Main window chrome |
| `Shadow.Popup` | Medium, focused | Dropdowns, menus |
| `Shadow.Card` | Medium, soft | Cards, panels |
| `Shadow.Small` | Small, tight | Buttons, small elements |
| `Shadow.Medium` | Medium | Hover states |
| `Shadow.Large` | Large | Elevated cards |

### Glow Usage

| Token | Effect | Usage |
|-------|--------|-------|
| `Glow.Focus` | Soft accent | Focus rings |
| `Glow.ButtonHover` | Subtle accent | Button hover states |
| `Glow.Primary` | Medium accent | Primary actions |
| `Glow.Selection` | Tight accent | Selection indicators |
| `Glow.ThemeCard.Selected` | Strong accent | Selected theme cards |

### Rules

- No inline `DropShadowEffect` definitions
- No nested shadows (one effect per element)
- Safe mode: use `Shadow.*.Safe` variants

---

## 7. Animation Standards

### Duration Limits

| Context | Max Duration |
|---------|-------------|
| Hover | 150ms |
| Press | 50ms |
| Release | 80ms |
| Theme switch | 200ms |
| Window open | 200ms |
| Window close | 150ms |

### Allowed Easing

- `CubicEase EaseOut` — general UI transitions
- `QuadraticEase EaseOut` — press states

### Prohibited

- Bouncy/spring animations
- Layout-animating transitions on scrollable containers
- Animations on DataGrid rows
- Blur effects during animation

### Safe Mode

- Skip all non-essential animations
- Use instant state changes instead

---

## 8. Control Standards

### All Controls MUST:

- Use semantic tokens for colors
- Use centralized shadows/glows
- Support keyboard navigation (Tab, Enter, Space, Escape)
- Support reduced motion preference
- Use `SnapsToDevicePixels="True"`
- Use `UseLayoutRounding="True"`

### SnapsToDevicePixels

Mandatory on:
- Root elements of control templates
- Borders
- Lines and dividers
- Text blocks in dense layouts

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [TOKEN_RULES.md](TOKEN_RULES.md) | Token naming conventions |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |
| [ACCESSIBILITY_RULES.md](ACCESSIBILITY_RULES.md) | Accessibility rules |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Performance and rendering budget |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
