namespace Som3a.DurationEstimator.Engine;

public static class InputValidation
{
    public static ValidationResult ValidateQuantity(decimal quantity)
    {
        if (quantity < 0)
            return ValidationResult.Invalid("Quantity must be >= 0.");
        if (quantity == 0)
            return ValidationResult.Invalid("Quantity is zero. Duration cannot be calculated.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateProductivityRate(decimal rate)
    {
        if (rate <= 0)
            return ValidationResult.Invalid("Productivity rate must be > 0.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateCrewSize(int crewSize)
    {
        if (crewSize < 1)
            return ValidationResult.Invalid("Crew size must be >= 1.");
        if (crewSize > 50)
            return ValidationResult.Invalid("Crew size must be <= 50.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateHoursPerDay(decimal hours)
    {
        if (hours <= 0 || hours > 24)
            return ValidationResult.Invalid("Hours per day must be > 0 and <= 24.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateAll(decimal quantity, decimal rate, int crewSize, decimal hoursPerDay)
    {
        var q = ValidateQuantity(quantity);
        if (!q.IsValid) return q;

        var r = ValidateProductivityRate(rate);
        if (!r.IsValid) return r;

        var c = ValidateCrewSize(crewSize);
        if (!c.IsValid) return c;

        var h = ValidateHoursPerDay(hoursPerDay);
        if (!h.IsValid) return h;

        return ValidationResult.Valid();
    }

    public static bool IsExcessiveDuration(decimal durationWorkingDays)
    {
        return durationWorkingDays > 365 * 5;
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ValidationResult Valid() => new() { IsValid = true };
    public static ValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
}
