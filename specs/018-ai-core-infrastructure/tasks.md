# Tasks: AI Core Infrastructure

**Input**: Design documents from `specs/018-ai-core-infrastructure/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not requested in spec — no explicit test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Projects at repository root: `Som3a.AI/`, `Som3a.AI.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Som3a.AI .NET 8.0 class library project exists at Som3a.AI/Som3a.AI.csproj
- [x] T002 [P] Som3a.AI.Tests project - deferred (not in initial scope)
- [x] T003 NuGet packages present: OpenAI SDK, Anthropic SDK, System.Text.Json
- [x] T004 [P] Project reference Som3a.Domain exists
- [x] T005 Folder structure created: Providers/, Orchestration/, Prompts/, Parsers/, Configuration/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and models that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 [P] IAIProvider interface exists in Som3a.Contracts (with HealthCheckAsync, ExecutePromptAsync, StreamPromptAsync)
- [x] T007 [P] ProviderConfig and ProviderHealthStatus created in Som3a.AI/Configuration/ProviderConfig.cs
- [x] T008 [P] EncryptionService (DPAPI) created in Som3a.AI/Configuration/EncryptionService.cs
- [x] T009 [P] PromptTemplate model exists in Som3a.Contracts (enhanced with lifecycle state)
- [x] T010 [P] PromptExecution model with status tracking created in Som3a.AI/Orchestration/PromptExecutionContext.cs
- [x] T011 [P] TokenUsage record exists in Som3a.Contracts; TokenTracker in Som3a.AI/Tracking/TokenTracker.cs
- [x] T012 [P] BaseStructuredParser abstract class with JSON Schema validation created in Som3a.AI/Parsers/BaseStructuredParser.cs

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Configure and Connect AI Providers (Priority: P1) 🎯 MVP

**Goal**: A planning engineer can configure up to 6 AI providers with API keys, verify connectivity via health checks, and designate provider failover priorities

**Independent Test**: Configure a single provider with valid and invalid API keys, verify health check responses, and confirm connection status indicators update correctly

### Implementation for User Story 1

- [x] T013 [P] [US1] OpenAIProvider implemented in Som3a.AI/Providers/OpenAIProvider.cs
- [x] T014 [P] [US1] ClaudeProvider implemented in Som3a.AI/Providers/ClaudeProvider.cs
- [x] T015 [P] [US1] DeepSeekProvider (REST HTTP client) in Som3a.AI/Providers/DeepSeekProvider.cs
- [x] T016 [P] [US1] GLMProvider implemented in Som3a.AI/Providers/GLMProvider.cs
- [x] T017 [P] [US1] KimiProvider implemented in Som3a.AI/Providers/KimiProvider.cs
- [x] T018 [P] [US1] CodexProvider implemented in Som3a.AI/Providers/CodexProvider.cs
- [x] T019 [US1] Health check integration via AIProviderBase.HealthCheckAsync() and AIOrchestrator availability aggregation

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Run AI Prompt Orchestration (Priority: P1)

**Goal**: A planning engineer triggers an AI operation; the orchestration engine builds context, selects provider via failover routing, manages retry/streaming, and returns structured output

**Independent Test**: Invoke the orchestration engine with a test prompt template and mock domain data, verify it routes to the correct provider, handles fallback when provider fails, and returns structured output

### Implementation for User Story 2

- [x] T020 [P] [US2] ProviderRouter with degradation tracking in Som3a.AI/Orchestration/ProviderRouter.cs
- [x] T021 [P] [US2] RetryHandler with exponential backoff in Som3a.AI/Orchestration/RetryHandler.cs
- [x] T022 [P] [US2] RequestQueue with token-bucket rate limiting in Som3a.AI/Orchestration/RequestQueue.cs
- [x] T023 [P] [US2] StreamingHandler for IAsyncEnumerable streaming in Som3a.AI/Orchestration/StreamingHandler.cs
- [x] T024 [P] [US2] TokenTracker for per-provider usage in Som3a.AI/Tracking/TokenTracker.cs
- [x] T025 [US2] OrchestrationEngine — context building, provider routing, retry, streaming, output in Som3a.AI/Orchestration/OrchestrationEngine.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Manage Prompt Templates (Priority: P2)

**Goal**: Administrators manage system prompt templates (shared, read-only); engineers create personal custom templates; both follow Draft → Published → Deprecated lifecycle with versioning

**Independent Test**: Create a new template, assign it to a category, define a JSON Schema output requirement, and verify the template is available for orchestration

### Implementation for User Story 3

- [x] T026 [P] [US3] PromptTemplateRegistry with file-based storage in Som3a.AI/Prompts/PromptTemplateRegistry.cs
- [x] T027 [P] [US3] TemplateValidator in Som3a.AI/Prompts/TemplateValidator.cs
- [x] T028 [US3] ContextBudgetEstimator in Som3a.AI/Prompts/ContextBudgetEstimator.cs
- [x] T029 [US3] AuditTrail for template changes in Som3a.AI/Prompts/AuditTrail.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Parse Structured AI Output (Priority: P2)

**Goal**: AI responses are validated against JSON Schema and parsed into typed domain entities (Activity, WBSNode, Relationship, Duration, ValidationResult)

**Independent Test**: Feed known-good and known-bad JSON responses to each parser and verify correct entity creation or error reporting

### Implementation for User Story 4

- [x] T030 [P] [US4] ActivityParser in Som3a.AI/Parsers/ActivityParser.cs
- [x] T031 [P] [US4] WBSParser in Som3a.AI/Parsers/WBSParser.cs
- [x] T032 [P] [US4] RelationshipParser in Som3a.AI/Parsers/RelationshipParser.cs
- [x] T033 [P] [US4] DurationParser in Som3a.AI/Parsers/DurationParser.cs
- [x] T034 [P] [US4] ReviewParser in Som3a.AI/Parsers/ReviewParser.cs

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T035 [P] Error logging wiring via ServiceRegistration (Som3a.AI/ServiceRegistration.cs)
- [x] T036 AI services DI registration helper in Som3a.AI/ServiceRegistration.cs
- [x] T037 [P] AGENTS.md updated to Phase 18
- [x] T038 Constitution compliance review — all checks pass (no UI in this layer)
- [x] T039 Build verification — run dotnet build

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion — can be implemented independently
- **User Story 2 (Phase 4)**: Depends on Foundational completion — can use mocks for providers, templates, parsers; real integration with US1/US3/US4 occurs in Polish phase
- **User Story 3 (Phase 5)**: Depends on Foundational completion — no dependency on US1/US2
- **User Story 4 (Phase 6)**: Depends on Foundational completion — no dependency on US1/US2/US3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories — **MVP scope**
- **User Story 2 (P1)**: Can start after Foundational — Interfaces for providers (IAIProvider), templates (PromptTemplate), and parsers (BaseStructuredParser) exist from Foundational phase; concrete implementations can be mocked
- **User Story 3 (P2)**: Can start after Foundational — No dependencies on US1 or US2
- **User Story 4 (P2)**: Can start after Foundational — No dependencies on US1, US2, or US3

### Within Each User Story

- Models before services
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- All 6 provider implementations (T013-T018) can run in parallel
- All US2 components (T020-T024) can run in parallel before OrchestrationEngine (T025)
- All US3 components (T026-T027) can run in parallel
- All US4 parsers (T030-T034) can run in parallel

---

## Parallel Execution Examples

### Phase 3: User Story 1 (All 6 providers in parallel)

```bash
# Implement all provider adapters simultaneously (no cross-dependencies):
Task: "Implement OpenAIProvider in Som3a.AI/Providers/OpenAIProvider.cs"
Task: "Implement ClaudeProvider in Som3a.AI/Providers/ClaudeProvider.cs"
Task: "Implement DeepSeekProvider in Som3a.AI/Providers/DeepSeekProvider.cs"
Task: "Implement GLMProvider in Som3a.AI/Providers/GLMProvider.cs"
Task: "Implement KimiProvider in Som3a.AI/Providers/KimiProvider.cs"
Task: "Implement CodexProvider in Som3a.AI/Providers/CodexProvider.cs"
```

### Phase 4: User Story 2 (All components in parallel before engine)

```bash
# Implement all orchestration components simultaneously:
Task: "Implement ProviderRouter in Som3a.AI/Orchestration/ProviderRouter.cs"
Task: "Implement RetryPolicy in Som3a.AI/Orchestration/RetryPolicy.cs"
Task: "Implement RequestQueue in Som3a.AI/Orchestration/RequestQueue.cs"
Task: "Implement StreamingHandler in Som3a.AI/Orchestration/StreamingHandler.cs"
Task: "Implement TokenTracker in Som3a.AI/Orchestration/TokenTracker.cs"
```

### Phase 6: User Story 4 (All 5 parsers in parallel)

```bash
# Implement all structured parsers simultaneously:
Task: "Implement ActivityParser in Som3a.AI/Parsers/ActivityParser.cs"
Task: "Implement WBSParser in Som3a.AI/Parsers/WBSParser.cs"
Task: "Implement RelationshipParser in Som3a.AI/Parsers/RelationshipParser.cs"
Task: "Implement DurationParser in Som3a.AI/Parsers/DurationParser.cs"
Task: "Implement ReviewParser in Som3a.AI/Parsers/ReviewParser.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (all 6 provider adapters + health checks)
4. **STOP and VALIDATE**: Test User Story 1 independently — configure a provider, run health check, verify status
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently (with mocks) → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (all 6 providers in parallel)
   - Developer B: User Story 2 (all components in parallel, then engine)
   - Developer C: User Story 3 + User Story 4 (templates and parsers)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability (US1-US4)
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Test tasks are excluded per spec (no explicit TDD/test requirement in feature specification)
