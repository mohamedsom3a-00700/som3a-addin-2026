using System.Text.Json;
using Som3a.DurationEstimator.Engine;

namespace Som3a.DurationEstimator.Export;

public interface ISchedulingPipelineWriter
{
    Task<string> WriteAsync(
        string projectId,
        Calendar.CalendarConfig calendar,
        IReadOnlyList<DurationEstimate> estimates,
        CancellationToken ct = default);

    Task WriteToFileAsync(
        string filePath,
        string projectId,
        Calendar.CalendarConfig calendar,
        IReadOnlyList<DurationEstimate> estimates,
        CancellationToken ct = default);
}

public class SchedulingPipelineWriter : ISchedulingPipelineWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<string> WriteAsync(
        string projectId,
        Calendar.CalendarConfig calendar,
        IReadOnlyList<DurationEstimate> estimates,
        CancellationToken ct = default)
    {
        var payload = BuildPayload(projectId, calendar, estimates);
        return await Task.FromResult(JsonSerializer.Serialize(payload, JsonOptions));
    }

    public async Task WriteToFileAsync(
        string filePath,
        string projectId,
        Calendar.CalendarConfig calendar,
        IReadOnlyList<DurationEstimate> estimates,
        CancellationToken ct = default)
    {
        var json = await WriteAsync(projectId, calendar, estimates, ct);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(filePath, json, ct);
    }

    private static object BuildPayload(string projectId, Calendar.CalendarConfig calendar, IReadOnlyList<DurationEstimate> estimates)
    {
        return new
        {
            schemaVersion = "1.0.0",
            generatedAt = DateTime.UtcNow.ToString("O"),
            sourcePlugin = "duration-estimator",
            sourceVersion = "1.0.0",
            projectId,
            calendar = new
            {
                workingDays = calendar.WorkingDays.Select(d => d.ToString()),
                hoursPerDay = calendar.HoursPerDay,
                holidays = calendar.Holidays.Select(h => h.ToString("yyyy-MM-dd")),
                startDate = calendar.StartDate.ToString("yyyy-MM-dd")
            },
            activities = estimates.Select(e => new
            {
                activityId = e.ActivityId,
                quantity = e.Quantity,
                productivityRate = e.AppliedProductivityRate,
                crewSize = e.CrewSize,
                hoursPerDay = e.HoursPerDay,
                durationWorkingDays = e.DurationWorkingDays,
                calendarDurationDays = e.CalendarDurationDays,
                startDate = e.StartDate?.ToString("yyyy-MM-dd"),
                endDate = e.EndDate?.ToString("yyyy-MM-dd"),
                modifiers = e.AppliedModifiers.Select(m => new
                {
                    type = m.ModifierType.ToString().ToLowerInvariant(),
                    percentage = m.Percentage,
                    description = m.Description
                }),
                variance = e.OptimisticDuration.HasValue ? new
                {
                    optimistic = e.OptimisticDuration,
                    mostLikely = e.MostLikelyDuration,
                    pessimistic = e.PessimisticDuration,
                    standardDeviation = e.StandardDeviation,
                    confidence95Lower = e.ConfidenceInterval95Lower,
                    confidence95Upper = e.ConfidenceInterval95Upper
                } : null,
                isAnomaly = e.IsAnomaly,
                anomalyReason = e.AnomalyReason
            }),
            metadata = new
            {
                totalActivities = estimates.Count,
                totalDurationWorkingDays = estimates.Sum(e => e.DurationWorkingDays),
                anomalyCount = estimates.Count(e => e.IsAnomaly)
            }
        };
    }
}
