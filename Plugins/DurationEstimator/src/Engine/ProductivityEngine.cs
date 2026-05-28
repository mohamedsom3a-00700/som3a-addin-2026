namespace Som3a.DurationEstimator.Engine;

public interface IProductivityEngine
{
    decimal CalculateWorkingDays(decimal quantity, decimal productivityRate, int crewSize, decimal hoursPerDay);
    decimal ApplyModifiers(decimal baseRate, IEnumerable<ProductivityModifier> modifiers);
}

public class ProductivityEngine : IProductivityEngine
{
    public decimal CalculateWorkingDays(decimal quantity, decimal productivityRate, int crewSize, decimal hoursPerDay)
    {
        if (quantity == 0m)
            return 0m;

        if (quantity < 0m)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be >= 0.");

        if (productivityRate <= 0m)
            throw new ArgumentOutOfRangeException(nameof(productivityRate), "Productivity rate must be > 0.");

        if (crewSize < 1)
            throw new ArgumentOutOfRangeException(nameof(crewSize), "Crew size must be >= 1.");

        if (hoursPerDay <= 0m || hoursPerDay > 24m)
            throw new ArgumentOutOfRangeException(nameof(hoursPerDay), "Hours per day must be > 0 and <= 24.");

        decimal dailyOutput = productivityRate * crewSize * hoursPerDay;
        return quantity / dailyOutput;
    }

    public decimal ApplyModifiers(decimal baseRate, IEnumerable<ProductivityModifier> modifiers)
    {
        if (baseRate <= 0m)
            throw new ArgumentOutOfRangeException(nameof(baseRate), "Base rate must be > 0.");

        decimal totalAdjustment = 0m;
        foreach (var modifier in modifiers)
        {
            totalAdjustment += modifier.Percentage;
        }

        decimal effectiveRate = baseRate * (1m + totalAdjustment / 100m);

        if (effectiveRate <= 0m)
            effectiveRate = baseRate * 0.001m;

        return effectiveRate;
    }
}
