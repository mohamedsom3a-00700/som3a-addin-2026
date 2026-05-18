# Implementation Plans Index

**Project**: Som3a Add-in 2026 - UI Foundation & Theme Engine
**Date**: 2026-05-18
**Status**: All phases saved for implementation

---

## Execution Order

| # | Phase | Plan File | Priority |
|---|-------|-----------|----------|
| 1 | ComboBox Refactor | `phase-1-combobox-refactor.md` | **HIGH** |
| 2 | Shadow System | `phase-2-shadow-system.md` | Medium |
| 3 | Theme Engine Architecture | `phase-3-theme-engine.md` | Medium |
| 4 | Settings UX Rebuild | `phase-4-settings-ux.md` | Medium |
| 5 | Control Standardization | `phase-5-control-standardization.md` | Medium |
| 6 | Animation & Motion | `phase-6-animation-system.md` | Medium |
| 7 | Runtime Theme Manager | `phase-7-runtime-theme-manager.md` | **HIGH** |
| 8 | Advanced Polish | `phase-8-advanced-polish.md` | Low |

---

## Phase Summary

### Phase 1: ComboBox Refactor
Fix ComboBox popup rendering, add all states, DynamicResource-only.

### Phase 2: Shadow System
Centralize shadows (Window, Popup, Card, Small, Medium, Large) in Effects/ folder.

### Phase 3: Theme Engine Architecture
Create Dark/, Light/, Custom/ theme folders with full color palettes.

### Phase 4: Settings UX Rebuild
Replace theme dropdown with visual theme cards.

### Phase 5: Control Standardization
Create CheckBox, RadioButton, ToggleButton, improve TextBox, DataGrid.

### Phase 6: Animation & Motion
Build comprehensive animation library with easing functions.

### Phase 7: Runtime Theme Manager
Implement Services/ThemeManager.cs for dynamic theme switching.

### Phase 8: Advanced Polish
Acrylic effects, glow system, keyboard navigation, accessibility.

---

## Dependency Notes

- Phase 1 (T001) creates `Theme/Effects/Shadows.xaml` required by Phase 2
- Phase 2 provides shadow/glow infrastructure for all later phases
- Phase 3 creates theme structure required by Phase 4 and 7
- Phase 7 (ThemeManager) requires Phase 3 to be complete
- Phase 8 should be last (final polish after all features)

---

## Suggested Implementation Branch Strategy

```bash
# Main feature branch
git checkout -b feature/theme-engine-v2

# Per-phase branches (merge as complete)
feature/combobox-fix        # Phase 1
feature/shadow-system      # Phase 2
feature/theme-engine       # Phase 3
feature/settings-ux        # Phase 4
feature/control-standard   # Phase 5
feature/animation-system   # Phase 6
feature/runtime-theme       # Phase 7
feature/advanced-polish    # Phase 8

# Or: Single branch, sequential implementation
# Merge to main after Phase 7 (MVP complete)
```

---

## Total Tasks: 36

- Phase 1: 6 tasks
- Phase 2: 4 tasks
- Phase 3: 4 tasks
- Phase 4: 4 tasks
- Phase 5: 6 tasks
- Phase 6: 4 tasks
- Phase 7: 4 tasks
- Phase 8: 4 tasks