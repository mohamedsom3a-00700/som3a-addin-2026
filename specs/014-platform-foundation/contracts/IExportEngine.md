# Contract: IExportEngine

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the contract for the export pipeline — transforming domain entities into formatted output files (Excel, CSV, JSON, XML, Primavera-compatible tables).

## Interface

```csharp
public interface IExportEngine
{
    Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default);
    Task<ValidationResult> ValidateAsync(ExportRequest request, CancellationToken ct = default);
    ExportEngineCapabilities GetCapabilities();
}
```

## Request/Response Types

```csharp
public class ExportRequest
{
    public ExportFormat Format { get; set; }
    public string TargetPath { get; set; }
    public IEnumerable<object> Data { get; set; }       // Domain entities to export
    public ExportOptions Options { get; set; }
}

public class ExportResult
{
    public bool IsSuccess { get; set; }
    public string OutputPath { get; set; }
    public int RowCount { get; set; }
    public int SheetCount { get; set; }                  // For multi-sheet formats
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationMessage> Warnings { get; set; }
}

public class ExportOptions
{
    public bool IncludeHeaders { get; set; } = true;
    public string? SheetName { get; set; }
    public Dictionary<string, string>? ColumnMappings { get; set; }
    public bool ApplyStyling { get; set; } = true;
}

public enum ExportFormat
{
    Excel,
    Csv,
    Json,
    Xml,
    PrimaveraCompatible
}

public class ExportEngineCapabilities
{
    public List<ExportFormat> SupportedFormats { get; set; }
    public int MaxRowsPerSheet { get; set; }
    public bool SupportsMultiSheet { get; set; }
    public bool SupportsStyling { get; set; }
}
```

## Pipeline

```text
Validate → Review Warnings → Generate Output → Apply Styling → Write File
```

1. **Validate**: Pre-export check (required fields present, data integrity).
2. **Review**: User reviews warnings; can proceed or cancel.
3. **Generate**: Format-specific output generation.
4. **Style**: Apply formatting (Excel styles, JSON pretty-print).
5. **Write**: Final file write to TargetPath.

## Performance

- 500 Activity entities to Excel ≤ 10 seconds (SC-004).
- Supports 50,000-row exports without excessive memory.
