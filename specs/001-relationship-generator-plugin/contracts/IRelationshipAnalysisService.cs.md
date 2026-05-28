# IRelationshipAnalysisService

**File**: `WpfApp2/Services/IRelationshipAnalysisService.cs` (interface)
**Location**: `WpfApp2/Services/RelationshipAnalysisService.cs` (implementation)

Provides parallel execution group detection, critical path analysis, and resource conflict detection.

## Interface

```csharp
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
```

## Methods

### AnalyzeParallelGroupsAsync
- **Input**: RelationshipNetwork with topological ordering
- **Output**: List of ParallelExecutionGroup, one per connected component level
- **Algorithm**:
  1. Get weakly connected components of the graph
  2. Within each component, topological sort (Kahn's algorithm)
  3. Activities at the same topological level = parallel execution group
  4. Each level becomes a group with reason text
- **Contracts**:
  - Returns empty list if network has cycles (must validate first)
  - Single path (A→B→C) returns one group per component with single-activity groups
  - Fork/join patterns return multi-activity groups at parallel levels

### AnalyzeCriticalPathAsync
- **Input**: RelationshipNetwork with topological ordering
- **Output**: CriticalPathResult with the longest dependent path
- **Algorithm**:
  1. Topological sort (Kahn's algorithm)
  2. DP relaxation in topological order: `dist[v] = max(dist[u] + duration[u])` for all `u→v`
  3. Backtrack from max-distance node to reconstruct critical path
  4. Edge weight = duration of predecessor activity (not lag)
- **Contracts**:
  - Returns CriticalPathResult with empty Path if network has cycles
  - All activities without duration default to 0 weight (excluded from duration sum)
  - Isolated activities (no relationships) have path length 0 (singleton critical path)

### DetectResourceConflictsAsync
- **Input**: RelationshipNetwork with resource assignments
- **Output**: List of ResourceConflict where parallel activities share a constrained resource
- **Algorithm**:
  1. Get parallel execution groups
  2. For each group, find activities with shared ResourceType (from ResourceAssignments)
  3. Flag as Warning for shareable resources (Labor, Material), Critical for exclusive resources (Equipment)
- **Contracts**:
  - Returns empty list if no resource conflicts detected
  - Each conflict reported once per (group, resource type) pair
  - ResourceType string matched case-insensitively

## Dependencies

- `Som3a.Domain.Relationships.RelationshipNetwork` — graph container
- `Som3a.Domain.Activities.Activity.Duration` — for critical path
- `Som3a.Domain.Activities.Activity.ResourceAssignments` — for resource conflict
