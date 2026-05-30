# Feature Specification: Persistence & Platform Database Infrastructure

**Feature Branch**: `027-persistence-infrastructure`

**Created**: 2026-05-30

**Status**: Draft

**Input**: User description: "Introduce a stable enterprise persistence layer for settings, AI metadata, diagnostics, plugin metadata, templates, and platform history using SQLite with repository pattern, unit of work, migrations, backup/recovery, and async operations."

## User Scenarios & Testing

### User Story 1 - Settings Persist Across Sessions (Priority: P1)

A planning engineer customizes their theme, selects accent colors, configures AI provider keys, adjusts accessibility settings, and sets Excel export preferences. After closing and reopening the platform, all settings are restored exactly as configured — no need to re-enter preferences.

**Why this priority**: Settings persistence is the most fundamental database use case. Without it, every user session starts from defaults, making the platform unusable for daily work.

**Independent Test**: Can be fully tested by changing a setting, restarting the platform, and verifying the setting value matches the previous session.

**Acceptance Scenarios**:

1. **Given** a user has configured theme to Dark, accent color to blue, and AI provider key for OpenAI, **When** the platform is closed and reopened, **Then** all three settings are restored to their previous values
2. **Given** a user has never configured settings (first launch), **When** the platform opens, **Then** sensible defaults are applied and no errors occur
3. **Given** a user changes a setting value, **When** the change is saved, **Then** the new value is immediately persisted and survives a restart

---

### User Story 2 - AI Execution History Tracking (Priority: P1)

A planning engineer runs multiple AI operations — BOQ activity generation, WBS generation, relationship logic analysis. Each execution is logged with prompt details, provider used, token consumption, timestamps, and output validation results. The engineer can review past executions to compare results across different AI providers or prompt variations.

**Why this priority**: AI is the core differentiator of the platform. Persisting AI history enables auditing, cost tracking, provider comparison, and debugging — essential for enterprise adoption.

**Independent Test**: Can be fully tested by running two AI operations via different providers, then querying the execution history to verify both are recorded with correct metadata.

**Acceptance Scenarios**:

1. **Given** a user triggers AI activity generation via Claude, **When** the operation completes, **Then** the execution is recorded with provider name, token count, prompt used, timestamp, and success/failure status
2. **Given** a user queries AI execution history, **When** filtering by date range and provider, **Then** only matching records are returned
3. **Given** an AI operation fails due to provider timeout, **When** the failure occurs, **Then** the failed execution is logged with error details and retry information

---

### User Story 3 - Plugin State Management (Priority: P2)

An IT administrator installs a set of planning plugins, enables/disables specific plugins for different user roles, and configures plugin-specific settings. Plugin states and configurations survive platform restarts and Excel host reloads.

**Why this priority**: Plugin architecture is the foundation of platform extensibility. Persisting plugin metadata and states ensures consistent behavior across sessions without manual reconfiguration.

**Independent Test**: Can be fully tested by installing two plugins, disabling one, restarting the platform, and verifying only the enabled plugin loads.

**Acceptance Scenarios**:

1. **Given** three plugins are installed with one disabled, **When** the platform restarts, **Then** only the two enabled plugins load
2. **Given** a plugin updates to a new version, **When** the platform detects the version change, **Then** the plugin version history is recorded without data loss
3. **Given** a plugin becomes corrupted, **When** the platform attempts to load it, **Then** the error is logged and the plugin is marked as unhealthy without crashing the platform

---

### User Story 4 - Diagnostics Logging & Crash Recovery (Priority: P2)

When an unexpected error occurs during Excel export or AI generation, the platform logs the error details, stack trace, and platform state snapshot. On next startup, the user is notified of the recorded crash and can review diagnostics logs to identify recurring issues.

**Why this priority**: Enterprise reliability requires systematic error tracking. Persisting diagnostics enables support teams to diagnose issues without relying on user-reported error descriptions.

**Independent Test**: Can be fully tested by triggering a controlled error, restarting the platform, and verifying the error appears in the diagnostics history.

**Acceptance Scenarios**:

1. **Given** an AI provider returns an invalid response, **When** the parsing fails, **Then** the error with timestamp, provider name, and raw response is logged
2. **Given** the platform crashes unexpectedly, **When** it restarts, **Then** a crash report is generated and stored with platform state info
3. **Given** a user views the diagnostics dashboard, **When** they filter by error severity, **Then** only matching log entries are displayed

---

### User Story 5 - Template Management (Priority: P3)

A planning engineer creates custom WBS templates for residential and commercial projects. These templates are stored in the database and are available across all projects and sessions. The engineer can also load productivity benchmarks for common construction trades.

**Why this priority**: Templates improve productivity but are not critical for core platform functionality. They represent an enhancement over file-based template storage.

**Independent Test**: Can be fully tested by creating a WBS template, restarting the platform, and verifying the template appears in the template browser with all data intact.

**Acceptance Scenarios**:

1. **Given** a user creates a new WBS template with code structure and node names, **When** saved, **Then** the template is available in the template browser after restart
2. **Given** productivity benchmarks are imported from a reference database, **When** queried by trade category, **Then** matching benchmark rates are returned
3. **Given** a user exports all templates for backup, **When** restored on a fresh installation, **Then** all templates are available

### Edge Cases

- What happens when the database file is corrupted or missing?
- How does the system handle concurrent write attempts from multiple plugins? → SQLite WAL mode with built-in locking handles single-writer concurrency; plugins retry automatically on lock contention
- What happens when a migration fails partway through?
- How does the system handle a full disk or insufficient disk space?
- What happens when an AI execution record contains unexpectedly large data?
- How does the system behave when the database schema is from a future version?
- What happens during backup when the platform is in active use?

## Requirements

### Functional Requirements

- **FR-001**: System MUST initialize the database automatically on first launch, creating the database file and applying all necessary schema migrations
- **FR-002**: System MUST persist all platform settings (theme, accent, accessibility, performance, Excel) between application restarts
- **FR-003**: System MUST log every AI execution with provider name, prompt input, response output, token count, duration, and success/failure status
- **FR-004**: System MUST record plugin installation, version history, enable/disable state, and health status in the database
- **FR-005**: System MUST log application errors, exceptions, crash reports, and export history with timestamps and relevant context
- **FR-006**: System MUST store WBS templates, activity templates, relationship templates, and productivity benchmarks persistently
- **FR-007**: System MUST provide atomic transaction support across multiple repository operations (Unit of Work pattern)
- **FR-008**: System MUST support automatic database migrations with version tracking and rollback on failure
- **FR-009**: System MUST provide manual backup and restore functionality for the complete database
- **FR-010**: System MUST perform all database operations asynchronously with support for cancellation tokens
- **FR-011**: System MUST encrypt sensitive data (API keys, provider credentials) at rest using platform-native encryption mechanisms
- **FR-012**: System MUST allow individual plugins to persist their own state and configuration data via a plugin persistence API
- **FR-013**: System MUST support querying AI execution history by date range, provider, and status filters
- **FR-014**: System MUST detect database corruption on startup and initiate recovery or inform the user with clear instructions
- **FR-015**: System MUST implement a persistence validation engine to verify schema integrity and detect corrupted records
- **FR-016**: System MUST support configurable per-category data retention with automated cleanup jobs (AI history: 1 year default, diagnostics logs: 90 days default, crash reports: 2 years default)

### Key Entities

- **SettingsRecord**: User preferences including theme, accent color, font selection, accessibility, performance, and per-plugin settings — identified by GUID, keyed by category and name
- **AIExecutionRecord**: A single AI operation — captures provider, model, prompt, response, token usage, duration, timestamp, success/failure, retry count
- **AIRuntimeRecord**: Token usage summary per session — provider-level aggregation of consumed tokens, cost estimates, operation counts
- **PluginRecord**: Installed plugin metadata — plugin ID, name, version, description, author, dependencies, enabled/disabled state, health status
- **DiagnosticsLog**: Application error log — severity level, timestamp, component, error message, stack trace, platform state snapshot
- **CrashReport**: Unexpected termination record — timestamp, last operation, memory usage, thread state, Excel interop status
- **ExportHistoryRecord**: Completed export operation — export format, row count, duration, timestamp, success/failure
- **TemplateRecord**: Shared template entity — type (WBS, Activity, Relationship, ProductivityBenchmark), name, category, structured content, version, last modified
- **BackupManifest**: Backup operation metadata — timestamp, file path, size, checksum, included table list, platform version

## Success Criteria

### Measurable Outcomes

- **SC-001**: Database initializes and is ready for queries within 100ms of platform startup on a typical engineering workstation
- **SC-002**: Settings load and apply within 50ms — users see their configured theme and preferences immediately on startup
- **SC-003**: AI execution history queries return results within 300ms for up to 10,000 records
- **SC-004**: Plugin metadata loads within 100ms during platform initialization — no startup delay attributable to persistence
- **SC-005**: All five repository interfaces (Settings, AI, Plugin, Diagnostics, Templates) are implemented and pass contract verification tests
- **SC-006**: Unit of Work commits transactions atomically — partial failures trigger full rollback leaving database in consistent state
- **SC-007**: Migration engine applies schema changes with version tracking and supports rollback to previous version on failure
- **SC-008**: Backup and restore completes without data loss — restored database passes integrity verification
- **SC-009**: All database operations execute asynchronously — no UI thread is blocked during read or write operations
- **SC-010**: Platform builds with zero errors and all existing VSTO host tests pass — persistence layer does not break existing functionality

## Clarifications

### Session 2026-05-30

- Q: Data retention policy — How long should AI execution history, diagnostics logs, and crash reports be retained? → A: Configurable per-category retention with sensible defaults (AI history: 1 year, diagnostics logs: 90 days, crash reports: 2 years)
- Q: Concurrent write strategy — How to handle multiple plugins writing simultaneously? → A: SQLite WAL mode with built-in file-level locking and automatic retry
- Q: Entity ID strategy — What primary key strategy should entities use? → A: GUIDs for all entities

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The database will be stored as a single SQLite file in the platform data directory (AppData/Som3a/platform.db)
- Sensitive data (API keys, credentials) will be encrypted using platform-native Windows DPAPI
- Database migrations will run automatically on platform startup with rollback on failure
- Manual backup will be triggered by user action; automated scheduled backup defaults to weekly
- All plugins share the same database instance with table-level isolation rather than per-plugin databases
- The persistence layer is implemented within the existing Som3a.Infrastructure (or Planova.Infrastructure) project
- Performance targets assume a modern engineering workstation with SSD storage
- Database file size is expected to stay under 500MB for typical usage over 12 months given per-category retention defaults
- Data retention defaults are configurable: AI history 1 year, diagnostics logs 90 days, crash reports 2 years
- All entities use GUID as primary key for future cloud sync readiness and plugin isolation
- Migration compatibility spans +/- 2 versions — migrations are written to support skipping at most 2 version jumps
- Cloud synchronization is explicitly out of scope for this phase but the repository abstraction supports future migration to PostgreSQL or SQL Server
