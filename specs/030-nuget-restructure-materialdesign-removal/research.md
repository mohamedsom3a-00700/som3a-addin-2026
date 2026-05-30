# Research: NuGet Restructure & MaterialDesign Removal

**Date**: 2026-05-30
**Feature**: NuGet Restructure & MaterialDesign Removal (Phase 1C)

## Decisions

### Decision 1: WPF-UI vs. Custom-Only Controls
- **Decision**: Add WPF-UI (Wpf.Ui) package alongside FluentIcons.WPF. Pilot on a single low-risk page (DiagnosticsPage) in Phase 3 before widespread adoption. If incompatible with the custom theme engine, fallback to FluentIcons.WPF only + custom controls.
- **Rationale**: The master plan requires modern Fluent controls (NavigationView, card layouts, progress bars) for Phase 8 Settings redesign. Rebuilding these from scratch would be significantly more effort than using a well-maintained open-source library.
- **Alternatives considered**: 
  - Custom-only controls: Rejected — too much effort for NavigationView, progress bars, and card layouts.
  - MahApps.Metro: Rejected — explicitly prohibited by Constitution §XIV.
  - MaterialDesign retention: Rejected — Phase 1C goal is explicit removal.

### Decision 2: Zero Visual Changes in Phase 1C
- **Decision**: This phase is strictly dependency removal and compilation fix. No intentional visual changes. Placeholder icons and temporary brush stubs are acceptable.
- **Rationale**: Separating dependency removal from visual migration reduces risk. If a build or runtime failure occurs, the root cause is narrowed to a missing reference rather than a style mismatch.
- **Alternatives considered**:
  - Replace all icons now: Rejected — increases scope and risk; defers to Phase 3 per master plan.

### Decision 3: Build Error Resolution Strategy
- **Decision**: Fix compilation errors by replacing broken references with direct custom theme engine tokens (`Brush.*`, `Color.*`, `Shadow.*`, `Glow.*`). No new styles are created.
- **Rationale**: The custom theme engine already defines all necessary primitives and semantic tokens. Using them as stubs ensures the theme system remains the single source of truth.
- **Alternatives considered**:
  - Add WPF-UI styles as replacements: Rejected — would introduce visual changes prematurely.
  - Hardcode fallback values: Rejected — violates Constitution §III (DynamicResource-only) and §XIV (no hardcoded brushes).

### Decision 4: VSTO Add-in Cleanup Scope
- **Decision**: Remove MaterialDesign package references and binding redirects from the VSTO add-in. Do not remove any other references.
- **Rationale**: The VSTO add-in does not host WPF visual elements from MaterialDesign. Its references are limited to package/binding artifacts carried over from earlier integration attempts.
- **Alternatives considered**:
  - Leave VSTO untouched: Rejected — stale binding redirects can cause runtime resolution errors during interop.

## Known Unknowns

None remaining. All technical decisions were resolved using the master plan, existing theme engine documentation, and constitutional constraints.

## References

- Master Plan: `specs/future-plan-fluent-ui-migration.md` — Phase 1C section
- Constitution: `.specify/memory/constitution.md` — §III, §XIV, §XV, §XVI
- AGENTS.md — Theme engine architecture and build commands
