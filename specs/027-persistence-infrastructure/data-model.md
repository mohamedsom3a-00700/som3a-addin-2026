# Data Model: Persistence & Platform Database Infrastructure

**Date**: 2026-05-30

## Entity Overview

All entities use **GUID** as primary key. Timestamps in UTC. Soft deletes via `IsDeleted` flag where applicable.

---

## SettingsRecord

Stores all user-configurable platform settings.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| Category | TEXT(64) | e.g. "Theme", "Accessibility", "Performance", "Excel" |
| Name | TEXT(128) | Setting key within category |
| Value | TEXT | JSON-serialized value |
| PluginId | TEXT? | Null for platform settings; plugin ID for plugin settings |
| UpdatedAt | TEXT (ISO 8601) | |
| CreatedAt | TEXT (ISO 8601) | |

**Unique constraint**: (Category, Name, PluginId)
**Index**: (Category, Name)

---

## AIExecutionRecord

Tracks a single AI operation (prompt execution).

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| ProviderName | TEXT(64) | e.g. "OpenAI", "Claude", "DeepSeek" |
| ModelName | TEXT(64) | e.g. "gpt-4", "claude-3-opus" |
| PromptText | TEXT | Full prompt sent to provider |
| ResponseText | TEXT | Full response from provider |
| TokenInput | INTEGER | Prompt token count |
| TokenOutput | INTEGER | Response token count |
| DurationMs | INTEGER | Execution duration in milliseconds |
| Status | TEXT(16) | "Success", "Failure", "Timeout", "Cancelled" |
| ErrorMessage | TEXT? | Error details if failed |
| RetryCount | INTEGER | Number of retry attempts |
| PluginId | TEXT? | Plugin that initiated the execution |
| ExecutedAt | TEXT (ISO 8601) | |

**Index**: (ExecutedAt, ProviderName, Status)

---

## AIRuntimeRecord

Provider-level token aggregation per session.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| ProviderName | TEXT(64) | |
| SessionId | TEXT(64) | Platform session identifier |
| TokenInputTotal | INTEGER | |
| TokenOutputTotal | INTEGER | |
| OperationCount | INTEGER | Number of operations in session |
| EstimatedCost | REAL | Estimated cost in USD |
| StartedAt | TEXT (ISO 8601) | |
| UpdatedAt | TEXT (ISO 8601) | |

**Index**: (SessionId, ProviderName)

---

## PluginRecord

Installed plugin metadata and runtime state.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| PluginId | TEXT(64) | Unique plugin identifier (from [Plugin] attribute) |
| Name | TEXT(128) | Plugin display name |
| Version | TEXT(16) | Semantic version |
| Description | TEXT | |
| Author | TEXT(128) | |
| Dependencies | TEXT | JSON array of plugin ID + version constraints |
| IsEnabled | INTEGER (bool) | Enable/disable state |
| HealthStatus | TEXT(16) | "Healthy", "Degraded", "Unhealthy", "Unknown" |
| HealthMessage | TEXT? | Last health check message |
| Settings | TEXT | JSON blob of plugin-specific settings |
| InstalledAt | TEXT (ISO 8601) | |
| UpdatedAt | TEXT (ISO 8601) | |

**Unique constraint**: PluginId
**Index**: (IsEnabled, HealthStatus)

---

## PluginVersionRecord

Version history for plugin updates.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| PluginRecordId | GUID (FK → PluginRecord.Id) | |
| Version | TEXT(16) | |
| InstalledAt | TEXT (ISO 8601) | |
| InstalledBy | TEXT | "AutoUpdate", "User", "Admin" |

**Index**: (PluginRecordId, Version)

---

## DiagnosticsLog

Application error and event log entries.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| Severity | TEXT(16) | "Debug", "Info", "Warning", "Error", "Fatal" |
| Component | TEXT(64) | Source component name |
| Message | TEXT | |
| StackTrace | TEXT? | |
| PlatformState | TEXT? | JSON snapshot of platform state |
| LoggedAt | TEXT (ISO 8601) | |

**Index**: (LoggedAt, Severity, Component)

---

## CrashReport

Unexpected termination records.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| LastOperation | TEXT(128) | Operation running at crash time |
| MemoryUsageMb | INTEGER | |
| ThreadState | TEXT? | JSON of thread state |
| ExcelInteropStatus | TEXT? | Excel COM status at crash |
| CrashDump | TEXT? | Path to crash dump file |
| LoggedAt | TEXT (ISO 8601) | |

---

## ExportHistoryRecord

Completed export operation metadata.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| Format | TEXT(16) | "Excel", "CSV", "JSON", "XML", "Primavera" |
| RowCount | INTEGER | |
| FileSize | INTEGER | Bytes |
| DurationMs | INTEGER | |
| Status | TEXT(16) | "Success", "Partial", "Failure" |
| ErrorMessage | TEXT? | |
| ExportedAt | TEXT (ISO 8601) | |

**Index**: (ExportedAt, Format)

---

## TemplateRecord

Shared template entity for WBS, activities, relationships, and productivity benchmarks.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| TemplateType | TEXT(16) | "WBS", "Activity", "Relationship", "ProductivityBenchmark" |
| Name | TEXT(128) | |
| Category | TEXT(64) | e.g. "Residential", "Commercial", "Industrial" |
| Content | TEXT | JSON structured template content |
| Version | INTEGER | Monotonically increasing |
| IsDefault | INTEGER (bool) | Built-in template flag |
| LastModifiedAt | TEXT (ISO 8601) | |
| CreatedAt | TEXT (ISO 8601) | |

**Unique constraint**: (TemplateType, Name, Category)
**Index**: (TemplateType, Category)

---

## BackupManifest

Backup operation metadata.

| Field | Type | Notes |
|---|---|---|
| Id | GUID (PK) | |
| FilePath | TEXT | Absolute path to backup DB file |
| FileSizeBytes | INTEGER | |
| Checksum | TEXT | SHA-256 hash of backup file |
| IncludedTables | TEXT | JSON array of table names |
| PlatformVersion | TEXT(16) | Platform version at backup time |
| CreatedAt | TEXT (ISO 8601) | |
| IsAutoBackup | INTEGER (bool) | Automatic (scheduled) vs manual |

---

## Entity Relationship Diagram

```
SettingsRecord ────── (standalone, no FK dependencies)

AIExecutionRecord ─── (standalone)
AIRuntimeRecord ───── (standalone)

PluginRecord ───────── 1 ──── * ──── PluginVersionRecord

DiagnosticsLog ─────── (standalone)
CrashReport ────────── (standalone)
ExportHistoryRecord ── (standalone)

TemplateRecord ─────── (standalone)

BackupManifest ─────── (standalone, metadata only)
```

## Validation Rules

| Entity | Rule |
|---|---|
| All | Id must be non-empty GUID |
| All | Timestamp fields must be valid ISO 8601 UTC |
| SettingsRecord | Category max 64 chars, Name max 128 chars |
| AIExecutionRecord | DurationMs >= 0, TokenInput >= 0, TokenOutput >= 0 |
| PluginRecord | PluginId max 64 chars, Name max 128 chars, Version must be semver |
| DiagnosticsLog | Severity one of: Debug, Info, Warning, Error, Fatal |
| TemplateRecord | TemplateType one of: WBS, Activity, Relationship, ProductivityBenchmark |
| BackupManifest | FilePath must be absolute, Checksum must be 64-char hex |

## Data Retention Rules

| Entity | Retention | Cleanup Trigger |
|---|---|---|
| AIExecutionRecord | 1 year | Startup background job |
| AIRuntimeRecord | 1 year | Startup background job |
| DiagnosticsLog | 90 days | Startup background job |
| CrashReport | 2 years | Startup background job |
| ExportHistoryRecord | 1 year | Startup background job |
| SettingsRecord | Indefinite | Never auto-deleted |
| PluginRecord | Indefinite | Never auto-deleted |
| PluginVersionRecord | Indefinite | Never auto-deleted |
| TemplateRecord | Indefinite | Never auto-deleted |
| BackupManifest | Indefinite | Never auto-deleted |
