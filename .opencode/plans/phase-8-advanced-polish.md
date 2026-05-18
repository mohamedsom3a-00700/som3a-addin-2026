# Phase 8: Advanced Polish

**Branch**: `feature/advanced-polish`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Final polish phase for Fluent Design perfection: acrylic effects, glow system, keyboard navigation, and accessibility improvements. This phase is the **final validation gate** — all Theme Validation Checklist items must pass before the project is considered complete.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**: Various

---

## Tasks

### T033 Add acrylic-inspired depth effects
**Files**: `WpfApp2/Theme/Fluent/FluentEffects.xaml` or new effects file

Create pseudo-acrylic backgrounds for depth:
```xaml
<!-- Semi-transparent surface for title bars, sidebars, overlays -->
<SolidColorBrush x:Key="AcrylicSurface" Color="#E0102030"/>
<SolidColorBrush x:Key="AcrylicSurfaceLight" Color="#E0FFFFFF"/>

<!-- Blur placeholder (WPF doesn't support true blur, use opacity) -->
<SolidColorBrush x:Key="BlurSurface" Color="#80000000"/>
```

Apply to:
- Title bars
- Sidebars
- Overlays
- Popup backgrounds (optional, per Popup Architecture Rules — avoid AllowsTransparency)

### T034 Enhance glow system for selection
**File**: `WpfApp2/Theme/Effects/Glow.xaml`

Add selection-specific glows for:
- Selected list items
- Active tabs
- Focused inputs
- Toggle states
- Theme card selected state (already in Glow.xaml from Phase 2)

```xaml
<!-- Enhanced Selection Glow -->
<DropShadowEffect x:Key="Glow.ListItem.Selected"
                  Color="#3A86FF" BlurRadius="8"
                  ShadowDepth="0" Opacity="0.5"/>

<!-- Active Tab Glow -->
<DropShadowEffect x:Key="Glow.Tab.Active"
                  Color="#3A86FF" BlurRadius="6"
                  ShadowDepth="0" Opacity="0.4"/>

<!-- Focused Input Glow -->
<DropShadowEffect x:Key="Glow.Input.Focused"
                  Color="#3A86FF" BlurRadius="10"
                  ShadowDepth="0" Opacity="0.35"/>
```

### T035 Keyboard navigation audit
**Files**: All control styles

Ensure:
- Tab order logical across all windows
- Arrow keys work in lists (ListView, DataGrid, ComboBox)
- Enter/Space activates buttons
- Escape closes popups and dialogs
- Focus indicators visible on all interactive elements
- Skip-to-content support for main windows

Audit checklist per control:
```
[ ] Button — Tab, Enter/Space, visible focus ring
[ ] ComboBox — Tab, Arrow keys (open/close/navigate), Escape
[ ] TextBox — Tab, visible focus ring
[ ] CheckBox — Tab, Space (toggle), visible focus ring
[ ] RadioButton — Tab, Arrow keys (group navigation), Space, visible focus ring
[ ] ToggleButton — Tab, Space (toggle), visible focus ring
[ ] DataGrid — Arrow keys (cell nav), Enter (edit), Tab (cell nav)
[ ] ListView — Arrow keys (item nav), Enter (activate), visible selection
[ ] SettingsWindow theme cards — Tab (card to card), Enter/Space (select)
```

### T036 Accessibility improvements
**Files**: Base tokens, all controls

Improvements per WCAG 2.1:
- Contrast ratio ≥ 4.5:1 for text (verify with theme validation checklist)
- Focus indicators meet WCAG 2.1 (2px minimum, visible against all backgrounds)
- Screen reader support via `AutomationProperties`
- Reduced motion support (check system animation toggle — future consideration)

```csharp
// Accessibility support example
<Button AutomationProperties.Name="Minimize window"
         AutomationProperties.HelpText="Click to minimize the window"/>
```

---

## Theme Validation Checklist — Final Gate

Every theme (Dark, Light, Custom) MUST pass all 8 checks:

```
[ ] Contrast ratio: Text on background ≥ 4.5:1 (WCAG AA)
[ ] DataGrid readability: Row text, header text, selection visible
[ ] Disabled states: Clearly dimmed, not invisible
[ ] Hover visibility: Interactive elements show hover state
[ ] Focus visibility: Keyboard focus ring clearly visible
[ ] Popup readability: ComboBox, Tooltip, Popup content readable
[ ] Accessibility: No text disappears on any background combination
[ ] DPI readability: All text and UI elements scale correctly at 100%, 125%, 150%, 200%
```

**This checklist MUST be 100% complete before Phase 8 is marked done.**

---

## Visual Audit Checklist

Before Phase 8 completion, verify:
- [ ] Title bar is polished (gradient, subtle depth)
- [ ] Sidebar has proper depth (subtle shadow or acrylic effect)
- [ ] Cards have consistent elevation (shadow hierarchy)
- [ ] Buttons have smooth hover/focus transitions
- [ ] Theme card selection is visually striking
- [ ] Settings window feels like a premium Fluent app
- [ ] All windows feel cohesive as a suite
- [ ] Dark/Light/Custom themes are visually consistent in quality
- [ ] No jarring visual inconsistencies across themes

---

## Dependency Order

T033, T034, T035, T036 (can be parallel — all polish items)

---

## Acceptance Criteria

- [ ] Acrylic-like depth achieved on surfaces (title bar, sidebar, overlays)
- [ ] Selection glows consistent (list items, tabs, inputs, toggles, theme cards)
- [ ] Full keyboard navigation working (Tab order, Arrow keys, Enter/Space, Escape)
- [ ] Accessibility compliance improved (contrast, focus indicators, AutomationProperties)
- [ ] All polish items feel cohesive — premium enterprise software look
- [ ] All themes pass Theme Validation Checklist (100%)
- [ ] Visual audit complete — no inconsistencies

---

## Constitution Check

Per constitution:
- **Principle V (Feature Completeness)**: Every UI feature theme-aware, runtime switchable, keyboard accessible, DPI safe ✅
- **Theme System Architecture**: Premium enterprise software, Modern Windows Fluent application ✅
- **VisualStateManager Strategy**: All controls have required states (Normal, Hover, Pressed, Focused, Disabled, Selected, PopupOpen) ✅
- **Design Authority Rules**: No architecture changes made without approval ✅
- **Final Architecture Goal**: The UI system feels like premium enterprise software ✅

(End of file — total 170 lines)