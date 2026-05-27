using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public interface IActivitySequencingService
    {
        Task<IReadOnlyList<ActivitySequenceOrder>> SequenceAsync(
            IReadOnlyList<GeneratedActivity> activities,
            CancellationToken ct = default);

        Task<IReadOnlyList<ActivityDependency>> SuggestDependenciesAsync(
            IReadOnlyList<GeneratedActivity> activities,
            CancellationToken ct = default);
    }
}
