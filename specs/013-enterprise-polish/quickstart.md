# Quickstart: Enterprise Polish

**Feature**: Enterprise Polish (Phase 10)
**Spec**: [spec.md](spec.md) | **Tasks**: [tasks.md](tasks.md)

## Prerequisites

- [ ] Phase 4 (Control Standardization) complete
- [ ] Phase 5 (Navigation Shell) complete
- [ ] Phase 6 (MVVM Cleanup) complete
- [ ] Phase 9 (Plugin Platform) complete
- [ ] Repository on `013-enterprise-polish` branch
- [ ] `msbuild` succeeds (no errors)

## Recommended Execution Order

### Step 1: Establish Baselines (1-2 days)

Run baseline measurements BEFORE making any changes:

```powershell
# Performance baseline (T001)
# Use Visual Studio Diagnostic Tools or PerfView
# Measure: startup time, memory, DataGrid scroll fps, theme switch time

# Accessibility baseline (T002)
# Run Windows Accessibility Insights on each window
# Run contrast checker on all themes

# DPI baseline (T003)
# Set display to 100%, test all windows
# Set display to 125%, test all windows
# Set display to 150%, test all windows
# Set display to 200%, test all windows
# Set up multi-monitor mixed DPI (if available)

# Hardcoded value sweep (T005)
powershell.exe -Command "Get-ChildItem -Path 'WpfApp2' -Recurse -Filter '*.xaml' | Select-String -Pattern '#[0-9A-Fa-f]{6,8}', 'CornerRadius=\"[0-9]', 'Margin=\"[0-9]', 'DropShadowEffect' | Group-Object Path"
```

### Step 2: Technical Debt Cleanup (3-5 days)

Fix quick wins first:

1. Migrate ToastWindow to ModernWindow (T009)
2. Fix all hardcoded values found in sweep (T010-T012)
3. Fix ValidationEngine TODO (T013)
4. Fill MVVM_COMPLIANCE.md (T014)
5. Run dispose/event leak audits (T015-T017)

### Step 3: Performance + Accessibility + DPI (parallel, 5-10 days)

These three workstreams can run in parallel:

```powershell
# Performance (Phase 3)
# Implement Freeze() on brushes/transforms
# Optimize resource dictionaries
# Validate animation budgets
# Add reduced-motion support

# Accessibility (Phase 4)
# Fix keyboard navigation gaps
# Add AutomationProperties
# Fix focus indicators
# Validate contrast ratios
# Implement high contrast mode

# DPI (Phase 5)
# Fix clipping/overflow at all DPI levels
# Implement per-monitor DPI popup positioning
# Test multi-monitor mixed DPI
# Document DPI regression checklist
```

### Step 4: Excel Stability Validation (3-5 days)

Close all 15 Phase 3 validation tasks:

```powershell
# Launch Excel, load add-in
# Test each of the 14 windows
# Test rapid theme switching (10x)
# Test safe mode (force software rendering)
# Run DataGrid 10k row scroll test
# Run 2-hour memory stability test
```

### Step 5: Diagnostics Finalization (2-3 days)

Hardening:
- Validate DiagnosticsService snapshots
- Validate LoggingService rotation
- Extend ValidationEngine scanning
- Polish diagnostics panel UX

### Step 6: Documentation (2-3 days)

Create all 6 audit reports:
- `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md`
- `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md`
- `Docs/Architecture/DPI_AUDIT_REPORT.md`
- `Docs/Architecture/EXCEL_STABILITY_REPORT.md`
- `Docs/Architecture/LOCALIZATION_READINESS.md`
- `Docs/Architecture/ENTERPRISE_POLISH_CHECKLIST.md`

### Step 7: Production Gate (1 day)

```powershell
# Final build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Excel host test
# Verify all acceptance criteria met in ENTERPRISE_POLISH_CHECKLIST.md

# Constitution compliance review
```

## Key Commands

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Run accessibility baseline (Windows Accessibility Insights)
# Command: accessibilityinsights.exe --quiet --config accessibility-config.json

# Run hardcoded value grep
Get-ChildItem -Path "WpfApp2" -Recurse -Filter "*.xaml" | Select-String -Pattern "#[0-9A-Fa-f]{6,8}" | Where-Object { $_.Filename -notmatch "Base\\(Colors|Spacing|Radius)" }
```

## Validation Checklist Gates

| Gate | Criteria | When |
|------|----------|------|
| Baselines captured | All T001-T008 complete | End of Phase 1 |
| Tech debt clean | T009-T017 all complete | End of Phase 2 |
| Performance optimized | Startup <1s, memory stable, animations <=200ms | End of Phase 3 |
| Accessible | 100% keyboard nav, AutomationProperties, WCAG AA | End of Phase 4 |
| DPI correct | No clipping/overflow at any DPI, multi-monitor OK | End of Phase 5 |
| Excel stable | All Phase 3 tasks closed, 2h memory stable | End of Phase 6 |
| Diagnostics validated | All services verified, panels polished | End of Phase 7 |
| Documents complete | All 6 reports created | End of Phase 8 |
| Production gate | Build passes, host test passes, checklists green | Final |
