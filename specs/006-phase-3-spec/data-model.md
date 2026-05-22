# Data Model: Theme Engine 2.0

**Phase**: 1 — Design & Contracts

## Entities

### Theme Preference

Represents the user's selected visual theme.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| SelectedTheme | String | The active theme name | One of: "Dark", "Light", "Custom" |
| AccentColor | String | The selected accent color hex value | Must be a valid 6-digit hex color (e.g., "3A86FF") |

**Persistence**: `Properties.Settings.Default` (ApplicationSettingsBase) — user-scoped settings.
**Lifecycle**: Loaded on application startup. Saved on every theme or accent change.
**Default values**: `SelectedTheme = "Dark"`, `AccentColor = "3A86FF"`.

**State transitions**:
- User opens settings → Current preference displayed
- User clicks theme card → Preference updated, all windows synchronize
- User clicks accent swatch → AccentColor updated, accent-dependent elements update
- Application restarts → Preference restored from storage

---

### Theme Resource Set

Represents a complete collection of visual definitions for a theme.

| Field | Type | Description |
|-------|------|-------------|
| Name | String | Theme identifier: "Dark", "Light", "Custom" |
| SourceFile | String | Path to the merged ResourceDictionary (e.g., `Theme/Dark/DarkTheme.xaml`) |
| IsBuiltIn | Boolean | True for Dark/Light/Custom; future user-created themes would be False |

**Cardinality**: Exactly 3 built-in sets (Dark, Light, Custom). The Custom set incorporates the user's `AccentColor` into its token overrides at runtime.

---

### Render Mode

Represents the runtime rendering configuration based on hosting environment detection.

| Field | Type | Description |
|-------|------|-------------|
| Mode | Enum | `WindowChrome` (full effects, animations) or `FallbackSafe` (reduced effects, no animations) |
| IsAutoDetected | Boolean | True if mode was determined by WindowRenderModeDetector |
| DetectionTimestamp | DateTime | When the mode was last evaluated |

**Lifecycle**: Determined once at application startup. Re-evaluation is triggered only on rendering failure events.

---

### Accent Color Swatch

Represents one of the 8 predefined color presets for the Custom theme.

| Field | Type | Description |
|-------|------|-------------|
| Name | String | Display name (e.g., "Blue", "Green", "Orange", "Red") |
| HexValue | String | 6-digit hex color (e.g., "3A86FF", "2ED573") |
| DisplayOrder | Integer | Position in the swatch grid (1-8) |
| Variants | Object | Predefined hover, pressed, glow, border, subtle variants as hex values |

**Cardinality**: Exactly 8 swatches, hardcoded in `Theme/Custom/CustomColors.xaml`.

---

## Relationships

```text
Theme Preference (1) ────── uses ────── Theme Resource Set (1..*)
     │                                             │
     │ references                                   │ contains
     ▼                                             ▼
Accent Color Swatch (1..8)                  Accent Color (1)
     │                                             │
     └────────── defines variants ────────────────┘
                         │
                         ▼
               Render Mode (1)
                    │
                    ├── WindowChrome → full effects + animations
                    └── FallbackSafe → safe shadows + no animations
```

## Validation Rules

| Rule | Description | Enforced At |
|------|-------------|-------------|
| Theme name must be valid | Only "Dark", "Light", or "Custom" accepted | ThemeManager.ApplyTheme() |
| Accent hex must be valid | Must match `^[0-9A-Fa-f]{6}$` | ThemeManager.ApplyAccentColor() |
| No duplicate resource keys | Each x:Key must be unique across all loaded dictionaries | Build-time XAML parsing |
| Resource loading order | Dependencies must load before consumers | ThemeResources.xaml merge order |
| Theme must load successfully | Failure preserves current theme | ThemeManager safe fallback |
