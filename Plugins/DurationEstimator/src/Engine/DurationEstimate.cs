namespace Som3a.DurationEstimator.Engine;

public class DurationEstimate
{
    public string ActivityId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AppliedProductivityRate { get; set; }
    public int CrewSize { get; set; }
    public decimal HoursPerDay { get; set; }
    public decimal DurationWorkingDays { get; set; }
    public int CalendarDurationDays { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? OptimisticDuration { get; set; }
    public decimal? MostLikelyDuration { get; set; }
    public decimal? PessimisticDuration { get; set; }
    public decimal? StandardDeviation { get; set; }
    public decimal? ConfidenceInterval95Lower { get; set; }
    public decimal? ConfidenceInterval95Upper { get; set; }
    public List<ProductivityModifier> AppliedModifiers { get; set; } = new();
    public bool IsAnomaly { get; set; }
    public string? AnomalyReason { get; set; }
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
    public string CalculationVersion { get; set; } = "1.0.0";
}
