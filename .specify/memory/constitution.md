<!--
Sync Impact Report
==================

Version Change:
1.2.0 -> 2.0.0-planned

Status: Phases 14-19 implemented (v1.3.0). Phases 20-27 planned for Enterprise Planning Platform (v2.0.0).

Modified Principles:
- (none — enterprise phases do not alter core constitutional principles)

Added Governance Sections:
- Amendment Record (visible changelog — v1.3.0)
- Enterprise Planning Platform phases documented (v2.0.0-planned)

Removed Sections:
- (none)

Templates Requiring Updates:
- (none — enterprise plan documented in enterprise_planning_platform_plan.md)

Follow-up TODOs:
- Execute Phase 20: BOQ Activity Generator
- Execute Phase 21: Relationship Generator
- Execute Phase 22: Duration Estimator
- Execute Phase 23: Dashboard & Home
- Execute Phase 24: Localization & RTL
- Execute Phase 25: Full Platform Rebranding
- Execute Phase 26: Release Candidate
- Execute Phase 27: Persistence & Database
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

- DynamicResource enforcement
- Token architecture validation
- Runtime synchronization validation
- Excel rendering validation
- Theme safety validation

### Enforcement

Constitutional violations MUST block implementation approval.

Non-compliant code MUST be refactored before merge approval.

---

## Amendment Record

| Version | Date | Summary |
|---------|------|---------|
| 1.0.0 | 2026-05-01 | Initial constitution — core principles I–VI, governance, amendment process |
| 1.1.0 | 2026-05-10 | Theme expansion, MVVM cleanup, Popup/Window architecture rules added |
| 1.2.0 | 2026-05-21 | Major redesign: DynamicResource-only, Primitive & Semantic tokens, Excel Rendering Safety, WindowChrome Enforcement, Centralized Effects, Theme Safety & Recovery. 10 principles consolidated, 10 removed |
| 1.3.0 | 2026-05-27 | Platform Foundation (Phase 14): .NET 8.0 libraries (Contracts, Domain, AI, Exporting, Validation, Diagnostics, Infrastructure, Localization, Plugin.SDK), Bridge interop. Shell Refactor (Phase 15): categorized sidebar, dynamic plugin pages, keyboard nav, unsaved-changes dialog. Dynamic Settings Platform (Phase 16): type-safe settings, sidebar grouping. Theme Expansion (Phase 17): translucent backgrounds, blur effects, theme-switch fix. AI Core Infrastructure (Phase 18): provider abstraction, prompt orchestration, token tracking. WBS Engine (Phase 19): alpha codes, color style sync, shell polish, settings panel |
| 2.0.0-planned | 2026-Q3 (planned) | **Enterprise Planning Platform** — Phases 20–27. BOQ Activity Generator (Phase 20): AI-powered activity generation from Excel BOQ data. Relationship Generator (Phase 21): predecessor/successor logic engine with loop detection. Duration Estimator (Phase 22): productivity analysis, variance estimation, AI recommendations. Dashboard & Home (Phase 23): landing page with widgets, diagnostics, quick actions. Localization & RTL (Phase 24): full English + Arabic with dynamic RTL switching. Full Platform Rebranding (Phase 25): Planova identity, brand assets, visual system overhaul. Release Candidate (Phase 26): production packaging, installer, final validation. Persistence & Database (Phase 27): SQLite infrastructure, repository pattern, migration engine |

**Version**: 2.0.0-planned | **Ratified**: 2026-05-21 | **Last Amended**: 2026-05-27
