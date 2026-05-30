namespace Som3a.Infrastructure.Persistence.Models;

public class AIExecutionRecord
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
    public int TokenInput { get; set; }
    public int TokenOutput { get; set; }
    public int DurationMs { get; set; }
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public string? PluginId { get; set; }
    public DateTime ExecutedAt { get; set; }
}
