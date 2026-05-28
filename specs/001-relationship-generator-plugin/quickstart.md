# Quickstart: Relationship Generator Plugin

**Feature**: Relationship Generator Plugin (Phase 21)
**Date**: 2026-05-27

## Prerequisites

- Phase 14 (Platform Foundation) — Som3a.Domain, Som3a.Validation, Som3a.AI, Som3a.Contracts
- Phase 18 (AI Core Infrastructure) — OrchestrationEngine, PromptTemplateRegistry, RelationshipParser
- Phase 20 (BOQ Activity Generator) — Generated activities with trade categories
- Excel VSTO Add-in host with active workbook containing generated activities

## Key Files

| File | Purpose |
|------|---------|
| `WpfApp2/Pages/RelationshipGeneratorPage.xaml` | Plugin page UI |
| `WpfApp2/ViewModels/RelationshipGeneratorViewModel.cs` | MVVM ViewModel |
| `WpfApp2/Services/RelationshipGenerationService.cs` | AI orchestration for relationship generation |
| `WpfApp2/Services/RelationshipValidationService.cs` | Network validation orchestration |
| `WpfApp2/Services/RelationshipAnalysisService.cs` | Parallel execution + critical path analysis |
| `WpfApp2/Services/RelationshipExportService.cs` | Excel export |
| `WpfApp2/Controls/RelationshipEditorGrid.xaml` | Inline relationship editor with dropdowns |
| `Som3a.Domain/Relationships/Relationship.cs` | Core domain entity (enhanced) |
| `Som3a.Domain/Relationships/RelationshipNetwork.cs` | Graph container |
| `Som3a.Validation/Relationships/LoopDetector.cs` | DFS cycle detection |
| `Som3a.Validation/Relationships/DependencyValidator.cs` | Relationship dependency validation |
| `Som3a.AI/Prompts/RelationshipPrompt.cs` | AI prompt template |
| `Som3a.AI/Parsers/RelationshipParser.cs` | AI JSON output parser |

## Flow

```
1. Engineer opens Relationship Generator page
2. System loads activities from Phase 20 (or main activity list)
3. Engineer reviews activities in source grid
4. Engineer triggers AI relationship generation
5. System builds AI context (activity list with trade categories)
6. AI generates relationships → parsed into Relationship[]
7. Auto-retry once with simplified prompt on failure
8. On second failure: manual editor mode activated
9. Validation runs automatically (5-rule pipeline)
10. Engineer reviews + edits relationships in inline grid
   - Predecessor/Successor dropdowns
   - Type dropdown (FS/SS/FF/SF)
   - Lag spinner (+/- days)
   - Accept/Reject toggle per row
11. Engineer runs parallel analysis + critical path view
12. Engineer exports to Excel (new columns in activity sheet)
```

## Key Design Decisions

- **AI with fallback**: Auto-retry once; manual editor on second failure
- **Relationship types**: FS/SS/FF/SF with positive, zero, or negative lag
- **Validation pipeline**: 5 independent rules run on generation + on user request
- **Graph algorithms**: DFS for cycles, Kahn topological sort for parallel groups + critical path
- **Edit preservation**: Matched by (predecessorId, successorId) across re-generation
- **Grid editor**: WPF DataGrid with ComboBox columns, per-row accept/reject
- **Export**: New columns in existing activity sheet, accepted-only by default

## Testing

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Domain tests
dotnet test Som3a.Domain.Tests --filter "Category=RelationshipDomainTests"

# Loop detection tests
dotnet test WpfApp2.Tests --filter "Category=LoopDetectorTests"

# Service tests
dotnet test WpfApp2.Tests --filter "Category=RelationshipGenerationServiceTests"
dotnet test WpfApp2.Tests --filter "Category=RelationshipValidationServiceTests"
dotnet test WpfApp2.Tests --filter "Category=RelationshipAnalysisServiceTests"
```
