# Data Model: Theme Expansion

**Feature**: 017-theme-expansion
**Date**: 2026-05-26

## Entity: BackgroundSettings *(new)*

Encapsulates all background-related configuration for the Custom theme. Persisted in `Properties.Settings.Default` as individual keys.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| BackgroundType | `enum BackdropStyle` | Yes | `Solid`, `Gradient`, `Image`, or `Blur` (shortcut for Image+Blur) |
| ImagePath | `string?` | No | Absolute filesystem path to the background image file |
| ImageFileName | `string?` | No (derived) | Extracted filename from ImagePath for display purposes |
| BlurIntensity | `double` | Yes | 0.0 (sharp) to 1.0 (maximum blur), default 0.0 |
| BlurEnabled | `bool` | Yes | Whether DWM blur is active; independent of Intensity so blur can be toggled without losing intensity value |
| SolidColor | `Color?` | No | Fallback solid background color when image is unavailable |
| GradientStart | `Color?` | No | Top-left gradient color (used in `Gradient` mode) |
| GradientEnd | `Color?` | No | Bottom-right gradient color (used in `Gradient` mode) |

**Validation rules**:
- `ImagePath`, if set, must point to an existing file with extension `.png`, `.jpg`, `.jpeg`, or `.bmp`
- File size must be ≤ 10MB; max dimension (width or height) ≤ 4096px
- `BlurIntensity` clamped to [0.0, 1.0]
- If `BackgroundType` is `Image` and `ImagePath` is null/empty, fall back to `SolidColor` (or default token)
- If `BackgroundType` is `Blur` and DWM blur is unsupported (safe mode, OS too old), render without blur

**State transitions**:
- `Image` selected → file validated → `ImagePath` set, `ImageFileName` derived → backdrop applied
- Image file deleted/moved externally → next theme load detects missing file → `BackgroundType` reset to `Solid` → user notified
- Image exceeds size/dimension limits → rejected at file picker → previous background preserved
- Corrupt image selected → file picker accepts but load fails → error displayed → background falls back to `SolidColor`

**Relationships**:
- One `ThemePreset` (Custom) holds one `BackgroundSettings` (1:1)
- `BackgroundSettings` is serialized to individual `Properties.Settings.Default` keys, not a single blob

**Persistence mapping**:
| Settings Key | Type | Default |
|---|---|---|
| `WindowBackdropStyle` | `string` | `"Solid"` |
| `BackgroundImagePath` | `string` | `""` |
| `BackgroundBlurIntensity` | `double` | `0.0` |
| `BackgroundBlurEnabled` | `bool` | `false` |

---

## Entity: FontSettings *(new)*

Stores the user's font family selection and Arabic font readiness data. Persisted in `Properties.Settings.Default`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| FontFamilyName | `string` | Yes | Font family name as reported by `Fonts.SystemFontFamilies` (e.g., "Segoe UI", "Cairo") |
| IsArabicCompatible | `bool` | Yes (derived) | Whether the font contains Arabic Unicode block glyphs (U+0600–U+06FF) |
| PreviewThumbnail | `byte[]?` | No | Cached preview image bytes (generated on demand, not persisted) |
| FallbackFamilyName | `string` | Yes | Fallback font if selected font is unavailable; always "Segoe UI" |

**Validation rules**:
- `FontFamilyName` must match a font family name from `Fonts.SystemFontFamilies`
- If the selected font is not found at load time, fall back to `FallbackFamilyName` ("Segoe UI") and notify user
- `PreviewThumbnail` regenerated each time font selection page opens; not persisted to disk

**State transitions**:
- Font selected → `FontFamilyName` set → `ThemeManager.SetResource("CustomFontFamily", new FontFamily(fontName))` → WPF propagates to all windows
- Previously selected font removed from system → next load fallback to "Segoe UI" → user notified → font reverts to default
- Arabic-compatible font selected with Arabic localization active → Arabic text renders correctly

**Relationships**:
- One `ThemePreset` (Custom) holds one `FontSettings` (1:1)

**Persistence mapping**:
| Settings Key | Type | Default |
|---|---|---|
| `SelectedFontFamily` | `string` | `"Segoe UI"` |

---

## Entity: AccentVariant *(extended)*

A computed color derived from the primary accent color. Five variants are generated automatically using HSL transformations. Stored as named resources in `Application.Current.Resources`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| VariantType | `enum AccentVariantType` | Yes | `Hover`, `Pressed`, `Glow`, `Border`, `Subtle` |
| Color | `Color` | Yes (computed) | The derived color value |
| Brush | `SolidColorBrush` | Yes (computed) | Frozen SolidColorBrush for resource use |
| DeltaE | `double` | Yes (computed) | Perceptual distance (CIE76) from base accent; must be ≥ 5 |

**HSL transformation rules**:

| Variant | Hue | Saturation | Lightness |
|---------|-----|------------|-----------|
| Hover | Unchanged | Unchanged | +10% (clamped to 100%) |
| Pressed | Unchanged | Unchanged | -10% (clamped to 0%) |
| Glow | Unchanged | +15% | +5% |
| Border | Unchanged | -10% | +20% |
| Subtle | Unchanged | -30% | +30% |

**Validation rules**:
- Each variant's perceptual distance (ΔE CIE76) from base accent must be ≥ 5
- If ΔE < 5, apply a minimum lightness adjustment of ±5% until distance threshold is met
- All variant brushes must be frozen (WPF performance best practice)

**Resource naming convention**:
```
Accent.Color.Hover      → Accent.Brush.Hover
Accent.Color.Pressed    → Accent.Brush.Pressed
Accent.Color.Glow       → Accent.Brush.Glow
Accent.Color.Border     → Accent.Brush.Border
Accent.Color.Subtle     → Accent.Brush.Subtle
```

**Relationships**:
- One base accent color generates five `AccentVariant` instances (1:5)
- Variants are regenerated whenever the base accent color changes via `ThemeManager.ApplyAccentColor()`

---

## Entity: MaterialIconReference *(new)*

Maps a logical icon identifier to the Material Design PackIcon `PackIconKind` enum. Used by plugins and shell navigation to reference icons without hardcoding Material enum values.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| LogicalName | `string` | Yes | Application-level icon name (e.g., "home", "settings", "wbs") |
| PackIconKind | `PackIconKind` | Yes | Corresponding Material Design icon enum value |
| Category | `string` | Yes | Icon category for organization (e.g., "Navigation", "Action", "File") |
| FallbackGlyph | `string?` | No | Text glyph to display if PackIcon rendering fails |

**Icon registry** (subset — full list defined during implementation):

| LogicalName | PackIconKind | Category |
|---|---|---|
| home | Home | Navigation |
| settings | Cog | Navigation |
| wbs | FamilyTree | Planning |
| boq | FileDocumentEdit | Planning |
| activities | FormatListBulleted | Planning |
| relationships | SourceBranch | Planning |
| export | FileExport | File |
| ai | Robot | AI |
| diagnostics | ChartBar | Diagnostics |
| accessibility | Human | Settings |

**Validation rules**:
- `LogicalName` must be unique across the registry
- If `PackIconKind` maps to an unrecognized enum value, log warning and use `FallbackGlyph`
- Category names align with sidebar categories from Phase 15 (Shell Refactor)

**Relationships**:
- One `MaterialIconReference` is referenced by one or more UI elements (1:N)
- Plugins reference icons by `LogicalName`, not by Material enum directly

---

## Entity: ThemePreset *(modified)*

Existing entity representing a named theme configuration. Extended with new fields for background and font.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Name | `string` | Yes | "Dark", "Light", or "Custom" |
| ColorTokens | `Dictionary<string, Color>` | Yes | Semantic color token overrides |
| AccentColor | `Color` | Yes | Base accent color |
| AccentVariants | `List<AccentVariant>` | Yes (Custom only) | Generated accent variant colors |
| BackgroundSettings | `BackgroundSettings?` | No (Custom only) | Background configuration (null for Dark/Light) |
| FontSettings | `FontSettings?` | No (Custom only) | Font selection (null for Dark/Light; uses default) |
| AccentSwatches | `List<Color>` | Yes (Custom only) | 8 preset swatch colors for quick selection |

**State transitions**:
- Dark/Light themes: `BackgroundSettings` and `FontSettings` are null; use theme-default solid backdrop and Segoe UI
- Custom theme: both settings are active and user-editable
- Switching Dark/Light → Custom: previously saved Custom settings are restored from persistence
- Switching Custom → Dark/Light: Custom settings are preserved in memory (not lost)

---

## Settings Persistence Schema

All new and modified settings stored in `Properties/Settings.settings` (ApplicationSettingsBase):

| Settings Key | Type | Scope | Default | Phase |
|---|---|---|---|---|
| `SelectedTheme` | `string` | User | `"Dark"` | Existing |
| `AccentColor` | `string` | User | `"#3A86FF"` | Existing |
| `WindowBackdropStyle` | `string` | User | `"Solid"` | New |
| `BackgroundImagePath` | `string` | User | `""` | New |
| `BackgroundBlurIntensity` | `double` | User | `0.0` | New |
| `BackgroundBlurEnabled` | `bool` | User | `false` | New |
| `SelectedFontFamily` | `string` | User | `"Segoe UI"` | New |
