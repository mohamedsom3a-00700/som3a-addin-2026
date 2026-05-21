# Som3a Add-in 2026 Constitution v2

**Version**: 2.0.0  
**Ratified**: 2026-05-21  
**Last Amended**: 2026-05-21  
**Previous Version**: 1.2.0  

---

## What's New in v2.0.0

- Added: AI Execution Rules (Principle XVII)
- Added: Token Naming Standards (Principle XVIII)
- Added: Review Gate Requirements (Principle XIX)
- Added: Phase 11 — Legacy Window Migration governance
- Consolidated: All governance docs referenced by file path
- Maintained: All 16 core principles from v1.2.0

---

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

**Rationale**: Modular dictionaries improve scalability, runtime replacement safety, maintainability, and feature isolation.

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

- `Brush.Background.Primary`
- `Brush.Text.Primary`
- `Brush.Accent.Primary`

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

## Extended Principles (v2.0.0)

---

### XVII. AI Execution Rules

Before creating ANY new token, control, dictionary, or architecture, AI agents MUST:

1. **Inspect** current implementation
2. **Reuse** existing structures
3. **Extend** existing systems
4. **Avoid** duplicate architecture

**Inline Value Prohibition:**

- No inline colors
- No inline shadows
- No inline margins
- No inline font sizes
- No inline border thicknesses
- No inline corner radius

**MUST USE:**

- `DynamicResource` for themeable properties
- Semantic tokens for UI colors
- Centralized effects from `Effects/*.xaml`
- Shared control templates
- Component tokens for component-specific values

**Full rules documented in:** `Docs/Architecture/AGENT_RULES.md`

---

### XVIII. Token Naming Standards

All tokens MUST follow standardized naming conventions.

**Layers:**

```text
Primitive Tokens     → Primitive.<Color>.<Shade>
Semantic Tokens      → Brush.<Category>.<State>
Spacing Tokens       → Spacing.<Name>
Radius Tokens        → Radius.<Size>
Elevation Tokens     → Elevation.<Level>
Motion Tokens        → Motion.<Type>.<Property>
Component Tokens     → Component.<Control>.<Part>.<State>
```

**Full rules documented in:** `Docs/Architecture/TOKEN_RULES.md`

---

### XIX. Review Gate Requirements

Every phase MUST pass ALL review gates before proceeding:

1. **Local Manual Testing** — Build, Excel host, DPI, popup, rendering
2. **GitHub PR Review** — Architecture, style, performance
3. **CodeRabbit Review** — Code smells, performance, MVVM, memory, leaks, duplicates
4. **Manual Architectural Review** — Token usage, naming, DynamicResource, no inline values

**Full checklist documented in:** `Docs/Architecture/REVIEW_CHECKLIST.md`

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

- DynamicResource enforcement
- Token architecture validation
- Runtime synchronization validation
- Excel rendering validation
- Theme safety validation
- AI execution rule compliance
- Token naming standard compliance

### Enforcement

Constitutional violations MUST block implementation approval.

Non-compliant code MUST be refactored before merge approval.

---

## Phase 11 — Legacy Window Migration

### Scope

Existing standalone feature windows MAY be migrated to the Shell/Page architecture gradually.

### Rules

- Migration is OPTIONAL per window
- Most-used windows migrated first
- Original `.xaml` preserved until migration validated
- Ribbon launcher updated to open Page in Shell
- Each migrated window MUST pass Excel host test before original removal

### Migration Pattern

```text
MyWindow.xaml
    ↓
Pages/MyPage.xaml
    + Shell navigation registration
```

---

**Version**: 2.0.0 | **Ratified**: 2026-05-21 | **Last Amended**: 2026-05-21
