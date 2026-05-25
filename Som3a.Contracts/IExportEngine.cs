namespace Som3a.Contracts
{
    public interface IExportEngine
    {
        Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default);
        Task<ValidationResult> ValidateAsync(ExportRequest request, CancellationToken ct = default);
        ExportEngineCapabilities GetCapabilities();
    }

    public class ExportRequest
    {
        public ExportFormat Format { get; set; }
        public string TargetPath { get; set; } = string.Empty;
        public IEnumerable<object> Data { get; set; } = Array.Empty<object>();
        public ExportOptions Options { get; set; } = new();
    }

    public class ExportResult
    {
        public bool IsSuccess { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public int SheetCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
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
        public List<ExportFormat> SupportedFormats { get; set; } = new();
        public int MaxRowsPerSheet { get; set; }
        public bool SupportsMultiSheet { get; set; }
        public bool SupportsStyling { get; set; }
    }
}
