namespace Som3a.DurationEstimator.Variance;

public interface IRiskAdjuster
{
    decimal GetRiskAdjustedDuration(VarianceResult variance, decimal confidenceLevel);
    string GetConfidenceLabel(decimal confidenceLevel);
}

public class RiskAdjuster : IRiskAdjuster
{
    public decimal GetRiskAdjustedDuration(VarianceResult variance, decimal confidenceLevel)
    {
        if (variance.HasSingleRate || variance.StandardDeviation == 0)
            return variance.ExpectedDuration;

        return confidenceLevel switch
        {
            >= 99m => variance.Confidence99Upper,
            >= 95m => variance.Confidence95Upper,
            >= 90m => variance.Confidence90Upper,
            _ => variance.ExpectedDuration
        };
    }

    public string GetConfidenceLabel(decimal confidenceLevel)
    {
        return confidenceLevel switch
        {
            >= 99m => "Very High (99%)",
            >= 95m => "High (95%)",
            >= 90m => "Moderate (90%)",
            _ => "Expected"
        };
    }
}
