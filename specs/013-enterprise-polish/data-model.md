# Data Model: Enterprise Polish

**Branch**: `013-enterprise-polish` | **Date**: 2026-05-25

## Overview

Enterprise Polish is an optimization and validation phase — no new persistent data entities are introduced. Data models defined here describe artifacts produced and consumed during the phase.

## PerformanceBaseline

| Field | Type | Description |
|-------|------|-------------|
| WindowName | string | Name of window tested (e.g., "MainWindow", "SettingsWindow") |
| StartupTimeMs | int | Time from user action to fully rendered window (ms) |
| MemoryBaselineMB | float | Private working set after window load (MB) |
| MemoryAfter2hMB | float | Private working set after 2h of typical use (MB) |
| DataGridScrollFps | float | Frames per second during continuous DataGrid scrolling (10k rows) |
| ThemeSwitchTimeMs | int | Time for all windows to update after theme switch (ms) |
| AnimationDurations | Dictionary<string, int> | Measured duration per named animation storyboard (ms) |
| TestDate | DateTime | When baseline was captured |
| HardwareConfig | string | Description of test machine (CPU, RAM, GPU, OS) |

## AccessibilityViolation

| Field | Type | Description |
|-------|------|-------------|
| WindowName | string | Window containing the violation |
| ElementType | string | WPF control type (Button, TextBox, DataGrid, etc.) |
| ViolationType | enum | KeyboardNav, FocusVisibility, AutomationProperty, ContrastRatio, ReducedMotion, HighContrast |
| Severity | enum | Critical, Major, Minor |
| Description | string | Human-readable description of the violation |
| ExpectedValue | string | What should be present (e.g., "FocusVisualStyle with Glow.Focus") |
| ActualValue | string | What was found (e.g., "No FocusVisualStyle set") |
| Remediation | string | Steps to fix |
| Fixed | bool | Whether the violation has been resolved |

## DpiTestResult

| Field | Type | Description |
|-------|------|-------------|
| WindowName | string | Window tested |
| DpiScale | enum | 100%, 125%, 150%, 200% |
| DisplayConfig | enum | SingleMonitor, MultiMonitorSameDPI, MultiMonitorMixedDPI |
| ClippingIssues | string[] | List of elements that were clipped |
| OverflowIssues | string[] | List of elements that overflowed their container |
| MisalignmentIssues | string[] | List of elements that were misaligned |
| TextRenderingIssues | string[] | List of text elements with rendering problems |
| PopupIssues | string[] | List of popups with positioning/sizing problems |
| Pass | bool | Whether this configuration passed all checks |

## Phase3ValidationTask

| Field | Type | Description |
|-------|------|-------------|
| TaskId | string | e.g., "WS-A T022" |
| Description | string | Task description |
| Status | enum | NotStarted, InProgress, Passed, Failed |
| FailureReason | string | If Failed, why |
| ResolvedInPhase10 | bool | Whether Phase 10 addresses this task |
| ResolutionTaskId | string | References the Phase 10 task (e.g., "T039") |

## ValidationEngineExtension

| Field | Type | Description |
|-------|------|-------------|
| ScanType | enum | InlineColor, MissingToken, DuplicateStyle, InvalidResource |
| FilePattern | string | Glob pattern for files to scan (e.g., "*.xaml") |
| ScannerImplementation | string | Method name implementing the scan |
| TokenChecklist | string[] | If ScanType is MissingToken, the list of required tokens |
| KnownFalsePositives | string[] | Patterns known to trigger false positives (for filtering) |

## AuditReport

| Field | Type | Description |
|-------|------|-------------|
| ReportName | string | e.g., "PERFORMANCE_AUDIT_REPORT" |
| TargetPath | string | `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md` |
| CreatedDate | DateTime | When the report was generated |
| BaselineReference | string | Pointer to baseline test configuration |
| Findings | string[] | Key findings and measurements |
| Recommendations | string[] | Optimization recommendations (even if not implemented) |
| AcceptanceCriteriaMet | bool[] | Which Phase 10 acceptance criteria this report supports |

## EnterprisePolishChecklist

| Field | Type | Description |
|-------|------|-------------|
| AcceptanceCriterionId | string | e.g., "SC-001" |
| Description | string | Criterion description |
| RequiredTasks | string[] | References to Phase 10 tasks that satisfy this criterion |
| PassStatus | enum | NotStarted, InProgress, Passed, Failed |
| Notes | string | Additional context or remedial actions needed |
