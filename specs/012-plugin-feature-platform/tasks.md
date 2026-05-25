# Tasks: Plugin & Feature Platform

**Input**: Design documents from `/specs/012-plugin-feature-platform/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested — no test tasks generated.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `WpfApp2/` — single WPF application project
- **New directories**: `WpfApp2/Contracts/`, `WpfApp2/Modules/`
- **Existing directories**: `WpfApp2/Services/`, `WpfApp2/Controls/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directories and register the plugin platform in the existing infrastructure.

- [X] T001 Create `WpfApp2/Contracts/` directory for module contract interfaces
- [X] T002 Create `WpfApp2/Modules/` directory for module assembly deployment
- [X] T003 [P] Register `ModuleRegistry`, `PluginLoader`, and `ModuleDiagnosticsService` in `WpfApp2/CompositionRoot.cs` (singleton lifetime)
- [X] T004 Add `Modules/` to .csproj as a deployment folder (`<Content Include="Modules\**\*.*">`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core contract interfaces and base types that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 [P] Create `IModule.cs` in `WpfApp2/Contracts/IModule.cs` — module identity (Id, Version, DisplayName, Description), Initialize(IModuleInitializationContext) method
- [X] T006 [P] Create `ModuleManifest.cs` in `WpfApp2/Contracts/ModuleManifest.cs` — Id, Version, DisplayName, Description, Hash, HashAlgorithm, Capabilities[], Dependencies[]
- [X] T007 [P] Create `ModuleInfo.cs` and `ModuleState` enum in `WpfApp2/Contracts/ModuleInfo.cs` — Id, Version, DisplayName, Description, State, Capabilities, MemoryBytes, LoadTimeMs, LastError; states: Registered, Loading, Active, Failed, Unloaded, Disabled
- [X] T008 [P] Create `ModuleStateChangedEventArgs.cs` in `WpfApp2/Contracts/ModuleStateChangedEventArgs.cs` — ModuleId, OldState, NewState
- [X] T009 Create `IModuleRegistry.cs` in `WpfApp2/Contracts/IModuleRegistry.cs` — Register(ModuleManifest), GetModuleInfo(), GetAllModules(), GetModulesByState(), RetryModule(), UnloadModule(), DisableModule(), ModuleStateChanged event
- [X] T010 Create `IPluginLoader.cs` in `WpfApp2/Contracts/IPluginLoader.cs` — DiscoverModules(), LoadModule(string moduleId), ModulesDirectory property

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Developer Registers a New Module (Priority: P1) 🎯 MVP

**Goal**: An internal developer can create a module implementing IModule, register it, and have its page appear in navigation and its ribbon action appear in the ribbon.

**Independent Test**: Create a minimal hello-world module, deploy to `Modules/`, start app, verify page is listed in navigation, click it, verify it renders. Verify ribbon button appears.

### Implementation for User Story 1

- [X] T011 [P] [US1] Create `IModuleInitializationContext.cs` in `WpfApp2/Contracts/IModuleInitializationContext.cs` — Navigation, Ribbon, Commands properties
- [X] T012 [P] [US1] Create `INavigationRegistrar.cs` in `WpfApp2/Contracts/INavigationRegistrar.cs` — RegisterPage(string id, string title, Type pageType)
- [X] T013 [P] [US1] Create `IRibbonRegistrar.cs` in `WpfApp2/Contracts/IRibbonRegistrar.cs` — AddButton(id, label, tooltip, onClick), AddMenu(id, label, items), AddToggleButton(id, label, onToggle, initialState); RibbonMenuItem class
- [X] T014 [P] [US1] Create `ICommandRegistrar.cs` in `WpfApp2/Contracts/ICommandRegistrar.cs` — RegisterCommand(string name, ICommand command)
- [X] T015 [US1] Implement `ModuleRegistry.cs` in `WpfApp2/Services/ModuleRegistry.cs` — in-memory Dictionary<string, ModuleInfo>, Register() stores manifest metadata, GetModuleInfo() returns runtime info, GetAllModules(), GetModulesByState(), RetryModule(), UnloadModule(), DisableModule(), fire ModuleStateChanged
- [X] T016 [US1] Implement `NavigationRegistrar.cs` in `WpfApp2/Services/NavigationRegistrar.cs` — stores registered pages, integrates with existing Navigation Shell (Phase 5) to add pages dynamically
- [X] T017 [US1] Implement `RibbonRegistrar.cs` in `WpfApp2/Services/RibbonRegistrar.cs` — stores ribbon action callbacks, integrates with existing VSTO ribbon infrastructure to replay registrations on ribbon load
- [X] T018 [US1] Implement `CommandRegistrar.cs` in `WpfApp2/Services/CommandRegistrar.cs` — stores ICommand instances keyed by name, provides lookup for command dispatch
- [X] T019 [US1] Implement `ModuleInitializationContext.cs` in `WpfApp2/Services/ModuleInitializationContext.cs` — aggregates NavigationRegistrar, RibbonRegistrar, CommandRegistrar and IServiceContainer.Resolve for module dependency injection

**Checkpoint**: A developer can create a module and register it. Navigation pages and ribbon actions appear. (MVP scope.)

---

## Phase 4: User Story 2 — System Loads Modules on Demand (Priority: P1)

**Goal**: Application starts without loading module assemblies. Modules are lazily loaded when first accessed. An inline loading spinner is shown during load.

**Independent Test**: Register two modules. Start app. Verify neither assembly is loaded (check AppDomain.GetAssemblies()). Navigate to module A's page. Verify A loads and renders. Navigate to B. Verify B loads and A remains active.

### Implementation for User Story 2

- [X] T020 [US2] Implement `PluginLoader.cs` in `WpfApp2/Services/PluginLoader.cs` — DiscoverModules() scans `Modules/` subdirectories for `module.json` files, parses ModuleManifest; LoadModule(string id) reads assembly bytes, validates SHA256 hash against manifest, calls Assembly.LoadFrom, finds IModule type via reflection, calls Initialize(IModuleInitializationContext), returns IModule instance
- [X] T021 [US2] Implement SHA256 integrity check in `PluginLoader.cs` — compute hash of assembly bytes, compare to manifest.Hash, throw InvalidOperationException with detailed message on mismatch
- [X] T022 [US2] Create `ModuleLoadingOverlay.xaml` and `.xaml.cs` in `WpfApp2/Controls/ModuleLoadingOverlay.xaml` — WPF ContentControl with centered spinner animation shown during module load; visibility bound to loading state
- [X] T023 [US2] Wire lazy loading trigger into Navigation Shell — when user navigates to a module page, check ModuleRegistry state; if Registered, transition to Loading, invoke PluginLoader.LoadModule(), then complete navigation; show ModuleLoadingOverlay during load
- [X] T024 [US2] Wire lazy loading for commands — when a module command is invoked, if module not yet loaded, load it before executing the command
- [X] T025 [US2] Integrate startup scan — on application startup, call PluginLoader.DiscoverModules() to build manifest cache; register all discovered modules with ModuleRegistry

**Checkpoint**: Modules are loaded lazily on first access with inline loading spinner.

---

## Phase 5: User Story 3 — Feature Isolation & Stability (Priority: P2)

**Goal**: A fault in one module does not crash the host application or affect other loaded modules. Failed modules enter permanent Failed state and require manual retry from diagnostics UI.

**Independent Test**: Create a module that throws during Initialize. Register alongside a working module. Navigate to failing module — verify app doesn't crash. Navigate to working module — verify it works normally. Verify failing module shows Failed state in diagnostics.

### Implementation for User Story 3

- [X] T026 [US3] Implement try/catch fault boundaries in `PluginLoader.LoadModule()` — wrap assembly loading, reflection, and Initialize() in try/catch; on exception set ModuleState to Failed, store LastError, log exception, do not rethrow
- [X] T027 [US3] Implement try/catch fault boundaries in navigation dispatch — wrap module page initialization in try/catch; on exception show inline error message within content area instead of crashing
- [X] T028 [US3] Implement try/catch fault boundaries in command dispatch — wrap command execution in try/catch; on exception log and show toast notification
- [X] T029 [US3] Implement try/catch fault boundaries in ribbon action dispatch — wrap ribbon callback in try/catch; on exception log and show toast notification
- [X] T030 [US3] Implement `RetryModule()` in `ModuleRegistry.cs` — transition from Failed to Registered state (clears LastError), next navigation triggers re-load attempt
- [X] T031 [US3] Add logging for all module lifecycle events — log module discovery, registration, load start, load success, load failure, retry, unload, disable events (use existing IEventBus from Phase 6)

**Checkpoint**: Module failures are contained. Faulty modules enter Failed state. Working modules unaffected.

---

## Phase 6: User Story 4 — Plugin Diagnostics & Validation (Priority: P3)

**Goal**: An administrator can inspect all modules (registered/loaded/failed), their versions, and resource usage. The system validates module contracts at registration time and rejects non-compliant modules.

**Independent Test**: Register valid and invalid modules (version mismatch, missing deps, bad hash). Verify diagnostics UI shows status for all modules. Verify invalid modules are rejected with specific error messages.

### Implementation for User Story 4

- [X] T032 [US4] Implement `ModuleDiagnosticsService.cs` in `WpfApp2/Services/ModuleDiagnosticsService.cs` — GetSnapshot() returns List<DiagnosticsSnapshot> aggregated from ModuleRegistry; subscribes to ModuleStateChanged to update cached diagnostics
- [X] T033 [US4] Create Diagnostics UI view — add diagnostics page to existing SettingsWindow or Navigation Shell showing table of modules with columns: Name, Id, Version, State, Capabilities, Memory, Load Time, Last Error; with Retry button for Failed modules and Unload/Disable buttons for Active modules
- [X] T034 [US4] Implement contract validation at registration time in `ModuleRegistry.cs` — validate that module assembly contains a type implementing IModule (via reflection), validate manifest fields are non-empty and well-formed
- [X] T035 [US4] Implement version validation — compare module contract version string against host expected version; reject with clear error if mismatch
- [X] T036 [US4] Implement dependency validation — check all dependency IDs exist in registry, detect circular dependencies via dependency graph cycle detection; reject with specific error listing missing or circular dependencies
- [ ] T037 [US4] Implement settings schema rendering in Diagnostics UI — if module declares settings (via ISettingsSchema), render configuration fields from schema; save changes back to module's settings dictionary

**Checkpoint**: Administrators can monitor and manage all modules through the diagnostics view. Contract violations produce specific, actionable error messages.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [X] T038 [P] Create sample hello-world module project at `WpfApp2.Modules.Sample/` to validate the full workflow (IModule implementation, module.json, build, deploy, test)
- [X] T039 [P] Add `AssemblyResolve` event handler in `App.xaml.cs` to resolve module assembly dependencies (load dependent assemblies from module's directory)
- [X] T040 Run quickstart.md validation — follow the quickstart guide end-to-end, verify all steps produce expected results
- [X] T041 Constitution compliance review — verify no DynamicResource violations in new controls, no inline effects, no WindowChrome bypass, no code-behind business logic
- [X] T042 Build validation — run msbuild and verify zero errors with all new files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — no dependencies on other stories
- **US2 (Phase 4)**: Depends on Foundational and US1 (uses ModuleRegistry, IPluginLoader, navigation registration)
- **US3 (Phase 5)**: Depends on Foundational and US2 (fault boundaries wrap PluginLoader navigation flow)
- **US4 (Phase 6)**: Depends on US1 and US3 (diagnostics reads ModuleRegistry, retry uses state machine)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — No dependencies on other stories. **MVP candidate.**
- **US2 (P1)**: Uses IModuleRegistry from US1, INavigationRegistrar from US1 — must wait for US1.
- **US3 (P2)**: Wraps PluginLoader and navigation flows from US2 — must wait for US2.
- **US4 (P3)**: Reads ModuleRegistry state from US1/3 — must wait for US1 and US3.

### Within Each User Story

- Contracts before services
- Services before integration
- Integration before UI
- Story complete before moving to next priority

### Parallel Opportunities

- All Phase 1 tasks marked [P] can run in parallel
- All Phase 2 tasks marked [P] (T005-T008 contract files) can run in parallel
- Within US1: T011-T014 (contract interfaces) can run in parallel; services (T015-T019) depend on them
- Within US4: T034-T036 (validation rules) can run in parallel
- Polish tasks T038-T039 [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all contract interfaces for US1 together:
Task: "IModuleInitializationContext.cs"
Task: "INavigationRegistrar.cs"
Task: "IRibbonRegistrar.cs"
Task: "ICommandRegistrar.cs"

# Services (sequential, after contracts):
Task: "ModuleRegistry.cs"
Task: "NavigationRegistrar.cs"
Task: "RibbonRegistrar.cs"
Task: "CommandRegistrar.cs"
Task: "ModuleInitializationContext.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 — Developer can register a module with page + ribbon action
4. **STOP and VALIDATE**: Create sample module, verify page appears in navigation, verify ribbon button works
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (module registration) → Test independently → **MVP!**
3. Add US2 (lazy loading) → Test independently → Deploy
4. Add US3 (fault isolation) → Test independently → Deploy
5. Add US4 (diagnostics + validation) → Test independently → Deploy

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (module contracts + registration)
   - Developer B: Standby or begin US2 early if US1 interfaces are stable
3. After US1 complete:
   - Developer A: US2 (lazy loading)
   - Developer B: US3 (fault isolation)
4. After US2/US3 complete:
   - Both: US4 (diagnostics)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
