namespace Som3a.DurationEstimator.Calendar;

public class CalendarConfig
{
    public List<DayOfWeek> WorkingDays { get; set; } = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
    public List<DateTime> Holidays { get; set; } = new();
    public decimal HoursPerDay { get; set; } = 8m;
    public DateTime StartDate { get; set; } = DateTime.Today;

    public void Validate()
    {
        if (WorkingDays.Count < 1)
            throw new InvalidOperationException("Calendar must have at least 1 working day.");

        if (HoursPerDay <= 0m || HoursPerDay > 24m)
            throw new InvalidOperationException("HoursPerDay must be > 0 and <= 24.");
    }
}
