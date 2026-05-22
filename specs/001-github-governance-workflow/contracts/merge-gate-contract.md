# Merge Gate Contract

**Feature**: GitHub Enterprise Governance & Review Workflow  
**Date**: 2026-05-21  
**Source**: [spec.md](spec.md)

---

## Purpose

Defines the mandatory requirements that must be satisfied before any pull request can be merged into the integration (`develop`) or release (`main`) branch.

---

## Contract

### Parties

- **Contributor**: Developer or AI agent creating the pull request
- **Code Reviewer**: General reviewer validating functional correctness
- **Architecture Reviewer**: Designated reviewer validating token usage, DynamicResource compliance, and Excel-safe rendering
- **Automated Systems**: CI pipeline running build, test, and code analysis

---

### Requirements (All MUST Pass)

| ID | Requirement | Enforced By | Overrideable |
|----|-------------|-------------|--------------|
| MG-001 | Automated build verification passes | GitHub Actions status check | Yes (with incident ticket) |
| MG-002 | Host compatibility test passes | GitHub Actions status check | Yes (with incident ticket) |
| MG-003 | Automated code analysis has no unresolved critical issues | Code analysis status check | Yes (with incident ticket) |
| MG-004 | Integration branches (develop, feature/*, main): at least two approving reviewers. Release branches (release/*, hotfix/*): at least one approving reviewer (manual release approval). | GitHub branch protection / PR gating logic | No |
| MG-005 | One approving reviewer is designated architecture reviewer | CODEOWNERS + team discipline | No |
| MG-006 | Architecture review checklist completed | Manual review | No |
| MG-007 | No rendering, popup, or theme regressions | Manual testing + CI | No |
| MG-008 | Correct merge strategy for target branch | GitHub branch settings + PR template | No |

---

### Merge Strategy by Target Branch

| Source | Target | Required Strategy | Rationale |
|--------|--------|-------------------|-----------|
| Feature branch | `develop` | Squash and merge | Keeps integration history clean and readable |
| `develop` | `main` | Create a merge commit | Preserves full phase milestone history and simplifies rollbacks |

---

### Override Policy

**Who**: Tech leads and repository owners only.

**What can be overridden**:
- MG-001 (build) — If CI tooling failure is confirmed
- MG-002 (host test) — If test environment issue is confirmed
- MG-003 (code analysis) — If analysis tool produces false positives

**What CANNOT be overridden**:
- MG-004 (two reviewers)
- MG-005 (architecture reviewer)
- MG-006 (architecture review checklist)
- MG-007 (no regressions)

**Procedure**:
1. Create incident ticket documenting the issue
2. Comment on PR: "Override approved per incident #{ticket-number}. Manual verification completed."
3. Administrator merges with elevated privileges

---

### Failure Handling

| Scenario | Action |
|----------|--------|
| Build fails | Fix code; re-push; re-run CI |
| Host test fails intermittently | Re-run up to 3 times; if still failing, manual verification session required |
| Code analysis finds critical issue | Fix issue or provide explicit justification; re-run analysis |
| Reviewer requests changes | Address feedback; re-request review |
| Architecture review finds violations | Fix violations (inline colors, duplicate styles, etc.); re-request review |

---

### Audit Trail

Each merge gate produces the following audit artifacts:
- GitHub Actions run logs (build, test, analysis)
- Pull request review comments
- Architecture review checklist completion
- Override incident ticket reference (if applicable)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-05-21 | Initial contract |
