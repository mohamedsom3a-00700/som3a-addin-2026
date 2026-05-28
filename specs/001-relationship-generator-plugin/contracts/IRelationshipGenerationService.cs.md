# IRelationshipGenerationService

**File**: `WpfApp2/Services/IRelationshipGenerationService.cs` (interface)
**Location**: `WpfApp2/Services/RelationshipGenerationService.cs` (implementation)

Handles AI-powered generation of predecessor/successor relationships between activities.

## Interface

```csharp
public interface IRelationshipGenerationService
{
    bool IsAIAvailable { get; }

    Task<IReadOnlyList<Relationship>> GenerateRelationshipsAsync(
        IReadOnlyList<Activity> activities,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Relationship>> RegenerateRelationshipsAsync(
        IReadOnlyList<Activity> activities,
        IReadOnlyList<Relationship> existingRelationships,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Relationship>> GetFallbackRelationshipsAsync(
        IReadOnlyList<Activity> activities,
        CancellationToken ct = default);
}
```

## Methods

### GenerateRelationshipsAsync
- **Input**: List of activities (with Name, Description, TradeCategory, Quantity, Unit, WbsPath)
- **Output**: List of generated Relationship objects with Type, Lag, Rationale, Confidence
- **Behavior**:
  1. Build AI prompt context from activity list
  2. Call AI provider via OrchestrationEngine (first attempt)
  3. On failure/timeout: retry once with simplified prompt
  4. On second failure: throw `AIGenerationException` (caller shows manual editor)
  5. Parse AI JSON response via RelationshipParser
  6. Assign default IsAccepted = false, IsUserModified = false
- **Contracts**:
  - Activities list must not be empty (ArgumentValidationException otherwise)
  - Returns at least N-1 relationships for N activities (activities may be unrelated)
  - Progress updates per activity batch (10 at a time)

### RegenerateRelationshipsAsync
- **Input**: Activities + existing relationships (with user edits)
- **Output**: Updated relationship list preserving user modifications
- **Behavior**:
  1. Run GenerateRelationshipsAsync for new AI output
  2. Match existing relationships by (predecessorId, successorId) pair
  3. Preserve IsAccepted, IsUserModified, user-modified Type/Lag for matched relationships
  4. New unmatched relationships default to unaccepted
  5. Relationships for removed activities are dropped

### GetFallbackRelationshipsAsync
- **Input**: List of activities
- **Output**: Heuristic-based relationships using trade precedence matrix
- **Usage**: Called when AI is unavailable (IsAIAvailable == false) or manual mode desired
- **Behavior**: Returns empty list (calling code provides the manual editor UI)

## Events

None — results returned via Task<IReadOnlyList<Relationship>>.
