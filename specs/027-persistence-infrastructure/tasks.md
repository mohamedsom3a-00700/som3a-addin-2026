---

description: "Task list for Persistence & Platform Database Infrastructure (Phase 27)"
---

# Tasks: Persistence & Platform Database Infrastructure

**Input**: Design documents from `specs/027-persistence-infrastructure/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Minimal test tasks included per plan.md requirement; focus on contract verification via in-memory SQLite.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize Persistence directory structure and add NuGet dependencies

- [x] T001 Create `Persistence/` directory structure under `Som3a.Infrastructure/` — SQLite/, Repositories/, Interfaces/, Migrations/, Seeders/, UnitOfWork/, Backup/
- [x] T002 [P] Add `Microsoft.Data.Sqlite` NuGet package (8.0+) to `Som3a.Infrastructure/Som3a.Infrastructure.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core SQLite infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `SQLiteConfiguration` class (data directory, filename, connection string builder) in `Som3a.Infrastructure/Persistence/SQLite/SQLiteConfiguration.cs`
- [x] T004 [P] Create `ConnectionManager` (reader/writer split — one long-lived write connection, on-demand read connections, SemaphoreSlim(1,1) write gate) in `Som3a.Infrastructure/Persistence/SQLite/ConnectionManager.cs`
- [x] T005 [P] Create `DatabaseContext` (connection setup, PRAGMA application: WAL, synchronous=NORMAL, busy_timeout=5000, foreign_keys=ON, temp_store=MEMORY) in `Som3a.Infrastructure/Persistence/SQLite/DatabaseContext.cs`
- [x] T006 Create `DatabaseFactory` (singleton factory for UnitOfWork creation, initialization orchestration) in `Som3a.Infrastructure/Persistence/SQLite/DatabaseFactory.cs`
- [x] T007 Create migration engine with PRAGMA user_version tracking, numbered SQL scripts, and automatic rollback on failure in `Som3a.Infrastructure/Persistence/Migrations/MigrationEngine.cs`
- [x] T008 Create initial schema migration SQL (001_initial_schema.sql) — all entity tables: SettingsRecord, AIExecutionRecord, AIRuntimeRecord, PluginRecord, PluginVersionRecord, DiagnosticsLog, CrashReport, ExportHistoryRecord, TemplateRecord, BackupManifest with GUID PKs, indexes, unique constraints in `Som3a.Infrastructure/Persistence/Migrations/001_initial_schema.sql`
- [x] T009 [P] Create `IUnitOfWork` interface (BeginTransactionAsync, CommitAsync, RollbackAsync, IsInTransaction, repository accessors) in `Som3a.Infrastructure/Persistence/Interfaces/IUnitOfWork.cs`
- [x] T010 [P] Create `UnitOfWork` implementation (BEGIN IMMEDIATE transaction, routes through ConnectionManager/WriteConnection) in `Som3a.Infrastructure/Persistence/UnitOfWork/UnitOfWork.cs`
- [x] T011 [P] Create generic `IRepository<T>` base interface (GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync) in `Som3a.Infrastructure/Persistence/Interfaces/IRepository.cs`
- [x] T012 Create `DataRetentionConfiguration` class with per-category retention defaults (AI: 1yr, diagnostics: 90d, crash reports: 2yr, export history: 1yr) in `Som3a.Infrastructure/Persistence/DataRetentionConfiguration.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin independently

---

## Phase 3: User Story 1 — Settings Persist Across Sessions (Priority: P1) 🎯 MVP

**Goal**: Users can change settings (theme, accent, AI keys, accessibility, export prefs) and they persist across restarts

**Independent Test**: Change a setting, restart the platform, verify the setting value matches the previous session

### Implementation for User Story 1

- [x] T013 [P] [US1] Create `SettingsRecord` entity class (Id, Category, Name, Value, PluginId, UpdatedAt, CreatedAt) in `Som3a.Infrastructure/Persistence/Models/SettingsRecord.cs`
- [x] T014 [US1] Implement `ISettingsRepository` interface (GetSettingAsync, GetSettingsByCategoryAsync, GetPluginSettingsAsync, SetSettingAsync, SetSettingsBulkAsync, DeleteSettingAsync, DeletePluginSettingsAsync, HasCategoryAsync) in `Som3a.Infrastructure/Persistence/Interfaces/ISettingsRepository.cs`
- [x] T015 [US1] Implement `SettingsRepository` (upsert via INSERT OR REPLACE, category+name+pluginId unique constraint) in `Som3a.Infrastructure/Persistence/Repositories/SettingsRepository.cs`

**Checkpoint**: User Story 1 is fully functional — settings survive platform restarts

---

## Phase 4: User Story 2 — AI Execution History Tracking (Priority: P1)

**Goal**: Every AI operation is logged with provider, prompt, response, token usage, duration, and status; filterable by date/provider/status

**Independent Test**: Run two AI operations via different providers, query execution history, verify both are recorded with correct metadata

### Implementation for User Story 2

- [x] T016 [P] [US2] Create `AIExecutionRecord` entity class (Id, ProviderName, ModelName, PromptText, ResponseText, TokenInput, TokenOutput, DurationMs, Status, ErrorMessage, RetryCount, PluginId, ExecutedAt) in `Som3a.Infrastructure/Persistence/Models/AIExecutionRecord.cs`
- [x] T017 [P] [US2] Create `AIRuntimeRecord` entity class (Id, ProviderName, SessionId, TokenInputTotal, TokenOutputTotal, OperationCount, EstimatedCost, StartedAt, UpdatedAt) in `Som3a.Infrastructure/Persistence/Models/AIRuntimeRecord.cs`
- [x] T018 [US2] Implement `IAIRepository` interface (LogExecutionAsync, GetExecutionAsync, QueryExecutionsAsync, CountExecutionsAsync, UpsertRuntimeRecordAsync, GetRuntimeRecordAsync, GetSessionRuntimeRecordsAsync, CleanupAsync) in `Som3a.Infrastructure/Persistence/Interfaces/IAIRepository.cs`
- [x] T019 [US2] Implement `AIRepository` (parameterized queries with date/provider/status filtering, pagination via skip/take) in `Som3a.Infrastructure/Persistence/Repositories/AIRepository.cs`

**Checkpoint**: User Story 2 is fully functional — AI executions are tracked and queryable

---

## Phase 5: User Story 3 — Plugin State Management (Priority: P2)

**Goal**: Plugin metadata, enable/disable state, version history, and health status persist across restarts and Excel host reloads

**Independent Test**: Install two plugins, disable one, restart platform, verify only enabled plugin loads

### Implementation for User Story 3

- [x] T020 [P] [US3] Create `PluginRecord` entity class (Id, PluginId, Name, Version, Description, Author, Dependencies, IsEnabled, HealthStatus, HealthMessage, Settings, InstalledAt, UpdatedAt) in `Som3a.Infrastructure/Persistence/Models/PluginRecord.cs`
- [x] T021 [P] [US3] Create `PluginVersionRecord` entity class (Id, PluginRecordId, Version, InstalledAt, InstalledBy) in `Som3a.Infrastructure/Persistence/Models/PluginVersionRecord.cs`
- [x] T022 [US3] Implement `IPluginRepository` interface (RegisterPluginAsync, GetPluginAsync, GetAllPluginsAsync, GetEnabledPluginsAsync, SetPluginEnabledAsync, SetPluginHealthAsync, RecordVersionAsync, GetVersionHistoryAsync, UnregisterPluginAsync) in `Som3a.Infrastructure/Persistence/Interfaces/IPluginRepository.cs`
- [x] T023 [US3] Implement `PluginRepository` (upsert by PluginId, FK to PluginVersionRecord, health status tracking) in `Som3a.Infrastructure/Persistence/Repositories/PluginRepository.cs`

**Checkpoint**: User Story 3 is fully functional — plugin states persist across restarts

---

## Phase 6: User Story 4 — Diagnostics Logging & Crash Recovery (Priority: P2)

**Goal**: Errors, crashes, and export history are logged persistently; diagnostics dashboard can query by severity/component/date range

**Independent Test**: Trigger a controlled error, restart platform, verify error appears in diagnostics history

### Implementation for User Story 4

- [x] T024 [P] [US4] Create `DiagnosticsLog` entity class (Id, Severity, Component, Message, StackTrace, PlatformState, LoggedAt) in `Som3a.Infrastructure/Persistence/Models/DiagnosticsLog.cs`
- [x] T025 [P] [US4] Create `CrashReport` entity class (Id, LastOperation, MemoryUsageMb, ThreadState, ExcelInteropStatus, CrashDump, LoggedAt) in `Som3a.Infrastructure/Persistence/Models/CrashReport.cs`
- [x] T026 [P] [US4] Create `ExportHistoryRecord` entity class (Id, Format, RowCount, FileSize, DurationMs, Status, ErrorMessage, ExportedAt) in `Som3a.Infrastructure/Persistence/Models/ExportHistoryRecord.cs`
- [x] T027 [US4] Implement `ILogRepository` interface (WriteLogAsync, WriteLogBatchAsync, QueryLogsAsync, RecordCrashAsync, GetRecentCrashesAsync, RecordExportAsync, QueryExportHistoryAsync, CleanupLogsAsync, CleanupCrashesAsync, CleanupExportHistoryAsync) in `Som3a.Infrastructure/Persistence/Interfaces/ILogRepository.cs`
- [x] T028 [US4] Implement `DiagnosticsRepository` (severity/comparison filtering, batched writes, export history queries) in `Som3a.Infrastructure/Persistence/Repositories/DiagnosticsRepository.cs`

**Checkpoint**: User Story 4 is fully functional — errors, crashes, and exports are logged persistently

---

## Phase 7: User Story 5 — Template Management (Priority: P3)

**Goal**: Users can create, save, and query WBS/activity/relationship/productivity templates across sessions

**Independent Test**: Create a WBS template, restart platform, verify template appears in browser with all data intact

### Implementation for User Story 5

- [x] T029 [P] [US5] Create `TemplateRecord` entity class (Id, TemplateType, Name, Category, Content, Version, IsDefault, LastModifiedAt, CreatedAt) in `Som3a.Infrastructure/Persistence/Models/TemplateRecord.cs`
- [x] T030 [US5] Implement `ITemplateRepository` interface (AddTemplateAsync, UpdateTemplateAsync, GetTemplateAsync, GetTemplatesByTypeAsync, GetTemplatesByTypeAndCategoryAsync, GetDefaultTemplatesAsync, DeleteTemplateAsync, SearchTemplatesAsync) in `Som3a.Infrastructure/Persistence/Interfaces/ITemplateRepository.cs`
- [x] T031 [US5] Implement `TemplateRepository` (type+name+category unique constraint, partial match search, version increment on update) in `Som3a.Infrastructure/Persistence/Repositories/TemplateRepository.cs`

**Checkpoint**: User Story 5 is fully functional — templates persist across restarts

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Backup/restore, DPAPI encryption, data retention cleanup, validation engine, corruption detection, composition root registration, contract verification tests

- [x] T032 [P] Create `BackupManifest` entity class (Id, FilePath, FileSizeBytes, Checksum, IncludedTables, PlatformVersion, CreatedAt, IsAutoBackup) in `Som3a.Infrastructure/Persistence/Models/BackupManifest.cs`
- [x] T033 [P] Implement `BackupService` (VACUUM INTO for live backups, PRAGMA integrity_check verification, SHA-256 checksum) in `Som3a.Infrastructure/Persistence/Backup/BackupService.cs`
- [x] T034 [P] Implement `RestoreService` (close connections, replace DB file, re-run migrations, integrity verification) in `Som3a.Infrastructure/Persistence/Backup/RestoreService.cs`
- [x] T035 [P] Implement DPAPI encryption helper for sensitive column values (encrypt/decrypt using `System.Security.Cryptography.ProtectedData`) in `Som3a.Infrastructure/Security/DataProtection.cs`
- [x] T036 [P] Implement `DataRetentionService` (background cleanup at startup, chunked batch deletes of 500 rows per category) in `Som3a.Infrastructure/Persistence/DataRetentionService.cs`
- [x] T037 [P] Implement entity validation engine (GUID non-empty, string max length, enum value validation per data-model.md rules) in `Som3a.Infrastructure/Persistence/Validation/EntityValidator.cs`
- [x] T038 Implement database corruption detection and recovery (PRAGMA integrity_check on startup, rebuild from backup if corrupted) in `Som3a.Infrastructure/Persistence/Diagnostics/DatabaseHealthCheck.cs`
- [x] T039 Register persistence services (DatabaseFactory singleton, UnitOfWork transient, all repositories) in `Som3a.Infrastructure/Configuration/ServiceRegistration.cs` (or update CompositionRoot)
- [x] T040 [P] Set up `Som3a.Infrastructure.Tests` xUnit test project with `Microsoft.Data.Sqlite` reference and in-memory SQLite test helpers in `tests/Som3a.Infrastructure.Tests/`
- [x] T041 [P] Write repository contract verification tests for `SettingsRepository` (upsert, get, delete, categories) using in-memory SQLite in `tests/Som3a.Infrastructure.Tests/Repositories/SettingsRepositoryTests.cs`
- [x] T042 [P] Write repository contract verification tests for `AIRepository` (log execution, query with filters, count, runtime records) using in-memory SQLite in `tests/Som3a.Infrastructure.Tests/Repositories/AIRepositoryTests.cs`
- [x] T043 [P] Write migration integration test (apply all migrations, verify schema version, rollback) using file-based SQLite in `tests/Som3a.Infrastructure.Tests/Migration/MigrationEngineTests.cs`
- [x] T044 [P] Write backup/restore integration test (backup, verify checksum, restore, verify data integrity) using file-based SQLite in `tests/Som3a.Infrastructure.Tests/Backup/BackupServiceTests.cs`
- [x] T045 Update implementation plan, architecture docs, and run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories are INDEPENDENT — can proceed in parallel (US1 and US2 are P1, can be built simultaneously)
  - Sequentially: P1 → P2 → P3 priority order
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1) MVP**: Can start after Foundational — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 3 (P2)**: Can start after Foundational — No dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational — No dependencies on other stories
- **User Story 5 (P3)**: Can start after Foundational — No dependencies on other stories

### Within Each User Story

- Models before services
- Service interfaces before implementations
- Repositories before cross-cutting concerns
- Story complete before moving to next priority

### Parallel Opportunities

- T002 (NuGet package) can run in parallel with T001 (directory structure)
- All Foundational [P] tasks can run in parallel (T004, T005, T009, T010, T011)
- All [P] tasks within a story can run in parallel (models, interfaces)
- All user stories can be worked on in parallel by different team members after Phase 2
- All Polish [P] tasks can run in parallel (backup, DPAPI, retention, validation, tests)

---

## Parallel Example: User Story 1

```bash
# Launch all models and interfaces for User Story 1 together:
Task: "Create SettingsRecord entity in Som3a.Infrastructure/Persistence/Models/SettingsRecord.cs"
Task: "Implement ISettingsRepository interface in Som3a.Infrastructure/Persistence/Interfaces/ISettingsRepository.cs"
```

## Parallel Example: User Story 2

```bash
# Launch all models and interfaces for User Story 2 together:
Task: "Create AIExecutionRecord entity in Som3a.Infrastructure/Persistence/Models/AIExecutionRecord.cs"
Task: "Create AIRuntimeRecord entity in Som3a.Infrastructure/Persistence/Models/AIRuntimeRecord.cs"
Task: "Implement IAIRepository interface in Som3a.Infrastructure/Persistence/Interfaces/IAIRepository.cs"
```

## Parallel Example: All User Stories (after Phase 2)

```bash
# Build all five stories in parallel:
Task: "Phase 3: User Story 1 — Settings Persistence"
Task: "Phase 4: User Story 2 — AI Execution History"
Task: "Phase 5: User Story 3 — Plugin State Management"
Task: "Phase 6: User Story 4 — Diagnostics Logging"
Task: "Phase 7: User Story 5 — Template Management"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Settings Persistence)
4. **STOP and VALIDATE**: Test US1 independently — verify settings survive restart
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (Settings) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (AI History) → Test independently → Deploy/Demo
4. Add User Stories 3+4 (Plugin + Diagnostics) → Test independently → Deploy/Demo
5. Add User Story 5 (Templates) → Test independently → Deploy/Demo
6. Add Polish (Backup, Encryption, Retention, Validation, Tests) → Finalize

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Settings) + User Story 2 (AI History)
   - Developer B: User Story 3 (Plugin) + User Story 4 (Diagnostics)
   - Developer C: User Story 5 (Templates) + Polish tasks
3. Polish phase merges all completed stories with cross-cutting concerns

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Use `Microsoft.Data.Sqlite` for all SQLite access (not System.Data.SQLite)
- All DB operations async with CancellationToken
- GUID primary keys for all entities
- WAL journal mode enabled via DatabaseContext PRAGMAs
- DPAPI encryption handled in separate Security utility, called by relevant repositories
- Data retention cleanup runs as background task after platform startup
