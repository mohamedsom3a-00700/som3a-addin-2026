# Research: Settings & Personalization UX

## Decision Record

### Settings Persistence Mechanism

- **Decision**: Extend `Properties.Settings.Default` (ApplicationSettingsBase) for runtime settings + JSON file for import/export
- **Rationale**: The existing ThemeManager already uses Properties.Settings.Default for theme/accent persistence. Extending this same mechanism maintains consistency. JSON is the native format for the existing codebase (ThemeSettings.cs was designed for JSON).
- **Alternatives considered**:
  - XML serialization: Unnecessary complexity, JSON is more compact and the project already targets JSON
  - Registry storage: Not portable, harder to debug, no import/export support
  - Database (SQLite): Overkill for ~15 settings values in a single-user add-in

### Settings Window Architecture

- **Decision**: Single window with sidebar + content area (Frame/ContentControl), not separate windows or tabs
- **Rationale**: Matches Windows 11 Settings app pattern. A single ModernWindow with a sidebar and content switcher avoids creating multiple windows and is consistent with the spec's "without navigating away" requirement.
- **Alternatives considered**:
  - TabControl: Less discoverable for 6 categories, doesn't match Windows 11 aesthetic
  - Separate windows per category: Violates UX principle, adds window management complexity
  - NavigationWindow: Doesn't match ModernWindow pattern from project constitution

### Live Preview Mechanism

- **Decision**: In-panel preview area using a snapshot-style rendering approach
- **Rationale**: The spec assumes a dedicated preview area within the Appearance panel. Preview applies theme resources to a scoped element (e.g., a Border/Grid acting as a mini-window preview) without mutating the main application theme.
- **Alternatives considered**:
  - Full-window preview: Complex to implement, could confuse users, more resource-intensive
  - Screenshot mockup: Static, doesn't reflect actual DynamicResource behavior
  - Separate preview window: Overkill for a preview

### Settings Category Panel Implementation

- **Decision**: Each panel is a separate UserControl loaded dynamically into the content area
- **Rationale**: Decouples panel development, enables independent testing, and matches MVVM patterns. Each panel has its own ViewModel slice if needed.
- **Alternatives considered**:
  - Single large XAML with visibility toggles: Hard to maintain, violates modularity
  - TemplateSelector: Over-engineering for 6 panels

### Animation Control Implementation

- **Decision**: AnimationManager service wrapping existing animation infrastructure; toggles AnimationScale attached property on root elements
- **Rationale**: The existing Animations.xaml uses Storyboards with set durations. Rather than conditionally starting/stopping storyboards, an AnimationScale factor (1.0, 0.3, 0.0) is applied to animation durations throughout the visual tree via an attached property.
- **Alternatives considered**:
  - Conditional storyboard toggling: Hard to maintain, requires modifying every animation trigger
  - Replacing ResourceDictionaries: Performance overhead, complex

### Density Control Implementation

- **Decision**: Apply density multipliers to spacing tokens via a transient ResourceDictionary overlay
- **Rationale**: The spec says density affects padding/margins but not font sizes. An overlay dictionary with the same Spacing.* keys but different values can be inserted/removed from MergedDictionaries at runtime, matching the existing ThemeManager pattern.
- **Alternatives considered**:
  - Per-control property binding: Requires modifying every control template
  - Global style refresh: More disruptive, less targeted

### Import/Export Format

- **Decision**: JSON with version field and forward compatibility
- **Rationale**: Matches the existing codebase preferences. Version field enables future schema changes. Unknown keys are ignored on import (graceful forward compatibility).
- **Alternatives considered**:
  - XML: More verbose, no benefit for this use case
  - Binary: Not human-readable, harder to debug
  - Ini file: Doesn't support nested settings structure

### Settings ViewModel Pattern

- **Decision**: Single SettingsViewModel with category sub-viewmodels exposed as properties
- **Rationale**: All settings exist in one window context. A parent ViewModel manages the selected category and shares the save/apply/cancel state machine across all panels. Each panel gets its own child ViewModel for isolated concern.
- **Alternatives considered**:
  - Separate ViewModel per panel with messaging: More complex wiring, harder to manage cross-panel state (Apply/Cancel affects all panels)
  - Single massive ViewModel: Violates separation of concerns

## Best Practices Confirmed

1. **WPF MVVM**: INotifyPropertyChanged, ICommand, ViewModel-first composition (confirmed from Phase 6 patterns)
2. **DynamicResource**: All themeable bindings use DynamicResource (constitution §III)
3. **ThemeManager**: Singleton, debounce timer, accent application, persistence, events — all existing (Phase 3)
4. **ModernWindow**: All windows inherit; WindowRenderModeDetector for VSTO safety (Phase 2)
5. **Centralized effects**: Shadows.xaml, Glow.xaml, Animations.xaml — reuse, don't create (Phases 1-3)
6. **Constitution v1.2.0**: 16 principles governing all UI work
