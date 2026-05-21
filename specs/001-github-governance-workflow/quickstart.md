# Quickstart: GitHub Enterprise Governance & Review Workflow

**Feature**: GitHub Enterprise Governance & Review Workflow  
**Date**: 2026-05-21  
**Source**: [spec.md](spec.md)

---

## For Developers: Creating a Feature Branch and Pull Request

### 1. Create a Feature Branch

```bash
# Ensure you're on the integration branch
git checkout develop
git pull origin develop

# Create your feature branch following the naming convention
git checkout -b feature/phase-01-design-system-core
```

**Branch naming rule**: `feature/phase-<NN>-<short-name>`
- `<NN>`: Two-digit phase number (e.g., `01`, `02`)
- `<short-name>`: Lowercase, hyphenated description (e.g., `design-system-core`)

---

### 2. Work on Your Phase

Commit your changes as you work:

```bash
git add .
git commit -m "P1-T003: Add primitive token system"
git push -u origin feature/phase-01-design-system-core
```

---

### 3. Open a Pull Request

When ready, open a PR targeting `develop`:

1. Go to GitHub repository → Pull Requests → New Pull Request
2. Base: `develop` ← Compare: `feature/phase-01-design-system-core`
3. Fill in the PR template:
   - Phase reference: "Phase 01 - Design System Core"
   - Summary of changes
   - Testing performed
   - Merge gate checklist

---

### 4. Pass Merge Gates

Before merging, ensure all gates pass:

| Gate | Status | How to Verify |
|------|--------|--------------|
| Build passes | Required | Check GitHub Actions status |
| Host compatibility test | Required | Check GitHub Actions status |
| Code analysis clean | Required | Check automated analysis results |
| 2 approving reviewers | Required | One general + one architecture reviewer |
| Architecture review | Required | Reviewer completes checklist |
| No regressions | Required | Manual testing confirms |

---

### 5. Merge

Once all gates pass:

1. Ensure **Squash and merge** is selected (for feature → develop PRs)
2. Click "Squash and merge"
3. Delete the feature branch after merge

---

## For Tech Leads: Reviewing a Pull Request

### 1. Architecture Review Checklist

When assigned as architecture reviewer, verify:

- [ ] Token usage consistency (DynamicResource, no inline colors)
- [ ] Naming consistency (follows Primitive → Semantic → Component hierarchy)
- [ ] No inline colors or shadows
- [ ] No duplicate styles
- [ ] Excel-safe rendering considered
- [ ] Animations ≤200ms if applicable
- [ ] MVVM separation maintained

### 2. Approval

If all items pass, submit "Approve" with comment.

If issues found, submit "Request changes" with specific checklist items referenced.

---

## For Release Managers: Integration to Release

> **Note**: The repository uses `master` as the release branch (equivalent to `main` in documentation). When documentation references `main`, use `master`.

### 1. Create Release PR

After a phase is validated on `develop`:

```bash
git checkout master
git pull origin master
git checkout -b release/phase-01
git merge develop
# Resolve any conflicts
git push -u origin release/phase-01
```

Open PR: `master` ← `release/phase-01`

### 2. Release PR Requirements

- At least one manual approval
- Standard merge (NOT squash)
- Automated analysis already passed on integration; may skip redundant checks

### 3. Merge

Select **Create a merge commit** and merge.

---

## For Administrators: Emergency Override

If automated analysis or host test is failing due to tooling issues:

1. Create an incident ticket documenting the failure
2. In the PR, comment: "Override approved per incident #{ticket-number}. Manual testing and architecture review completed."
3. Merge with administrator privileges

**Non-overrideable gates**: Architecture review and two-reviewer requirement can NEVER be bypassed.

---

## Common Pitfalls

| Pitfall | Prevention |
|---------|------------|
| Direct push to `master` or `develop` | Branch protection blocks this; always use PRs |
| Wrong branch name | Follow `feature/phase-<NN>-<short-name>` exactly |
| Missing architecture reviewer | CODEOWNERS auto-assigns; verify assignment |
| Wrong merge strategy | Check PR merge button: squash for feature→develop, merge commit for release→master |
| Forgetting post-phase review gate | Complete review checklist before marking phase done |

---

## Reference Documents

- [AGENT_RULES.md](../../Docs/Architecture/AGENT_RULES.md) — AI execution standards
- [TOKEN_RULES.md](../../Docs/Architecture/TOKEN_RULES.md) — Token naming conventions
- [REVIEW_CHECKLIST.md](../../Docs/Architecture/REVIEW_CHECKLIST.md) — Full architecture review checklist
- [BRANCH_PROTECTION.md](../../Docs/Architecture/BRANCH_PROTECTION.md) — Branch protection rules and merge gates
- [BRANCH_NAMING.md](../../Docs/Architecture/BRANCH_NAMING.md) — Branch naming conventions and lifecycle
- [spec.md](spec.md) — Feature specification
