# Contract: IBOQContextBuilder

**Layer**: WpfApp2.Services
**Purpose**: Reads BOQ data from the active Excel workbook and builds an AI-ready context.

```csharp
namespace WpfApp2.Services;

public interface IBOQContextBuilder
{
    Task<BOQContext> BuildContextAsync(CancellationToken ct = default);

    bool TryIdentifyBoqSheet(out string? sheetName);
}
```
