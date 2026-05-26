# Quickstart: AI Core Infrastructure

**Prerequisites**:
- Som3a.Domain (Phase 14) built
- Dynamic Settings (Phase 16) built
- .NET 8.0 SDK installed

## 1. Create the Som3a.AI Project

```powershell
dotnet new classlib -n Som3a.AI -o Som3a.AI
dotnet new xunit -n Som3a.AI.Tests -o Som3a.AI.Tests
dotnet add Som3a.AI/Som3a.AI.csproj reference Som3a.Domain/Som3a.Domain.csproj
dotnet add Som3a.AI/Som3a.AI.csproj package OpenAI
dotnet add Som3a.AI/Som3a.AI.csproj package Anthropic
dotnet add Som3a.AI/Som3a.AI.csproj package System.Security.Cryptography.ProtectedData
dotnet add Som3a.AI.Tests/Som3a.AI.Tests.csproj reference Som3a.AI/Som3a.AI.csproj
dotnet add Som3a.AI.Tests/Som3a.AI.Tests.csproj package Moq
```

## 2. Build the Provider Layer

Create the provider adapter interface and all 6 provider implementations in `Som3a.AI/Providers/`. The `IAIProvider` interface is defined in [contracts](./contracts/IAIProvider.cs.md).

```csharp
// Example: Registering providers in CompositionRoot or factory
services.RegisterSingleton<IAIProvider, OpenAIProvider>("openai");
services.RegisterSingleton<IAIProvider, ClaudeProvider>("claude");
// ... remaining providers
```

## 3. Build the Orchestration Engine

Create `OrchestrationEngine` in `Som3a.AI/Orchestration/` implementing [IOrchestrationEngine](./contracts/IOrchestrationEngine.cs.md). The engine:

1. Resolves the prompt template from the registry
2. Builds context from domain entities
3. Routes to the primary provider (with failover chain)
4. Applies retry policy with exponential backoff
5. Validates structured output against schema
6. Records token usage

## 4. Build the Prompt Template Registry

Create `PromptTemplateRegistry` in `Som3a.AI/Prompts/` implementing [IPromptTemplateRegistry](./contracts/IPromptTemplateRegistry.cs.md). Templates are stored as JSON files in `%APPDATA%\Som3a\templates\`.

## 5. Build the Structured Parsers

Create parser implementations in `Som3a.AI/Parsers/` implementing [IStructuredParser\<T\>](./contracts/IStructuredParser.cs.md). Each parser validates AI output against the expected JSON Schema and produces domain entities.

## 6. Wire Up to WpfApp2

The Som3a.AI library is consumed by WpfApp2 via the Bridge layer. In `CompositionRoot.RegisterServices()`:

```csharp
// Register AI services
var aiAssembly = typeof(OrchestrationEngine).Assembly;
container.RegisterSingleton<IOrchestrationEngine, OrchestrationEngine>();
container.RegisterSingleton<IPromptTemplateRegistry, PromptTemplateRegistry>();
container.RegisterSingleton<ITokenTracker, TokenTracker>();

// Streaming events flow through EventBus to UI
```

## 7. Build & Verify

```powershell
dotnet build Som3a.AI/Som3a.AI.csproj
dotnet test Som3a.AI.Tests/Som3a.AI.Tests.csproj
```

## Project Layout Summary

```
Som3a.AI/
├── Providers/     → IAIProvider + 6 adapters + health checks
├── Orchestration/ → Engine, Router, Queue, Retry, Streaming, TokenTracker
├── Prompts/       → Registry, Template model, Validator, Budget estimator
├── Parsers/       → 5 structured parsers (Activity, WBS, Relationship, Duration, Review)
├── Configuration/ → ProviderConfig, EncryptionService
└── Som3a.AI.csproj

Som3a.AI.Tests/
├── Providers/     → Mock-based unit tests
├── Orchestration/ → Engine, router, retry, queue tests
├── Prompts/       → Registry, validator tests
├── Parsers/       → Known-good/bad JSON input tests
└── Som3a.AI.Tests.csproj
```
