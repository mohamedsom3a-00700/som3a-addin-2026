# Tasks: WBS Engine

**Input**: Design documents from `specs/019-wbs-engine/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not requested in spec — no explicit test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

WPF pages/views at `WpfApp2/Pages/WBS/`, ViewModels at `WpfApp2/ViewModels/WBS/`, Services at `WpfApp2/Services/WBS/`, Domain at `Som3a.Domain/WBS/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Add ClosedXML NuGet package to WpfApp2 project
- [x] T002 Create folder structure: WpfApp2/Services/WBS/, WpfApp2/Pages/WBS/, WpfApp2/ViewModels/WBS/
- [x] T003 Create WBSTemplate model class in Som3a.Domain/WBS/WBSTemplate.cs with fields: Id, Name, Category, Description, Tags, RootNode, Version, IsSystem, OwnerId, CreatedAt

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and base services that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 [P] Define IWBSTemplateService interface in WpfApp2/Services/WBS/IWBSTemplateService.cs (ListTemplatesAsync, GetTemplateAsync, CreateCustomTemplateAsync, ImportTemplateAsync, ExportTemplateAsync, GetRecommendedTemplates)
- [x] T005 [P] Define IWBSCodeGenerator interface in WpfApp2/Services/WBS/IWBSCodeGenerator.cs (GenerateCode, RenumberNode, RenumberSubtree, GetNextSiblingCode)
- [x] T006 [P] Define IWBSTreeValidator interface in WpfApp2/Services/WBS/IWBSTreeValidator.cs (ValidateTree, ValidateNode, DetectsCycle)
- [x] T007 [P] Define IWBSExportService interface in WpfApp2/Services/WBS/IWBSExportService.cs (ExportToExcelAsync, ExportToJsonAsync, ExportToXmlAsync)
- [x] T008 [P] Define IWBSAIService interface in WpfApp2/Services/WBS/IWBSAIService.cs (GenerateWBSAsync, RegenerateWBSAsync, IsAIAvailable)
- [x] T009 Verify WBSNode in Som3a.Domain/WBS/WBSNode.cs has Id (GUID) field — add if missing

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Browse and Select WBS Templates (Priority: P1) 🎯 MVP

**Goal**: A planning engineer can browse 15+ WBS templates across 5 categories, preview tree structures, and get recommendations based on project type

**Independent Test**: Browse all template categories, select a template, verify its tree structure displays with correct levels, codes, and names

### Implementation for User Story 1

- [x] T010 [P] [US1] Implement WBSTemplateService in WpfApp2/Services/WBS/WBSTemplateService.cs — load 3+ built-in templates per category (15+ total) as embedded JSON, support custom template storage in AppData/Som3a/wbs-templates/
- [x] T011 [P] [US1] Implement GetRecommendedTemplates — keyword matching from project description to template category tags
- [x] T012 [P] [US1] Create WBSTemplateBrowserViewModel in WpfApp2/ViewModels/WBS/WBSTemplateBrowserViewModel.cs — template list, category filter, preview selection, recommendation highlight
- [x] T013 [US1] Create WBSTemplateBrowserPage in WpfApp2/Pages/WBS/WBSTemplateBrowserPage.xaml + .cs — template grid with category tabs, tree preview panel, recommendation badge

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Generate WBS Using AI (Priority: P1)

**Goal**: A planning engineer enters a project description and triggers AI-powered WBS generation; system falls back to manual-only mode when AI is unavailable

**Independent Test**: Enter a project description (e.g., "3-story office building"), trigger AI generation, verify output is a valid WBSNode tree with 3+ levels; verify graceful message when AI unavailable

### Implementation for User Story 2

- [x] T014 [P] [US2] Implement WBSCodeGenerator in WpfApp2/Services/WBS/WBSCodeGenerator.cs — depth-first hierarchical numbering (1, 1.1, 1.1.1), renumber subtree on insert/delete
- [x] T015 [P] [US2] Implement WBSTreeValidator in WpfApp2/Services/WBS/WBSTreeValidator.cs — cycle detection via DFS, depth limit check (default 10), naming validation
- [x] T016 [P] [US2] Implement WBSAIService in WpfApp2/Services/WBS/WBSAIService.cs — invoke Phase 18 OrchestrationEngine with WBS prompt template, parse via WBSParser, emit progress events, handle AI unavailability with clear error
- [x] T017 [US2] Create WBSGeneratorViewModel in WpfApp2/ViewModels/WBS/WBSGeneratorViewModel.cs — project description input, AI generation command with progress, accept/regenerate/cancel, fallback to manual mode
- [x] T018 [US2] Create WBSGeneratorPage in WpfApp2/Pages/WBS/WBSGeneratorPage.xaml + .cs — project description text area, generate button with progress indicator, generated tree preview, accept/regenerate buttons

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Edit WBS Structure Manually (Priority: P2)

**Goal**: A planning engineer can add, remove, rename, and reorder nodes; codes auto-update; custom templates can be saved from edited trees

**Independent Test**: Create a WBS tree from scratch with 5+ nodes across 3 levels, add/remove/reorder nodes, verify code auto-generation and tree validation

### Implementation for User Story 3

- [x] T019 [P] [US3] Implement tree node add/remove/reorder operations in WBSEditorViewModel (WpfApp2/ViewModels/WBS/WBSEditorViewModel.cs) — integrate WBSCodeGenerator and WBSTreeValidator
- [x] T020 [P] [US3] Implement undo support — track WBSChange history for at least one level of undo
- [x] T021 [P] [US3] Implement "Save as custom template" — create WBSTemplate from current root node and persist to AppData via WBSTemplateService
- [x] T022 [US3] Create WBSEditorPage in WpfApp2/Pages/WBS/WBSEditorPage.xaml + .cs — tree view with add/remove/rename/reorder buttons, inline rename, validation error display, save-as-template button

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Export WBS (Priority: P2)

**Goal**: A planning engineer exports the final WBS tree to Excel (with indentation), JSON, and XML formats

**Independent Test**: Create a WBS tree with 3+ levels, export to each format, verify exported files contain correct structure, codes, and paths

### Implementation for User Story 4

- [x] T023 [P] [US4] Implement WBSExportService.ExportToExcelAsync in WpfApp2/Services/WBS/WBSExportService.cs — ClosedXML-based .xlsx with indentation, grouping, code/name/path columns
- [x] T024 [P] [US4] Implement WBSExportService.ExportToJsonAsync in WpfApp2/Services/WBS/WBSExportService.cs — System.Text.Json serialization with full node hierarchy
- [x] T025 [P] [US4] Implement WBSExportService.ExportToXmlAsync in WpfApp2/Services/WBS/WBSExportService.cs — System.Xml.Linq hierarchical XML
- [x] T026 [US4] Create WBSExportViewModel in WpfApp2/ViewModels/WBS/WBSExportViewModel.cs — format selection, file path picker, export command with progress
- [x] T027 [US4] Create WBSExportPage in WpfApp2/Pages/WBS/WBSExportPage.xaml + .cs — format radio buttons, file path selector, export button, success/error feedback

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T028 [P] Wire WBS services into CompositionRoot.RegisterServices() — register IWBSTemplateService, IWBSCodeGenerator, IWBSTreeValidator, IWBSExportService, IWBSAIService
- [x] T029 [P] Register WBS pages in Shell navigation — add WBSTemplateBrowserPage, WBSGeneratorPage, WBSEditorPage, WBSExportPage routes
- [x] T030 [P] Verify all WBS WPF pages use DynamicResource for themeable properties — no StaticResource or inline colors
- [x] T031 Run build verification — ensure WpfApp2 builds without errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion — can be implemented independently
- **User Story 2 (Phase 4)**: Depends on Foundational completion + US1 (templates used as base for AI) + Phase 18 AI Core (already built)
- **User Story 3 (Phase 5)**: Depends on Foundational completion — needs WBS tree from US1/US2 for editing, but can be tested with mock tree
- **User Story 4 (Phase 6)**: Depends on Foundational completion — needs WBS tree from US2/US3 but can be tested with mock tree
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories — **MVP scope**
- **User Story 2 (P1)**: Can start after Foundational + US1 — Uses WBSTemplateService to find base template; uses Phase 18 AI engine (already built)
- **User Story 3 (P2)**: Can start after Foundational — Can be tested with manually created tree or mock data
- **User Story 4 (P2)**: Can start after Foundational — Can be tested with mock WBS tree data

### Within Each User Story

- Services before ViewModels
- ViewModels before Pages
- Story complete before moving to next priority

### Parallel Opportunities

- All Foundational tasks (T004-T009) can run in parallel
- US1 tasks T010-T011 (template service) can run in parallel with T012 (viewmodel)
- US2 tasks T014-T016 (code gen, validator, AI service) can run in parallel
- US4 export service tasks T023-T025 can run in parallel
- Phase 7 Polish tasks T028-T030 can run in parallel

---

## Parallel Execution Examples

### Phase 3: User Story 1

```bash
# Template service creation and viewmodel can be done in parallel:
Task: "Implement WBSTemplateService in WpfApp2/Services/WBS/WBSTemplateService.cs"
Task: "Create WBSTemplateBrowserViewModel in WpfApp2/ViewModels/WBS/WBSTemplateBrowserViewModel.cs"
```

### Phase 4: User Story 2

```bash
# All 3 core services can be done in parallel before viewmodel:
Task: "Implement WBSCodeGenerator in WpfApp2/Services/WBS/WBSCodeGenerator.cs"
Task: "Implement WBSTreeValidator in WpfApp2/Services/WBS/WBSTreeValidator.cs"
Task: "Implement WBSAIService in WpfApp2/Services/WBS/WBSAIService.cs"
```

### Phase 6: User Story 4

```bash
# All 3 export format implementations can be done in parallel:
Task: "Implement ExportToExcelAsync in WpfApp2/Services/WBS/WBSExportService.cs"
Task: "Implement ExportToJsonAsync in WpfApp2/Services/WBS/WBSExportService.cs"
Task: "Implement ExportToXmlAsync in WpfApp2/Services/WBS/WBSExportService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1 (template browsing + selection)
4. **STOP and VALIDATE**: Browse templates, preview trees, verify recommendations

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently (with Phase 18 AI) → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability (US1-US4)
- Each user story should be independently completable and testable
- Test tasks excluded per spec (no explicit TDD/test requirement)
- Phase 18 AI Core must be built and available for US2 (AI generation)
- WBSNode already exists in Som3a.Domain/WBS/ from Phase 14
