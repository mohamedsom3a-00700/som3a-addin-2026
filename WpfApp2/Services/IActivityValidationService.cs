using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public interface IActivityValidationService
    {
        Task<IReadOnlyList<ActivityValidationResult>> ValidateAsync(
            IReadOnlyList<GeneratedActivity> activities,
            BOQContext context,
            CancellationToken ct = default);

        ActivityValidationResult ValidateSingle(
            GeneratedActivity activity,
            IReadOnlyList<GeneratedActivity> allActivities,
            BOQContext context);
    }
}
