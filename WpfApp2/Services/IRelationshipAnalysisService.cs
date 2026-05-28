using Som3a_WPF_UI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public interface IRelationshipAnalysisService
    {
        Task<IReadOnlyList<ParallelExecutionGroup>> AnalyzeParallelGroupsAsync(
            RelationshipNetwork network,
            CancellationToken ct = default);

        Task<CriticalPathResult> AnalyzeCriticalPathAsync(
            RelationshipNetwork network,
            CancellationToken ct = default);

        Task<IReadOnlyList<ResourceConflict>> DetectResourceConflictsAsync(
            RelationshipNetwork network,
            CancellationToken ct = default);
    }
}
