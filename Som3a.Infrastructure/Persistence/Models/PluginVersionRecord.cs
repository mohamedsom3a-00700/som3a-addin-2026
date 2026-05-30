namespace Som3a.Infrastructure.Persistence.Models;

public class PluginVersionRecord
{
    public Guid Id { get; set; }
    public Guid PluginRecordId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; }
    public string InstalledBy { get; set; } = "User";
}
