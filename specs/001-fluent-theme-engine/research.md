# Research: WPF Fluent UI Migration — Theme Engine & Runtime Switching

**Feature**: WPF Fluent UI Migration
**Date**: 2026-05-18
**Branch**: 002-fluent-theme-engine

## Research Topics

### RT-001: WPF Runtime Theme Switching with ResourceDictionary

**Decision**: DynamicResource-based theme switching via merged Application resource dictionaries.

**Rationale**: WPF's `DynamicResource` binding system natively supports runtime resource updates. When a merged `ResourceDictionary` is replaced, all `DynamicResource` bindings that reference those keys update automatically without visual flicker. This is the standard WPF pattern for runtime theming and is already used throughout the existing codebase. The ThemeManager replaces the theme-specific ResourceDictionary at the end of the MergedDictionaries list, preserving base tokens and control styles.

**Alternatives considered**:
- Style keyed by theme name (requires style selector): Over-complicated for this scenario.
- Multiple complete ResourceDictionary files (one per theme): Increases file duplication and maintenance burden.
- Code-behind brush manipulation: Violates the DynamicResource Only principle and creates fragile code.

---

### RT-002: WindowChrome vs AllowsTransparency — Excel VSTO Compatibility

**Decision**: WindowChrome as default with automatic runtime fallback detection to WindowStyle=None + AllowsTransparency=True.

**Rationale**: WindowChrome provides a modern borderless window with standard window controls, correct DPI scaling, and no rendering overhead. However, some Excel configurations (especially in specific Office versions and DPI configurations) cause `WindowChrome` to render incorrectly. The automatic runtime detection approach (checking for rendering failures or known problematic Excel versions) provides a self-healing fallback without user action.

**Alternatives considered**:
- Static fallback (WindowStyle=None always): Creates compatibility risks on standard environments where WindowChrome works perfectly.
- Manual config flag: Requires user/admin intervention and adds deployment complexity.
- Conditional build configuration: Adds build management overhead and doesn't adapt to runtime conditions.

---

### RT-003: VisualStateManager vs Triggers — Migration Strategy

**Decision**: VSM for four high-priority controls (ComboBox, Button, ToggleButton, ThemeCards) only. Other controls remain on Triggers.

**Rationale**: VisualStateManager provides cleaner state management for complex controls with multiple mutually exclusive states (especially PopupOpen for ComboBox), better animation integration, and future-proofing. However, full VSM migration of all controls is scope-creep for this feature. The four chosen controls have the most complex state requirements and benefit most from VSM. Keeping others on Triggers maintains consistency with the existing codebase while still delivering the key VSM benefits.

**Alternatives considered**:
- Full VSM migration: Scope too large; would delay the core theme switching feature.
- Trigger-only (no VSM): Misses PopupOpen state management and cleaner state organization.
- No VSM at all: VSM PopupOpen state is the most impactful improvement for the ComboBox.

---

### RT-004: Popup Architecture for ComboBox in Borderless Windows

**Decision**: Popup with AllowsTransparency=False, correct Placement="Bottom" + PlacementTarget binding, centralized DropShadowEffect, and no clipping containers.

**Rationale**: AllowsTransparency=True on Popups in borderless/WPF environments causes z-order and rendering issues especially in Excel-hosted scenarios. Keeping AllowsTransparency=False while properly setting Placement and PlacementTarget ensures the popup renders in the correct layer. The centralized DropShadowEffect in Effects/Shadows.xaml avoids inline shadow definitions and enables consistent shadow rendering across all popups. No clipping containers means the popup uses a flat structure inside a Border with shadow effect — no nested Grid/DropShadowEffect combinations.

**Alternatives considered**:
- AllowsTransparency=True with proper z-order management: Creates Excel hosting compatibility issues.
- Border-based popup overlay: Not suitable for ComboBox which needs to overlay all content including Excel.
- Custom popup implementation: Overkill; WPF Popup works with proper configuration.

---

### RT-005: Custom Theme Accent Color Persistence

**Decision**: Both theme selection and accent color stored in Properties.Settings.Default (ApplicationSettingsBase). Separate fields: SelectedTheme (string) and AccentColor (string hex).

**Rationale**: .NET Framework 4.8's Properties.Settings.Default provides a straightforward, built-in persistence mechanism that works in VSTO environments. Storing two separate fields (SelectedTheme and AccentColor) allows independent persistence while keeping the persistence mechanism simple. The ThemeManager reads both on startup and applies them in sequence (theme base first, then accent color override).

**Alternatives considered**:
- JSON config file: Adds file I/O dependency and parsing overhead.
- Registry: Overkill for theme preferences; requires elevation on some systems.
- Single combined field (e.g., "Custom:Teal"): Harder to parse and extend if accent format changes.

---

### RT-006: Animation Architecture — Centralized Library

**Decision**: Centralized Animations.xaml with Storyboard definitions for control state transitions (hover, focus, press) and popup open/close animations. All animations ≤ 200ms with CubicEase EaseOut.

**Rationale**: Centralized animations in a ResourceDictionary allow reuse across all control templates while keeping control styles clean. Storyboards with easing functions provide smooth, professional-looking transitions. The ≤ 200ms budget ensures snappy responsiveness critical for VSTO environments. The scope is intentionally limited to control state + popup animations — page transitions are out of scope per clarification.

**Alternatives considered**:
- Per-control inline Storyboards: Duplication across control styles, harder to maintain.
- No centralized animations (default WPF): Works but lacks the polished Fluent feel.
- Third-party animation library: Violates the no-third-party-UI-frameworks constraint.

---

### RT-007: DataGrid Virtualization for Performance

**Decision**: Enable EnableRowVirtualization="True" and EnableColumnVirtualization="True" on DataGrid. No animations on DataGrid rows per Performance Budget Rules.

**Rationale**: DataGrid with 500+ rows in a VSTO add-in requires virtualization to maintain smooth scrolling. WPF's built-in virtualization handles row recycling efficiently. Disabling row animations avoids the performance cost of animating potentially hundreds of row elements simultaneously. The existing DataGridStyles.xaml already exists and can be enhanced with virtualization settings.

**Alternatives considered**:
- Custom virtualization logic: Unnecessary; WPF built-in virtualization is sufficient.
- No virtualization with performance monitoring: Would cause frame drops on large datasets.
- deferred scrolling mode: May frustrate users expecting immediate scroll feedback.