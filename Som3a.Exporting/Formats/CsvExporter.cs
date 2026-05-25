using System.Text;
using Som3a.Contracts;

namespace Som3a.Exporting.Formats
{
    public class CsvExporter : IExportEngine
    {
        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var data = request.Data.ToList();
                var sb = new StringBuilder();
                var delimiter = ",";

                if (request.Options.ColumnMappings != null && request.Options.IncludeHeaders)
                {
                    var headers = string.Join(delimiter, request.Options.ColumnMappings.Keys);
                    sb.AppendLine(headers);
                }

                foreach (var item in data)
                {
                    var values = request.Options.ColumnMappings != null
                        ? request.Options.ColumnMappings.Select(kvp => GetPropertyValue(item, kvp.Value))
                        : Enumerable.Empty<string>();

                    sb.AppendLine(string.Join(delimiter, values.Select(EscapeCsv)));
                }

                await File.WriteAllTextAsync(request.TargetPath, sb.ToString(), ct);

                return new ExportResult
                {
                    IsSuccess = true,
                    OutputPath = request.TargetPath,
                    RowCount = data.Count,
                    SheetCount = 1,
                    Duration = sw.Elapsed
                };
            }
            catch (Exception ex)
            {
                return new ExportResult { IsSuccess = false, ErrorMessage = ex.Message, Duration = sw.Elapsed };
            }
        }

        public Task<ValidationResult> ValidateAsync(ExportRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.TargetPath))
                return Task.FromResult(ValidationResult.Failure("TargetPath must not be empty."));
            return Task.FromResult(ValidationResult.Success());
        }

        public ExportEngineCapabilities GetCapabilities()
        {
            return new ExportEngineCapabilities
            {
                SupportedFormats = new List<ExportFormat> { ExportFormat.Csv },
                MaxRowsPerSheet = int.MaxValue,
                SupportsMultiSheet = false,
                SupportsStyling = false
            };
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static string GetPropertyValue(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj)?.ToString() ?? string.Empty;
        }
    }
}
