# Tasks: Framework & Project Format Upgrade

**Input**: Design documents from `/specs/028-framework-project-upgrade/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are not generated for this phase — existing tests must pass unchanged, and no new test authoring is required.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF host**: `WpfApp2/` 
- **Tests**: `Tests/`
- **VSTO add-in**: root of `Som3a Addin 2026/`

---

## Phase 1: Setup (Environment Preparation)

**Purpose**: Prepare the development environment and capture baselines before any project changes.

- [ ] T001 Verify .NET 8.0 SDK is installed (`dotnet --list-sdks`); install if missing
- [ ] T002 Record pre-migration build time baseline: `Measure-Command { dotnet build WpfApp2\Som3a_WPF_UI.csproj -c Debug }`
- [ ] T003 [P] Capture pre-upgrade screenshots of 3-5 key pages (ShellWindow, HomePage, SettingsPage, DiagnosticsPage, one AI page) in both Dark and Light themes; save to `specs/028-framework-project-upgrade/baseline-screenshots/`
- [ ] T004 [P] Verify all 6 current NuGet packages in `WpfApp2\packages.config` are documented with current versions and their minimum .NET 8.0-compatible version per research.md

---

## Phase 2: Foundational (Project Format Conversion)

**Purpose**: Convert the .csproj from legacy .NET Framework format to SDK-style. This blocks all user story work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Convert `WpfApp2\Som3a_WPF_UI.csproj` to SDK-style format: `<Project Sdk="Microsoft.NET.Sdk">`, remove legacy `ToolsVersion`, `ProjectGuid`, `TargetFrameworkProfile`, and all framework assembly references (WPF assemblies are implicit)
- [ ] T006 Set `<TargetFramework>net8.0-windows</TargetFramework>`, `<UseWPF>true</UseWPF>`, `<OutputType>WinExe</OutputType>` in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T007 Verify `WpfApp2\Som3a_WPF_UI.csproj` retains `<AssemblyName>Som3a_WPF_UI</AssemblyName>`, `<RootNamespace>Som3a_WPF_UI</RootNamespace>`, and `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` (preserving existing AssemblyInfo.cs)
- [ ] T008 [P] Remove all legacy `<Reference>` elements from `WpfApp2\Som3a_WPF_UI.csproj` that reference WPF assemblies (PresentationFramework, PresentationCore, WindowsBase, System.Xaml) — these are now implicit

**Checkpoint**: `WpfApp2\Som3a_WPF_UI.csproj` is now SDK-style with net8.0-windows target. NuGet packages not yet migrated.

---

## Phase 3: User Story 1 - Upgrade WPF Application to .NET 8.0 (Priority: P1) 🎯 MVP

**Goal**: Core .csproj format conversion and target framework upgrade enabling .NET 8.0 compilation.

**Independent Test**: `dotnet build WpfApp2\Som3a_WPF_UI.csproj` should compile, though may have compile errors from missing package references (resolved in US2) and API changes (resolved in US3).

- [ ] T009 [US1] Run `dotnet restore WpfApp2\Som3a_WPF_UI.csproj` and verify it resolves all implicit WPF framework references
- [ ] T010 [US1] Run `dotnet build WpfApp2\Som3a_WPF_UI.csproj` to validate the SDK-style format parses correctly; fix any format-level errors in the .csproj
- [ ] T011 [US1] Remove `WpfApp2\packages.config` (NuGet packages will be added as PackageReference in US2)
- [ ] T012 [US1] Add any missing assembly references that were removed during format conversion but are not covered by implicit WPF references (e.g., non-WPF assembly references from the legacy .csproj)

**Checkpoint**: Project builds with SDK-style format; compile errors may exist due to missing NuGet packages (US2) and API incompatibilities (US3).

---

## Phase 4: User Story 2 - Migrate NuGet Package Management (Priority: P2)

**Goal**: Migrate all 6 existing NuGet packages from packages.config to PackageReference in the SDK-style .csproj.

**Independent Test**: `dotnet restore WpfApp2\Som3a_WPF_UI.csproj` resolves all packages without conflicts.

- [ ] T013 [P] [US2] Add PackageReference for MaterialDesignColors (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T014 [P] [US2] Add PackageReference for MaterialDesignThemes (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T015 [P] [US2] Add PackageReference for Microsoft.Web.WebView2 (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T016 [P] [US2] Add PackageReference for Microsoft.Xaml.Behaviors.Wpf (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T017 [P] [US2] Add PackageReference for System.Text.Json (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T018 [P] [US2] Add PackageReference for Newtonsoft.Json (minimum .NET 8.0-compatible version) in `WpfApp2\Som3a_WPF_UI.csproj`
- [ ] T019 [US2] Run `dotnet restore` and verify all 6 packages resolve without version conflicts; fix any conflict by adjusting to the minimum compatible version
- [ ] T020 [US2] Run `dotnet build` and verify all NuGet assembly references resolve correctly

**Checkpoint**: All NuGet packages restored. Build may still fail due to .NET API incompatibilities (US3).

---

## Phase 5: User Story 3 - Fix .NET API Compatibility Issues (Priority: P3)

**Goal**: Fix compile errors caused by APIs removed or changed between .NET Framework 4.8 and .NET 8.0. Zero logic changes.

**Independent Test**: `dotnet build WpfApp2\Som3a_WPF_UI.csproj` succeeds with zero errors. Git diff shows only API compatibility fixes — no logic changes.

- [ ] T021 [US3] Run `dotnet build WpfApp2\Som3a_WPF_UI.csproj` and collect all compile errors
- [ ] T022 [US3] Fix `System.Configuration` errors — add `<PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />` if missing, update `ConfigurationManager.AppSettings` calls if needed
- [ ] T023 [US3] Fix `BinaryFormatter` errors — add `<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>` if used, or replace with JSON serialization using minimum change
- [ ] T024 [US3] Fix `AppDomain.CreateDomain` errors — replace with `AppDomain.CurrentDomain` or remove calls
- [ ] T025 [US3] Fix `System.Drawing` errors — add `<PackageReference Include="System.Drawing.Common" Version="8.0.0" />` if `Bitmap`/`Image` types fail to resolve
- [ ] T026 [US3] Fix `System.Runtime.Remoting` errors — replace with modern IPC or remove calls
- [ ] T027 [US3] Fix any remaining compile errors not covered above — for each error, apply the minimum .NET 8.0 equivalent API change with zero behavioral modification
- [ ] T028 [US3] Verify `dotnet build WpfApp2\Som3a_WPF_UI.csproj` succeeds with zero errors

**Checkpoint**: Build succeeds with zero errors. Ready for verification and testing.

---

## Phase 6: Verification & Testing

**Purpose**: Validate that the upgrade introduced no regressions and all success criteria are met.

- [ ] T029 Run all existing tests: `dotnet test Tests\Som3a_WPF_UI.Tests\Som3a_WPF_UI.Tests.csproj` — must pass unchanged
- [ ] T030 Run all existing tests: `dotnet test Tests\Som3a.Infrastructure.Tests\Som3a.Infrastructure.Tests.csproj` — must pass unchanged
- [ ] T031 [P] Audit VSTO add-in project's `app.config` for binding redirects referencing WPF assemblies; update version ranges to match the upgraded project output
- [ ] T032 [P] Compare post-upgrade screenshots of 3-5 key pages against pre-upgrade baselines in both Dark and Light themes; document any visual differences
- [ ] T033 Record post-migration build time: `Measure-Command { dotnet build WpfApp2\Som3a_WPF_UI.csproj -c Debug }`; verify ≤150% of pre-migration baseline
- [ ] T034 Verify git diff shows only project format changes and API compatibility fixes — zero logic changes

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and documentation updates.

- [ ] T035 Run constitution compliance review — verify no XAML, theme, animation, or resource dictionary changes were introduced (all constitutional checks are N/A for this phase)
- [ ] T036 Update `AGENTS.md` to point to `specs/028-framework-project-upgrade/plan.md` as current active plan
- [ ] T037 Clean up temporary files: remove `packages.config`, remove any leftover legacy project artifacts
- [ ] T038 Run quickstart.md validation steps end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 - Project Upgrade (Phase 3)**: Depends on Phase 2 completion
- **US2 - NuGet Migration (Phase 4)**: Depends on Phase 3 completion (.csproj must be SDK-style)
- **US3 - API Fixes (Phase 5)**: Depends on Phase 4 completion (packages must resolve first to see compile errors)
- **Verification (Phase 6)**: Depends on Phase 5 completion (build must succeed first)
- **Polish (Phase 7)**: Depends on all prior phases

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — modifies .csproj format
- **User Story 2 (P2)**: Can start after US1 — adds PackageReferences to the SDK-style .csproj
- **User Story 3 (P3)**: Can start after US2 — needs packages resolved to expose compile errors

### Parallel Opportunities

- All [P] tasks within Setup phase can run in parallel
- All [P] tasks within each user story can run in parallel
- Phase 6 tasks T031 and T032 can run in parallel
- Phase 7 tasks T035, T036, T037 can run in parallel

---

## Parallel Example: User Story 2

```powershell
# All PackageReference additions can run in parallel:
Task: "Add MaterialDesignColors PackageReference"
Task: "Add MaterialDesignThemes PackageReference"
Task: "Add Microsoft.Web.WebView2 PackageReference"
Task: "Add Microsoft.Xaml.Behaviors.Wpf PackageReference"
Task: "Add System.Text.Json PackageReference"
Task: "Add Newtonsoft.Json PackageReference"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1 (SDK-style + net8.0-windows)
4. **STOP and VALIDATE**: Run `dotnet build` — expect compile errors from missing packages, but verify .csproj format is valid
5. Continue with US2 + US3 to reach zero-error build

### Sequential Delivery (Recommended for this phase)

Tasks must execute sequentially due to tight coupling:

1. Setup → Foundational → US1 (.csproj format) → US2 (NuGet packages) → US3 (API fixes) → Verification
2. Each step unblocks the next; cannot skip ahead until prior step builds successfully

### Parallel Execution Constraints

Limited parallelism due to sequential dependencies — all modify the same project file. Parallel opportunities exist only within phases where tasks modify different files (all [P] marked tasks).

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each logical group
- Stop at any checkpoint to verify build state
- Avoid: same file edits in parallel, cross-story dependencies that break independence
- **Important**: This phase modifies `WpfApp2\Som3a_WPF_UI.csproj` — all tasks in Phases 2-4 should be coordinated to avoid merge conflicts
