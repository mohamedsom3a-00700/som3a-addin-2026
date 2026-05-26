namespace Som3a.AI.Configuration;

public class ProviderConfig
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? ApiKeyEncrypted { get; set; }
    public int RateLimitRpm { get; set; } = 30;
    public int PriorityOrder { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
}

public enum ProviderHealthStatus
{
    NotConfigured,
    Connected,
    Failed,
    Degraded
}
