# Data Model: Platform Foundation

**Feature**: Phase 14 — Platform Foundation
**Date**: 2026-05-25

## Entity Relationship Diagram

```text
BOQDocument 1──* BOQSection 1──* BOQItem
                                        │
                                        │ (derived from / related to)
                                        ▼
Calendar ◄──── Activity ────► WBSNode (tree: Parent/Children)
                │    ▲
                │    │ (Predecessor/Successor)
                │    │
                ▼    │
Constraint    Relationship ──┘
                │
Resource ◄──────┘ (ResourceAssignment)
```

## Entities

### BOQDocument

Represents an entire Bill of Quantities document.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| ProjectName | string (max 200) | Yes | Name of the construction project |
| CreatedDate | DateTimeOffset | Yes | When the BOQ was created or imported |
| Sections | List\<BOQSection\> | Yes | Top-level BOQ sections |
| TotalItems | int (computed) | No | Total count of BOQItem across all sections |
| Source | string | No | Origin of the BOQ (e.g., Excel file path, import source) |

**Validation**: TotalItems must equal sum of all section item counts.

---

### BOQSection

A logical grouping within a BOQ (e.g., "Concrete Works", "Electrical").

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| SectionName | string (max 200) | Yes | Display name |
| SectionCode | string (max 50) | Yes | Short code (e.g., "CW-01") |
| DisplayOrder | int | Yes | Sort order within the document |
| Items | List\<BOQItem\> | Yes | Line items in this section |
| ParentDocument | BOQDocument | Yes | Owning document reference |

**Validation**: DisplayOrder must be unique within the parent document. SectionCode must be non-empty.

---

### BOQItem

A single line item in a BOQ (e.g., "Supply and install 200mm concrete slab").

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| ItemCode | string (max 50) | Yes | Unique item code within BOQ |
| Description | string (max 500) | Yes | Full item description |
| Quantity | decimal | Yes | Numeric quantity |
| Unit | string (max 20) | Yes | Unit of measure (e.g., "m3", "kg", "ea") |
| UnitPrice | decimal | No | Unit price (optional for planning-only BOQs) |
| TotalPrice | decimal (computed) | No | Quantity × UnitPrice |
| Classification | string (max 100) | No | Trade/discipline classification |
| BOQReference | string (max 50) | No | External BOQ reference code |
| ParentSection | BOQSection | Yes | Owning section |

**Validation**: Quantity > 0. Unit must be non-empty. ItemCode must be unique within the BOQ.

---

### Activity

A construction work activity derived from or related to BOQ items.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| ActivityId | string (max 20) | Yes | Human-readable activity ID (e.g., "A001") |
| Name | string (max 200) | Yes | Short activity name |
| Description | string (max 1000) | No | Detailed description |
| Quantity | decimal | No | Work quantity |
| Unit | string (max 20) | No | Unit of measure |
| Duration | TimeSpan | No | Estimated duration |
| ProductivityRate | decimal | No | Units per day |
| WBSNode | WBSNode | No | Parent WBS node |
| Relationships | List\<Relationship\> | No | Predecessor/successor links |
| Constraints | List\<Constraint\> | No | Scheduling constraints |
| ResourceAssignments | List\<ResourceAssignment\> | No | Assigned resources |
| BOQReferences | List\<string\> | No | Linked BOQ item codes |
| Calendar | Calendar | No | Activity-specific calendar (falls back to project calendar) |

**Validation**: ActivityId must be unique. If Duration is set, it must be > TimeSpan.Zero. ProductivityRate must be > 0 if set.

---

### WBSNode

A node in a Work Breakdown Structure tree.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Code | string (max 50) | Yes | WBS code (e.g., "1.2.3") |
| Name | string (max 200) | Yes | Node name |
| Level | int (computed) | Yes | Depth in the tree (0 = root) |
| Parent | WBSNode? | No | Parent node (null for root) |
| Children | List\<WBSNode\> | No | Child nodes |
| FullPath | string (computed) | No | Full hierarchical path (e.g., "Project/Phase/Zone") |
| Activities | List\<Activity\> | No | Activities assigned to this node |

**Validation**: Code must be non-empty. Tree must not contain cycles. Level must match actual depth from root.

---

### Relationship

A dependency link between two activities.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Predecessor | Activity | Yes | The predecessor activity |
| Successor | Activity | Yes | The successor activity |
| Type | RelationshipType | Yes | FS (Finish-to-Start), SS (Start-to-Start), FF (Finish-to-Finish), SF (Start-to-Finish) |
| Lag | TimeSpan | Yes | Lag duration (positive = delay, negative = overlap, zero = none) |
| ValidationStatus | ValidationStatus | Yes | Valid / Warning / Error |
| ValidationMessage | string | No | Human-readable validation message |

**Validation**: Predecessor != Successor. Lag can be negative (representing overlap). Type must be a valid enum value.

**RelationshipType enum**: `FS`, `SS`, `FF`, `SF`

**ValidationStatus enum**: `Valid`, `Warning`, `Error`

---

### Calendar

A work calendar definition with working days and holidays.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Name | string (max 100) | Yes | Calendar name (e.g., "Standard 6-day week") |
| WorkDays | HashSet\<DayOfWeek\> | Yes | Working days of the week |
| Holidays | List\<DateTime\> | No | Non-working dates |
| WorkingHoursPerDay | decimal | Yes | Hours per working day (e.g., 8, 10) |
| TimeZone | string | No | IANA timezone identifier |

**Validation**: WorkDays must contain at least 1 day. WorkingHoursPerDay must be > 0 and ≤ 24.

---

### Constraint

A scheduling constraint on an activity.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Activity | Activity | Yes | The constrained activity |
| ConstraintType | ConstraintType | Yes | Type of constraint |
| ConstraintDate | DateTime | Yes | The constraint date |
| FloatValue | int | No | Float in working days |

**ConstraintType enum**: `StartOn`, `FinishOn`, `MandatoryStart`, `MandatoryFinish`, `StartOnOrAfter`, `FinishOnOrBefore`

---

### Resource

A resource that can be assigned to activities.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Name | string (max 100) | Yes | Resource name |
| ResourceType | ResourceType | Yes | Labor / Equipment / Material / Subcontractor |
| CostPerHour | decimal | No | Hourly cost rate |
| Budget | decimal | No | Total budget allocation |
| Assignments | List\<ResourceAssignment\> | No | Activity assignments |

**ResourceType enum**: `Labor`, `Equipment`, `Material`, `Subcontractor`

---

### ResourceAssignment

Links a resource to an activity with quantity and cost details.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Resource | Resource | Yes | Assigned resource |
| Activity | Activity | Yes | Target activity |
| Quantity | decimal | Yes | Units assigned |
| Cost | decimal (computed) | No | Cost = Quantity × Resource.CostPerHour × Activity.Duration.Hours |

---

### PluginDescriptor

Metadata for a discovered plugin (not a domain entity — belongs to Plugin SDK).

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | string (max 100) | Yes | Unique plugin identifier |
| Name | string (max 200) | Yes | Display name |
| Version | string (max 20) | Yes | Semantic version (e.g., "1.0.0") |
| Priority | int | Yes | Load priority (lower = first) |
| Dependencies | List\<string\> | No | Required plugin IDs |
| AssemblyPath | string | Yes | Full path to plugin assembly |
| Status | PluginStatus | Yes | Loaded / Error / Disabled |
| NavigationItems | List\<NavigationItemInfo\> | No | Registered navigation items |
| SettingsSections | List\<SettingsSectionInfo\> | No | Registered settings sections |
| Commands | List\<CommandInfo\> | No | Registered commands |

**PluginStatus enum**: `Loaded`, `Error`, `Disabled`

---

## Serialization Rules

1. **Domain entities**: Serialized with System.Text.Json using `ReferenceHandler.Preserve` for cycle handling.
2. **Bridge DTOs**: Use ID-based references instead of nested object references (e.g., `Activity.WBSNodeId` instead of `Activity.WBSNode`).
3. **JSON field naming**: camelCase for all properties.
4. **Collections**: Serialized as JSON arrays.
5. **Enums**: Serialized as strings (not integers).
6. **Date/time**: ISO 8601 format with offset (DateTimeOffset).
