# Feature Specification: BOQ Activity Generator

**Feature Branch**: `020-boq-activity-generator`

**Created**: 2026-05-27

**Status**: Draft

**Input**: User description: "Phase 20 — BOQ Activity Generator Plugin: AI-powered generation of construction activities from Excel BOQ data — the first user-visible AI feature delivering immediate planning engineer value."

## User Scenarios & Testing

### User Story 1 - Generate Activities from BOQ Using AI (Priority: P1)

A planning engineer opens the BOQ Activity Generator workspace and selects an active Excel workbook containing BOQ data. The system reads the BOQ sheet, identifies BOQ items with their quantities, units, and classifications, and builds an AI context. The engineer triggers AI generation, and the system produces a structured list of construction activities — intelligently grouping similar BOQ items into single activities, applying verb-noun naming conventions, and linking each activity to its source BOQ items. The entire flow works even if the engineer only needs to re-generate activities after editing the BOQ.

**Why this priority**: Activity generation from BOQ is the core value of this phase — it transforms raw bill-of-quantities data into a structured activity list that forms the foundation of construction scheduling, saving engineers hours of manual work.

**Independent Test**: Can be tested by loading an Excel workbook with a valid BOQ sheet, triggering AI generation, and verifying the output is a list of activities each with Activity ID, Name, Description, BOQ Reference, Quantity, and Unit.

**Acceptance Scenarios**:

1. **Given** the engineer opens the BOQ Activity Generator and selects a workbook with BOQ data, **When** the system reads the BOQ, **Then** all BOQ items, quantities, and classifications are parsed and displayed in a preview
2. **Given** BOQ data has been loaded and the engineer triggers AI generation for the first time in the session, **When** the system displays a data privacy notice, **Then** the engineer must explicitly consent to proceed with AI generation
3. **Given** a BOQ has been successfully read and consent provided, **When** the engineer triggers AI generation, **Then** a list of at least 50 activities is produced from a typical construction BOQ
4. **Given** the AI generates activities, **When** the result is displayed, **Then** each activity has an Activity ID (sequential), Name (verb-noun format), Description, BOQ Reference, Quantity, and Unit
5. **Given** multiple similar BOQ items exist (e.g., same material across different floors), **When** AI generates activities, **Then** similar items are grouped into a single activity where appropriate
6. **Given** the engineer triggers AI generation, **When** generation is in progress, **Then** a non-blocking status message with estimated time is displayed and the engineer can continue other work
7. **Given** the engineer regenerates activities after editing the BOQ, **When** generation completes, **Then** previously edited activities retain their user modifications where the BOQ reference still exists

---

### User Story 2 - Review and Edit Generated Activities (Priority: P1)

After AI generation, the engineer is presented with an interactive grid showing all generated activities. The grid supports in-place editing of activity names, descriptions, quantities, and units. The system runs validation checks: duplicate activity detection, naming convention compliance (verb-noun format), missing BOQ references, and quantity consistency against source BOQ data. Validation errors are highlighted inline with clear messages. The engineer can fix issues directly in the grid and re-run validation to confirm corrections.

**Why this priority**: Validation and editability are critical — engineers must trust the AI output and have full control to correct it before using activities downstream.

**Independent Test**: Can be tested by generating activities from a BOQ that contains known issues (duplicate items, inconsistent quantities), then verifying the validation grid highlights each issue and the engineer can correct them inline.

**Acceptance Scenarios**:

1. **Given** activities have been generated, **When** the engineer views the grid, **Then** all activities are displayed in editable rows with columns for ID, Name, Description, BOQ Reference, Quantity, and Unit
2. **Given** the engineer edits an activity name in the grid, **When** the change is submitted, **Then** the grid reflects the new value immediately
3. **Given** a duplicate activity is detected, **When** validation runs, **Then** the duplicate is highlighted with a warning and the engineer can merge or remove it
4. **Given** an activity name does not follow verb-noun convention, **When** validation runs, **Then** a naming convention warning is shown on the affected row
5. **Given** an activity references a BOQ item that no longer exists, **When** validation runs, **Then** a missing reference error is shown
6. **Given** an activity quantity differs significantly from its source BOQ quantity, **When** validation runs, **Then** a quantity inconsistency warning is displayed

---

### User Story 3 - Sequence Activities and Suggest Dependencies (Priority: P2)

The engineer reviews and sequences the generated activities by WBS hierarchy and trade order. The system detects which activities can run in parallel (e.g., different building zones) versus sequentially (e.g., foundation before structure). Basic dependency suggestions are offered (e.g., "Excavation must precede Foundation"). The engineer can accept, modify, or reject dependency suggestions. The sequencing is preserved when activities are re-exported or re-generated.

**Why this priority**: Sequencing provides the logical flow between activities, transforming a flat list into a structured schedule that can feed the Relationship Generator (Phase 21).

**Independent Test**: Can be tested by generating activities from a multi-trade BOQ, then verifying the system suggests at least 5 logical dependencies and the engineer can accept/reject each one.

**Acceptance Scenarios**:

1. **Given** activities are generated and validated, **When** the engineer opens the sequencing view, **Then** activities are ordered by WBS hierarchy and trade sequence
2. **Given** activities from different trades or zones, **When** the system analyzes the list, **Then** parallel activity groups are identified and displayed
3. **Given** the system identifies sequential relationships, **When** dependency suggestions are presented, **Then** each suggestion includes a confidence indicator and a clear rationale
4. **Given** dependency suggestions are presented, **When** the engineer accepts a suggestion, **Then** the dependency is added to the activity relationship
5. **Given** sequencing and dependencies are set, **When** activities are re-generated from updated BOQ, **Then** previously accepted dependencies are preserved for matching activities

---

### User Story 4 - Export Generated Activities to Excel (Priority: P2)

After review and sequencing, the engineer exports the final activity list to a dedicated Excel sheet within the same workbook. The export includes Activity ID, Name, Description, BOQ Reference, Quantity, Unit, and any dependency information. The exported sheet uses consistent styling matching the application theme.

**Why this priority**: Export closes the loop — engineers take the generated activities back into their workflow for review, approval, and downstream scheduling in Primavera or other planning tools.

**Independent Test**: Can be tested by generating and sequencing activities, then exporting to an Excel sheet and verifying the sheet contains all expected columns with correct data and proper styling.

**Acceptance Scenarios**:

1. **Given** a validated and sequenced activity list, **When** the engineer exports to Excel, **Then** a new sheet is created in the workbook with columns: Activity ID, Name, Description, BOQ Reference, Quantity, Unit, Dependencies
2. **Given** an export is triggered, **When** the Excel sheet is created, **Then** the sheet styling (fonts, colors, borders) matches the application theme
3. **Given** the engineer edits exported activities in Excel and returns to the generator, **When** they trigger re-generation, **Then** user edits in the exported sheet are preserved for activities whose BOQ references still exist
4. **Given** an export file already exists from a previous generation, **When** the engineer exports again, **Then** the previous sheet is updated (not duplicated) and the engineer is asked to confirm

---

### Edge Cases

- What happens when the selected Excel sheet contains no recognizable BOQ structure? The system should display a clear error message with guidance on expected BOQ format (items, quantities, units, classifications).
- How does the system handle a BOQ with thousands of items that could generate more activities than the AI context window allows? The system should optimize context by grouping and truncating low-priority items, with a notification to the engineer about any items excluded from generation.
- What happens when the AI returns activities that exceed the maximum supported activity count? The system should cap at a configurable maximum (default 500 activities) and notify the engineer of truncation.
- How does the system handle BOQ items with zero or negative quantities? Items with zero quantities should be flagged for review; negative quantities should be rejected with a validation error.
- What happens when all AI providers are unavailable during generation? The system should show a clear error message indicating AI unavailability and suggest retrying later; the engineer can still view previously generated activities and edit them manually.
- How does the system handle an activity name that is edited to be empty? The grid should reject empty names with an inline validation error.
- What happens when the engineer closes the generator workspace with unsaved edits? The system should prompt the engineer to save or discard changes before closing.
- What happens when the engineer declines the data privacy consent? The system should not send any data to AI providers, show a clear message that AI generation is unavailable, and allow the engineer to manually enter or edit activities instead.
- What happens when the engineer triggers re-generation within 10 seconds of the previous generation? The system should show a cooldown warning with remaining time and block the request until the cooldown expires.

## Requirements

### Functional Requirements

- **FR-001**: System MUST read BOQ data from the active Excel workbook, parsing items with their quantities, units, and classifications
- **FR-002**: System MUST build an AI context from the parsed BOQ data, optimized for token limits with intelligent item grouping and truncation
- **FR-003**: System MUST identify activity candidates from BOQ item groupings, grouping similar items (e.g., same material, different floors) into single activities where appropriate
- **FR-004**: AI activity generation MUST produce a list of activities from the BOQ context, each with Activity ID, Name, Description, BOQ Reference, Quantity, and Unit
- **FR-005**: System MUST display a non-blocking status message with estimated time during AI generation; the engineer may continue other work while generation runs in background
- **FR-006**: Activity names MUST follow verb-noun naming convention (e.g., "Pour Concrete Foundation", not "Concrete Foundation Pour")
- **FR-007**: System MUST validate generated activities for: duplicate detection, naming convention compliance, missing BOQ references, and quantity consistency
- **FR-008**: System MUST provide an editable grid view where engineers can review and modify all activity fields inline
- **FR-009**: System MUST highlight validation errors and warnings inline in the grid with clear, actionable messages
- **FR-010**: System MUST order activities by WBS hierarchy and trade sequence in the sequencing view
- **FR-011**: System MUST detect parallel activity groups and suggest sequential dependencies between activities
- **FR-012**: Dependency suggestions MUST include a confidence indicator and rationale
- **FR-013**: Engineers MUST be able to accept, modify, or reject dependency suggestions per dependency
- **FR-014**: System MUST export the final activity list (including dependencies) to a dedicated Excel sheet within the workbook
- **FR-015**: Exported Excel sheet MUST include columns: Activity ID, Name, Description, BOQ Reference, Quantity, Unit, and Dependencies
- **FR-016**: Exported Excel sheet styling MUST be consistent with the application theme
- **FR-017**: System MUST preserve user edits to activities when re-generation is triggered, matching by BOQ reference where available
- **FR-018**: System MUST preserve accepted dependencies when activities are re-generated, matching by stable activity reference
- **FR-019**: System MUST support a configurable maximum activity count (default 500) with notification of truncation
- **FR-020**: System MUST reject BOQ items with negative quantities and flag zero-quantity items for review
- **FR-021**: System MUST prompt the engineer to save or discard changes when closing the generator workspace with unsaved edits
- **FR-022**: System MUST display a data privacy notice describing what BOQ data will be sent to AI providers and obtain opt-in consent before the first AI generation attempt each session
- **FR-023**: System MUST enforce a minimum 10-second cooldown between AI generation requests and display a warning if the user attempts to regenerate before the cooldown elapses

### Key Entities

- **BOQContext**: Represents the parsed BOQ data from Excel, containing items with descriptions, quantities, units, and classifications, optimized for AI consumption
- **GeneratedActivity**: A construction activity produced by the AI engine, with Activity ID (sequential), Name (verb-noun), Description, BOQ Reference(s), Quantity, Unit, and validation status
- **ActivityValidationResult**: Contains validation outcomes for each activity, including duplicate flags, naming convention issues, missing references, and quantity consistency warnings
- **ActivitySequenceOrder**: Defines the ordering of activities by WBS hierarchy and trade sequence, including parallel group membership and dependency relationships
- **ActivityDependency**: A suggested or accepted predecessor/successor relationship between two activities, with confidence indicator and rationale
- **ActivityExportConfig**: Configuration for Excel export including target sheet name, column layout, and styling parameters

## Success Criteria

### Measurable Outcomes

- **SC-001**: Engineers can load a BOQ from an Excel sheet and generate at least 50 construction activities from a typical BOQ within 60 seconds
- **SC-002**: Generated activity names follow verb-noun convention with at least 95% compliance automatically
- **SC-003**: Validation catches 100% of duplicate activities, missing BOQ references, and negative quantities
- **SC-004**: Engineers can review and edit any activity field in the grid within 1 second of clicking the cell
- **SC-005**: The system suggests at least 5 logical dependencies from a multi-trade BOQ with reasonable accuracy (no contradictory dependencies)
- **SC-006**: Excel export produces a correctly formatted sheet with all 7 columns (ID, Name, Description, BOQ Reference, Quantity, Unit, Dependencies) in under 5 seconds
- **SC-007**: User edits to activities are preserved across re-generation cycles with at least 95% accuracy for matching BOQ references
- **SC-008**: The feature completes successfully in Excel VSTO host without rendering issues or crashes

## Clarifications

### Session 2026-05-27

- Q: What progress feedback should the user see during AI generation? → A: Non-blocking status message with estimated time; user can continue other work
- Q: What data privacy or confidentiality measures for BOQ data sent to AI providers? → A: Show a clear data notice with opt-in consent before first AI generation each session
- Q: Should the system limit the frequency of AI generation requests? → A: Yes, debounce with a minimum 10-second cooldown between generations and warn the user

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- BOQ data is structured in Excel sheets with columns for Item Description, Quantity, Unit, and Classification, as is standard in construction industry BOQ documents
- AI activity generation builds on Phase 18 AI Core Infrastructure (IOrchestrationEngine, PromptTemplateRegistry, ActivityParser)
- The BOQ domain entities (BOQDocument, BOQSection, BOQItem, Quantity, Unit) already exist in Som3a.Domain from Phase 14 and are reused
- The WBS Engine from Phase 19 provides WBS hierarchy context for activity sequencing
- The activity generator is implemented as a WPF plugin page within the existing Shell workspace (not a standalone window)
- Excel export uses the existing Excel interop/integration infrastructure from the WpfApp2 host
- AI generation requires internet connectivity to reach configured AI providers from Phase 18; the generator shows a clear message when AI is unavailable
- Activity naming follows verb-noun convention by default (e.g., "Excavate Foundation", "Pour Concrete Slab")
- Maximum activity count is configurable with a default of 500 activities per generation
- A typical construction BOQ contains 200-500 line items that produce 50-150 activities after intelligent grouping
