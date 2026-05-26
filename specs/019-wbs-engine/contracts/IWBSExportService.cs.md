# Contract: IWBSExportService

**Layer**: WpfApp2.Services.WBS
**Purpose**: Export WBS tree to Excel, JSON, and XML formats.

```csharp
namespace WpfApp2.Services.WBS;

public interface IWBSExportService
{
    Task ExportToExcelAsync(WBSNode root, string filePath);
    
    Task ExportToJsonAsync(WBSNode root, string filePath);
    
    Task ExportToXmlAsync(WBSNode root, string filePath);
}
```
