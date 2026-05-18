# Som3a Add-in 2026 Constitution

## Core Principles

### I. Library-First Modular Architecture
Every UI feature starts as a modular component; Theme system MUST follow the defined folder structure (Base/, Dark/, Light/, Custom/, Controls/, Effects/); ResourceDictionaries must be independently testable and self-contained; No organizational-only libraries - clear purpose required for each module.

### II. MVVM Architecture (NON-NEGOTIABLE)
Strict separation of visuals and business logic; Views MUST use MVVM-friendly bindings; Business logic MUST NOT live in code-behind unless required for Office interop; Keep ResourceDictionaries modular.

### III. DynamicResource Only (NON-NEGOTIABLE)
Every theme-aware resource MUST use DynamicResource; StaticResource MUST NOT be used for theme colors or themeable properties; This ensures runtime theme switching without restart.

### IV. Runtime Theme Switching
Theme switching MUST update without restart; Merged dictionaries MUST be replaced dynamically; User preferences MUST be preserved across sessions.

### V. Feature Completeness Standard
Every UI feature MUST be theme-aware, runtime switchable, keyboard accessible, and DPI safe; No hardcoded colors allowed; All features MUST use centralized resources and support dark/light/custom themes.

### VI. Performance & Efficiency
Brushes and templates MUST be reused; Unnecessary visual trees MUST be avoided; Virtualization MUST be used in grids/lists; DropShadowEffect MUST NOT be repeated on frequent elements.

---

## Resource Loading Order

Resource loading order MUST be explicit and followed strictly to avoid conflicts and runtime lookup failures:

```
1. Primitive Tokens (raw palette colors)
2. Semantic Tokens (UI meaning aliases)
3. Typography
4. Radius
5. Effects (Shadows, Glow)
6. Controls
7. Theme Overrides (Dark/Light/Custom)
8. Window Styles
```

**ThemeResources.xaml MUST act as an aggregator** — use `ResourceDictionary.MergedDictionaries` to merge in this exact order. DO NOT replace existing ThemeResources.xaml architecture completely.

## Theme System Architecture

The UI system must feel like:
- Premium enterprise software
- Modern Windows Fluent application
- High-end desktop productivity suite
- Consistent commercial-grade UI platform

The architecture optimizes for: long-term maintainability, runtime stability, consistency, scalability, and future theme extensibility.

### Required Folder Structure

```
Theme/
  Base/
    Colors.xaml          (Primitive + Semantic token definitions)
    Typography.xaml
    Spacing.xaml
    Radius.xaml
    Elevation.xaml
  Dark/
    DarkColors.xaml
    DarkTheme.xaml
  Light/
    LightColors.xaml
    LightTheme.xaml
  Custom/
    CustomColors.xaml
    CustomTheme.xaml
  Controls/
    Button.xaml
    ComboBox.xaml
    TextBox.xaml
    DataGrid.xaml
    ScrollViewer.xaml
    CheckBox.xaml
    TabControl.xaml
    Window.xaml
    ProgressBar.xaml
  Effects/
    Shadows.xaml
    Animations.xaml
    Glow.xaml
  ThemeManager.xaml
```

### Color Token Standards

All colors must use tokenized keys following two-tier naming convention:

#### Primitive Tokens
Raw palette color definitions (raw values, no UI semantics):

```xaml
<!-- Example primitive tokens -->
<SolidColorBrush x:Key="Blue500" Color="#3A86FF"/>
<SolidColorBrush x:Key="Slate900" Color="#0E1720"/>
<SolidColorBrush x:Key="WhiteAlpha95" Color="#F2FFFFFF"/>
```

#### Semantic Tokens
UI-meaning aliases that reference primitives:

```xaml
<!-- Example semantic tokens (alias their primitives) -->
<SolidColorBrush x:Key="Brush.Background.Primary" Color="{StaticResource Slate900}"/>
<SolidColorBrush x:Key="Brush.Background.Secondary" Color="{StaticResource Slate800}"/>
<SolidColorBrush x:Key="Brush.Background.Card" Color="{StaticResource Slate700}"/>
<SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource WhiteAlpha95}"/>
<SolidColorBrush x:Key="Brush.Text.Secondary" Color="{StaticResource WhiteAlpha75}"/>
<SolidColorBrush x:Key="Brush.Text.Disabled" Color="{StaticResource WhiteAlpha40}"/>
<SolidColorBrush x:Key="Brush.Accent.Primary" Color="{StaticResource Blue500}"/>
<SolidColorBrush x:Key="Brush.Stroke.Card" Color="{StaticResource WhiteAlpha20}"/>
<SolidColorBrush x:Key="Brush.Control.Background" Color="#330E1720"/>
<SolidColorBrush x:Key="Brush.Control.Stroke" Color="{StaticResource WhiteAlpha20}"/>
```

**Rule**: Controls SHOULD reference semantic tokens (`{DynamicResource Brush.Background.Primary}`) where applicable. Existing flat keys (`BackgroundBrush`, `AccentBrush`) are kept for backward compatibility but new work should prefer semantic naming. This separation is REQUIRED for future custom theme generation.

No inline color values in control templates.

### Typography Standards

Centralized typography styles required:

```xaml
Style.Text.Title
Style.Text.Subtitle
Style.Text.Body
Style.Text.Caption
Style.Text.Button
```

No inline font sizing unless necessary for specific UI requirements.

## Popup Architecture Rules

ComboBox and popup-based controls require explicit popup architecture to render correctly in borderless/WPF environments hosted inside Excel.

Every Popup MUST follow these rules:

```text
- AllowsTransparency: False when possible (True only when transparent blending is required)
- PlacementTarget: Must be correctly bound to the parent control
- Placement: Bottom (or appropriate relative placement)
- StackingBehavior: FlowDirection-aware
- Render in top visual layer without clipping from parent containers
- Support WindowChrome hosting (no AllowsTransparency conflicts)
- Support keyboard navigation (Arrow keys, Escape to close, Tab)
- Support DPI scaling (no hardcoded pixel values)
- Use DropShadowEffect from Effects/Shadows.xaml (not inline)
```

Avoid:
- `AllowsTransparency="True"` unless absolutely necessary
- Inline `DropShadowEffect` definitions in popup templates
- Hardcoded popup z-index or clipping containers

---

## Window System

### ModernWindow Base

All application windows MUST inherit from ModernWindow with these required features:

- Borderless design with custom title bar
- Transparent background support
- Drop shadow rendering
- DPI awareness
- Theme-aware chrome

### Required Shadow System

```xaml
Shadow.Window (BlurRadius=30, ShadowDepth=0, Opacity=0.35)
Shadow.Small
Shadow.Medium
Shadow.Large
Shadow.Glow
```

## UI Quality Standards

### Control Design Language

**Radius Rules:**
- Buttons: 8
- Inputs: 8
- Panels: 12
- Windows: 14
- Cards: 16

**Spacing Rules (4pt Grid System):**
4, 8, 12, 16, 20, 24, 32, 40

**Elevation:**
Use shadow and contrast; do NOT use thick borders.

### Control Standards

Every screen MUST maintain:

- Consistent padding
- Visual hierarchy
- Aligned spacing
- Readable contrast
- High readability

### Animation Rules

Animations MUST be subtle. Allowed: Fade, Opacity, Translate, Glow transition, Border transition. Avoid: Large scaling, Bouncy effects, Heavy blur animations.

### DataGrid Standards

Required features:

- Theme-aware headers
- Selection states
- Alternating rows
- Hover states
- Custom ScrollViewer
- Row virtualization

---

## Theme Validation Checklist

Every theme (Dark, Light, Custom) MUST pass these validation gates before being considered complete:

```text
[ ] Contrast ratio: Text on background ≥ 4.5:1 (WCAG AA)
[ ] DataGrid readability: Row text, header text, selection visible
[ ] Disabled states: Clearly dimmed, not invisible
[ ] Hover visibility: Interactive elements show hover state
[ ] Focus visibility: Keyboard focus ring clearly visible
[ ] Popup readability: ComboBox, Tooltip, Popup content readable
[ ] Accessibility: No text disappears on any background combination
[ ] DPI readability: All text and UI elements scale correctly at 100%, 125%, 150%, 200%
```

Use the theme validation checklist before marking any theme as complete.

---

## VisualStateManager Strategy

The project relies heavily on Triggers. For production-grade architecture, introduce VisualStateManager gradually.

### Initial Focus Controls
Start with:
- ComboBox (PopupOpen state)
- Button (all states)
- ToggleButton (Selected state)
- Window buttons (min/max/close)

### Required States
Every stateful control MUST implement these states:
```text
- Normal
- Hover
- Pressed
- Focused
- Disabled
- Selected (where applicable)
- PopupOpen (for popup controls)
```

### Migration Rule
Gradual migration only:
1. Identify controls using complex Trigger chains
2. Convert one control type at a time
3. Validate in Excel-hosted environment
4. Then proceed to next control type

---

## Incremental Migration Rules

Migration MUST follow a strict validation sequence. **DO NOT migrate multiple windows before Excel-host validation succeeds.**

```
1. Build foundation (Phase 1-3)
2. Validate one prototype window
3. Validate inside Excel (VSTO host)
4. Validate DPI behavior (100%, 125%, 150%, 200%)
5. Validate WebView2 integration
6. Then migrate gradually (window by window)
```

Every window migration is a separate validation gate.

---

## Performance Budget Rules

The UI must remain stable inside Excel-hosted VSTO environments. These restrictions are enforced:

```text
- AVOID nested DropShadows (each shadow is a GPU-rendered pass)
- AVOID deep Grid nesting (max 3 levels inside visual containers)
- AVOID BlurEffect on scrolling containers
- Virtualize all large lists (ListView, DataGrid, ListBox with >50 items)
- AVOID animations on DataGrid rows
- Minimize expensive visual trees (prefer flat structures)
- Reuse brushes/templates globally (no per-element style definitions)
- AVOID AllowsTransparency=True on frequently updated elements
- Prefer StaticResource for rarely-changing resources, DynamicResource only for themeable ones
```

Performance violations are **blocking** — do not merge PRs that introduce nested DropShadows or BlurEffect on scrollable areas.

---

## WindowChrome Enforcement

The architecture MUST prioritize `WindowChrome` for borderless window rendering.

**Preferred approach**:
```csharp
WindowChrome windowChrome = new WindowChrome()
{
    GlassFrameThickness = new Thickness(0),
    CornerRadius = new CornerRadius(14),
    UseAeroCaptionButtons = false
};
WindowChrome.SetWindowChrome(window);
```

**Avoid**:
```xml
WindowStyle="None"
AllowsTransparency="True"
```

...except for **fallback-safe mode only**.

Fallback mode (WindowStyle=None + AllowsTransparency=True) MUST remain available for:
- Excel hosting edge cases
- WebView2 instability scenarios
- Multi-monitor DPI edge cases

All windows default to WindowChrome. Fallback mode is opt-in only and must be gated behind a compatibility check.

---

## Design Authority Rules

OpenCode and all generated code MUST respect architecture governance. These restrictions are **binding**:

```text
OpenCode MUST NOT:
- Replace ThemeResources.xaml architecture without approval
- Rename existing tokens without a documented migration strategy
- Remove the ThemeManager service
- Replace the MVVM structure
- Introduce third-party UI frameworks or control libraries
- Break backward compatibility with existing windows
- Add AllowsTransparency=True without fallback-safe mode documentation
- Add inline DropShadowEffect definitions (use Effects/Shadows.xaml instead)
- Replace Triggers with VisualStateManager without incremental migration plan
```

Any architecture change requires:
1. Documentation of proposed changes
2. Approval by project lead
3. Migration plan for existing code
4. Passing all theme validation checks

---

## Governance

All PRs and reviews MUST verify compliance with these principles. Complexity must be justified. Use the `Docs/PROJECT_GUIDE.md` for runtime development guidance.

### Amendment Procedure

Amendments to this constitution require:

1. Documentation of proposed changes
2. Approval by project lead
3. Migration plan for existing code
4. Version increment per semantic versioning rules

### Versioning Policy

- MAJOR: Backward incompatible governance/principle removals or redefinitions
- MINOR: New principle/section added or materially expanded guidance
- PATCH: Clarifications, wording, typo fixes, non-semantic refinements

### Compliance Review

Every feature implementation MUST pass the Definition of Done:

- Theme-aware
- Runtime switchable
- Keyboard accessible
- DPI safe
- Visually consistent
- No hardcoded colors
- Uses centralized resources
- Supports dark/light/custom themes

**Version**: 1.1.0 | **Ratified**: 2026-05-18 | **Last Amended**: 2026-05-18# Som3a Add-in 2026 Constitution

## Core Principles

### I. Library-First Modular Architecture
Every UI feature starts as a modular component; Theme system MUST follow the defined folder structure (Base/, Dark/, Light/, Custom/, Controls/, Effects/); ResourceDictionaries must be independently testable and self-contained; No organizational-only libraries - clear purpose required for each module.

### II. MVVM Architecture (NON-NEGOTIABLE)
Strict separation of visuals and business logic; Views MUST use MVVM-friendly bindings; Business logic MUST NOT live in code-behind unless required for Office interop; Keep ResourceDictionaries modular.

### III. DynamicResource Only (NON-NEGOTIABLE)
Every theme-aware resource MUST use DynamicResource; StaticResource MUST NOT be used for theme colors or themeable properties; This ensures runtime theme switching without restart.

### IV. Runtime Theme Switching
Theme switching MUST update without restart; Merged dictionaries MUST be replaced dynamically; User preferences MUST be preserved across sessions.

### V. Feature Completeness Standard
Every UI feature MUST be theme-aware, runtime switchable, keyboard accessible, and DPI safe; No hardcoded colors allowed; All features MUST use centralized resources and support dark/light/custom themes.

### VI. Performance & Efficiency
Brushes and templates MUST be reused; Unnecessary visual trees MUST be avoided; Virtualization MUST be used in grids/lists; DropShadowEffect MUST NOT be repeated on frequent elements.

---

## Resource Loading Order

Resource loading order MUST be explicit and followed strictly to avoid conflicts and runtime lookup failures:

```
1. Primitive Tokens (raw palette colors)
2. Semantic Tokens (UI meaning aliases)
3. Typography
4. Radius
5. Effects (Shadows, Glow)
6. Controls
7. Theme Overrides (Dark/Light/Custom)
8. Window Styles
```

**ThemeResources.xaml MUST act as an aggregator** — use `ResourceDictionary.MergedDictionaries` to merge in this exact order. DO NOT replace existing ThemeResources.xaml architecture completely.

## Theme System Architecture

The UI system must feel like:
- Premium enterprise software
- Modern Windows Fluent application
- High-end desktop productivity suite
- Consistent commercial-grade UI platform

The architecture optimizes for: long-term maintainability, runtime stability, consistency, scalability, and future theme extensibility.

### Required Folder Structure

```
Theme/
  Base/
    Colors.xaml          (Primitive + Semantic token definitions)
    Typography.xaml
    Spacing.xaml
    Radius.xaml
    Elevation.xaml
  Dark/
    DarkColors.xaml
    DarkTheme.xaml
  Light/
    LightColors.xaml
    LightTheme.xaml
  Custom/
    CustomColors.xaml
    CustomTheme.xaml
  Controls/
    Button.xaml
    ComboBox.xaml
    TextBox.xaml
    DataGrid.xaml
    ScrollViewer.xaml
    CheckBox.xaml
    TabControl.xaml
    Window.xaml
    ProgressBar.xaml
  Effects/
    Shadows.xaml
    Animations.xaml
    Glow.xaml
  ThemeManager.xaml
```

### Color Token Standards

All colors must use tokenized keys following two-tier naming convention:

#### Primitive Tokens
Raw palette color definitions (raw values, no UI semantics):

```xaml
<!-- Example primitive tokens -->
<SolidColorBrush x:Key="Blue500" Color="#3A86FF"/>
<SolidColorBrush x:Key="Slate900" Color="#0E1720"/>
<SolidColorBrush x:Key="WhiteAlpha95" Color="#F2FFFFFF"/>
```

#### Semantic Tokens
UI-meaning aliases that reference primitives:

```xaml
<!-- Example semantic tokens (alias their primitives) -->
<SolidColorBrush x:Key="Brush.Background.Primary" Color="#0E1720"/>
<SolidColorBrush x:Key="Brush.Background.Secondary" Color="#1C2B3A"/>
<SolidColorBrush x:Key="Brush.Background.Card" Color="#15202B"/>
<SolidColorBrush x:Key="Brush.Text.Primary" Color="#F2FFFFFF"/>
<SolidColorBrush x:Key="Brush.Text.Secondary" Color="#BFFFFFFF"/>
<SolidColorBrush x:Key="Brush.Text.Disabled" Color="#66FFFFFF"/>
<SolidColorBrush x:Key="Brush.Accent.Primary" Color="#3A86FF"/>
<SolidColorBrush x:Key="Brush.Stroke.Card" Color="#33FFFFFF"/>
<SolidColorBrush x:Key="Brush.Control.Background" Color="#330E1720"/>
<SolidColorBrush x:Key="Brush.Control.Stroke" Color="{StaticResource WhiteAlpha20}"/>
```

**Rule**: Controls SHOULD reference semantic tokens (`{DynamicResource Brush.Background.Primary}`) where applicable. Existing flat keys (`BackgroundBrush`, `AccentBrush`) are kept for backward compatibility but new work should prefer semantic naming. This separation is REQUIRED for future custom theme generation.

No inline color values in control templates.

### Typography Standards

Centralized typography styles required:

```xaml
Style.Text.Title
Style.Text.Subtitle
Style.Text.Body
Style.Text.Caption
Style.Text.Button
```

No inline font sizing unless necessary for specific UI requirements.

## Popup Architecture Rules

ComboBox and popup-based controls require explicit popup architecture to render correctly in borderless/WPF environments hosted inside Excel.

Every Popup MUST follow these rules:

```text
- AllowsTransparency: False when possible (True only when transparent blending is required)
- PlacementTarget: Must be correctly bound to the parent control
- Placement: Bottom (or appropriate relative placement)
- StackingBehavior: FlowDirection-aware
- Render in top visual layer without clipping from parent containers
- Support WindowChrome hosting (no AllowsTransparency conflicts)
- Support keyboard navigation (Arrow keys, Escape to close, Tab)
- Support DPI scaling (no hardcoded pixel values)
- Use DropShadowEffect from Effects/Shadows.xaml (not inline)
```

Avoid:
- `AllowsTransparency="True"` unless absolutely necessary
- Inline `DropShadowEffect` definitions in popup templates
- Hardcoded popup z-index or clipping containers

---

## Window System

### ModernWindow Base

All application windows MUST inherit from ModernWindow with these required features:

- Borderless design with custom title bar
- Transparent background support
- Drop shadow rendering
- DPI awareness
- Theme-aware chrome

### Required Shadow System

```xaml
Shadow.Window (BlurRadius=30, ShadowDepth=0, Opacity=0.35)
Shadow.Small
Shadow.Medium
Shadow.Large
Shadow.Glow
```

## UI Quality Standards

### Control Design Language

**Radius Rules:**
- Buttons: 8
- Inputs: 8
- Panels: 12
- Windows: 14
- Cards: 16

**Spacing Rules (4pt Grid System):**
4, 8, 12, 16, 20, 24, 32, 40

**Elevation:**
Use shadow and contrast; do NOT use thick borders.

### Control Standards

Every screen MUST maintain:

- Consistent padding
- Visual hierarchy
- Aligned spacing
- Readable contrast
- High readability

### Animation Rules

Animations MUST be subtle. Allowed: Fade, Opacity, Translate, Glow transition, Border transition. Avoid: Large scaling, Bouncy effects, Heavy blur animations.

### DataGrid Standards

Required features:

- Theme-aware headers
- Selection states
- Alternating rows
- Hover states
- Custom ScrollViewer
- Row virtualization

---

## Theme Validation Checklist

Every theme (Dark, Light, Custom) MUST pass these validation gates before being considered complete:

```text
[ ] Contrast ratio: Text on background ≥ 4.5:1 (WCAG AA)
[ ] DataGrid readability: Row text, header text, selection visible
[ ] Disabled states: Clearly dimmed, not invisible
[ ] Hover visibility: Interactive elements show hover state
[ ] Focus visibility: Keyboard focus ring clearly visible
[ ] Popup readability: ComboBox, Tooltip, Popup content readable
[ ] Accessibility: No text disappears on any background combination
[ ] DPI readability: All text and UI elements scale correctly at 100%, 125%, 150%, 200%
```

Use the theme validation checklist before marking any theme as complete.

---

## VisualStateManager Strategy

The project relies heavily on Triggers. For production-grade architecture, introduce VisualStateManager gradually.

### Initial Focus Controls
Start with:
- ComboBox (PopupOpen state)
- Button (all states)
- ToggleButton (Selected state)
- Window buttons (min/max/close)

### Required States
Every stateful control MUST implement these states:
```text
- Normal
- Hover
- Pressed
- Focused
- Disabled
- Selected (where applicable)
- PopupOpen (for popup controls)
```

### Migration Rule
Gradual migration only:
1. Identify controls using complex Trigger chains
2. Convert one control type at a time
3. Validate in Excel-hosted environment
4. Then proceed to next control type

---

## Incremental Migration Rules

Migration MUST follow a strict validation sequence. **DO NOT migrate multiple windows before Excel-host validation succeeds.**

```
1. Build foundation (Phase 1-3)
2. Validate one prototype window
3. Validate inside Excel (VSTO host)
4. Validate DPI behavior (100%, 125%, 150%, 200%)
5. Validate WebView2 integration
6. Then migrate gradually (window by window)
```

Every window migration is a separate validation gate.

---

## Performance Budget Rules

The UI must remain stable inside Excel-hosted VSTO environments. These restrictions are enforced:

```text
- AVOID nested DropShadows (each shadow is a GPU-rendered pass)
- AVOID deep Grid nesting (max 3 levels inside visual containers)
- AVOID BlurEffect on scrolling containers
- Virtualize all large lists (ListView, DataGrid, ListBox with >50 items)
- AVOID animations on DataGrid rows
- Minimize expensive visual trees (prefer flat structures)
- Reuse brushes/templates globally (no per-element style definitions)
- AVOID AllowsTransparency=True on frequently updated elements
- Prefer StaticResource for rarely-changing resources, DynamicResource only for themeable ones
```

Performance violations are **blocking** — do not merge PRs that introduce nested DropShadows or BlurEffect on scrollable areas.

---

## WindowChrome Enforcement

The architecture MUST prioritize `WindowChrome` for borderless window rendering.

**Preferred approach**:
```csharp
WindowChrome windowChrome = new WindowChrome()
{
    GlassFrameThickness = new Thickness(0),
    CornerRadius = new CornerRadius(14),
    UseAeroCaptionButtons = false
};
WindowChrome.SetWindowChrome(window);
```

**Avoid**:
```xml
WindowStyle="None"
AllowsTransparency="True"
```

...except for **fallback-safe mode only**.

Fallback mode (WindowStyle=None + AllowsTransparency=True) MUST remain available for:
- Excel hosting edge cases
- WebView2 instability scenarios
- Multi-monitor DPI edge cases

All windows default to WindowChrome. Fallback mode is opt-in only and must be gated behind a compatibility check.

---

## Design Authority Rules

OpenCode and all generated code MUST respect architecture governance. These restrictions are **binding**:

```text
OpenCode MUST NOT:
- Replace ThemeResources.xaml architecture without approval
- Rename existing tokens without a documented migration strategy
- Remove the ThemeManager service
- Replace the MVVM structure
- Introduce third-party UI frameworks or control libraries
- Break backward compatibility with existing windows
- Add AllowsTransparency=True without fallback-safe mode documentation
- Add inline DropShadowEffect definitions (use Effects/Shadows.xaml instead)
- Replace Triggers with VisualStateManager without incremental migration plan
```

Any architecture change requires:
1. Documentation of proposed changes
2. Approval by project lead
3. Migration plan for existing code
4. Passing all theme validation checks

---

## Governance

All PRs and reviews MUST verify compliance with these principles. Complexity must be justified. Use the `Docs/PROJECT_GUIDE.md` for runtime development guidance.

### Amendment Procedure

Amendments to this constitution require:

1. Documentation of proposed changes
2. Approval by project lead
3. Migration plan for existing code
4. Version increment per semantic versioning rules

### Versioning Policy

- MAJOR: Backward incompatible governance/principle removals or redefinitions
- MINOR: New principle/section added or materially expanded guidance
- PATCH: Clarifications, wording, typo fixes, non-semantic refinements

### Compliance Review

Every feature implementation MUST pass the Definition of Done:

- Theme-aware
- Runtime switchable
- Keyboard accessible
- DPI safe
- Visually consistent
- No hardcoded colors
- Uses centralized resources
- Supports dark/light/custom themes

**Version**: 1.1.0 | **Ratified**: 2026-05-18 | **Last Amended**: 2026-05-18