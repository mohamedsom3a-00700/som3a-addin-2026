using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services.WBS;

public interface IWBSAIService
{
    Task<WBSNode> GenerateWBSAsync(string projectDescription, string? preferredCategory = null, CancellationToken ct = default);
    Task<WBSNode> RegenerateWBSAsync(string projectDescription, string previousTreeJson, CancellationToken ct = default);
    Task<WBSNode> GenerateWBSFromExcelAsync(string excelContext, CancellationToken ct = default);
    bool IsAIAvailable { get; }
}
