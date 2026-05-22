# Data Model: Design System Core

**Branch**: `004-design-system-core` | **Date**: 2026-05-22

## Entity: PrimitiveToken

**Defined in**: `WpfApp2/Theme/Base/Colors.xaml`
**Type**: `Color` (XAML resource)
**Mutability**: Immutable at runtime (Constitution V)

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| Key | `x:Key` string | `Primitive.Blue.500` | Namespaced: `Primitive.<Color>.<Shade>` |
| Value | Hex ARGB | `#3A86FF` | Raw color value, never changes at runtime |

### Existing Primitive Tokens (15)

| Key | Value | Category |
|-----|-------|----------|
| `Primitive.Blue.500` | `#3A86FF` | Accent |
| `Primitive.Blue.600` | `#1E90FF` | Accent |
| `Primitive.Slate.900` | `#0E1720` | Background |
| `Primitive.Slate.800` | `#1C2B3A` | Background |
| `Primitive.Slate.700` | `#15202B` | Surface |
| `Primitive.White.95` | `#F2FFFFFF` | Text |
| `Primitive.White.75` | `#BFFFFFFF` | Text |
| `Primitive.White.40` | `#66FFFFFF` | Text/Disabled |
| `Primitive.White.20` | `#33FFFFFF` | Stroke/Border |
| `Primitive.White.10` | `#1AFFFFFF` | Subtle fill |
| `Primitive.White.5` | `#0DFFFFFF` | Subtle fill |
| `Primitive.Green.500` | `#2ED573` | Success |
| `Primitive.Orange.500` | `#FFA502` | Warning |
| `Primitive.Red.500` | `#FF4757` | Danger |
| `Primitive.Black.13` | `#22000000` | Progress background |

### New Primitive Tokens to Add

| Key | Value | Category | Rationale |
|-----|-------|----------|-----------|
| `Primitive.Transparency.Subtle` | `#0DFFFFFF` | Transparency | Documentation for 5% alpha on white |
| `Primitive.Transparency.Light` | `#1AFFFFFF` | Transparency | Documentation for 10% alpha on white |
| `Primitive.Transparency.MediumLow` | `#1FFFFFFF` | Transparency | Documentation for 12.5% alpha on white |
| `Primitive.Transparency.Medium` | `#2FFFFFFF` | Transparency | Documentation for ~18% alpha on white |
| `Primitive.Transparency.Strong` | `#3FFFFFFF` | Transparency | Documentation for ~25% alpha on white |
| `Primitive.White.33` | `#54FFFFFF` | ScrollBar thumb | For V2 violation (`#55FFFFFF`) |
| `Primitive.Black.8` | `#14000000` | GroupBox background | For V3 violation (`#14000000`) |
| `Primitive.Black.53` | `#88000000` | Overlay background | For V4 violation (`#88000000`) |

### State Machine

None. Primitive tokens are immutable constants.

---

## Entity: SemanticColorToken

**Defined in**: `WpfApp2/Theme/Base/Colors.xaml`
**Type**: `Color` (XAML resource)
**Mutability**: Mutable at runtime via theme overrides (DarkColors, LightColors, CustomColors)

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| Key | `x:Key` string | `Color.Button.Background` | Namespaced: `Color.<Category>.<Property>` |
| Value | Hex ARGB | `#2FFFFFFF` | Resolved from Primitive.* or direct hex (in base) |
| Reference | `StaticResource` | `Primitive.White.20` | Must reference a Primitive token per FR-004 |

### Existing Semantic Color Tokens (25)

See plan.md for full list. Key additions needed:

| Key | Value | Reference | Rationale |
|-----|-------|-----------|-----------|
| `Color.ScrollBar.Thumb` | `#55FFFFFF` | New Primitive | Replaces V2 inline hex |
| `Color.GroupBox.Background` | `#14000000` | New Primitive | Replaces V3 inline hex |
| `Color.Overlay.Background` | `#88000000` | New Primitive | Replaces V4 inline hex |
| `Color.Animation.ThumbFade` | `#66FFFFFF` | `Primitive.White.40` | Replaces V7 inline hex in animation |
| `Color.Animation.ButtonHoverDanger` | `#22FF4757` | `Primitive.Red.500` + alpha | Replaces V6 inline hex |

### State Machine

None. Semantic color tokens are theme-overridable constants.

---

## Entity: SemanticBrushToken

**Defined in**: `WpfApp2/Theme/Base/Colors.xaml`
**Type**: `SolidColorBrush` or `LinearGradientBrush`
**Mutability**: Mutable at runtime via DynamicResource theme overrides

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| Key | `x:Key` string | `Brush.Button.Background` | Namespaced: `Brush.<Category>.<Property>` |
| Color Reference | `{StaticResource Color.*}` or `{StaticResource Primitive.*}` | `Color.Button.Background` | Must reference a Color or Primitive token |

### Validation Rules

- Every `Brush.*` MUST reference either a `Color.*` semantic token or a `Primitive.*` token
- NO `Brush.*` may contain a direct hex `Color="#..."` attribute
- NO `Brush.*` may reference another `Brush.*` token (semantic-to-semantic chain prohibited)
- Derived state brushes (hover, pressed) compose from `Primitive.*` base + `Primitive.Transparency.*` alpha via `Color.*` tokens

### State Machine

None. Brush tokens resolve to color at theme load time.

---

## Entity: ComponentToken

**Defined in**: `WpfApp2/Theme/Base/ComponentTokens.xaml`
**Type**: `Thickness`, `CornerRadius`, `sys:Double`, `sys:Int32`
**Mutability**: Mutable at runtime via DynamicResource

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| Key | `x:Key` string | `Component.Button.Padding` | Namespaced: `Component.<Control>.<Property>` |
| Value | Resolved type | `12,6` (Thickness) | Must reference a Spacing.* or Brush.* token where DynamicResource is supported |

### Existing Component Tokens (17)

See plan.md research for full list.

### Validation Rules

- `Component.*` tokens SHOULD reference `Spacing.*`, `Padding.*`, `Radius.*`, or `Brush.*` tokens
- Where `DynamicResource` is not possible (e.g., `CornerRadius` in attribute syntax), hardcoded defaults with `DynamicResource` setter override are acceptable
- Component tokens isolate control styling from global semantic values

---

## Entity: LegacyFlatKey

**Defined in**: `WpfApp2/Theme/Base/Colors.xaml`
**Type**: `SolidColorBrush`
**Mutability**: Mutable at runtime (same as semantic counterpart)
**Deprecation**: Annotated with XML comment for Phase 6 removal

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| Key | `x:Key` string | `Blue500` | Flat naming (no dot-namespace) |
| Reference | `{StaticResource Primitive.*}` | `Primitive.Blue.500` | Alias to Primitive or Brush token |

### State Machine

```
[Active] ──Phase 6──→ [Removed]
```

- **Active**: Legacy key exists in Colors.xaml with deprecation comment
- **Removed**: Legacy key deleted in Phase 6 after all consumers migrated to `Brush.*` tokens

---

## Entity: TokenValidationResult

**Used by**: Build-time lint script and runtime ThemeManager validation

| Field | Type | Notes |
|-------|------|-------|
| Rule | string | Validation rule identifier (e.g., "no-inline-hex", "no-static-resource-themeable") |
| File | string | XAML file path |
| Line | int | Line number of violation |
| Severity | enum | Error, Warning, Info |
| Message | string | Human-readable description |

### Validation Rules (FR-010)

1. `no-inline-hex`: No `Color="#..."` outside `Base/Colors.xaml`, `Dark/DarkColors.xaml`, `Light/LightColors.xaml`, `Custom/CustomColors.xaml`
2. `no-static-resource-themeable`: No `StaticResource` on themeable DP properties (Background, Foreground, BorderBrush, Fill, Stroke, Effect)
3. `no-inline-drop-shadow`: No `<DropShadowEffect` outside `Effects/Shadows.xaml` and `Effects/Glow.xaml`
4. `no-hardcoded-spacing`: No `Margin="`, `Padding="`, `FontSize="`, `FontWeight="` in Control styles or windows (must reference named tokens)
5. `semantic-references-primitive`: Every `Brush.*` must reference a `Primitive.*` or `Color.*` token, not direct hex
6. `no-semantic-to-semantic`: No `Brush.*` may reference another `Brush.*` token

---

## Entity: ThemeResourcesAggregator

**Defined in**: `WpfApp2/Theme/ThemeResources.xaml`
**Type**: `ResourceDictionary` with `MergedDictionaries`

### Loading Order (FR-012)

```
1.  Base/Colors.xaml           — Primitive + Semantic + Legacy tokens
2.  Base/Typography.xaml        — Font families, sizes, weights, styles
3.  Base/Spacing.xaml           — Spacing scale, padding, sizes
4.  Base/Radius.xaml            — Corner radii
5.  Base/Elevation.xaml         — Elevation abstraction (Double tokens only)
6.  Base/Motion.xaml            — Animation durations and easing
7.  Base/ZIndex.xaml            — Z-index layering
8.  Base/Opacity.xaml           — Opacity states
9.  Base/ComponentTokens.xaml   — Component-specific tokens
10. Effects/Shadows.xaml       — DropShadowEffect definitions
11. Effects/Glow.xaml           — Glow effect definitions
12. Effects/Animations.xaml     — Storyboard animations
13. Controls/*.xaml            — All control styles
14. ModernWindow.xaml           — Window chrome template
15. WindowAnimations.xaml       — Window-level animations
```

Theme overrides (Dark, Light, Custom) are swapped at runtime by ThemeManager.

### Validation Rules

- No dictionary may be loaded before its dependencies
- No duplicate `x:Key` across dictionaries in the same merge level
- Converters are defined in ThemeResources.xaml (not duplicated)