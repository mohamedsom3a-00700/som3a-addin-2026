using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public interface IActivityGenerationService
    {
        Task<IReadOnlyList<GeneratedActivity>> GenerateActivitiesAsync(
            BOQContext context,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<GeneratedActivity>> RegenerateActivitiesAsync(
            BOQContext context,
            IReadOnlyList<GeneratedActivity> previousActivities,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default);

        bool IsAIAvailable { get; }
    }
}
