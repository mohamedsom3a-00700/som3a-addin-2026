# Data Model: Relationship Generator Plugin

**Feature**: Relationship Generator Plugin (Phase 21)
**Date**: 2026-05-27

## Existing Entities (Enhanced)

### Relationship

Defines a logical dependency between two activities. Existing entity in `Som3a.Domain/Relationships/` — enhanced with AI metadata fields.

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique identifier (GUID) |
| Predecessor | Activity | The preceding activity |
| Successor | Activity | The succeeding activity |
| Type | RelationshipType | FS, SS, FF, or SF |
| Lag | TimeSpan | Lag value (positive = delay, negative = overlap) |
| ValidationStatus | ValidationStatus | Valid, Warning, or Error |
| ValidationMessage | string? | Human-readable validation message |
| Rationale | string | NEW — AI reasoning for why this relationship exists |
| Confidence | RelationshipConfidence | NEW — High, Medium, or Low |
| IsAccepted | bool | NEW — Whether the engineer accepted this relationship |
| IsUserModified | bool | NEW — Whether the engineer modified this from the AI suggestion |
| GeneratedAt | DateTime? | NEW — Timestamp when this relationship was generated |

**Validation Rules**:
- Predecessor and Successor must be non-null
- Predecessor.Id != Successor.Id (no self-loops)
- Lag range: -365 to +365 days (warning outside this range)
- Predecessor and Successor must exist in the activity list

**State Transitions**:
```
[Generated] → [Pending Review] → [Accepted] or [Rejected]
     ↑                                  |
     └──────────────────────────────────┘
  (re-generation preserves IsAccepted and modifications)
```

---

## New Entities

### RelationshipNetwork

A graph container that wraps all relationships with adjacency structures for algorithm execution.

| Field | Type | Description |
|-------|------|-------------|
| Activities | IReadOnlyList<Activity> | All activities in the network |
| Relationships | IReadOnlyList<Relationship> | All relationships in the network |
| AdjacencyList | IReadOnlyDictionary<string, List<string>> | Directed graph (predecessor → successors) |
| ReverseAdjacencyList | IReadOnlyDictionary<string, List<string>> | Reverse graph (successor → predecessors) |
| NodeCount | int | Number of activity nodes |
| EdgeCount | int | Number of relationship edges |

**Methods**:
- `Build(List<Activity>, List<Relationship>)` — Build the adjacency structures
- `GetTopologicalOrder()` — Returns topological ordering or throws if cycle detected
- `GetConnectedComponents()` — Returns weakly connected components
- `HasCycles()` — Returns true if any cycles exist

---

### ParallelExecutionGroup

A set of activities that can execute concurrently.

| Field | Type | Description |
|-------|------|-------------|
| GroupId | string | Unique identifier (G-001, G-002, ...) |
| ActivityIds | string[] | Activity IDs in this parallel group |
| Reason | string | Explanation of why these activities can run in parallel |
| TopologicalLevel | int | Level in the topological sort (same level = parallel) |

---

### CriticalPathResult

The result of critical path analysis.

| Field | Type | Description |
|-------|------|-------------|
| Path | string[] | Ordered list of Activity IDs forming the critical path |
| TotalDuration | TimeSpan | Sum of durations along the critical path |
| CriticalActivities | int | Number of activities on the critical path |
| TotalActivities | int | Total number of activities in the network |
| CriticalityIndex | double | Ratio of critical activities to total (0.0 - 1.0) |

---

### ResourceConflict

A detected resource contention between parallel activities.

| Field | Type | Description |
|-------|------|-------------|
| ActivityIds | string[] | The conflicting activity IDs |
| ResourceType | string | The shared resource type (e.g., "Crane", "Concrete Pump") |
| Severity | ConflictSeverity | Warning or Critical |
| Message | string | Human-readable description of the conflict |

---

### ValidationReport

The complete validation output for a relationship network.

| Field | Type | Description |
|-------|------|-------------|
| Issues | NetworkValidationIssue[] | All detected issues |
| HasErrors | bool | True if any Error-severity issues exist |
| HasWarnings | bool | True if any Warning-severity issues exist |
| TotalIssueCount | int | Total number of issues detected |
| ValidatedAt | DateTime | Timestamp of the validation run |

---

### NetworkValidationIssue

A single issue found during network validation.

| Field | Type | Description |
|-------|------|-------------|
| IssueType | NetworkIssueType | CircularDependency, OpenStart, OpenEnd, DanglingActivity, RedundantRelationship, LagOutOfRange |
| Severity | Severity | Error or Warning |
| Message | string | Human-readable description |
| AffectedActivityIds | string[] | Activity IDs involved in the issue |
| AffectedRelationshipIds | string[] | Relationship IDs involved (if applicable) |
| Details | string? | Additional context (e.g., the cycle chain) |

---

## Enumerations

### RelationshipType (existing)
- **FS** — Finish-to-Start (successor starts after predecessor finishes)
- **SS** — Start-to-Start (successor starts after predecessor starts)
- **FF** — Finish-to-Finish (successor finishes after predecessor finishes)
- **SF** — Start-to-Finish (successor finishes after predecessor starts)

### ValidationStatus (existing)
- Valid, Warning, Error

### RelationshipConfidence (NEW)
- **High** — Strong trade sequence match
- **Medium** — Probable but not guaranteed sequence
- **Low** — Weak heuristic match; requires user review

### ConflictSeverity (NEW)
- **Warning** — Non-blocking resource concern
- **Critical** — Resource conflict that blocks execution

### NetworkIssueType (NEW)
- **CircularDependency** — Cycle detected in relationship graph
- **OpenStart** — Activity has no predecessors
- **OpenEnd** — Activity has no successors
- **DanglingActivity** — Activity has no relationships at all
- **RedundantRelationship** — Duplicate relationship between same activity pair
- **LagOutOfRange** — Lag value exceeds reasonable bounds

## Relationships

```
Activity 1 ── * Relationship * ── 1 Activity
     │                                   
     │ (wrapped by)                      
     ▼                                    
RelationshipNetwork 1 ── 1..* ParallelExecutionGroup
     │                                      
     │ (analyzed by)                        
     ▼                                      
CriticalPathResult (singleton output)
     │
     │ (validated by)
     ▼
ValidationReport 1 ── * NetworkValidationIssue
     │
     │ (with resource conflict detection)
     ▼
ResourceConflict * ── * Activity
```
