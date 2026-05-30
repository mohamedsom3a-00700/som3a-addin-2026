# Research: Persistence & Platform Database Infrastructure

**Date**: 2026-05-30

## Technology Decisions

### Decision 1: SQLite Provider — Microsoft.Data.Sqlite

- **Decision**: Use `Microsoft.Data.Sqlite` for .NET 8.0 persistence library
- **Rationale**: Officially maintained by Microsoft .NET team, clean modern API, full async support, cross-platform, primary EF Core provider. `System.Data.SQLite` is legacy-focused and adds unnecessary complexity for new code.
- **Alternatives considered**: System.Data.SQLite (legacy, .NET Framework focus)

### Decision 2: Journal Mode — WAL (Write-Ahead Logging)

- **Decision**: Enable WAL mode unconditionally (`PRAGMA journal_mode=WAL`)
- **Rationale**: Allows concurrent reads during writes — critical for multi-plugin architecture. Eliminates SQLITE_BUSY errors under normal load. 4-10x read latency improvement over DELETE mode.
- **Supporting PRAGMAs**: `synchronous=NORMAL` (safe with WAL, 40% less write overhead), `busy_timeout=5000` (5-second wait before busy), `temp_store=MEMORY`
- **Alternatives considered**: DELETE journal mode (blocks readers during writes)

### Decision 3: Connection Management — Reader/Writer Split

- **Decision**: One long-lived write connection + on-demand read connections (no pool library)
- **Rationale**: SQLite serializes writes at DB level regardless of connection count. Single writer + WAL readers is the simplest proven pattern. No heavyweight pooling library needed for a single-user desktop app.
- **Alternatives considered**: Full connection pool (overkill for single-user), single shared connection with lock (creates unnecessary read contention)

### Decision 4: Encryption — DPAPI at Application Level

- **Decision**: Use `System.Security.Cryptography.ProtectedData` (DPAPI) for encrypting sensitive column values
- **Rationale**: Simplest Windows-native encryption — key tied to Windows user account, zero developer key management. Fine-grained column-level encryption. No SQLite extension dependencies.
- **Alternatives considered**: SQLitePCLRaw.bundle_e_sqlite3mc full-DB encryption (cross-platform but more complex), SEE (commercial, overkill), AES-GCM with custom key management (more complexity for marginal gain)

### Decision 5: Migration Strategy — PRAGMA user_version with Numbered SQL Scripts

- **Decision**: Use `PRAGMA user_version` stored in database header + numbered migration SQL files
- **Rationale**: Simplest auditable approach for raw ADO.NET. Version tracked in DB header. Each migration is an atomic step within a transaction. Backup before migration for safety net.
- **Alternatives considered**: EF Core Code-First migrations (adds EF dependency), FluentMigrator (third-party library)

### Decision 6: Backup Strategy — VACUUM INTO

- **Decision**: Use `VACUUM INTO 'backup.db'` for live backups
- **Rationale**: Safe for in-use databases (SQLite 3.27.0+), yields compacted output, accessible from C# via direct SQL execution. Verify with `PRAGMA integrity_check`.
- **Alternatives considered**: `.backup` CLI command (not directly accessible from C#), file copy (unsafe for live DB), SQL dump (too slow)

### Decision 7: Write Serialization — Centralized Write Queue + WAL

- **Decision**: Use `SemaphoreSlim(1,1)` write gate with WAL for concurrent reads
- **Rationale**: All plugin writes serialize through a single gate eliminating SQLITE_BUSY. WAL mode allows reads to bypass the gate. Unit of Work internally routes through the queue.
- **Alternatives considered**: Per-plugin databases (complex cross-plugin queries impossible), busy timeout only (unpredictable retries)

### Decision 8: Data Retention — Startup Background Cleanup

- **Decision**: Run per-category retention cleanup in background task after main window renders
- **Rationale**: No UI freeze, no timer complexity, cleanup runs during user's natural startup flow. Batch delete in 500-row chunks to avoid long transactions.
- **Alternatives considered**: Scheduled timer (extra complexity), lazy cleanup on write (unpredictable pauses), startup blocking cleanup (delays perceived startup)

### Decision 9: Repository Pattern — Generic Base + Specific Extensions

- **Decision**: Generic `IRepository<T>` for CRUD entities, specific interfaces for domain-heavy entities
- **Rationale**: ~80% of entities need simple CRUD (settings, logs, templates). Domain entities (AI executions, plugins) need custom query methods. Avoid IQueryable leaking persistence concerns.
- **Alternatives considered**: Pure generic (loses type safety for domain queries), pure specific (too much boilerplate)

### Decision 10: Testing — In-Memory SQLite for Unit Tests

- **Decision**: Use `:memory:` SQLite connection string for repository unit tests; file-based for migration/concurrency integration tests
- **Rationale**: In-memory SQLite uses the same engine as production — real SQL semantics, constraint checking, transaction behavior. Much faster than file-based. Mock-based tests only for service orchestration.
- **Alternatives considered**: Mocking frameworks (brittle, don't test SQL), EF Core InMemory provider (not real SQL — different semantics)

### Decision 11: Entity ID — GUIDs for All Entities

- **Decision**: All database entities use GUID as primary key
- **Rationale**: No ID collision risk across plugins creating records independently. Future cloud sync readiness (stated goal). Storage overhead (~16 bytes) negligible for sub-500MB DB target.
- **Alternatives considered**: Auto-increment integers (simpler, faster, not sync-ready), hybrid (complexity of two systems)

## Dependency Analysis

| Dependency | Version | Purpose | Risk |
|---|---|---|---|
| Microsoft.Data.Sqlite | 8.0+ | SQLite data access | Low — Microsoft maintained |
| System.Text.Json | Built-in | Structured content serialization | None |
| SQLitePCLRaw.bundle_e_sqlite3mc | (if full-DB encryption needed) | Encryption extension | Low |

## Integration Patterns

| Integration | Pattern | Notes |
|---|---|---|
| Settings service → SettingsRepository | Constructor injection via DI | Settings service calls repository for CRUD |
| PluginHost → PluginRepository | Constructor injection | Plugin state loaded at startup via PluginHost |
| AIOrchestrator → AIRepository | Async callback | AI execution completes → Fire-and-forget persist |
| DiagnosticsService → ILogRepository | Queue + batch | Diagnostics batched every 5s to avoid write pressure |
| BackupService → All repositories | Unit of Work | Full DB backup via VACUUM INTO |
