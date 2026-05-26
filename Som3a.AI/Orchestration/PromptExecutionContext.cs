using System.Text.Json;

namespace Som3a.AI.Orchestration;

public class PromptExecutionContext
{
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public JsonDocument? ContextSnapshot { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Queued;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public int? TokensPrompt { get; set; }
    public int? TokensCompletion { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public List<string> FallbackChain { get; set; } = new();
}

public enum ExecutionStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}
