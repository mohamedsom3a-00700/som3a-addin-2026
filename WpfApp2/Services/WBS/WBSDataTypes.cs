using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int Level
    {
        get
        {
            var level = 0;
            var current = Parent;
            while (current != null) { level++; current = current.Parent; }
            return level;
        }
    }

    public WBSNode? Parent { get; set; }
    public List<WBSNode> Children { get; set; } = new();

    public string FullPath
    {
        get { return Code; }
    }
}

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
