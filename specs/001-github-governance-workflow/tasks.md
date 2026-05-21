# Tasks: GitHub Enterprise Governance & Review Workflow

**Input**: Design documents from `specs/001-github-governance-workflow/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Not requested for this governance/process feature.

**Organization**: Tasks grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize GitHub configuration directory and verify existing CI pipeline

- [X] T001 Create `.github/` directory structure at repository root
- [X] T002 [P] Audit existing CI pipeline in `.github/workflows/` and document available status checks
- [X] T003 [P] Verify repository permissions for branch protection configuration

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core branch protection and reviewer infrastructure that MUST be complete before ANY user story deliverables can be validated

**⚠️ CRITICAL**: No user story work can be tested until this phase is complete

- [X] T004 Configure branch protection rules for `develop` (integration branch) in GitHub repository settings
- [X] T005 Configure branch protection rules for `main` (release branch) in GitHub repository settings
- [X] T006 [P] Create `.github/CODEOWNERS` with architecture reviewer assignments for `.xaml`, `.cs`, and `Docs/Architecture/` files
- [X] T007 Configure merge method restrictions per branch: squash-only for `develop`, merge-commit-only for `main`
- [X] T008 Document branch protection settings in `Docs/Architecture/BRANCH_PROTECTION.md`

**Checkpoint**: Foundation ready — branch protection active, CODEOWNERS in place, merge methods restricted. User story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Developer Branching & Merge Workflow (Priority: P1) 🎯 MVP

**Goal**: Developers can create feature branches following the naming convention, open pull requests, and verify that merge is blocked until all gates are satisfied.

**Independent Test**: A developer creates a feature branch `feature/phase-01-test`, opens a PR to `develop`, and verifies that GitHub blocks merge until required checks and reviews are satisfied.

### Implementation for User Story 1

- [X] T009 [US1] Create `.github/pull_request_template.md` with phase reference, summary, testing, and merge gate checklist sections
- [X] T010 [US1] Document branch naming convention `feature/phase-<NN>-<short-name>` in `Docs/Architecture/BRANCH_NAMING.md`
- [X] T011 [US1] Document feature branch lifecycle (create → work → PR → merge → delete) in `Docs/Architecture/BRANCH_NAMING.md`
- [X] T012 [US1] Add PR template instructions for selecting correct merge strategy (squash for feature→develop, merge for release→main)
- [ ] T013 [US1] Validate branch protection blocks direct push to `develop` by attempting a test push
- [ ] T014 [US1] Validate branch protection blocks direct push to `main` by attempting a test push

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. A developer can create a branch, open a PR, and observe gate enforcement.

---

## Phase 4: User Story 2 - Automated & Manual Review Gates (Priority: P2)

**Goal**: Tech leads can verify that automated analysis triggers on PRs, manual architecture review checklists are available, and the review gate process is documented.

**Independent Test**: Opening a PR triggers automated checks, the architecture reviewer receives a CODEOWNERS assignment, and the review checklist is accessible in `Docs/Architecture/REVIEW_CHECKLIST.md`.

### Implementation for User Story 2

- [X] T015 [P] [US2] Create `Docs/Architecture/REVIEW_CHECKLIST.md` with pre-review automated gate verification section
- [X] T016 [P] [US2] Create `Docs/Architecture/REVIEW_CHECKLIST.md` with architecture review checklist (Token & Resource, Code Quality, Excel & Rendering, Performance, MVVM, Reuse sections)
- [X] T017 [P] [US2] Create `Docs/Architecture/REVIEW_CHECKLIST.md` with post-review sign-off instructions and AI agent compliance section
- [X] T018 [US2] Configure branch protection on `develop` to require 2 approving reviews
- [X] T019 [US2] Configure branch protection on `develop` to require all existing CI status checks to pass
- [X] T020 [US2] Document override policy (who, what, procedure, non-overrideable gates) in `Docs/Architecture/REVIEW_CHECKLIST.md`
- [X] T021 [US2] Document the review gate process (local testing, PR review, automated analysis, architecture review) in `Docs/Architecture/REVIEW_CHECKLIST.md`
- [ ] T022 [US2] Validate CODEOWNERS correctly assigns architecture reviewer when `.xaml` or `.cs` files are modified in a PR

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. PRs require 2 reviewers, automated checks run, and the review checklist is available.

---

## Phase 5: User Story 3 - AI Agent Execution Standards (Priority: P3)

**Goal**: AI agents can read documented execution rules and token naming standards, then generate code that passes the architecture review checklist without human correction.

**Independent Test**: An AI agent reads `Docs/Architecture/AGENT_RULES.md` and `Docs/Architecture/TOKEN_RULES.md`, then creates a sample token resource that follows the Primitive → Semantic → Component hierarchy.

### Implementation for User Story 3

- [X] T023 [P] [US3] Create `Docs/Architecture/AGENT_RULES.md` with inspect-before-create and reuse-before-duplicate mandates
- [X] T024 [P] [US3] Create `Docs/Architecture/AGENT_RULES.md` with NEVER/ALWAYS rules (no inline colors, no inline shadows, DynamicResource mandate, MVVM separation)
- [X] T025 [P] [US3] Create `Docs/Architecture/TOKEN_RULES.md` with token layer definitions (Primitive, Semantic, Component, Spacing, Radius, Elevation, Motion, ZIndex, Opacity)
- [X] T026 [P] [US3] Create `Docs/Architecture/TOKEN_RULES.md` with naming examples for each layer
- [X] T027 [P] [US3] Create `Docs/Architecture/PERFORMANCE_RULES.md` with ≤200ms animation budget, virtualization, and no nested shadows rules
- [X] T028 [P] [US3] Create `Docs/Architecture/POPUP_ARCHITECTURE.md` with `AllowsTransparency="False"`, `Placement="Bottom"`, and Excel-safe popup guidelines
- [X] T029 [P] [US3] Create `Docs/Architecture/EXCEL_RENDERING_RULES.md` with `WindowRenderModeDetector`, safe mode, and DPI handling guidelines
- [X] T030 [P] [US3] Create `Docs/Architecture/MVVM_RULES.md` with service/ViewModel separation and no code-behind business logic rules
- [X] T031 [P] [US3] Create `Docs/Architecture/ACCESSIBILITY_RULES.md` with keyboard navigation, focus visibility, and screen reader preparation guidelines
- [X] T032 [P] [US3] Create `Docs/Architecture/UI_GUIDELINES.md` with Windows 11 Fluent design rules and DynamicResource mandate
- [X] T033 [P] [US3] Create `Docs/Architecture/SHADOW_SYSTEM.md` with centralized effects rules and no inline `DropShadowEffect` mandate
- [X] T034 [US3] Document branch naming standards for AI agents in `Docs/Architecture/AGENT_RULES.md`
- [X] T035 [US3] Add AI self-verification instructions to `Docs/Architecture/AGENT_RULES.md` (target: 80% first-pass success per SC-004)

**Checkpoint**: All user stories should now be independently functional. AI agents have documented rules, developers have protected branches, and tech leads have review checklists.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cross-documentation consistency, validation, and governance completeness

- [X] T036 [P] Add cross-references between all `Docs/Architecture/` governance documents
- [X] T037 [P] Update `Docs/Architecture/REVIEW_CHECKLIST.md` to reference all specialized governance docs (PERFORMANCE, POPUP, EXCEL_RENDERING, MVVM, ACCESSIBILITY, SHADOW, UI_GUIDELINES, TOKEN_RULES, AGENT_RULES)
- [X] T038 Verify `quickstart.md` instructions match actual GitHub repository settings
- [X] T039 Create `Docs/Architecture/README.md` indexing all governance documents with descriptions and update frequencies
- [X] T040 Add governance document version headers (version, date, ratified by) to all `Docs/Architecture/` files
- [X] T041 Constitution compliance review — verify no UI code was introduced; confirm all constitution principles remain satisfied by inapplicability
- [ ] T042 Validate that `.github/pull_request_template.md` renders correctly in GitHub preview
- [ ] T043 Validate that `.github/CODEOWNERS` parses without syntax errors via GitHub CODEOWNERS validation
- [ ] T044 [P] Run manual test: create test feature branch, open PR, verify branch protection gates are enforced end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user story validation
- **User Stories (Phase 3–5)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Integrates with US1 (PR template + branch protection) but review checklist is independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) — Governance docs are independently authored and validated

### Within Each User Story

- Documentation tasks marked [P] can run in parallel (different files, no content dependencies)
- Configuration tasks (branch protection settings) must be applied sequentially
- Validation tasks must run after their corresponding implementation tasks

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All governance document creation tasks in US3 marked [P] can run in parallel
- All polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 3

```bash
# Launch all governance document creation tasks together:
Task: "Create Docs/Architecture/AGENT_RULES.md"
Task: "Create Docs/Architecture/TOKEN_RULES.md"
Task: "Create Docs/Architecture/PERFORMANCE_RULES.md"
Task: "Create Docs/Architecture/POPUP_ARCHITECTURE.md"
Task: "Create Docs/Architecture/EXCEL_RENDERING_RULES.md"
Task: "Create Docs/Architecture/MVVM_RULES.md"
Task: "Create Docs/Architecture/ACCESSIBILITY_RULES.md"
Task: "Create Docs/Architecture/UI_GUIDELINES.md"
Task: "Create Docs/Architecture/SHADOW_SYSTEM.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (branch protection + PR template)
4. **STOP and VALIDATE**: Create a test feature branch, open PR, verify gates block merge
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently (verify CODEOWNERS + review checklist) → Deploy/Demo
4. Add User Story 3 → Test independently (verify AI rules readability) → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple contributors:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Contributor A: User Story 1 (branch protection, PR template, branch naming docs)
   - Contributor B: User Story 2 (CODEOWNERS refinement, review checklist, CI gate config)
   - Contributor C: User Story 3 (all governance docs: AGENT_RULES, TOKEN_RULES, etc.)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- This feature is governance/process only — no UI code, no theme resources, no new effects