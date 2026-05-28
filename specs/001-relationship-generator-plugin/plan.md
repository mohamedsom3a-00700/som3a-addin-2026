# Implementation Plan: Relationship Generator Plugin

**Branch**: `021-relationship-generator-plugin` | **Date**: 2026-05-27 | **Spec**: `specs/001-relationship-generator-plugin/spec.md`

**Input**: Feature specification from `specs/001-relationship-generator-plugin/spec.md`

## Summary

Generate predecessor/successor relationships (FS/SS/FF/SF) between construction activities using AI analysis of trade sequence, space constraints, and resource flow. Validate the resulting logic network for cycles, open ends, dangling links, and redundant relationships. Analyze parallel execution paths and identify the critical path through the network.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for WpfApp2 host, .NET 8.0 for Som3a.Domain, Som3a.AI, Som3a.Validation, Som3a.Contracts libraries)

**Primary Dependencies**:
- `Som3a.Domain/Relationships/Relationship.cs` — existing domain entity with RelationshipType (FS/SS/FF/SF) and Lag
- `Som3a.Domain/Activities/Activity.cs` — existing activity entity, source of activity data
- `Som3a.Validation/Relationships/DependencyValidator.cs` — existing validator for activity references, self-loops, lag
- `Som3a.Validation/Relationships/LoopDetector.cs` — existing DFS cycle detector
- `Som3a.AI/Orchestration/OrchestrationEngine.cs` — Phase 18 AI orchestration
- `Som3a.AI/Parsers/RelationshipParser.cs` — existing parser for AI relationship output
- `Som3a.AI/Prompts/PromptTemplateRegistry.cs` — prompt management
- `WpfApp2` — shell hosting, ServiceContainer, EventBus, plugin page infrastructure
- Phase 20 generated activities as input

**Storage**: In-memory during session; save to active Excel sheet as new columns (Predecessor, Successor, Type, Lag) on user request

**Testing**: MSTest v3.1.1 (`MSTest.TestFramework`) with `Microsoft.NET.Test.Sdk` v17.8.0

**Target Platform**: Windows, Excel VSTO add-in (WPF .NET Framework 4.8 host)

**Project Type**: Desktop plugin (WPF page within existing VSTO add-in shell)

**Performance Goals**: Generate relationships for 100+ activities in under 30 seconds; cycle detection for any network size; critical path calc < 5 seconds

**Constraints**: Excel VSTO rendering safety; DynamicResource-only theme for any new UI; async non-blocking AI calls; auto-retry once on AI failure with simplified prompt; fallback to manual editor on second failure

**Scale/Scope**: Single-project networks with 100+ activities; relationship editor grid with inline dropdowns

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **II. MVVM Architecture Enforcement** — New plugin page uses ViewModel + code-behind UI-only; relationship logic in services.
- [x] **III. DynamicResource-Only** — Any new UI controls (grid, dropdowns) use DynamicResource for themeable properties.
- [x] **IV. Runtime Theme Mutation Governance** — No direct brush mutation; all theme changes via ThemeManager.
- [x] **IX. Animation Governance** — Inline grid editing may use subtle opacity transitions ≤200ms; no heavy animations.
- [x] **X. Excel Rendering Safety** — Excel save (new columns) uses existing VSTO interop; no window rendering changes.
- [x] **XII. Centralized Effects** — No new DropShadowEffect or inline effects needed for this feature.
- [x] **XIII. Performance & Rendering Efficiency** — DataGrid with EnableRowVirtualization=True for relationship editor grid.
- [x] **XIV. No Third-Party UI Frameworks** — Native WPF DataGrid, ComboBox for relationship editor.

## Project Structure

### Documentation (this feature)

```text
specs/001-relationship-generator-plugin/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Som3a.Domain/Relationships/
├── Relationship.cs              # Existing — enhanced with AI metadata fields
├── RelationshipType.cs          # Existing enum: FS, SS, FF, SF
├── RelationshipNetwork.cs       # NEW — graph container with adjacency list
├── ParallelExecutionGroup.cs    # NEW — entity for parallel group analysis
├── CriticalPathResult.cs        # NEW — critical path analysis result
└── ResourceConflict.cs          # NEW — resource conflict detection result

Som3a.Validation/Relationships/
├── DependencyValidator.cs       # Existing
├── LoopDetector.cs             # Existing (DFS-based)
├── NetworkValidator.cs         # NEW — orchestrates all validation rules + report
└── OpenEndDetector.cs          # NEW — identifies open-start / open-end activities

Som3a.AI/Prompts/
├── PromptTemplateRegistry.cs   # Existing — add Relationship prompt template
└── RelationshipPrompt.cs       # NEW — relationship generation prompt template

Som3a.AI/Parsers/
└── RelationshipParser.cs       # Existing — enhanced for richer structured output

WpfApp2/
├── Pages/
│   └── RelationshipGeneratorPage.xaml     # NEW — plugin page UI
│   └── RelationshipGeneratorPage.xaml.cs  # NEW — code-behind (UI-only)
├── ViewModels/
│   └── RelationshipGeneratorViewModel.cs  # NEW — MVVM ViewModel
├── Services/
│   ├── RelationshipGenerationService.cs   # NEW — AI orchestration for relationship generation
│   ├── RelationshipValidationService.cs   # NEW — validation orchestration
│   └── RelationshipAnalysisService.cs     # NEW — parallel execution + critical path analysis
├── Controls/
│   └── RelationshipEditorGrid.xaml        # NEW — inline grid editor with dropdowns
│   └── RelationshipEditorGrid.xaml.cs     # NEW — code-behind

Tests/
├── RelationshipGenerationServiceTests.cs         # NEW
├── RelationshipValidationServiceTests.cs          # NEW
├── RelationshipAnalysisServiceTests.cs            # NEW
├── RelationshipDomainTests.cs                     # NEW
└── LoopDetectorTests.cs                           # NEW
```

**Structure Decision**: Extends existing modular library pattern — domain entities in `Som3a.Domain/Relationships/`, validation in `Som3a.Validation/Relationships/`, AI in `Som3a.AI/`, WPF UI in `WpfApp2/`. No new projects.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
