# Data Model: Rendering & Window Infrastructure

**Date**: 2026-05-22

## Entities

### Window

| Field | Type | Description |
|-------|------|-------------|
| FileName | string | `.xaml` file path relative to WpfApp2/ |
| InheritsModernWindow | boolean | Whether the window inherits from ModernWindow |
| AllowsTransparency | boolean | Current AllowsTransparency setting |
| HasInlineEffects | boolean | Whether window has inline DropShadowEffect or effects |
| HasSnapsToDevicePixels | boolean | Whether SnapsToDevicePixels="True" is set |
| HasUseLayoutRounding | boolean | Whether UseLayoutRounding="True" is set |
| BackgroundSource | string | Background resource key (e.g., `Brush.Background.Root`) |
| BorderSource | string | Border resource key (e.g., `CardStrokeBrush`) |
| MigrationPhase | string | Current phase: "P2" (refactor now) or "P11" (deferred migration) |

### RenderMode

| Field | Type | Description |
|-------|------|-------------|
| Mode | enum (WindowChrome, FallbackSafe) | Current rendering strategy |
| DeterminedAt | DateTime | When mode was determined (app startup) |
| IsFixed | boolean | True — cannot change mid-session |
| GpuAvailable | boolean | Whether GPU acceleration is detected |
| TransparencySupported | boolean | Whether transparency rendering is supported |
| DefaultOnFailure | enum (FallbackSafe) | Mode to use on service failure |

### ShadowVariant

| Field | Type | Description |
|-------|------|-------------|
| Key | string | Resource key (e.g., `Shadow.Window`, `Shadow.Window.Safe`) |
| BlurRadius | double | Shadow blur radius in device-independent pixels |
| Opacity | double | Shadow opacity (0.0–1.0) |
| Direction | enum (Window, Card, Popup) | Shadow target category |
| IsSafeVariant | boolean | Whether this is a FallbackSafe variant |

### DpiScale

| Field | Type | Description |
|-------|------|-------------|
| ScaleX | double | Horizontal scale factor (1.0 = 100%) |
| ScaleY | double | Vertical scale factor (1.0 = 100%) |
| IsHighDpi | boolean | True when Scale >= 1.5 |

## State Transitions

```
App Startup
    ↓
RenderModeService.Initialize()
    ↓
[Success] → Determine mode (WindowChrome or FallbackSafe) → Cache for session
[Failure] → Default to FallbackSafe
    ↓
Windows open/close using cached mode throughout session
```
