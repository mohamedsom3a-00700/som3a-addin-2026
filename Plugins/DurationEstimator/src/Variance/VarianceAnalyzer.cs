namespace Som3a.DurationEstimator.Variance;

public interface IVarianceAnalyzer
{
    VarianceResult CalculateThreePoint(decimal optimisticRate, decimal mostLikelyRate, decimal pessimisticRate,
        decimal quantity, int crewSize, decimal hoursPerDay);
    VarianceResult CalculateFromSingle(decimal singleRate, decimal quantity, int crewSize, decimal hoursPerDay);
}

public record VarianceResult
{
    public decimal OptimisticDuration { get; init; }
    public decimal MostLikelyDuration { get; init; }
    public decimal PessimisticDuration { get; init; }
    public decimal ExpectedDuration { get; init; }
    public decimal StandardDeviation { get; init; }
    public decimal Confidence90Lower { get; init; }
    public decimal Confidence90Upper { get; init; }
    public decimal Confidence95Lower { get; init; }
    public decimal Confidence95Upper { get; init; }
    public decimal Confidence99Lower { get; init; }
    public decimal Confidence99Upper { get; init; }
    public bool HasSingleRate { get; init; }
}

public class VarianceAnalyzer : IVarianceAnalyzer
{
    private const decimal Z90 = 1.645m;
    private const decimal Z95 = 1.96m;
    private const decimal Z99 = 2.576m;

    private static decimal CalcDuration(decimal rate, decimal quantity, int crewSize, decimal hoursPerDay)
    {
        return quantity / (rate * crewSize * hoursPerDay);
    }

    public VarianceResult CalculateThreePoint(decimal optimisticRate, decimal mostLikelyRate, decimal pessimisticRate,
        decimal quantity, int crewSize, decimal hoursPerDay)
    {
        if (pessimisticRate <= 0 || mostLikelyRate <= 0 || optimisticRate <= 0)
            throw new ArgumentException("All productivity rates must be > 0.");

        if (optimisticRate < mostLikelyRate || mostLikelyRate < pessimisticRate)
            throw new ArgumentException("Optimistic rate (highest) >= Most Likely >= Pessimistic (lowest).");

        var od = CalcDuration(optimisticRate, quantity, crewSize, hoursPerDay);
        var md = CalcDuration(mostLikelyRate, quantity, crewSize, hoursPerDay);
        var pd = CalcDuration(pessimisticRate, quantity, crewSize, hoursPerDay);

        var expected = (od + 4m * md + pd) / 6m;
        var sd = (pd - od) / 6m;

        return new VarianceResult
        {
            OptimisticDuration = Math.Round(od, 2),
            MostLikelyDuration = Math.Round(md, 2),
            PessimisticDuration = Math.Round(pd, 2),
            ExpectedDuration = Math.Round(expected, 2),
            StandardDeviation = Math.Round(sd, 2),
            Confidence90Lower = Math.Round(expected - Z90 * sd, 2),
            Confidence90Upper = Math.Round(expected + Z90 * sd, 2),
            Confidence95Lower = Math.Round(expected - Z95 * sd, 2),
            Confidence95Upper = Math.Round(expected + Z95 * sd, 2),
            Confidence99Lower = Math.Round(expected - Z99 * sd, 2),
            Confidence99Upper = Math.Round(expected + Z99 * sd, 2),
            HasSingleRate = false
        };
    }

    public VarianceResult CalculateFromSingle(decimal singleRate, decimal quantity, int crewSize, decimal hoursPerDay)
    {
        var d = CalcDuration(singleRate, quantity, crewSize, hoursPerDay);

        return new VarianceResult
        {
            OptimisticDuration = Math.Round(d, 2),
            MostLikelyDuration = Math.Round(d, 2),
            PessimisticDuration = Math.Round(d, 2),
            ExpectedDuration = Math.Round(d, 2),
            StandardDeviation = 0,
            Confidence90Lower = Math.Round(d, 2),
            Confidence90Upper = Math.Round(d, 2),
            Confidence95Lower = Math.Round(d, 2),
            Confidence95Upper = Math.Round(d, 2),
            Confidence99Lower = Math.Round(d, 2),
            Confidence99Upper = Math.Round(d, 2),
            HasSingleRate = true
        };
    }
}
