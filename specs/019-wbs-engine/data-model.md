# Data Model: WBS Engine

## Entity: WBSNode

| Field | Type | Description |
|-------|------|-------------|
| Id | string (GUID) | Stable internal GUID — immutable across moves/renames |
| Code | string | Mutable hierarchical code (1, 1.1, 1.1.1) — derived from tree position |
| Name | string | Human-readable node name |
| Level | int | Computed depth (0 = root, 1 = child of root, etc.) |
| Parent | WBSNode? | Reference to parent node (null for root) |
| Children | List<WBSNode> | Child nodes |
| FullPath | string | Computed full path (e.g., "Building/Foundation/Footings") |
| Activities | List<Activity> | Associated activities (from Phase 14) |

**Validation Rules**:
- Code must be unique among siblings
- Name must be non-empty
- Level must match actual tree depth
- Parent cannot create a cycle (detected via DFS)
- Depth must not exceed configured max (default 10)

**State Transitions**:
```text
Node lives in a mutable tree — no formal lifecycle states.
Position, Code, and Name change in-place during editing.
```

---

## Entity: WBSTemplate

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique template identifier |
| Name | string | Display name |
| Category | string | Building, Infrastructure, MEP, Industrial, Fitout |
| Description | string | Short description of template use case |
| Tags | List<string> | Keywords for recommendation matching |
| RootNode | WBSNode | The root WBSNode defining the tree structure |
| Version | int | Template version number |
| IsSystem | bool | True for built-in templates; false for user-created |
| OwnerId | string? | User who created this template (null for system) |
| CreatedAt | DateTime | Creation timestamp |

**Validation Rules**:
- Name must be unique within category
- RootNode tree must pass tree validation
- System templates cannot be modified by users
- Category must be one of the 5 defined categories

---

## Entity: WBSProject

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique project identifier |
| RootNode | WBSNode | Root of the active WBS tree |
| SourceTemplateId | string? | The template used as starting point |
| AIGenerationContext | string? | Original project description used for AI generation |
| ModificationHistory | List<WBSChange> | Track of tree mutations (for undo) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

---

## Entity: WBSExportFormat

| Format | Description | File Extension |
|--------|-------------|---------------|
| Excel | Hierarchical with indentation and grouping | .xlsx |
| JSON | Structured node tree with all attributes | .json |
| XML | Hierarchical XML mirroring tree structure | .xml |

---

## Entity: WBSChange

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Change identifier (GUID) |
| Type | ChangeType | Add, Remove, Rename, Reorder, Regenerate |
| NodeId | string | Affected node Id |
| PreviousState | WBSNode? | Snapshot before change |
| Timestamp | DateTime | When the change occurred |
