# Quickstart: BOQ Activity Generator

**Feature**: BOQ Activity Generator (Phase 20)
**Date**: 2026-05-27

## Prerequisites

- Phase 14 (Platform Foundation) — Som3a.Domain, Som3a.Contracts, Som3a.AI, Som3a.Plugin.SDK
- Phase 18 (AI Core Infrastructure) — OrchestrationEngine, PromptTemplateRegistry, ActivityParser
- Phase 19 (WBS Engine) — WBS hierarchy context for sequencing
- Excel VSTO Add-in host with active workbook containing a BOQ sheet

## Key Files

| File | Purpose |
|------|---------|
| `WpfApp2/Pages/BOQActivityGeneratorPage.xaml` | Plugin page UI |
| `WpfApp2/Services/BOQContextBuilder.cs` | BOQ reading + AI context building |
| `WpfApp2/Services/ActivityGenerationService.cs` | AI orchestration for activity generation |
| `WpfApp2/Services/ActivityValidationService.cs` | Four-rule validation pipeline |
| `WpfApp2/Services/ActivitySequencingService.cs` | WBS ordering + dependency suggestions |
| `WpfApp2/Services/ActivityExportService.cs` | Excel export |
| `Som3a.Domain/Activities/Activity.cs` | Domain entity |
| `Som3a.AI/Prompts/BoqActivityPrompt.cs` | BOQ-to-activity prompt template |

## Flow

```
1. Engineer opens BOQ Activity Generator page
2. System reads active workbook → identifies BOQ sheet
3. Engineer reviews BOQ preview (items, quantities, classifications)
4. System shows data privacy notice → engineer consents
5. Engineer triggers AI generation
6. System builds optimized AI context → calls AI provider
7. Status message displayed with estimated time (non-blocking)
8. AI returns activity list → parsed into GeneratedActivity[]
9. Validation runs automatically (4 rules)
10. Engineer reviews + edits activities in grid
11. Engineer opens sequencing view → orders by WBS + trade
12. System suggests dependencies → engineer accepts/rejects
13. Engineer exports to Excel sheet
```

## Key Design Decisions

- **Async AI calls**: Non-blocking with progress status; 10-second cooldown between generations
- **Data privacy**: Opt-in consent before first AI call each session
- **Edit preservation**: User edits preserved across re-generation via BOQ reference matching
- **Validation pipeline**: Four independent rules, re-runnable without regeneration
- **Export**: Existing Excel interop, same workbook, new sheet "Generated Activities"

## Testing

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Domain tests
dotnet test Som3a.Domain.Tests --filter "Category=ActivityTests"

# AI tests
dotnet test Som3a.AI.Tests --filter "Category=BoqActivityPromptTests"

# UI tests
dotnet test WpfApp2.Tests --filter "Category=ActivityValidationServiceTests"
```
