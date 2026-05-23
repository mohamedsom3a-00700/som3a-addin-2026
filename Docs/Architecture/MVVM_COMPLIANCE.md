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
| Float_path.xaml.cs | | | | | | | | | |
| AssignTradeCodesWindow.xaml.cs | | | | | | | | | |
| LinksManagerWindow.xaml.cs | | | | | | | | | |
| SettingsWindow.xaml.cs | | | | | | | | | |
| SubDailyReportWindow.xaml.cs | | | | | | | | | |
| UnmergeFillDownWindow.xaml.cs | | | | | | | | | |
| XerEditorWindow.xaml.cs | | | | | | | | | |
| Fixpiecolors.xaml.cs | | | | | | | | | |
| MainWindow.xaml.cs | | | | | | | | | |
| StyleSelectorWindow.xaml.cs | | | | | | | | | |

## Pass/Fail Rules

- **PASS**: All 8 criteria pass for the file
- **FAIL**: Any criterion fails — file requires refactoring before it is MVVM-compliant

## Remediation Priority

1. Files with business logic or data access (highest risk)
2. Files with direct service instantiation (medium risk)
3. Files with code-behind event handlers (lowest risk)

## Related Documents

- [MVVM_RULES.md](MVVM_RULES.md) — Full MVVM architecture rules
- [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) — Architecture review checklist
