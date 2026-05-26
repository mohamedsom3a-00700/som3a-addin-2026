# Som3a Enterprise Planning Platform — Master Implementation Plan

**Version**: 1.0.0
**Date**: 2026-05-25
**Status**: Planning — not yet in execution
**Governed by**: `.specify/memory/constitution.md` (v1.2.0)
**Project**: Som3a Add-in 2026 → Enterprise Planning Platform Transformation
**Platform**: Excel VSTO Add-in (.NET Framework 4.8 + .NET 8.0, WPF)
**Branch Pattern**: `feature/phase-NN-short-name` (when execution begins)

---

## Executive Summary

Transform the current Excel VSTO Add-in into a complete **AI-powered Enterprise Planning Platform** for construction planning engineers — BOQ analysis, WBS generation, activity generation, logic relationships, duration estimation, productivity analysis, and Primavera-compatible scheduling workflows. The platform leverages the existing WPF Shell Workspace, Fluent theme engine, MVVM architecture, and plugin infrastructure as its foundation, extending them with new domain libraries, AI orchestration, and intelligent planning engines.

**Scope**: Phases 14–26 — 13 new phases building on the existing 0–11 foundation.

**Priority**: Phase 14 (Platform Foundation) first — establishes the Domain layer, Contracts, Plugin SDK, and AI Abstraction Layer as the architectural spine for all subsequent phases.

---

## Strategic Direction

| Area | Current State | Target State |
|------|--------------|-------------|
| Identity | Excel add-in with WPF features | Enterprise Planning Platform |
| Architecture | Feature-oriented windows | Plugin-driven workspace shell |
| AI | None | Cloud AI orchestration (Claude, DeepSeek, GLM, Kimi, Codex, OpenAI) |
| Domain | Excel rows directly | Internal domain model (BOQ, Activities, WBS, Relationships) |
| Settings | Static pages | Dynamic registry-based settings per plugin |
| Theme | Fluent | Fluent + Material hybrid |
| Export | Basic Excel | Multi-format pipeline (Excel, CSV, JSON, Primavera-compatible) |
| Localization | Architecture-ready | Full English + Arabic RTL |

---

## Technical Context

**Languages**: C# (.NET Framework 4.8 for VSTO/UI, .NET 8.0 for Domain/AI/Contracts)

**Primary Dependencies**:
- **Existing**: Theme Engine, ThemeManager, ModernWindow, Shell, NavigationService, ServiceContainer, EventBus, ModuleRegistry, DiagnosticsService, ValidationEngine
- **New .NET 8.0**: Som3a.Domain, Som3a.Contracts, Som3a.AI, Som3a.Plugin.SDK, Som3a.Exporting, Som3a.Localization
- **AI SDKs**: OpenAI, Anthropic SDKs; OpenCode orchestration
- **Material Design**: MaterialDesignThemes (icons, dialogs, selected controls)

**Storage**: Existing AppData/Som3a for settings/logs; new plugin settings via dynamic registry; cloud API keys encrypted locally

**Testing**: Unit tests (domain logic, AI parsers, validation); Integration tests (plugin loading, Excel export, shell navigation); UI tests (theme switching, RTL, accessibility); Performance tests (Excel export, AI execution, memory)

**Target Platform**: Windows (x64) — Excel VSTO Add-in host

**Performance Goals**: All shell windows open within 1s; DataGrid with 10,000+ rows scrolls at 60fps; AI operations async with progress indicators; theme switching within 1s across all windows

**Constraints**:
- Must not break existing VSTO functionality
- All new pages/windows must follow DynamicResource-only, centralized-effects, and MVVM rules
- Excel VSTO rendering safety preserved (Constitution X)
- WindowChrome enforcement per Constitution XI
- All phases stop for review gates (build, Excel host, PR, CodeRabbit, architecture review)

---

## Constitution Check

*GATE: Must pass before any phase begins.*

- [ ] **I. Library-First Modular Architecture** — New Domain, AI, Contracts, and Plugin SDK projects are modular class libraries. Each is independently testable and replaceable.
- [ ] **III. DynamicResource-Only Theme Architecture** — All new UI (Pages, dialogs, controls) uses DynamicResource exclusively for themeable properties.
- [ ] **IV. Runtime Theme Mutation Governance** — Theme mutation continues through ThemeManager only. New theme features (background images, blur, fonts) integrate via ThemeManager, not bypass it.
- [ ] **V. Primitive & Semantic Token Architecture** — New Material integration tokens added as Primitive/Semantic tokens. No inline colors.
- [ ] **IX. Animation Governance** — All new animations <=200ms. Reduced motion respected.
- [ ] **X. Excel Rendering Safety** — All new UI validated in Excel VSTO host. Safe mode preserved.
- [ ] **XI. WindowChrome Enforcement** — No new standalone windows; all new features use Pages within Shell.
- [ ] **XII. Centralized Effects Architecture** — No inline DropShadowEffect. Effects from Effects/Shadows.xaml and Effects/Glow.xaml.
- [ ] **XIV. No Third-Party UI Frameworks (except Material)** — ADR-006 authorizes Material Design integration for icons, dialogs, and selected controls.
- [ ] **XV. Resource Loading Order Enforcement** — New resource dictionaries added at proper positions in ThemeResources.xaml.
- [ ] **XVI. Theme Safety & Recovery** — New theme features include fallback recovery paths.

---

## Solution Structure

### New Projects (added to existing WpfApp2 solution)

```text
Som3a Addin 2026/
├── WpfApp2/                              # Existing VSTO + WPF host (.NET Framework 4.8)
│   ├── Services/                         # Existing services (ThemeManager, EventBus, DI, etc.)
│   ├── ViewModels/                       # Existing MVVM ViewModels
│   ├── Views/                            # Existing windows (being migrated to Pages)
│   ├── Pages/                            # Existing + new Pages
│   ├── Controls/                         # Existing custom controls
│   ├── Theme/                            # Existing theme engine (Fluent -> Fluent+Material)
│   └── Models/                           # Existing Excel-focused models
│
├── Som3a.Domain/                         # NEW — .NET 8.0 Class Library
│   ├── BOQ/                              # BOQDocument, BOQSection, BOQItem, Quantity, Unit
│   ├── Activities/                       # Activity, ActivityId, Duration, Productivity
│   ├── WBS/                              # WBSNode, WBSStructure, WBSLevel
│   ├── Relationships/                    # Relationship, Predecessor, Successor, Lag
│   ├── Calendars/                        # Calendar, WorkDay, Holiday
│   ├── Constraints/                      # Constraint, StartOn, FinishOn, Float
│   ├── Resources/                        # Resource, ResourceAssignment, Cost
│   └── Export/                           # ExportBatch, ExportTarget, ExportValidation
│
├── Som3a.Contracts/                      # NEW — .NET 8.0 Class Library
│   ├── IAIProvider.cs                    # AI provider interface
│   ├── IPlugin.cs                        # Plugin contract
│   ├── IExportEngine.cs                  # Export engine interface
│   ├── ISettingsModule.cs                # Settings registry contract
│   ├── INavigationService.cs            # Navigation contract (extend existing)
│   ├── IPromptProvider.cs               # Prompt governance contract
│   └── IDiagnosticsProvider.cs          # Diagnostics contract
│
├── Som3a.AI/                             # NEW — .NET 8.0 Class Library
│   ├── Providers/                        # OpenAIProvider, ClaudeProvider, DeepSeekProvider, etc.
│   ├── Orchestration/                    # AIOrchestrator, ContextBuilder, RetryHandler
│   ├── Prompts/                          # PromptTemplates (BOQ, WBS, Logic, Productivity, Review)
│   ├── Parsing/                          # StructuredOutputParser, JsonSchemaValidator
│   └── Tracking/                         # TokenTracker, UsageReporter
│
├── Som3a.Exporting/                      # NEW — .NET 8.0 Class Library
│   ├── Excel/                            # ExcelExportEngine, SheetGenerator, StyleApplier
│   ├── Primavera/                        # PrimaveraCompatibleWriter
│   ├── Formats/                          # CsvExporter, JsonExporter, XmlExporter
│   └── Pipeline/                         # ExportPipeline (Validate -> Review -> Generate -> Style -> Export)
│
├── Som3a.Plugin.SDK/                     # NEW — .NET 8.0 Class Library
│   ├── Attributes/                       # [Plugin], [SettingsSection], [NavigationItem], [Command]
│   ├── Discovery/                        # PluginDiscoverer, AssemblyScanner
│   ├── Validation/                       # PluginValidator, ContractVerifier
│   └── Hosting/                          # PluginHost, Sandbox
│
├── Som3a.Localization/                   # NEW — .NET 8.0 Class Library
│   ├── Resources/                        # .resx files (English, Arabic)
│   ├── Services/                         # LocalizationService, CultureManager
│   └── RTL/                              # RTLHelper, FlowDirectionManager
│
├── Som3a.Validation/                     # NEW — .NET 8.0 Class Library
│   ├── BOQ/                              # BOQStructureValidator
│   ├── WBS/                              # WBSIntegrityValidator
│   ├── Relationships/                    # LoopDetector, DependencyValidator
│   ├── Duration/                         # DurationLogicValidator
│   └── Export/                           # ExportValidator, PreExportChecker
│
├── Som3a.Diagnostics/                    # NEW — .NET 8.0 Class Library
│   ├── AI/                               # AIRequestLogger, TokenUsageTracker
│   ├── Plugins/                          # PluginHealthMonitor
│   └── Export/                           # ExportDiagnostics
│
├── Som3a.Infrastructure/                 # NEW — .NET 8.0 Class Library
│   ├── Security/                         # ApiKeyEncryption, SecureStorage
│   ├── Configuration/                    # AppConfiguration, FeatureFlags
│   └── Telemetry/                        # PerformanceCounters, UsageTelemetry
│
└── Plugins/                              # NEW — Plugin assemblies (.NET 8.0)
    ├── BOQActivityGenerator/             # Phase 20 — AI activity generation from BOQ
    ├── RelationshipGenerator/            # Phase 21 — Logic engine
    ├── DurationEstimator/                # Phase 22 — Productivity + duration engine
    ├── WBSGenerator/                     # Phase 19 — WBS templates + AI generation
    ├── ProductivityAnalyzer/             # Phase 22 auxiliary
    ├── ScheduleReviewer/                 # AI schedule analysis
    ├── RiskAnalyzer/                     # Risk identification plugin
    └── ExportManager/                    # Export pipeline plugin
```

### Documentation (this feature)

```text
specs/014-enterprise-planning-platform/   # (future — per-phase detailed specs)
├── plan.md                               # This file (master plan)
├── spec.md                               # Master specification
└── tasks.md                              # Master task breakdown
```

Note: The master plan lives at repo root (`enterprise_planning_platform_plan.md`). Per-phase detailed plans will be created as `specs/014-.../plan.md` through `specs/026-.../plan.md` when execution begins.

---

## Phase 14 — Platform Foundation

**Goal**: Establish the architectural backbone — Domain layer, Contracts, Plugin SDK, AI Abstraction Layer, and Export Layer. All subsequent phases depend on these foundations.

**Branch**: `feature/phase-14-platform-foundation`

### P14-T001: Create Solution Structure
- Add new .NET 8.0 class library projects to the existing WpfApp2 solution
- Configure project references between libraries
- Set up NuGet package references (OpenAI, Anthropic, MaterialDesignThemes, System.Text.Json)
- Create interop bridge between .NET Framework 4.8 WPF host and .NET 8.0 libraries

### P14-T002: Implement Som3a.Domain
- BOQ domain: BOQDocument, BOQSection, BOQItem, Quantity, Unit, Description, Classification
- Activity domain: ActivityId, Name, Description, Quantity, Unit, Duration, Productivity, Relationships, Constraints, WBSNode
- WBS domain: WBSNode with Code, Name, Level, Parent, Children, Path
- Relationships domain: Predecessor, Successor, Type (FS/SS/FF/SF), Lag, Validation
- Calendars: Calendar, WorkDay, Holiday, WorkingHours
- Constraints: StartOn, FinishOn, Float, MandatoryStart, MandatoryFinish
- Resources: Resource, ResourceAssignment, Cost, Budget

### P14-T003: Implement Som3a.Contracts
- `IAIProvider` — Execute prompt, stream response, validate output
- `IPlugin` — Initialize, Register, Load UI, Register Settings, Register Commands
- `IExportEngine` — Validate, Generate, Style, Export (Excel, CSV, JSON)
- `ISettingsModule` — RegisterSettings, Validate, Export, Import
- `IPromptProvider` — GetTemplate, BuildContext, ValidatePrompt
- `IDiagnosticsProvider` — CollectSnapshot, ReportHealth, LogDiagnostics

### P14-T004: Implement Som3a.Plugin.SDK
- `[Plugin]` attribute — Id, Name, Version, Priority, Dependencies
- `[SettingsSection]` attribute — Category, Order, UI control type
- `[NavigationItem]` attribute — Category, Icon, Order, Page type
- `[Command]` attribute — Id, Name, KeyBinding, Ribbon group
- `PluginDiscoverer` — Scan assemblies, discover plugins, validate contracts
- `AssemblyScanner` — Load plugin assemblies from Plugins directory
- `PluginValidator` — Verify contracts, check dependencies, validate versions
- `PluginHost` — Lifecycle management, sandbox isolation, error isolation
- `PluginSandbox` — Exception containment, resource limiting

### P14-T005: Implement Som3a.AI (Abstraction Layer)
- `AIProviderBase` — Common provider functionality (retry, timeout, logging)
- `OpenAIProvider` — GPT-4, GPT-4o integration
- `ClaudeProvider` — Anthropic Claude integration
- `DeepSeekProvider` — DeepSeek integration
- `GLMProvider` — GLM integration
- `KimiProvider` — Kimi integration
- `CodexProvider` — Codex integration (for code generation tasks)
- `AIOrchestrator` — Provider routing, fallback, load balancing
- `ContextBuilder` — Build AI context from domain entities
- `RetryHandler` — Exponential backoff, circuit breaker
- `TokenTracker` — Track token usage per request/session
- `StructuredOutputParser` — Parse AI responses into domain entities

### P14-T006: Implement Som3a.Exporting
- `ExportPipeline` — Validation -> Review -> Generation -> Styling -> Export
- `ExcelExportEngine` — Sheet generation, styling, multi-sheet export
- `PrimaveraCompatibleWriter` — Primavera-compatible table formatting
- `CsvExporter`, `JsonExporter`, `XmlExporter`
- `StyleApplier` — Apply theme-consistent styling to exported sheets
- `ExportValidator` — Pre-export validation

### P14-T007: Build Validation
- `msbuild` solution — WpfApp2 (NETFX 4.8) compiles
- `dotnet build` all .NET 8.0 libraries (Som3a.Contracts, Som3a.Domain, etc.)
- All contracts implementable and testable
- Domain entities serializable/deserializable (JSON)
- Plugin SDK can discover a test plugin

### P14-T008: Architecture Review Gate
- Verify solution structure matches enterprise folder layout
- Verify .NET Framework 4.8 <-> .NET 8.0 interop bridge works
- Verify all interfaces follow naming conventions
- Verify domain entities match enterprise planning domain

**Acceptance Criteria**:
- [ ] All 7 new projects created in solution
- [ ] Domain layer with BOQ, Activity, WBS, Relationship entities
- [ ] Contracts layer with IAIProvider, IPlugin, IExportEngine, ISettingsModule, IPromptProvider
- [ ] Plugin SDK with discovery, validation, hosting
- [ ] AI abstraction with 6 provider stubs, orchestrator, context builder
- [ ] Export engine with pipeline, Excel writer, format exporters
- [ ] Build passes with zero errors
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean
- [ ] Manual architecture review passed

---

## Phase 15 — Shell Refactor

**Goal**: Complete the migration of all remaining standalone windows to Pages within the unified Shell workspace. Add full sidebar categories and dynamic navigation support.

**Branch**: `feature/phase-15-shell-refactor`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 11 (Legacy Window Migration), Phase 8 (Navigation Shell Platform)

### P15-T001: Verify Phase 11 Completion
- Confirm all 14 windows migrated to Pages
- Verify Shell navigation works for all migrated pages
- Verify Ribbon launchers updated for all pages

### P15-T002: Add Sidebar Categories
- **Planning**: BOQ, WBS, Activities, Relationships, Durations
- **Analysis**: Float Path, Critical Path, Diagnostics
- **Excel**: Links, Formatting, Reports
- **AI**: Activity Generator, WBS Generator, Review Engine
- **Settings**: Appearance, Accessibility, Performance, Diagnostics

### P15-T003: Implement Dynamic Navigation Registration
- Navigation items registered via `[NavigationItem]` attribute from Plugin SDK
- Sidebar dynamically builds from registered navigation items
- Category grouping based on attribute category
- Icon support via Material Design icons

### P15-T004: Workspace Cleanup
- Remove legacy window references
- Remove deprecated navigation paths
- Clean up orphaned ViewModels and Views
- Ensure all features accessible via Shell only

**Acceptance Criteria**:
- [ ] All features accessible via Shell sidebar
- [ ] Sidebar categories populated dynamically from plugin registrations
- [ ] No orphaned standalone windows remain
- [ ] Build passes
- [ ] Excel VSTO host test passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean

---

## Phase 16 — Dynamic Settings Platform

**Goal**: Replace static settings pages with a dynamic registry-based settings system where every plugin registers its own settings sections, validation rules, UI controls, and persistence handlers.

**Branch**: `feature/phase-16-dynamic-settings-platform`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 15 (Shell Refactor)

### P16-T001: Implement Settings Registry
- `ISettingsModule` — RegisterSettings, Validate, Export, Import
- `SettingsRegistry` — Central registry for all plugin settings
- `SettingsSection` — Category, order, UI renderer
- Plugin settings auto-discovered via `[SettingsSection]` attribute

### P16-T002: Create Dynamic Settings UI
- Settings page dynamically builds sections from registry
- Each section renders its registered UI controls
- Validation runs on settings change
- Import/export per-plugin or full settings export

### P16-T003: Migrate Existing Settings
- Theme settings -> registry
- Accessibility settings -> registry
- Performance settings -> registry
- Diagnostics settings -> registry
- Excel settings -> registry

### P16-T004: Add Settings Persistence
- Per-plugin JSON settings files
- Encrypted API key storage
- Settings versioning for migration
- Default settings fallback

**Acceptance Criteria**:
- [ ] All plugins can register settings via ISettingsModule
- [ ] Settings UI dynamically built from registry
- [ ] Existing settings migrated to registry
- [ ] Import/export works per-plugin and full settings
- [ ] API keys encrypted at rest
- [ ] Build passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean

---

## Phase 17 — Theme Expansion

**Goal**: Enhance the existing Fluent theme engine with Material Design integration, background image support, blur effects, font selection, and shell window controls.

**Branch**: `feature/phase-17-theme-expansion`

**Prerequisites**: Phase 7 (Settings & Personalization UX), Phase 3 (Theme Engine 2.0)

### P17-T001: Material Design Integration
- Add MaterialDesignThemes NuGet package
- Material icons library for sidebar, toolbar, navigation
- Material DialogHost for dialogs
- Selected Material controls (sliders, chips, toggles)
- Preserve all existing Fluent tokens — no replacement, only addition

### P17-T002: Background Image & Blur Support
- `WindowBackdrop` DP on ModernWindow (solid/gradient/image/blur)
- Blur effect via Win32 DWM API (SetWindowCompositionAttribute)
- Background image picker in Custom theme settings
- Dynamic blur toggle in settings

### P17-T003: Font System
- Font family selection in Custom theme
- Font preview thumbnails in settings
- Arabic typography presets (for Phase 24)
- Dynamic font switching across all windows

### P17-T004: Shell Window Controls
- Minimize, Maximize, Restore, Close buttons unified across all windows
- Window control hover/pressed states with theme tokens
- Title bar drag region refinement

### P17-T005: Accent Customization Enhancements
- Expanded accent picker (color wheel)
- Custom hex input
- Accent variant generation (hover, pressed, glow, border, subtle)

### P17-T006: Light Theme Fixes
- Correct semantic tokens for light theme
- Contrast validation (WCAG 2.1 AA — 4.5:1 minimum)
- Accent consistency across light/dark/custom

**Acceptance Criteria**:
- [ ] Material icons render in sidebar and navigation
- [ ] Material DialogHost works for all dialogs
- [ ] Background image support with blur toggle
- [ ] Font selection with preview thumbnails
- [ ] Light theme contrast passes WCAG 2.1 AA
- [ ] All accent variants generated correctly
- [ ] Build passes
- [ ] Excel VSTO host test passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean

---

## Phase 18 — AI Core Infrastructure

**Goal**: Implement the full AI abstraction layer with provider adapters, orchestration engine, prompt governance system, structured output parsing, and streaming support.

**Branch**: `feature/phase-18-ai-core-infrastructure`

**Prerequisites**: Phase 14 (Platform Foundation — Contracts + Domain), Phase 16 (Dynamic Settings)

### P18-T001: Implement AI Provider Adapters
- Complete all 6 providers (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex)
- Provider configuration via dynamic settings
- Health check endpoint per provider
- API key management (encrypted at rest)

### P18-T002: Implement AI Orchestration Engine
- Prompt execution pipeline
- Context building from domain entities
- Structured output enforcement (JSON Schema)
- Retry handling with exponential backoff
- Streaming response support
- Token tracking and usage reporting
- Provider fallback (primary -> secondary -> fallback)
- Request queue and rate limiting

### P18-T003: Implement Prompt Governance System
- Prompt template registry
- Template categories: BOQ, WBS, Logic, Productivity, Validation, Review
- Template versioning
- Context window budgeting
- Prompt validation (mustache/template syntax)
- System prompt + user prompt separation
- Few-shot example management
- Output schema enforcement per template

### P18-T004: Structured Output Parsers
- BOQ activity parser -> List<Activity>
- WBS structure parser -> WBSNode tree
- Relationship parser -> List<Relationship>
- Duration parser -> Duration + Productivity data
- Review parser -> ValidationResult + Recommendations
- JSON Schema validation for all outputs

**Acceptance Criteria**:
- [ ] All 6 AI providers connect and respond
- [ ] Orchestrator handles provider failover
- [ ] Prompt templates for all 6 categories
- [ ] Structured parsers produce valid domain entities
- [ ] Token tracking and usage reports
- [ ] Streaming responses work
- [ ] Build passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean

---

## Phase 19 — WBS Engine

**Goal**: Implement WBS template system, AI-powered WBS generation, and WBS export capabilities.

**Branch**: `feature/phase-19-wbs-engine`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 18 (AI Core Infrastructure)

### P19-T001: WBS Template System
- Building templates (residential, commercial, industrial)
- Infrastructure templates (roads, bridges, utilities)
- MEP templates (mechanical, electrical, plumbing)
- Industrial templates (oil & gas, manufacturing)
- Fitout templates (office, retail, hospitality)
- Template editor for custom structures
- Template import/export

### P19-T002: AI WBS Generation
- AI prompt: project type -> WBS structure
- Context: project description, scope, industry standards
- Output: WBSNode tree with Code, Name, Level, Parent
- Validation: tree integrity, naming conventions, level depth limits

### P19-T003: WBS Export
- Excel WBS sheet with indentation/grouping
- WBS code auto-generation (1, 1.1, 1.1.1, etc.)
- Full path generation
- Export as structured data (JSON, XML)

**Acceptance Criteria**:
- [ ] 15+ WBS templates across 5 categories
- [ ] AI generates valid WBS trees from project description
- [ ] WBS exports correctly to Excel with proper indentation
- [ ] Template editor functional
- [ ] Build passes

---

## Phase 20 — BOQ Activity Generator Plugin

**Goal**: AI-powered generation of construction activities from Excel BOQ data — the first user-visible AI feature delivering immediate planning engineer value.

**Branch**: `feature/phase-20-boq-activity-generator`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 18 (AI Core), Phase 19 (WBS Engine)

### P20-T001: BOQ Context Builder
- Read BOQ from Excel sheet
- Build AI context with BOQ items, quantities, classifications
- Identify activity candidates from BOQ groupings
- Context size optimization for token limits

### P20-T002: AI Activity Generation
- Prompt: BOQ context -> Activity list
- Output: Activity ID, Name, Description, BOQ Reference, Quantity, Unit
- Intelligent grouping (similar items -> single activity)
- Activity naming conventions (verb-noun standard)

### P20-T003: Activity Validation
- Duplicate detection
- Naming convention check
- Missing BOQ reference check
- Quantity consistency validation
- User review/edit grid

### P20-T004: Activity Sequencing
- Order activities by WBS/trade sequence
- Detect parallel vs sequential work
- Basic dependency suggestions

### P20-T005: Excel Export
- Export generated activities to dedicated Excel sheet
- Styling consistent with theme
- Activity ID, Name, Description, BOQ Ref, Quantity, Unit columns
- Preserve user edits on re-generation

**Acceptance Criteria**:
- [ ] BOQ successfully read from Excel sheet
- [ ] AI generates 50+ activities from typical BOQ
- [ ] Generated activities pass validation
- [ ] User can review and edit in grid
- [ ] Exported Excel sheet correctly formatted
- [ ] Build passes

---

## Phase 21 — Relationship Generator Plugin

**Goal**: AI-powered logic engine that generates predecessor/successor relationships, supports all 4 relationship types, adds lag, validates for loops, and analyzes parallel execution paths.

**Branch**: `feature/phase-21-relationship-generator`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 18 (AI Core), Phase 20 (BOQ Activity Generator)

### P21-T001: Logic Engine
- Build predecessor/successor candidates from activity list
- Generate relationship types (FS, SS, FF, SF)
- Add lag values (positive, negative, zero)
- Dependency reasoning (trade sequence, space constraints, resource flow)

### P21-T002: Loop Detection & Validation
- Circular dependency detection (graph cycle algorithm)
- Open-end detection (no predecessor, no successor)
- Dangling activity detection
- Redundant relationship detection
- Validation report generation

### P21-T003: Parallel Execution Analysis
- Identify independent work streams
- Calculate parallel execution groups
- Critical path to longest path identification
- Resource conflict awareness

**Acceptance Criteria**:
- [ ] All 4 relationship types (FS/SS/FF/SF) generated correctly
- [ ] Loop detection catches all cycles
- [ ] Validation reports identify open ends and dangling activities
- [ ] Parallel execution groups identified
- [ ] Build passes

---

## Phase 22 — Duration Estimator Plugin

**Goal**: Productivity analysis engine that calculates durations from quantities, benchmarks productivity rates, estimates variances, and provides AI-based recommendations.

**Branch**: `feature/phase-22-duration-estimator`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 18 (AI Core), Phase 20 (BOQ Activity Generator)

### P22-T001: Productivity Engine
- Activity quantity -> productivity rate -> duration calculation
- Productivity benchmark database
- Trades library (concrete, steel, MEP, finishes, etc.)
- Crew size factor adjustment
- Learning curve and complexity factors

### P22-T002: Duration Calculation
- Duration = Quantity / (Productivity * Crew * Hours Per Day)
- Calendar-aware duration (working days only)
- Zone/phased construction adjustment
- Weather and site condition factors

### P22-T003: Variance Analysis
- Optimistic / Most Likely / Pessimistic estimates
- Standard deviation calculation
- Confidence intervals
- Risk-adjusted durations

### P22-T004: AI Recommendations
- AI productivity rate suggestions based on project type
- Duration benchmarking against similar projects
- Anomaly detection (unusual durations flagged)
- Optimization suggestions

**Acceptance Criteria**:
- [ ] Duration calculated correctly from quantity * productivity
- [ ] Calendar-aware durations (working days only)
- [ ] Variance analysis with 3-point estimates
- [ ] AI productivity suggestions validated
- [ ] Build passes

---

## Phase 23 — Dashboard & Home

**Goal**: Create the Home dashboard as the landing page when the Shell workspace opens, with widgets for version, updates, recent tools, diagnostics summary, AI status, and quick actions.

**Branch**: `feature/phase-23-dashboard-home`

**Prerequisites**: Phase 15 (Shell Refactor), Phase 16 (Dynamic Settings), Phase 18 (AI Core)

### P23-T001: Home Page Widgets
- **Current version** — app version, .NET version, Excel version, Windows version
- **Latest updates** — changelog from git releases
- **Recent tools** — last 5 used features with quick-launch
- **Recent projects** — last opened Excel files/projects
- **Diagnostics summary** — render mode, GPU, theme, memory
- **AI provider status** — online/offline per provider, token usage this session
- **Performance summary** — startup time, last operation duration
- **Quick actions** — New BOQ Analysis, Generate WBS, Open Settings
- **Plugin status** — loaded plugin count, plugin health

### P23-T002: Diagnostics Dashboard
- Render mode indicator
- Active theme with accent swatch
- Current DPI scale
- Memory usage gauge
- GPU acceleration status
- Popup diagnostics
- Last error log entry

**Acceptance Criteria**:
- [ ] Home page renders on shell open
- [ ] All 9 widget types functional
- [ ] Diagnostics dashboard updates in real-time
- [ ] Quick actions launch correctly
- [ ] Plugin status reflects loaded/healthy/unhealthy
- [ ] Build passes
- [ ] Excel VSTO host test passes

---

## Phase 24 — Localization & RTL

**Goal**: Full Arabic + English localization with dynamic RTL switching, Arabic typography presets, and culture-aware formatting for numbers, dates, and currencies.

**Branch**: `feature/phase-24-localization-rtl`

**Prerequisites**: Phase 15 (Shell Refactor), Phase 17 (Theme Expansion — Arabic font presets)

### P24-T001: Localization Infrastructure
- Som3a.Localization project with .resx files
- English (default) and Arabic resource sets
- `LocalizationService` with `CultureManager`
- Dynamic language switching without restart
- Culture-aware formatting (number separators, date formats, currency)

### P24-T002: RTL Engine
- `RTLHelper` for FlowDirection management
- Dynamic FlowDirection switching per window
- RTL-aware layout (mirrored sidebar, reversed navigation)
- Arabic typography presets (font family, size adjustments)
- Right-to-left text alignment for all controls

### P24-T003: UI Translation Coverage
- All shell menus and navigation
- All settings pages
- All dashboard widgets
- All plugin UI (at minimum English fallback)
- Error messages and validation feedback

### P24-T004: Font Switching
- Arabic font family selection
- Font fallback chain (Arabic -> Latin)
- Ligature support for Arabic script
- Font size scaling for Arabic readability

**Acceptance Criteria**:
- [ ] Language switch between English and Arabic without restart
- [ ] RTL layout mirrors correctly (sidebar right, text right-aligned)
- [ ] Arabic typography renders correctly with proper shaping
- [ ] Number/date/currency formatting culture-aware
- [ ] All static UI text translated
- [ ] Build passes
- [ ] Excel VSTO host test passes in both languages

---

## Phase 25 — Full Platform Rebranding & Visual Identity System

**Goal**: Transform the entire platform identity from Som3a Addin 2026 into Planova Platform while introducing a complete enterprise branding and visual identity system.

**Branch**: `feature/phase-25-platform-rebranding`

**Prerequisites**: Phase 14 (Platform Foundation), Phase 15 (Shell Refactor), Phase 17 (Theme Expansion), Phase 24 (Localization & RTL)

### 25.1 Objectives

#### Core Rebranding
- Rename solution
- Rename projects
- Rename assemblies
- Rename namespaces
- Rename package identities
- Rename output binaries

### 25.2 Visual Identity System

#### Required Deliverables

##### Brand Assets Folder

```
Assets/
 ├── Branding/
 │   ├── Logos/
 │   │   ├── SVG/
 │   │   ├── PNG/
 │   │   ├── ICO/
 │   │   ├── PDF/
 │   │   ├── Transparent/
 │   │   ├── Dark/
 │   │   └── Light/
 │   │
 │   ├── Ribbon/
 │   ├── Icons/
 │   ├── Splash/
 │   ├── Wallpapers/
 │   ├── Fonts/
 │   └── Social/
```

### 25.3 Logo Asset Requirements

#### Export Formats

##### SVG

Used for:
- UI scaling
- Website
- Documentation
- Future web platform

##### PNG Sizes
- 64x64
- 128x128
- 256x256
- 512x512
- 1024x1024

##### ICO

Used for:
- EXE icon
- Installer icon
- Shortcut icon

##### Transparent PNG

Used inside:
- Shell
- Splash screen
- Ribbon
- Dashboard

### 25.4 Official Brand Colors

#### Dark Theme Palette

| Token          | Color   |
| -------------- | ------- |
| Background     | #0E1720 |
| Surface        | #13202B |
| Border         | #243647 |
| Accent Blue    | #2D9CFF |
| Accent Cyan    | #00D1FF |
| Accent Orange  | #FF8A3D |
| Text Primary   | #FFFFFF |
| Text Secondary | #B7C5D3 |

### 25.5 Light Theme Palette

| Token          | Color   |
| -------------- | ------- |
| Background     | #F5F7FA |
| Surface        | #FFFFFF |
| Border         | #D6E2EE |
| Accent Blue    | #2D9CFF |
| Accent Cyan    | #00B8E6 |
| Accent Orange  | #FF8A3D |
| Text Primary   | #102030 |
| Text Secondary | #5B7186 |

### 25.6 Theme Refactoring

#### Required Changes

##### Dark Theme
- Replace old slate tokens
- Add neon cyan highlights
- Improve glass effects
- Add blueprint-inspired overlays

##### Light Theme
- Convert to engineering-white style
- Soft surfaces
- Professional contrast
- Blueprint grid accents

##### Custom Theme

Add support for:
- Background images
- Blur intensity
- Accent override
- Font selection
- Font preview thumbnails

### 25.7 Shell Branding Updates

#### Sidebar
- Add Planova logo
- Add mini animated icon
- Add branding footer

#### Home Page
- Product branding section
- Version information
- Release notes card

#### Splash Screen

Animated startup:
- Blueprint lines
- Building formation
- Logo reveal
- Glow animation

### 25.8 Ribbon Rebranding

#### Requirements
- New Planova ribbon icons
- Unified icon language
- Gradient engineering style
- Dark/light compatible icons

### 25.9 Typography System

#### English Fonts
- Inter
- Segoe UI Variable

#### Arabic Fonts
- Cairo
- IBM Plex Sans Arabic
- Tajawal

#### Features
- Dynamic font switching
- RTL support
- Typography presets

### 25.10 Namespace Migration Strategy

#### Important Rule

Namespace migration occurs ONLY after:
- Feature stabilization
- Plugin stabilization
- Architecture freeze

to avoid:
- Broken references
- Merge conflicts
- Build instability

### 25.11 Final Project Structure

```
Planova.Platform
Planova.Domain
Planova.Contracts
Planova.AI
Planova.Exporting
Planova.Localization
Planova.Diagnostics
Planova.Plugin.SDK
Planova.Infrastructure
```

### 25.12 Branding Validation Checklist

#### Technical
- Assembly names
- Project names
- Resource keys
- Config files
- Installer metadata
- EXE metadata

#### UI
- Logos
- Theme colors
- Ribbon icons
- Splash assets
- Typography

#### Documentation
- README
- Specs
- Architecture docs
- GitHub branding
- Release documentation

---

### 25.13 Master Branding Reference System

**Goal**: Create a centralized visual reference system that becomes the authoritative design source for:
- UI
- Themes
- Icons
- Splash screens
- Ribbon assets
- Branding consistency
- Future AI-generated assets

### 25.14 Master Brand Asset

#### Official Reference File
```
planova-master-brand-reference.png
```

#### Storage Location
```
Assets/Branding/Master/
```

### 25.15 Branding Asset Architecture

#### Required Folder Structure

```
Assets/
 └── Branding/
      │
      ├── Master/
      │    └── planova-master-brand-reference.png
      │
      ├── References/
      │    ├── UI Inspirations/
      │    ├── Splash Inspirations/
      │    ├── Ribbon Inspirations/
      │    └── Theme Inspirations/
      │
      ├── Logos/
      │    ├── SVG/
      │    ├── PNG/
      │    ├── ICO/
      │    ├── Transparent/
      │    ├── Dark/
      │    ├── Light/
      │    └── Monochrome/
      │
      ├── Ribbon/
      │
      ├── Splash/
      │
      ├── Wallpapers/
      │
      ├── Icons/
      │
      ├── Theme/
      │
      └── Fonts/
```

### 25.16 Master Brand Responsibilities

The selected logo becomes the official visual authority for:

#### Branding Direction
- Engineering aesthetic
- Construction-tech identity
- AI planning style
- Professional enterprise appearance

#### Color Direction
- Primary blue
- Cyan glow
- Accent orange
- Dark engineering surfaces
- Blueprint overlays

#### UI Language
- Thin lines
- Glass effects
- Glow accents
- Grid overlays
- Blueprint-inspired visuals

### 25.17 Asset Extraction Pipeline

The system should later generate from the master asset:

#### Logo Variants
- Horizontal logo
- Vertical logo
- Symbol only
- Monochrome dark
- Monochrome light
- Transparent version

#### App Assets
- EXE icon
- MSI installer icon
- Ribbon icons
- Sidebar icon
- Splash screen logo

#### Theme Assets
- Wallpapers
- Blueprint overlays
- Background textures
- Glow patterns

### 25.18 Official Design Tokens Extraction

#### Colors extracted from master branding
```
PrimaryBlue
PrimaryCyan
AccentOrange
DarkSurface
BlueprintGlow
GlassBorder
EngineeringWhite
```

These tokens become:
- Global theme tokens
- Shell colors
- Ribbon colors
- Dashboard accents
- Plugin UI accents

### 25.19 Branding-Driven Theme Refactor — Dark

#### Visual Style
- Futuristic engineering workspace
- Neon cyan highlights
- Blueprint overlays
- Deep dark surfaces
- Soft reflections

### 25.20 Branding-Driven Theme Refactor — Light

#### Visual Style
- Professional engineering white
- Soft gray surfaces
- Blueprint-inspired accents
- High readability
- Technical UI feel

### 25.21 Branding-Driven UI Components

#### Components affected by branding

##### Shell
- Sidebar
- Navigation cards
- Home dashboard
- Workspace background

##### Ribbon
- Tool icons
- Accent highlights
- Hover states

##### Plugins
- Cards
- Headers
- Export visuals

##### Splash Screen
- Startup animation
- Glow effects
- Animated logo reveal

### 25.22 Future AI Design Integration

The branding folder also becomes:

#### AI Visual Context Source

Used by:
- OpenCode
- Claude
- DeepSeek
- Future design automation

to maintain:
- Visual consistency
- Unified iconography
- Unified UI language

### 25.23 Branding Workflow Rule

#### Important Rule

No UI redesign should happen without referencing:
```
Assets/Branding/Master/planova-master-brand-reference.png
```

This file becomes the **Official Visual Source of Truth**.

### 25.24 Final Branding Goal

Transform the platform visually into **Planova Platform** with:
- Enterprise construction-tech identity
- AI planning aesthetics
- Unified engineering workspace appearance
- Modern Fluent/Material hybrid visuals
- Professional scheduling platform branding

---

## Phase 26 — Release Candidate & Production Packaging

**Goal**: Prepare Planova Platform for production release, enterprise deployment, installer packaging, and final validation.

**Branch**: `feature/phase-26-release-candidate`

**Prerequisites**: All Phases 14–25

### 26.1 Release Pipeline

```
Validation
 → Optimization
 → Packaging
 → Installer
 → Final QA
 → Release Candidate
```

### 26.2 Final Validation

#### Required Validation Areas

##### UI Validation
- Theme switching
- Shell navigation
- RTL
- Accessibility

##### Plugin Validation
- Loading
- Isolation
- Stability

##### AI Validation
- Prompt outputs
- Retry handling
- Structured JSON parsing

##### Excel Validation
- Export speed
- Large workbook support
- Interop cleanup

### 26.3 Performance Optimization

#### Required Optimizations
- Virtualized UI
- Batch Excel writes
- Lazy loading
- Background processing
- Memory cleanup

### 26.4 Production Packaging

#### Deliverables

##### Installer
- MSI installer
- Desktop shortcut
- Start menu integration

##### Executables
- Signed binaries
- Production icons
- Metadata branding

##### Configurations
- Production appsettings
- AI provider configs
- Logging configs

### 26.5 Documentation Export

#### Required Documents
- User guide
- Admin guide
- Plugin SDK guide
- Architecture guide
- AI provider setup
- Troubleshooting guide

### 26.6 Diagnostics Release Mode

#### Production Diagnostics
- Crash recovery
- Safe logging
- Export diagnostics
- Plugin diagnostics

### 26.7 Release Candidate Checklist

#### Technical
- Clean build
- No warnings
- Stable plugins
- Stable exports

#### UI
- Final branding
- Final themes
- Responsive layouts

#### AI
- Provider fallback working
- Retry system stable
- Token tracking stable

#### Documentation
- Complete
- Versioned
- Exported

### 26.8 Final Deliverables

#### Platform Output
- Planova Platform Installer
- Enterprise Workspace Shell
- AI Planning Plugins
- Dynamic WBS Engine
- BOQ Activity Generator
- Relationship Generator
- Duration Estimator
- Enterprise Export Engine

### 26.9 Final Product Identity

#### Official Product Name

**Planova Platform**

#### Official Subtitle

**AI Planning Platform**

#### Brand Direction
- Enterprise
- Construction Technology
- AI-Assisted Planning
- Modern Engineering Workspace

#### Final Result

The platform evolves from:
- Traditional Excel Add-in

into:
- Enterprise AI Planning Workspace

with:
- Plugin-driven architecture
- AI orchestration
- Dynamic planning engines
- Modern Fluent/Material UI
- Professional engineering branding

**Acceptance Criteria**:
- [ ] All branding assets created and validated
- [ ] Theme colors updated to new brand palette
- [ ] Shell rebranded with Planova identity
- [ ] Ribbon icons updated
- [ ] Typography system implemented
- [ ] Namespace migration scoped for post-stabilization
- [ ] Installer created and tested
- [ ] Release candidate built and validated
- [ ] Documentation exported
- [ ] Build passes with zero errors
- [ ] Excel VSTO host test passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean

---

## Git Branch Strategy

```text
feature/phase-14-platform-foundation
feature/phase-15-shell-refactor
feature/phase-16-dynamic-settings-platform
feature/phase-17-theme-expansion
feature/phase-18-ai-core-infrastructure
feature/phase-19-wbs-engine
feature/phase-20-boq-activity-generator
feature/phase-21-relationship-generator
feature/phase-22-duration-estimator
feature/phase-23-dashboard-home
feature/phase-24-localization-rtl
feature/phase-25-platform-rebranding
feature/phase-26-release-candidate
```

Plugin branches follow pattern:
```text
feature/plugin-boq-generator
feature/plugin-wbs-generator
feature/plugin-relationship-generator
feature/plugin-duration-estimator
feature/plugin-schedule-reviewer
feature/plugin-risk-analyzer
feature/plugin-export-manager
```

---

## Mandatory Review Gates (per phase)

Each phase MUST stop after completion for:

1. **Build**: `msbuild` passes with zero errors
2. **Excel Host**: Manual VSTO host test
3. **GitHub PR**: Architecture + style + performance review
4. **CodeRabbit**: Code smells, performance, MVVM, memory, leaks, duplicates
5. **Architecture Review**: Token usage, DynamicResource, no inline values, naming consistency

---

## Testing Strategy

| Type | Scope | Phase |
|------|-------|-------|
| Unit Tests | Domain logic, AI parsers, validation engines | All |
| Integration Tests | Plugin loading, Excel export, shell navigation | 14, 15, 19-22 |
| UI Tests | Theme switching, RTL, accessibility | 17, 23, 24 |
| Performance Tests | Excel export speed, AI execution, memory usage | 26 |
| Regression Tests | Full feature suite across all themes/DPIs | 26 |

---

## Risk Analysis

| Risk | Mitigation |
|------|-----------|
| .NET 4.8 <-> 8.0 interop complexity | Early proof-of-concept bridge in Phase 14 |
| AI hallucination on schedule data | Structured output enforcement + validation pipeline |
| Plugin explosion / coupling | Strong contracts in Som3a.Contracts + Plugin SDK |
| Material vs Fluent token conflicts | Additive integration — no replacement of existing tokens |
| RTL rendering instability | Early RTL infrastructure in Phase 24, continuous testing |
| Excel performance degradation | Batch processing, async operations, no blocking UI |
| Theme expansion breaking existing themes | Existing themes preserved, new features additive |

---

## ADR Registry

| ADR | Decision |
|-----|----------|
| ADR-001 | Shell becomes the main platform — all windows migrate to Pages |
| ADR-002 | Plugin-first architecture — all AI features as isolated plugins |
| ADR-003 | Cloud AI only — Claude, DeepSeek, GLM, Kimi, Codex, OpenAI via OpenCode |
| ADR-004 | Internal domain model required — no direct Excel row operations |
| ADR-005 | Dynamic settings registry — replace static settings pages |
| ADR-006 | Fluent + Material hybrid — preserve existing, add material icons/dialogs/controls |

---

## Immediate Next Steps

1. **Create** `Som3a.Domain`, `Som3a.Contracts`, `Som3a.AI`, `Som3a.Plugin.SDK` projects (Phase 14 core)
2. **Refactor** Shell navigation for new sidebar categories (Phase 15 prep)
3. **Implement** AI provider abstraction with at least 1 working provider (Phase 18 early validation)
4. **Start** Domain entity design workshops to finalize the core domain model
