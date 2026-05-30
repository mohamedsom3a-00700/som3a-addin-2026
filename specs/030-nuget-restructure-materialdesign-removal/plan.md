# Implementation Plan: NuGet Restructure & MaterialDesign Removal

**Branch**: `[fluent/phase-1c]` | **Date**: 2026-05-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/030-nuget-restructure-materialdesign-removal/spec.md`

## Summary

Remove the legacy MaterialDesign UI library (MaterialDesignThemes and MaterialDesignColors) from both the WPF host (WpfApp2) and the VSTO add-in. Install the modern Fluent icon library (FluentIcons.WPF) and the WPF-UI control library (Wpf.Ui) as the new dependency foundation. Clean all theme resource dictionary references, remove assembly binding redirects, and fix all compilation errors by replacing broken references with custom theme engine tokens. This phase produces **zero intentional visual changes** — placeholder icons and temporary brush stubs are acceptable. Full visual migration (icon replacement, control styling) is deferred to Phase 3.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (WPF host), .NET Framework 4.8 (VSTO add-in)

**Primary Dependencies**: WPF-UI (Wpf.Ui), FluentIcons.WPF, CommunityToolkit.Mvvm (already installed)

**Storage**: N/A — no new data persistence in this phase

**Testing**: MSBuild compilation, VSTO smoke test protocol

**Target Platform**: Windows 10/11, Excel VSTO host, out-of-process WPF shell

**Project Type**: Desktop application (WPF + VSTO add-in)

**Performance Goals**: Build completes in under 60 seconds; zero runtime regressions

**Constraints**: Excel VSTO rendering safety; no visual changes; theme engine must remain intact; must pass full smoke test before merge

**Scale/Scope**: Two projects affected (WpfApp2, VSTO add-in); ~20 XAML files and ~10 C# files may need reference fixes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — No new monolithic dictionaries introduced. Resource removals preserve modular isolation.
- [x] **III. DynamicResource-Only** — Removing old resource dictionaries does not affect existing DynamicResource bindings. Custom theme engine remains intact.
- [x] **IV. Runtime Theme Mutation Governance** — ThemeManager is untouched. No new mutation paths introduced.
- [x] **IX. Animation Governance** — No new animations. Existing storyboards remain ≤200ms.
- [x] **X. Excel Rendering Safety** — No new window types. WindowRenderModeDetector remains unchanged.
- [x] **XI. WindowChrome Enforcement** — No new windows. ModernWindow inheritance unaffected.
- [x] **XII. Centralized Effects** — No new inline effects. Effects remain in Effects/Shadows.xaml.
- [x] **XV. Resource Loading Order** — Removal of old dictionaries from ThemeResources.xaml must not disrupt the loading sequence of remaining dictionaries.

### Constitutional Concern Flagged

- **XIV. No Third-Party UI Frameworks**: The master plan explicitly requires adding `WPF-UI` (Wpf.Ui), which is a third-party WPF control library. While the constitution prohibits MahApps.Metro, MaterialDesignInXaml, Telerik, DevExpress, and WinUI wrappers, WPF-UI is not listed. However, it is still a third-party UI framework. **Mitigation**: WPF-UI integration is limited to package availability in this phase. A pilot page (DiagnosticsPage) will validate compatibility with the custom theme engine in Phase 3 before any widespread adoption. If incompatible, the fallback is to use only `FluentIcons.WPF` for icons and retain the custom theme engine, skipping WPF-UI controls entirely.

## Complexity Tracking

> Justification for constitutional tension above.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| WPF-UI third-party control library | Master plan (Phases 1C, 3, 8) requires modern Fluent controls for Settings redesign, sidebar, and NavigationView. CommunityToolkit.Mvvm provides MVVM but not visual controls. | Using only custom controls + FluentIcons.WPF would require rebuilding NavigationView, card layouts, and progress bars from scratch — significantly more effort than using a well-maintained open-source Fluent control library. Fallback plan exists if incompatibility is found. |

## Project Structure

### Documentation (this feature)

```text
specs/030-nuget-restructure-materialdesign-removal/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (minimal for this feature)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/
├── Som3a_WPF_UI.csproj          # Package references updated
├── Theme/
│   └── ThemeResources.xaml      # Old resource dictionaries removed
├── Converters/
│   └── MaterialIconConverter.cs # Removed
└── ... (existing pages, ViewModels, Services)

Som3aAddin/
├── app.config                   # Binding redirects cleaned
└── Som3aAddin.csproj            # Package references updated
```

**Structure Decision**: No new directories or files created. Existing files are modified or removed. The cleanup is strictly subtractive with the exception of two new NuGet package references in the WPF host project.

## Phase 0: Research & Unknown Resolution

**Status**: Complete (see [research.md](research.md))

**Findings**:
- The MaterialDesign library is referenced in three areas: NuGet packages, XAML resource dictionaries, and C# converters.
- WPF-UI (Wpf.Ui) and FluentIcons.WPF are actively maintained, compatible with .NET 8.0-windows, and align with the Fluent design system.
- The custom theme engine uses only `{DynamicResource}` bindings, so removing MaterialDesign resource keys will not break existing themeable properties as long as no control still references them.
- The VSTO add-in's `app.config` may contain assembly binding redirects that must be manually cleaned.

**Key decisions**:
- Zero visual changes in this phase. Build-first, smoke-test-second.
- Broken references are fixed with custom theme engine tokens, not by adding new styles.
- Backup branch strategy: preserve the old state on `fluent/phase-1b` before starting `fluent/phase-1c`.

## Phase 1: Design & Contracts

**Status**: Complete (see [data-model.md](data-model.md), [quickstart.md](quickstart.md))

### Data Model

No new data entities are introduced in this phase. The existing `SettingsRecord` and theme token architecture are untouched.

### Contracts

No new external interfaces or API contracts are introduced. The only "contract" is the dependency manifest (NuGet package list), which is captured implicitly in the `.csproj` files.

### Quickstart

See [quickstart.md](quickstart.md) for developer setup and verification steps.

## Phase 2: Task Planning

**Next command**: `/speckit.tasks` to generate `tasks.md`.

---

## Constitutional Re-Check (Post-Design)

All constitutional principles remain satisfied. The WPF-UI concern is documented in Complexity Tracking with a validated fallback plan. No new violations introduced.
