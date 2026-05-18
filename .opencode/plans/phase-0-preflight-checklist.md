# Phase 0: Pre-Flight Architecture Validation

**Branch**: `feature/architecture-foundations`
**Date**: 2026-05-18
**Status**: Pre-Implementation Validation

---

## Summary

Before any UI phase begins, the architecture foundation must be validated. This phase is a checklist — if any item fails, the subsequent phases cannot start.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0

---

## Pre-Flight Checklist

All items below MUST be `true` before Phase 1 begins.

### Foundation

- [ ] `Theme/Base/Colors.xaml` exists and contains primitive + semantic token layers
- [ ] `Theme/Base/Typography.xaml` exists
- [ ] `Theme/Base/Spacing.xaml` exists
- [ ] `Theme/Base/Radius.xaml` exists
- [ ] `Theme/ThemeResources.xaml` acts as aggregator using `MergedDictionaries`
- [ ] `Theme/Controls/` contains at minimum ButtonStyles, ComboBoxStyles, TextBoxStyles, DataGridStyles
- [ ] `Theme/Effects/` folder exists (or will be created in Phase 2)

### Token Architecture

- [ ] Primitive tokens defined: `Blue500`, `Slate900`, `Slate800`, `Slate700`, `WhiteAlpha95`, `WhiteAlpha75`, `WhiteAlpha40`, `WhiteAlpha20`, `WhiteAlpha10`, `SuccessGreen`, `WarningOrange`, `DangerRed`, `InfoBlue`
- [ ] Semantic tokens defined: `Brush.Background.*`, `Brush.Text.*`, `Brush.Accent.*`, `Brush.Stroke.*`, `Brush.Control.*`
- [ ] Legacy flat keys retained: `AccentBrush`, `BackgroundBrush`, `CardBrush`, `SurfaceBrush`, `TextMainBrush`, `TextSubBrush`, `TextDisabledBrush`, `CardStrokeBrush`, `ControlBgBrush`, `ControlStrokeBrush`
- [ ] No hardcoded inline color values in any control template

### Popup Architecture

- [ ] ComboBox popup uses `Placement="Bottom"`
- [ ] ComboBox popup does NOT use `AllowsTransparency="True"` unless explicitly required
- [ ] All popup shadows reference centralized `Effects/Shadows.xaml` (no inline DropShadowEffect)
- [ ] Popup `PlacementTarget` correctly bound to parent control

### Window Architecture

- [ ] `ModernWindow` base class exists
- [ ] Windows use `WindowChrome` (not `WindowStyle="None"` + `AllowsTransparency="True"`)
- [ ] Fallback-safe mode documented for Excel hosting edge cases
- [ ] DPI awareness enabled (`SnapsToDevicePixels="True"`, `UseLayoutRounding="True"`)

### Performance Budget

- [ ] No nested `DropShadowEffect` definitions in any existing file
- [ ] No `BlurEffect` on scrollable containers
- [ ] DataGrid has `EnableRowVirtualization="True"` where applicable
- [ ] Large lists (50+ items) use virtualization

### Resource Loading Order

- [ ] `ThemeResources.xaml` documents the correct loading order:
  ```
  1. Primitive Tokens
  2. Semantic Tokens
  3. Typography
  4. Radius
  5. Effects (Shadows, Glow, Animations)
  6. Controls
  7. Theme Overrides (Dark/Light/Custom)
  8. Window Styles
  ```

### Governance

- [ ] Constitution v1.1.0 updated with all 12 architecture corrections
- [ ] Design Authority rules in constitution (OpenCode restrictions)
- [ ] Incremental Migration Rules documented
- [ ] Theme Validation Checklist in constitution
- [ ] Performance Budget Rules in constitution
- [ ] WindowChrome Enforcement in constitution
- [ ] VisualStateManager Strategy in constitution
- [ ] All PRs will be reviewed against these rules

---

## Dependency Order

No dependencies — this is a pure checklist phase.

---

## Acceptance Criteria

- [ ] All checklist items pass
- [ ] No blocking issues identified
- [ ] Architecture foundation is solid before Phase 1 begins

---

## Notes

If any checklist item fails, create a blocking issue with:
1. The failing item
2. Current state
3. Required fix
4. Owner assignment

Phase 1 MUST NOT start until Phase 0 is 100% complete.

(End of file - total 97 lines)