using Som3a.Contracts;

namespace Som3a.Exporting.Pipeline
{
    public class ExportPipeline : IExportEngine
    {
        private readonly IEnumerable<IExportEngine> _engines;

        public ExportPipeline(IEnumerable<IExportEngine> engines)
        {
            _engines = engines;
        }

        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var engine = _engines.FirstOrDefault(e => e.GetCapabilities().SupportedFormats.Contains(request.Format));
            if (engine == null)
                return new ExportResult { IsSuccess = false, ErrorMessage = $"No engine supports format {request.Format}." };

            return await engine.ExportAsync(request, ct);
        }

        public async Task<ValidationResult> ValidateAsync(ExportRequest request, CancellationToken ct = default)
        {
            var engine = _engines.FirstOrDefault(e => e.GetCapabilities().SupportedFormats.Contains(request.Format));
            if (engine == null)
                return ValidationResult.Failure($"No engine supports format {request.Format}.");

            return await engine.ValidateAsync(request, ct);
        }

        public ExportEngineCapabilities GetCapabilities()
        {
            var allFormats = new List<ExportFormat>();
            foreach (var engine in _engines)
                allFormats.AddRange(engine.GetCapabilities().SupportedFormats);

            return new ExportEngineCapabilities
            {
                SupportedFormats = allFormats.Distinct().ToList(),
                MaxRowsPerSheet = int.MaxValue,
                SupportsMultiSheet = true,
                SupportsStyling = true
            };
        }
    }
}
