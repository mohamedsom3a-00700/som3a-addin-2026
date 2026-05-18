# Implementation Plans Index

**Project**: Som3a Add-in 2026 - UI Foundation & Theme Engine
**Date**: 2026-05-18
**Status**: All phases updated with Architecture Revisions (v1.1.0)
**Governed by**: `.specify/memory/constitution.md` v1.1.0

---

## Execution Order

| # | Phase | Plan File | Priority |
|---|-------|-----------|----------|
| 0 | Pre-Flight Architecture Validation | `phase-0-preflight-checklist.md` | **BLOCKING** |
| 1 | ComboBox Refactor | `phase-1-combobox-refactor.md` | **HIGH** |
| 2 | Shadow System | `phase-2-shadow-system.md` | Medium |
| 3 | Theme Engine Architecture | `phase-3-theme-engine.md` | Medium |
| 4 | Settings UX Rebuild | `phase-4-settings-ux.md` | **HIGH** |
| 5 | Control Standardization | `phase-5-control-standardization.md` | Medium |
| 6 | Animation & Motion | `phase-6-animation-system.md` | Medium |
| 7 | Runtime Theme Manager | `phase-7-runtime-theme-manager.md` | **HIGH** |
| 8 | Advanced Polish | `phase-8-advanced-polish.md` | Low |

---

## Phase Summary

### Phase 0: Pre-Flight Architecture Validation
Pre-implementation checklist validating all architecture foundations before Phase 1 begins. This is a **blocking gate** — all items must pass.

### Phase 1: ComboBox Refactor
Fix ComboBox popup rendering, add all states, DynamicResource-only theming. Per Popup Architecture Rules and Incremental Migration Rules.

### Phase 2: Shadow System
Centralize shadows (Window, Popup, Card, Small, Medium, Large) and glow effects (Focus, Selection, Accent) in Effects/ folder. No inline DropShadowEffect allowed.

### Phase 3: Theme Engine Architecture
Create Dark/, Light/, Custom/ theme folders with full semantic token palettes. Establish resource loading order. Theme validation checklist gates.

### Phase 4: Settings UX Rebuild
Replace theme ComboBox with **Theme Cards** (Dark, White, Custom). Modern Fluent settings experience with preview thumbnails, selection glow, and smooth animations. RadioButton/ComboBox selection **DEPRECATED**.

### Phase 5: Control Standardization
Create CheckBox, RadioButton, ToggleButton; improve TextBox, DataGrid; add ScrollViewer styles. Introduce VisualStateManager gradually per VSM Strategy.

### Phase 6: Animation & Motion
Build comprehensive animation library with easing functions. All animations ≤ 200ms. No bouncy effects. No animations on DataGrid rows.

### Phase 7: Runtime Theme Manager
Implement Services/ThemeManager.cs for dynamic theme switching. User preference persistence. All windows update on theme change.

### Phase 8: Advanced Polish
Acrylic depth effects, glow system, keyboard navigation audit, accessibility improvements. **Final validation gate** — all themes must pass Theme Validation Checklist.

---

## Architecture Revisions Applied (v1.1.0)

| Correction | Where Applied |
|-----------|--------------|
| ThemeResources.xaml as aggregator only | Phase 3, Phase 7, Constitution |
| Primitive + Semantic token architecture | Colors.xaml (done), Phase 3, Phase 7 |
| Incremental Migration Rules | All phases, Constitution |
| Theme Cards (not ComboBox/RadioButton) | Phase 4 (T015-T018) |
| Popup Architecture Rules | Phase 1 (T002), Phase 2, Constitution |
| Resource Loading Order (8-step) | Phase 3 (T014), Phase 7, Constitution |
| Theme Validation Checklist (8 gates) | Phase 3, Phase 4, Phase 8, Constitution |
| VisualStateManager Strategy | Phase 5 (T019-T024), Constitution |
| Performance Budget Rules | Phase 2, Phase 5, Phase 6, Constitution |
| Design Authority Rules | Constitution |
| WindowChrome Enforcement | Constitution |
| ComboBox hardcoded color fixes | ComboBoxStyles.xaml (done) |

---

## Dependency Notes

- Phase 0 is **blocking** — must be 100% complete before Phase 1
- Phase 1 (T001) creates `Theme/Effects/Shadows.xaml` referenced by Phase 2
- Phase 2 provides shadow/glow infrastructure for all later phases
- Phase 3 creates theme structure required by Phase 4 and Phase 7
- Phase 7 (ThemeManager) requires Phase 3 to be complete
- Phase 8 should be last (final polish after all features)

---

## Incremental Migration Gates

Every phase transition requires validation:

```
Phase 0 → Phase 1: Architecture foundation verified
Phase 1 → Phase 2: ComboBox validates in Excel, DPI, popup rendering
Phase 2 → Phase 3: Shadow system complete, no inline DropShadowEffect
Phase 3 → Phase 4: All themes pass Theme Validation Checklist
Phase 4 → Phase 5: Theme cards working, ThemeManager integrated
Phase 5 → Phase 6: All controls have required VSM states
Phase 6 → Phase 7: Animation library complete
Phase 7 → Phase 8: Runtime theme switching validated
Phase 8 → DONE: All 8 theme validation checks pass, visual audit complete
```

**DO NOT migrate multiple windows before Excel-host validation succeeds.**

---

## Suggested Branch Strategy

```bash
# Main feature branch
git checkout -b feature/fluent-ui-v2

# Per-phase branches (merge as complete)
feature/preflight-validation      # Phase 0
feature/combobox-fix             # Phase 1
feature/shadow-system            # Phase 2
feature/theme-engine             # Phase 3
feature/settings-ux             # Phase 4
feature/control-standard        # Phase 5
feature/animation-system         # Phase 6
feature/runtime-theme            # Phase 7
feature/advanced-polish          # Phase 8

# Or: Single branch, sequential implementation
# Merge to main after Phase 7 (MVP complete)
```

---

## Total Tasks: 36

- Phase 0: 0 tasks (checklist only)
- Phase 1: 6 tasks (T001-T006)
- Phase 2: 4 tasks (T007-T010)
- Phase 3: 4 tasks (T011-T014)
- Phase 4: 4 tasks (T015-T018)
- Phase 5: 6 tasks (T019-T024)
- Phase 6: 4 tasks (T025-T028)
- Phase 7: 4 tasks (T029-T032)
- Phase 8: 4 tasks (T033-T036)

---

## Key Files to Create

```
NEW FILES:
WpfApp2/Theme/Dark/DarkColors.xaml
WpfApp2/Theme/Dark/DarkTheme.xaml
WpfApp2/Theme/Light/LightColors.xaml
WpfApp2/Theme/Light/LightTheme.xaml
WpfApp2/Theme/Custom/CustomColors.xaml
WpfApp2/Theme/Custom/CustomTheme.xaml
WpfApp2/Theme/Effects/Shadows.xaml
WpfApp2/Theme/Effects/Glow.xaml
WpfApp2/Theme/Effects/Animations.xaml
WpfApp2/Theme/Controls/ThemeCardStyles.xaml
WpfApp2/Theme/Controls/CheckBoxStyles.xaml
WpfApp2/Theme/Controls/RadioButtonStyles.xaml
WpfApp2/Theme/Controls/ToggleButtonStyles.xaml
WpfApp2/Theme/Controls/ScrollViewerStyles.xaml
WpfApp2/Services/ThemeManager.cs
WpfApp2/Properties/Settings.settings
WpfApp2/Properties/Settings.Designer.cs
```

---

## Key Constitution References

- **Principle III (DynamicResource Only)**: Every theme-aware resource uses DynamicResource
- **Principle IV (Runtime Theme Switching)**: Theme switching updates without restart
- **Principle V (Feature Completeness)**: Every UI feature is theme-aware, accessible, DPI safe
- **Principle VI (Performance & Efficiency)**: Reused brushes, virtualized lists, no nested DropShadows
- **Popup Architecture Rules**: No inline DropShadowEffect, correct Placement/PlacementTarget
- **Resource Loading Order**: 8-step explicit merge order
- **Primitive & Semantic Token Architecture**: Two-tier token separation
- **VisualStateManager Strategy**: Gradual VSM migration, one control at a time
- **Incremental Migration Rules**: Excel host validation gate before advancing
- **Performance Budget Rules**: ≤200ms animations, no BlurEffect on scrollable, no nested DropShadows
- **WindowChrome Enforcement**: WindowChrome preferred, AllowsTransparency fallback
- **Design Authority Rules**: OpenCode restrictions (10 binding rules)
- **Theme Validation Checklist**: 8-gate validation before each theme is considered complete

(End of file — total 194 lines)