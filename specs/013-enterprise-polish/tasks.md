# Tasks: Enterprise Polish

**Input**: Design documents from `/specs/013-enterprise-polish/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Manual testing required — no automated test framework specified for validation tasks.

**Organization**: Tasks are grouped by workstream (WS-A through WS-G) to enable parallel execution where possible.

## Path Conventions

- **Project**: `WpfApp2/` — single WPF application project
- **Audit reports**: `Docs/Architecture/`
- **Existing directories**: `WpfApp2/Services/`, `WpfApp2/Controls/`, `WpfApp2/Theme/`
- **Existing files modified**: `WpfApp2/Controls/ToastWindow.xaml`, `WpfApp2/Services/ValidationEngine.cs`, `AGENTS.md`

---

## Phase 1: Setup & Baselines

**Purpose**: Establish baseline measurements and catalog all issues before making changes.

- [ ] T001 Run performance baseline on reference PC — measure startup time, memory usage, DataGrid scroll fps, theme switch time, animation durations for all 14 windows
- [ ] T002 Run accessibility baseline — catalog keyboard navigation gaps, missing AutomationProperties, focus indicator visibility, contrast ratios in all 3 themes
- [ ] T003 Run DPI baseline — test all 14 windows at 100%, 125%, 150%, 200% DPI; test multi-monitor mixed DPI if available; catalog clipping/overflow/misalignment
- [ ] T004 Run Excel VSTO baseline — execute Phase 3 T057–T062 validation tasks; document pass/fail for each
- [ ] T005 Run hardcoded-value sweep — regex scan across all `.xaml` files; catalog `#HEX` colors, inline effects, hardcoded margins outside `Theme/Base/`
- [ ] T006 Review ValidationEngine.cs TODO — design proper XAML/dictionary resource resolver
- [ ] T007 Review ToastWindow.xaml — plan migration to `controls:ModernWindow`
- [ ] T008 Run freezable audit — search all `.xaml` for `SolidColorBrush`, `LinearGradientBrush`, `Pen`, `Transform` resources that can be frozen

---

## Phase 2: Technical Debt Cleanup (WS-E)

**Purpose**: Quick cleanup items that unblock other workstreams.

- [ ] T009 Migrate ToastWindow.xaml from plain `<Window>` to `controls:ModernWindow` — inherit ModernWindow, apply DynamicResource backgrounds, add SnapsToDevicePixels/UseLayoutRounding, verify all themes
- [ ] T010 Fix all hardcoded `#HEX` values found in T005 — replace with existing semantic tokens or create new tokens in `Theme/Base/Colors.xaml`
- [ ] T011 Fix all inline effects found in T005 — replace with `{DynamicResource Shadow.*}` or `{DynamicResource Glow.*}` references
- [ ] T012 Replace all hardcoded margins/padding found in T005 with `{DynamicResource Spacing.*}` or `{DynamicResource Padding.*}` tokens
- [ ] T013 Fix ValidationEngine TODO — implement proper XAML/dictionary resource resolver to scan ALL `.xaml` files (not just known locations)
- [ ] T014 Fill MVVM_COMPLIANCE.md per-file audit table — assess all 10 files for pass/fail status with specific violations where applicable
- [ ] T015 Run dispose pattern audit — ensure all services with unmanaged resources implement `IDisposable`; verify Dispose() is called in CompositionRoot teardown
- [ ] T016 Run event handler unsubscription audit — search for `+=` event subscriptions that lack corresponding `-=` unsubscription; fix all leaks found
- [ ] T017 Run static event cleanup — identify and remove any static event handlers on services that prevent garbage collection

---

## Phase 3: Performance Hardening (WS-A)

**Purpose**: Optimize memory, rendering, startup, and animations.

- [ ] T018 Implement `Freeze()` on all freezable resources — scan `Theme/` for `SolidColorBrush`, `LinearGradientBrush`, `Pen`, `Transform`, `GeometryDrawing` definitions; call `.Freeze()` after creation; validate at runtime
- [ ] T019 Optimize resource dictionaries — audit `MergedDictionaries` across all windows and `ThemeResources.xaml`; remove orphaned/unreferenced dictionaries; deduplicate any duplicate loads
- [ ] T020 Optimize startup — identify and defer non-critical initialization to background/async; measure improvement against T001 baseline
- [ ] T021 Validate all animations <=200ms — check every `Storyboard.Duration` in `Theme/Effects/Animations.xaml` and `Theme/WindowAnimations.xaml`; fix any exceeding budget
- [ ] T022 Implement reduced-motion support — wrap all storyboards in condition that checks system "Reduce motion" setting; when enabled, skip animations and show content immediately
- [ ] T023 Optimize rendering — profile layout passes per window; simplify deep visual trees; verify `SnapsToDevicePixels`/`UseLayoutRounding` on all controls
- [ ] T024 Re-measure performance after optimization — compare startup time, memory, DataGrid scroll fps, theme switch time against T001 baseline; document improvements

---

## Phase 4: Accessibility Compliance (WS-B)

**Purpose**: Ensure full keyboard accessibility, focus visibility, screen reader support, and contrast compliance.

- [ ] T025 Fix all keyboard navigation gaps found in T002 — repair Tab order, ensure all elements are `IsTabStop`/`Focusable` correctly, test full Tab cycle per window
- [ ] T026 Add `AutomationProperties.Name` to all interactive elements missing it — use meaningful descriptions, not technical names
- [ ] T027 Add `AutomationProperties.HelpText` and/or `AutomationProperties.LabeledBy` to complex controls (ComboBox, DataGrid, TreeView)
- [ ] T028 Fix focus indicator visibility — ensure `FocusVisualStyle` using `{DynamicResource Glow.Focus}` is applied to all interactive elements; verify focus is never lost (no invisible focus traps)
- [ ] T029 Validate and fix WCAG 2.1 AA contrast (4.5:1) — measure all text/background combinations in Dark, Light, and 2 Custom accent themes; adjust token values where needed
- [ ] T030 Implement high contrast mode awareness — detect system high contrast mode; fall back to system colors for backgrounds and text when active
- [ ] T031 Validate Enter/Space activation on all button-like elements, Escape close on all dialogs/keyboard-dismissible popups
- [ ] T032 Re-audit accessibility after fixes — run Windows Accessibility Insights; compare against T002 baseline; generate final metrics

---

## Phase 5: DPI & Multi-Monitor Validation (WS-C)

**Purpose**: Ensure correct rendering at all DPI levels and across multi-monitor mixed-DPI configurations.

- [ ] T033 Fix all DPI clipping/overflow/misalignment issues found in T003 — adjust ControlTemplates, popup position logic, window size constraints
- [ ] T034 Implement per-monitor DPI awareness in popup positioning — ensure ComboBox popups, context menus, and tooltips render at correct size and position on the monitor they appear on
- [ ] T035 Verify font scaling consistency across all DPI levels — ensure text is not truncated, overlapping, or differently sized between monitors
- [ ] T036 Validate multi-monitor mixed-DPI — test moving windows between 100% and 150% monitors; verify no visual artifacts, clipped controls, or misaligned layouts
- [ ] T037 Re-test DPI at all 4 levels (100%, 125%, 150%, 200%) after fixes — document pass/fail per window per level
- [ ] T038 Document DPI regression checklist in `Docs/Architecture/DPI_AUDIT_REPORT.md` — provide step-by-step test to re-run on future changes

---

## Phase 6: Excel Host Stability (WS-D)

**Purpose**: Close all Phase 3 validation gaps and certify Excel VSTO stability.

- [ ] T039 Close Phase 3 WS-A T022 — validate theme switch <1s, persistence across restarts, all windows update simultaneously
- [ ] T040 Close Phase 3 WS-A T031 — verify all control states (Normal/Hover/Pressed/Focused/Disabled) render correctly in Dark + Light
- [ ] T041 Close Phase 3 WS-A T032 — verify ComboBox popup renders above content with shadow, no clipping in Excel host
- [ ] T042 Close Phase 3 WS-A T038 — validate DPI at 100%, 125%, 150%, 200% in Excel host — no clipping/overflow (coordinate with WS-C results)
- [ ] T043 Close Phase 3 WS-A T043 — validate DataGrid scrolling with 10,000+ rows inside Excel host
- [ ] T044 Close Phase 3 WS-A T044 — validate rapid theme switching (10x) inside Excel — no crash, no memory leak
- [ ] T045 Close Phase 3 WS-A T048 — verify focus indicators (Glow.Focus) visible on all interactive elements in Excel host
- [ ] T046 Close Phase 3 WS-A T049 — validate WCAG 2.1 AA contrast (4.5:1) in Dark + Light in Excel host (coordinate with WS-B results)
- [ ] T047 Close Phase 3 WS-A T050 — validate ComboBox keyboard navigation (Tab → Arrows → Escape) inside Excel host
- [ ] T048 Close Phase 3 WS-B T057 — runtime check: Excel VSTO — all windows update on theme switch
- [ ] T049 Close Phase 3 WS-B T058 — runtime check: accent color reflects in glow effects + progress bars
- [ ] T050 Close Phase 3 WS-B T059 — runtime check: rapid theme switching (10x) — no crash inside Excel
- [ ] T051 Close Phase 3 WS-B T060 — regression: progress bar renders correctly in all 7 progress bar windows
- [ ] T052 Close Phase 3 WS-B T061 — regression: TreeView hover color in Float_path.xaml
- [ ] T053 Close Phase 3 WS-B T062 — regression: close button hover color across all windows
- [ ] T054 Validate safe mode activation/deactivation — force safe mode, verify shadow variants, disabled animations, correct rendering; deactivate and verify normal mode restored
- [ ] T055 Validate crash recovery — corrupt theme settings file, missing resource dictionaries; verify app falls back to safe default theme without crashing
- [ ] T056 Run memory stability test — run add-in in Excel for 2+ hours with typical workflows; measure memory growth (must be <20%)

---

## Phase 7: Diagnostics Finalization (WS-F)

**Purpose**: Harden all diagnostic services and polish their UX.

- [ ] T057 Validate DiagnosticsService snapshot coverage — confirm `CaptureSnapshot()` returns accurate data for all windows and all render modes
- [ ] T058 Validate LoggingService — verify 5MB rollover, 3-file rotation, correct path at `AppData/Som3a/Logs/`, no file handle leaks
- [ ] T059 Extend ValidationEngine — implement full XAML scanning for inline colors (all `.xaml` files), token coverage (verify all 70+ tokens expected by control templates exist), duplicate style detection
- [ ] T060 Polish diagnostics panel UX — add loading indicator during snapshot capture, error state display when services unavailable, empty state when no diagnostics data
- [ ] T061 Integrate plugin diagnostics — ensure PluginDiagnosticsService data (module state, version, memory, load time) appears in the diagnostics panel
- [ ] T062 Validate crash recovery path — simulate corrupt theme settings, missing dictionary at startup; verify app loads safe defaults

---

## Phase 8: Documentation & Audit Reports (WS-G)

**Purpose**: Produce all required audit documentation and update project references.

- [ ] T063 Create `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md` — baseline metrics, optimizations applied, post-optimization measurements
- [ ] T064 Create `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md` — keyboard nav results, AutomationProperties coverage, contrast measurements, WCAG 2.1 AA compliance, reduced motion, high contrast
- [ ] T065 Create `Docs/Architecture/DPI_AUDIT_REPORT.md` — per-level results (100/125/150/200%), multi-monitor mixed-DPI results, DPI regression checklist
- [ ] T066 Create `Docs/Architecture/EXCEL_STABILITY_REPORT.md` — all 15 Phase 3 task results, window test pass results, memory stability, safe mode validation
- [ ] T067 Create `Docs/Architecture/LOCALIZATION_READINESS.md` — document architecture readiness for future i18n: resource extraction patterns, string externalization points, RTL considerations
- [ ] T068 Create `Docs/Architecture/ENTERPRISE_POLISH_CHECKLIST.md` — master validation checklist tracking all Phase 10 acceptance criteria with pass/fail per item
- [ ] T069 Update `AGENTS.md` — add Phase 10 spec/plan paths between SPECKIT markers, add audit report references, add build/test commands

---

## Phase 9: Production Validation Gate (Final)

**Purpose**: Final validation that all Phase 10 criteria are met before closing.

- [ ] T070 Run final build — `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — zero errors, zero warnings (existing warnings only)
- [ ] T071 Run full Excel VSTO host test — all 14 windows, theme switching, popups, safe mode, DPI at 100% and 150%
- [ ] T072 Verify all Phase 10 acceptance criteria are met — review `ENTERPRISE_POLISH_CHECKLIST.md` for 100% pass
- [ ] T073 Constitution compliance review — verify no DynamicResource violations, no inline effects, no WindowChrome bypass, no code-behind business logic in any modified files
- [ ] T074 Clean up temporary test artifacts, ensure no debug code or test fixtures remain in production code

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — baselines start immediately
- **Tech Debt Cleanup (Phase 2)**: Depends on Setup (T005, T007, T008 findings) — unblocks Phase 3-7
- **Perf (Phase 3)**: Depends on Setup (T001 baseline) — parallel with Phases 4, 5
- **Accessibility (Phase 4)**: Depends on Setup (T002 baseline) — parallel with Phases 3, 5
- **DPI (Phase 5)**: Depends on Setup (T003 baseline) — parallel with Phases 3, 4
- **Excel Stability (Phase 6)**: Depends on Phase 2 cleanup (ToastWindow migration, hardcoded fixes) — can start but results depend on other phases
- **Diagnostics (Phase 7)**: Depends on Phase 6 (Excel stability validation ensures diagnostics reliability)
- **Documentation (Phase 8)**: Depends on all audit data from Phases 3-7
- **Production Validation (Phase 9)**: Depends on all phases complete

### Parallel Opportunities

- Phases 3 (Perf), 4 (Accessibility), and 5 (DPI) can run in parallel after Setup and Tech Debt Cleanup
- Within Phase 6 (Excel Stability): T039–T053 (Phase 3 task closures) can run in parallel if multiple testers available
- Within Phase 8 (Documentation): T063–T068 can run in parallel

### Implementation Strategy

1. **Phase 1**: Establish baselines — measure before touching anything
2. **Phase 2**: Quick cleanup — ToastWindow, hardcoded values, ValidationEngine TODO — unblocks everything else
3. **Phases 3-5 (parallel)**: Perf + Accessibility + DPI — independent workstreams running concurrently
4. **Phase 6**: Excel Stability — close all Phase 3 gaps — depends on above to fix issues first
5. **Phase 7**: Diagnostics — final hardening after stability validated
6. **Phase 8**: Documentation — all audit reports written from actual results
7. **Phase 9**: Final gate — build + host test + checklist validation
