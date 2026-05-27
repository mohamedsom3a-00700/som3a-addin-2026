# Contract: IActivityExportService

**Layer**: WpfApp2.Services
**Purpose**: Exports the final sequenced activity list to an Excel sheet within the active workbook.

```csharp
namespace WpfApp2.Services;

public interface IActivityExportService
{
    Task ExportAsync(
        IReadOnlyList<GeneratedActivity> activities,
        ActivityExportConfig config,
        CancellationToken ct = default);

    bool SheetExists(string sheetName);

    void RemoveSheet(string sheetName);
}
```
