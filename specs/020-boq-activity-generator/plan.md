# Implementation Plan: BOQ Activity Generator

**Branch**: `020-boq-activity-generator` | **Date**: 2026-05-27 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/020-boq-activity-generator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

AI-powered generation of construction activities from Excel BOQ data — the first user-visible AI feature. A planning engineer loads a BOQ from Excel, the system builds an AI context, generates structured construction activities with intelligent grouping, verb-noun naming, and BOQ references. Activities are validated, editable in a grid, sequenced with basic dependency suggestions, and exported to Excel.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for VSTO/WPF host, .NET 8.0 for Domain/AI/Contracts class libraries)

**Primary Dependencies**: Theme Engine, ThemeManager, ModernWindow, Shell, NavigationService, ServiceContainer, EventBus, ModuleRegistry, Som3a.Domain (Phase 14), Som3a.Contracts (Phase 14), Som3a.AI (Phase 18), Som3a.Plugin.SDK (Phase 14), WBS Engine (Phase 19), Excel interop (existing WpfApp2)

**Storage**: BOQ data read from active Excel workbook; generated activities held in memory during session; persisted via Excel export sheet

**Testing**: Unit tests (activity parsing, validation logic, domain entities); Integration tests (AI provider calls, Excel read/write); UI tests (editable grid, plugin page loading); Performance tests (generation time, export time)

**Target Platform**: Windows (x64) — Excel VSTO Add-in host

**Project Type**: Desktop application plugin — WPF plugin page within existing Shell workspace

**Performance Goals**: AI generation completes within 60 seconds for typical 200-500 BOQ items; grid cell edits respond within 1 second; Excel export completes within 5 seconds

**Constraints**: Must not break existing VSTO functionality; DynamicResource-only for all themeable UI; Excel VSTO rendering safety preserved; WindowChrome enforcement; MVVM separation; animations ≤200ms; centralized effects only

**Scale/Scope**: 200-500 BOQ items producing 50-150 activities; max 500 activities configurable; single-user desktop session

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Feature introduces a plugin page within the existing Shell workspace; no monolithic dictionaries; resources follow existing isolated patterns.
- [x] **III. DynamicResource-Only** — All new UI (grid, status messages, dialogs) uses DynamicResource for themeable brushes, colors, borders, effects per spec Constitutional Constraints.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation path goes through ThemeManager exclusively; no direct brush mutation from plugin code.
- [x] **IX. Animation Governance** — Any new animations (status transitions, grid feedback) complete within 200ms and remain GPU-safe.
- [x] **X. Excel Rendering Safety** — All UI is a Page within the existing Shell; WindowRenderModeDetector already active; fallback mode available if needed.
- [x] **XI. WindowChrome Enforcement** — No new windows; feature is a plugin page within Shell using existing ModernWindow infrastructure.
- [x] **XII. Centralized Effects** — No inline DropShadowEffect; all effects sourced from centralized Effects/Shadows.xaml per spec.
- [x] **XV. Resource Loading Order** — No new top-level resource dictionaries needed; plugin page uses existing theme resources.

## Project Structure

### Documentation (this feature)

```text
specs/020-boq-activity-generator/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
WpfApp2/
├── Pages/
│   └── BOQActivityGeneratorPage.xaml          # Plugin page (Shell workspace)
│   └── BOQActivityGeneratorPage.xaml.cs
├── ViewModels/
│   └── BOQActivityGeneratorViewModel.cs       # MVVM ViewModel
├── Services/
│   ├── BOQContextBuilder.cs                   # BOQ reading + AI context building
│   ├── ActivityGenerationService.cs           # AI orchestration for activity generation
│   ├── ActivityValidationService.cs           # Validation logic
│   ├── ActivitySequencingService.cs           # Sequencing + dependency suggestions
│   └── ActivityExportService.cs               # Excel export
├── Models/
│   └── GeneratedActivity.cs                  # Activity model

Som3a.Domain/
└── Activities/
    ├── Activity.cs                            # Domain entity (extends Phase 14)
    ├── ActivityId.cs                          # Value object
    ├── ActivityGroupingRule.cs                # Grouping logic
    └── ActivityValidationResult.cs            # Validation result type

Som3a.AI/
└── Prompts/
    └── BoqActivityPrompt.cs                   # BOQ-to-activity prompt templates

Tests/
├── Som3a.Domain.Tests/
│   └── ActivityTests.cs
├── Som3a.AI.Tests/
│   └── BoqActivityPromptTests.cs
└── WpfApp2.Tests/
    ├── ActivityValidationServiceTests.cs
    └── ActivityExportServiceTests.cs
```

**Structure Decision**: New plugin page under WpfApp2/Pages/ + supporting services under WpfApp2/Services/ + domain extensions under Som3a.Domain/Activities/ + AI prompt under Som3a.AI/Prompts/. This follows the existing Phase 18/19 pattern for AI-powered features.

## Complexity Tracking

No constitution violations exist. Complexity tracking section is not required.
