# Research: AI Core Infrastructure

**Feature**: AI Core Infrastructure (Phase 18)
**Date**: 2026-05-26

## Decisions

### 1. Provider SDK Strategy

**Decision**: Use official SDKs for OpenAI and Anthropic (Claude); use direct REST HTTP clients for DeepSeek, GLM, Kimi, Codex

- **Rationale**: OpenAI and Anthropic provide mature, maintained .NET SDKs. The other four providers either lack official .NET SDKs or their SDKs are community-maintained with low adoption. REST clients provide consistent interface with less dependency risk.
- **Alternatives considered**:
  - Using community SDKs for all 6 — introduces inconsistent API patterns and dependency maintenance burden
  - Using only REST for all 6 — loses SDK-specific optimizations (retry, streaming helpers) for OpenAI/Anthropic
  - Wrapping all behind a generic HTTP client — over-abstracted; SDK benefits outweigh consistency

### 2. API Key Encryption

**Decision**: Use Windows Data Protection API (DPAPI) via `System.Security.Cryptography.ProtectedData`

- **Rationale**: DPAPI is the standard Windows encryption mechanism for desktop applications. It encrypts data tied to the current Windows user account, requires no additional NuGet packages, and is FIPS-compliant. Keys are stored in the Dynamic Settings registry (Phase 16) as encrypted base64 strings.
- **Alternatives considered**:
  - AES with a managed key (DPAPI is simpler and more secure for single-user desktop)
  - Azure Key Vault (overkill for desktop app; requires network and Azure subscription)
  - Plaintext with machine-scoped ACL (insecure)

### 3. JSON Schema Validation

**Decision**: Use `System.Text.Json` with manual schema validation helpers

- **Rationale**: `System.Text.Json` is included in .NET 8.0 with no external dependencies. JSON Schema libraries (JsonSchema.Net, NJsonSchema) add dependency risk. The prompt output schemas are well-defined and limited in scope (~5 schemas), so manual validation with `Utf8JsonReader`/`JsonDocument` is maintainable and faster.
- **Alternatives considered**:
  - JsonSchema.Net NuGet package (more features but external dependency)
  - Newtonsoft.Json with JsonSchema (legacy; not recommended for .NET 8.0)
  - Custom validation with reflection (over-engineered for 5 schemas)

### 4. Template Storage Format

**Decision**: Store prompt templates as JSON files in `%APPDATA%\Som3a\templates\`

- **Rationale**: JSON is human-readable, easy to version control individually, supports schema evolution, and integrates naturally with `System.Text.Json`. File-per-template approach allows independent versioning, addition, and deletion without a database.
- **Alternatives considered**:
  - Single JSON file registry (merge conflicts on concurrent edits; harder to version independently)
  - SQLite database (overkill for template storage; adds dependency)
  - YAML files (equivalent tradeoffs; JSON preferred for tooling interoperability)

### 5. Testing Strategy

**Decision**: Standard xUnit + Moq for unit tests; no integration tests against real providers in CI

- **Rationale**: xUnit is the established project testing framework. Moq provides interface mocking for provider adapters. Real provider integration tests are excluded from CI to avoid network dependency, API key exposure, and cost — run manually on-demand.
- **Alternatives considered**:
  - NUnit (less common in modern .NET projects)
  - Integration tests with real API keys in CI (security risk; cost exposure)
  - WireMock for HTTP-level provider mocking (overkill; Moq interfaces are sufficient)

### 6. Streaming Pattern

**Decision**: Use `IAsyncEnumerable<T>` for streaming responses; consumed via EventBus events

- **Rationale**: `IAsyncEnumerable<T>` is the idiomatic .NET streaming pattern, supporting cancellation, `await foreach`, and composition. The orchestration engine exposes `IAsyncEnumerable<StreamChunk>` which downstream EventBus subscribers consume and relay as UI events.
- **Alternatives considered**:
  - Reactive Extensions (Rx.NET) — powerful but adds a dependency for a simple use case
  - Callback/delegate pattern — harder to compose and cancel
  - `Channel<T>` — useful for producer-consumer but overkill for single-consumer streaming

### 7. Rate Limiting Strategy

**Decision**: Token-bucket algorithm per provider, user-configurable RPM

- **Rationale**: Token-bucket is the standard rate limiting algorithm, allowing burst handling while enforcing average rate limits. User configuration via Dynamic Settings (Phase 16) enables per-provider RPM adjustment without code changes.
- **Alternatives considered**:
  - Fixed-window counter (simple but allows burst spikes at window boundaries)
  - Sliding-window log (more accurate but higher memory overhead)
  - Leaky-bucket (similar to token-bucket but less flexible for bursts)

### 8. Provider Failover Logic

**Decision**: Circuit-breaker pattern with configurable failure thresholds

- **Rationale**: After N consecutive failures (configurable, default 3), the provider is marked as degraded and skipped for a cooldown period (configurable, default 60s). This prevents hammering a failing provider and enables faster fallback. Status transitions: Healthy → Degraded (after N failures) → Healthy (after cooldown + successful health check).
- **Alternatives considered**:
  - Immediate failover on first failure (too aggressive; transient failures cause unnecessary fallback)
  - Always try primary first even if degraded (wastes latency; constrains failover purpose)
  - Round-robin across all providers (no primary preference; inconsistent for cost optimization)

## Dependencies

| Dependency | Version | Purpose |
|------------|---------|---------|
| OpenAI .NET SDK | latest stable | OpenAI provider adapter |
| Anthropic .NET SDK | latest stable | Claude provider adapter |
| Som3a.Domain | Phase 14 | Domain entities (Activity, WBSNode, etc.) |
| Dynamic Settings | Phase 16 | Provider config & rate limits |
| ServiceContainer | Existing | DI for provider registration |
| EventBus | Existing | Streaming progress events |
| Som3a.Diagnostics | Phase 14 | Error logging |
| System.Text.Json | .NET 8.0 built-in | JSON Schema validation, template serialization |

## Best Practices

- All provider network calls wrapped in `CancellationToken` — user can cancel long-running operations
- Provider adapters registered as singletons in ServiceContainer (reuse HTTP clients)
- Template registry uses file watcher (`FileSystemWatcher`) for live template updates (optional, Phase 19+)
- Structured parser errors include original JSON fragment and schema path for debugging
- Token usage flushed to Diagnostics service on session end, not per-request (minimize I/O)
