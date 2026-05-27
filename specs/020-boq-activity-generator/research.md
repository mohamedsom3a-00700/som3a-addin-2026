# Research: BOQ Activity Generator

**Feature**: BOQ Activity Generator (Phase 20)
**Date**: 2026-05-27

## Decisions

### 1. BOQ Data Reading Strategy

**Decision**: Read BOQ from active Excel workbook using existing Excel interop (VSTO) via ExcelApplication.ActiveWorkbook

- **Rationale**: The WpfApp2 host already has Excel VSTO interop infrastructure. Using ActiveWorkbook avoids file-path selection UX and ensures the engineer works on the current workbook. The BOQ sheet is identified by name convention ("BOQ", "Bill of Quantities", or engineer-selected sheet).
- **Alternatives considered**:
  - ClosedXML (requires file path; cannot read from unsaved workbooks)
  - Manual CSV import (loses Excel formatting; extra step for engineers)

### 2. AI Context Building

**Decision**: Compress BOQ items into a structured text prompt with item groups, quantities, units, and classifications; truncate lowest-priority items when token limit exceeded

- **Rationale**: AI providers have context window limits. Grouping similar items reduces token count while preserving semantic meaning. Items are prioritized by total value (quantity * unit rate estimate), with lowest-value items truncated first when needed. The engineer is notified of any truncated items.
- **Alternatives considered**:
  - Send raw BOQ as table (too many tokens; poor AI comprehension)
  - Chunk into multiple API calls (increases cost; risk of duplicate activity generation)

### 3. Activity Naming Convention Enforcement

**Decision**: AI prompt instructs verb-noun format; post-generation validation checks compliance using regex pattern; non-compliant names flagged for user edit

- **Rationale**: Verb-noun (e.g., "Pour Concrete", "Install Rebar") is industry standard for construction scheduling. AI prompt guidance alone is insufficient — post-generation validation catches edge cases. Validation is non-blocking (warning, not error) since engineers may have legitimate exceptions.
- **Pattern**: `^(Install|Pour|Erect|Excavate|Form|Fix|Lay|Weld|Paint|Test|Commission|Demolish|Backfill|Compact|Grout|Seal|Protect|Supply|Fabricate|Deliver|Remove|Cut|Drill|Bolt|Weld)\s+.+`
- **Alternatives considered**:
  - Strict AI-only enforcement (unreliable; AI may deviate)
  - Manual engineer review only (no automated quality check)

### 4. Activity Validation Logic

**Decision**: Four-rule validation pipeline executed on generation and on user request

- **Rationale**: A pipeline architecture (each rule independent, pass/fail with message) allows adding rules incrementally and re-running without regeneration.
- **Rules**:
  1. **Duplicate detection**: Same Name + BOQ Reference combination → warning (merge suggested)
  2. **Naming convention**: Verb-noun regex match failure → warning
  3. **Missing BOQ reference**: BOQ Ref does not match any current BOQ item → error
  4. **Quantity consistency**: Activity quantity differs from source BOQ quantity by >10% → warning
- **Alternatives considered**:
  - AI-only validation (unreliable; cannot guarantee correctness)
  - Single-pass validation (harder to extend; no granular error messages)

### 5. Activity Sequencing Strategy

**Decision**: Order activities by WBS hierarchy plus trade sequence heuristic; dependency suggestions based on trade precedence patterns

- **Rationale**: The WBS Engine (Phase 19) provides the WBS hierarchy. Within each WBS node, activities are ordered by trade sequence (e.g., excavation → foundation → structure → MEP → finishes). Dependency suggestions use a trade-precedence knowledge base (e.g., "Excavation must precede Foundation").
- **Trade precedence order**: Site Preparation → Excavation → Foundation → Structure → Roof → External Walls → MEP Rough-in → Insulation → Drywall → Finishes → Landscaping
- **Alternatives considered**:
  - AI-only dependency generation (over-engineered for basic dependencies; Phase 21 handles advanced logic)
  - Manual sequencing only (no automation; less useful to engineers)

### 6. Edit Preservation Across Re-generation

**Decision**: Match activities by BOQ reference across generation cycles; preserve edited fields for matched activities

- **Rationale**: When the BOQ is updated and activities re-generated, previously edited activities should retain user modifications. Activities are matched by their BOQ reference. If a BOQ reference exists in both old and new generations, the edited Name, Description, and custom fields are preserved.
- **Fields preserved**: Name (if user-modified), Description (if user-modified), accepted dependencies
- **Alternatives considered**:
  - Complete regeneration (loses all user edits; frustrating)
  - Match by Activity ID (breaks when activities are reordered or regrouped)

### 7. Excel Export Format

**Decision**: Export to a new sheet named "Generated Activities" within the active workbook; use existing Excel interop for cell-level styling

- **Rationale**: Keeping export in the same workbook maintains context for the engineer. The existing Excel interop in WpfApp2 handles cell formatting (fonts, colors, borders aligned with theme colors). Column order: Activity ID, Name, Description, BOQ Reference, Quantity, Unit, Dependencies.
- **Alternatives considered**:
  - New workbook (separates data from source; harder to cross-reference)
  - ClosedXML export (cannot modify the active unsaved workbook)

## Dependencies

| Dependency | Source | Purpose |
|------------|--------|---------|
| Som3a.Domain.BOQ.BOQDocument, BOQItem, Quantity, Unit | Phase 14 | BOQ data model |
| Som3a.Domain.WBS.WBSNode | Phase 14/19 | WBS hierarchy for sequencing |
| Som3a.AI.Orchestration.OrchestrationEngine | Phase 18 | AI activity generation |
| Som3a.AI.Prompts.PromptTemplateRegistry | Phase 18 | BOQ-to-activity prompt management |
| Som3a.AI.Parsers.ActivityParser | Phase 18 | Parse AI output to activity list |
| Excel VSTO Interop (Microsoft.Office.Interop.Excel) | WpfApp2 existing | BOQ reading + Excel export |
| ThemeManager / Shell | WpfApp2 existing | Plugin page hosting |
| ServiceContainer / EventBus | WpfApp2 existing | DI + event messaging |

## Best Practices

- AI generation calls are async with non-blocking status message and estimated time
- Progress indication uses Phase 18 orchestration progress events
- Data privacy consent is obtained before first AI call each session
- A 10-second cooldown is enforced between generation requests
- Grid validation runs on each cell edit (debounced) and on explicit validation request
- Export uses the active workbook's theme colors via ThemeManager
- All service dependencies injected via constructor (ServiceContainer)
- Validation messages are clear, actionable, and displayed inline in the grid
- Activities maintain stable identity across re-generation via BOQ reference matching
