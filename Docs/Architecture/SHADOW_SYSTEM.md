# Shadow System Architecture

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Files**: `Theme/Effects/Shadows.xaml`, `Theme/Effects/Glow.xaml`

---

## 1. Centralized Effects Mandate

ALL visual effects MUST originate from centralized dictionaries:

- `Theme/Effects/Shadows.xaml` — DropShadowEffect definitions
- `Theme/Effects/Glow.xaml` — Glow/highlight effect definitions
- `Theme/Effects/Animations.xaml` — Shared storyboards and easing

**Inline effects are PROHIBITED.**

---

## 2. Shadow Definitions

### Standard Shadows

| Key | Blur | Depth | Direction | Color | Opacity | Usage |
|-----|------|-------|-----------|-------|---------|-------|
| `Shadow.Window` | 30 | 8 | 270 | #000000 | 0.4 | Main window chrome |
| `Shadow.Popup` | 15 | 3 | 270 | #000000 | 0.3 | Dropdowns, menus |
| `Shadow.Popup.Small` | 12 | 2 | 270 | #000000 | 0.25 | Small tooltips |
| `Shadow.Card` | 16 | 4 | 270 | #000000 | 0.25 | Cards, panels |
| `Shadow.Small` | 8 | 2 | 270 | #000000 | 0.2 | Small elements |
| `Shadow.Medium` | 12 | 3 | 270 | #000000 | 0.25 | Medium elements |
| `Shadow.Large` | 20 | 5 | 270 | #000000 | 0.3 | Elevated cards |

### Safe-Mode Shadows

For `FallbackSafe` rendering mode (Excel host, GPU issues):

| Key | Blur | Depth | Opacity | Usage |
|-----|------|-------|---------|-------|
| `Shadow.Window.Safe` | 16 | 4 | 0.25 | Reduced window shadow |
| `Shadow.Popup.Safe` | 8 | 2 | 0.2 | Reduced popup shadow |
| `Shadow.Card.Safe` | 10 | 3 | 0.2 | Reduced card shadow |

**Safe mode switches automatically via `RenderModeService`.**

---

## 3. Glow Definitions

### Accent Glows

All glow effects use `Color="{DynamicResource AccentColorValue}"` for dynamic accent coloring.

| Key | Blur | Depth | Opacity | Usage |
|-----|------|-------|---------|-------|
| `Glow.Focus` | 10 | 0 | 0.35 | Focus rings |
| `Glow.ButtonHover` | 8 | 0 | 0.3 | Button hover |
| `Glow.Primary` | 12 | 0 | 0.4 | Primary actions |
| `Glow.Selection` | 6 | 0 | 0.5 | Selection indicators |
| `Glow.Accent` | 8 | 0 | 0.4 | General accent glow |
| `Glow.ThemeCard.Selected` | 14 | 0 | 0.45 | Selected theme cards |

---

## 4. Elevation Token Mapping

Elevation tokens link semantic elevation levels to shadow effects:

```xaml
<!-- Theme/Base/Elevation.xaml -->
<StaticResource x:Key="Elevation.Card" ResourceKey="Shadow.Card"/>
<StaticResource x:Key="Elevation.Popup" ResourceKey="Shadow.Popup"/>
<StaticResource x:Key="Elevation.Window" ResourceKey="Shadow.Window"/>
<StaticResource x:Key="Elevation.Small" ResourceKey="Shadow.Small"/>
<StaticResource x:Key="Elevation.Medium" ResourceKey="Shadow.Medium"/>
<StaticResource x:Key="Elevation.Large" ResourceKey="Shadow.Large"/>
```

Controls consume elevation tokens, not shadow keys directly:

```xml
<!-- CORRECT -->
<Setter Property="Effect" Value="{DynamicResource Elevation.Card}"/>

<!-- INCORRECT -->
<Setter Property="Effect" Value="{DynamicResource Shadow.Card}"/>
```

---

## 5. Rules

### DO:

- Reference centralized shadow/glow keys
- Use `Elevation.*` tokens for semantic elevation
- Use `Glow.*` for accent-driven effects
- Test effects in both WindowChrome and FallbackSafe modes

### DO NOT:

- Define `DropShadowEffect` inline in any control template
- Nest multiple shadows on a single element
- Use `BlurEffect` as a substitute for shadow
- Hardcode shadow colors (always use `#000000` with opacity)

---

## 6. Validation Checklist

- [ ] All shadows centralized in `Effects/Shadows.xaml`
- [ ] All glows centralized in `Effects/Glow.xaml`
- [ ] Safe-mode variants exist for all major shadows
- [ ] Elevation tokens map correctly to shadow keys
- [ ] No inline effects in any control template
- [ ] No nested shadows

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Performance and rendering budget |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
