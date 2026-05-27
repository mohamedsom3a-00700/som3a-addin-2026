# Contract: IActivityValidationService

**Layer**: WpfApp2.Services
**Purpose**: Validates generated activities against rules: duplicates, naming conventions, BOQ references, quantity consistency.

```csharp
namespace WpfApp2.Services;

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
```
