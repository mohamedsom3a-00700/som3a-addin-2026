# Implementation Plan: WBS Engine

**Branch**: `019-wbs-engine` | **Date**: 2026-05-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/019-wbs-engine/spec.md`

## Summary

Implement a WBS (Work Breakdown Structure) Engine with AI-powered generation, template system with 15+ templates across 5 categories, manual tree editor with auto-code generation, and multi-format export (Excel, JSON, XML). Builds on Som3a.Domain.WBSNode (Phase 14) and Som3a.AI orchestration (Phase 18).

## Technical Context

**Language/Version**: C# (.NET 8.0 for domain/data logic; .NET Framework 4.8 for WpfApp2 WPF pages)

**Primary Dependencies**: Som3a.Domain (Phase 14 — WBSNode entity); Som3a.AI (Phase 18 — OrchestrationEngine, WBSParser); ClosedXML or EPPlus for Excel export; existing Shell/NavigationService for WPF pages; existing EventBus for UI events

**Storage**: File system (AppData/Som3a/templates/) for WBS templates (JSON); in-memory for active WBS project tree; Excel files at user-chosen export path

**Testing**: xUnit for unit tests (tree validation, code auto-generation, export format correctness); integration tests (AI generation pipeline, Excel export)

**Target Platform**: Windows x64 — WPF pages hosted in existing VSTO Shell workspace

**Project Type**: WPF pages (WpfApp2) + domain services (Som3a.Domain)

**Performance Goals**: AI generation ≤30s (SC-002); manual edits applied ≤1s (SC-005); Excel export ≤5s for 200-node trees (SC-006); tree validation completes instantly

**Constraints**: No third-party UI frameworks (Constitution §XIV); all AI calls async via Phase 18 orchestration; DynamicResource-only for all themeable WPF properties; template storage follows same pattern as Phase 18 PromptTemplateRegistry

**Scale/Scope**: Single-user desktop app; 15+ templates across 5 categories; WBS trees up to 10 levels (configurable); 200 nodes max for export performance target

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — WBS logic lives in Som3a.Domain.WBS (existing) and new WpfApp2 Pages/ViewModels; no monolithic dictionaries.
- [x] **III. DynamicResource-Only** — All new WBS WPF pages use DynamicResource for themeable properties.
- [x] **IV. Runtime Theme Mutation Governance** — Theme changes via ThemeManager; WBS pages use standard theme resources.
- [x] **IX. Animation Governance** — UI transitions (tree expand/collapse) ≤200ms.
- [x] **X. Excel Rendering Safety** — Excel export uses library-based file generation (ClosedXML/EPPlus), not VSTO interop during rendering.
- [x] **XI. WindowChrome Enforcement** — WBS pages hosted in existing Shell workspace; no new standalone windows.
- [x] **XII. Centralized Effects** — No inline effects in WBS pages; use centralized Effects/Shadows.xaml.
- [x] **XV. Resource Loading Order** — Any new resource dictionaries follow prescribed loading order.

## Project Structure

### Documentation (this feature)

```text
specs/019-wbs-engine/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Som3a.Domain/
└── WBS/                          # Existing from Phase 14
    ├── WBSNode.cs                # Extended with Id (GUID), tree validation
    └── WBSTemplate.cs            # New: template container

WpfApp2/
├── Pages/
│   └── WBS/
│       ├── WBSTemplateBrowser.xaml / .cs    # US1: Browse templates
│       ├── WBSGeneratorPage.xaml / .cs      # US2: AI generation
│       ├── WBSEditorPage.xaml / .cs         # US3: Manual tree editor
│       └── WBSExportPage.xaml / .cs         # US4: Export
├── ViewModels/
│   └── WBS/
│       ├── WBSTemplateBrowserViewModel.cs
│       ├── WBSGeneratorViewModel.cs
│       ├── WBSEditorViewModel.cs
│       └── WBSExportViewModel.cs
└── Services/
    └── WBS/
        ├── WBSTemplateService.cs           # Template CRUD + recommendations
        ├── WBSCodeGenerator.cs             # Hierarchical code auto-generation
        ├── WBSTreeValidator.cs             # Tree integrity validation
        ├── WBSExportService.cs             # Excel/JSON/XML export
        └── WBSAIService.cs                 # AI generation via Phase 18 orchestration

WpfApp2.Tests/ (or Som3a.Domain.Tests/)
└── WBS/
    ├── WBSCodeGeneratorTests.cs
    ├── WBSTreeValidatorTests.cs
    └── WBSExportServiceTests.cs
```

**Structure Decision**: New WBS pages, ViewModels, and services live in `WpfApp2/` under feature folders (`Pages/WBS/`, `ViewModels/WBS/`, `Services/WBS/`). Domain entity (WBSNode) extended in `Som3a.Domain/WBS/`. No new .NET 8.0 project needed — Phase 18 already handles AI infrastructure.

## Complexity Tracking

No constitution violations detected — all checks pass cleanly.
