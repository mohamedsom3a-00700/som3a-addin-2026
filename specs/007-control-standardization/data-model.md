# Data Model: Control Standardization

**Date**: 2026-05-22
**Feature**: Control Standardization (Phase 4)
**Spec**: [spec.md](./spec.md)

---

## Entity: Control Style

The authoritative WPF `Style` definition for each control type.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `TargetType` | `System.Type` | The WPF control type this style targets | Must be one of the 10 standardized control types. Exactly one style per TargetType across all dictionaries. |
| `ResourceKey` | `string` | The x:Key used to reference this style | Convention: implicit key (no x:Key) for application-level styles; explicit key for named variants |
| `Setters` | `List<Setter>` | Property-value pairs defining default appearance | All Brush/Color values must use `{DynamicResource}` — zero hardcoded colors |
| `Triggers` | `List<Trigger>` | Property triggers for state changes (hover, focus, disabled) | Must cover all VSM states: Normal, MouseOver, Focused, Pressed, Disabled, Selected |
| `BasedOn` | `Style` (optional) | Parent style to inherit from | Used for theme-specific overrides; must not create circular references |
| `Location` | `string` (path) | File path to the XAML dictionary | Must be within `Theme/Controls/` directory |

**Validation rules**:
- All Brush/Color setters use `{DynamicResource}` (FR-011)
- No inline `DropShadowEffect` in any setter or template (FR-012)
- `FocusVisualStyle` set to `{DynamicResource Glow.Focus}` (FR-006)
- `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"` present

**Relationships**:
- A Control Style references 1+ Semantic Tokens via `{DynamicResource}`
- A Control Style may contain 1 Control Template
- A Control Style is uniquely identified by its TargetType

---

## Entity: Control Template

The `ControlTemplate` that defines the visual structure and state animations of a control.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `TargetType` | `System.Type` | The control type this template is for | Must match the parent Style's TargetType |
| `VisualTree` | `FrameworkElementFactory` | The visual element hierarchy | All child elements must use DynamicResource for themeable properties |
| `Triggers` | `List<Trigger>` | VisualStateManager triggers or data triggers | All animations ≤200ms duration (FR-006, Constitution §IX) |
| `Resources` | `ResourceDictionary` | Template-scoped resources | Must not duplicate resources available at application level |

**Relationships**:
- Contained within 1 Control Style
- References 1+ Effect resources from `Effects/Shadows.xaml` or `Effects/Glow.xaml`

---

## Entity: Virtualization Configuration

Settings applied to scrollable containers to ensure memory-efficient rendering.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `TargetContainers` | `List<Type>` | Controls this config applies to | DataGrid, ListView, TreeView, ScrollViewer |
| `VirtualizationMode` | `enum` | Standard or Recycling | MUST be `Recycling` (FR-004) |
| `EnableRowVirtualization` | `bool` | Whether rows are virtualized | MUST be `True` (FR-004) |
| `EnableColumnVirtualization` | `bool` | Whether columns are virtualized (DataGrid only) | Optional; `True` recommended for wide DataGrids |
| `ScrollUnit` | `enum` | Item or Pixel | MUST be `Item` when virtualization is enabled (conflict with Pixel virtualization) |
| `IsVirtualizing` | `bool` | Master switch for virtualization | MUST be `True` |

**Validation rules**:
- `VirtualizationMode="Recycling"` and `EnableRowVirtualization="True"` must both be present (FR-004)
- `ScrollUnit="Item"` when virtualization enabled
- No `VirtualizationMode="Standard"` on any scrollable container

---

## Entity: Keyboard Navigation Map

The Tab order and keyboard interaction patterns for each control type.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `ControlType` | `System.Type` | The control type | All 10 standardized types must be mapped |
| `TabIndexOrder` | `int` | Default Tab index | Logical top-to-bottom, left-to-right order |
| `IsTabStop` | `bool` | Whether control receives Tab focus | `True` for all interactive controls (FR-007) |
| `IsEnabled` | `bool` | Whether control is interactive | Keyboard must skip disabled controls |
| `KeyBindings` | `List<KeyGesture>` | Custom keyboard shortcuts | Arrow keys for lists (TreeView: FR-008), Enter/Space for activation, Escape for dialogs |
| `FocusVisualKey` | `string` | Resource key for focus indicator | `{DynamicResource Glow.Focus}` for all controls (FR-006) |

**Validation rules**:
- All interactive controls reachable via Tab (SC-004)
- Focus indicator visible in Dark and Light themes
- Escape closes all dialogs and popups

---

## Entity: Empty State Configuration

Placeholder display settings for data-bound controls with no data.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `TargetControls` | `List<Type>` | Controls with empty state handling | DataGrid, ListView, TreeView |
| `EmptyMessage` | `string` | Text displayed when data source is empty | "No data available" (standardized across all) |
| `LoadingIndicatorStyle` | `Style` | Visual style for loading state | ProgressBar or loading animation; consistent style |
| `TriggerCondition` | `Trigger` | When to show the empty state | Bind to `HasItems` or `Items.Count == 0` |

**Relationships**:
- Referenced by Control Style for DataGrid, ListView, TreeView
- Displayed when data source is empty or null (FR-014)

---

## State: Control Visual State Machine

Each control type exposes these visual states (VSM):

```
Normal ──→ MouseOver ──→ Pressed ──→ Selected (for list controls)
  │                                                     │
  └──→ Focused                                          │
  │                                                     │
  └──→ Disabled                                         │
                                                        │
KeyboardFocus ──────────────────────────────────────────┘
```

| State | Trigger | Visual Feedback |
|-------|---------|-----------------|
| Normal | Default | Base styling from semantic tokens |
| MouseOver | Mouse enters bounds | Background/border color transition (≤200ms) |
| Focused | Tab/click focus | `Glow.Focus` effect visible |
| Pressed | Mouse down | Background color darkens (≤100ms) |
| Disabled | `IsEnabled=false` | Opacity reduces (`Opacity.Disabled`), no interaction |
| Selected | Item selected (lists) | Accent color background |
| KeyboardFocus | Tab focus on any interactive | Consistent focus ring via `Glow.Focus` |
