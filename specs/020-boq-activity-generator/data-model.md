# Data Model: BOQ Activity Generator

**Feature**: BOQ Activity Generator (Phase 20)
**Date**: 2026-05-27

## Entities

### BOQContext

Represents the parsed BOQ data from the active Excel workbook, prepared for AI consumption.

| Field | Type | Description |
|-------|------|-------------|
| WorkbookName | string | Name of the active workbook |
| SheetName | string | Name of the BOQ sheet |
| Items | BOQItem[] | Parsed BOQ items |
| ItemCount | int | Total number of BOQ items parsed |
| TotalQuantity | decimal | Sum of all item quantities |
| IsTruncated | bool | Whether items were truncated for token limits |
| TruncatedItemCount | int | Number of items excluded due to truncation |

**Validation Rules**:
- At least 1 BOQ item must be present
- Zero-quantity items flagged for review (not rejected)
- Negative-quantity items rejected with error

---

### GeneratedActivity

A construction activity produced by the AI engine from BOQ data, editable by the planning engineer.

| Field | Type | Description |
|-------|------|-------------|
| ActivityId | string | Sequential identifier (A-001, A-002, ...) |
| Name | string | Activity name in verb-noun format (e.g., "Pour Concrete Foundation") |
| Description | string | Detailed description of the activity scope |
| BoqReferences | string[] | References to source BOQ item identifiers |
| Quantity | decimal | Activity quantity (may differ from sum of BOQ items) |
| Unit | string | Unit of measure (e.g., m³, m², ton, each) |
| WbsPath | string | WBS hierarchy path for sequencing |
| TradeCategory | string | Trade classification (e.g., "Structure", "MEP", "Finishes") |
| Dependencies | string[] | Activity IDs of predecessor activities |
| ValidationStatus | ValidationStatus | Current validation state |
| IsUserModified | bool | Whether the engineer edited this activity after generation |
| OriginalName | string | Auto-generated name before any user edit |
| SortOrder | int | Display/sequence order within the activity list |

**Validation Rules**:
- Name must not be empty
- Name should follow verb-noun convention (warning, not error)
- At least 1 BOQ reference required
- Quantity must be >= 0
- Duplicate Name + BOQ Reference combination flagged (warning)

**Lifecycle/State Transitions**:
```
[Generated] → [Validated] → [Sequenced] → [Exported]
     ↑            |   ↑            |
     └────────────┘   └────────────┘
  (re-generation preserves user edits via BOQ reference matching)
```

- **Generated**: Initially produced by AI
- **Validated**: Passed validation checks (or user acknowledged warnings)
- **Sequenced**: Assigned WBS order and dependencies
- **Exported**: Written to Excel sheet

---

### ActivityValidationResult

Contains the validation outcome for a single activity.

| Field | Type | Description |
|-------|------|-------------|
| ActivityId | string | The validated activity's ID |
| IsValid | bool | True if no errors (warnings allowed) |
| Errors | ValidationIssue[] | Blocking issues (must be resolved) |
| Warnings | ValidationIssue[] | Non-blocking issues (advisory) |
| ValidatedAt | DateTime | Timestamp of validation run |

---

### ValidationIssue

A single validation finding.

| Field | Type | Description |
|-------|------|-------------|
| IssueType | IssueType | DuplicateDetection, NamingConvention, MissingReference, QuantityInconsistency |
| Severity | Severity | Error or Warning |
| Message | string | Human-readable description |
| AffectedField | string | The field that failed validation |

**IssueType values**: DuplicateDetection, NamingConvention, MissingReference, QuantityInconsistency

---

### ActivitySequenceOrder

Defines the ordering of activities within the sequence view.

| Field | Type | Description |
|-------|------|-------------|
| ActivityId | string | Reference to the sequenced activity |
| WbsPath | string | WBS hierarchy path |
| SortOrder | int | Ordinal position in sequence |
| ParallelGroup | string | Identifier for parallel work group (null if sequential) |
| Dependencies | ActivityDependency[] | Predecessor relationships |

---

### ActivityDependency

A predecessor/successor relationship between two activities.

| Field | Type | Description |
|-------|------|-------------|
| PredecessorId | string | Activity ID of the predecessor |
| SuccessorId | string | Activity ID of the successor |
| Type | DependencyType | Finish-to-Start (default) |
| Confidence | string | High, Medium, or Low |
| Rationale | string | Explanation of why this dependency exists |
| IsAccepted | bool | Whether the engineer accepted this suggestion |
| IsUserModified | bool | Whether the engineer modified this from the suggestion |

**DependencyType values**: FinishToStart (default; only type in Phase 20)

---

### ActivityExportConfig

Configuration for Excel export.

| Field | Type | Description |
|-------|------|-------------|
| TargetSheetName | string | Name of the export sheet (default: "Generated Activities") |
| Columns | string[] | Column order for export |
| IncludeDependencies | bool | Whether to include the Dependencies column |
| OverwriteExisting | bool | Whether to overwrite an existing sheet |
| ThemeColors | object | Theme-derived colors for styling |

## Relationships

```
BOQContext 1 ── * BOQItem
     │
     │ (used by AI to generate)
     ▼
GeneratedActivity 1 ── * ActivityValidationResult
     │                      │
     │                      └── ValidationIssue *
     │
     │ (sequenced into)
     ▼
ActivitySequenceOrder 1 ── * ActivityDependency
     │
     │ (exported as)
     ▼
Excel sheet (ActivityExportConfig)
```

## Enumerations

### ValidationStatus
- Pending
- Passed
- HasWarnings
- HasErrors

### Severity
- Error
- Warning

### IssueType
- DuplicateDetection
- NamingConvention
- MissingReference
- QuantityInconsistency

### DependencyType
- FinishToStart
