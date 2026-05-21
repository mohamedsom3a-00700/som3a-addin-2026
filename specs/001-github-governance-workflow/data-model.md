# Data Model: GitHub Enterprise Governance & Review Workflow

**Feature**: GitHub Enterprise Governance & Review Workflow  
**Date**: 2026-05-21  
**Source**: [spec.md](spec.md)

---

## Entities

### Feature Branch

**What it represents**: A git branch created from the integration branch to contain phase-specific deliverables.

**Attributes**:
- `name`: string — Must match pattern `feature/phase-<NN>-<short-name>` (FR-008)
- `source_branch`: string — Always `develop` (integration branch)
- `target_branch`: string — Always `develop` for feature branches; `main` for release PRs
- `phase_number`: integer — Extracted from branch name (e.g., `01` from `feature/phase-01-design-system-core`)
- `created_date`: timestamp
- `author`: string — Developer or AI agent identifier

**Validation Rules**:
- MUST match regex `^feature/phase-\d{2}-[a-z0-9-]+$`
- MUST be created from `develop`
- MUST NOT be created from `main`

**Lifecycle**:
1. Created from `develop`
2. Work committed during phase implementation
3. Pull request opened targeting `develop`
4. Merge gates validated
5. Squash-merged into `develop`
6. Branch deleted after merge

---

### Pull Request

**What it represents**: A request to merge code from one branch to another, serving as the vehicle for review and gate enforcement.

**Attributes**:
- `id`: integer — GitHub PR number
- `source_branch`: string — Feature branch name
- `target_branch`: string — `develop` or `main`
- `title`: string — Must reference phase (e.g., "Phase 01: Design System Core")
- `description`: string — Summary, testing, and gate checklist
- `reviewers`: array of strings — At least two: one code reviewer, one architecture reviewer
- `status_checks`: array of objects — Build, host test, code analysis results
- `merge_strategy`: enum — `squash` (feature→develop) or `merge` (develop→main)
- `state`: enum — `open`, `approved`, `changes_requested`, `merged`, `closed`

**Validation Rules**:
- MUST have at least 2 approving reviewers (FR-006)
- MUST have all status checks passing (FR-003, FR-004, FR-005)
- MUST have architecture review checklist completed (FR-012)
- MUST use correct merge strategy for target branch (FR-013)

**Lifecycle**:
1. Opened by developer
2. Automated checks run
3. Reviewers assigned (CODEOWNERS + manual)
4. Reviews submitted
5. Issues addressed
6. All gates pass
7. Merged
8. Branch deleted

---

### Merge Gate

**What it represents**: A set of mandatory requirements that must be satisfied before merging.

**Attributes**:
- `build_pass`: boolean — Automated build verification (FR-003)
- `host_test_pass`: boolean — Host compatibility testing (FR-004)
- `code_analysis_clean`: boolean — No unresolved critical issues (FR-005)
- `reviewer_count`: integer — Must be ≥ 2 (FR-006)
- `architecture_review_done`: boolean — Architecture reviewer approved (FR-006)
- `no_regressions`: boolean — No rendering, popup, or theme regressions (FR-007)
- `merge_strategy_valid`: boolean — Correct merge method for target branch (FR-013)
- `override_ticket`: string (optional) — Incident ticket reference if override applied

**Validation Rules**:
- All boolean attributes MUST be `true` before merge, unless:
  - `code_analysis_clean` or `host_test_pass` is overridden by tech lead/owner with documented incident ticket (Q3 clarification)
  - `architecture_review_done` and `reviewer_count` are NEVER overrideable

---

### Review Gate

**What it represents**: A post-phase validation process confirming all quality checks were completed.

**Attributes**:
- `phase`: string — Phase identifier (e.g., "Phase 01")
- `local_testing_done`: boolean — Excel host, DPI, popup, rendering testing
- `pr_review_done`: boolean — Pull request reviewed and approved
- `automated_analysis_done`: boolean — Code analysis completed
- `architecture_review_done`: boolean — Manual architecture review passed
- `review_date`: timestamp — When the review gate was completed
- `reviewer`: string — Tech lead or designated reviewer

**Validation Rules**:
- All boolean attributes MUST be `true` before phase is considered complete (FR-011)
- MUST be documented in the phase completion record

---

### Governance Document

**What it represents**: An architecture rule document defining standards for AI and human contributors.

**Attributes**:
- `name`: string — Document name (e.g., "AGENT_RULES.md", "TOKEN_RULES.md")
- `category`: enum — `ai_rules`, `token_standards`, `review_checklist`, `performance`, `accessibility`, `popup`, `shadow`, `excel_rendering`, `mvvm`
- `version`: string — Document version
- `last_updated`: timestamp
- `content`: string — Markdown content with rules

**Validation Rules**:
- AI execution rules MUST include inspect-before-create and reuse-before-duplicate mandates (FR-009)
- Token naming standards MUST cover all token layers (FR-010)
- Review checklist MUST cover all architecture review items (FR-012)

---

## Relationships

```text ──creates──> Pull Request
Pull Request ──validates──> Merge Gate
Pull Request ──triggers──> Review Gate (post-phase)
Governance Document ──guides──> Pull Request (review checklist)
Governance Document ──guides──> Feature Branch (naming, AI rules)
```text

---

## State Transitions

### Pull Request State Machine

```text
[OPENED]
    ↓ (automated checks run)
[CHECKS_RUNNING]
    ↓ (checks pass)
[AWAITING_REVIEW]
    ↓ (reviews submitted)
[CHANGES_REQUESTED] ←──┐
    ↓ (issues fixed)   │
[APPROVED]             │
    ↓ (merge)          │
[MERGED] ──────────────┘
    ↓ (branch deleted)
[CLOSED]
```

### Phase Completion State Machine

```text
[PHASE_IN_PROGRESS]
    ↓ (work complete)
[PR_OPENED]
    ↓ (merge gates pass)
[MERGED_TO_INTEGRATION]
    ↓ (post-phase validation)
[REVIEW_GATE_RUNNING]
    ↓ (all checks pass)
[PHASE_COMPLETE]
    ↓ (release PR created)
[RELEASE_PR_OPENED]
    ↓ (approval)
[RELEASED]
```
