using System.Collections.Concurrent;
using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Prompts;

public class PromptTemplateRegistry
{
    private readonly string _storagePath;
    private readonly ConcurrentDictionary<string, EnhancedPromptTemplate> _templates = new();
    private readonly object _diskLock = new();

    public PromptTemplateRegistry(string? storagePath = null)
    {
        _storagePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "templates");

        Directory.CreateDirectory(_storagePath);
        LoadFromDisk();
    }

    public Task<EnhancedPromptTemplate> GetAsync(string templateId)
    {
        if (_templates.TryGetValue(templateId, out var template))
            return Task.FromResult(Clone(template));
        throw new KeyNotFoundException($"Prompt template '{templateId}' not found.");
    }

    public Task<List<EnhancedPromptTemplate>> ListAsync(string? category = null, TemplateLifecycleState? state = null)
    {
        var query = _templates.Values.AsEnumerable();

        if (category != null)
            query = query.Where(t => t.Category == category);
        if (state.HasValue)
            query = query.Where(t => t.LifecycleState == state.Value);

        return Task.FromResult(query.OrderBy(t => t.Name).Select(Clone).ToList());
    }

    public async Task<EnhancedPromptTemplate> CreateAsync(CreateTemplateRequest request, string actorId)
    {
        var template = new EnhancedPromptTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Category = request.Category,
            SystemPrompt = request.SystemPrompt,
            UserPrompt = request.UserPrompt,
            JsonSchema = request.JsonSchema,
            ContextRequirements = new List<string>(request.ContextRequirements ?? Enumerable.Empty<string>()),
            OwnershipScope = request.OwnershipScope,
            OwnerId = request.OwnershipScope == TemplateOwnership.Personal ? actorId : null,
            LifecycleState = TemplateLifecycleState.Draft,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };

        _templates[template.Id] = template;
        await SaveToDiskAsync(template);
        return Clone(template);
    }

    public async Task<EnhancedPromptTemplate> UpdateAsync(string templateId, UpdateTemplateRequest request, string actorId)
    {
        if (!_templates.TryGetValue(templateId, out var template))
            throw new KeyNotFoundException($"Prompt template '{templateId}' not found.");

        lock (_diskLock)
        {
            if (template.LifecycleState == TemplateLifecycleState.Published)
            {
                template.Version++;
                template.LifecycleState = TemplateLifecycleState.Draft;
            }

            if (request.Name != null) template.Name = request.Name;
            if (request.SystemPrompt != null) template.SystemPrompt = request.SystemPrompt;
            if (request.UserPrompt != null) template.UserPrompt = request.UserPrompt;
            if (request.JsonSchema != null) template.JsonSchema = request.JsonSchema;
            if (request.ContextRequirements != null) template.ContextRequirements = request.ContextRequirements.ToList();
            template.UpdatedAt = DateTime.UtcNow;
        }

        await SaveToDiskAsync(template);
        return Clone(template);
    }

    public async Task<EnhancedPromptTemplate> PublishAsync(string templateId, string actorId)
    {
        if (!_templates.TryGetValue(templateId, out var template))
            throw new KeyNotFoundException($"Prompt template '{templateId}' not found.");

        lock (_diskLock)
        {
            template.LifecycleState = TemplateLifecycleState.Published;
            template.UpdatedAt = DateTime.UtcNow;
        }

        await SaveToDiskAsync(template);
        return Clone(template);
    }

    public async Task<EnhancedPromptTemplate> DeprecateAsync(string templateId, string actorId)
    {
        if (!_templates.TryGetValue(templateId, out var template))
            throw new KeyNotFoundException($"Prompt template '{templateId}' not found.");

        lock (_diskLock)
        {
            template.LifecycleState = TemplateLifecycleState.Deprecated;
            template.DeprecatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
        }

        await SaveToDiskAsync(template);
        return Clone(template);
    }

    private void LoadFromDisk()
    {
        if (!Directory.Exists(_storagePath)) return;

        foreach (var file in Directory.GetFiles(_storagePath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<EnhancedPromptTemplate>(json);
                if (template != null && !string.IsNullOrEmpty(template.Id))
                    _templates[template.Id] = template;
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Cannot read template file {file}.", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Corrupt template file {file}.", ex);
            }
        }
    }

    private async Task SaveToDiskAsync(EnhancedPromptTemplate template)
    {
        var path = Path.Combine(_storagePath, $"{template.Id}.json");
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    private static EnhancedPromptTemplate Clone(EnhancedPromptTemplate source)
    {
        return new EnhancedPromptTemplate
        {
            Id = source.Id,
            Name = source.Name,
            Category = source.Category,
            SystemPrompt = source.SystemPrompt,
            UserPrompt = source.UserPrompt,
            JsonSchema = source.JsonSchema,
            ContextRequirements = new List<string>(source.ContextRequirements),
            LifecycleState = source.LifecycleState,
            Version = source.Version,
            OwnershipScope = source.OwnershipScope,
            OwnerId = source.OwnerId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            DeprecatedAt = source.DeprecatedAt,
            CreatedBy = source.CreatedBy
        };
    }
}

public class EnhancedPromptTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string? JsonSchema { get; set; }
    public List<string> ContextRequirements { get; set; } = new();
    public TemplateLifecycleState LifecycleState { get; set; } = TemplateLifecycleState.Draft;
    public int Version { get; set; } = 1;
    public TemplateOwnership OwnershipScope { get; set; } = TemplateOwnership.System;
    public string? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeprecatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public enum TemplateLifecycleState { Draft, Published, Deprecated }
public enum TemplateOwnership { System, Personal }

public record CreateTemplateRequest(
    string Name,
    string Category,
    string SystemPrompt,
    string UserPrompt,
    string? JsonSchema,
    List<string>? ContextRequirements,
    TemplateOwnership OwnershipScope);

public record UpdateTemplateRequest(
    string? Name,
    string? SystemPrompt,
    string? UserPrompt,
    string? JsonSchema,
    string[]? ContextRequirements);
