# Contract: ITemplateRepository

**Namespace**: `Som3a.Infrastructure.Persistence.Interfaces`

## Purpose

Manages WBS templates, activity templates, relationship templates, and productivity benchmarks.

## Interface

```csharp
public interface ITemplateRepository
{
    /// Creates a new template.
    Task AddTemplateAsync(TemplateRecord template, CancellationToken ct = default);

    /// Updates an existing template (matched by Id).
    Task UpdateTemplateAsync(TemplateRecord template, CancellationToken ct = default);

    /// Retrieves a template by ID.
    Task<TemplateRecord?> GetTemplateAsync(Guid id, CancellationToken ct = default);

    /// Retrieves all templates of a given type (e.g. all WBS templates).
    Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAsync(string templateType, CancellationToken ct = default);

    /// Retrieves templates by type and category (e.g. all residential WBS templates).
    Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAndCategoryAsync(string templateType, string category, CancellationToken ct = default);

    /// Retrieves built-in default templates.
    Task<IReadOnlyList<TemplateRecord>> GetDefaultTemplatesAsync(string templateType, CancellationToken ct = default);

    /// Deletes a template by ID.
    Task DeleteTemplateAsync(Guid id, CancellationToken ct = default);

    /// Searches templates by name (partial match).
    Task<IReadOnlyList<TemplateRecord>> SearchTemplatesAsync(string query, string? templateType = null, CancellationToken ct = default);
}
```
