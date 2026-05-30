namespace Som3a.Infrastructure.Persistence.Models;

public class PluginRecord
{
    public Guid Id { get; set; }
    public string PluginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Dependencies { get; set; } = "[]";
    public bool IsEnabled { get; set; } = true;
    public string HealthStatus { get; set; } = "Unknown";
    public string? HealthMessage { get; set; }
    public string Settings { get; set; } = "{}";
    public DateTime InstalledAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
