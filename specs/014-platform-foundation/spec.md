# Feature Specification: Platform Foundation

**Feature Branch**: `feature/phase-14-platform-foundation`

**Created**: 2026-05-25

**Status**: Draft

**Input**: User description: "Phase 14 — Platform Foundation — Establish the architectural backbone: Domain layer, Contracts, Plugin SDK, AI Abstraction Layer, and Export Layer."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Domain Model Ready for Planning Features (Priority: P1)

Planning engineers need a structured, in-memory representation of construction planning data — Bills of Quantities (BOQ), Activities, Work Breakdown Structures (WBS), and Relationships — that is independent of any specific data source (Excel, databases, import files). Without this domain model, every feature must parse raw Excel rows repeatedly, making AI-powered features unreliable and plugin development impossible.

**Why this priority**: The domain model is the single source of truth for all planning data. Every subsequent phase (WBS Engine, BOQ Activity Generator, Relationship Generator, Duration Estimator) depends on these entities. Without P1, no AI or planning feature can be built with confidence.

**Independent Test**: A planning engineer can instantiate domain entities (BOQDocument, Activity, WBSNode, Relationship) in code, serialize them to JSON, and verify the structure mirrors real planning data (e.g., an activity has an ID, description, quantity, duration, and links to its WBS node and predecessor relationships).

**Acceptance Scenarios**:

1. **Given** a BOQ with 3 sections and 50 line items read from Excel, **When** mapped into the domain model, **Then** the BOQDocument entity contains 3 BOQSection entities, each with correct BOQItem entities preserving quantity, unit, description, and classification.
2. **Given** a list of 100 Activity entities, **When** serialized to JSON and deserialized back, **Then** all properties are preserved exactly, including nested WBS references and relationship links.
3. **Given** a WBS structure with 4 levels (Project → Phase → Zone → Task), **When** traversed via the Parent/Children properties, **Then** the full path "Project/Phase/Zone/Task" is reconstructed correctly.
4. **Given** a relationship between Activity A (predecessor) and Activity B (successor) with type "FS" and lag "+3 days", **When** queried, **Then** the relationship entity reports correct predecessor, successor, type, and lag values.

---

### User Story 2 - Plugin Developers Can Build Against Stable Contracts (Priority: P1)

Plugin developers need well-defined interfaces (contracts) for AI providers, export engines, settings modules, and navigation — so they can build features without knowing implementation details. The contracts establish the "language" plugins and the platform speak.

**Why this priority**: Without contracts, plugins couple to implementation details and break when the platform changes. Contracts are the architectural boundary that enables parallel development of the platform core and its plugins. Equal priority with P1 because the domain model is unusable without contracts to interact with it.

**Independent Test**: A developer writes a minimal plugin that implements `IPlugin` and `IAIProvider`, registers it, and receives typed responses from the AI abstraction layer without knowing which AI provider (OpenAI, Claude, etc.) is actually serving the request.

**Acceptance Scenarios**:

1. **Given** a plugin implementing `IPlugin`, **When** the platform discovers and loads it, **Then** the plugin's `Initialize`, `RegisterSettings`, and `LoadUI` methods are called in the correct lifecycle order.
2. **Given** a plugin calling `IAIProvider.ExecutePrompt(prompt, schema)`, **When** the AI provider responds, **Then** the plugin receives a structured result matching the provided JSON schema, regardless of which provider (OpenAI, Claude, etc.) serviced the request.
3. **Given** a plugin calling `ISettingsModule.RegisterSettings(section)`, **When** the settings page opens, **Then** the plugin's settings controls appear in the correct category without the plugin knowing how the settings UI is rendered.
4. **Given** a plugin calling `IExportEngine.Export(data, target)`, **When** the export completes, **Then** data is written to the target format (Excel, CSV, JSON) without the plugin handling format-specific logic.

---

### User Story 3 - New AI Features Can Be Added Without Platform Changes (Priority: P2)

Product owners and planning engineers want to add new AI-powered features (e.g., risk analysis, schedule optimization) without waiting for platform-level code changes. The Plugin SDK enables external teams or customers to develop, package, and deploy plugins that the platform discovers and loads automatically.

**Why this priority**: While AI features are built in later phases, the Plugin SDK must exist first so those features have a standard way to integrate. P2 because no existing users are blocked — this is future-enablement.

**Independent Test**: A developer creates a new plugin assembly in the Plugins directory, restarts the platform, and sees the new plugin listed in the Plugin Status dashboard with its navigation items, settings, and commands registered.

**Acceptance Scenarios**:

1. **Given** a plugin assembly placed in the Plugins directory decorated with `[Plugin(Id="RiskAnalyzer", Name="Risk Analyzer", Version="1.0")]`, **When** the platform starts, **Then** the plugin is automatically discovered, validated, and registered.
2. **Given** a plugin with the `[NavigationItem(Category="AI", Icon="Shield", Order=3)]` attribute, **When** the sidebar renders, **Then** the "Risk Analyzer" item appears under the AI category with the shield icon.
3. **Given** a plugin with `[SettingsSection(Category="AI", Order=2)]` on its settings class, **When** the settings page opens, **Then** an AI section appears with the plugin's settings controls, without any platform code changes.
4. **Given** a plugin that throws an unhandled exception during initialization, **When** the platform loads it, **Then** the plugin is marked as unhealthy in the diagnostics dashboard, but all other plugins continue to function normally.

---

### User Story 4 - Planning Data Can Be Exported to Multiple Formats (Priority: P2)

Planning engineers need to export domain data (activities, WBS, relationships) to multiple formats: Excel sheets for sharing with teams, CSV for data analysis, JSON for integration with other tools, and Primavera-compatible tables for scheduling software.

**Why this priority**: The export engine is foundational infrastructure. While export features are built in later phases (WBS export in Phase 19, activity export in Phase 20), the pipeline must exist first. P2 because no users are blocked — P1 domain+P1 contracts enable the planning features; export follows.

**Independent Test**: A developer passes a list of Activity entities to the export pipeline, specifies "Excel" as the target, and receives a validated, styled Excel sheet without writing any Excel-specific code.

**Acceptance Scenarios**:

1. **Given** a list of 50 Activity entities, **When** exported to Excel via the engine, **Then** a multi-sheet workbook is generated with proper column headers, data types, and styling.
2. **Given** the same list, **When** exported to CSV, **Then** a properly delimited CSV file is produced with headers matching column names.
3. **Given** the same list, **When** exported to JSON, **Then** a valid JSON array is produced preserving all entity properties and nested relationships.
4. **Given** an export with a known validation error (e.g., missing required field), **When** the pre-export validator runs, **Then** the export is blocked with a clear error message identifying the issue.

---

### User Story 5 - AI Providers Are Abstracted and Interchangeable (Priority: P3)

The platform must support multiple AI providers (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex) through a unified interface, with automatic failover if the primary provider is unavailable. Planning engineers should not need to know which AI model processes their request.

**Why this priority**: AI-powered features (Phases 19-22) depend on this abstraction, but no users are blocked during P1/P2 implementation. P3 because it is enabling infrastructure that can be validated independently before any AI features exist.

**Independent Test**: A developer configures two AI providers (primary: Claude, fallback: DeepSeek), sends a prompt, and when the primary provider is intentionally made unreachable, the orchestration layer routes to the fallback provider and returns a valid response.

**Acceptance Scenarios**:

1. **Given** provider "Claude" configured as primary and "DeepSeek" as secondary, **When** a prompt is sent and Claude returns an error, **Then** the orchestrator automatically retries with DeepSeek and returns the successful response to the caller.
2. **Given** a long-running AI request, **When** the response is streaming, **Then** partial results are delivered to the caller incrementally rather than waiting for the full response.
3. **Given** a session tracking 5 AI requests, **When** the session ends, **Then** total token usage and estimated cost are reported per provider.
4. **Given** a prompt template with a defined JSON schema, **When** the AI response is parsed, **Then** the output is validated against the schema and any schema violations are reported clearly.

---

### Edge Cases

- What happens when a .NET 8.0 library is referenced from the .NET Framework 4.8 WPF host? Interop bridge failure must trigger graceful degradation: platform remains usable for existing features (theme, navigation, Excel tools), AI and export features show a clear disabled/unavailable status, and the failure is logged to diagnostics.
- What happens when a plugin references a contract version that doesn't match the platform version?
- What happens when the BOQ has 10,000+ line items — can the domain model handle the volume without excessive memory? Target: 50,000 BOQ items must be loadable, queryable, and serializable without exceeding 500MB memory on standard enterprise PC (16GB RAM).
- What happens when two plugins register conflicting navigation item orders or settings section names? Resolution: last-registered wins, with a diagnostic warning logged so developers can adjust priority/order attributes.
- What happens when an AI provider returns a malformed response that doesn't match the expected JSON schema?
- What happens when the Plugin SDK discovers an assembly that isn't a valid plugin?
- What happens when export target path is inaccessible or read-only?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a domain model with entities for BOQ (BOQDocument, BOQSection, BOQItem including quantity, unit, description, classification).
- **FR-002**: System MUST provide a domain model with entities for Activities (ActivityId, name, description, quantity, unit, duration, productivity, relationships, constraints, WBS reference).
- **FR-003**: System MUST provide a domain model with entities for WBS (WBSNode with code, name, level, parent, children, full path).
- **FR-004**: System MUST provide a domain model with entities for Relationships (predecessor, successor, type FS/SS/FF/SF, lag value, validation status).
- **FR-005**: System MUST provide a domain model with entities for Calendars (calendar name, work days, holidays, working hours per day).
- **FR-006**: System MUST provide a domain model with entities for Constraints (start-on, finish-on, float, mandatory start/finish).
- **FR-007**: System MUST provide a domain model with entities for Resources (resource name, type, cost rate, assignment, budget).
- **FR-008**: System MUST define the IPlugin contract with lifecycle methods: Initialize, Register, LoadUI, RegisterSettings, RegisterCommands.
- **FR-009**: System MUST define the IAIProvider contract with methods for prompt execution, streaming, and output validation.
- **FR-010**: System MUST define the IExportEngine contract with methods for validation, generation, styling, and export to multiple formats.
- **FR-011**: System MUST define the ISettingsModule contract with methods for registering settings, validating values, exporting, and importing configurations.
- **FR-012**: System MUST define the IPromptProvider contract with methods for retrieving templates, building context, and validating prompts.
- **FR-013**: System MUST define the IDiagnosticsProvider contract with methods for collecting snapshots, reporting health, and logging diagnostics.
- **FR-014**: System MUST provide a Plugin SDK with attributes for [Plugin], [SettingsSection], [NavigationItem], and [Command] registration.
- **FR-015**: System MUST provide a PluginDiscoverer that scans assemblies in the Plugins directory and identifies valid plugins.
- **FR-016**: System MUST provide a PluginValidator that verifies contracts, checks dependencies, and validates plugin versions.
- **FR-017**: System MUST provide a PluginHost that manages plugin lifecycle (load, initialize, start, stop, unload) with error isolation between plugins.
- **FR-018**: System MUST provide an AI abstraction layer with provider adapters for OpenAI, Claude, DeepSeek, GLM, Kimi, and Codex.
- **FR-019**: System MUST provide an AIOrchestrator that handles provider routing, automatic failover, retry logic, and response parsing.
- **FR-020**: System MUST provide a TokenTracker that records token usage per request, per session, and per provider.
- **FR-021**: System MUST provide a StructuredOutputParser that converts AI responses into typed domain entities with JSON Schema validation.
- **FR-022**: System MUST provide an ExportPipeline that sequences validation, review, generation, styling, and export steps.
- **FR-023**: System MUST provide exporters for Excel (multi-sheet with styling), CSV, JSON, and XML formats.
- **FR-024**: System MUST provide a PrimaveraCompatibleWriter that formats data into Primavera-compatible table layouts.
- **FR-025**: All new .NET 8.0 class libraries MUST compile and interoperate with the existing .NET Framework 4.8 WPF host via an interop bridge.
- **FR-026**: All domain entities MUST be serializable to and deserializable from JSON without data loss.
- **FR-027**: System MUST encrypt API keys at rest using Windows DPAPI (Data Protection API) tied to the current user account, ensuring keys are never stored in plain text on disk.
- **FR-028**: System MUST handle interop bridge failures with graceful degradation: existing platform features (theme, navigation, Excel tools) remain fully functional; AI and export features display a clear unavailable status; failure is logged to diagnostics.
- **FR-029**: System MUST resolve plugin registration conflicts (navigation order, settings section names) using a last-registered-wins rule with a diagnostic warning logged for each conflict.
- **FR-030**: All .NET 8.0 libraries (Domain, AI, Exporting, Plugin.SDK) MUST emit structured log events through the interop bridge to the existing DiagnosticsService in the WPF host, reusing the existing log storage path (AppData/Som3a/Logs) and rotation policy (5MB rollover, 3-file rotation).

### Key Entities

- **BOQDocument**: Represents an entire Bill of Quantities. Contains multiple BOQSection entities. Key attributes: ProjectName, CreatedDate, TotalItems.
- **BOQSection**: A logical grouping within a BOQ. Contains BOQItem entities. Key attributes: SectionName, SectionCode, DisplayOrder.
- **BOQItem**: A single line item in a BOQ. Key attributes: ItemCode, Description, Quantity, Unit, UnitPrice, Classification, BOQReference.
- **Activity**: A construction work activity derived from or related to BOQ items. Key attributes: ActivityId, Name, Description, Quantity, Unit, Duration, ProductivityRate, WBSNode, Relationships (list).
- **WBSNode**: A node in a Work Breakdown Structure tree. Key attributes: Code (e.g., "1.2.3"), Name, Level, Parent (reference), Children (list), FullPath.
- **Relationship**: A dependency link between two activities. Key attributes: Predecessor (Activity), Successor (Activity), Type (enum: FS/SS/FF/SF), Lag (TimeSpan), ValidationStatus.
- **Calendar**: A work calendar definition. Key attributes: Name, WorkDays (set of days), Holidays (list of dates), WorkingHoursPerDay, TimeZone.
- **Constraint**: A scheduling constraint on an activity. Key attributes: Activity, ConstraintType (enum), ConstraintDate, FloatValue.
- **Resource**: A resource that can be assigned to activities. Key attributes: Name, ResourceType, CostPerHour, Budget, Assignments (list).
- **PluginDescriptor**: Metadata for a discovered plugin. Key attributes: Id, Name, Version, Priority, Dependencies, AssemblyPath, Status (Loaded/Error/Disabled).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A domain entity (e.g., Activity) can be instantiated, populated with 20 properties, serialized to JSON, deserialized back, and all properties verified in under 1 second on standard hardware.
- **SC-002**: A plugin implementing IPlugin and decorated with attributes is discovered and registered within 5 seconds of application startup.
- **SC-003**: When the primary AI provider fails, the orchestrator fails over to the secondary provider within the retry timeout (maximum 30 seconds total across all retry attempts).
- **SC-004**: Exporting 500 Activity entities to an Excel workbook completes within 10 seconds on standard hardware.
- **SC-005**: All 7 new .NET 8.0 class library projects compile successfully with zero errors in a single build pass.
- **SC-006**: The domain model correctly represents all 7 entity types (BOQ, Activity, WBS, Relationship, Calendar, Constraint, Resource) with complete properties as defined in the data model.
- **SC-007**: Plugin error isolation: when one plugin throws an unhandled exception, the remaining plugins continue to function with zero impact.
- **SC-008**: A developer unfamiliar with the codebase can create a minimal plugin implementing IPlugin in under 30 minutes following the Plugin SDK documentation.
- **SC-009**: The domain model loads, queries, and serializes 50,000 BOQ items without exceeding 500MB memory on standard enterprise hardware (16GB RAM).

## Clarifications

### Session 2026-05-25

- Q: What is the minimum acceptable standard for API key protection at rest? → A: DPAPI-encrypted (Windows Data Protection API tied to user account) — strong protection, zero config, works offline.
- Q: When the interop bridge between .NET 4.8 host and .NET 8.0 libraries fails, what should happen? → A: Graceful degradation — platform remains usable for non-AI/export features, AI/export features show clear disabled status, error logged to diagnostics.
- Q: What is the target data volume for the in-memory domain model (BOQ line items)? → A: 50,000 items — covers large infrastructure projects while remaining realistic for standard enterprise PC (16GB RAM).
- Q: How should the platform resolve plugin conflicts (e.g., two plugins registering same navigation order or settings section name)? → A: Last-registered wins with diagnostic warning — deterministic, visible to developers, no user intervention needed.
- Q: How should the new .NET 8.0 libraries handle observability (logging, diagnostics)? → A: Structured logging via shared diagnostics channel — .NET 8.0 libraries emit structured events through the interop bridge to the existing DiagnosticsService in the WPF host, reusing existing log infrastructure (AppData/Som3a/Logs, 5MB rollover).

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All new class libraries MUST be modular and independently testable (Constitution I).
- The interop bridge between .NET Framework 4.8 and .NET 8.0 MUST NOT compromise Excel VSTO rendering safety (Constitution X).
- Any new UI elements introduced in this phase MUST use DynamicResource for themeable properties (Constitution III).
- The Plugin SDK MUST enforce strong contracts to prevent plugin coupling (ADR-002).

## Assumptions

- The existing WpfApp2 solution (.NET Framework 4.8) will be extended with new .NET 8.0 class library projects; the VSTO host remains on .NET Framework 4.8.
- The interop bridge will use a combination of .NET Standard 2.0 shared contracts and process-level communication where necessary for the 4.8 ↔ 8.0 boundary.
- API keys are stored encrypted at rest using Windows DPAPI tied to the current user account; no external key management service required.
- AI provider SDKs (OpenAI, Anthropic) are available as NuGet packages targeting .NET 8.0.
- The Plugins directory will be a subdirectory of the application installation path, scanned at startup.
- Plugin assemblies are .NET 8.0 class libraries loaded via AssemblyLoadContext for isolation.
- Domain entities will use plain C# classes with properties (POCO style) rather than Entity Framework or ORM, since the primary data source is Excel and in-memory operation.
- JSON serialization will use System.Text.Json (built-in for .NET 8.0).
- All contracts are defined as C# interfaces in a dedicated Contracts project, not as WCF/web service contracts.
- Existing services (ServiceContainer, EventBus, ModuleRegistry) in the WPF host will continue to function; new .NET 8.0 libraries will have their own DI if needed.
- Excel interop (Microsoft.Office.Interop.Excel) remains in the .NET Framework 4.8 host; the .NET 8.0 export engine works with abstract data and the host bridges Excel operations.
