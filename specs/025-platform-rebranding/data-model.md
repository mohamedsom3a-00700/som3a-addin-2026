# Data Model: Full Platform Rebranding & Visual Identity System

**Phase 1 — Design & Contracts**
**Date**: 2026-05-29

## Entities

### BrandAsset

Represents a logo or visual brand file in the Assets/Branding/ hierarchy.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Unique asset identifier (e.g., "logo-primary", "logo-symbol") |
| category | enum | Logos, Ribbon, Splash, Wallpapers, Icons, Theme, Fonts |
| formats | string[] | SVG, PNG, ICO |
| variants | string[] | transparent, dark, light, monochrome |
| sizes | int[] | [64, 128, 256, 512, 1024] (PNG only) |
| sourcePath | string | Relative path from Assets/Branding/ to source file |

### BrandThemeToken

Named color value extracted from the master branding, used as global theme tokens.

| Field | Type | Description |
|-------|------|-------------|
| name | string | Token name (e.g., "PrimaryBlue", "PrimaryCyan") |
| darkValue | Color | Hex color for dark theme |
| lightValue | Color | Hex color for light theme |
| tokenType | enum | Primitive, Semantic |
| wcagPasses | bool | Whether this token passes WCAG 2.1 AA |

### TypographyPreset

Named font configuration selectable from appearance settings.

| Field | Type | Description |
|-------|------|-------------|
| name | string | Display name (e.g., "Inter", "Cairo") |
| fontFamily | string | Font family name for WPF |
| language | enum | English, Arabic |
| source | enum | SystemInstalled, Bundled |
| fallbackChain | string[] | Ordered fallback families |
| rtlSupported | bool | Whether font supports RTL shaping |
| fontFiles | string[] | Relative paths to bundled font files (if applicable) |

### MasterBrandReference

The authoritative visual reference file.

| Field | Type | Description |
|-------|------|-------------|
| filePath | string | Path to reference file |
| format | string | PNG |
| validationRule | string | All UI assets must visually align with this reference |

## Relationships

- A **BrandThemeToken** is consumed by theme dictionaries (Colors.xaml, DarkColors.xaml, LightColors.xaml)
- A **TypographyPreset** is applied via ThemeManager to the shell and all child windows
- **BrandAsset** files are placed in Assets/Branding/ by an external designer; this phase creates the folder structure and consumes the files at runtime
- The **MasterBrandReference** serves as the validation source for SC-004 (ribbon icons) and SC-002 (color accuracy)

## Validation Rules

| Rule | Entity | Constraint |
|------|--------|------------|
| V-001 | BrandThemeToken | All text/background combinations must pass WCAG 2.1 AA (4.5:1 normal, 3:1 large) |
| V-002 | TypographyPreset | Must define at least one fallback family in fallbackChain |
| V-003 | BrandAsset | Primary logo must exist in SVG, PNG (all 5 sizes), and ICO formats |
| V-004 | BrandAsset | All required subdirectories under Assets/Branding/ must exist and contain at least one file |

## State Transitions

N/A — Branding entities are static configuration that do not undergo runtime state changes.
