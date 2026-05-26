# Contract: IPromptTemplateRegistry

**Layer**: Som3a.AI.Prompts
**Purpose**: Template catalog — manages lifecycle, categories, versioning, ownership, and validation of prompt templates.

```csharp
namespace Som3a.AI.Prompts;

public interface IPromptTemplateRegistry
{
    Task<PromptTemplate> GetAsync(string templateId);
    
    Task<IReadOnlyList<PromptTemplateSummary>> ListAsync(
        PromptCategory? category = null,
        TemplateLifecycleState? state = null);
    
    Task<PromptTemplate> CreateAsync(CreateTemplateRequest request, string actorId);
    
    Task<PromptTemplate> UpdateAsync(string templateId, UpdateTemplateRequest request, string actorId);
    
    Task<PromptTemplate> PublishAsync(string templateId, string actorId);
    
    Task<PromptTemplate> DeprecateAsync(string templateId, string actorId);
    
    Task<PromptTemplate> UnpublishAsync(string templateId, string actorId);
    
    Task<bool> ValidateSyntaxAsync(string systemPrompt, string userPrompt, JsonDocument outputSchema);
}

public record PromptTemplateSummary(
    string Id,
    string Name,
    PromptCategory Category,
    TemplateLifecycleState LifecycleState,
    int Version,
    DateTime UpdatedAt);

public record CreateTemplateRequest(
    string Name,
    PromptCategory Category,
    string SystemPrompt,
    string UserPrompt,
    JsonDocument OutputSchema,
    string[] ContextRequirements,
    TemplateOwnership OwnershipScope);

public record UpdateTemplateRequest(
    string? Name,
    string? SystemPrompt,
    string? UserPrompt,
    JsonDocument? OutputSchema,
    string[]? ContextRequirements);
```
