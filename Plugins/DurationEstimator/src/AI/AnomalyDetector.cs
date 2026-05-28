namespace Som3a.DurationEstimator.AI;

public interface IAnomalyDetector
{
    List<AnomalyFlag> Analyze(IReadOnlyList<Engine.DurationEstimate> estimates);
}

public record AnomalyFlag
{
    public string ActivityId { get; init; } = string.Empty;
    public AnomalyType Type { get; init; }
    public string Explanation { get; init; } = string.Empty;
    public decimal DeviationFactor { get; init; }
}

public class AnomalyDetector : IAnomalyDetector
{
    public List<AnomalyFlag> Analyze(IReadOnlyList<Engine.DurationEstimate> estimates)
    {
        var flags = new List<AnomalyFlag>();

        if (estimates.Count == 0) return flags;

        var avg = estimates.Average(e => e.DurationWorkingDays);

        foreach (var est in estimates)
        {
            if (est.DurationWorkingDays > avg * 3m && avg > 0)
            {
                flags.Add(new AnomalyFlag
                {
                    ActivityId = est.ActivityId,
                    Type = AnomalyType.TooLong,
                    Explanation = $"Duration ({est.DurationWorkingDays:F1}d) is {est.DurationWorkingDays / avg:F1}x the average ({avg:F1}d).",
                    DeviationFactor = avg > 0 ? est.DurationWorkingDays / avg : 0
                });
            }
            else if (est.DurationWorkingDays < avg * 0.3m && est.DurationWorkingDays > 0 && avg > 0.01m)
            {
                flags.Add(new AnomalyFlag
                {
                    ActivityId = est.ActivityId,
                    Type = AnomalyType.TooShort,
                    Explanation = $"Duration ({est.DurationWorkingDays:F1}d) is significantly lower than average ({avg:F1}d).",
                    DeviationFactor = avg > 0 ? est.DurationWorkingDays / avg : 0
                });
            }
            else if (est.DurationWorkingDays > 365 * 5)
            {
                flags.Add(new AnomalyFlag
                {
                    ActivityId = est.ActivityId,
                    Type = AnomalyType.Impossible,
                    Explanation = "Duration exceeds 5 years — possible data entry error.",
                    DeviationFactor = 0
                });
            }
        }

        return flags;
    }
}
