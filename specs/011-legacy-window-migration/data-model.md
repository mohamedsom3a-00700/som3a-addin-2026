# Data Model: Legacy Window Migration

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-25

## Entities

### MigrationRecord

Tracks the migration status of each window from standalone Window to shell Page.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| WindowName | String | Canonical name of the window | Must match the original `.xaml` file name without extension. Unique across all records. |
| OriginalPath | String | Relative path to original standalone XAML | Path from `WpfApp2/` root, e.g., `MainWindow.xaml` or `Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml`. |
| PagePath | String | Relative path to migrated Page | Path in `WpfApp2/Pages/`, e.g., `MainPage.xaml`. Null if not yet created. |
| MigrationStatus | Enum | Current state | Values: `NotStarted`, `InProgress`, `Validated`, `RolledBack`. |
| PageKey | String | NavigationService key for this Page | Must be unique. Format: lowercase-with-hyphens, e.g., `main`, `settings`. |
| Priority | Integer | Migration order priority | Lower = migrate first. Range 1–14. Unique. |
| Complexity | Enum | Migration complexity assessment | Values: `Low`, `Medium`, `High`. |
| ValidatedBy | String | Name of reviewer who validated | Null if not validated. |
| ValidatedDate | DateTime | Date of validation | Null if not validated. |
| Notes | String | Free-form notes | Optional. Use for blocking issues, rollback reasons, or special considerations. |

**Relationships**:
- One `MigrationRecord` per window.
- `PageKey` maps to `NavigationPage.Key` registered in `NavigationService`.
- `OriginalPath` must remain valid until `MigrationStatus` reaches `Validated`.

**Validation Rules**:
- `WindowName` must be unique across all records.
- `PageKey` must be unique across all registered pages.
- `MigrationStatus` transitions: `NotStarted` → `InProgress` → `Validated` OR `NotStarted` → `InProgress` → `RolledBack`.
- `Validated` records cannot transition to `RolledBack`.
- `OriginalPath` cannot be deleted while `MigrationStatus` is not `Validated`.

**Lifecycle**:
1. Record created with `MigrationStatus = NotStarted`, `Priority` assigned.
2. Page creation begins → `MigrationStatus = InProgress`.
3. Page validated in Excel VSTO → `MigrationStatus = Validated`, `ValidatedBy` and `ValidatedDate` set.
4. Validation fails → `MigrationStatus = RolledBack`, `Notes` document issue.
5. After rollback, developer can set `MigrationStatus = InProgress` again to retry.

---

### WindowInventory

Static catalog of all windows that are candidates for migration.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| TotalCount | Integer | Total windows to migrate | Fixed at 14. |
| CompletedCount | Integer | Windows with `Validated` status | Range 0–14. |
| InProgressCount | Integer | Windows with `InProgress` status | Range 0–14. |
| RemainingCount | Integer | Windows not yet started | Computed: `TotalCount - CompletedCount - InProgressCount`. |

**Purpose**: Provides at-a-glance migration progress without enumerating all records.

---

### ValidationChecklistItem

Individual validation criterion for a migrated Page.

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| CheckItemId | String | Unique identifier | Format: `WindowName-Check-N`. |
| WindowName | String | Foreign key to MigrationRecord | Must reference a valid MigrationRecord. |
| Criterion | String | Human-readable validation criterion | Max 200 characters. |
| Status | Enum | Pass/Fail/NotTested | Default: `NotTested`. |
| TestedBy | String | Name of tester | Null if not tested. |
| TestedDate | DateTime | Date of test | Null if not tested. |
| Evidence | String | Notes or screenshot reference | Optional. |

**Validation Criteria by Window** (standard items):

| Criterion | Description |
|-----------|-------------|
| Page opens in shell | Page loads without blank/black backgrounds |
| DPI 100% | All elements render correctly at 100% DPI |
| DPI 125% | All elements scale correctly at 125% DPI |
| DPI 150% | All elements scale correctly at 150% DPI |
| Theme switch | Page updates when theme changes (Dark/Light/Custom) |
| Popup rendering | ComboBox and similar popups render without clipping |
| Keyboard nav | Tab, Enter, Escape work correctly |
| FallbackSafe | Page renders correctly when FallbackSafe mode active |
| Functional parity | All operations produce identical results to standalone |

---

## State Transitions

### MigrationRecord Lifecycle

```text
[NotStarted] --start--> [InProgress] --validate pass--> [Validated]
                           |                               |
                           |---validate fail--> [RolledBack]
                           |                               |
                           <--------retry--------(back to InProgress)
```

### Validation Checklist Workflow

```text
[Created with window] --> [NotTested] --test pass--> [Pass]
                              |                        |
                              |--test fail--> [Fail] <--retry--
```

## MigrationRecord Table (All 14 Windows)

| # | WindowName | OriginalPath | PagePath | MigrationStatus | PageKey | Priority | Complexity |
|---|-----------|-------------|----------|----------------|---------|----------|------------|
| 1 | SettingsWindow | Views/SettingsWindow.xaml | Pages/SettingsPage.xaml | NotStarted | settings | 1 | Medium |
| 2 | MainWindow | MainWindow.xaml | Pages/MainPage.xaml | NotStarted | main | 2 | High |
| 3 | ProjectAnalysisWindow | ProjectAnalysisWindow.xaml | Pages/ProjectAnalysisPage.xaml | NotStarted | project-analysis | 3 | High |
| 4 | Float_path | Float_path.xaml | Pages/FloatPathPage.xaml | NotStarted | float-path | 4 | Medium |
| 5 | LinksManagerWindow | LinksManagerWindow.xaml | Pages/LinksManagerPage.xaml | NotStarted | links-manager | 5 | Medium |
| 6 | SubDailyReportWindow | SubDailyReportWindow.xaml | Pages/SubDailyReportPage.xaml | NotStarted | sub-daily-report | 6 | Medium |
| 7 | AssignTradeCodesWindow | AssignTradeCodesWindow.xaml | Pages/AssignTradeCodesPage.xaml | NotStarted | assign-trade-codes | 7 | Medium |
| 8 | PrimaveraCompareWindow | Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml | Pages/PrimaveraComparePage.xaml | NotStarted | primavera-compare | 8 | High |
| 9 | PrimaveraResultsWindow | Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml | Pages/PrimaveraResultsPage.xaml | NotStarted | primavera-results | 9 | High |
| 10 | XerEditorWindow | XerEditorWindow.xaml | Pages/XerEditorPage.xaml | NotStarted | xer-editor | 10 | High |
| 11 | Fixpiecolors | Fixpiecolors.xaml | Pages/FixPieColorsPage.xaml | NotStarted | fix-pie-colors | 11 | Low |
| 12 | StyleSelectorWindow | StyleSelectorWindow.xaml | Pages/StyleSelectorPage.xaml | NotStarted | style-selector | 12 | Low |
| 13 | UnmergeFillDownWindow | UnmergeFillDownWindow.xaml | Pages/UnmergeFillDownPage.xaml | NotStarted | unmerge-fill-down | 13 | Low |
| 14 | ToastWindow | Controls/Toast/ToastWindow.xaml | Pages/ToastPage.xaml | NotStarted | toast | 14 | Low |

## ValidationChecklistItem Entries (Per Window)

Standard validation criteria applied to each window:

| Criterion | CheckItemId Pattern |
|-----------|-------------------|
| Page opens in shell without black/blank backgrounds | `{WindowName}-Check-001` |
| DPI 100%: All elements render correctly | `{WindowName}-Check-002` |
| DPI 125%: All elements scale correctly | `{WindowName}-Check-003` |
| DPI 150%: All elements scale correctly | `{WindowName}-Check-004` |
| Theme switch: Page updates when theme changes | `{WindowName}-Check-005` |
| Popup rendering: ComboBox/popups render without clipping | `{WindowName}-Check-006` |
| Keyboard navigation: Tab, Enter, Escape work | `{WindowName}-Check-007` |
| FallbackSafe mode: Page renders correctly | `{WindowName}-Check-008` |
| Functional parity: Operations produce identical results | `{WindowName}-Check-009` |

Total validation items: 14 windows x 9 criteria = 126 items.

## Summary

The migration data model tracks each window individually through the migration pipeline. The `MigrationRecord` is the primary entity; `ValidationChecklistItem` provides granular pass/fail tracking per validation criterion. Progress is measured via `WindowInventory` aggregate counts.