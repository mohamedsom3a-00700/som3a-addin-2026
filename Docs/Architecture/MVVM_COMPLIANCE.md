# MVVM Compliance Checklist

**Project**: Som3a Add-in 2026
**Version**: 1.0.0
**Date**: 2026-05-23

## Audit Criteria

For each view file (.xaml.cs), verify the following PASS/FAIL criteria:

### Data Access

- [ ] PASS — No database queries, file I/O (outside of OpenFileDialog/SaveFileDialog), or network calls in code-behind
- [ ] FAIL — Data access code present in code-behind

### Service Instantiation

- [ ] PASS — No `new ServiceX()` instantiation in code-behind; all services come from `IServiceContainer`
- [ ] FAIL — Direct service instantiation found in code-behind

### Business Rules

- [ ] PASS — No business rules, calculations, or data transformations in code-behind
- [ ] FAIL — Business logic present in code-behind

### Complex Conditionals

- [ ] PASS — No complex if/else chains, switch statements, or loops in code-behind (outside of UI-specific code like focus management)
- [ ] FAIL — Complex conditional logic found in code-behind

### ViewModel Constructor Injection

- [ ] PASS — All ViewModel dependencies received through constructor injection (via `IServiceContainer` or explicit parameters)
- [ ] FAIL — ViewModel instantiates its own dependencies with `new`

### ViewModel UI References

- [ ] PASS — ViewModel does not reference UI types (Window, Control, MessageBox, etc.)
- [ ] FAIL — ViewModel references UI types directly

### ViewModel Resource Dictionary Access

- [ ] PASS — ViewModel does not manipulate ResourceDictionaries
- [ ] FAIL — ViewModel accesses or modifies ResourceDictionaries

### Command Binding

- [ ] PASS — User actions use Command bindings (ICommand) instead of code-behind event handlers where feasible
- [ ] FAIL — Business-action event handlers exist in code-behind instead of ViewModel commands

## Per-File Audit

| File | Data Access | Service New | Business Rules | Complex Logic | DI Constructor | No UI Types | No Resource Dict | Commands | Status |
|------|-------------|-------------|----------------|---------------|----------------|-------------|------------------|----------|--------|
| Float_path.xaml.cs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| AssignTradeCodesWindow.xaml.cs | PASS | PASS | PASS | PASS | FAIL | PASS | PASS | PASS | FAIL |
| LinksManagerWindow.xaml.cs | PASS | FAIL | PASS | PASS | PASS | PASS | PASS | PASS | FAIL |
| SettingsWindow.xaml.cs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| SubDailyReportWindow.xaml.cs | PASS | PASS | PASS | PASS | PASS | FAIL | PASS | PASS | FAIL |
| UnmergeFillDownWindow.xaml.cs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| XerEditorWindow.xaml.cs | PASS | PASS | PASS | PASS | PASS | FAIL | PASS | PASS | FAIL |
| Fixpiecolors.xaml.cs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| MainWindow.xaml.cs | PASS | PASS | PASS | PASS | FAIL | PASS | PASS | FAIL | FAIL |
| StyleSelectorWindow.xaml.cs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS |

## Pass/Fail Rules

- **PASS**: All 8 criteria pass for the file
- **FAIL**: Any criterion fails — file requires refactoring before it is MVVM-compliant

## Audit Results

**Date**: 2026-05-25 | **Pass**: 5/10 | **Fail**: 5/10

### Detailed Violations

| File | Violation | Details |
|------|-----------|---------|
| AssignTradeCodesWindow.xaml.cs | DI Constructor FAIL | `AssignTradeCodesViewModel()` uses parameterless constructor; creates `new AssignTradeCodesService()` inline instead of via container |
| LinksManagerWindow.xaml.cs | Service New FAIL | Code-behind directly instantiates `new LinksManagerService(excelApp)` and `new LinksManagerViewModel(...)` instead of resolving from `IServiceContainer` |
| SubDailyReportWindow.xaml.cs | No UI Types FAIL | `SubDailyReportViewModel` contains WPF visual tree helpers (`FindVisualChild<T>`, `ScrollViewer`, `DependencyObject`) that belong in code-behind |
| XerEditorWindow.xaml.cs | No UI Types FAIL | `XerEditorViewModel` calls `MessageBox.Show(...)` in 8+ locations — UI code in ViewModel |
| MainWindow.xaml.cs | DI Constructor FAIL | `MainViewModel()` uses parameterless constructor; creates `new CompareController()` inline |
| MainWindow.xaml.cs | Commands FAIL | Business-action handlers (`Preview_Click`, `Start_Click`, `Cancel_Click`) are code-behind events instead of Command bindings |

### Top Issues
1. **ViewModel constructor injection missing** (2 files): `AssignTradeCodesViewModel`, `MainViewModel`
2. **UI types in ViewModel** (2 files): `SubDailyReportViewModel`, `XerEditorViewModel`
3. **Direct service instantiation** (1 file): `LinksManagerWindow.xaml.cs`
4. **Code-behind event handlers** (1 file): `MainWindow.xaml.cs`

## Remediation Priority

1. Files with business logic or data access (highest risk)
2. Files with direct service instantiation (medium risk)
3. Files with code-behind event handlers (lowest risk)

## Related Documents

- [MVVM_RULES.md](MVVM_RULES.md) — Full MVVM architecture rules
- [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) — Architecture review checklist
