# Implementation Plan: Platform Foundation

**Branch**: `feature/phase-14-platform-foundation` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/014-platform-foundation/spec.md`

## Summary

Establish the architectural backbone of the Som3a Enterprise Planning Platform by creating 7 new .NET 8.0 class library projects within the existing WpfApp2 solution: Domain layer (BOQ, Activities, WBS, Relationships, Calendars, Constraints, Resources), Contracts layer (IPlugin, IAIProvider, IExportEngine, ISettingsModule, IPromptProvider, IDiagnosticsProvider), Plugin SDK (attributes, discovery, validation, hosting), AI Abstraction Layer (6 provider adapters, orchestrator, context builder, retry handler, token tracker, structured output parser), and Export Layer (pipeline, Excel/CSV/JSON/XML exporters, Primavera writer). All libraries interoperate with the .NET Framework 4.8 WPF host via a .NET Standard 2.0 interop bridge.

## Technical Context

**Language/Version**: C# — .NET Framework 4.8 (existing WpfApp2 VSTO host) + .NET 8.0 (new class libraries)

**Primary Dependencies**:
- **Existing**: WpfApp2 solution, ServiceContainer, EventBus, ModuleRegistry, ThemeManager, ModernWindow
- **NuGet (.NET 8.0)**: System.Text.Json, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging
- **NuGet (AI)**: OpenAI (>=2.0.0), Anthropic.SDK — provider packages installed in Som3a.AI only
- **Interop bridge**: .NET Standard 2.0 shared contracts project for types crossing the 4.8 ↔ 8.0 boundary

**Storage**:
- Domain entities: In-memory (POCO), serialized to JSON for persistence
- API keys: Windows DPAPI-encrypted files (user-scoped)
- Plugin settings: Per-plugin JSON files under AppData/Som3a/Plugins/
- Logs/diagnostics: Existing AppData/Som3a/Logs/ (5MB rollover, 3-file rotation) via shared diagnostics channel

**Testing**:
- Unit tests: xUnit (.NET 8.0) for domain entities, contract validation, AI parsers, export formatters
- Integration tests: Plugin discovery/loading, interop bridge round-trip serialization, export pipeline end-to-end
- Build validation: `msbuild` for .NET Framework 4.8; `dotnet build` for .NET 8.0 projects

**Target Platform**: Windows (x64) — Excel VSTO Add-in host (.NET Framework 4.8) with companion .NET 8.0 libraries

**Project Type**: Desktop application (WPF/VSTO) extended with class library infrastructure

**Performance Goals**:
- Domain entity round-trip (instantiate → serialize → deserialize → verify) in <1s (SC-001)
- Plugin discovery and registration within 5s of app startup (SC-002)
- AI provider failover within 30s total across retries (SC-003)
- Export 500 activities to Excel in <10s (SC-004)
- Domain model handles 50,000 BOQ items in <500MB memory (SC-009)

**Constraints**:
- Must not break existing WpfApp2 VSTO functionality
- Interop bridge must degrade gracefully — existing features remain usable if .NET 8.0 layer is unreachable (FR-028)
- API keys must be encrypted at rest via Windows DPAPI (FR-027)
- All .NET 8.0 libraries must emit structured diagnostics through shared channel to WPF host (FR-030)
- Plugin assemblies loaded via AssemblyLoadContext for isolation; one plugin failure must not affect others (SC-007)

**Scale/Scope**:
- 7 new .NET 8.0 class library projects + 1 .NET Standard 2.0 bridge project
- 10 domain entities (BOQDocument, BOQSection, BOQItem, Activity, WBSNode, Relationship, Calendar, Constraint, Resource, PluginDescriptor)
- 6 contract interfaces (IPlugin, IAIProvider, IExportEngine, ISettingsModule, IPromptProvider, IDiagnosticsProvider)
- 6 AI provider adapters (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex)
- 4 export format writers (Excel, CSV, JSON, XML) + 1 Primavera-compatible writer
- 4 SDK attributes (Plugin, SettingsSection, NavigationItem, Command)
- Target: 50,000 BOQ items, 500 Activity entities per export

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — All 7 new projects are independent class libraries with clear separation of concerns. Domain, Contracts, AI, Exporting, Plugin.SDK, Localization, Validation, Diagnostics, and Infrastructure are each self-contained, testable, and replaceable. No monolithic dictionaries or couplings.
- [x] **III. DynamicResource-Only Theme Architecture** — This phase introduces no new UI elements or resource dictionaries. The theme system is unaffected. Existing DynamicResource rules remain in place for any future UI added by this phase's consumers.
- [x] **IV. Runtime Theme Mutation Governance** — No theme mutation occurs in this phase. ThemeManager is not affected by new class libraries.
- [x] **V. Primitive & Semantic Token Architecture** — No new tokens introduced. Existing token system unchanged.
- [x] **IX. Animation Governance** — No animations introduced. Class libraries have no UI rendering.
- [x] **X. Excel Rendering Safety** — The interop bridge is designed with graceful degradation: if the .NET 8.0 layer fails, existing Excel VSTO features remain fully operational (FR-028). WindowRenderModeDetector is not affected.
- [x] **XI. WindowChrome Enforcement** — No new windows introduced. ModernWindow is not affected.
- [x] **XII. Centralized Effects Architecture** — No effects introduced. Class libraries have no visual effects.
- [x] **XIV. No Third-Party UI Frameworks** — No UI frameworks introduced. MaterialDesignThemes is deferred to Phase 17 (Theme Expansion), not this phase. ADR-006 authorization is preserved for future.
- [x] **XV. Resource Loading Order Enforcement** — No new resource dictionaries. ThemeResources.xaml is unchanged.
- [x] **XVI. Theme Safety & Recovery** — Theme system unchanged. Existing fallback recovery preserved.

**Note**: Phase 15 (Shell Refactor) introduces ADR-006 Material Design integration — this is the constitution exception for XIV that was authorized. Phase 14 has no such exception and is fully compliant.

**Result**: All gates pass. No constitution violations. Many principles are N/A for a class-library infrastructure phase.

## Project Structure

### Documentation (this feature)

```text
specs/014-platform-foundation/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output — interop patterns, DPAPI, plugin isolation
├── data-model.md        # Phase 1 output — entity definitions and relationships
├── quickstart.md        # Phase 1 output — developer onboarding guide
├── contracts/           # Phase 1 output — interface contract definitions
│   ├── IAIProvider.md
│   ├── IPlugin.md
│   ├── IExportEngine.md
│   ├── ISettingsModule.md
│   ├── IPromptProvider.md
│   └── IDiagnosticsProvider.md
└── tasks.md             # Phase 2 output (/speckit.tasks command — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Som3a Addin 2026/
├── WpfApp2/                              # Existing VSTO + WPF host (.NET Framework 4.8)

├── Som3a.Bridge/                         # NEW — .NET Standard 2.0 bridge library
│   ├── Som3a.Bridge.csproj
│   ├── InteropContracts.cs               # Shared types crossing 4.8 ↔ 8.0 boundary
│   └── DiagnosticsChannel.cs             # Structured log event facade

├── Som3a.Domain/                         # NEW — .NET 8.0 Class Library
│   ├── Som3a.Domain.csproj
│   ├── BOQ/
│   │   ├── BOQDocument.cs
│   │   ├── BOQSection.cs
│   │   └── BOQItem.cs
│   ├── Activities/
│   │   └── Activity.cs
│   ├── WBS/
│   │   └── WBSNode.cs
│   ├── Relationships/
│   │   └── Relationship.cs
│   ├── Calendars/
│   │   └── Calendar.cs
│   ├── Constraints/
│   │   └── Constraint.cs
│   ├── Resources/
│   │   └── Resource.cs
│   └── Export/
│       └── ExportBatch.cs

├── Som3a.Contracts/                      # NEW — .NET 8.0 Class Library
│   ├── Som3a.Contracts.csproj
│   ├── IPlugin.cs
│   ├── IAIProvider.cs
│   ├── IExportEngine.cs
│   ├── ISettingsModule.cs
│   ├── IPromptProvider.cs
│   └── IDiagnosticsProvider.cs

├── Som3a.Plugin.SDK/                     # NEW — .NET 8.0 Class Library
│   ├── Som3a.Plugin.SDK.csproj
│   ├── Attributes/
│   │   ├── PluginAttribute.cs
│   │   ├── SettingsSectionAttribute.cs
│   │   ├── NavigationItemAttribute.cs
│   │   └── CommandAttribute.cs
│   ├── Discovery/
│   │   ├── PluginDiscoverer.cs
│   │   └── AssemblyScanner.cs
│   ├── Validation/
│   │   ├── PluginValidator.cs
│   │   └── ContractVerifier.cs
│   └── Hosting/
│       ├── PluginHost.cs
│       └── PluginSandbox.cs

├── Som3a.AI/                             # NEW — .NET 8.0 Class Library
│   ├── Som3a.AI.csproj
│   ├── Providers/
│   │   ├── AIProviderBase.cs
│   │   ├── OpenAIProvider.cs
│   │   ├── ClaudeProvider.cs
│   │   ├── DeepSeekProvider.cs
│   │   ├── GLMProvider.cs
│   │   ├── KimiProvider.cs
│   │   └── CodexProvider.cs
│   ├── Orchestration/
│   │   ├── AIOrchestrator.cs
│   │   ├── ContextBuilder.cs
│   │   └── RetryHandler.cs
│   ├── Prompts/
│   │   └── PromptRepository.cs
│   ├── Parsing/
│   │   ├── StructuredOutputParser.cs
│   │   └── JsonSchemaValidator.cs
│   └── Tracking/
│       ├── TokenTracker.cs
│       └── UsageReporter.cs

├── Som3a.Exporting/                      # NEW — .NET 8.0 Class Library
│   ├── Som3a.Exporting.csproj
│   ├── Pipeline/
│   │   └── ExportPipeline.cs
│   ├── Excel/
│   │   ├── ExcelExportEngine.cs
│   │   └── StyleApplier.cs
│   ├── Primavera/
│   │   └── PrimaveraCompatibleWriter.cs
│   └── Formats/
│       ├── CsvExporter.cs
│       ├── JsonExporter.cs
│       └── XmlExporter.cs

├── Som3a.Localization/                   # NEW — .NET 8.0 Class Library
│   ├── Som3a.Localization.csproj
│   ├── Resources/
│   │   ├── Strings.resx                  # English (default)
│   │   └── Strings.ar.resx               # Arabic
│   ├── Services/
│   │   ├── LocalizationService.cs
│   │   └── CultureManager.cs
│   └── RTL/
│       ├── RTLHelper.cs
│       └── FlowDirectionManager.cs

├── Som3a.Validation/                     # NEW — .NET 8.0 Class Library
│   ├── Som3a.Validation.csproj
│   ├── BOQ/
│   │   └── BOQStructureValidator.cs
│   ├── WBS/
│   │   └── WBSIntegrityValidator.cs
│   ├── Relationships/
│   │   ├── LoopDetector.cs
│   │   └── DependencyValidator.cs
│   ├── Duration/
│   │   └── DurationLogicValidator.cs
│   └── Export/
│       └── ExportValidator.cs

├── Som3a.Diagnostics/                    # NEW — .NET 8.0 Class Library
│   ├── Som3a.Diagnostics.csproj
│   ├── AI/
│   │   ├── AIRequestLogger.cs
│   │   └── TokenUsageTracker.cs
│   ├── Plugins/
│   │   └── PluginHealthMonitor.cs
│   └── Export/
│       └── ExportDiagnostics.cs

└── Som3a.Infrastructure/                 # NEW — .NET 8.0 Class Library
    ├── Som3a.Infrastructure.csproj
    ├── Security/
    │   ├── ApiKeyEncryption.cs
    │   └── SecureStorage.cs
    ├── Configuration/
    │   ├── AppConfiguration.cs
    │   └── FeatureFlags.cs
    └── Telemetry/
        └── PerformanceCounters.cs
```

**Structure Decision**: The enterprise folder structure defined in the master plan adds new .NET 8.0 projects alongside the existing WpfApp2 folder. Each library has a single responsibility: Domain (data), Contracts (interfaces), Plugin.SDK (extensibility), AI (providers), Exporting (output), Localization (i18n), Validation (rules), Diagnostics (logging), Infrastructure (security/config). A .NET Standard 2.0 bridge project (Som3a.Bridge) provides shared types for the 4.8 ↔ 8.0 boundary.

## Complexity Tracking

> No constitution violations to justify. All gates pass cleanly.
