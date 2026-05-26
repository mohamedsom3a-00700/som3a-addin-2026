# Data Model: AI Core Infrastructure

## Entity: AIProvider

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Provider key (openai, claude, deepseek, glm, kimi, codex) |
| DisplayName | string | Human-readable name |
| Endpoint | Uri | Base API URL |
| ApiKeyEncrypted | string | DPAPI-encrypted API key (empty if not configured) |
| HealthStatus | ProviderHealthStatus | Current connectivity |
| LastHealthCheckAt | DateTime? | Last health check timestamp |
| RateLimitRpm | int | Max requests per minute (user-configurable) |
| PriorityOrder | int | Failover priority (1=primary, 2=secondary, 3=fallback) |
| IsEnabled | bool | Whether provider is available for orchestration |

**Validation Rules**:
- Id must be one of the 6 supported provider keys
- RateLimitRpm must be > 0 and ≤ provider max
- ApiKeyEncrypted must be valid base64 when non-empty

**State** (ProviderHealthStatus):
- NotConfigured (default, no API key set)
- Connected (health check passed)
- Failed (health check failed)
- Degraded (circuit-breaker threshold reached)

---

## Entity: PromptTemplate

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique template identifier (GUID) |
| Name | string | Short display name |
| Category | PromptCategory | BOQ, WBS, Logic, Productivity, Validation, Review |
| SystemPrompt | string | System-level instructions sent to AI |
| UserPrompt | string | User-facing prompt template (may contain {{placeholders}}) |
| OutputSchema | JsonDocument | JSON Schema required for structured output |
| ContextRequirements | string[] | Domain entity types required for context building |
| LifecycleState | TemplateLifecycleState | Draft, Published, Deprecated |
| Version | int | Monotonically increasing version number |
| OwnershipScope | TemplateOwnership | System or Personal |
| OwnerId | string? | User identifier (null for system templates) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |
| DeprecatedAt | DateTime? | When deprecated (null if not deprecated) |

**Validation Rules**:
- Name must be unique within scope (system or per-owner)
- SystemPrompt must be non-empty
- OutputSchema must be valid JSON Schema
- ContextRequirements must reference valid domain entity types
- Draft templates cannot be used by orchestration engine
- Deprecated templates cannot be selected for new prompts

**State Transitions** (TemplateLifecycleState):
```
Draft ──┬──→ Published (via publication action)
         └──→ [deleted] (author only, deletes permanently)

Published ──→ Deprecated (via deprecation action)
           └──→ Draft (via unpublish action, reverts to draft)

Deprecated ──→ [retained indefinitely for audit]
```

---

## Entity: PromptExecution

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique execution identifier (GUID) |
| TemplateId | string | The PromptTemplate used |
| ProviderId | string | Which provider was invoked |
| ContextSnapshot | JsonDocument | Domain entity context at execution time |
| Status | ExecutionStatus | Queued, Running, Completed, Failed, Cancelled |
| StartedAt | DateTime | Execution start time |
| CompletedAt | DateTime? | Execution end time |
| DurationMs | long? | Total execution duration |
| TokensPrompt | int? | Prompt tokens consumed |
| TokensCompletion | int? | Completion tokens consumed |
| TokensTotal | int? | Total tokens consumed |
| RawResponse | string? | Raw AI response (for debugging, truncated) |
| StructuredOutput | JsonDocument? | Parsed structured output (null if failed) |
| ErrorMessage | string? | Error details if failed |
| RetryCount | int | Number of retries attempted |
| FallbackChain | string[] | Providers tried in order (primary → secondary → ...) |

**State** (ExecutionStatus):
```
Queued → Running → Completed
                → Failed (after retries exhausted)
                → Cancelled (user or timeout)
```

---

## Entity: StructuredOutput

| Type | Produced By | Domain Entity |
|------|-------------|---------------|
| ActivityList | ActivityParser | List<Activity> (from Som3a.Domain) |
| WBSTree | WBSParser | WBSNode hierarchy (from Som3a.Domain) |
| RelationshipList | RelationshipParser | List<Relationship> (from Som3a.Domain) |
| DurationEstimate | DurationParser | Duration + ProductivityData (from Som3a.Domain) |
| ValidationResult | ReviewParser | ValidationResult + Recommendations (from Som3a.Domain) |

---

## Entity: TokenUsage

| Field | Type | Description |
|-------|------|-------------|
| ProviderId | string | Provider this usage belongs to |
| SessionId | string | Application session identifier |
| Period | TokenPeriod | Current session or rolling window |
| TokensPromptTotal | long | Cumulative prompt tokens |
| TokensCompletionTotal | long | Cumulative completion tokens |
| TokensGrandTotal | long | Cumulative total |
| EstimatedCost | decimal | Estimated cost (if rate info available) |
| LastUpdatedAt | DateTime | Last update timestamp |

---

## Entity: AuditEntry

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique identifier (GUID) |
| EntityType | string | "PromptTemplate" or "AIProvider" |
| EntityId | string | ID of the entity changed |
| Action | string | Created, Updated, Deprecated, Deleted, StateTransition |
| ActorId | string | User who performed the action |
| Timestamp | DateTime | When the action occurred |
| PreviousState | JsonDocument? | Snapshot before change |
| NewState | JsonDocument? | Snapshot after change |
