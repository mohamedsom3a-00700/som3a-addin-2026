using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public interface IRelationshipGenerationService
    {
        bool IsAIAvailable { get; }

        Task<IReadOnlyList<Relationship>> GenerateRelationshipsAsync(
            IReadOnlyList<ActivityItem> activities,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<Relationship>> RegenerateRelationshipsAsync(
            IReadOnlyList<ActivityItem> activities,
            IReadOnlyList<Relationship> existingRelationships,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default);
    }
}
