# Feature Specification: WBS Engine

**Feature Branch**: `019-wbs-engine`

**Created**: 2026-05-26

**Status**: Draft

**Input**: User description: "Phase 19 — WBS Engine: Implement WBS template system, AI-powered WBS generation, and WBS export capabilities."

## User Scenarios & Testing

### User Story 1 - Browse and Select WBS Templates (Priority: P1)

A planning engineer opens the WBS workspace and browses pre-built WBS templates organized by project category (residential, commercial, industrial, infrastructure, MEP, industrial, fitout). Each template shows its name, category, level count, and a preview of the tree structure. The engineer selects a template as the starting point for their project. All template browsing works without AI connectivity.

**Why this priority**: Templates are the foundation of WBS generation — without a template library, engineers must build WBS structures from scratch with no starting point.

**Independent Test**: Can be tested by browsing all template categories, selecting a template, and verifying its tree structure is displayed with correct levels and naming.

**Acceptance Scenarios**:

1. **Given** the engineer opens the WBS workspace, **When** they view available templates, **Then** templates are organized by category (residential, commercial, industrial, infrastructure, MEP, fitout) with at least 3 templates per category
2. **Given** the engineer selects a template, **When** it is opened, **Then** the full WBS tree is displayed with Code, Name, Level, and Parent for each node
3. **Given** the engineer has an existing project context, **When** they browse templates, **Then** recommended templates are highlighted based on project type match

---

### User Story 2 - Generate WBS Using AI (Priority: P1)

A planning engineer provides a project description (project type, scope, industry standards) and triggers AI-powered WBS generation. The AI engine selects the most appropriate base template, generates the WBS structure with codes and names, and presents the result for review. The engineer can accept, modify, or regenerate the WBS. If no AI provider is available, the WBS engine works in manual-only mode — users can still browse templates, manually build WBS trees, and export without AI generation.

**Why this priority**: AI generation is the core value — it turns project descriptions into structured WBS trees, saving hours of manual effort.

**Independent Test**: Can be tested by entering a project description, triggering AI generation, and verifying the output is a valid WBSNode tree with correct Code, Name, Level, and Parent fields.

**Acceptance Scenarios**:

1. **Given** the engineer enters a project description with project type and scope, **When** they trigger AI generation, **Then** a valid WBSNode tree is produced with at least 3 levels of hierarchy
2. **Given** the AI generates a WBS, **When** the result is displayed, **Then** each node shows Code (auto-generated numbering 1, 1.1, 1.1.1), Name, Level, and Parent reference
3. **Given** the engineer is unsatisfied with the generated WBS, **When** they request regeneration, **Then** a new WBS is generated with varied structure
4. **Given** a generated WBS, **When** the engineer manually edits a node, **Then** the change is reflected immediately and child codes update accordingly
5. **Given** the WBS does not meet expectations, **When** the engineer accepts the result, **Then** the WBS is saved and becomes the active project WBS

---

### User Story 3 - Edit WBS Structure Manually (Priority: P2)

A planning engineer opens the WBS editor to modify an existing WBS tree. They can add, remove, rename, and reorder nodes at any level. The system validates tree integrity (no cycles, consistent level depth, naming conventions) and auto-generates WBS codes based on position in the hierarchy. Users can save a modified WBS tree as a new custom template, creating reusable structures for future projects.

**Why this priority**: Manual editing provides the fine-grained control engineers need to tailor WBS structures to specific project requirements after AI generation.

**Independent Test**: Can be tested by creating a new WBS tree from scratch, adding 5+ nodes across 3 levels, and verifying code auto-generation and tree validation.

**Acceptance Scenarios**:

1. **Given** the engineer opens the WBS editor, **When** they add a child node to an existing node, **Then** the new node is created with an auto-generated code (e.g., adding to 1.2 produces 1.2.1)
2. **Given** the engineer renames a node, **When** the change is submitted, **Then** the node name is updated and all child codes remain consistent
3. **Given** the engineer removes a node with children, **When** confirmed, **Then** the node and all descendants are removed, and sibling codes are renumbered
4. **Given** the engineer attempts to create a cycle, **When** validation runs, **Then** the system rejects the change with a clear error message

---

### User Story 4 - Export WBS (Priority: P2)

A planning engineer exports the final WBS tree for use in other tools. The system supports export to Excel (with indentation and grouping), JSON, and XML formats. WBS codes are included in the export with full path information.

**Why this priority**: Export enables integration with downstream workflows (Primavera scheduling, cost estimation, reporting).

**Independent Test**: Can be tested by creating a WBS tree with 3+ levels and exporting to each format, verifying the exported file contains correct structure and codes.

**Acceptance Scenarios**:

1. **Given** a completed WBS tree, **When** the engineer exports to Excel, **Then** the sheet shows the WBS with proper indentation, grouping, and auto-generated codes
2. **Given** a completed WBS tree, **When** the engineer exports to JSON, **Then** each node contains Code, Name, Level, Parent reference, and FullPath
3. **Given** a completed WBS tree, **When** the engineer exports to XML, **Then** the XML structure mirrors the WBS hierarchy with all node attributes

---

### Edge Cases

- What happens when the AI generates a WBS with more than 10 levels? The system should cap at a configurable maximum depth (default 10) and notify the user of truncation.
- How does the system handle a WBS template that has been deleted while an engineer is using it? The editor should maintain the in-memory copy and notify the user that the template is no longer available for future use.
- What happens when the project description is too vague for AI generation? The system should prompt the user for more specific information (project type, scope, key deliverables).
- How does the engine handle circular parent references in manual editing? Tree validation must detect and prevent cycles before save.
- What happens when an export file already exists at the target path? The system should prompt the user to overwrite or choose a new location.
- What happens when AI is unavailable during WBS generation? The system switches to manual-only mode; AI generation shows a clear message and users can build WBS trees manually.

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide pre-built WBS templates across 5 categories: Building (residential, commercial, industrial), Infrastructure (roads, bridges, utilities), MEP (mechanical, electrical, plumbing), Industrial (oil & gas, manufacturing), Fitout (office, retail, hospitality)
- **FR-002**: System MUST include at least 15 total templates with at least 3 templates per category
- **FR-003**: System MUST support template selection with preview of tree structure
- **FR-004**: AI-powered WBS generation MUST accept project description with project type, scope, and industry standards; system MUST work in manual-only mode (template browsing, manual editing, export) when AI is unavailable
- **FR-005**: AI generation MUST produce a valid WBSNode tree with Code, Name, Level, and Parent for each node
- **FR-006**: System MUST validate generated WBS trees for tree integrity (no cycles, consistent level depth, naming conventions)
- **FR-007**: System MUST assign each WBS node a stable internal GUID (immutable across moves/renames) and an auto-generated hierarchical Code (1, 1.1, 1.1.1) derived from tree position
- **FR-008**: System MUST support manual WBS editing: add, remove, rename, and reorder nodes at any level
- **FR-009**: System MUST update child codes when parent is renumbered or reordered
- **FR-010**: System MUST support WBS export to Excel with indentation and grouping
- **FR-011**: System MUST support WBS export to JSON and XML with full node attributes
- **FR-012**: System MUST support template import and export for sharing custom templates
- **FR-013**: System MUST limit WBS depth to a configurable maximum (default 10 levels)
- **FR-014**: System MUST recommend templates based on project type match when context is available
- **FR-015**: System MUST detect and prevent circular references in WBS tree during manual editing

### Key Entities

- **WBSTemplate**: A pre-built WBS structure template with name, category, version, and tree of WBSNode definitions
- **WBSNode**: Represents a node in the WBS tree with Id (stable internal GUID), Code (mutable hierarchical auto-generated 1, 1.1, 1.1.1), Name, Level, Parent, Children, and FullPath
- **WBSProject**: Represents the active project WBS with link to source template, AI generation context, modification history, and current tree state
- **WBSExportFormat**: Supported export formats (Excel, JSON, XML) with format-specific configuration

## Success Criteria

### Measurable Outcomes

- **SC-001**: Engineers can browse at least 15 WBS templates across 5 categories on first use
- **SC-002**: AI WBS generation from project description completes and returns a valid tree within 30 seconds
- **SC-003**: Generated WBS trees contain at least 3 levels of hierarchy for well-described projects
- **SC-004**: WBS code auto-generation produces correct hierarchical codes (1, 1.1, 1.1.1) for any tree depth
- **SC-005**: Manual WBS edits (add, remove, rename, reorder) take effect in under 1 second
- **SC-006**: Excel WBS export with indentation and grouping completes within 5 seconds for trees up to 200 nodes
- **SC-007**: JSON and XML exports produce valid, schema-conformant files for trees of any size
- **SC-008**: Tree validation catches 100% of circular references and depth limit violations

## Clarifications

### Session 2026-05-26

- Q: Template customization & creation workflow → A: Users create custom templates by modifying a system template in the WBS editor and saving as a new template (no separate template designer)
- Q: AI unavailability fallback → A: AI is optional; WBS engine works in manual-only mode (browse templates, manual edit, export) when AI is unavailable; AI generation shows clear error
- Q: WBS node identity model → A: Dual identity — stable internal GUID per node for references, mutable hierarchical Code (1, 1.1, 1.1.1) derived from tree position for display

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- WBS templates are stored as structured data (JSON) in the application data folder, managed by the template registry
- AI WBS generation builds on Phase 18 AI Core Infrastructure (IOrchestrationEngine, PromptTemplateRegistry, WBSParser)
- The WBS domain entity (WBSNode) already exists in Som3a.Domain from Phase 14 and is reused without modification
- Template editor is implemented as a WPF page within the existing Shell workspace (not a standalone window)
- WBS templates are read-only system defaults; users cannot modify built-in templates (only create custom ones)
- Excel export uses the existing Excel interop/integration infrastructure from the WpfApp2 host
- The 5 template categories (Building, Infrastructure, MEP, Industrial, Fitout) with 3+ templates each = 15 total minimum
- AI generation requires internet connectivity to reach configured AI providers from Phase 18
- WBS code auto-generation follows the industry standard hierarchical numbering convention (1, 1.1, 1.1.1)
