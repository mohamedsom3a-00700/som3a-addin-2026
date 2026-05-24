# Tasks: MVVM & Architecture Cleanup

**Input**: Design documents from `/specs/009-mvvm-architecture-cleanup/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Unit tests for the service container, event bus, and ViewModel base class are required to validate correctness. Acceptance is verified through passing tests AND build validation AND manual Excel host testing per the project's testing strategy.

## Test Tasks

- [ ] T041 [US1] Write unit test for `ServiceContainer` — verify singleton returns same instance, transient returns different instances, scoped returns same instance within a scope and different across scopes
- [ ] T042 [US1] [P] Write unit test for `ServiceContainer` — verify circular dependency throws `InvalidOperationException` with type chain
- [ ] T043 [US1] [P] Write unit test for `ServiceContainer` — verify unregistered service throws `InvalidOperationException` with type name
- [ ] T044 [US1] [P] Write unit test for `ServiceContainer` — verify `ServiceResolved` and `ServiceRegistered` events fire correctly
- [ ] T045 [US2] Write unit test for `EventBus` — verify publish delivers event to matching subscribers
- [ ] T046 [US2] [P] Write unit test for `EventBus` — verify weak references allow subscriber GC and auto-pruning on next publish
- [ ] T047 [US2] [P] Write unit test for `EventBus` — verify subscriber isolation (one exception does not block others)
- [ ] T048 [US2] [P] Write unit test for `EventBus` — verify optional filter predicate works
- [ ] T049 [US3] Write unit test for `ViewModelBase` — verify `SetProperty` raises property changed only when value differs

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF Desktop Add-in**: `WpfApp2/` is the primary project directory
- Infrastructure services go in `WpfApp2/Services/`
- ViewModels go in `WpfApp2/ViewModels/`
- Composition root is `WpfApp2/App.xaml.cs`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for architecture cleanup

- [X] T001 Create `WpfApp2/ViewModels/` directory for centralized ViewModel location
- [X] T002 [P] Create `Services/` directory structure confirmation — verify existing `WpfApp2/Services/` exists
- [X] T003 [P] Verify `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` compiles before any changes

---

## Phase 2: Foundational — Service Container (Priority: P1)

**Purpose**: Core dependency injection infrastructure that MUST be complete before event bus or MVVM cleanup can begin. This phase delivers User Story 1 (register and resolve services).

**⚠️ CRITICAL**: No user story work can begin until this phase is complete. This IS User Story 1.

**Goal**: Developers can register and resolve services with singleton, transient, and scoped lifetimes through a central service container.

**Independent Test**: Register a test service as singleton, resolve it in two different places, verify same instance returned. Register as transient, resolve twice, verify different instances. Register with dependencies, verify auto-resolution.

### Implementation for Foundational (User Story 1)

- [X] T004 [US1] Define `IServiceContainer` interface in `WpfApp2/Services/ServiceContainer.cs` with registration (`RegisterSingleton`, `RegisterTransient`, `RegisterScoped`) and resolution (`Resolve<T>`, `Resolve(Type)`) methods
- [X] T005 [US1] Define `IServiceScope` interface with `Resolve<T>()` and `IDisposable` in `WpfApp2/Services/ServiceContainer.cs`
- [X] T006 [US1] Implement `ServiceContainer` class with singleton/transient/scoped lifetime management in `WpfApp2/Services/ServiceContainer.cs`
- [X] T007 [US1] [P] Implement circular dependency detection during resolution — throw descriptive `InvalidOperationException` identifying the chain in `WpfApp2/Services/ServiceContainer.cs`
- [X] T008 [US1] [P] Implement unregistered service detection — throw descriptive `InvalidOperationException` identifying the missing type in `WpfApp2/Services/ServiceContainer.cs`
- [X] T009 [US1] [P] Implement `ServiceResolved` and `ServiceRegistered` observable events on `IServiceContainer` for downstream diagnostics in `WpfApp2/Services/ServiceContainer.cs`
- [X] T010 [US1] Implement `ServiceScope` class with scoped resolution support in `WpfApp2/Services/ServiceContainer.cs`
- [X] T011 [US1] Create composition root wiring in `WpfApp2/App.xaml.cs` — instantiate `ServiceContainer` as application-wide singleton and register core services
- [X] T012 [US1] Migrate existing `ThemeManager.Instance` singleton access to use `IServiceContainer` resolution in composition root — register ThemeManager as singleton service

**Checkpoint**: Service container is functional. Services can be registered, resolved with all three lifetimes, and errors for circular/missing dependencies are clear. Build passes.

---

## Phase 3: User Story 2 - Event Bus (Priority: P2)

**Goal**: Components can communicate without direct references by publishing and subscribing to typed events through a central event bus.

**Independent Test**: Publish an event from one component and verify a different, unrelated component receives it. Verify the publisher has no reference to the subscriber. Verify subscriber exception doesn't block other subscribers. Verify dead subscribers are auto-pruned.

### Implementation for User Story 2

- [X] T013 [P] [US2] Define `IEventBus` interface in `WpfApp2/Services/EventBus.cs` with `Publish<TEvent>`, `Subscribe<TEvent>`, and `Unsubscribe` methods
- [X] T014 [P] [US2] Define `SubscriptionToken` class implementing `IDisposable` for automatic unsubscription in `WpfApp2/Services/EventBus.cs`
- [X] T015 [US2] Implement `EventBus` class with typed event routing in `WpfApp2/Services/EventBus.cs` — subscribers receive only events of their subscribed type
- [X] T016 [US2] [P] Implement weak reference subscriber storage — dead subscribers are auto-pruned on next publish without memory leaks in `WpfApp2/Services/EventBus.cs`
- [X] T017 [US2] [P] Implement subscriber isolation — one subscriber's exception does not prevent others from receiving the event; errors reported via `SubscriberError` event in `WpfApp2/Services/EventBus.cs`
- [X] T018 [US2] [P] Implement optional per-subscriber event filtering via `Func<TEvent, bool>` predicate in `WpfApp2/Services/EventBus.cs`
- [X] T019 [US2] [P] Implement `EventPublished`, `EventSubscribed`, and `SubscriberError` observable events on `IEventBus` for downstream diagnostics in `WpfApp2/Services/EventBus.cs`
- [X] T020 [US2] Register `EventBus` as singleton in composition root (`WpfApp2/App.xaml.cs`)
- [X] T021 [US2] Create `IModule` interface with `ModuleId`, `Name`, `Priority`, and `Initialize(IServiceContainer, IEventBus)` in `WpfApp2/Services/ModuleRegistry.cs`
- [X] T022 [US2] Create `IModuleRegistry` interface with `RegisterModule`, `InitializeAll`, `GetRegisteredModules`, and observable events in `WpfApp2/Services/ModuleRegistry.cs`
- [X] T023 [US2] Implement `ModuleRegistry` class with priority-based initialization order and duplicate detection in `WpfApp2/Services/ModuleRegistry.cs`
- [X] T024 [US2] Register `ModuleRegistry` as singleton in composition root (`WpfApp2/App.xaml.cs`) and call `InitializeAll()` during application startup

**Checkpoint**: Event bus and module registry are functional. Components can publish/subscribe with weak references, error isolation, and typed events. Modules can register services and subscriptions at startup. Build passes.

---

## Phase 4: User Story 3 - MVVM Cleanup (Priority: P3)

**Goal**: View code-behind files contain no business logic — all interaction handling, data processing, and application rules are in ViewModels that receive dependencies through the service container.

**Independent Test**: Audit each view file against checklist: no data access, no service calls, no business rules, no complex conditional logic in code-behind. Verify all ViewModels receive dependencies through constructor injection from the service container.

### Implementation for User Story 3

- [X] T025 [US3] Create base ViewModel class with `INotifyPropertyChanged` implementation in `WpfApp2/ViewModels/ViewModelBase.cs`
- [X] T026 [US3] [P] Audit all existing code-behind files against MVVM compliance checklist — identify files with business logic, data access, service instantiation, or complex conditionals
- [X] T027 [US3] [P] Refactor identified code-behind files — extract business logic to ViewModels in `WpfApp2/ViewModels/` and wire dependencies via constructor injection
- [X] T028 [US3] [P] Relocate any existing ViewModels scattered across the project into `WpfApp2/ViewModels/` with namespace updates
- [X] T029 [US3] [P] Replace direct service instantiation (`new ServiceX()`) in existing ViewModels with constructor injection from `IServiceContainer`
- [X] T030 [US3] [P] Replace code-behind event handlers with Command bindings where feasible — wire `ICommand` properties in ViewModels to XAML `Command` bindings
- [X] T031 [US3] Create MVVM compliance checklist document in `Docs/Architecture/MVVM_COMPLIANCE.md` with audit criteria and pass/fail rules
- [X] T032 [US3] Create `CompositionRoot.cs` in `WpfApp2/` — centralize all service and module registrations, called from `App.xaml.cs`

**Checkpoint**: All code-behind files pass MVVM compliance audit (no business logic). ViewModels receive dependencies via constructor injection. Build passes.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T033 [P] Extract theme-related services (`ThemeManager`, `WindowRenderModeDetector`, `ThemeSettings`) into `IServiceContainer` registration in composition root
- [X] T034 [P] Extract navigation service (`NavigationService` from Phase 5) into `IServiceContainer` registration in composition root
- [X] T035 [P] Ensure all new and refactored services follow the research pattern — constructor injection only, no service locator anti-pattern usage in `WpfApp2/Services/`
- [X] T036 [P] Verify no `new ServiceX()` instantiation remains in ViewModels or code-behind across the entire `WpfApp2/` project
- [X] T037 Run `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — build must succeed with zero errors
- [X] T038 Update `AGENTS.md` with new architecture patterns — service container, event bus, module registry, ViewModel conventions
- [X] T039 Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration, Excel rendering safety, and WindowChrome inheritance remain intact after refactoring
- [X] T040 Update `Docs/Architecture/MVVM_RULES.md` with concrete service container, event bus, and module registry conventions from this phase

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational / US1 (Phase 2)**: Depends on Setup completion — BLOCKS all subsequent phases
- **US2 - Event Bus (Phase 3)**: Depends on US1 completion — ServiceContainer is required for EventBus registration at startup
- **US3 - MVVM Cleanup (Phase 4)**: Depends on US1 and US2 completion — ViewModels need ServiceContainer for DI and EventBus for decoupled communication
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundational — the service container is the prerequisite for all other stories
- **User Story 2 (P2)**: Depends on US1 — EventBus registers as a service in the container; ModuleRegistry depends on both
- **User Story 3 (P3)**: Depends on US1 + US2 — MVVM refactoring requires ServiceContainer for DI and EventBus for decoupled patterns

### Within Each Phase

- Container interface before implementation
- Core implementation before observable events
- Registration in composition root after implementation is complete
- Build validation after each phase

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- Within US1: interface definition (T004, T005) before implementation (T006-T010), but T007/T008/T009 can run in parallel once the implementation structure exists
- Within US2: interface definitions (T013, T014) before implementation (T015-T019), but T016/T017/T018/T019 can run in parallel
- Within US3: code-behind audit (T026) must precede refactoring tasks (T027-T030), but individual file refactors can be parallelized across files
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1 (Foundational)

```text
# Launch all interface definitions together:
Task: "Define IServiceContainer interface in WpfApp2/Services/ServiceContainer.cs"
Task: "Define IServiceScope interface in WpfApp2/Services/ServiceContainer.cs"

# Launch all observable events and error handling together:
Task: "Implement circular dependency detection in WpfApp2/Services/ServiceContainer.cs"
Task: "Implement unregistered service detection in WpfApp2/Services/ServiceContainer.cs"
Task: "Implement ServiceResolved and ServiceRegistered events in WpfApp2/Services/ServiceContainer.cs"
```

## Parallel Example: User Story 2 (Event Bus)

```text
# Launch all interface definitions together:
Task: "Define IEventBus interface in WpfApp2/Services/EventBus.cs"
Task: "Define SubscriptionToken class in WpfApp2/Services/EventBus.cs"

# Launch all event bus features together:
Task: "Implement weak reference storage in WpfApp2/Services/EventBus.cs"
Task: "Implement subscriber isolation in WpfApp2/Services/EventBus.cs"
Task: "Implement event filtering in WpfApp2/Services/EventBus.cs"
Task: "Implement observable diagnostics events in WpfApp2/Services/EventBus.cs"
```

## Parallel Example: User Story 3 (MVVM Cleanup)

```text
# Audit all code-behind files first (blocks refactoring):
Task: "Audit code-behind files against MVVM compliance checklist"

# Launch per-file refactoring in parallel once audit is done:
Task: "Refactor Float_path.xaml code-behind — extract logic to ViewModel"
Task: "Refactor AssignTradeCodesWindow.xaml code-behind — extract logic to ViewModel"
Task: "Refactor LinksManagerWindow.xaml code-behind — extract logic to ViewModel"
Task: "Refactor SettingsWindow.xaml code-behind — extract logic to ViewModel"
```

---

## Implementation Strategy

### MVP First (Phase 2 Only — Service Container)

1. Complete Phase 1: Setup
2. Complete Phase 2: ServiceContainer — support singleton, transient, scoped resolution with circular/unregistered detection and observable hooks
3. **STOP and VALIDATE**: Register test services, resolve with all lifetimes, verify error detection
4. Build passes — MVP ready

### Incremental Delivery

1. Complete Setup + ServiceContainer → Foundation ready, US1 deliverable
2. Add EventBus + ModuleRegistry (US2) → Components can communicate decoupled → test independently
3. Add MVVM Cleanup (US3) → Code-behind clean, ViewModels wired via DI → all stories complete
4. Each phase adds value without breaking previous phases

### Single Developer Strategy

Single developer completes phases sequentially in dependency order:
1. Phase 1: Setup
2. Phase 2: US1 — ServiceContainer
3. Phase 3: US2 — EventBus
4. Phase 4: US3 — MVVM Cleanup
5. Phase 5: Polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Build must pass after each phase before moving to the next
- No existing functionality should break during refactoring — preserve behavior
- The service container, event bus, and module registry are internal infrastructure — no external API or new XAML files
- MVVM cleanup is the highest-risk phase — prioritize files by complexity, refactor incrementally
