# Data Model: Fluent Icon & Control Migration

**Date**: 2026-05-31
**Branch**: `032-fluent-icon-control-migration`

## Entities

### FluentIconInstance (Runtime, not persisted)

Represents a resolved Fluent 2 icon at runtime.

| Field | Type | Description |
|-------|------|-------------|
| IconKind | `FluentIcons.Common.Symbol` | The Fluent 2 icon enum value |
| IconName | `string` | PascalCase string name (e.g., `"Home"`, `"Settings"`) |
| Size | `IconSize` | `Small`, `Medium`, `Large` (default: `Medium`) |
| ThemeVariant | `ThemeVariant` | `Dark` or `Light` (auto-resolved from theme) |

**Resolution flow**: `string` → `FluentIconConverter` → `FluentIcon` control

### IconMappingTable (Documentation, not code)

A mapping document listing every icon name used in the application and its Fluent 2 equivalent.

| Current Icon Name | Fluent 2 Symbol | Fallback | Notes |
|-------------------|-----------------|----------|-------|
| `ChartTimelineVariant` | `Symbol.ChartTimelineVariant` | `Symbol.Chart` | Planning category |
| `Compare` | `Symbol.Compare` | `Symbol.ArrowsLeftRight` | Primavera Compare |
| `FileDocument` | `Symbol.Document` | `Symbol.Page` | Primavera Results |
| `Home` | `Symbol.Home` | — | Dashboard |
| `Cog` | `Symbol.Settings` | — | Settings |
| `null` | `Symbol.Question` | — | Fallback for unregistered |

**Full mapping**: To be generated during implementation by auditing all `SidebarRegistrationService` registrations and ViewModel icon assignments.

### SidebarPageRegistration (Existing, Unchanged)

Registered in `SidebarRegistrationService.cs`. No schema changes needed — icon names are already Fluent 2 compatible.

## State Transitions

### Icon Resolution State Machine

```
[Icon Name String]
    ↓
[FluentIconConverter.Convert()]
    ↓ (success)
[FluentIcon control rendered]
    ↓ (failure — unknown name)
[Fallback Icon (Symbol.Error CircleHalfFilled)]
    ↓
[Diagnostic warning logged]
```

## Validation Rules

- Icon name MUST be a valid `FluentIcons.Common.Symbol` enum member name
- Icon name MUST NOT be null or empty (fallback to `Symbol.Error` if so)
- Icon name MUST be PascalCase (e.g., `"Home"`, not `"home"` or `"HOME"`)
- Unicode codepoint strings (e.g., `"\U000F05D2"`) MUST be replaced with FluentIcons Symbol enum values
