---

description: "Task list for Phase 26: Release Candidate & Production Packaging"

---

# Tasks: Release Candidate & Production Packaging

**Input**: Design documents from `/specs/026-release-candidate-packaging/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not requested — task focus is on implementation only (validation suites are functional deliverables, not test scripts).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF host**: `WpfApp2/` at repository root
- **.NET 8 libraries**: `Som3a.*/` at repository root
- **Scripts**: `WpfApp2/Scripts/`
- **Installer**: `WpfApp2/Setup/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the new directories, script scaffolding, and build infrastructure needed for all subsequent phases.

- [X] T001 Create WpfApp2/Scripts/ directory with validate-*.ps1 scaffolding (validate-ui.ps1, validate-plugins.ps1, validate-ai.ps1, validate-excel.ps1)
- [X] T002 [P] Create WpfApp2/Setup/ directory with WiX project scaffolding (Product.wxs, Bundle.wxs, Resources/)
- [X] T003 [P] Create WpfApp2/Setup/Resources/ directory with installer assets (icons, banners, EULA)
- [X] T004 Create build-sign.ps1 script in WpfApp2/Scripts/ with signtool.exe and mage.exe command templates

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Performance optimizations and production configuration that MUST be complete before user stories can be validated.

**⚠️ CRITICAL**: No user story can be validated against production targets until foundational optimizations are applied.

- [X] T005 Add EnableColumnVirtualization to DataGridStyles.xaml in WpfApp2/Theme/Controls/DataGridStyles.xaml
- [X] T006 [P] Audit Excel interop cleanup across all WpfApp2 ViewModels and Pages — replace two-dot COM chains with local variables, add try/finally with Marshal.ReleaseComObject
- [X] T007 [P] Add periodic GC (GCCollectionMode.Optimized, Gen 2 on 5-min timer) in App.xaml.cs idle handler
- [X] T008 [P] Implement lazy page loading in Shell navigation (WorkspaceHost.cs — defer ViewModel construction until page is selected)
- [X] T009 [P] Create production appsettings.json in WpfApp2/ with error-level logging, 100MB log cap, and production AI provider endpoint stubs
- [X] T010 [P] Create production appsettings.json for all Som3a.* .NET 8 projects matching the WPF host configuration

**Checkpoint**: Foundation ready — performance baselines set, production configuration in place, user story implementation can now begin.

---

## Phase 3: User Story 1 - Release Engineer Pipeline (Priority: P1) 🎯 MVP

**Goal**: Build the release pipeline script that runs validation → optimization → packaging → installer → QA stages in sequence with stage failure handling and resume support.

**Independent Test**: Run the pipeline from validation through to a built MSI installer producing a signed release candidate.

- [X] T011 [US1] Create pipeline orchestrator script (run-pipeline.ps1) in WpfApp2/Scripts/ with sequential stage execution (validation → optimization → packaging → installer → QA)
- [X] T012 [US1] Implement release pipeline stage tracking — write stage status (pending/running/passed/failed) to build/pipeline-state.json
- [X] T013 [P] [US1] Implement stage failure handling — stop pipeline on failure, write diagnostics to build/pipeline-state.json, support resume via --resume-from flag
- [X] T014 [US1] Implement release candidate artifact generation — produce build/release-candidate.json with version (SemVer), buildNumber, buildDate, branch, stage results
- [X] T015 [US1] Add SemVer versioning to build output — embed Major.Minor.Patch in assembly metadata and MSI ProductVersion via build argument

---

## Phase 4: User Story 2 - Quality Engineer Validation (Priority: P1)

**Goal**: Create all 4 validation suites (UI, plugins, AI, Excel) that independently verify release candidate quality with pass/fail reporting.

**Independent Test**: Run the validation suite against a built release candidate and review the validation report.

- [X] T016 [P] [US2] Implement UI validation script (validate-ui.ps1) in WpfApp2/Scripts/ — verify theme switching, shell navigation, RTL layout, and accessibility compliance
- [X] T017 [P] [US2] Implement plugin validation script (validate-plugins.ps1) in WpfApp2/Scripts/ — verify all plugins load, operate in isolation, and sustain 4-hour load with zero crashes and <5% memory growth
- [X] T018 [P] [US2] Implement AI validation script (validate-ai.ps1) in WpfApp2/Scripts/ — verify all providers return valid prompt outputs, retry handling, and structured JSON parsing
- [X] T019 [P] [US2] Implement Excel validation script (validate-excel.ps1) in WpfApp2/Scripts/ — verify export speed meets SC-008 targets, large workbook support, and interop resource cleanup
- [X] T020 [US2] Create ValidationReport output — aggregate results from all 4 suites into build/validation-report.json with pass/fail per area and overall status

---

## Phase 5: User Story 3 - IT Administrator Deployment (Priority: P2)

**Goal**: Create the MSI installer with WiX Toolset, VSTO registry registration, desktop shortcut, Start Menu integration, code signing, and upgrade support.

**Independent Test**: Run the MSI installer on a clean Windows 10/11 machine and verify all components are correctly installed.

- [X] T021 [P] [US3] Create Product.wxs in WpfApp2/Setup/ with MSI definition including ProductCode (*), UpgradeCode (stable GUID), version binding, and MajorUpgrade element
- [X] T022 [P] [US3] Add VSTO registry keys to Product.wxs — HKCU\Software\Microsoft\Office\Excel\Addins\Som3aPlanova with Description, FriendlyName, LoadBehavior (3), Manifest (file:///[INSTALLFOLDER]...vsto|vstolocal)
- [X] T023 [P] [US3] Add desktop shortcut and Start Menu shortcut components to Product.wxs
- [X] T024 [US3] Create Bundle.wxs in WpfApp2/Setup/ with Burn bootstrapper — chain .NET Framework 4.8 redist check, .NET 8 runtime install, and MSI package
- [X] T025 [US3] Implement code signing step — add sign-release.ps1 in WpfApp2/Scripts/ that signs all DLLs (signtool.exe), signs VSTO manifests (mage.exe), and produces signed MSI

---

## Phase 6: User Story 4 - End User Production Release (Priority: P2)

**Goal**: Implement crash diagnostics capture, safe logging, and AI provider fallback — ensuring the production install provides a stable, recoverable experience.

**Independent Test**: Install the production build and perform core workflows (theme switching, WBS generation, BOQ analysis, export) verifying crash recovery and AI fallback.

- [X] T026 [US4] Implement crash recovery in Som3a.Diagnostics — capture DiagnosticsSnapshot on unhandled exception (memory, active plugins, recent operations, error context) with no PII
- [X] T027 [US4] Implement safe logging in Som3a.Diagnostics — ensure error-level logging with 100MB cap, no user data or Excel content in logs
- [X] T028 [US4] Implement diagnostics export — write DiagnosticsSnapshot as structured JSON to AppData/Som3a/crash-reports/ with timestamp filename
- [X] T029 [US4] Verify AI provider fallback — ensure AIOrchestrator in Som3a.AI activates fallback provider within 5 seconds of primary failure without user-visible interruption

---

## Phase 7: User Story 5 - Technical Writer Documentation Export (Priority: P3)

**Goal**: Export all 6 documentation guides (User, Admin, Plugin SDK, Architecture, AI Setup, Troubleshooting), versioned to match the release.

**Independent Test**: Export all documentation and verify the output is complete, versioned, and correctly formatted.

- [X] T030 [P] [US5] Create User Guide export script — export docs/UserGuide.md with SemVer version header matching release
- [X] T031 [P] [US5] Create Admin Guide export script — export docs/AdminGuide.md with SemVer version header matching release
- [X] T032 [P] [US5] Create Plugin SDK Guide export script — export docs/PluginSDKGuide.md with SemVer version header matching release
- [X] T033 [P] [US5] Create Architecture Guide export script — export docs/ArchitectureGuide.md with SemVer version header matching release
- [X] T034 [P] [US5] Create AI Provider Setup Guide export script — export docs/AIProviderSetupGuide.md with SemVer version header matching release
- [X] T035 [P] [US5] Create Troubleshooting Guide export script — export docs/TroubleshootingGuide.md with SemVer version header matching release
- [X] T036 [US5] Create documentation manifest — build/docs-manifest.json listing all 6 guides with title, path, version, and export timestamp

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation pass, checklist verification, and release candidate sign-off.

- [X] T037 Run quickstart.md validation — execute all steps in quickstart.md against a clean build machine
- [X] T038 Verify release candidate checklist — confirm clean build (zero warnings), stable plugins, stable exports, final branding, final themes, responsive layouts, AI provider fallback, retry system, token tracking
- [X] T039 [P] Sign all release artifacts — run sign-release.ps1 to produce signed MSI with timestamped Authenticode signatures
- [X] T040 Produce final release candidate — trigger pipeline end-to-end, produce build/release-candidate.json with all stages passed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories (performance baselines must be set before validation targets)
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (Phase 3) must come before US2 (Phase 4) because the pipeline orchestrates validation suites
  - US2 (Phase 4) must come before US3 (Phase 5) because MSI needs validated build
  - US3 (Phase 5) must come before US4 (Phase 6) because end user needs installed product
  - US5 (Phase 7) is independent of US3/US4 — can run in parallel with installer phase
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Pipeline script — no code dependencies on existing projects beyond build tooling
- **User Story 2 (P1)**: Validation suites — needs pipeline from US1 to produce candidates to validate against
- **User Story 3 (P2)**: MSI installer — needs validated build from US2
- **User Story 4 (P2)**: Crash recovery — modifications to existing Som3a.Diagnostics; independent of US3
- **User Story 5 (P3)**: Documentation export — purely script-driven; fully independent of US1-US4

### Within Each User Story

- Setup scripts before implementation
- Validation scripts before pipeline (but organized as separate stories for independence)
- Models unchanged — this phase uses existing data models
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Phase 1 Setup tasks marked [P] can run in parallel
- All Phase 2 Foundational tasks marked [P] can run in parallel
- US5 (Phase 7) documentation scripts marked [P] can all run in parallel
- US2 validation scripts marked [P] can run in parallel
- US3 installer components marked [P] can run in parallel

---

## Parallel Example: Phase 2 Foundational

```powershell
# Launch all foundational optimizations in parallel:
Task: "Add EnableColumnVirtualization to DataGridStyles.xaml"
Task: "Audit Excel interop cleanup across all ViewModels and Pages"
Task: "Add periodic GC in App.xaml.cs idle handler"
Task: "Implement lazy page loading in WorkspaceHost.cs"
Task: "Create production appsettings.json in WpfApp2/"
Task: "Create production appsettings.json for .NET 8 projects"
```

## Parallel Example: Phase 7 Documentation Export

```powershell
# All 6 guides can be exported in parallel:
Task: "Create User Guide export script"
Task: "Create Admin Guide export script"
Task: "Create Plugin SDK Guide export script"
Task: "Create Architecture Guide export script"
Task: "Create AI Provider Setup Guide export script"
Task: "Create Troubleshooting Guide export script"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2 — both P1)

1. Complete Phase 1: Setup (scripts directory, WiX scaffolding, signing scripts)
2. Complete Phase 2: Foundational (performance optimizations, production config)
3. Complete Phase 3: User Story 1 (pipeline orchestrator with stage tracking + failure resume)
4. Complete Phase 4: User Story 2 (all 4 validation suites)
5. **STOP and VALIDATE**: Run pipeline end-to-end — should produce a validated build artifact
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready with performance baselines
2. Add US1 Pipeline + US2 Validation → Release pipeline produces validated candidates (MVP!)
3. Add US3 MSI Installer → Installable production package
4. Add US4 Crash Recovery → Production-stable with diagnostics
5. Add US5 Documentation → Complete release with all guides

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 + Phase 2 together
2. Once Foundational is done:
   - Developer A: US1 Pipeline + US2 Validation (sequential — pipeline needs validation)
   - Developer B: US5 Documentation (fully independent, all parallel)
3. After US2 complete:
   - Developer A: US4 Crash Recovery
   - Developer B: US3 MSI Installer (parallel with US4)
4. Final: Polish phase together

---

## Notes

- Phase 26 is a release and packaging phase — most tasks are scripts and infrastructure, not new feature code
- Tasks marked [P] target different files and have no ordering dependencies
- No unit or integration tests requested — validation suites (US2) serve as functional verification
- Path conventions: all script files under `WpfApp2/Scripts/`, installer files under `WpfApp2/Setup/`
- All PowerShell scripts should follow existing script conventions in the repository
