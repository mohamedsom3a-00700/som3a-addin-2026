namespace Som3a.Infrastructure.Persistence.Models;

public class DiagnosticsLog
{
    public Guid Id { get; set; }
    public string Severity { get; set; } = "Info";
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? PlatformState { get; set; }
    public DateTime LoggedAt { get; set; }
}
