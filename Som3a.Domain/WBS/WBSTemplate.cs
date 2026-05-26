namespace Som3a.Domain.WBS;

public class WBSTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public WBSNode? RootNode { get; set; }
    public int Version { get; set; } = 1;
    public bool IsSystem { get; set; } = true;
    public string? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("WBSTemplate Name must not be empty.");
        if (string.IsNullOrWhiteSpace(Category))
            throw new InvalidOperationException("WBSTemplate Category must not be empty.");
        if (RootNode == null)
            throw new InvalidOperationException("WBSTemplate RootNode must not be null.");
    }
}
