using Microsoft.Extensions.Logging;
using Som3a.Contracts;
using Som3a.DurationEstimator.Calendar;
using Som3a.DurationEstimator.Engine;
using Som3a.DurationEstimator.Export;

namespace Som3a.DurationEstimator;

public interface IDurationEstimatorService
{
    DurationEstimate CalculateEstimate(string activityId, decimal quantity, decimal productivityRate, int crewSize, decimal hoursPerDay);
    DurationEstimate Recalculate(DurationEstimate estimate, decimal? newRate = null, int? newCrewSize = null, decimal? newHours = null);
    Task<ExportResult> ExportToExcelAsync(IReadOnlyList<DurationEstimate> estimates, string filePath, CancellationToken ct = default);
    Task<string> ExportToPipelineAsync(string projectId, CalendarConfig calendar, IReadOnlyList<DurationEstimate> estimates, CancellationToken ct = default);
    void SetCalendar(CalendarConfig calendar);
    void SetModifiers(IEnumerable<ProductivityModifier> modifiers);
}

public class DurationEstimatorService : IDurationEstimatorService
{
    private readonly IDurationCalculator _calculator;
    private readonly IDurationExporter _exporter;
    private readonly ISchedulingPipelineWriter _pipelineWriter;
    private readonly ILogger<DurationEstimatorService> _logger;
    private CalendarConfig? _calendar;
    private List<ProductivityModifier> _modifiers = new();

    public DurationEstimatorService(
        IDurationCalculator calculator,
        IDurationExporter exporter,
        ISchedulingPipelineWriter pipelineWriter,
        ILogger<DurationEstimatorService> logger)
    {
        _calculator = calculator;
        _exporter = exporter;
        _pipelineWriter = pipelineWriter;
        _logger = logger;
    }

    public DurationEstimate CalculateEstimate(string activityId, decimal quantity, decimal productivityRate, int crewSize, decimal hoursPerDay)
    {
        _logger.LogInformation("Calculating duration for activity {ActivityId}: Qty={Quantity}, Rate={Rate}, Crew={Crew}, Hours={Hours}",
            activityId, quantity, productivityRate, crewSize, hoursPerDay);

        var estimate = _calculator.Calculate(activityId, quantity, productivityRate, crewSize, hoursPerDay, _calendar, modifiers: _modifiers);
        return estimate;
    }

    public DurationEstimate Recalculate(DurationEstimate estimate, decimal? newRate = null, int? newCrewSize = null, decimal? newHours = null)
    {
        var rate = newRate ?? estimate.AppliedProductivityRate;
        var crew = newCrewSize ?? estimate.CrewSize;
        var hours = newHours ?? estimate.HoursPerDay;

        if (newRate.HasValue)
        {
            _logger.LogInformation("Productivity rate changed for {ActivityId}: {OldRate} -> {NewRate}",
                estimate.ActivityId, estimate.AppliedProductivityRate, newRate);
        }

        var result = _calculator.Calculate(
            estimate.ActivityId, estimate.Quantity, rate, crew, hours, _calendar, estimate.StartDate, _modifiers);

        _logger.LogInformation("Recalculation complete for {ActivityId}: {OldDuration} -> {NewDuration}",
            estimate.ActivityId, estimate.DurationWorkingDays, result.DurationWorkingDays);

        return result;
    }

    public Task<ExportResult> ExportToExcelAsync(IReadOnlyList<DurationEstimate> estimates, string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting {Count} duration estimates to Excel at {Path}", estimates.Count, filePath);
        return _exporter.ExportToExcelAsync(estimates, filePath, ct);
    }

    public Task<string> ExportToPipelineAsync(string projectId, CalendarConfig calendar, IReadOnlyList<DurationEstimate> estimates, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting {Count} duration estimates to scheduling pipeline for project {ProjectId}", estimates.Count, projectId);
        return _pipelineWriter.WriteAsync(projectId, calendar, estimates, ct);
    }

    public void SetCalendar(CalendarConfig calendar)
    {
        calendar.Validate();
        _calendar = calendar;
        _logger.LogInformation("Calendar configuration updated: {WorkingDays} days, {Hours}h/day, {Holidays} holidays",
            string.Join(",", calendar.WorkingDays), calendar.HoursPerDay, calendar.Holidays.Count);
    }

    public void SetModifiers(IEnumerable<ProductivityModifier> modifiers)
    {
        _modifiers = modifiers.ToList();
        _logger.LogInformation("{Count} productivity modifiers applied", _modifiers.Count);
    }
}
