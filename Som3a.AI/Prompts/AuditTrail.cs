using System.Text.Json;

namespace Som3a.AI.Prompts;

public class AuditTrail
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public AuditTrail(string? logPath = null)
    {
        _logPath = logPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "audit", "templates.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
    }

    public void Record(string entityType, string entityId, string action, string actorId, object? previousState = null)
    {
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorId = actorId,
            Timestamp = DateTime.UtcNow,
            PreviousState = previousState != null ? JsonSerializer.Serialize(previousState) : null
        };

        lock (_lock)
        {
            var entries = LoadEntries();
            entries.Add(entry);
            SaveEntries(entries);
        }
    }

    public List<AuditEntry> GetHistory(string? entityId = null)
    {
        lock (_lock)
        {
            var entries = LoadEntries();
            if (entityId != null)
                entries = entries.Where(e => e.EntityId == entityId).ToList();
            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }
    }

    private List<AuditEntry> LoadEntries()
    {
        if (!File.Exists(_logPath)) return new();
        try
        {
            var json = File.ReadAllText(_logPath);
            return JsonSerializer.Deserialize<List<AuditEntry>>(json) ?? new();
        }
        catch { return new(); }
    }

    private void SaveEntries(List<AuditEntry> entries)
    {
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_logPath, json);
    }
}

public class AuditEntry
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? PreviousState { get; set; }
}
