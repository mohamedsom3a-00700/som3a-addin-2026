# Quickstart: WBS Engine

**Prerequisites**:
- Som3a.Domain (Phase 14) — WBSNode entity
- Som3a.AI (Phase 18) — OrchestrationEngine, WBSParser
- ClosedXML NuGet package for Excel export

## 1. Extend WBSNode (if needed)

The `WBSNode` entity in `Som3a.Domain/WBS/WBSNode.cs` already has `Id` (GUID), `Code`, `Name`, `Level`, `Parent`, `Children`, `FullPath`. Verify it supports dual identity (stable GUID + mutable Code).

## 2. Create WBS Template Service

Create `WpfApp2/Services/WBS/WBSTemplateService.cs` implementing `IWBSTemplateService`:
- Load 15+ built-in templates from embedded resources
- User templates stored as JSON in `%APPDATA%\Som3a\wbs-templates\`
- Template recommendation via keyword matching

## 3. Create WBS Code Generator

Create `WpfApp2/Services/WBS/WBSCodeGenerator.cs` implementing `IWBSCodeGenerator`:
- Format: `ParentCode.SiblingIndex` (1-based)
- Root nodes: 1, 2, 3... Children: 1.1, 1.2, 2.1...
- Renumber entire subtree on insert/delete

## 4. Create WBS Tree Validator

Create `WpfApp2/Services/WBS/WBSTreeValidator.cs` implementing `IWBSTreeValidator`:
- Cycle detection via DFS
- Depth limit check (default 10)
- Naming convention validation

## 5. Create AI WBS Service

Create `WpfApp2/Services/WBS/WBSAIService.cs` implementing `IWBSAIService`:
- Uses Phase 18 `OrchestrationEngine.ExecuteAsync()` with WBS prompt template
- Uses Phase 18 `WBSParser` to parse AI output into WBSNode tree
- Falls back gracefully when AI unavailable

## 6. Create WBS Export Service

Create `WpfApp2/Services/WBS/WBSExportService.cs` implementing `IWBSExportService`:
- Excel: ClosedXML with indentation, grouping, auto-fit columns
- JSON: System.Text.Json with tree structure
- XML: System.Xml.Linq with hierarchical elements

## 7. Create WPF Pages

Create pages in `WpfApp2/Pages/WBS/`:
- `WBSTemplateBrowserPage` — browse and select templates (US1)
- `WBSGeneratorPage` — AI generation with progress (US2)
- `WBSEditorPage` — tree editor with drag/drop support (US3)
- `WBSExportPage` — format selection and export (US4)

## 8. Register Services

In `CompositionRoot.RegisterServices()`:
```csharp
container.RegisterSingleton<IWBSTemplateService, WBSTemplateService>();
container.RegisterSingleton<IWBSCodeGenerator, WBSCodeGenerator>();
container.RegisterSingleton<IWBSTreeValidator, WBSTreeValidator>();
container.RegisterSingleton<IWBSExportService, WBSExportService>();
container.RegisterSingleton<IWBSAIService, WBSAIService>();
```

## 9. Build & Verify

```powershell
dotnet build WpfApp2/WpfApp2.csproj
# Or use MSBuild:
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```
