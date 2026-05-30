# Implementation Plan: Framework & Project Format Upgrade

**Branch**: `028-framework-project-upgrade` | **Date**: 2026-05-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/028-framework-project-upgrade/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Upgrade WpfApp2 (Som3a_WPF_UI.csproj) from .NET Framework 4.8 to .NET 8.0-windows, convert to SDK-style project format, and migrate NuGet management from packages.config to PackageReference. No logic changes. All existing tests must pass unchanged. VSTO add-in configuration must be audited for compatibility.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (upgrading from C# 7.3 / .NET Framework 4.8)

**Primary Dependencies**: WPF (built-in via `UseWPF` property); existing NuGet packages migrated to PackageReference with minimum compatible versions:
- MaterialDesignColors 3.1.0 → minimum .NET 8.0-compatible version
- MaterialDesignThemes 5.1.0 → minimum .NET 8.0-compatible version
- Microsoft.Web.WebView2 1.0.3912.50 → same or minimum compatible
- Microsoft.Xaml.Behaviors.Wpf 1.1.39 → same or minimum compatible
- System.Text.Json 6.0.0 → same or minimum compatible
- Newtonsoft.Json 13.0.4 → same or minimum compatible

**Storage**: N/A — no storage changes in this phase

**Testing**: 
- Som3a_WPF_UI.Tests (MSTest) — must pass unchanged
- Som3a.Infrastructure.Tests (xUnit) — must pass unchanged
- VSTO smoke test (manual) — ribbon, ShellWindow, sidebar, navigation, theme, Excel interop

**Target Platform**: Windows (Excel VSTO host + standalone WPF; VSTO add-in remains on .NET Framework 4.8)

**Project Type**: Desktop application — WPF host + VSTO add-in

**Performance Goals**: Build time ≤150% of pre-migration baseline; no runtime performance regression

**Constraints**: 
- Zero logic changes — only project format + API compatibility fixes allowed
- No new NuGet packages — only existing ones migrated
- VSTO add-in project configuration (binding redirects, app.config) must be audited
- Pre-upgrade screenshots of 3-5 key pages (both themes) for visual regression comparison

**Scale/Scope**: Single WPF project (.csproj), 6 NuGet packages, existing ~20+ ViewModels

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — N/A: No new resource dictionaries or UI libraries introduced.
- [x] **III. DynamicResource-Only** — N/A: No XAML changes; framework upgrade only.
- [x] **IV. Runtime Theme Mutation Governance** — N/A: No theme mutation code changed.
- [x] **IX. Animation Governance** — N/A: No animation additions or modifications.
- [x] **X. Excel Rendering Safety** — N/A: No window rendering or visual changes.
- [x] **XI. WindowChrome Enforcement** — N/A: No window or control changes.
- [x] **XII. Centralized Effects** — N/A: No effect system changes.
- [x] **XV. Resource Loading Order** — N/A: No new resource dictionaries added.

**Gate verdict**: PASS — all constitutional checks are N/A for this infrastructure-only upgrade.

## Project Structure

### Documentation (this feature)

```text
specs/028-framework-project-upgrade/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── project-format.md
├── checklists/
│   └── requirements.md
└── spec.md              # Feature specification
```

### Source Code (repository root)

```text
WpfApp2/
├── Som3a_WPF_UI.csproj          # [MODIFIED] SDK-style, net8.0-windows, PackageReference
├── packages.config              # [REMOVED] Migrated to PackageReference in .csproj
├── app.config                   # [REVIEWED] Binding redirects updated
└── ... (all existing .cs, .xaml files remain unchanged unless API fixes needed)

Som3a Addin 2026/
└── ... (VSTO add-in project config audited for WpfApp2 compatibility)

Tests/
├── Som3a_WPF_UI.Tests/          # [VERIFIED] Must pass unchanged
└── Som3a.Infrastructure.Tests/  # [VERIFIED] Must pass unchanged
```

**Structure Decision**: No new directories required. This phase modifies existing `WpfApp2/Som3a_WPF_UI.csproj`, removes `packages.config`, and applies minimal API compatibility fixes to existing source files. VSTO add-in configuration files may need binding redirect updates.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations — all constitutional checks are N/A.
