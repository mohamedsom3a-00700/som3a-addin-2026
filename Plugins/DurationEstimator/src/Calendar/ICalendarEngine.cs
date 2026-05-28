namespace Som3a.DurationEstimator.Calendar;

public interface ICalendarEngine
{
    void Configure(CalendarConfig config);
    CalendarConfig GetConfig();
    int CalculateCalendarDays(DateTime startDate, decimal workingDays);
    DateTime CalculateEndDate(DateTime startDate, decimal workingDays);
    bool IsWorkingDay(DateTime date);
    int CountWorkingDays(DateTime start, DateTime end);
}
