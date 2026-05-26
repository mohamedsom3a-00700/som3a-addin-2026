# Feature Specification: AI Core Infrastructure

**Feature Branch**: `018-ai-core-infrastructure`

**Created**: 2026-05-26

**Status**: Draft

**Input**: User description: "Phase 18 — AI Core Infrastructure: Implement the full AI abstraction layer with provider adapters, orchestration engine, prompt governance system, structured output parsing, and streaming support."

## User Scenarios & Testing

### User Story 1 - Configure and Connect AI Providers (Priority: P1)

A planning engineer opens the Settings page and configures up to 6 AI providers (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex) with their API keys. The system verifies connectivity by sending a health check request to each configured provider and displays connection status (connected, failed, not configured). The engineer can designate a primary, secondary, and fallback provider for automatic failover.

**Why this priority**: Without provider connectivity, no AI features can function. This is the foundational capability that all subsequent AI features depend on.

**Independent Test**: Can be fully tested by configuring a single provider with valid and invalid API keys, verifying health check responses, and confirming connection status indicators update correctly.

**Acceptance Scenarios**:

1. **Given** the engineer is on the AI Provider settings page, **When** they enter a valid API key for a provider, **Then** the system performs a health check and displays "Connected" status with a green indicator
2. **Given** the engineer enters an invalid API key, **When** the health check runs, **Then** the system displays "Failed" with the error message and a red indicator
3. **Given** no provider is configured, **When** the engineer views the settings, **Then** all provider cards show "Not Configured" status
4. **Given** the engineer has configured at least 2 providers, **When** they set a primary/secondary/fallback order, **Then** the system persists this preference and uses it during orchestration

---

### User Story 2 - Run AI Prompt Orchestration (Priority: P1)

A planning engineer triggers an AI-powered operation (e.g., generate activities from BOQ). The orchestration engine builds the prompt context from domain entities, selects the appropriate prompt template, sends it to the primary provider, and returns the structured response. If the primary provider fails, the engine automatically falls back to the secondary provider. The engineer sees streaming progress updates and receives the final structured result.

**Why this priority**: This is the core value proposition — users interact with AI through the orchestration engine. It must be working before any AI-powered planning features can be built.

**Independent Test**: Can be tested by invoking the orchestration engine with a test prompt template and mock domain data, verifying it routes to the correct provider, handles fallback when provider fails, and returns structured output.

**Acceptance Scenarios**:

1. **Given** a configured primary provider, **When** a prompt is submitted to the orchestration engine, **Then** the engine builds context from domain entities, sends to the primary provider, and returns a structured response
2. **Given** the primary provider is unreachable, **When** a prompt is submitted, **Then** the engine falls back to the secondary provider and returns the response
3. **Given** all configured providers are unreachable, **When** a prompt is submitted, **Then** the engine returns a clear error message indicating no providers are available
4. **Given** a prompt execution in progress, **When** streaming is enabled, **Then** the engine emits incremental progress events to subscribers
5. **Given** a prompt execution completes, **When** tokens are consumed, **Then** the engine records token usage for the session

---

### User Story 3 - Manage Prompt Templates (Priority: P2)

Administrators manage system prompt templates (shared, read-only to all other users), organizing them into categories (BOQ, WBS, Logic, Productivity, Validation, Review). Planning engineers can create personal custom templates visible only to themselves. Administrators and engineers can view, create, and version templates within their respective scopes. Each template defines system prompts, user prompts, context requirements, and expected output schema.

**Why this priority**: Prompt templates govern AI behavior and output quality. Template management is needed before domain-specific AI features can use consistent, validated prompts.

**Independent Test**: Can be tested by creating a new template, assigning it to a category, defining a JSON Schema output requirement, and verifying the template is available for orchestration.

**Acceptance Scenarios**:

1. **Given** the administrator opens the template registry, **When** they view existing templates, **Then** templates are organized by category and each shows version, last modified date, and status
2. **Given** the administrator creates a new template, **When** they define system and user prompts with JSON Schema output, **Then** the template is validated for syntax correctness and saved
3. **Given** a template with output schema, **When** the orchestration engine uses it, **Then** the structured output parser validates the AI response against the schema

---

### User Story 4 - Parse Structured AI Output (Priority: P2)

The structured output parser receives raw AI responses and converts them into domain entities (Activity, WBSNode, Relationship, Duration, ValidationResult). The parser validates the response against the expected JSON Schema, reports parsing errors with location details, and returns clean domain objects ready for downstream processing.

**Why this priority**: Without structured parsing, AI output is just raw text. Parsers bridge the gap between AI-generated content and usable domain entities.

**Independent Test**: Can be tested by feeding known-good and known-bad JSON responses to each parser (Activity, WBS, Relationship, Duration, Review) and verifying correct entity creation or error reporting.

**Acceptance Scenarios**:

1. **Given** a valid AI response matching the expected JSON Schema, **When** the structured parser processes it, **Then** valid domain entities are created with all required fields populated
2. **Given** an AI response with missing required fields, **When** the parser processes it, **Then** the system reports specific validation errors with field locations
3. **Given** an AI response with invalid data types, **When** the parser processes it, **Then** the system reports type mismatch errors

---

### Edge Cases

- What happens when rate limits are exceeded for all configured providers? The engine should queue the request and retry when the rate limit window resets, notifying the user of the delay.
- How does the system handle partial stream failures? The engine buffers streamed content and retries from the last successful chunk, merging results.
- What happens when a prompt template references context fields that don't exist? The system should fail validation at template registration time, not at execution time.
- How does the engine handle concurrent prompt submissions? Requests are queued and processed sequentially per provider, with configurable concurrency limits.
- What happens when API key permissions change mid-session? The next health check or request to that provider detects the failure and marks the provider as unavailable, triggering fallback.

## Requirements

### Functional Requirements

- **FR-001**: System MUST support configuring 6 AI providers (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex) with encrypted API key storage
- **FR-002**: System MUST perform health checks against configured providers and report connection status
- **FR-003**: System MUST support designating primary, secondary, and fallback providers for automatic failover
- **FR-004**: Orchestration engine MUST build prompt context from domain entities before submission
- **FR-005**: Orchestration engine MUST enforce JSON Schema output structure on AI responses
- **FR-006**: System MUST implement retry handling with exponential backoff for transient failures
- **FR-007**: System MUST support streaming responses with incremental progress events
- **FR-008**: System MUST track token usage per provider, per session, and per execution
- **FR-009**: System MUST maintain an audit trail for all template changes (who, what, when) including version history and state transitions
- **FR-010**: System MUST log provider failures, parsing errors, and rate limit violations for troubleshooting
- **FR-011**: System MUST implement request queuing and rate limiting per provider
- **FR-012**: Prompt template registry MUST support categories: BOQ, WBS, Logic, Productivity, Validation, Review
- **FR-013**: Prompt templates MUST follow a three-state lifecycle: Draft (invisible to orchestration) → Published (available for selection) → Deprecated (cannot be selected for new prompts; existing executions remain valid)
- **FR-014**: System MUST validate prompt template syntax at registration time
- **FR-015**: System MUST separate system prompts from user prompts in all templates
- **FR-016**: System MUST enforce template authorization: administrators manage system templates (shared, read-only to others); engineers manage personal custom templates (private to them)
- **FR-017**: Structured output parsers MUST produce domain entities for: Activity, WBSNode, Relationship, Duration/Productivity, ValidationResult
- **FR-018**: System MUST validate all parsed output against the expected JSON Schema before returning to callers
- **FR-019**: System MUST report structured parsing errors with specific field-level details
- **FR-020**: Provider API keys MUST be encrypted at rest using platform security services
- **FR-021**: System MUST support context window budgeting to ensure prompts fit within provider token limits

### Key Entities

- **AIProvider**: Represents a configured AI service (OpenAI, Claude, DeepSeek, etc.) with endpoint, API key (encrypted), health status, rate limit configuration, and priority order for failover
- **PromptTemplate**: A versioned template with system prompt, user prompt, context requirements, output JSON Schema, category, ownership scope (system vs. personal), owner identifier, and version metadata
- **PromptExecution**: Represents a single AI prompt execution with input context, selected provider, token usage, duration, status, and structured output
- **TokenUsage**: Tracks tokens consumed per provider, per session, and per execution, with cost estimation
- **ProviderHealthStatus**: Current connectivity status (Connected, Failed, NotConfigured) per provider, last checked timestamp
- **StructuredOutput**: Typed domain entities (Activity, WBSNode, Relationship, Duration, ValidationResult) produced by the structured output parsers

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can configure any of the 6 AI providers within 2 minutes of entering the settings page
- **SC-002**: Provider health checks complete within 5 seconds for reachable providers
- **SC-003**: Provider failover occurs automatically within 10 seconds of primary provider failure
- **SC-004**: Structured output parsers produce valid domain entities from known-good AI responses in under 1 second
- **SC-005**: Prompt template validation catches 100% of syntax errors at registration time
- **SC-006**: Token usage tracking reports accurate counts within 5% of provider-reported values
- **SC-007**: Streaming responses begin delivering content within 2 seconds of prompt submission
- **SC-008**: Users can create and version prompt templates with no loss of previously published versions
- **SC-009**: All provider failures and parsing errors are logged and retrievable within 30 seconds of occurrence

## Clarifications

### Session 2026-05-26

- Q: Template management authorization → A: Administrators manage system templates (shared, read-only to others); engineers can create personal custom templates (private to them)
- Q: Template lifecycle states → A: Draft → Published → Deprecated (three states; draft templates are invisible to orchestration; deprecated templates cannot be selected for new prompts but existing executions remain valid)
- Q: Observability and audit requirements → A: Token tracking + template audit trail (version history, who changed what/when) + error logging (provider failures, parsing errors, rate limit events)

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Users have stable internet connectivity to reach configured AI provider endpoints
- API keys are managed and rotated by users through the settings interface; the system encrypts them at rest but does not manage key lifecycle
- Provider pricing and rate limits vary; the system uses user-provided rate limit configuration rather than hardcoded defaults
- The orchestration engine runs within the WPF process; long-running operations use async/await patterns to avoid UI blocking
- Prompt templates are stored as structured files (JSON/YAML) in a well-known directory within the application data folder
- The six providers specified (OpenAI, Claude, DeepSeek, GLM, Kimi, Codex) represent the complete set for v1; provider adapter interface is designed for future additions
- Streaming responses are consumed by UI components via EventBus events; the orchestration engine itself is streaming-agnostic
- This phase builds on Som3a.Domain entity types (Activity, WBSNode, Relationship, Duration, ValidationResult) from Phase 14 and Dynamic Settings registry from Phase 16
- AI provider SDKs and NuGet packages are added as dependencies in the Som3a.AI project
