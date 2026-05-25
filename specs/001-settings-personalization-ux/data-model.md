# Data Model: Settings & Personalization UX

## Entity: UserSettings

Represents the complete set of user-configurable preferences. Persisted via Properties.Settings.Default and exportable to JSON.

| Field | Type | Default | Constraints | Description |
|-------|------|---------|-------------|-------------|
| SelectedTheme | string | "Dark" | OneOf("Dark", "Light", "Custom") | Current theme mode |
| AccentColor | string | "#3A86FF" | Valid hex color (#RRGGBB) | Current accent color |
| AnimationSpeed | string | "Full" | OneOf("Off", "Reduced", "Full") | Animation intensity level |
| UiDensity | string | "Normal" | OneOf("Compact", "Normal", "Spacious") | UI spacing density |
| BackgroundStyle | string | "Gradient" | OneOf("Solid", "Gradient") | Window background style |
| HighContrastEnabled | bool | false | — | High contrast mode toggle |
| FocusIndicatorEnabled | bool | true | — | Focus glow indicator visibility |
| RenderMode | string | "Auto" | OneOf("Auto", "Safe", "Full") | Excel render mode override |
| SafeModeEnabled | bool | false | — | Force safe rendering mode |

## Entity: SettingsCategory

Represents a top-level settings group in the sidebar navigation.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | string | Unique, kebab-case | Category identifier (e.g., "appearance") |
| DisplayName | string | Non-empty | User-visible category name |
| Icon | string | Must reference existing icon | Icon key for sidebar display |
| PanelType | Type | Must be UserControl | The panel UserControl type to load |
| Order | int | 1-6, unique | Display order in sidebar |

### Categories (fixed set)

| Id | DisplayName | Order | Description |
|----|-------------|-------|-------------|
| appearance | Appearance | 1 | Theme, accent, background, preview |
| performance | Performance | 2 | Animation speed, UI density |
| accessibility | Accessibility | 3 | High contrast, focus indicators |
| diagnostics | Diagnostics | 4 | Import/export, system info |
| excel | Excel | 5 | Render mode, safe mode, DPI |
| plugins | Plugins | 6 | Plugin list (stub until Phase 9) |

## Entity: SettingsExport

Represents a serializable snapshot of UserSettings for import/export operations.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Version | string | Semantic version | Schema version for forward compatibility |
| ExportedAt | DateTime | ISO 8601 | When the snapshot was created |
| Settings | UserSettings | — | The actual settings values |
| AppVersion | string | — | Source application version for diagnostics |

## State Transitions

### Live Preview State Machine

```
[Idle] → click theme card → [Previewing]
[Previewing] → click different card → [Previewing] (updated preview)
[Previewing] → click Apply → [Applied] → save → [Idle]
[Previewing] → click Cancel → [Idle] (preview discarded)
[Previewing] → switch category → [Idle] (preview discarded)
```

### Settings Save Lifecycle

```
[Unsaved] → user changes setting → [Dirty]
[Dirty] → save (auto/apply/import) → success → [Saved]
[Dirty] → save → failure → [Dirty] + toast notification
[Saved] → user changes setting → [Dirty]
```

### Import/Export Flow

```
Export:
[Dirty/Saved] → click Export → file dialog → user selects path → serialize to JSON → [Saved]

Import:
[Any] → click Import → file dialog → user selects file → validate JSON → 
  success → apply all settings → [Saved]
  failure → show error → [No change]
```
