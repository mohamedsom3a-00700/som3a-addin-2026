# Research: Legacy Window Migration

**Phase**: 0 — Research
**Date**: 2026-05-25
**Feature**: `/specs/011-legacy-window-migration/spec.md`

## Window Inventory

All 14 windows enumerated from `WpfApp2/` root and `WpfApp2/Windows/PrimaveraComparison/`:

| # | Window | Location | Complexity | Priority Rationale |
|---|--------|----------|------------|-------------------|
| 1 | MainWindow | Root | High | Primary entry; ribbon-driven project analysis |
| 2 | SettingsWindow | Views/ | Medium | High-usage settings UI; already has theme cards |
| 3 | Float_path | Root | Medium | Trade code float window; moderate usage |
| 4 | AssignTradeCodesWindow | Root | Medium | Assignment dialog; moderate usage |
| 5 | Fixpiecolors | Root | Low | Utility dialog; low usage |
| 6 | LinksManagerWindow | Root | Medium | Links management; moderate usage |
| 7 | StyleSelectorWindow | Root | Low | Style picker; low usage |
| 8 | SubDailyReportWindow | Root | Medium | Report generation; moderate usage |
| 9 | UnmergeFillDownWindow | Root | Low | Data operation dialog; low usage |
| 10 | XerEditorWindow | Root | High | XER file editing; high complexity |
| 11 | ProjectAnalysisWindow | Root | High | Analysis grid; high complexity, high usage |
| 12 | PrimaveraCompareWindow | Windows/PrimaveraComparison/ | High | Comparison view; moderate complexity |
| 13 | PrimaveraResultsWindow | Windows/PrimaveraComparison/ | High | Results display; moderate complexity |
| 14 | ToastWindow | Controls/Toast/ | Low | Notification popup; already in Controls/ |

## Decision: Migration Priority Order

### Tier 1 — High Priority (Migrate First)
1. **SettingsWindow** (Views/) — Already uses theme cards, moderate complexity, high user visibility
2. **MainWindow** (root) — Primary entry; complex but critical; MVP for shell validation

### Tier 2 — Medium Priority
3. **ProjectAnalysisWindow** — High usage, data grid, complex interactions
4. **Float_path** — Moderate complexity, ribbon-accessed
5. **LinksManagerWindow** — Moderate complexity
6. **SubDailyReportWindow** — Moderate complexity

### Tier 3 — Lower Priority
7. **AssignTradeCodesWindow** — Dialog complexity
8. **PrimaveraCompareWindow** — Comparison logic
9. **PrimaveraResultsWindow** — Results display
10. **XerEditorWindow** — High complexity editor

### Tier 4 — Can Remain Standalone or Migrate Last
11. **Fixpiecolors** — Utility, low usage
12. **StyleSelectorWindow** — Low usage
13. **UnmergeFillDownWindow** — Low usage
14. **ToastWindow** — Already a popup-notification pattern; may not fit shell model

## Decision: PageBase Pattern

Existing `PageBase.cs` from Phase 8 provides:
- `OnNavigatedTo()` override for page activation
- `OnNavigatedFrom()` override for page deactivation
- Theme refresh via `ThemeManager` subscription

**Finding**: `PageBase` is sufficient for all 14 windows. No changes needed.

## Decision: Ribbon Integration

Ribbon XML or Ribbon designer code currently does:
```csharp
var window = new TargetWindow();
window.Show();
```

Must change to:
```csharp
NavigationService.Instance.NavigateTo("target-page-key");
```

**Finding**: Ribbon integration is a find-and-replace operation per button callback. No new infrastructure needed.

## Decision: Code-Behind Extraction

Windows contain mixed code-behind:
- UI setup (`InitializeComponent`) — stays in Page
- Event handlers — move to Page code-behind or ViewModel
- Service calls — stay in existing services (Phase 6 DI)
- Window-specific state — some may need ViewModel

**Finding**: Phase 6 MVVM infrastructure (ServiceContainer, EventBus) is available for dependency injection into Page constructors.

## Decision: Validation Strategy

Per-window validation checklist:

1. Page opens in shell without black backgrounds
2. All controls render correctly at 100%, 125%, 150% DPI
3. Theme switching updates Page correctly
4. Popup controls (ComboBox, etc.) render without clipping
5. Keyboard navigation (Tab, Enter, Escape) works
6. FallbackSafe mode renders correctly (no transparency)
7. Functional parity — all operations produce same results as standalone window

**Finding**: Validation is manual per window. Automated screenshot comparison could be added in Phase 10 (Diagnostics) but is not required for Phase 11.

## Alternatives Considered

### Alternative A: Migrate all windows simultaneously
- **Rejected because**: High risk of breaking all windows at once; no rollback capability per window

### Alternative B: Create new Shell-first windows, deprecate old ones
- **Rejected because**: Phase 5 scope restriction explicitly says existing windows remain standalone until Phase 11

### Alternative C: Migrate only the most-used 5 windows, leave others
- **Rejected because**: Spec requirement SC-006 specifies all 14 windows migrated within Phase 11 scope

## Summary

Migration approach is **incremental Tier-based**:
1. SettingsWindow and MainWindow first (high visibility, validation learning)
2. Medium complexity windows second
3. High complexity or low usage windows third
4. Original XAML preserved until each Page validates
5. Ribbon integration via NavigationService
6. Manual validation per window with documented checklist