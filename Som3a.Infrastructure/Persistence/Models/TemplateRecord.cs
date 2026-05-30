namespace Som3a.Infrastructure.Persistence.Models;

public class TemplateRecord
{
    public Guid Id { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = "{}";
    public int Version { get; set; } = 1;
    public bool IsDefault { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
