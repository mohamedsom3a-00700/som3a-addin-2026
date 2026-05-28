namespace Som3a.DurationEstimator.Engine;

public class CrewSizeFactor
{
    private static readonly Dictionary<int, decimal> EfficiencyFactors = new()
    {
        { 1, 1.00m },
        { 2, 0.95m },
        { 3, 0.90m },
        { 4, 0.85m },
        { 5, 0.82m },
        { 6, 0.78m },
        { 7, 0.75m },
        { 8, 0.72m },
        { 9, 0.70m },
        { 10, 0.68m },
    };

    public static decimal GetEfficiencyFactor(int crewSize)
    {
        if (crewSize < 1)
            throw new ArgumentOutOfRangeException(nameof(crewSize), "Crew size must be >= 1.");

        if (crewSize > 50)
            throw new ArgumentOutOfRangeException(nameof(crewSize), "Crew size must be <= 50.");

        if (EfficiencyFactors.TryGetValue(crewSize, out var factor))
            return factor;

        return 0.68m - (crewSize - 10) * 0.01m;
    }

    public static decimal GetEffectiveCrewSize(int crewSize)
    {
        return crewSize * GetEfficiencyFactor(crewSize);
    }
}
