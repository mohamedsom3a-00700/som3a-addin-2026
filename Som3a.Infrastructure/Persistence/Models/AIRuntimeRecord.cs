namespace Som3a.Infrastructure.Persistence.Models;

public class AIRuntimeRecord
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int TokenInputTotal { get; set; }
    public int TokenOutputTotal { get; set; }
    public int OperationCount { get; set; }
    public double EstimatedCost { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
