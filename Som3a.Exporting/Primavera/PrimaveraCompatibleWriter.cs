using System.Text;
using Som3a.Contracts;

namespace Som3a.Exporting.Primavera
{
    public class PrimaveraCompatibleWriter : IExportEngine
    {
        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var data = request.Data.ToList();
                var sb = new StringBuilder();

                if (request.Options.IncludeHeaders)
                {
                    sb.AppendLine("ActivityID,ActivityName,Duration,Start,Finish,Predecessors,Successors");
                }

                foreach (var item in data)
                {
                    var activityId = GetProp(item, "ActivityId");
                    var name = GetProp(item, "Name");
                    var duration = GetProp(item, "Duration") ?? "0";
                    var rels = GetProp(item, "Relationships") ?? string.Empty;
                    sb.AppendLine($"{activityId},{name},{duration},,,{rels},");
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
                SupportedFormats = new List<ExportFormat> { ExportFormat.PrimaveraCompatible },
                MaxRowsPerSheet = int.MaxValue,
                SupportsMultiSheet = false,
                SupportsStyling = false
            };
        }

        private static string GetProp(object obj, string name)
        {
            return obj.GetType().GetProperty(name)?.GetValue(obj)?.ToString() ?? string.Empty;
        }
    }
}
