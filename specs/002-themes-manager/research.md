# Phase 0 Research: Update Themes Manager

**Feature**: 002-themes-manager  
**Date**: 2026-05-19  
**Status**: Complete — no unresolved unknowns

---

## Research Tasks

| # | Unknown | Resolution | Source |
|---|---------|------------|--------|
| 1 | `FluentEffects.xaml` fate: update or remove? | **Remove entirely.** Keys are unreferenced; centralized effects already exist in `Effects/Shadows.xaml` and `Effects/Glow.xaml`. | Code audit via grep |
| 2 | Accent persistence on theme switch | **Preserve existing accent.** Spec is authoritative; `_currentAccentColor` is an independent user preference. | Spec FR-001, Clarification Session |
| 3 | Theme load failure behavior | **Preserve current theme on new-theme load failure.** Only fallback to Dark if current dictionary is invalidated. | Spec FR-003, Clarification Session |
| 4 | Rapid theme switching UX | **Debounce/coalesce (≈150ms).** Only the final selection is applied to prevent transient states and race conditions. | Spec US-3, Clarification Session |
| 5 | Token naming convention for new brushes | **Semantic `Brush.*` namespace** (e.g., `Brush.Background.Root`). Aligns with `Theme/Base/Colors.xaml`. Legacy flat keys retained for backward compatibility. | Spec FR-000, Clarification Session |

---

## Technology Decisions

### Decision: No third-party theme libraries
- **Rationale**: Constitution §XIV prohibits third-party UI frameworks. The existing hand-rolled ResourceDictionary system is sufficient and already functional.
- **Alternatives considered**: None (constitutionally prohibited).

### Decision: Continue using `Properties.Settings.settings` for persistence
- **Rationale**: Already in use by `ThemeManager.cs`. JSON fallback (`ThemeSettings.cs`) exists but is legacy; no migration needed per spec assumption.
- **Alternatives considered**: Migrate to JSON-only — rejected because it would require migration logic and the existing `.settings` approach works.

### Decision: Keep per-window `MergedDictionaries` loading
- **Rationale**: The spec explicitly scopes this feature to not address global `ThemeResources.xaml` loading, which is an architectural concern for a future feature.
- **Alternatives considered**: Global App-level dictionary loading — deferred.

### Decision: Use `System.Windows.Application.Current.Dispatcher.InvokeAsync` for thread safety
- **Rationale**: Standard WPF pattern for marshaling to the UI thread. No external dependencies required.
- **Alternatives considered**: `Dispatcher.BeginInvoke` — `InvokeAsync` is preferred in modern .NET Framework 4.8 for async-friendly dispatch.

---

## Risk Notes

1. **Orphaned file removal**: Deleting `FluentEffects.xaml` and `FluentWhite.xaml` from the project requires updating `.csproj` to avoid build errors. Verified: both files are explicitly listed in `Som3a_WPF_UI.csproj`.
2. **12-window migration scope**: Touching 12 XAML files creates regression risk. The plan mitigates this with a systematic grep-based audit (SC-003).
3. **VSTO rendering**: All window background changes must respect `WindowRenderModeDetector`. The root background brush change is a static `DynamicResource` swap, which does not affect rendering mode detection.

---

**Conclusion**: All unknowns are resolved. The implementation can proceed with confidence.
