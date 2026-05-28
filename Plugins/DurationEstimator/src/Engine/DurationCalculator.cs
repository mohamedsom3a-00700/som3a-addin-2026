using Som3a.DurationEstimator.Calendar;

namespace Som3a.DurationEstimator.Engine;

public interface IDurationCalculator
{
    DurationEstimate Calculate(
        string activityId,
        decimal quantity,
        decimal productivityRate,
        int crewSize,
        decimal hoursPerDay,
        CalendarConfig? calendar = null,
        DateTime? startDate = null,
        IEnumerable<ProductivityModifier>? modifiers = null
    );
}

public class DurationCalculator : IDurationCalculator
{
    private readonly IProductivityEngine _productivityEngine;
    private readonly ICalendarEngine _calendarEngine;

    public DurationCalculator(IProductivityEngine productivityEngine, ICalendarEngine calendarEngine)
    {
        _productivityEngine = productivityEngine;
        _calendarEngine = calendarEngine;
    }

    public DurationEstimate Calculate(
        string activityId,
        decimal quantity,
        decimal productivityRate,
        int crewSize,
        decimal hoursPerDay,
        CalendarConfig? calendar = null,
        DateTime? startDate = null,
        IEnumerable<ProductivityModifier>? modifiers = null)
    {
        decimal effectiveRate = productivityRate;
        if (modifiers != null && modifiers.Any())
        {
            effectiveRate = _productivityEngine.ApplyModifiers(productivityRate, modifiers);
        }

        decimal durationWorkingDays = _productivityEngine.CalculateWorkingDays(
            quantity, effectiveRate, crewSize, hoursPerDay);

        var estimate = new DurationEstimate
        {
            ActivityId = activityId,
            Quantity = quantity,
            AppliedProductivityRate = effectiveRate,
            CrewSize = crewSize,
            HoursPerDay = hoursPerDay,
            DurationWorkingDays = durationWorkingDays,
            LastCalculated = DateTime.UtcNow,
            CalculationVersion = "1.0.0"
        };

        if (modifiers != null)
        {
            estimate.AppliedModifiers = modifiers.ToList();
        }

        if (calendar != null)
        {
            _calendarEngine.Configure(calendar);
            var effectiveStartDate = startDate ?? calendar.StartDate;
            estimate.StartDate = effectiveStartDate;

            if (durationWorkingDays > 0)
            {
                var endDate = _calendarEngine.CalculateEndDate(effectiveStartDate, durationWorkingDays);
                estimate.EndDate = endDate;
                estimate.CalendarDurationDays = _calendarEngine.CalculateCalendarDays(effectiveStartDate, durationWorkingDays);
            }
        }

        return estimate;
    }
}
