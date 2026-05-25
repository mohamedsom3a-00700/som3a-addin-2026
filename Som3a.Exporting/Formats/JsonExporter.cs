using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.Exporting.Formats
{
    public class JsonExporter : IExportEngine
    {
        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = request.Options.ApplyStyling,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var json = JsonSerializer.Serialize(request.Data, options);
                await File.WriteAllTextAsync(request.TargetPath, json, ct);

                return new ExportResult
                {
                    IsSuccess = true,
                    OutputPath = request.TargetPath,
                    RowCount = request.Data.Count(),
                    SheetCount = 1,
                    Duration = sw.Elapsed
                };
            }
            catch (Exception ex)
            {
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Duration = sw.Elapsed
                };
            }
        }

        public Task<ValidationResult> ValidateAsync(ExportRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.TargetPath))
                return Task.FromResult(ValidationResult.Failure("TargetPath must not be empty."));
            if (request.Data == null || !request.Data.Any())
                return Task.FromResult(ValidationResult.Failure("Data must not be empty."));
            return Task.FromResult(ValidationResult.Success());
        }

        public ExportEngineCapabilities GetCapabilities()
        {
            return new ExportEngineCapabilities
            {
                SupportedFormats = new List<ExportFormat> { ExportFormat.Json },
                MaxRowsPerSheet = int.MaxValue,
                SupportsMultiSheet = false,
                SupportsStyling = true
            };
        }
    }
}
