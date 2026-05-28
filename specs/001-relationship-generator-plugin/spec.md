# Feature Specification: Relationship Generator Plugin

**Feature Branch**: `021-relationship-generator-plugin`

**Created**: 2026-05-27

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 21"

## User Scenarios & Testing

### User Story 1 - Generate Activity Relationships from AI (Priority: P1)

A planning engineer has a list of construction activities (generated from BOQ analysis in Phase 20) and needs to automatically generate logical predecessor/successor relationships between them. The engineer launches the Relationship Generator, selects the activities, and the system produces dependency links based on trade sequence, space constraints, and resource flow.

**Why this priority**: This is the core value of the feature — automating what is currently a manual, error-prone process of linking hundreds of activities.

**Independent Test**: Can be fully tested by providing a known activity list (e.g., 20 construction activities with clear trade sequences) and verifying the generated relationships match expected FS/SS/FF/SF patterns.

**Acceptance Scenarios**:

1. **Given** a list of 50+ construction activities with descriptions and quantities, **When** the engineer triggers relationship generation, **Then** the system produces predecessor/successor links for at least 80% of activities.
2. **Given** activities spanning multiple trades (concrete, steel, MEP, finishes), **When** relationships are generated, **Then** cross-trade dependencies follow correct sequence (e.g., concrete before steel before MEP).
3. **Given** the generated relationship list, **When** inspected by the engineer, **Then** each relationship displays its type (FS/SS/FF/SF), predecessor, successor, and lag value.

---

### User Story 2 - Validate Relationships for Errors (Priority: P1)

After generating relationships (manually or via AI), the engineer needs to validate the logic network for cycles, open-ended activities, dangling links, and redundant relationships. The system produces a validation report highlighting all issues.

**Why this priority**: Invalid relationships cause scheduling errors. Validation must catch all issues before the schedule is used for planning or export.

**Independent Test**: Can be tested independently by introducing known errors (cycles, open ends, dangling activities) into a relationship set and verifying the validation report catches every type of error.

**Acceptance Scenarios**:

1. **Given** a relationship set containing a circular dependency (A→B→C→A), **When** validation runs, **Then** the system detects and reports the cycle with the involved activity chain.
2. **Given** a relationship set with activities having no predecessors, **When** validation runs, **Then** the system identifies all open-start activities.
3. **Given** a relationship set with activities having no successors, **When** validation runs, **Then** the system identifies all open-end activities.
4. **Given** a relationship set containing redundant links (A→B and A→B with same type), **When** validation runs, **Then** the system flags the duplicate relationship.

---

### User Story 3 - Analyze Parallel Execution Paths (Priority: P2)

The engineer needs to understand which activities can run in parallel versus sequentially, identify independent work streams, and determine the critical path through the logic network.

**Why this priority**: Parallel execution analysis helps optimize construction schedules and resource allocation, but is a secondary capability built on top of the relationship engine.

**Independent Test**: Can be tested with a known activity network where parallel paths are clearly identifiable, verifying the system correctly groups independent activities.

**Acceptance Scenarios**:

1. **Given** a relationship network with two independent chains (A→B→C and D→E→F with no cross-links), **When** parallel analysis runs, **Then** Activities A-D-E-F are identified as independent work streams.
2. **Given** a relationship network with a known longest path, **When** critical path analysis runs, **Then** the system identifies the sequence of activities forming the critical path.
3. **Given** activities with shared resource types, **When** parallel analysis runs, **Then** potential resource conflicts across parallel groups are flagged.

---

### Edge Cases

- What happens when a single activity has no relationships at all (isolated activity)?
- How does the system handle activities with lag values that exceed activity durations?
- What happens when the relationship network contains both positive and negative lag values?
- How does the system behave when presented with an empty activity list?
- What happens when AI-generated relationships conflict with user-defined relationships?

## Requirements

### Functional Requirements

- **FR-001**: System MUST generate predecessor/successor relationships from a list of construction activities using AI analysis of trade sequence, space constraints, and resource flow.
- **FR-002**: System MUST support all four relationship types: Finish-to-Start (FS), Start-to-Start (SS), Finish-to-Finish (FF), and Start-to-Finish (SF).
- **FR-003**: System MUST allow positive, negative, and zero lag values on any relationship.
- **FR-004**: System MUST provide dependency reasoning for each generated relationship (why this relationship was suggested).
- **FR-005**: System MUST detect and report circular dependencies (graph cycles) with the full activity chain involved.
- **FR-006**: System MUST identify activities with no predecessors (open-start) and no successors (open-end).
- **FR-007**: System MUST detect dangling activities — activities that exist but are not linked into the main network.
- **FR-008**: System MUST detect redundant/duplicate relationships between the same activity pair.
- **FR-009**: System MUST generate a validation report summarizing all detected issues with severity levels.
- **FR-010**: System MUST analyze the relationship network to identify independent work streams that can execute in parallel.
- **FR-011**: System MUST calculate parallel execution groups — sets of activities that can run concurrently.
- **FR-012**: System MUST identify the critical path (longest path) through the relationship network.
- **FR-013**: System MUST flag potential resource conflicts when activities with shared resource types are scheduled in parallel.
- **FR-014**: Users MUST be able to review, edit, accept, or reject each generated relationship via an inline grid editor with dropdowns for relationship type, lag value, and predecessor/successor selection, with per-row accept/reject toggle.
- **FR-015**: System MUST preserve user edits to relationships when re-running AI generation.
- **FR-016**: System MUST support relationships from activities generated by the BOQ Activity Generator (Phase 20).
- **FR-017**: System MUST auto-retry AI relationship generation once with a simplified prompt if the initial AI call fails or times out.
- **FR-018**: If both AI generation attempts fail, System MUST present a manual relationship editor allowing the user to define relationships without AI assistance.
- **FR-019**: System MUST maintain relationships in-memory during the session for consumption by downstream phases (Duration Estimator).
- **FR-020**: System MUST allow users to save generated relationships to the active Excel sheet as new columns (Predecessor, Successor, Type, Lag) on user request.

### Key Entities

- **Activity**: A construction task with ID, name, description, quantity, unit, and associated BOQ reference. Acts as a node in the relationship network.
- **Relationship**: A logical dependency between two activities consisting of predecessor activity, successor activity, relationship type (FS/SS/FF/SF), and lag value.
- **Logic Network**: The complete set of relationships forming a directed graph of all activities.
- **Validation Report**: A structured list of issues found during relationship validation, with issue type, severity, affected activities, and description.
- **Parallel Execution Group**: A set of activities that can execute concurrently without dependency conflicts.
- **Critical Path**: The longest sequence of dependent activities through the network, determining the minimum project duration.
- **Resource Conflict**: A detected situation where two or more activities in the same parallel group require the same constrained resource.

### Out of Scope

- Visual network diagram / drag-and-drop relationship canvas
- Automatic correction of detected loops (reported only)
- Primavera or scheduling tool export (relationship data consumed in-memory by subsequent phases)
- Multi-project or cross-project relationship networks
- Relationship templates or libraries

## Success Criteria

### Measurable Outcomes

- **SC-001**: Engineers can generate relationships for 100+ activities in under 30 seconds.
- **SC-002**: AI-generated relationships achieve at least 80% acceptance rate without manual edits.
- **SC-003**: Loop detection catches 100% of circular dependencies regardless of network size.
- **SC-004**: Validation report identifies all open ends, dangling activities, and redundant relationships with zero false negatives.
- **SC-005**: Parallel execution analysis correctly identifies all independent work streams for networks of any size with graceful degradation on very large networks.
- **SC-006**: Engineers report at least 70% reduction in time spent manually linking activities.
- **SC-007**: Generated relationship network passes manual review by a senior planning engineer for correctness and completeness.

## Clarifications

### Session 2026-05-27

- Q1: AI Failure & Fallback Behavior → A: Auto-retry once with simplified prompt; on second failure, show manual relationship editor.
- Q2: Out of Scope Boundaries → A: Visual network diagram excluded; loop auto-fix out of scope; manual add/edit included.
- Q3: Relationship Data Consumption Format → A: In-memory during session; save to same Excel sheet as new columns on user request.
- Q4: Manual Relationship Editing UX Depth → A: Inline grid editor with dropdowns for type, lag, predecessor; per-row accept/reject.
- Q5: Scalability Limits for Graph Algorithms → A: No fixed limit — optimize for best-effort performance.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The BOQ Activity Generator (Phase 20) is complete and provides the activity list as input to this feature.
- AI Core Infrastructure (Phase 18) is complete and provides AI orchestration, prompt governance, and structured output parsing.
- Activity names follow verb-noun naming conventions for reliable AI relationship inference.
- Activities are organized by trade and WBS hierarchy for improved dependency reasoning.
- The feature operates within the Shell workspace as a plugin page registered under the "Planning" sidebar category.
- Target users are planning engineers familiar with construction scheduling logic and relationship types.
- Resource type information is available from activity metadata for resource conflict detection.
