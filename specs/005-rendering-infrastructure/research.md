# Research Findings: Rendering & Window Infrastructure

**Date**: 2026-05-22

## Render Mode Detection Strategy

- **Decision**: Use existing WindowRenderModeDetector as the detection backend; wrap in RenderModeService for caching and event exposure
- **Rationale**: WindowRenderModeDetector already has VSTO detection, DPI detection, and render test — reuse avoids duplication
- **Alternatives considered**:
  - Building new detection from scratch (rejected: existing code works and is tested)
  - Using .NET `System.Windows.Media.RenderCapability` directly (rejected: lacks VSTO-specific heuristics)

## FallbackSafe Mode Behavior

- **Decision**: Default to FallbackSafe on any RenderModeService initialization failure
- **Rationale**: Safety-first approach — degraded visuals are acceptable, broken rendering (black windows) is not
- **Alternatives considered**:
  - Default to WindowChrome and retry (rejected: risk of black windows on first render)
  - Block window opening until user acknowledges (rejected: poor UX, blocks workflow)

## Render Mode Lifecycle

- **Decision**: Determine once at app startup, fixed for session
- **Rationale**: GPU capabilities and transparency support do not change without Excel restart in VSTO hosting. RenderModeChanged event reserved for diagnostics only
- **Alternatives considered**:
  - Reactive detection on every window open (rejected: unnecessary overhead, no real benefit for VSTO)
  - Listen for system monitor/DPI change events (rejected: complexity without real need)

## Safe Shadow Variants

- **Decision**: Safe variants use 50% blur radius and 75% opacity of full variants
- **Rationale**: Reduces GPU rendering load while maintaining visual structure; consistent with existing architecture patterns
- **Alternatives considered**:
  - Removing shadows entirely in safe mode (rejected: degrades visual hierarchy too much)
  - Same blur/opacity as full (rejected: contradicts safe mode purpose)

## Existing Architecture Audit (Phase 2 Scope)

| Component | Status | Action |
|-----------|--------|--------|
| ModernWindow.cs | Functional, 4 new DPs needed | Extend |
| WindowRenderModeDetector.cs | VSTO detection, DPI detection, render test | Wrap in RenderModeService |
| ThemeManager.cs | Singleton, debounce, accent, persistence | Audit only — no changes |
| DpiHelper.cs | Needs review and extension | Extend/Create |
| Shadows.xaml | 7 shadow effects + 1 progress glow | Add safe variants |
| WindowAnimations.xaml | Exists, needs standardization | Standardize durations + safe gating |
| 14 window XAML files | Various states of compliance | Refactor per FR-006 |
