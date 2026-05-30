# Feature Specification: CommunityToolkit.Mvvm ViewModel Refactor

**Feature Branch**: `[031-ctk-mvvm-refactor]`

**Created**: 2026-05-31

**Status**: Draft

**Input**: User description: "specs\\future-plan-fluent-ui-migration.md PHASE 2"

## Clarifications

### Session 2026-05-31

- **Q**: Should the migration be performed all at once or incrementally in batches? → **A**: Incremental batches (5-7 ViewModels per batch), with build validation after each.
- **Q**: When a bindable property contains custom setter logic, how should it be handled during migration? → **A**: Refactor side effects into `OnPropertyChanged` / `OnPropertyChanging` partial method hooks so the property can still be source-generated.
- **Q**: What verification mechanism should be used to ensure no manual patterns remain after migration? → **A**: Automated script + CI gate — a PowerShell/Bash script scans for forbidden patterns (`grep`) and runs both locally and as a build/CI step that fails the build if any remain.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Eliminate Manual UI Boilerplate (Priority: P1)

As a developer maintaining the application, I want the presentation logic layer to use compile-time code generation for property and command bindings so that I no longer need to write and maintain repetitive, error-prone manual boilerplate code.

**Why this priority**: Manual implementation of property change notifications and command classes is the largest source of subtle binding bugs and maintenance overhead in the UI layer. Eliminating this directly improves reliability and development velocity.

**Independent Test**: Can be fully tested by compiling the solution after migrating the base presentation logic class and verifying that build artifacts include auto-generated binding code files.

**Acceptance Scenarios**:

1. **Given** the application solution is opened in the development environment, **When** the build process runs, **Then** it succeeds with zero errors and produces generated binding code artifacts for all migrated presentation logic classes.
2. **Given** a developer adds a new bindable property to any presentation logic class, **When** the code is compiled, **Then** the property change notification is generated automatically without requiring any manual code.

---

### User Story 2 - Standardize Command Binding Generation (Priority: P2)

As a developer, I want UI command bindings (such as button clicks) to be generated automatically at compile time so that manual command helper classes can be removed entirely from the codebase.

**Why this priority**: Removing custom command implementations reduces the surface area for bugs, ensures consistency across all UI screens, and simplifies onboarding for new developers.

**Independent Test**: Can be fully tested by searching the codebase for any remaining manual command helper classes and confirming their complete absence after migration.

**Acceptance Scenarios**:

1. **Given** the codebase contains manual command helper classes before the refactor, **When** the migration is complete, **Then** those helper classes are deleted and all UI screens continue to function correctly.
2. **Given** a user interacts with any button or menu bound to a presentation logic command, **When** the action is triggered, **Then** the command executes as expected with no behavioral change.

---

### User Story 3 - Preserve Application Behavior (Priority: P3)

As a quality assurance engineer, I want the refactor to produce zero behavioral regressions so that end users experience no difference in functionality, performance, or stability.

**Why this priority**: Refactoring for maintainability must not degrade the user experience. Preserving behavior is the gate for approving the engineering change.

**Independent Test**: Can be fully tested by executing the standard application smoke test (ribbon launch, shell navigation, theme switching, Excel interop) and confirming all steps pass without crashes or UI anomalies.

**Acceptance Scenarios**:

1. **Given** the application is launched from the host environment, **When** the user opens the main shell and navigates through multiple pages, **Then** all pages render correctly and respond to input.
2. **Given** the user switches between Dark and Light themes, **When** the theme change is applied, **Then** all controls update their appearance immediately and consistently.

---

### Edge Cases

- What happens if a presentation logic class is not correctly configured for code generation (e.g., missing partial keyword)?
- How does the system handle any remaining manual property change patterns that were missed during the migration sweep?
- What is the expected build behavior if a new UI screen is added before the migration is fully complete?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The base presentation logic class MUST be migrated to a standard compile-time observable object base provided by the approved MVVM toolkit.
- **FR-002**: All bindable properties across every presentation logic class MUST use compile-time source generation for property change notifications instead of manual implementation.
- **FR-003**: All UI command methods across every presentation logic class MUST use compile-time source generation for command bindings instead of manual helper classes.
- **FR-004**: All existing manual command helper classes and manual asynchronous command helpers MUST be removed from the codebase.
- **FR-005**: Every presentation logic class MUST be correctly configured for the source generation toolchain (e.g., declared as a partial class where required by the generator).
- **FR-006**: The build system MUST successfully generate binding code artifacts for all migrated presentation logic classes, and the application MUST compile with zero errors.
- **FR-007**: An automated verification script MUST scan the codebase for any remaining manual property change notification or command helper patterns. The build MUST fail if forbidden patterns are detected after migration.

### Key Entities *(include if feature involves data)*

- **Presentation Logic Class (ViewModel)**: Encapsulates the state and behavior for a specific UI screen or component. Represents the contract between the user interface and the underlying business logic.
- **Generated Binding Code**: Compile-time artifacts produced by the source generator that wire UI elements to data properties and commands automatically.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 20+ presentation logic classes compile without any manual property change notification boilerplate code remaining.
- **SC-002**: Build output confirms successful generation of binding code artifacts for every modified presentation logic class.
- **SC-003**: Zero manual command helper class references remain anywhere in the codebase (verified by automated search).
- **SC-004**: Standard application smoke test passes: ribbon opens, shell renders, navigation works, theme switches, Excel interop functions, no crashes.
- **SC-005**: No runtime regressions in UI binding behavior (commands execute, properties update, and UI reflects changes immediately).

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The approved MVVM toolkit package (CommunityToolkit.Mvvm) is already installed and available in the project as a result of prior phase work.
- All presentation logic classes follow the existing naming convention and are located in the designated directory.
- This refactor is a mechanical code change and does not alter business logic, user interface layout, or feature behavior.
- Development team members are familiar with source generator requirements such as partial class declarations.
- The existing test suite and smoke test protocol are available to validate zero-regression.
- Migration will proceed incrementally in batches of 5-7 ViewModels with build validation after each batch to minimize risk.
- Bindable properties with custom setter side effects will be refactored into `OnPropertyChanged` / `OnPropertyChanging` partial method hooks to retain behavior while using source generation.
