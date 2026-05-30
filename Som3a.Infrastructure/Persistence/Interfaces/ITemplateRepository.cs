using Som3a.Infrastructure.Persistence.Models;

namespace Som3a.Infrastructure.Persistence.Interfaces;

public interface ITemplateRepository
{
    Task AddTemplateAsync(TemplateRecord template, CancellationToken ct = default);

    Task UpdateTemplateAsync(TemplateRecord template, CancellationToken ct = default);

    Task<TemplateRecord?> GetTemplateAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAsync(string templateType, CancellationToken ct = default);

    Task<IReadOnlyList<TemplateRecord>> GetTemplatesByTypeAndCategoryAsync(string templateType, string category, CancellationToken ct = default);

    Task<IReadOnlyList<TemplateRecord>> GetDefaultTemplatesAsync(string templateType, CancellationToken ct = default);

    Task DeleteTemplateAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TemplateRecord>> SearchTemplatesAsync(string query, string? templateType = null, CancellationToken ct = default);
}
