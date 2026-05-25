# Contract: IPromptProvider

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the contract for the prompt governance system — managing AI prompt templates, building context from domain entities, and validating prompt structure before execution.

## Interface

```csharp
public interface IPromptProvider
{
    Task<PromptTemplate> GetTemplateAsync(string templateId, CancellationToken ct = default);
    Task<string> BuildContextAsync<T>(T entity, CancellationToken ct = default) where T : class;
    ValidationResult ValidatePrompt(PromptTemplate template, Dictionary<string, string> parameters);
    Task<IReadOnlyList<PromptTemplate>> ListTemplatesAsync(string? category = null);
}
```

## Prompt Template

```csharp
public class PromptTemplate
{
    public string Id { get; set; }
    public string Category { get; set; }          // "BOQ", "WBS", "Logic", "Productivity", "Validation", "Review"
    public string Version { get; set; }
    public string SystemPrompt { get; set; }      // System-level instruction
    public string UserPromptTemplate { get; set; } // Mustache-style template with {{parameters}}
    public string? JsonSchema { get; set; }        // Expected output schema
    public int MaxTokens { get; set; } = 4096;
    public float Temperature { get; set; } = 0.3f;
    public List<string>? Examples { get; set; }   // Few-shot examples
}

public class PromptContext
{
    public Dictionary<string, object> EntityData { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
    public int EstimatedTokenCount { get; set; }
}
```

## Template Categories

| Category | Purpose | Example Template |
|----------|---------|-----------------|
| BOQ | Activity generation from BOQ items | "Given this BOQ section, generate construction activities..." |
| WBS | WBS structure creation | "Create a WBS for a {{projectType}} project..." |
| Logic | Relationship generation | "Analyze these activities and suggest predecessor/successor links..." |
| Productivity | Duration estimation | "Based on industry benchmarks, estimate productivity for {{trade}}..." |
| Validation | Error checking | "Review this schedule and identify logical errors..." |
| Review | Schedule analysis | "Analyze this project plan and provide recommendations..." |

## Context Building

`BuildContextAsync<T>` converts domain entities into structured prompt context:
- BOQDocument → hierarchical BOQ summary with key items
- List<Activity> → activity table with IDs, names, quantities
- WBSNode → tree structure with depth and paths
- Relationship list → predecessor/successor matrix

## Validation

`ValidatePrompt` checks:
- All `{{parameters}}` in template have matching entries in the parameters dictionary.
- No missing required parameters.
- Estimated token count fits within `MaxTokens`.
- JSON Schema (if provided) is valid.
