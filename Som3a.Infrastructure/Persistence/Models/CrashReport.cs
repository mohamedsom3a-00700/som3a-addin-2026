namespace Som3a.Infrastructure.Persistence.Models;

public class CrashReport
{
    public Guid Id { get; set; }
    public string LastOperation { get; set; } = string.Empty;
    public int MemoryUsageMb { get; set; }
    public string? ThreadState { get; set; }
    public string? ExcelInteropStatus { get; set; }
    public string? CrashDump { get; set; }
    public DateTime LoggedAt { get; set; }
}
