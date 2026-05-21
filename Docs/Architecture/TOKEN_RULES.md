# Token Naming Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Governed by**: Constitution v1.2.0, Principle V

---

## Token Layers

The design system implements **three hierarchical token layers** (Primitive, Semantic, Component) and a separate **Utility token category** for cross-cutting concerns.

```text
Primitive Tokens
    ↓ (referenced by)
Semantic Tokens
    ↓ (referenced by)
Component Tokens
    ↓ (used by)
Control Templates
```

---

## 1. Primitive Tokens

### Purpose

Contain raw immutable palette values. No UI semantics.

### Naming Convention

```text
Primitive.<ColorFamily>.<Shade>
```

### Examples

```xaml
<Color x:Key="Primitive.Blue.500">#3A86FF</Color>
<Color x:Key="Primitive.Slate.900">#0E1720</Color>
<Color x:Key="Primitive.Slate.800">#1C2B3A</Color>
<Color x:Key="Primitive.Slate.700">#15202B</Color>
<Color x:Key="Primitive.White.95">#F2FFFFFF</Color>
<Color x:Key="Primitive.White.75">#BFFFFFFF</Color>
<Color x:Key="Primitive.White.40">#66FFFFFF</Color>
<Color x:Key="Primitive.White.20">#33FFFFFF</Color>
<Color x:Key="Primitive.White.10">#1AFFFFFF</Color>
<Color x:Key="Primitive.White.5">#0DFFFFFF</Color>
<Color x:Key="Primitive.Green.500">#2ED573</Color>
<Color x:Key="Primitive.Orange.500">#FFA502</Color>
<Color x:Key="Primitive.Red.500">#FF4757</Color>
<Color x:Key="Primitive.Blue.600">#1E90FF</Color>
```

### Rules

- **Immutable** during application execution
- Defined as `Color` (not `SolidColorBrush`) to enable brush variant generation
- Referenced by Semantic Tokens using `{StaticResource}`
- **NEVER** referenced directly from control templates

---

## 2. Semantic Tokens

### Purpose

Contain meaning-based aliases that reference primitive values. Convey UI intent.

### Naming Convention

```text
Brush.<Category>.<State>
```

### Categories

| Category | Examples |
|----------|----------|
| `Background` | `Brush.Background.Primary`, `Brush.Background.Secondary`, `Brush.Background.Card` |
| `Text` | `Brush.Text.Primary`, `Brush.Text.Secondary`, `Brush.Text.Disabled`, `Brush.Text.OnAccent` |
| `Accent` | `Brush.Accent.Primary`, `Brush.Accent.Success`, `Brush.Accent.Warning`, `Brush.Accent.Danger`, `Brush.Accent.Info` |
| `Stroke` | `Brush.Stroke.Card`, `Brush.Stroke.Info`, `Brush.Stroke.Status` |
| `Fill` | `Brush.Fill.Info`, `Brush.Fill.Status` |
| `Control` | `Brush.Control.Background`, `Brush.Control.Stroke`, `Brush.Control.HoverBackground`, `Brush.Control.SelectedBackground` |

### Rules

- Defined as `SolidColorBrush`, `LinearGradientBrush`, or `RadialGradientBrush`
- Reference Primitive Tokens using `{StaticResource Primitive.*}`
- Consumed by Component Tokens and Control Templates using `{DynamicResource}`
- MAY be overridden at runtime by theme dictionaries (Dark/Light/Custom)

---

## 3. Component Tokens

### Purpose

Isolate component-specific styling values from direct semantic token usage. Enable per-component theming without affecting global semantics.

### Naming Convention

```text
Component.<Control>.<Part>.<State>
```

### Examples

```xaml
<Thickness x:Key="Component.Button.Padding" Value="{DynamicResource ButtonPadding}"/>
<CornerRadius x:Key="Component.Button.Radius" Value="{DynamicResource MediumRadius}"/>
<sys:Double x:Key="Component.ComboBox.CornerRadius" Value="{DynamicResource MediumRadius}"/>
<Thickness x:Key="Component.Card.Padding" Value="{DynamicResource CardPadding}"/>
<CornerRadius x:Key="Component.Card.Radius" Value="{DynamicResource CardRadius}"/>
```

### Rules

- Reference Semantic and Spacing tokens using `{DynamicResource}`
- Used exclusively by Control Templates
- May vary per theme or component variant

---

## 4. Utility Tokens

### Spacing

```text
Spacing.<Size>
```

Examples: `Spacing.XSmall`, `Spacing.Small`, `Spacing.Medium`, `Spacing.Large`, `Spacing.XLarge`, `Spacing.XXLarge`

### Padding

```text
Padding.<Size>
```

Examples: `Padding.XSmall`, `Padding.Small`, `Padding.Medium`, `Padding.Large`, `Padding.XLarge`

### Radius

```text
Radius.<Size>
```

Examples: `Radius.Small`, `Radius.Medium`, `Radius.Large`, `Radius.Card`, `Radius.Window`, `Radius.Pill`

### Elevation

```text
Elevation.<Level>
```

Examples: `Elevation.Card`, `Elevation.Popup`, `Elevation.Window`, `Elevation.Small`, `Elevation.Medium`, `Elevation.Large`

- Maps to shadow effect keys from `Effects/Shadows.xaml`
- Enables safe-mode variant switching

### Motion

```text
Motion.<Type>.<Property>
```

Examples:
- `Motion.Fade.Duration`
- `Motion.Hover.Duration`
- `Motion.Press.Duration`
- `Motion.ThemeSwitch.Duration`
- `Motion.Easing.Default`

### Z-Index

```text
ZIndex.<Layer>
```

Examples: `ZIndex.Popup`, `ZIndex.Tooltip`, `ZIndex.Overlay`, `ZIndex.Dialog`

### Opacity

```text
Opacity.<State>
```

Examples: `Opacity.Disabled`, `Opacity.Hover`, `Opacity.Pressed`, `Opacity.Subtle`

---

## 5. Legacy Flat Keys

Existing flat keys are **preserved for backward compatibility** but new work MUST use semantic naming.

| Legacy Key | Semantic Equivalent | Status |
|------------|---------------------|--------|
| `AccentBrush` | `Brush.Accent.Primary` | Deprecated |
| `BackgroundBrush` | `Brush.Background.Primary` | Deprecated |
| `CardBrush` | `Brush.Background.Card` | Deprecated |
| `SurfaceBrush` | `Brush.Background.Secondary` | Deprecated |
| `TextMainBrush` | `Brush.Text.Primary` | Deprecated |
| `TextSubBrush` | `Brush.Text.Secondary` | Deprecated |
| `TextDisabledBrush` | `Brush.Text.Disabled` | Deprecated |
| `CardStrokeBrush` | `Brush.Stroke.Card` | Deprecated |
| `ControlBgBrush` | `Brush.Control.Background` | Deprecated |
| `ControlStrokeBrush` | `Brush.Control.Stroke` | Deprecated |

---

## 6. Token Validation Checklist

Before adding a new token:

- [ ] Does a semantically equivalent token already exist?
- [ ] Is the name descriptive and category-appropriate?
- [ ] Does it follow the naming convention for its layer?
- [ ] Is it placed in the correct file (Colors.xaml, Spacing.xaml, etc.)?
- [ ] Is it referenced correctly (StaticResource for primitives, DynamicResource for semantic/component)?
- [ ] Is the legacy flat key created for backward compatibility (semantic tokens only)?

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Rendering and animation budget |
| [UI_GUIDELINES.md](UI_GUIDELINES.md) | Windows 11 Fluent design rules |
| [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) | Branch protection and merge rules |
| [BRANCH_NAMING.md](BRANCH_NAMING.md) | Branch naming conventions |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
