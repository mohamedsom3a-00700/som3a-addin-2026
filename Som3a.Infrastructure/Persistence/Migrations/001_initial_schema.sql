-- Migration 001: Initial Schema
-- Creates all entity tables for the persistence layer.

CREATE TABLE IF NOT EXISTS SettingsRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    Category        TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Value           TEXT    NOT NULL,
    PluginId        TEXT    NOT NULL DEFAULT '',
    UpdatedAt       TEXT    NOT NULL,
    CreatedAt       TEXT    NOT NULL,
    UNIQUE(Category, Name, PluginId)
);

CREATE INDEX IF NOT EXISTS IX_SettingsRecord_Category_Name ON SettingsRecord(Category, Name);

CREATE TABLE IF NOT EXISTS AIExecutionRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    ProviderName    TEXT    NOT NULL,
    ModelName       TEXT    NOT NULL,
    PromptText      TEXT    NOT NULL,
    ResponseText    TEXT    NOT NULL,
    TokenInput      INTEGER NOT NULL,
    TokenOutput     INTEGER NOT NULL,
    DurationMs      INTEGER NOT NULL,
    Status          TEXT    NOT NULL,
    ErrorMessage    TEXT    NULL,
    RetryCount      INTEGER NOT NULL DEFAULT 0,
    PluginId        TEXT    NOT NULL DEFAULT '',
    ExecutedAt      TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_AIExecutionRecord_ExecutedAt ON AIExecutionRecord(ExecutedAt, ProviderName, Status);

CREATE TABLE IF NOT EXISTS AIRuntimeRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    ProviderName    TEXT    NOT NULL,
    SessionId       TEXT    NOT NULL,
    TokenInputTotal INTEGER NOT NULL DEFAULT 0,
    TokenOutputTotal INTEGER NOT NULL DEFAULT 0,
    OperationCount  INTEGER NOT NULL DEFAULT 0,
    EstimatedCost   REAL    NOT NULL DEFAULT 0.0,
    StartedAt       TEXT    NOT NULL,
    UpdatedAt       TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_AIRuntimeRecord_Session ON AIRuntimeRecord(SessionId, ProviderName);

CREATE TABLE IF NOT EXISTS PluginRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    PluginId        TEXT    NOT NULL UNIQUE,
    Name            TEXT    NOT NULL,
    Version         TEXT    NOT NULL,
    Description     TEXT    NOT NULL DEFAULT '',
    Author          TEXT    NOT NULL DEFAULT '',
    Dependencies    TEXT    NOT NULL DEFAULT '[]',
    IsEnabled       INTEGER NOT NULL DEFAULT 1,
    HealthStatus    TEXT    NOT NULL DEFAULT 'Unknown',
    HealthMessage   TEXT    NULL,
    Settings        TEXT    NOT NULL DEFAULT '{}',
    InstalledAt     TEXT    NOT NULL,
    UpdatedAt       TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_PluginRecord_State ON PluginRecord(IsEnabled, HealthStatus);

CREATE TABLE IF NOT EXISTS PluginVersionRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    PluginRecordId  TEXT    NOT NULL,
    Version         TEXT    NOT NULL,
    InstalledAt     TEXT    NOT NULL,
    InstalledBy     TEXT    NOT NULL DEFAULT 'User',
    FOREIGN KEY (PluginRecordId) REFERENCES PluginRecord(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_PluginVersionRecord_Plugin ON PluginVersionRecord(PluginRecordId, Version);

CREATE TABLE IF NOT EXISTS DiagnosticsLog (
    Id              TEXT    NOT NULL PRIMARY KEY,
    Severity        TEXT    NOT NULL,
    Component       TEXT    NOT NULL,
    Message         TEXT    NOT NULL,
    StackTrace      TEXT    NULL,
    PlatformState   TEXT    NULL,
    LoggedAt        TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_DiagnosticsLog_LoggedAt ON DiagnosticsLog(LoggedAt, Severity, Component);

CREATE TABLE IF NOT EXISTS CrashReport (
    Id                TEXT    NOT NULL PRIMARY KEY,
    LastOperation     TEXT    NOT NULL DEFAULT '',
    MemoryUsageMb     INTEGER NOT NULL DEFAULT 0,
    ThreadState       TEXT    NULL,
    ExcelInteropStatus TEXT   NULL,
    CrashDump         TEXT    NULL,
    LoggedAt          TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS ExportHistoryRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    Format          TEXT    NOT NULL,
    RowCount        INTEGER NOT NULL DEFAULT 0,
    FileSize        INTEGER NOT NULL DEFAULT 0,
    DurationMs      INTEGER NOT NULL DEFAULT 0,
    Status          TEXT    NOT NULL,
    ErrorMessage    TEXT    NULL,
    ExportedAt      TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_ExportHistoryRecord_ExportedAt ON ExportHistoryRecord(ExportedAt, Format);

CREATE TABLE IF NOT EXISTS TemplateRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    TemplateType    TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Category        TEXT    NOT NULL DEFAULT '',
    Content         TEXT    NOT NULL DEFAULT '{}',
    Version         INTEGER NOT NULL DEFAULT 1,
    IsDefault       INTEGER NOT NULL DEFAULT 0,
    LastModifiedAt  TEXT    NOT NULL,
    CreatedAt       TEXT    NOT NULL,
    UNIQUE(TemplateType, Name, Category)
);

CREATE INDEX IF NOT EXISTS IX_TemplateRecord_Type ON TemplateRecord(TemplateType, Category);

CREATE TABLE IF NOT EXISTS BackupManifest (
    Id              TEXT    NOT NULL PRIMARY KEY,
    FilePath        TEXT    NOT NULL,
    FileSizeBytes   INTEGER NOT NULL DEFAULT 0,
    Checksum        TEXT    NOT NULL,
    IncludedTables  TEXT    NOT NULL DEFAULT '[]',
    PlatformVersion TEXT    NOT NULL,
    CreatedAt       TEXT    NOT NULL,
    IsAutoBackup    INTEGER NOT NULL DEFAULT 0
);
