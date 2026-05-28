# Research: Relationship Generator Plugin

**Feature**: Relationship Generator Plugin (Phase 21)
**Date**: 2026-05-27

## Decisions

### 1. Relationship Generation Strategy

**Decision**: Use Phase 20's `ActivityDependency` types and the existing `Relationship` domain entity. Enhance the AI prompt to request richer structured output including type (FS/SS/FF/SF), lag, and reasoning for each relationship.

- **Rationale**: The existing `Relationship` domain entity (`Som3a.Domain/Relationships/Relationship.cs`) already supports four relationship types (FS/SS/FF/SF) and lag values. The `RelationshipParser` in `Som3a.AI/Parsers/` already handles structured JSON parsing from AI responses. The AI prompt template (`RelationshipPrompt`) instructs the model to reason about trade sequence (e.g., structural precedes MEP), space constraints (e.g., concurrent work in confined area needs separation), and resource flow (e.g., same crew moves from one area to next).
- **AI Input Context**: Activity list with Name, Description, TradeCategory, WbsPath, Quantity, Unit, and Phase 20-generated dependencies. Activities grouped by trade category for relationship inference.
- **Alternatives considered**:
  - Rule-based generation using trade precedence matrix (brittle; cannot handle novel sequences)
  - Manual entry only (defeats the purpose of automation)

### 2. Graph Algorithm Selection

**Decision**: Use DFS-based cycle detection (existing `LoopDetector` in `Som3a.Validation`), topological sort for critical path and parallel group detection.

- **Rationale**: `LoopDetector.cs` already implements DFS-based cycle detection in the dependency graph. The same DFS/Kahn topological sort approach identifies: (a) cycles — report the chain; (b) topological order — find critical path via longest-path DP on DAG; (c) sources (no predecessors) and sinks (no successors) for open-end detection; (d) independent subgraphs for parallel execution groups. All algorithms are O(V+E) and handle 100+ activities easily.
- **Open-start detection**: Walk the graph, find all nodes with in-degree = 0 → these are open-start (no predecessors).
- **Open-end detection**: Find all nodes with out-degree = 0 → open-end (no successors).
- **Dangling detection**: After building the main graph, any activity that appears in the activity list but has zero relationships is flagged as dangling.
- **Redundant relationship detection**: Sort (predecessorId, successorId, type, lag) tuples; detect identical tuples → flag as redundant.
- **Critical path**: Apply DP (longest path) on DAG after topological sort. Edge weight = activity duration on predecessor activity. Total duration = sum of durations along path.
- **Parallel groups**: Use DAG's connected components → each weakly connected component is an independent work stream. Within each component, activities at the same topological level can run in parallel.
- **Alternatives considered**:
  - Tarjan's SCC algorithm (over-engineered; cycles are at graph level, not strongly connected)
  - Bellman-Ford for longest path (O(VE) vs O(V+E) for topological sort on DAG; more expensive)

### 3. AI Prompt Design

**Decision**: Extend `RelationshipPrompt` with structured instruction specifying: activity list with trade categories, relationship type definitions (FS/SS/FF/SF with examples), lag semantics (positive = delay, negative = overlap), and expected JSON output schema including rationale for each relationship.

- **Schema requirement**: The prompt MUST constrain the AI to output valid JSON conforming to the relationship schema: `[{ predecessorId, successorId, type, lag, rationale, confidence }]`.
- **Prompt sections**:
  1. System instruction (role: construction scheduling expert)
  2. Activity definitions (name, trade, description, dependencies)
  3. Relationship type table with examples
  4. Lag semantics guide
  5. Output JSON schema expectation
  6. Chain-of-thought request (hidden, not in output)
- **Alternatives considered**:
  - Free-text output parsed by regex (fragile; schema violations common)
  - Structured function calling (provider-specific; not all providers support it)

### 4. User Edit Preservation Across Re-generation

**Decision**: Match activities by ActivityId across generation cycles; preserve user edits (accepted/rejected relationships, manual modifications) for matched activities.

- **Rationale**: Activities maintain stable identity via their ActivityId (domain entity). When relationship generation is re-run (e.g., activities changed), existing relationships are matched by (predecessorId, successorId) pair. If a relationship exists in both old and new sets, user-modified fields (IsAccepted, Type, Lag) are preserved. Newly generated relationships default to pending review.
- **Fields preserved**: IsAccepted, IsUserModified, Type (if user-modified), Lag (if user-modified).
- **Alternatives considered**:
  - Full regeneration (loses all manual edits; frustrating)
  - Match by BOQ reference (Phase 20's approach — but relationships are between activities, not BOQ items)

### 5. Fallback UX for AI Failure

**Decision**: First AI attempt uses full prompt with all activities. On failure/timeout, retry once with simplified prompt (fewer activities, reduced context). On second failure, present manual relationship editor where the user can define relationships without AI assistance.

- **Retry strategy**: The existing `RetryHandler` in `Som3a.AI/Orchestration/` already handles retry with exponential backoff. The simplified prompt truncates the activity list to 50 highest-priority items (by trade category), drops detailed descriptions, and uses a shorter system instruction.
- **Manual editor**: The `RelationshipEditorGrid` user control provides dropdowns for predecessor, successor, type (FS/SS/FF/SF), and lag (numeric spinner). Plus/minus buttons add/remove rows. Accept/reject toggle per row. Validation runs on add/edit.
- **Alternatives considered**:
  - Three retries (unnecessary; two attempts sufficient per spec)
  - Always require AI (bad UX when API is down)

### 6. Existing Domain Enhancement

**Decision**: The existing `Relationship` entity requires additional fields for AI metadata (rationale, confidence, isAccepted, isUserModified). Extend the entity rather than create a new one.

- **Fields to add**: `Rationale` (string), `Confidence` (enum: High/Medium/Low), `IsAccepted` (bool), `IsUserModified` (bool), `GeneratedAt` (DateTime).
- **New domain entities**:
  - `RelationshipNetwork` — graph container with adjacency list, topological order, connected components
  - `ParallelExecutionGroup` — group ID, activity IDs, reason for parallel feasibility
  - `CriticalPathResult` — path (ordered activity IDs), total duration, criticality index
  - `ResourceConflict` — conflicting activity IDs, shared resource type, severity
- **Alternatives considered**:
  - New sub-entity `AIRelationship` extending `Relationship` (creates parallel hierarchy; confusing)
  - Keep `Relationship` clean and store AI metadata separately (fragmentation; harder to maintain)

## Dependencies

| Dependency | Source | Purpose |
|------------|--------|---------|
| Som3a.Domain.Relationships.Relationship, RelationshipType | Phase 14 | Core relationship entity |
| Som3a.Domain.Activities.Activity | Phase 14 | Source activity data |
| Som3a.Validation.Relationships.DependencyValidator | Phase 14 | Existing validator |
| Som3a.Validation.Relationships.LoopDetector | Phase 14 | DFS cycle detection |
| Som3a.AI.Orchestration.OrchestrationEngine | Phase 18 | AI orchestration |
| Som3a.AI.Prompts.PromptTemplateRegistry | Phase 18 | Prompt template management |
| Som3a.AI.Parsers.RelationshipParser | Phase 18 | Parse AI JSON output |
| Som3a.Domain.Activities.GeneratedActivity, BOQContext | Phase 20 | Input activity data |
| WpfApp2 shell, ServiceContainer, EventBus | Existing | Plugin hosting, DI, events |
| Excel VSTO Interop | WpfApp2 existing | Excel save on user request |

## Best Practices

- Graph algorithms are O(V+E) — no hidden O(V²) or O(E²) operations
- AI calls are async with progress indication via IProgress<GenerationProgress>
- Validation runs automatically on generation and on explicit user request
- Grid validation debounced on each cell edit (300ms)
- Save to Excel uses existing VSTO interop; creates/updates columns in the same sheet
- All service dependencies injected via constructor (ServiceContainer)
- Relationship editor grid uses DataGrid with EnableRowVirtualization=True for 100+ rows
- Edit preservation matched by (predecessorId, successorId) pair across re-generation
- Critical path analysis handles both zero-lag and non-zero lag edges correctly
