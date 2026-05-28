# IRelationshipValidationService

**File**: `WpfApp2/Services/IRelationshipValidationService.cs` (interface)
**Location**: `WpfApp2/Services/RelationshipValidationService.cs` (implementation)

Orchestrates all network validation rules and produces a structured validation report.

## Interface

```csharp
public interface IRelationshipValidationService
{
    Task<ValidationReport> ValidateNetworkAsync(
        RelationshipNetwork network,
        CancellationToken ct = default);

    Task<IReadOnlyList<NetworkValidationIssue>> ValidateSingleRelationshipAsync(
        Relationship relationship,
        RelationshipNetwork network,
        CancellationToken ct = default);
}
```

## Methods

### ValidateNetworkAsync
- **Input**: Fully constructed RelationshipNetwork (activities + relationships)
- **Output**: ValidationReport with all detected issues grouped by severity
- **Validation pipeline** (runs all rules independently):
  1. **DependencyValidator** (existing): Valid activity references, no self-loops, lag range check
  2. **LoopDetector** (existing): DFS cycle detection with chain reporting
  3. **OpenEndDetector** (new): Identifies all open-start and open-end activities
  4. **RedundantDetection** (new): Detects duplicate (predecessor, successor, type, lag) tuples
  5. **DanglingDetection** (new): Finds activities with zero relationships
- **Contracts**:
  - Empty network (no relationships) returns warnings for all activities as dangling
  - Single isolated activity returns dangling warning but no errors
  - All detection algorithms run regardless of previous rule failures (comprehensive report)

### ValidateSingleRelationshipAsync
- **Input**: A single relationship + the full network context
- **Output**: Issues specific to that relationship (empty list if valid)
- **Usage**: Called on each cell edit in the relationship editor grid (debounced 300ms)
- **Checks**: Self-loop, valid activity refs, duplicate detection, lag out of range

## Dependencies

- `Som3a.Validation.Relationships.DependencyValidator` — existing validator
- `Som3a.Validation.Relationships.LoopDetector` — existing DFS detector
- New validators (OpenEndDetector, redundant detection, dangling detection) — implemented directly in the service or as helper classes
