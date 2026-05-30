namespace Som3a.Infrastructure.Persistence.Models;

public class ExportHistoryRecord
{
    public Guid Id { get; set; }
    public string Format { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int FileSize { get; set; }
    public int DurationMs { get; set; }
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
    public DateTime ExportedAt { get; set; }
}
