namespace Som3a.Domain.Calendars
{
    public class Calendar
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public HashSet<DayOfWeek> WorkDays { get; set; } = new();
        public List<DateTime> Holidays { get; set; } = new();
        public decimal WorkingHoursPerDay { get; set; } = 8;
        public string? TimeZone { get; set; }

        public void Validate()
        {
            if (WorkDays.Count < 1)
                throw new InvalidOperationException("Calendar must have at least 1 working day.");
            if (WorkingHoursPerDay <= 0 || WorkingHoursPerDay > 24)
                throw new InvalidOperationException(
                    "WorkingHoursPerDay must be > 0 and <= 24.");
        }
    }
}
