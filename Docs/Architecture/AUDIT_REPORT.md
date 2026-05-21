# Architecture Audit Report

**Project**: Som3a Add-in 2026  
**Date**: 2026-05-21  
**Auditor**: OpenCode AI Agent  
**Phase**: Phase 0 — Governance Foundation  
**Scope**: Theme architecture, ThemeManager, popup system, shadow system  

---

## 1. Theme Architecture (P0-T001)

### Existing Token Files

| File | Status | Notes |
|------|--------|-------|
| `Theme/Base/Colors.xaml` | EXISTS | Has primitive + semantic layers. Needs cleanup. |
| `Theme/Base/Typography.xaml` | EXISTS | Basic font sizes and weights. Needs style definitions. |
| `Theme/Base/Spacing.xaml` | EXISTS | Padding and size constants. Needs expansion. |
| `Theme/Base/Radius.xaml` | EXISTS | Corner radius values. Good coverage. |
| `Theme/Base/Elevation.xaml` | MISSING | Needed for Phase 1. |
| `Theme/Base/Motion.xaml` | MISSING | Needed for Phase 1. |
| `Theme/Base/ZIndex.xaml` | MISSING | Needed for Phase 1. |
| `Theme/Base/Opacity.xaml` | MISSING | Needed for Phase 1. |
| `Theme/Base/ComponentTokens.xaml` | MISSING | Needed for Phase 1. |

### Inline Color Violations

| File | Line | Violation | Recommended Token |
|------|------|-----------|-------------------|
| `ButtonStyles.xaml` | ~10 | `Background="#2FFFFFFF"` | `Brush.Button.Background` |
| `ButtonStyles.xaml` | ~77 | `Background="#3FFFFFFF"` (hover) | `Brush.Button.HoverBackground` |
| `ButtonStyles.xaml` | ~108 | `Background="#22FFFFFF"` (pressed) | `Brush.Button.PressedBackground` |
| `DataGridStyles.xaml` | ~10 | `AlternatingRowBackground="#0AFFFFFF"` | `Brush.DataGrid.AlternatingRow` |
| `DataGridStyles.xaml` | ~41 | `Background="#15FFFFFF"` (hover) | `Brush.DataGrid.HoverRow` |
| `DataGridStyles.xaml` | ~44 | `Background="#333A86FF"` (selected) | `Brush.DataGrid.SelectedRow` |
| `MainWindow.xaml` | ~317 | `Background="#22000000"` | `Brush.Progress.Background` |

### Hardcoded Geometry Violations

| File | Line | Violation | Recommended Token |
|------|------|-----------|-------------------|
| `ComboBoxStyles.xaml` | ~57 | `CornerRadius="10"` | `{DynamicResource MediumRadius}` |
| `MainWindow.xaml` | multiple | Hardcoded margins (`Margin="12"`, `Height="42"`, etc.) | Use spacing tokens where applicable |
| `ModernWindow.xaml` | multiple | Hardcoded sizes (`Width="36"`, `Height="32"`, `Padding="12,0"`) | Use spacing tokens where applicable |

### Duplicate Colors Detected

| Hex Value | Occurrences | Locations |
|-----------|-------------|-----------|
| `#0E1720` | 4+ | Colors.xaml, DarkColors.xaml, CustomColors.xaml, MainWindow.xaml |
| `#33FFFFFF` | 3+ | Multiple stroke/fill tokens |
| `#1C2B3A` | 3+ | Colors.xaml, CustomColors.xaml, MainWindow.xaml |

### Resource Loading Order

Current order in `ThemeResources.xaml`:
1. Base/Colors.xaml
2. Base/Typography.xaml
3. Base/Spacing.xaml
4. Base/Radius.xaml
5. Effects/Shadows.xaml
6. Effects/Glow.xaml
7. Effects/Animations.xaml
8. Controls/... (all)
9. ModernWindow.xaml
10. WindowAnimations.xaml

**Gap**: Missing Elevation, Motion, ZIndex, Opacity, ComponentTokens.

---

## 2. ThemeManager Review (P0-T002)

### Current State

| Aspect | Status | Details |
|--------|--------|---------|
| Singleton pattern | ✅ | Double-checked locking with `_lock` |
| Debounce timer | ✅ | 150ms delay for theme switching |
| Accent application | ✅ | Updates `AccentColor`, `AccentBrush`, glow effects |
| Persistence | ✅ | Uses `Properties.Settings.Default` |
| Events | ✅ | `ThemeChanged` event with args |
| Theme enumeration | ✅ | `AppTheme.Dark`, `Light`, `Custom` |
| Dictionary replacement | ✅ | Safe remove/add with fallback |
| Runtime accent | ✅ | `ApplyAccentColor()` generates light variant |

### Gaps Identified

| Gap | Impact | Phase Addressed |
|-----|--------|-----------------|
| No accent variant generation (hover/pressed/glow/border/subtle) | Limits runtime accent engine | P3 |
| No runtime resource validation | Could miss missing tokens | P8 |
| No fallback recovery beyond try/catch | Risk of silent failures | P8 |
| `ThemeSettings.cs` is stub | No JSON serialization | P3 |

---

## 3. Popup System Review (P0-T003)

### ComboBoxStyles.xaml Assessment

| Requirement | Status | Detail |
|-------------|--------|--------|
| `AllowsTransparency="False"` | ✅ | Compliant |
| `Placement="Bottom"` | ✅ | Compliant |
| `PlacementTarget` bound | ✅ | Compliant |
| Centralized shadow | ✅ | `Effect="{DynamicResource Shadow.Popup}"` |
| `MaxHeight` on popup | ✅ | `MaxHeight="250"` |
| `SnapsToDevicePixels` | ✅ | Set on root Grid |
| Keyboard navigation | ⚠️ | Basic; needs full audit |

### Gaps

- `ComboBoxItemStyles.xaml` needs full review
- No popup diagnostics or clipping detection
- No width-synchronization between ComboBox and popup

---

## 4. Shadow System Review (P0-T004)

### Shadows.xaml Assessment

| Effect | Key | Blur | Depth | Opacity | Status |
|--------|-----|-----|-------|---------|--------|
| Window | `Shadow.Window` | 30 | 8 | 0.4 | ✅ |
| Popup | `Shadow.Popup` | 15 | 3 | 0.3 | ✅ |
| Popup Small | `Shadow.Popup.Small` | 12 | 2 | 0.25 | ✅ |
| Card | `Shadow.Card` | 16 | 4 | 0.25 | ✅ |
| Small | `Shadow.Small` | 8 | 2 | 0.2 | ✅ |
| Medium | `Shadow.Medium` | 12 | 3 | 0.25 | ✅ |
| Large | `Shadow.Large` | 20 | 5 | 0.3 | ✅ |
| Progress Glow | `ProgressGlow` | 8 | 0 | 0.8 | ✅ |

### Glow.xaml Assessment

| Effect | Key | Blur | Opacity | Dynamic Color | Status |
|--------|-----|------|---------|---------------|--------|
| Focus | `Glow.Focus` | 10 | 0.35 | `AccentColorValue` | ✅ |
| Button Hover | `Glow.ButtonHover` | 8 | 0.3 | `AccentColorValue` | ✅ |
| Primary | `Glow.Primary` | 12 | 0.4 | `AccentColorValue` | ✅ |
| Selection | `Glow.Selection` | 6 | 0.5 | `AccentColorValue` | ✅ |
| Accent | `Glow.Accent` | 8 | 0.4 | `AccentColorValue` | ✅ |
| ThemeCard Selected | `Glow.ThemeCard.Selected` | 14 | 0.45 | `AccentColorValue` | ✅ |

### Gaps

- No safe-mode shadow variants (lower blur/opacity for FallbackSafe)
- No elevation token layer linking shadows to semantic elevation levels
- No inline effects detected in reviewed files ✅

---

## Summary

| Category | Issues Found | Priority |
|----------|-------------|----------|
| Inline colors | 7 instances | High |
| Hardcoded geometry | 3+ instances | Medium |
| Duplicate colors | 3+ hex values | Medium |
| Missing token files | 5 files | High |
| Missing safe-mode shadows | 3 variants | Medium |
| ThemeManager gaps | 4 gaps | Medium |
| Popup gaps | 3 gaps | Medium |

---

**Next Steps:**
1. Create missing token files (Phase 1)
2. Replace inline colors with semantic tokens (Phase 1)
3. Add safe-mode shadow variants (Phase 2)
4. Extend ThemeManager with accent generation (Phase 3)
