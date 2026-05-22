# Theme API Contract

**Phase**: 1 — Design & Contracts

## ThemeManager Service

The `ThemeManager` singleton is the sole gateway for all theme mutations across the application.

### Public Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ApplyTheme(theme, accentColor?)` | `string theme` (Dark/Light/Custom), `string? accentColor` (hex or null) | `void` | Switch theme and optionally set accent color. Coalesces rapid calls (≈150ms debounce). |
| `LoadThemeFromSettings()` | None | `void` | Load persisted theme + accent from `Properties.Settings.Default` and apply. |
| `SaveCurrentTheme()` | None | `void` | Persist current theme + accent to `Properties.Settings.Default`. |
| `ApplyAccentColor(accentColor)` | `string accentColor` (hex) | `void` | Update accent color resources and glow effects without changing theme. |

### Public Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentTheme` | `string` | Currently active theme name |
| `CurrentAccentColor` | `string` | Currently active accent color hex |

### Public Events

| Event | Payload | Description |
|-------|---------|-------------|
| `ThemeChanged` | `(string theme, string accentColor)` | Fired after theme + accent fully applied. Dispatched on UI thread. |

---

## RenderModeService

Determines the safe rendering mode at runtime.

### Public Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Initialize()` | None | `void` | Run detection on startup. Defaults to FallbackSafe on failure. |
| `GetCurrentMode()` | None | `RenderMode` | Current mode: `WindowChrome` or `FallbackSafe` |
| `IsSafeModeRequired()` | None | `bool` | Shortcut for `GetCurrentMode() == FallbackSafe` |
| `IsGpuAvailable()` | None | `bool` | Whether GPU acceleration is detected |
| `IsTransparencySupported()` | None | `bool` | Whether transparency rendering is supported |

### Public Events

| Event | Payload | Description |
|-------|---------|-------------|
| `RenderModeChanged` | `(RenderMode mode)` | Fired when render mode changes (currently startup only) |

---

## App.Config Interface

Settings stored in `Properties/Settings.settings` (ApplicationSettingsBase):

| Setting | Type | Default | Scope |
|---------|------|---------|-------|
| `SelectedTheme` | `string` | `"Dark"` | User |
| `AccentColor` | `string` | `"3A86FF"` | User |

---

## Token Layer Contract

Resource dictionaries follow a strict Primitive → Semantic → Component → Control layer chain:

| Layer | File(s) | References | Referenced By |
|-------|---------|------------|---------------|
| Primitive | `Base/Colors.xaml` | Direct hex values | Semantic Color tokens |
| Semantic Color | `Base/Colors.xaml` | Primitive tokens | Semantic Brush tokens |
| Semantic Brush | `Base/Colors.xaml` | Semantic Color tokens | Component tokens + Controls |
| Component | `Base/ComponentTokens.xaml` | Semantic Brush, Spacing, Radius tokens | Control templates |
| Control | `Controls/*.xaml` | Component tokens + Semantic Brush | Window XAML |
| Theme Override | `{Dark,Light,Custom}/*.xaml` | Overrides Semantic Brush tokens | Runtime swap by ThemeManager |
