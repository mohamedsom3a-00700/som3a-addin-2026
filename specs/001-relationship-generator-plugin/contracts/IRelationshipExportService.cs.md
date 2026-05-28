# IRelationshipExportService

**File**: `WpfApp2/Services/IRelationshipExportService.cs` (interface)
**Location**: `WpfApp2/Services/RelationshipExportService.cs` (implementation)

Exports generated relationships to the active Excel sheet as new columns.

## Interface

```csharp
public interface IRelationshipExportService
{
    Task ExportToExcelAsync(
        IReadOnlyList<Relationship> relationships,
        RelationshipExportConfig config,
        CancellationToken ct = default);

    bool SheetExists(string sheetName);

    void RemoveSheet(string sheetName);
}
```

## Methods

### ExportToExcelAsync
- **Input**: List of relationships + export configuration
- **Output**: void (Excel sheet modified in-place)
- **Behavior**:
  1. Identify the active worksheet (or the activity sheet from Phase 20)
  2. Add columns: Predecessor, Successor, Type, Lag (if not present)
  3. Populate rows: one row per (predecessorId, successorId, type, lag)
  4. Apply Excel formatting (headers bold, column widths auto-fit, borders)
  5. Only export accepted relationships (IsAccepted == true) by default
- **Contracts**:
  - Throws `InvalidOperationException` if no active workbook
  - Throws `ArgumentValidationException` if relationships list is empty
  - Preserves existing columns and data (only appends or updates relationship columns)
  - Column order: Predecessor, Successor, Type, Lag, Rationale (if IncludeRationale is true)
  - Header style uses theme colors via ThemeManager

### SheetExists / RemoveSheet
- Standard helpers to check and clean up export sheets

## Configuration

```csharp
public class RelationshipExportConfig
{
    public string SheetName { get; set; } = "Relationship Logic";
    public bool IncludeOnlyAccepted { get; set; } = true;
    public bool IncludeRationale { get; set; } = false;
    public bool OverwriteExisting { get; set; } = true;
    public string? ActivitySheetName { get; set; } // Phase 20 activity sheet
}
```
