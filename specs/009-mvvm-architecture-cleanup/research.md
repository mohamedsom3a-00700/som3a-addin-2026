# Research: MVVM & Architecture Cleanup

## Service Container Pattern

**Decision**: Use explicit generic registration and resolution API with constructor-based dependency injection.

**Rationale**: Constructor injection is the industry-standard DI pattern for .NET Framework applications. It makes dependencies explicit at compile time, enables clear lifetime management (singleton/transient/scoped), and integrates naturally with unit testing. An explicit API (no attribute-based or convention-based auto-registration) keeps bootstrapping visible and debuggable — appropriate for the current project scale.

**Alternatives considered**:
- **Service locator anti-pattern**: Hides dependencies, makes testing harder, introduces hidden coupling. Rejected.
- **Property injection**: Less explicit, dependencies can be left unset at runtime. Rejected for primary pattern but available for edge cases.
- **Auto-registration via assembly scanning**: Adds complexity, reduces bootstrapping transparency. Deferred to Phase 9 plugin platform.

## Event Bus Pattern

**Decision**: Use a typed publish/subscribe event bus with weak references and async-compatible dispatch.

**Rationale**: A typed event bus (generic events with compile-time type safety) is simpler and more maintainable than string-based event routing. Weak references prevent memory leaks when subscribers outlive their owning components (windows, pages). Async-compatible dispatch ensures publishers aren't blocked by slow subscribers while maintaining in-order delivery per subscriber.

**Alternatives considered**:
- **Strong-reference event bus**: Risk of memory leaks. Rejected.
- **String-keyed events**: Prone to typos, no compile-time safety. Rejected.
- **Synchronous blocking dispatch**: Publishers blocked by slow subscribers. Rejected for high-frequency events.

## Module Registration Pattern

**Decision**: Explicit module registration at a single composition point (composition root), with each module implementing an `IModule` interface.

**Rationale**: A composition root makes all application wiring visible in one place. Each module self-registers its services, pages, and commands through a standard interface, keeping feature code self-contained without hidden initialization. This pattern is simple, testable, and easy to debug — no reflection, no scanning, no config files.

**Alternatives considered**:
- **Convention-based scanning**: Auto-discovers modules by naming pattern. More "magic", harder to debug. Deferred to Phase 9.
- **Configuration file driven**: Module list in XML/JSON. Adds file I/O dependency and parsing complexity. Not justified at current scale.
- **Attribute-based registration**: Requires reflection at startup. Reduces bootstrapping transparency. Rejected.

## MVVM Audit Approach

**Decision**: Manual code review with an audit checklist, performed incrementally on a per-file basis.

**Rationale**: Automated MVVM violation detection in WPF code-behind is unreliable (heuristic-based, high false-positive rate). A manual audit with a clear checklist (no data access, no service calls, no business rules, no complex conditionals in code-behind) is more accurate for the project's size (~14 windows). The audit is prioritized by file complexity and user impact.

**Alternatives considered**:
- **Automated static analysis**: Requires custom Roslyn analyzers. High upfront investment for limited accuracy at current project scale.
- **Full rewrite**: Highest risk of regression. Rejected in favor of incremental refactoring.

## Technology Choices

**Decision**: The infrastructure is implemented using native C# and WPF constructs — no third-party DI or event bus libraries.

**Rationale**: The project constitution explicitly prohibits third-party UI frameworks (Constitution §XIV). While `Microsoft.Extensions.DependencyInjection` is a Microsoft library (not a third-party UI framework), a custom lightweight implementation avoids adding a NuGet dependency for what is a relatively simple container requirement at this scale. A custom implementation also keeps the API surface aligned with the project's specific needs (explicit registration, observable hooks, clear error messages).

**Alternatives considered**:
- **Microsoft.Extensions.DependencyInjection**: Microsoft library, but introduces external dependency. May be reconsidered if the container requirements grow.
- **Autofac, Ninject, Unity**: Third-party DI containers. Violate the project's "no third-party" spirit and add versioning/deployment complexity for an Excel VSTO add-in.

## Code-Behind Refactoring Strategy

**Decision**: Refactor code-behind by extracting business logic into ViewModels and services, one file at a time, with existing behavior preserved through regression testing.

**Rationale**: Incremental refactoring minimizes regression risk. Each file is handled individually: identify code-behind logic, extract to ViewModel or service, wire via data binding or command pattern, verify behavior matches before moving to the next file. The infrastructure (service container, event bus) must exist first so that extracted ViewModels have a proper dependency injection mechanism.

**Alternatives considered**:
- **Batch refactoring of all files**: Highest risk of regression. Rejected.
- **Rewrite from scratch**: Unnecessary — existing functionality is correct, only its location needs to change. Rejected.
