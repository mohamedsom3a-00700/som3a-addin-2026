using Som3a.Contracts;
using Som3a.DurationEstimator.Engine;

namespace Som3a.DurationEstimator.Export;

public interface IDurationExporter
{
    Task<ExportResult> ExportToExcelAsync(IReadOnlyList<DurationEstimate> estimates, string filePath, CancellationToken ct = default);
}

public class DurationExporter : IDurationExporter
{
    private readonly IExportEngine _exportEngine;

    public DurationExporter(IExportEngine exportEngine)
    {
        _exportEngine = exportEngine;
    }

    public async Task<ExportResult> ExportToExcelAsync(IReadOnlyList<DurationEstimate> estimates, string filePath, CancellationToken ct = default)
    {
        var request = new ExportRequest
        {
            Format = ExportFormat.Excel,
            TargetPath = filePath,
            Data = estimates.Select(e => new
            {
                e.ActivityId,
                e.Quantity,
                e.AppliedProductivityRate,
                e.CrewSize,
                e.HoursPerDay,
                e.DurationWorkingDays,
                e.CalendarDurationDays,
                e.StartDate,
                e.EndDate,
                e.OptimisticDuration,
                e.MostLikelyDuration,
                e.PessimisticDuration,
                e.StandardDeviation,
                e.IsAnomaly,
                e.AnomalyReason
            }),
            Options = new ExportOptions
            {
                SheetName = "Duration Estimates",
                IncludeHeaders = true,
                ApplyStyling = true
            }
        };

        return await _exportEngine.ExportAsync(request, ct);
    }
}
