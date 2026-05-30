# Contracts: Brand Token Interface

**Phase 1 — Design & Contracts**
**Date**: 2026-05-29

## Overview

This feature defines two contracts for external consumption:
1. **Brand Token Naming Convention** — How theme tokens are named and organized
2. **Asset Directory Contract** — The expected structure of Assets/Branding/ that external designers and tools must follow

## 1. Brand Token Naming Convention

All new brand tokens follow this naming pattern for theme dictionaries:

### Primitive Tokens (Colors.xaml)

```text
Primitive.{Category}.{Variant}
```

Categories: Brand, Surface, Text, Accent, Border, Effect
Variants: Blue, Cyan, Orange, Dark, Light, Primary, Secondary

| Token Key | Dark Value | Light Value |
|-----------|-----------|-------------|
| Primitive.Brand.Background | #0E1720 | #F5F7FA |
| Primitive.Brand.Surface | #13202B | #FFFFFF |
| Primitive.Brand.Border | #243647 | #D6E2EE |
| Primitive.Brand.AccentBlue | #2D9CFF | #2D9CFF |
| Primitive.Brand.AccentCyan | #00D1FF | #00B8E6 |
| Primitive.Brand.AccentOrange | #FF8A3D | #FF8A3D |
| Primitive.Brand.TextPrimary | #FFFFFF | #102030 |
| Primitive.Brand.TextSecondary | #B7C5D3 | #5B7186 |
| Primitive.Brand.BlueprintGlow | #00D1FF | #2D9CFF |
| Primitive.Brand.GlassBorder | #2A3F52 | #C8D8E8 |

### Semantic Tokens (DarkColors.xaml / LightColors.xaml)

```text
Brush.{Meaning}
```

| Semantic Key | Maps To (Dark) | Maps To (Light) |
|-------------|----------------|-----------------|
| Brush.Window.Background | Primitive.Brand.Background | Primitive.Brand.Background |
| Brush.Surface.Default | Primitive.Brand.Surface | Primitive.Brand.Surface |
| Brush.Border.Default | Primitive.Brand.Border | Primitive.Brand.Border |
| Brush.Accent.Primary | Primitive.Brand.AccentBlue | Primitive.Brand.AccentBlue |
| Brush.Accent.Cyan | Primitive.Brand.AccentCyan | Primitive.Brand.AccentCyan |
| Brush.Accent.Orange | Primitive.Brand.AccentOrange | Primitive.Brand.AccentOrange |
| Brush.Text.Primary | Primitive.Brand.TextPrimary | Primitive.Brand.TextPrimary |
| Brush.Text.Secondary | Primitive.Brand.TextSecondary | Primitive.Brand.TextSecondary |
| Brush.Glow.Default | Primitive.Brand.BlueprintGlow | Primitive.Brand.BlueprintGlow |
| Brush.Effect.GlassBorder | Primitive.Brand.GlassBorder | Primitive.Brand.GlassBorder |

## 2. Asset Directory Contract

The Assets/Branding/ directory must follow this structure for all tools and consumers:

```text
Assets/Branding/
├── Master/
│   └── planova-master-brand-reference.png
├── Logos/
│   ├── SVG/           # Vector source files
│   ├── PNG/           # Raster sizes 64,128,256,512,1024
│   ├── ICO/           # Windows icon files
│   ├── Transparent/   # Logo on transparent background
│   ├── Dark/          # Logo optimized for dark backgrounds
│   ├── Light/         # Logo optimized for light backgrounds
│   └── Monochrome/    # Single-color versions
├── Ribbon/            # Ribbon toolbar icons (24x24, 32x32)
├── Splash/            # Splash screen background assets
├── Wallpapers/        # Desktop wallpaper backgrounds
├── Icons/             # Sidebar, toolbar, navigation icons
├── Theme/             # Theme-specific decorative assets (blueprint overlays)
└── Fonts/             # Bundled font files (TTF/WOFF)
```
