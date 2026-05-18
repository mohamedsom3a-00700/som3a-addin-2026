# Data Model: WPF Fluent UI Theme Engine

**Feature**: WPF Fluent UI Migration — Theme Engine & Runtime Switching
**Date**: 2026-05-18

## Entities

### Theme

Represents a complete visual configuration applied to the application.

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Theme identifier: "Dark", "Light", or "Custom" |
| IsCustom | bool | True if this is the Custom theme with user-selectable accent |
| AccentColor | string | Hex color code for the accent (e.g., "#3A86FF"). Only meaningful when IsCustom=true |

**Relationships**: Each Theme references a set of ColorTokens via semantic token aliases in the merged ResourceDictionary. The ThemeManager loads one Theme at a time into the application resources.

**State transitions**: Theme has no runtime state transitions. It is loaded or unloaded by ThemeManager.

---

### ColorToken

Represents a named color value used in UI templates. Two classifications exist within the token system.

| Field | Type | Description |
|-------|------|-------------|
| Key | string | ResourceDictionary key (e.g., "Blue500", "Brush.Background.Primary") |
| Type | enum | Primitive or Semantic |
| ColorValue | Color | The actual color value (only present in Primitive tokens; Semantic tokens alias a Primitive) |
| Aliases | list<string> | For Semantic tokens, the Primitive token they reference |

**Primitive tokens** (raw palette, no UI semantics):
- `Blue500`: #3A86FF
- `Slate900`: #0E1720
- `Slate800`: #1C2B3A
- `Slate700`: #15202B
- `WhiteAlpha95`: #F2FFFFFF
- `WhiteAlpha75`: #BFFFFFFF
- `WhiteAlpha40`: #66FFFFFF
- `WhiteAlpha20`: #33FFFFFF
- `WhiteAlpha10`: #1AFFFFFF
- `SuccessGreen`: #2ED573
- `WarningOrange`: #FFA502
- `DangerRed`: #FF4757
- `InfoBlue`: #1E90FF

**Semantic tokens** (UI-meaning aliases):
- `Brush.Background.Primary` → aliases Primitive for theme background
- `Brush.Background.Secondary` → aliases Primitive for surface/panel background
- `Brush.Background.Card` → aliases Primitive for card background
- `Brush.Text.Primary` → aliases Primitive for primary text
- `Brush.Text.Secondary` → aliases Primitive for secondary/subtitle text
- `Brush.Text.Disabled` → aliases Primitive for disabled text
- `Brush.Accent.Primary` → aliases Primitive for accent color (swappable in Custom theme)
- `Brush.Accent.Success/Warning/Danger/Info` → aliases Primitive for status colors
- `Brush.Stroke.Card` → aliases Primitive for card borders
- `Brush.Control.Background` → aliases Primitive for input control backgrounds
- `Brush.Control.Stroke` → aliases Primitive for control borders

**Relationships**: Semantic tokens are defined as SolidColorBrush resources that reference Primitive token colors. Theme dictionaries override Semantic token values while Primitive tokens remain constant.

---

### ThemeCard

Represents a selectable theme option displayed in the SettingsWindow.

| Field | Type | Description |
|-------|------|-------------|
| Theme | Theme | The Theme entity this card represents |
| IsSelected | bool | True if this card is currently the active selection |
| PreviewGradientStart | Color | Start color for the preview thumbnail gradient |
| PreviewGradientEnd | Color | End color for the preview thumbnail gradient |
| AccentStripColor | Color | Color of the accent strip at the bottom of the preview |
| Name | string | Display name (e.g., "Dark", "White (Light)", "Custom") |
| Description | string | Brief description (e.g., "Full dark theme", "Clean light theme") |
| AccentSwatches | list<Color> | Available accent color options (Custom theme only) |

**Relationships**: ThemeCard is bound to a Theme. Multiple ThemeCards exist in the SettingsWindow's theme selection panel. Only one ThemeCard per theme name can be IsSelected at a time.

---

### AccentSwatch

Represents a single accent color option on the Custom theme card.

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Color name (e.g., "Blue", "Teal", "Purple") |
| HexValue | string | Hex color code (e.g., "#3A86FF") |
| IsSelected | bool | True if this swatch is the currently selected accent |

**Preset swatches**: Blue (#3A86FF), Green (#2ED573), Purple (#A855F7), Orange (#FFA502), Pink (#EC4899), Teal (#14B8A6), Red (#EF4444), Cyan (#06B6D4)

**Relationships**: Each AccentSwatch belongs to the Custom ThemeCard. The selected swatch's HexValue is stored as the Custom theme's AccentColor.

---

### ControlStyle

Represents a WPF style/template for a control type.

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Style name (e.g., "FluentButton", "RoundComboBox", "ThemeCard") |
| TargetType | string | WPF target type (e.g., "Button", "ComboBox", "Border") |
| HasVisualStateManager | bool | True if the style uses VSM; false if Triggers |
| States | list<string> | All visual states supported (e.g., Normal, Hover, Pressed, Focused, Disabled, PopupOpen) |
| AnimationKey | string | Reference to animations.xaml Storyboard key (if applicable) |
| ShadowKey | string | Reference to Shadows.xaml DropShadowEffect key (if applicable) |
| GlowKey | string | Reference to Glow.xaml DropShadowEffect key (if applicable) |

**Relationships**: ControlStyle resources are loaded via ResourceDictionary and referenced by control templates via DynamicResource.

**VSM controls** (this feature scope): ComboBox, Button, ToggleButton, ThemeCard Border
**Trigger-based controls** (remaining): TextBox, CheckBox, RadioButton, DataGrid, ScrollViewer

---

### ApplicationSettings

Represents the persisted user preferences for theme selection.

| Field | Type | Description |
|-------|------|-------------|
| SelectedTheme | string | "Dark", "Light", or "Custom" |
| AccentColor | string | Hex color code for Custom theme accent (e.g., "#3A86FF") |

**Relationships**: ApplicationSettings is read by ThemeManager on startup and written when the user changes theme/accent. Both fields persist across application restarts.

---

## Validation Rules

| Entity | Rule | Enforcement |
|--------|------|-------------|
| Theme.Name | Must be one of: "Dark", "Light", "Custom" | ThemeManager.ApplyTheme validates input |
| Theme.AccentColor | Must be a valid hex color code (#RRGGBB format) | SettingsViewModel validates before save |
| AccentSwatch.IsSelected | Exactly one swatch must be IsSelected=true at any time | SettingsViewModel enforces mutual exclusivity |
| ThemeCard.IsSelected | Exactly one card must be IsSelected=true at any time | SettingsViewModel enforces mutual exclusivity |
| ControlStyle.AnimationKey | Animation duration must be ≤ 200ms | Animation library enforces budget |
| ApplicationSettings.SelectedTheme | Must be non-null and non-empty | ThemeManager applies default "Dark" if null |

## State Transitions

### Theme State Machine

```
[No Theme] --> [Dark] <--> [Light]
                  |            |
                  +-> [Custom] +-> [Custom:Teal]
                  |            |         |
                  +-> [Custom:Blue]      |
                  |                     |
                  +-> [Custom:Purple] <-+
                  |                     |
                  +-> [Custom:Green] ---+
```

Each transition is triggered by ThemeManager.ApplyTheme(themeName, accentColor).

### ThemeCard Selection State

```
[Unselected] --[click]--> [Selected]
     ^                    |
     |                    v
     +<---[click another card]---+
```

### AccentSwatch Selection State

Same as ThemeCard: only one swatch selected at a time within the Custom theme card.

## Key Attributes Without Implementation Details

| Attribute | Value |
|-----------|-------|
| Number of built-in themes | 3 (Dark, Light, Custom) |
| Accent color presets for Custom | 8 options |
| Controls with VSM migration | 4 (ComboBox, Button, ToggleButton, ThemeCard) |
| Controls on Triggers | 3 (TextBox, CheckBox, DataGrid) |
| Theme validation gates | 8 checkpoints |
| Animation budget maximum | 200ms |
| DPI configurations supported | 100%, 125%, 150%, 200% |
| Required VSM states per control | Normal, Hover, Pressed, Focused, Disabled, Selected (where applicable), PopupOpen (ComboBox) |