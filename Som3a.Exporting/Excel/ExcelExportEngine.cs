using Som3a.Contracts;

namespace Som3a.Exporting.Excel
{
    public class ExcelExportEngine : IExportEngine
    {
        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var data = request.Data.ToList();

                return new ExportResult
                {
                    IsSuccess = false,
                    OutputPath = request.TargetPath,
                    ErrorMessage = "Excel export is handled in the .NET Framework 4.8 VSTO host via the interop bridge. " +
                                   "Call the bridge channel to marshal data and use the VSTO Excel interop.",
                    RowCount = data.Count,
                    SheetCount = 0,
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
                SupportedFormats = new List<ExportFormat> { ExportFormat.Excel },
                MaxRowsPerSheet = 1048576,
                SupportsMultiSheet = true,
                SupportsStyling = true
            };
        }
    }
}
