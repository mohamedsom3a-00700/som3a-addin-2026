# Contract: IActivityGenerationService

**Layer**: WpfApp2.Services
**Purpose**: AI-powered generation of construction activities from BOQ context via Phase 18 orchestration engine.

```csharp
namespace WpfApp2.Services;

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

public record GenerationProgress(string Phase, int PercentComplete, string StatusMessage);
```
