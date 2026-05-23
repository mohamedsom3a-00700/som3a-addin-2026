# Feature Specification: MVVM & Architecture Cleanup

**Feature Branch**: `009-mvvm-architecture-cleanup`

**Created**: 2026-05-23

**Status**: Draft

**Input**: User description: "Phase 6 — MVVM & Architecture Cleanup from implementation_plan.md"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register and Resolve Application Services via Central Infrastructure (Priority: P1)

A developer adds a new application service and registers it through a central service infrastructure. Other components that depend on that service receive it through resolution rather than creating it directly or passing it manually. The developer can verify the service is correctly wired by testing a component that uses it.

**Why this priority**: Centralized service registration and resolution is the foundation of the architecture cleanup. Without it, service coupling cannot be reduced, and ViewModel isolation cannot be achieved. All other stories depend on this infrastructure being in place.

**Independent Test**: Can be fully tested by registering a test service, resolving it in a dependent component, and verifying the resolved instance matches expectations (same instance for singletons, new instance for transient).

**Acceptance Scenarios**:

1. **Given** a service is registered in the central infrastructure, **When** a dependent component requests it, **Then** the service is provided without the dependent component needing to know how to create it
2. **Given** a service is registered as a singleton, **When** it is resolved multiple times, **Then** the same instance is returned each time
3. **Given** a service is registered as transient, **When** it is resolved multiple times, **Then** a new instance is returned each time
4. **Given** a service has dependencies on other registered services, **When** it is resolved, **Then** all its dependencies are automatically provided

---

### User Story 2 - Components Communicate Without Direct References via Event Bus (Priority: P2)

A feature needs to notify other parts of the application that something happened (e.g., a data refresh is needed, a setting changed). Instead of the feature holding direct references to all interested components, it publishes an event through a central event system. Any component that cares about that event receives it and reacts accordingly.

**Why this priority**: The event bus is the primary mechanism for reducing coupling between features. Without it, features must hold direct references to each other or use brittle callback chains. The service registration infrastructure must exist first (Story 1) to host the event bus.

**Independent Test**: Can be tested by publishing an event from one component and verifying that a different, unrelated component receives and processes the event without the publisher knowing about the subscriber.

**Acceptance Scenarios**:

1. **Given** a component publishes an event, **When** one or more subscribers are registered for that event type, **Then** all subscribers receive the event
2. **Given** no subscribers are registered for an event type, **When** the event is published, **Then** no error occurs and the publish completes normally
3. **Given** a subscriber throws an exception while handling an event, **When** the event is published, **Then** other subscribers still receive the event
4. **Given** a component no longer needs to receive events, **When** it unsubscribes, **Then** it no longer receives subsequent events of that type

---

### User Story 3 - View Contains No Business Logic (Priority: P3)

A developer opens a view file (XAML and its code-behind) and finds that all user interaction logic, data processing, and application rules are in separate ViewModel files. The view only contains UI-specific code (layout, animations, control configuration). Business logic changes require editing ViewModel files, not view files.

**Why this priority**: Separating business logic from view code is the core promise of MVVM and directly impacts testability, maintainability, and the ability to change UI without changing logic. It builds on the service infrastructure (Story 1) to ensure ViewModels can receive the services they need.

**Independent Test**: Can be tested by performing an audit of each view file against a checklist: no data access, no service calls, no business rules, no complex conditional logic in code-behind.

**Acceptance Scenarios**:

1. **Given** a view is opened in the application, **When** a user interacts with it (clicks, types, selects), **Then** all interaction handling is delegated to the associated ViewModel through commands and bindings
2. **Given** a ViewModel needs data from a service, **When** it is created, **Then** the service is provided through the central infrastructure — not created inside the ViewModel
3. **Given** all views in the application, **When** a code-behind audit is performed, **Then** no view contains business logic, data access, or direct service instantiation

---

### Edge Cases

- What happens when a circular dependency is detected during service resolution? The system should detect this and report it clearly.
- What happens if an event subscriber is no longer alive (e.g., the owning window was closed)? Subscribers that are no longer active should automatically stop receiving events without requiring manual cleanup.
- How does the system behave when a service fails to initialize during resolution? A clear error should indicate which service failed and why.
- What happens to existing code-behind logic that was previously acceptable? A phased cleanup with priorities for the most problematic files.
- How does the event bus handle high-frequency events (e.g., progress updates)? Publishers are responsible for pacing their own events. The event bus delivers all published events without throttling, coalescing, or blocking — it is a simple publish/deliver channel.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a central service registration and resolution mechanism so that services can be registered once and provided to all dependent components
- **FR-002**: Services MUST be resolvable with different lifetimes: singleton (same instance always), transient (new instance each time), and scoped (same instance within a defined scope)
- **FR-003**: System MUST detect circular dependencies during service resolution and report them as clear errors rather than hanging or crashing
- **FR-004**: System MUST provide an event bus that allows components to publish events and other components to subscribe to events without knowing about each other
- **FR-005**: Event subscribers MUST be able to subscribe to specific event types and receive only events of that type
- **FR-006**: Event subscriptions MUST automatically release subscribers that are no longer active or have been closed
- **FR-007**: A subscriber exception during event handling MUST NOT prevent other subscribers from receiving the event
- **FR-008**: All ViewModels MUST receive their service dependencies through the central infrastructure — direct instantiation of services within ViewModels is prohibited
- **FR-009**: Code-behind files MUST NOT contain business logic, data access code, or service instantiation — only UI-specific code is permitted
- **FR-010**: System MUST provide a mechanism for self-contained feature units (modules) to register their own services, pages, and commands as part of application startup
- **FR-011**: Modules MUST register through a single explicit composition point — adding a new feature module requires adding one registration line in the composition root, not editing existing module or application code
- **FR-012**: An audit capability MUST exist to verify MVVM compliance — identifying views with business logic in code-behind, ViewModels creating services directly, and violations of separation rules
- **FR-013**: System MUST expose observable hooks for key infrastructure operations — service registration, service resolution, event publication, and event subscription — enabling downstream diagnostics without coupling to a specific logging framework
- **FR-014**: When a service type is requested but not registered, the service container MUST throw a descriptive exception identifying the unregistered type — fail fast rather than returning null or a fallback

### Key Entities

- **Service Container**: The central mechanism that holds service registrations and resolves them on demand. Manages service lifetimes and dependency chains.
- **Event Bus**: The communication channel that enables decoupled publish/subscribe between components. Events flow from publisher to all subscribers without either knowing about the other.
- **Module**: A self-contained feature unit that registers its own services, commands, and UI components during application startup. Modules can be added independently.
- **ViewModel**: The UI logic container that holds user interaction handling, data transformation, and command definitions — separated from the visual view.
- **View**: The visual representation of a UI component (screen, window, page, control) containing only layout, styling, and UI-specific behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can add a new service and make it available to the entire application by adding a single registration line — no manual wiring in dependent components required
- **SC-002**: A new feature module can be added to the application by creating the module class and registering it — no modifications to existing application code are needed
- **SC-003**: An event publisher and subscriber can be added without either component knowing about the other — verified by independent unit tests
- **SC-004**: After cleanup, at least 80% of ViewModels receive their dependencies through the service container rather than creating them directly
- **SC-005**: After cleanup, at least 90% of views pass MVVM compliance audit (no business logic in code-behind)
- **SC-006**: A circular dependency scenario is detected and reported with a clear error message within 1 second of attempted resolution — no hang or crash
- **SC-007**: The service resolution and event bus systems add no more than 50ms to application startup time

## Clarifications

### Session 2026-05-23

- Q: Should the service container and event bus include built-in diagnostics/logging support? → A: Yes, minimal observability hooks — core operations (resolve, register, publish, subscribe, unsubscribe) expose observable events for downstream diagnostics without coupling to a logging framework.
- Q: How many services, modules, and event types should the infrastructure support? → A: Medium scale — 20-100 services, 3-10 modules, 10-50 event types. Dictionary-based lookup with basic concurrency support.
- Q: How should modules be discovered and loaded? → A: Explicit registration in a single composition point at application startup. Convention-based or config-driven discovery deferred to Phase 9 plugin platform.
- Q: What should happen when a component requests a service that has not been registered? → A: The service container must throw a clear, descriptive exception at resolution time identifying the unregistered service type — fail fast.
- Q: Should the event bus include built-in throttling or coalescing for high-frequency events? → A: No — publishers are responsible for pacing their own events. The event bus remains a simple publish/deliver channel.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The existing ThemeManager, service detection, and rendering infrastructure remain unchanged by this phase — the focus is on application architecture, not UI.
- Existing windows and their code-behind files are the primary candidates for cleanup — new feature pages (Phase 5 shell) will follow the new patterns from the start.
- No existing functionality is broken during the architecture cleanup — refactoring preserves behavior.
- The cleanup is performed incrementally: infrastructure first (DI, event bus, module system), then refactoring of existing code.
- The MVVM compliance audit covers only the code-behind separation rule — ViewModel test coverage is outside this phase's scope.
- The module system does not replace the existing theme or rendering systems — it layers on top for feature registration only.
- The infrastructure targets medium scale: up to 100 services, 10 modules, and 50 event types. Lookup and concurrency are designed to handle this range efficiently.
- Modules use explicit registration at a single composition root. Convention-based or config-driven auto-discovery is deferred to the Phase 9 plugin platform.
