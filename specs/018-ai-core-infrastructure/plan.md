# Implementation Plan: AI Core Infrastructure

**Branch**: `018-ai-core-infrastructure` | **Date**: 2026-05-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/018-ai-core-infrastructure/spec.md`

## Summary

Implement the full AI abstraction layer for the Som3a Enterprise Planning Platform: 6 provider adapters (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex), orchestration engine with streaming and failover, prompt governance system with Draft→Published→Deprecated lifecycle, and structured output parsers producing domain entities (Activity, WBSNode, Relationship, Duration, ValidationResult). This infrastructure powers all downstream AI features (Phases 19-27).

## Technical Context

**Language/Version**: C# (.NET 8.0 for Som3a.AI class library; .NET Framework 4.8 for WpfApp2 UI host)

**Primary Dependencies**: OpenAI SDK, Anthropic SDK; DeepSeek/GLM/Kimi/Codex HTTP clients (REST); Som3a.Domain (Phase 14 entities); Dynamic Settings registry (Phase 16); ServiceContainer/EventBus (existing WpfApp2 services); System.Text.Json for schema validation; Som3a.Diagnostics for logging

**Storage**: Dynamic Settings registry (Phase 16) for provider configuration (API keys encrypted at rest); file system (AppData/Som3a/templates/) for prompt template storage; in-memory token usage aggregated per session

**Testing**: xUnit for unit tests (parser logic, schema validation, provider adapter mocks); integration tests (orchestration pipeline, failover, streaming); no UI tests needed (infrastructure layer)

**Target Platform**: Windows x64 — part of Som3a.AI .NET 8.0 library consumed by WpfApp2 .NET Framework 4.8 VSTO host via Som3a.Bridge

**Project Type**: .NET 8.0 class library (Som3a.AI) with internal services consumed by WpfApp2 UI

**Performance Goals**: Provider health check ≤5s; failover ≤10s; structured parsing ≤1s; streaming response begins ≤2s; token count within 5% of provider-reported

**Constraints**: No third-party UI frameworks (Constitution §XIV); all provider communication async non-blocking; API keys encrypted at rest (DPAPI/Windows Data Protection); standard AI SDK rate limit compliance

**Scale/Scope**: Single-user desktop app (Excel VSTO); 6 AI providers; 6 prompt template categories; 5 structured parser types; one active request per provider per user (per-provider RequestQueue with token-bucket rate limiting allows concurrent requests to different providers)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Som3a.AI is a standalone .NET 8.0 class library; no monolithic dictionaries introduced.
- [x] **III. DynamicResource-Only** — No themeable UI resources in this layer; all configuration is code-based settings.
- [x] **IV. Runtime Theme Mutation Governance** — No theme mutation in Som3a.AI; all UI remains in WpfApp2 with ThemeManager.
- [x] **IX. Animation Governance** — No animations in this infrastructure layer.
- [x] **X. Excel Rendering Safety** — No UI in Som3a.AI; streaming progress events consumed via EventBus by WpfApp2.
- [x] **XI. WindowChrome Enforcement** — No windows introduced by this library.
- [x] **xII. Centralized Effects** — No effects in this infrastructure layer.
- [x] **XV. Resource Loading Order** — No ResourceDictionaries introduced by Som3a.AI.

## Project Structure

### Documentation (this feature)

```text
specs/018-ai-core-infrastructure/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Som3a.AI/
├── Providers/
│   ├── IAIProvider.cs              # Provider adapter interface
│   ├── OpenAIProvider.cs
│   ├── ClaudeProvider.cs
│   ├── DeepSeekProvider.cs
│   ├── GLMProvider.cs
│   ├── KimiProvider.cs
│   └── CodexProvider.cs
├── Orchestration/
│   ├── OrchestrationEngine.cs      # Prompt execution pipeline
│   ├── PromptExecutionContext.cs   # Context builder from domain entities
│   ├── ProviderRouter.cs           # Primary → secondary → fallback
│   ├── StreamingHandler.cs         # Streaming response support
│   ├── RetryPolicy.cs              # Exponential backoff
│   ├── RequestQueue.cs             # Rate limiting + queue
│   └── TokenTracker.cs             # Usage tracking
├── Prompts/
│   ├── PromptTemplateRegistry.cs   # Template catalog
│   ├── PromptTemplate.cs           # Template model with lifecycle
│   ├── TemplateValidator.cs        # Syntax validation at registration
│   └── ContextBudgetEstimator.cs   # Token window budgeting
├── Parsers/
│   ├── BaseStructuredParser.cs     # Abstract JSON Schema validator
│   ├── ActivityParser.cs           # → List<Activity>
│   ├── WBSParser.cs                # → WBSNode tree
│   ├── RelationshipParser.cs       # → List<Relationship>
│   ├── DurationParser.cs           # → Duration + Productivity
│   └── ReviewParser.cs             # → ValidationResult + Recommendations
├── Configuration/
│   ├── ProviderConfig.cs           # Provider settings model
│   └── EncryptionService.cs        # API key encryption (DPAPI)
└── Som3a.AI.csproj

Som3a.AI.Tests/
├── Providers/
│   ├── ProviderAdapterTests.cs
│   └── HealthCheckTests.cs
├── Orchestration/
│   ├── OrchestrationEngineTests.cs
│   ├── ProviderRouterTests.cs
│   ├── RetryPolicyTests.cs
│   └── RequestQueueTests.cs
├── Prompts/
│   ├── TemplateRegistryTests.cs
│   └── TemplateValidatorTests.cs
├── Parsers/
│   ├── ActivityParserTests.cs
│   ├── WBSParserTests.cs
│   ├── RelationshipParserTests.cs
│   ├── DurationParserTests.cs
│   └── ReviewParserTests.cs
└── Som3a.AI.Tests.csproj
```

**Structure Decision**: Standard .NET 8.0 class library layout with feature folders (Providers, Orchestration, Prompts, Parsers, Configuration) and corresponding test project mirroring the structure. No UI or data access layers — pure service/infrastructure code.

## Complexity Tracking

No constitution violations detected — all checks pass cleanly.
