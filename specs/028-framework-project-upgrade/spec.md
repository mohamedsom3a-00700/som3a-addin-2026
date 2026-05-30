# Feature Specification: Framework & Project Format Upgrade

**Feature Branch**: `028-framework-project-upgrade`

**Created**: 2026-05-30

**Status**: Draft

**Input**: User description: "specs\future-plan-fluent-ui-migration.md phase 1A"

## Clarifications

### Session 2026-05-30

- Q: Should NuGet packages be upgraded to minimum compatible versions or kept at exact versions? → A: Upgrade to minimum compatible versions for .NET 8.0 compatibility; stay as close to current versions as possible.
- Q: Must existing unit/integration tests pass after the upgrade? → A: All existing tests must pass unchanged — zero test modifications allowed.
- Q: How thorough should WPF rendering verification be? → A: Compare against pre-upgrade screenshots of 3-5 key pages in both Dark and Light themes.
- Q: Should VSTO add-in configuration (binding redirects, assembly references) be updated as part of this phase? → A: Audit and update VSTO add-in configuration to reflect the upgraded WPF project.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upgrade WPF Application to .NET 8.0 (Priority: P1)

As a developer, I need to upgrade the existing WPF application from .NET Framework 4.8 to .NET 8.0 so that the codebase can leverage the modern .NET ecosystem and unblock the full Fluent UI migration plan.

**Why this priority**: This is the foundational step — all subsequent migration phases (out-of-process architecture, new NuGet packages, Fluent UI integration) depend on this upgrade completing first.

**Independent Test**: Build the upgraded project and verify zero compile errors. Run a full VSTO smoke test to confirm the upgrade introduces no behavioral changes.

**Acceptance Scenarios**:

1. **Given** the WpfApp2 project targeting .NET Framework 4.8, **When** the project file is converted to SDK-style format targeting .NET 8.0-windows, **Then** all existing source files compile without errors.

2. **Given** the upgraded .NET 8.0 project, **When** the project is built, **Then** the build succeeds with zero errors.

3. **Given** the upgraded project, **When** the VSTO add-in is loaded in Excel and the ShellWindow opens, **Then** the ribbon buttons are visible, sidebar renders correctly, navigation to 3 pages works, theme switching (Dark/Light) works, and Excel cell write via interop succeeds without crashes.

---

### User Story 2 - Migrate NuGet Package Management (Priority: P2)

As a developer, I want to migrate from the legacy package management format to the modern format so that dependency management aligns with modern .NET conventions and enables transitive package resolution.

**Why this priority**: This is a prerequisite for future NuGet additions (FluentIcons.WPF, WPF-UI in Phase 1C) and ensures clean dependency graph resolution.

**Independent Test**: Verify all existing NuGet packages are restored and resolved without conflicts, and the project builds successfully with no missing assembly references.

**Acceptance Scenarios**:

1. **Given** the project using the legacy package management format, **When** all packages are migrated to the modern format, **Then** package restore succeeds and all assemblies resolve correctly.

2. **Given** the migrated package references, **When** the project is built, **Then** the output binaries function identically to the pre-migration build.

---

### User Story 3 - Fix .NET API Compatibility Issues (Priority: P3)

As a developer, I want to fix any compile errors caused by APIs that were removed or changed between .NET Framework 4.8 and .NET 8.0 so that the project builds cleanly without behavioral changes.

**Why this priority**: While many APIs are compatible, some Framework-specific APIs (e.g., certain AppDomain APIs, Windows Forms interop) have changed or been removed in .NET 8.0 and require targeted fixes.

**Independent Test**: Build the project and verify zero compile errors. Compare git diff to confirm only API compatibility fixes were applied — no logic, business rules, or UI behavior was altered.

**Acceptance Scenarios**:

1. **Given** a compile error caused by a removed or changed API, **When** the minimum fix is applied to use the .NET 8.0 equivalent, **Then** the build succeeds and the behavior remains identical.

2. **Given** all API compatibility fixes are applied, **When** the git diff is reviewed, **Then** no logic changes or behavioral modifications are present — only project format changes and minimal API compatibility adjustments.

---

### Edge Cases

- What happens when a .NET Framework 4.8 API has no direct .NET 8.0 equivalent? — Use the documented compatibility replacement or polyfill, preserving the original behavior.
- What happens if a NuGet package does not support .NET 8.0? — Upgrade to the minimum compatible version that supports .NET 8.0; if none exists, document the blocker and evaluate alternatives.
- What happens if the build produces different warnings than before? — Warnings specific to the framework upgrade (e.g., deprecated API warnings) are expected and acceptable; new warnings unrelated to the upgrade must be investigated.
- What happens if WPF rendering behavior differs subtly between .NET Framework 4.8 and .NET 8.0? — Compare against pre-upgrade screenshots of 3-5 key pages in both themes (Dark/Light); any visual regression must be investigated and resolved.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The WPF project MUST target .NET 8.0 for Windows after the upgrade.
- **FR-002**: The project file MUST use the modern SDK-style format after the upgrade.
- **FR-003**: All existing NuGet packages MUST be re-added using `PackageReference` format — no packages may be dropped or replaced; version upgrades are allowed only to the minimum compatible version for .NET 8.0.
- **FR-004**: The build MUST complete with zero compile errors after all changes.
- **FR-005**: Zero application logic, business rules, or UI behavior MUST be altered during the upgrade.
- **FR-006**: Any API compatibility fixes MUST use the minimum change needed to compile — no refactoring, renaming, or restructuring.
- **FR-007**: All existing unit and integration tests MUST pass unchanged after the upgrade — zero test modifications allowed.
- **FR-008**: The VSTO add-in project's configuration (binding redirects, assembly references, app.config) MUST be audited and updated to remain compatible with the upgraded WPF project.

### Key Entities

This feature does not introduce new data entities — it is purely a project infrastructure upgrade.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Building the upgraded project completes with zero errors.
- **SC-002**: All existing software dependencies are restored without conflicts.
- **SC-003**: Code review of changes shows only project format modifications and minimal API compatibility fixes — zero logic changes.
- **SC-004**: Full application smoke test passes: ribbon buttons visible, ShellWindow opens, sidebar renders, navigate to 3 pages, theme switch (Dark/Light) works, Excel cell write succeeds — no crashes.
- **SC-005**: All existing pages render without visual regressions compared to pre-upgrade screenshots in both Dark and Light themes.
- **SC-006**: Build time does not increase by more than 50% compared to the pre-migration build time for the same configuration.
- **SC-007**: All existing unit and integration tests pass unchanged after the upgrade.
- **SC-008**: VSTO add-in loads and functions correctly with the upgraded WPF project after configuration audit.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- All existing NuGet packages have .NET 8.0-compatible versions available or can be upgraded to compatible versions.
- The WPF application does not use any .NET Framework 4.8-exclusive APIs that have no replacement in .NET 8.0.
- The VSTO add-in project (which must remain on .NET Framework 4.8) requires an audit of its binding redirects, assembly references, and app.config to maintain compatibility with the upgraded WPF project.
- API compatibility fixes, if needed, can be identified via compile errors and resolved without behavioral changes.
- The build environment (CI/CD pipeline) has .NET 8.0 SDK installed or can be updated to support it.
- No third-party dependencies will block the upgrade — any blockers will be documented and addressed in a subsequent phase.
- Visual behavior of WPF controls is consistent between .NET Framework 4.8 and .NET 8.0 for the same XAML and styles.
