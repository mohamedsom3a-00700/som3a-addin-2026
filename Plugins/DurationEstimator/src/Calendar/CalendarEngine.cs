namespace Som3a.DurationEstimator.Calendar;

public class CalendarEngine : ICalendarEngine
{
    private CalendarConfig _config = new();

    public void Configure(CalendarConfig config)
    {
        config.Validate();
        _config = config;
    }

    public CalendarConfig GetConfig() => _config;

    public DateTime CalculateEndDate(DateTime startDate, decimal workingDays)
    {
        if (workingDays <= 0)
            return startDate;

        var remaining = (int)Math.Ceiling(workingDays);
        var current = startDate;

        if (IsWorkingDay(current))
            remaining--;

        while (remaining > 0)
        {
            current = current.AddDays(1);
            if (IsWorkingDay(current))
                remaining--;
        }

        return current;
    }

    public int CalculateCalendarDays(DateTime startDate, decimal workingDays)
    {
        if (workingDays <= 0)
            return 0;

        var endDate = CalculateEndDate(startDate, workingDays);
        return (endDate - startDate).Days + 1;
    }

    public bool IsWorkingDay(DateTime date)
    {
        if (!_config.WorkingDays.Contains(date.DayOfWeek))
            return false;

        if (_config.Holidays.Contains(date.Date))
            return false;

        return true;
    }

    public int CountWorkingDays(DateTime start, DateTime end)
    {
        if (end < start)
            return 0;

        int count = 0;
        var current = start.Date;

        while (current <= end.Date)
        {
            if (IsWorkingDay(current))
                count++;
            current = current.AddDays(1);
        }

        return count;
    }
}
