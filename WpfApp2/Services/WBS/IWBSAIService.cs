using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public interface IWBSAIService
{
    Task<WBSNode> GenerateWBSAsync(string projectDescription, string? preferredCategory = null, CancellationToken ct = default);
    Task<WBSNode> RegenerateWBSAsync(string projectDescription, string previousTreeJson, CancellationToken ct = default);
    bool IsAIAvailable { get; }
}
