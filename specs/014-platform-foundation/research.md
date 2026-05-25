# Research: Platform Foundation

**Feature**: Phase 14 — Platform Foundation
**Date**: 2026-05-25
**Status**: Complete

---

## 1. .NET Framework 4.8 ↔ .NET 8.0 Interop Bridge

### Decision

Use a **.NET Standard 2.0 bridge project** (`Som3a.Bridge`) containing shared interfaces, DTOs, and a diagnostics event facade. The WPF host (.NET Framework 4.8) and the new class libraries (.NET 8.0) both reference this bridge. For process-level communication where types cannot be shared directly, use **named pipes** or **stdin/stdout** with JSON serialization.

### Rationale

- .NET Standard 2.0 is the highest version consumable by both .NET Framework 4.8 and .NET 8.0.
- Shared contracts (interfaces, enums, simple DTOs) defined in the bridge avoid type duplication.
- Named pipes provide low-latency IPC on Windows without requiring network stack; suitable for same-machine communication between VSTO host and companion .NET 8.0 process.
- The graceful degradation requirement (FR-028) is satisfied by wrapping all bridge calls in try/catch with fallback to disabled-UI state.

### Alternatives Considered

| Alternative | Verdict |
|-------------|---------|
| .NET Framework 4.8 compatibility shim for .NET 8.0 | Rejected: .NET 8.0 cannot load Framework 4.8 assemblies directly. |
| gRPC between processes | Rejected: adds HTTP/2 dependency, overkill for same-machine IPC. |
| COM interop | Rejected: complex registration, fragile deployment. |
| Single process with both runtimes | Rejected: CLR hosting complexity, not recommended by Microsoft. |
| In-process side-by-side via AssemblyLoadContext | Rejected: .NET 8.0 cannot run inside .NET Framework 4.8 CLR. |

---

## 2. DPAPI Key Encryption on .NET 8.0

### Decision

Use `System.Security.Cryptography.ProtectedData.Protect()` and `Unprotect()` with `DataProtectionScope.CurrentUser`. Keys are stored as base64-encoded byte arrays in a JSON file at `AppData/Som3a/keys.json`.

### Rationale

- `ProtectedData` is available natively on Windows in .NET 8.0 via the `System.Security.Cryptography.ProtectedData` NuGet package.
- `CurrentUser` scope ensures keys can only be decrypted by the same Windows user who encrypted them — no master password needed.
- Base64 encoding allows safe JSON storage.
- Zero external dependencies — no Azure Key Vault, no Windows Credential Manager COM complexity.

### Alternatives Considered

| Alternative | Verdict |
|-------------|---------|
| Windows Credential Manager | Rejected: COM interop adds deployment complexity. |
| AES with master password | Rejected: UX friction per FR requirement (should work offline, no per-session prompt). |
| Plain text | Rejected: violates FR-027. |
| Azure Key Vault | Rejected: requires internet, Azure subscription — violates offline assumption. |

---

## 3. Plugin Isolation via AssemblyLoadContext

### Decision

Use `System.Runtime.Loader.AssemblyLoadContext` in .NET 8.0 to load each plugin assembly in its own context. Each `PluginSandbox` wraps an `AssemblyLoadContext` and provides exception containment, resource limiting, and unload capability.

### Rationale

- `AssemblyLoadContext` is the recommended .NET mechanism for plugin isolation — supports full unloading (via `Unload()`), separate dependency resolution per context.
- Exception isolation: plugin exceptions caught at the `PluginHost` boundary, plugin marked unhealthy, diagnostic logged (SC-007, FR-029).
- Each context can resolve its own NuGet dependencies independently.
- The existing WPF host (4.8) uses its own `AppDomain` isolation for its module system; the .NET 8.0 plugin system mirrors this pattern.

### Alternatives Considered

| Alternative | Verdict |
|-------------|---------|
| AppDomain isolation | Rejected: AppDomain is .NET Framework only; not available in .NET 8.0. |
| Single AssemblyLoadContext for all plugins | Rejected: no per-plugin isolation, no unload without affecting others. |
| Process-level isolation per plugin | Rejected: excessive resource overhead for 8+ plugins, startup cost prohibitive. |

---

## 4. System.Text.Json Circular Reference Handling

### Decision

Use `System.Text.Json` with `ReferenceHandler.Preserve` for serialization of domain entities that contain circular references (e.g., Activity ↔ WBSNode, Relationship ↔ Activity). For the interop bridge, serialize to a canonical JSON format that uses ID-based references instead of nested objects to avoid circular reference serialization overhead.

### Rationale

- `ReferenceHandler.Preserve` adds `$id` and `$ref` metadata to JSON, allowing round-trip serialization of object graphs with cycles.
- ID-based references in the bridge format (e.g., `Activity.WBSNodeId` instead of `Activity.WBSNode`) are simpler for interop and match the domain's natural identity model.
- System.Text.Json is built-in for .NET 8.0 — zero NuGet dependency.
- Performance target SC-001 (<1s for round-trip) is achievable with System.Text.Json's source generator in AOT mode.

### Alternatives Considered

| Alternative | Verdict |
|-------------|---------|
| Newtonsoft.Json (Json.NET) | Rejected: adds dependency, slower than System.Text.Json for this use case. |
| Custom binary serialization | Rejected: not interoperable for bridge communication. |
| Flattened DTOs only (no circular refs) | Partially adopted: DTOs for bridge use ID refs; domain entities use ReferenceHandler.Preserve. |

---

## 5. AI Provider SDK Selection

### Decision

| Provider | SDK / Package | Notes |
|----------|--------------|-------|
| OpenAI | `OpenAI` NuGet (>=2.0.0) | Official .NET SDK, supports GPT-4, GPT-4o, streaming, JSON mode |
| Claude | `Anthropic.SDK` NuGet | Community-maintained, supports Claude 3, streaming |
| DeepSeek | OpenAI-compatible HTTP client | DeepSeek API is OpenAI-compatible; reuse OpenAI client with custom base URL |
| GLM | Custom HTTP client | GLM API uses its own format; lightweight HttpClient wrapper |
| Kimi | OpenAI-compatible HTTP client | Moonshot/Kimi API is OpenAI-compatible |
| Codex | `OpenAI` NuGet (same) | Codex is an OpenAI model; same SDK |

### Rationale

- OpenAI and Anthropic have mature .NET SDKs.
- DeepSeek and Kimi expose OpenAI-compatible APIs — the same client library works with a different `BaseUrl`.
- GLM requires a custom lightweight wrapper (standard REST/JSON).
- All providers implement `IAIProvider` — orchestration layer routes without knowing provider specifics.

### Alternatives Considered

| Alternative | Verdict |
|-------------|---------|
| LangChain.NET | Rejected: heavy dependency for orchestration we already handle via AIOrchestrator. |
| Semantic Kernel (Microsoft) | Rejected: adds complexity; our domain-specific orchestration is simpler. |
| Direct HttpClient for all providers | Rejected: loses SDK benefits (streaming, token counting, retry). |

---

## Summary of Key Technical Decisions

| # | Decision | Rationale Summary |
|---|----------|-------------------|
| 1 | .NET Standard 2.0 bridge for shared types | Compatible with both .NET Framework 4.8 and .NET 8.0 |
| 2 | Named pipes for process-level IPC | Low-latency, Windows-native, no network dependency |
| 3 | DPAPI via ProtectedData for API key encryption | Native Windows, zero config, user-scoped |
| 4 | AssemblyLoadContext for plugin isolation | Native .NET 8.0 isolation, supports unload |
| 5 | System.Text.Json with ReferenceHandler.Preserve | Built-in, fast, handles cycles |
| 6 | ID-based references for bridge DTOs | Simplifies interop, avoids cycle issues |
| 7 | OpenAI SDK for OpenAI/DeepSeek/Kimi/Codex | OpenAI-compatible APIs reuse same client |
| 8 | Anthropic.SDK for Claude | Community-maintained, supports Claude 3 |
| 9 | Custom HttpClient for GLM | GLM uses proprietary API format |
