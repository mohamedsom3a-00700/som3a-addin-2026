<!--
Sync Impact Report
==================

Version Change:
1.1.0 → 1.2.0

Modified Principles:
- I. Library-First Modular Architecture (expanded)
- III. DynamicResource Only → III. DynamicResource-Only Theme Architecture (expanded)
- IV. Runtime Theme Switching → IV. Runtime Theme Mutation Governance (redefined)
- VI. Performance & Efficiency → XIII. Performance & Rendering Efficiency (redefined)

Added Principles:
- V. Primitive & Semantic Token Architecture
- VI. Runtime Theme Synchronization
- VII. Runtime Theme Persistence
- VIII. Full Theme Editing Governance
- IX. Animation Governance
- X. Excel Rendering Safety
- XI. WindowChrome Enforcement
- XII. Centralized Effects Architecture
- XIV. No Third-Party UI Frameworks
- XV. Resource Loading Order Enforcement
- XVI. Theme Safety & Recovery

Added Governance Sections:
- Enforcement

Removed Sections:
- Resource Loading Order (merged into Principle XV)
- Theme System Architecture (superseded by individual principles)
- Popup Architecture Rules (superseded by Excel Rendering Safety)
- Window System (superseded by WindowChrome Enforcement)
- UI Quality Standards (superseded by individual principles)
- Theme Validation Checklist (to be moved to spec checklist)
- VisualStateManager Strategy (to be moved to spec/plan guidance)
- Incremental Migration Rules (to be moved to plan guidance)
- Performance Budget Rules (superseded by Performance & Rendering Efficiency)
- Design Authority Rules (superseded by Enforcement)

Templates Requiring Updates:
✅ .specify/templates/plan-template.md
✅ .specify/templates/spec-template.md
✅ .specify/templates/tasks-template.md

Follow-up TODOs:
- Migrate remaining hardcoded gradients
- Add automated resource validation tooling
-->

# Som3a Add-in 2026 Constitution

## Core Principles

---

### I. Library-First Modular Architecture

All UI systems MUST be organized into modular ResourceDictionary libraries.

Each UI concern MUST remain isolated:

- Base Tokens
- Semantic Tokens
- Effects
- Control Styles
- Theme Overrides
- Window Chrome
- Runtime Services

Every dictionary MUST remain independently testable and replaceable.

**Rationale**: Modular dictionaries improve scalability, runtime replacement safety,
maintainability, and feature isolation.

---

### II. MVVM Architecture Enforcement

Business logic MUST reside inside:

- Services
- ViewModels
- Controllers

Code-behind MUST remain UI-only.

ViewModels MUST NOT:

- Access UI controls directly
- Manipulate ResourceDictionaries
- Handle rendering logic

Services MUST NOT behave as ViewModels.

**Rationale**: Strict MVVM separation improves maintainability, testability, and scalability.

---

### III. DynamicResource-Only Theme Architecture

All themeable properties MUST use:

```xml
{DynamicResource ResourceKey}
```

StaticResource is prohibited for:

- Brushes
- Colors
- Gradients
- Typography
- Borders
- Hover states
- Selection states
- Effects
- Shadows
- Glow effects

Inline colors are prohibited.

Hardcoded gradients are prohibited.

Hardcoded brushes are prohibited.

**Rationale**: Runtime theme switching requires live resource reevaluation.

---

### IV. Runtime Theme Mutation Governance

Theme mutation MUST occur exclusively through ThemeManager.

Direct brush mutation from:

- Windows
- Controls
- ViewModels
- Code-behind

is prohibited.

Only Semantic Tokens MAY mutate during runtime.

Primitive Tokens MUST remain immutable during application execution.

**Rationale**: Protects runtime consistency and prevents unsafe rendering behavior.

---

### V. Primitive & Semantic Token Architecture

The design system MUST implement two independent token layers.

**Primitive Tokens**

Contain raw immutable values.

Examples:

- `Primitive.Blue.500`
- `Primitive.Gray.900`

**Semantic Tokens**

Contain meaning-based aliases.

Examples:

- `AccentBrush`
- `WindowBackgroundBrush`
- `TextPrimaryBrush`

Controls MUST consume Semantic Tokens only.

**Rationale**: Allows safe runtime theme editing without breaking UI consistency.

---

### VI. Runtime Theme Synchronization

All open windows MUST synchronize instantly after theme changes.

Synchronization MUST include:

- Main windows
- Dialogs
- Popups
- Floating windows
- Context menus
- Tooltips

Theme switching MUST NOT require reopening windows.

**Rationale**: Ensures consistent UX and prevents visual fragmentation.

---

### VII. Runtime Theme Persistence

Theme configuration MUST persist automatically.

Persistence MUST include:

- Theme mode
- Accent colors
- Custom semantic colors
- Editor state

Persistence MUST use:

`Properties.Settings.Default`

The application MUST restore themes during startup automatically.

**Rationale**: Improves user experience and consistency across Excel sessions.

---

### VIII. Full Theme Editing Governance

The application MUST support full semantic theme editing.

Editable theme areas include:

- Accent colors
- Window backgrounds
- Card surfaces
- Border colors
- Primary text
- Secondary text
- Hover states
- Selection states
- Glow colors

The editor MUST validate:

- Contrast
- Readability
- Rendering safety

Primitive token editing is prohibited at runtime.

**Rationale**: Ensures safe personalization without breaking rendering stability.

---

### IX. Animation Governance

Animations MUST remain:

- Lightweight
- GPU-safe
- Excel-host-safe

Theme transition animations MUST:

- Complete within 200ms
- Avoid blocking UI threads
- Avoid layout thrashing
- Avoid rendering flicker

Allowed animation types:

- Fade
- Crossfade
- Opacity transitions
- Brush interpolation

Heavy transform animations are prohibited during theme switching.

**Rationale**: Excel VSTO hosting environments are sensitive to rendering instability.

---

### X. Excel Rendering Safety

The UI system MUST remain stable inside Excel VSTO host.

The application MUST support:

- Transparency fallback
- WindowChrome fallback
- Animation fallback
- Safe rendering mode detection

WindowRenderModeDetector MUST automatically activate fallback mode when required.

Black window rendering MUST be prevented.

**Rationale**: Excel rendering pipelines behave differently from standalone WPF.

---

### XI. WindowChrome Enforcement

WindowChrome MUST remain the primary rendering strategy.

Fallback rendering MUST activate automatically when:

- Transparency instability occurs
- Excel host rendering becomes unsafe
- GPU acceleration fails

Custom windows MUST inherit from ModernWindow.

**Rationale**: Provides consistent rendering and window behavior.

---

### XII. Centralized Effects Architecture

All effects MUST originate from centralized dictionaries.

Inline effects are prohibited.

Centralized effects include:

- DropShadowEffect
- Glow effects
- Focus effects
- Selection effects

Nested shadows are prohibited.

**Rationale**: Centralized effects reduce rendering overhead and improve consistency.

---

### XIII. Performance & Rendering Efficiency

Brushes MUST be reusable and shared globally.

Brush duplication SHOULD be minimized.

The UI system MUST avoid:

- Nested heavy visual trees
- Excessive transparency
- Large animation chains
- Runtime layout invalidation loops

Virtualized controls MUST remain virtualized.

**Rationale**: Improves responsiveness inside Excel-hosted WPF windows.

---

### XIV. No Third-Party UI Frameworks

The application MUST use native WPF exclusively.

The following are prohibited:

- MahApps.Metro
- MaterialDesignInXaml
- Telerik UI
- DevExpress UI
- WinUI wrappers

Only native WPF styling is allowed.

**Rationale**: Maintains architectural consistency and rendering predictability.

---

### XV. Resource Loading Order Enforcement

Theme resources MUST load in this order:

1. Primitive Tokens
2. Semantic Tokens
3. Typography
4. Radius
5. Spacing
6. Effects
7. Control Styles
8. Theme Overrides
9. Window Chrome
10. Runtime Overrides

ThemeResources.xaml MUST remain the centralized aggregator.

**Rationale**: Prevents invalid resource dependency ordering.

---

### XVI. Theme Safety & Recovery

The application MUST recover safely from:

- Missing resources
- Corrupted settings
- Invalid colors
- Failed dictionary loading
- Rendering fallback activation

Fallback theme loading MUST exist.

The system MUST default safely to Dark Theme when recovery fails.

**Rationale**: Prevents application instability and startup failures.

---

## Governance

### Amendment Process

All constitutional changes MUST:

- Be documented
- Update affected templates
- Update affected specs
- Include migration guidance
- Include version bump reasoning

### Versioning Policy

Constitution versioning follows semantic versioning.

**MAJOR**: Breaking governance changes.

**MINOR**: New principles or major governance expansion.

**PATCH**: Clarifications and wording improvements.

### Compliance Review

All new features MUST pass constitutional validation before implementation.

Validation includes:

<<<<<<< HEAD
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
=======
- DynamicResource enforcement
- Token architecture validation
- Runtime synchronization validation
- Excel rendering validation
- Theme safety validation

### Enforcement

Constitutional violations MUST block implementation approval.

Non-compliant code MUST be refactored before merge approval.

---

**Version**: 1.2.0 | **Ratified**: 2026-05-18 | **Last Amended**: 2026-05-19
>>>>>>> bf1e864 (fix(theme): resolve 7 inline issues + plan.md nitpick)
