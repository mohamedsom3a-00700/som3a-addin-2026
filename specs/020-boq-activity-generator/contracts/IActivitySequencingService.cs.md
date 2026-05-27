# Contract: IActivitySequencingService

**Layer**: WpfApp2.Services
**Purpose**: Orders activities by WBS hierarchy and trade sequence; suggests basic dependencies.

```csharp
namespace WpfApp2.Services;

public interface IActivitySequencingService
{
    Task<IReadOnlyList<ActivitySequenceOrder>> SequenceAsync(
        IReadOnlyList<GeneratedActivity> activities,
        CancellationToken ct = default);

    Task<IReadOnlyList<ActivityDependency>> SuggestDependenciesAsync(
        IReadOnlyList<GeneratedActivity> activities,
        CancellationToken ct = default);
}
```
