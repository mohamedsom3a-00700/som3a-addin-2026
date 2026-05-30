namespace Som3a.Infrastructure.Persistence;

public class DataRetentionConfiguration
{
    public TimeSpan AIHistoryRetention { get; set; } = TimeSpan.FromDays(365);

    public TimeSpan DiagnosticsLogRetention { get; set; } = TimeSpan.FromDays(90);

    public TimeSpan CrashReportRetention { get; set; } = TimeSpan.FromDays(730);

    public TimeSpan ExportHistoryRetention { get; set; } = TimeSpan.FromDays(365);

    public int BatchDeleteSize { get; set; } = 500;
}
