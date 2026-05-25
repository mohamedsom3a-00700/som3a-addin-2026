# Tasks: Platform Foundation

**Input**: Design documents from `/specs/014-platform-foundation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Unit tests are included where specified by success criteria. No TDD requirement — tests are written alongside or after implementation.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- .NET 8.0 libraries at repository root (e.g., `Som3a.Domain/`, `Som3a.Contracts/`)
- .NET Standard 2.0 bridge at `Som3a.Bridge/`
- WPF host at `WpfApp2/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create solution structure, initialize all .NET projects, configure NuGet references, and establish the interop bridge foundation.

- [X] T001 Create .NET Standard 2.0 bridge project `Som3a.Bridge/` with `Som3a.Bridge.csproj` and add project reference to existing WpfApp2 solution
- [X] T002 [P] Create .NET 8.0 class library `Som3a.Contracts/` with `Som3a.Contracts.csproj` — no dependencies
- [X] T003 [P] Create .NET 8.0 class library `Som3a.Domain/` with `Som3a.Domain.csproj` — reference Som3a.Contracts
- [X] T004 [P] Create .NET 8.0 class library `Som3a.Plugin.SDK/` with `Som3a.Plugin.SDK.csproj` — reference Som3a.Contracts
- [X] T005 [P] Create .NET 8.0 class library `Som3a.AI/` with `Som3a.AI.csproj` — reference Som3a.Contracts, Som3a.Domain
- [X] T006 [P] Create .NET 8.0 class library `Som3a.Exporting/` with `Som3a.Exporting.csproj` — reference Som3a.Contracts, Som3a.Domain
- [X] T007 [P] Create .NET 8.0 class library `Som3a.Localization/` with `Som3a.Localization.csproj` — reference Som3a.Contracts
- [X] T008 [P] Create .NET 8.0 class library `Som3a.Validation/` with `Som3a.Validation.csproj` — reference Som3a.Contracts, Som3a.Domain
- [X] T009 [P] Create .NET 8.0 class library `Som3a.Diagnostics/` with `Som3a.Diagnostics.csproj` — reference Som3a.Contracts
- [X] T010 [P] Create .NET 8.0 class library `Som3a.Infrastructure/` with `Som3a.Infrastructure.csproj` — reference Som3a.Contracts
- [X] T011 Add NuGet packages: `System.Text.Json`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging` to all .NET 8.0 projects
- [X] T012 Add NuGet packages: `OpenAI` (>=2.0.0), `Anthropic.SDK` to `Som3a.AI/` project
- [X] T013 [P] Add NuGet package: `System.Security.Cryptography.ProtectedData` to `Som3a.Infrastructure/` for DPAPI encryption
- [X] T014 Configure `Som3a.Bridge/` with shared DTO base classes (`InteropContracts.cs`, `DiagnosticsChannel.cs`) usable by both .NET Framework 4.8 and .NET 8.0
- [X] T015 Verify all 10 .NET projects compile with `dotnet build` — zero errors (SC-005 foundation)

**Checkpoint**: All projects created and compiling — ready for foundational infrastructure

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared infrastructure that MUST be complete before ANY user story can be implemented — bridge serialization, base DI setup, shared diagnostics channel, and base validation types.

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T016 Implement `Som3a.Bridge/InteropContracts.cs` — define base interface `IInteropSerializable` and shared DTO convention for ID-based references across 4.8 ↔ 8.0 boundary
- [X] T017 Implement `Som3a.Bridge/DiagnosticsChannel.cs` — define `IDiagnosticsChannel` facade interface and `DiagnosticEvent` DTO for structured logging across the bridge
- [X] T018 Implement `Som3a.Contracts/ValidationResult.cs` — shared validation result type with `IsSuccess`, `ErrorMessage`, and `Warnings` collection (used by all contracts)
- [X] T019 Implement `Som3a.Infrastructure/Configuration/AppConfiguration.cs` — application config loader reading from `AppData/Som3a/config.json` with defaults
- [X] T020 Implement `Som3a.Infrastructure/Configuration/FeatureFlags.cs` — feature flag manager for enabling/disabling AI, export, and plugin capabilities (supporting graceful degradation FR-028)
- [X] T021 Implement `Som3a.Infrastructure/Security/SecureStorage.cs` — DPAPI-based `StoreSecretAsync(key, value)` and `GetSecretAsync(key)` methods for API key encryption (FR-027)
- [X] T022 Implement `Som3a.Infrastructure/Security/ApiKeyEncryption.cs` — typed wrapper over SecureStorage with provider-specific key naming convention (`provider:{id}:apikey`)
- [X] T023 Implement `Som3a.Diagnostics/DiagnosticEvent.cs` — structured log event class with `EventId`, `Timestamp`, `Level`, `Source`, `Category`, `Message`, `Properties`, `ExceptionDetail`
- [X] T024 Implement `Som3a.Diagnostics/DiagnosticsSnapshot.cs` and `HealthReport.cs` / `HealthCheck.cs` — data types for health monitoring per IDiagnosticsProvider contract
- [X] T025 Implement `Som3a.Localization/Resources/Strings.resx` — English default resource file with placeholder keys for future translation
- [X] T026 Implement `Som3a.Localization/Resources/Strings.ar.resx` — Arabic resource file (placeholder, populated in Phase 24)
- [X] T027 Implement `Som3a.Localization/Services/LocalizationService.cs` — `LocalizationService` with `CultureManager` supporting `GetString(key)` and dynamic language switching scaffold
- [X] T028 Implement `Som3a.Localization/RTL/RTLHelper.cs` and `Som3a.Localization/RTL/FlowDirectionManager.cs` — RTL support scaffold (placeholder, populated in Phase 24)
- [X] T029 Add `Som3a.Bridge` reference to WpfApp2 project — verify .NET Framework 4.8 host can consume .NET Standard 2.0 bridge types
- [X] T030 Build full solution with MSBuild → zero errors across all 10 projects + WpfApp2

**Checkpoint**: Foundation ready — all infrastructure projects compiling, bridge operational, security and diagnostics primitives available. User story implementation can now begin.

---

## Phase 3: User Story 1 — Domain Model Ready for Planning Features (Priority: P1) 🎯 MVP

**Goal**: Planning engineers have a complete in-memory domain model for BOQ, Activities, WBS, Relationships, Calendars, Constraints, and Resources — serializable to JSON with full round-trip fidelity.

**Independent Test**: Instantiate a BOQDocument with 3 sections and 50 items, derive 5 Activity entities with WBS nodes and relationships, serialize to JSON, deserialize back, and verify all properties preserved — all in under 1 second (SC-001).

### Implementation for User Story 1

- [X] T031 [P] [US1] Implement `Som3a.Domain/BOQ/BOQItem.cs` — POCO with Id, ItemCode, Description, Quantity, Unit, UnitPrice, Classification, BOQReference, parent BOQSection reference
- [X] T032 [P] [US1] Implement `Som3a.Domain/BOQ/BOQSection.cs` — POCO with Id, SectionName, SectionCode, DisplayOrder, Items list, ParentDocument reference; validate DisplayOrder uniqueness
- [X] T033 [US1] Implement `Som3a.Domain/BOQ/BOQDocument.cs` — POCO with Id, ProjectName, CreatedDate, Sections list, computed TotalItems; validate TotalItems equals sum of all section items (depends on T031, T032)
- [X] T034 [P] [US1] Implement `Som3a.Domain/Calendars/Calendar.cs` — POCO with Id, Name, WorkDays (HashSet\<DayOfWeek\>), Holidays, WorkingHoursPerDay, TimeZone; validate WorkDays ≥ 1, hours > 0 and ≤ 24
- [X] T035 [P] [US1] Implement `Som3a.Domain/WBS/WBSNode.cs` — POCO with Id, Code, Name, computed Level, Parent, Children list, computed FullPath; validate no cycles, Level matches depth
- [X] T036 [US1] Implement `Som3a.Domain/Activities/Activity.cs` — POCO with Id, ActivityId, Name, Description, Quantity, Unit, Duration, ProductivityRate, WBSNode ref, Calendar ref, BOQReferences list, Relationships list, Constraints list, ResourceAssignments list (depends on T034, T035)
- [X] T037 [P] [US1] Implement `Som3a.Domain/Relationships/Relationship.cs` — POCO with Id, Predecessor (Activity ref), Successor (Activity ref), Type enum (FS/SS/FF/SF), Lag (TimeSpan), ValidationStatus enum, ValidationMessage; validate Predecessor != Successor
- [X] T038 [P] [US1] Implement `Som3a.Domain/Constraints/Constraint.cs` — POCO with Id, Activity ref, ConstraintType enum (StartOn/FinishOn/MandatoryStart/MandatoryFinish/StartOnOrAfter/FinishOnOrBefore), ConstraintDate, FloatValue
- [X] T039 [P] [US1] Implement `Som3a.Domain/Resources/Resource.cs` — POCO with Id, Name, ResourceType enum (Labor/Equipment/Material/Subcontractor), CostPerHour, Budget, Assignments list
- [X] T040 [P] [US1] Implement `Som3a.Domain/Resources/ResourceAssignment.cs` — POCO with Id, Resource ref, Activity ref, Quantity, computed Cost (= Quantity × CostPerHour × Activity.Duration.Hours)
- [X] T041 [US1] Implement `Som3a.Domain/Export/ExportBatch.cs` — POCO with Id, ExportTarget enum, data collection, creation timestamp (depends on all entity types)
- [X] T042 [US1] Configure System.Text.Json serialization in `Som3a.Domain/` — apply `ReferenceHandler.Preserve` for cycle handling; use camelCase policy; register custom converters if needed for TimeSpan and DateTimeOffset
- [X] T043 [US1] Implement JSON round-trip unit tests in `Som3a.Domain.Tests/Serialization/` — verify Activity with nested WBSNode and Relationships serializes/deserializes without data loss, verify 50,000 BOQ items under 500MB (SC-009)
- [X] T044 [US1] Implement domain validation unit tests in `Som3a.Domain.Tests/Validation/` — verify BOQDocument TotalItems integrity, WBSNode cycle detection, Relationship predecessor ≠ successor, Calendar working hours range
- [X] T045 [US1] Implement `Som3a.Validation/BOQ/BOQStructureValidator.cs` — validate BOQDocument structure (sections non-empty, item codes unique, quantities > 0)
- [X] T046 [US1] Implement `Som3a.Validation/WBS/WBSIntegrityValidator.cs` — validate WBSNode tree integrity (no cycles, root has null parent, Level matches depth, FullPath correct)

**Checkpoint**: Domain model fully functional — all 7 entity types implemented, serializable, validated. SC-001, SC-006, SC-009 verifiable.

---

## Phase 4: User Story 2 — Plugin Developers Can Build Against Stable Contracts (Priority: P1)

**Goal**: All 6 contract interfaces defined and ready for implementation — plugins, AI providers, and exporters interact through stable interfaces without coupling to implementation details.

**Independent Test**: Write a minimal class implementing IPlugin, pass it to a simple test host, and verify that Initialize/RegisterSettings/LoadUI/RegisterCommands are called in lifecycle order — without any real plugin infrastructure.

### Implementation for User Story 2

- [X] T047 [P] [US2] Implement `Som3a.Contracts/IPlugin.cs` — define `IPlugin` interface with Id, Name, Version, Priority, Dependencies properties and Initialize(context), RegisterSettings(registry), LoadUI(pageHost), RegisterCommands(registry), Shutdown() methods; define `IPluginContext`, `IPageHost`, `ICommandRegistry`, `ISettingsRegistry` minimal interfaces
- [X] T048 [P] [US2] Implement `Som3a.Contracts/IAIProvider.cs` — define `IAIProvider` interface with ProviderId, ProviderName, IsAvailable, ExecutePromptAsync(request, ct), StreamPromptAsync(request, ct), HealthCheckAsync(ct); define `AIRequest`, `AIResponse`, `AIStreamChunk`, `TokenUsage` types
- [X] T049 [P] [US2] Implement `Som3a.Contracts/IExportEngine.cs` — define `IExportEngine` interface with ExportAsync(request, ct), ValidateAsync(request, ct), GetCapabilities(); define `ExportRequest`, `ExportResult`, `ExportOptions`, `ExportFormat` enum, `ExportEngineCapabilities` types
- [X] T050 [P] [US2] Implement `Som3a.Contracts/ISettingsModule.cs` — define `ISettingsModule` interface with ModuleId, ModuleName, RegisterSettings(registry), ValidateAsync(), ExportAsync(filePath), ImportAsync(filePath); define `SettingsSection`, `SettingDefinition`, `SettingValueType` enum, `ValidationRule` types
- [X] T051 [P] [US2] Implement `Som3a.Contracts/IPromptProvider.cs` — define `IPromptProvider` interface with GetTemplateAsync(templateId, ct), BuildContextAsync\<T\>(entity, ct), ValidatePrompt(template, parameters), ListTemplatesAsync(category); define `PromptTemplate`, `PromptContext` types
- [X] T052 [P] [US2] Implement `Som3a.Contracts/IDiagnosticsProvider.cs` — define `IDiagnosticsProvider` interface with ProviderId, ProviderName, CollectSnapshotAsync(ct), ReportHealthAsync(ct), LogDiagnostic(evt), DiagnosticLogged event; define `DiagnosticLevel` enum, `HealthStatus` enum, `HealthReport`, `HealthCheck` types
- [X] T053 [US2] Add interface contract compliance tests in `Som3a.Contracts.Tests/` — verify each interface can be mocked and consumed without implementation reference; verify all request/response types serializable

**Checkpoint**: All 6 contracts defined and verified — any developer can now implement IPlugin or IAIProvider without knowing platform internals. SC-008 (30-min plugin creation) is now achievable.

---

## Phase 5: User Story 3 — New AI Features Can Be Added Without Platform Changes (Priority: P2)

**Goal**: Plugin SDK with attributes, discovery, validation, and hosting — enables external teams to develop, package, and deploy plugins that the platform discovers and loads automatically, with error isolation.

**Independent Test**: Create a test plugin assembly with `[Plugin]` attribute, place it in the Plugins directory, run PluginDiscoverer, and verify it discovers the plugin, validates contracts, and initializes it — all within 5 seconds (SC-002). Introduce a failing plugin to verify error isolation (SC-007).

### Implementation for User Story 3

- [X] T054 [P] [US3] Implement `Som3a.Plugin.SDK/Attributes/PluginAttribute.cs` — `[Plugin]` attribute with Id, Name, Version, Priority, Dependencies properties; inherits from System.Attribute
- [X] T055 [P] [US3] Implement `Som3a.Plugin.SDK/Attributes/SettingsSectionAttribute.cs` — `[SettingsSection]` attribute with Category, Order, Icon properties
- [X] T056 [P] [US3] Implement `Som3a.Plugin.SDK/Attributes/NavigationItemAttribute.cs` — `[NavigationItem]` attribute with Category, Icon, Order, PageType properties
- [X] T057 [P] [US3] Implement `Som3a.Plugin.SDK/Attributes/CommandAttribute.cs` — `[Command]` attribute with Id, Name, KeyGesture, RibbonGroup properties
- [X] T058 [US3] Implement `Som3a.Plugin.SDK/Discovery/AssemblyScanner.cs` — scan Plugins directory for .dll assemblies; load via AssemblyLoadContext per plugin for isolation
- [X] T059 [US3] Implement `Som3a.Plugin.SDK/Discovery/PluginDiscoverer.cs` — discover types decorated with `[Plugin]` attribute in scanned assemblies; build PluginDescriptor list (depends on T054, T058)
- [X] T060 [US3] Implement `Som3a.Plugin.SDK/Validation/ContractVerifier.cs` — verify plugin class implements IPlugin, check constructor accessibility, validate version format
- [X] T061 [US3] Implement `Som3a.Plugin.SDK/Validation/PluginValidator.cs` — check dependencies exist, validate contracts via ContractVerifier, resolve conflict (last-registered-wins per FR-029); produce PluginDescriptor with Status
- [X] T062 [US3] Implement `Som3a.Plugin.SDK/Hosting/PluginSandbox.cs` — wrap each plugin in its own AssemblyLoadContext; catch exceptions at boundary; provide unload capability
- [X] T063 [US3] Implement `Som3a.Plugin.SDK/Hosting/PluginHost.cs` — manage full lifecycle (load → validate → initialize → start → stop → unload); implement error isolation (one plugin failure → marked Error, others continue per SC-007); log conflict warnings per FR-029
- [X] T064 [P] [US3] Implement `Som3a.Diagnostics/Plugins/PluginHealthMonitor.cs` — track plugin load success/failure, memory usage, error rate, version; expose health report via IDiagnosticsProvider
- [X] T065 [US3] Implement plugin SDK integration tests in `Som3a.Plugin.SDK.Tests/` — test discovery of valid plugin, validation of invalid plugin, error isolation, conflict resolution, AssemblyLoadContext unload

**Checkpoint**: Plugin SDK fully operational — plugins auto-discovered, validated, loaded in isolation, with health monitoring. SC-002 and SC-007 verifiable.

---

## Phase 6: User Story 4 — Planning Data Can Be Exported to Multiple Formats (Priority: P2)

**Goal**: Export pipeline supporting Excel (multi-sheet with styling), CSV, JSON, and XML formats, plus Primavera-compatible tables — all through a unified IExportEngine interface.

**Independent Test**: Create 500 Activity entities, pass them to ExportPipeline with format=Excel, and receive a multi-sheet workbook in under 10 seconds (SC-004). Repeat for CSV, JSON, and XML formats.

### Implementation for User Story 4

- [X] T066 [US4] Implement `Som3a.Exporting/Pipeline/ExportPipeline.cs` — coordinate validate → review → generate → style → write sequence; implement IExportEngine contract; route to correct format writer based on ExportFormat
- [X] T067 [P] [US4] Implement `Som3a.Exporting/Formats/JsonExporter.cs` — serialize domain entities to JSON array using System.Text.Json with camelCase, preserving nested relationships via ID references per bridge DTO convention
- [X] T068 [P] [US4] Implement `Som3a.Exporting/Formats/CsvExporter.cs` — write CSV with configurable delimiter, header row from ColumnMappings, proper escaping for values containing commas/quotes
- [X] T069 [P] [US4] Implement `Som3a.Exporting/Formats/XmlExporter.cs` — serialize domain entities to XML with configurable root element name, attribute-based property mapping
- [X] T070 [US4] Implement `Som3a.Exporting/Excel/ExcelExportEngine.cs` — generate Excel workbook via the interop bridge to .NET Framework 4.8 host; support multi-sheet export, column headers from ColumnMappings, data type preservation; target ≤10s for 500 activities (SC-004)
- [X] T071 [US4] Implement `Som3a.Exporting/Excel/StyleApplier.cs` — apply theme-consistent styling to exported sheets (header formatting, alternating row colors, column auto-width); receive style configuration from ExportOptions
- [X] T072 [US4] Implement `Som3a.Exporting/Primavera/PrimaveraCompatibleWriter.cs` — format activity and relationship data into Primavera-compatible table layout (Activity ID, Name, Duration, Start, Finish, Predecessors, Successors columns)
- [X] T073 [US4] Implement `Som3a.Validation/Export/ExportValidator.cs` — pre-export validation: check required fields present, verify data integrity, validate target path accessible (FR-024); block export on validation error with clear message
- [X] T074 [US4] Implement `Som3a.Diagnostics/Export/ExportDiagnostics.cs` — log export start, completion, duration, row count, sheet count, errors; expose via IDiagnosticsProvider
- [X] T075 [US4] Implement export integration tests in `Som3a.Exporting.Tests/` — verify Excel export of 500 activities within 10s, verify CSV/JSON/XML round-trip fidelity, verify pre-export validation blocks on missing fields, verify Primavera format column mapping

**Checkpoint**: Export engine fully functional — all 5 formats operating, validation blocking bad exports, diagnostics tracking. SC-004 verifiable.

---

## Phase 7: User Story 5 — AI Providers Are Abstracted and Interchangeable (Priority: P3)

**Goal**: AI abstraction layer with 6 provider adapters, orchestration engine with automatic failover, structured output parsing, token tracking, and prompt template management — all through the unified IAIProvider interface.

**Independent Test**: Configure two providers (primary: test mock, fallback: test mock), send a prompt, have primary fail, and verify fallback handles the request. Verify token tracking reports usage per provider. Verify structured output parser corrects malformed JSON.

### Implementation for User Story 5

- [X] T076 [US5] Implement `Som3a.AI/Providers/AIProviderBase.cs` — abstract base class for all providers: implement common retry logic, timeout handling, diagnostic logging; implement `HealthCheckAsync` with configurable endpoint
- [X] T077 [P] [US5] Implement `Som3a.AI/Providers/OpenAIProvider.cs` — implement IAIProvider using OpenAI NuGet SDK; support GPT-4, GPT-4o models; configurable base URL for proxy/compatible APIs
- [X] T078 [P] [US5] Implement `Som3a.AI/Providers/ClaudeProvider.cs` — implement IAIProvider using Anthropic.SDK; support Claude 3 models; streaming response support
- [X] T079 [P] [US5] Implement `Som3a.AI/Providers/DeepSeekProvider.cs` — implement IAIProvider using OpenAI SDK with custom BaseUrl (DeepSeek API is OpenAI-compatible); reuse OpenAIProvider base logic
- [X] T080 [P] [US5] Implement `Som3a.AI/Providers/KimiProvider.cs` — implement IAIProvider using OpenAI SDK with custom BaseUrl (Moonshot/Kimi API is OpenAI-compatible)
- [X] T081 [P] [US5] Implement `Som3a.AI/Providers/GLMProvider.cs` — implement IAIProvider using HttpClient (GLM uses proprietary API format); lightweight JSON request/response wrapper
- [X] T082 [P] [US5] Implement `Som3a.AI/Providers/CodexProvider.cs` — implement IAIProvider using OpenAI SDK (Codex is an OpenAI model); customize for code-generation tasks
- [X] T083 [US5] Implement `Som3a.AI/Orchestration/RetryHandler.cs` — exponential backoff (1s, 2s, 4s, 8s) for transient HTTP errors (429, 5xx); circuit breaker after 5 consecutive failures; total failover time ≤30s (SC-003)
- [X] T084 [US5] Implement `Som3a.AI/Orchestration/ContextBuilder.cs` — convert domain entities to structured AI context: BOQDocument → hierarchical summary, List\<Activity\> → activity table, WBSNode → tree with depth, Relationship list → predecessor/successor matrix
- [X] T085 [US5] Implement `Som3a.AI/Orchestration/AIOrchestrator.cs` — provider routing with priority list, automatic failover, retry coordination via RetryHandler, context enrichment via ContextBuilder, response aggregation; implement graceful degradation when all providers unavailable (depends on T083, T084)
- [X] T086 [US5] Implement `Som3a.AI/Tracking/TokenTracker.cs` — record prompt tokens, completion tokens, total tokens per request; aggregate per session and per provider; expose usage data via properties (FR-020)
- [X] T087 [US5] Implement `Som3a.AI/Tracking/UsageReporter.cs` — generate token usage reports per session with cost estimates based on provider-specific pricing; log via diagnostics channel
- [X] T088 [US5] Implement `Som3a.AI/Parsing/JsonSchemaValidator.cs` — validate AI response JSON against a provided JSON Schema; report schema violations with path and reason
- [X] T089 [US5] Implement `Som3a.AI/Parsing/StructuredOutputParser.cs` — parse AI text response into typed domain entities (Activity list, WBSNode tree, Relationship list, duration data, review results) using System.Text.Json deserialization with JsonSchemaValidator enforcement (FR-021)
- [X] T090 [US5] Implement `Som3a.AI/Prompts/PromptRepository.cs` — load prompt templates from embedded resources; support mustache-style parameter substitution; validate template completeness; return PromptTemplate objects per IPromptProvider contract
- [X] T091 [P] [US5] Implement `Som3a.Diagnostics/AI/AIRequestLogger.cs` — log AI request/response (sanitized — no API keys in logs), provider used, duration, token usage, error details; expose via IDiagnosticsProvider
- [X] T092 [US5] Implement AI abstraction integration tests in `Som3a.AI.Tests/` — test provider failover (mock primary error → fallback success), test retry with exponential backoff, test structured output parsing with valid/invalid JSON, test token tracking accuracy, test context builder output for each entity type, test 30s total timeout (SC-003)

**Checkpoint**: AI abstraction layer fully functional — 6 providers, orchestration with failover, structured parsing, token tracking. SC-003 verifiable.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Build validation, constitution compliance review, developer quickstart validation, and final integration gate.

- [X] T093 Build validation — run `dotnet build` on all 10 projects; run MSBuild on WpfApp2; verify zero errors (SC-005 final)
- [X] T094 Verify success criteria: measure SC-001 (<1s entity round-trip), SC-002 (<5s plugin discovery), SC-003 (<30s AI failover), SC-004 (<10s 500-activity Excel export), SC-009 (<500MB for 50K BOQ items)
- [X] T095 [P] Constitution compliance review — verify all libraries are modular and independently testable (I); verify interop bridge does not compromise Excel rendering safety (X); verify no inline styles or effects introduced (III, XII); verify Plugin SDK enforces strong contracts (ADR-002)
- [X] T096 [P] Run quickstart.md validation — follow each step in quickstart.md (build, create entity, implement plugin, make AI call, export data, store API key) and verify all examples work as documented
- [X] T097 [P] Code review — verify all FR-001 through FR-030 are implemented; verify naming conventions consistent across all projects; verify no hardcoded strings where constants should be used
- [X] T098 Update `AGENTS.md` — add build commands for new .NET 8.0 projects (`dotnet build Som3a.Domain/` etc.); add project dependency graph section
- [X] T099 Implement `Som3a.Diagnostics/AI/TokenUsageTracker.cs` — centralized token usage aggregator collecting data from all AI providers and exposing per-session, per-project rollup (depends on T086)
- [X] T100 Run full solution build → all 10 .NET 8.0 projects + Som3a.Bridge + WpfApp2 compile with zero errors and zero warnings

**Checkpoint**: Phase 14 ready for review gate — all user stories independently testable, all success criteria verifiable, all FRs implemented.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion (T015) — BLOCKS all user stories
- **US1 Domain (Phase 3)**: Depends on Foundational (T030) — no dependency on other stories
- **US2 Contracts (Phase 4)**: Depends on Foundational (T030) — no dependency on other stories; can run in parallel with US1
- **US3 Plugin SDK (Phase 5)**: Depends on US2 Contracts (T052, T053) — needs IPlugin interface
- **US4 Export (Phase 6)**: Depends on US1 Domain (T041) + US2 Contracts (T049) — needs entities + IExportEngine
- **US5 AI (Phase 7)**: Depends on US1 Domain (T041) + US2 Contracts (T048) — needs entities + IAIProvider
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```text
Setup (Phase 1)
  ↓
Foundational (Phase 2)
  ├── US1 Domain (P1) ──┬── US4 Export (P2)
  └── US2 Contracts (P1) ──┼── US5 AI (P3)
                           └── US3 Plugin SDK (P2)
```

- US1 and US2 are both P1 — start in parallel after Foundational
- US3, US4, US5 are independent of each other — start in parallel after their respective prerequisites

### Within Each User Story

- Models/POCOs first (marked [P] — parallelizable within the phase)
- Services/orchestration second (depends on models)
- Integration/tests last

### Parallel Opportunities

- **Phase 1**: T002–T010 (8 project creations) can run in parallel; T011–T013 (NuGet packages) can run in parallel
- **Phase 2**: T018–T028 (11 files across Infra/Diagnostics/Localization) can run in parallel
- **Phase 3 (US1)**: T031–T032, T034–T035, T037–T040 (6 entity POCOs) can run in parallel
- **Phase 4 (US2)**: T047–T052 (6 contract interfaces) can run in parallel
- **Phase 5 (US3)**: T054–T057 (4 attributes) can run in parallel
- **Phase 6 (US4)**: T067–T069 (3 format exporters) can run in parallel
- **Phase 7 (US5)**: T077–T082 (6 provider adapters), T091 (AI diagnostics) can run in parallel
- **Phase 8**: T095–T097 (review/validation) can run in parallel

---

## Parallel Example: User Story 1 (Domain Model)

```bash
# Launch all entity POCOs together:
Task: "T031 Implement BOQItem in Som3a.Domain/BOQ/BOQItem.cs"
Task: "T032 Implement BOQSection in Som3a.Domain/BOQ/BOQSection.cs"
Task: "T034 Implement Calendar in Som3a.Domain/Calendars/Calendar.cs"
Task: "T035 Implement WBSNode in Som3a.Domain/WBS/WBSNode.cs"
Task: "T037 Implement Relationship in Som3a.Domain/Relationships/Relationship.cs"
Task: "T038 Implement Constraint in Som3a.Domain/Constraints/Constraint.cs"
Task: "T039 Implement Resource in Som3a.Domain/Resources/Resource.cs"
Task: "T040 Implement ResourceAssignment in Som3a.Domain/Resources/ResourceAssignment.cs"
```

## Parallel Example: User Story 5 (AI Providers)

```bash
# Launch all 6 providers together:
Task: "T077 Implement OpenAIProvider in Som3a.AI/Providers/OpenAIProvider.cs"
Task: "T078 Implement ClaudeProvider in Som3a.AI/Providers/ClaudeProvider.cs"
Task: "T079 Implement DeepSeekProvider in Som3a.AI/Providers/DeepSeekProvider.cs"
Task: "T080 Implement KimiProvider in Som3a.AI/Providers/KimiProvider.cs"
Task: "T081 Implement GLMProvider in Som3a.AI/Providers/GLMProvider.cs"
Task: "T082 Implement CodexProvider in Som3a.AI/Providers/CodexProvider.cs"
```

---

## Implementation Strategy

### MVP First (US1 Domain + US2 Contracts Only)

1. Complete Phase 1: Setup (T001–T015)
2. Complete Phase 2: Foundational (T016–T030)
3. Complete Phase 3: User Story 1 — Domain Model (T031–T046)
4. Complete Phase 4: User Story 2 — Contracts (T047–T053)
5. **STOP and VALIDATE**: Test US1 + US2 independently — entities serialize/deserialize correctly, contracts compilable and mockable
6. Deploy foundation: other developers can now build against stable domain + contracts

### Incremental Delivery

1. Setup + Foundational → Foundation ready (T001–T030)
2. Add US1 + US2 → Domain + Contracts ready (T001–T053) — **MVP for Phase 14**
3. Add US3 → Plugin SDK operational (T054–T065)
4. Add US4 → Export engine operational (T066–T075)
5. Add US5 → AI abstraction operational (T076–T092)
6. Polish → Phase 14 complete (T093–T100)

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (Phase 1–2)
2. Once Foundational is done:
   - Developer A: US1 Domain (Phase 3) + US4 Export (Phase 6)
   - Developer B: US2 Contracts (Phase 4) + US3 Plugin SDK (Phase 5)
   - Developer C: US5 AI Abstraction (Phase 7) — can start as soon as US1 + US2 complete
3. All converge on Polish (Phase 8) for final validation

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Build after each phase (`dotnet build`) before moving to next phase
- Total: 100 tasks across 8 phases
- MVP scope (US1 + US2): 53 tasks (T001–T053)
- Full scope: 100 tasks (T001–T100)
