# Research: WBS Engine

**Feature**: WBS Engine (Phase 19)
**Date**: 2026-05-26

## Decisions

### 1. Excel Export Library

**Decision**: Use ClosedXML for Excel (.xlsx) export

- **Rationale**: ClosedXML is an MIT-licensed .NET library for creating Excel files without Excel interop. It supports styling (indentation, grouping), worksheets, and is well-maintained. EPPlus is also viable but requires a commercial license for certain use cases. ClosedXML avoids dependency on Excel VSTO interop and works reliably in the add-in process.
- **Alternatives considered**:
  - EPPlus (commercial license required for v5+; free for v4.5 but outdated)
  - Excel VSTO interop (requires Excel installed; slow for batch export)
  - Manual XML generation (error-prone; no built-in styling support)

### 2. WBS Template Storage

**Decision**: Store templates as individual JSON files in `%APPDATA%\Som3a\wbs-templates\`

- **Rationale**: JSON is human-readable, supports schema evolution, and integrates with System.Text.Json. File-per-template allows independent versioning. Follows the same pattern as Phase 18's PromptTemplateRegistry.
- **Alternatives considered**:
  - Embedded resources (built-in templates only; cannot add custom templates)
  - SQLite (overkill for template storage; adds dependency)
  - Single JSON file (merge conflicts; harder to manage individually)

### 3. AI WBS Generation Prompt

**Decision**: Use a dedicated WBS prompt template in the Phase 18 PromptTemplateRegistry with category "WBS"

- **Rationale**: The Phase 18 infrastructure already supports categorized prompt templates with JSON Schema output. A WBS-specific template with output schema for WBSNode tree structure leverages existing validation and streaming support.
- **Schema required**:
  ```json
  { "type": "object", "required": ["wbs"], "properties": { "wbs": { ... recursive WBSNode } } }
  ```
- **Alternatives considered**:
  - Direct API calls bypassing Phase 18 (duplicates infrastructure; loses token tracking, failover, retry)
  - Rule-based generation without AI (limited to template selection only; no custom generation)

### 4. WBS Code Generation

**Decision**: Recursive depth-first numbering with immediate sibling renumbering

- **Rationale**: Standard hierarchical numbering (1, 1.1, 1.1.1) is industry convention. When a sibling is inserted or removed, all following siblings at the same level are renumbered. This matches Primavera and MS Project behavior.
- **Algorithm**: Depth-first traversal. Each node's Code = Parent.Code + "." + SiblingIndex (1-based). Root nodes get simple integers. On every tree mutation, only affected subtrees are renumbered.
- **Alternatives considered**:
  - Level-based codes (e.g., L1-01, L2-01) — non-standard; confusing for Primavera users
  - UUID-based codes — human-unfriendly for display and export

### 5. Tree Validation Strategy

**Decision**: Validate on every tree mutation (add, remove, reorder, AI import)

- **Rationale**: Immediate validation prevents invalid states from propagating. Validation covers: cycle detection (DFS with visited set), depth limit (configurable max, default 10), parent-child type consistency.
- **Alternatives considered**:
  - Batch validation on save only (bad UX; user can build invalid trees)
  - Database-level constraints (not applicable; in-memory tree)

### 6. Template Recommendation Logic

**Decision**: Keyword-based matching from project description to template category tags

- **Rationale**: Simple and predictable. The project description is parsed for keywords matching template category tags (e.g., "building", "residential" → Building category; "bridge", "road" → Infrastructure). Templates with highest match count are highlighted.
- **Alternatives considered**:
  - AI-powered recommendation (too heavy; AI already used for generation)
  - User-preference based (requires additional profile storage and learning)

### 7. WBSNode Entity Extension

**Decision**: Add `Id` (GUID) field to existing WBSNode; keep backward compatibility

- **Rationale**: The existing WBSNode in Som3a.Domain already has Id field (Guid), Level (calculated), Parent, Children, Code, Name, FullPath. The existing structure already supports the dual identity model (Id + Code) — no schema change needed.
- **Fields already present**: Id (GUID), Code (string), Name, Level (calculated), Parent, Children, FullPath, Activities

## Dependencies

| Dependency | Source | Purpose |
|------------|--------|---------|
| Som3a.Domain.WBS.WBSNode | Phase 14 | WBS tree data model |
| Som3a.AI.Orchestration.OrchestrationEngine | Phase 18 | AI WBS generation |
| Som3a.AI.Prompts.PromptTemplateRegistry | Phase 18 | WBS prompt template management |
| Som3a.AI.Parsers.WBSParser | Phase 18 | Parse AI output to WBSNode tree |
| ClosedXML | NuGet | Excel export |
| System.Text.Json | .NET 8.0 built-in | Template serialization |
| ThemeManager / Shell | WpfApp2 existing | WBS page hosting |

## Best Practices

- WBS editor page uses VirtualizingTreeView or similar for large trees
- Tree mutations are undoable (Command pattern) for at least one level
- AI generation calls are async with progress indicator and cancellation support
- Template import validates file format before loading
- Excel export uses separate background thread to avoid UI blocking
