using System.Xml.Linq;
using Som3a.Contracts;

namespace Som3a.Exporting.Formats
{
    public class XmlExporter : IExportEngine
    {
        public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var rootName = request.Options.SheetName ?? "Items";
                var root = new XElement(rootName);

                foreach (var item in request.Data)
                {
                    var itemElement = new XElement("Item");
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var value = prop.GetValue(item);
                        if (value != null)
                            itemElement.Add(new XAttribute(prop.Name, value.ToString()));
                    }
                    root.Add(itemElement);
                }

                await Task.Run(() => root.Save(request.TargetPath), ct);

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
                SupportedFormats = new List<ExportFormat> { ExportFormat.Xml },
                MaxRowsPerSheet = int.MaxValue,
                SupportsMultiSheet = false,
                SupportsStyling = true
            };
        }
    }
}
