# Branch Protection Rules

**Project**: Som3a Add-in 2026
**Version**: 1.0.0
**Date**: 2026-05-21
**Governed by**: Constitution v1.2.0, Merge Gate Contract

---

## Branch Model

The repository uses a three-tier branching model per the governance specification:

| Branch | Role | Protection Level |
|--------|------|-----------------|
| `main` (or `master`) | Release branch | Maximum — no direct push |
| `develop` | Integration branch | Strict — PRs required, 2 reviewers |
| `feature/phase-<NN>-<name>` | Feature branch | Standard — squash merge into develop |

> **Note**: The current repository default branch is `master`. When `develop` and `main` branches are created, the same protection rules apply. If `master` serves as the release branch, protection rules for `main` should be applied to `master`.

---

## Protected Branch: `develop` (Integration)

### Required Status Checks

| Check | Required | Source |
|-------|----------|--------|
| Build passes | Yes | CI pipeline |
| Host compatibility test | Yes | CI pipeline |
| Code analysis clean | Yes | CI pipeline |

### Required Reviews

| Requirement | Value | Overrideable |
|-------------|-------|-------------|
| Minimum approving reviews | 2 | No |
| Architecture reviewer (via CODEOWNERS) | Required | No |
| Dismiss stale reviews on push | Yes | — |
| Require code owner review | Yes | — |

### Merge Restrictions

| Setting | Value |
|---------|-------|
| Allow squash merging | Yes |
| Allow merge commits | No |
| Allow rebase merging | No |
| Required merge strategy | Squash and merge |
| Require linear history | No |

### Push Restrictions

| Setting | Value |
|---------|-------|
| Allow force pushes | No |
| Allow deletions | No |
| Include administrators | Yes (with override policy) |

---

## Protected Branch: `main` / `master` (Release)

### Required Status Checks

| Check | Required | Source |
|-------|----------|--------|
| Build passes | Yes | CI pipeline |
| Host compatibility test | Yes | CI pipeline |

### Required Reviews

| Requirement | Value | Overrideable |
|-------------|-------|-------------|
| Minimum approving reviews | 1 | No |
| Require code owner review | Yes | — |

### Merge Restrictions

| Setting | Value |
|---------|-------|
| Allow squash merging | No |
| Allow merge commits | Yes |
| Allow rebase merging | No |
| Required merge strategy | Create a merge commit |
| Require linear history | No |

### Push Restrictions

| Setting | Value |
|---------|-------|
| Allow force pushes | No |
| Allow deletions | No |
| Include administrators | Yes |

---

## Feature Branch Rules

Feature branches follow the naming convention `feature/phase-<NN>-<short-name>`.

- No direct push to `develop` or `main`
- PR required for merge into `develop`
- Squash and merge for feature → develop
- Create a merge commit for develop → main

---

## Override Policy

### Who Can Override

- Repository owners
- Tech leads

### What Can Be Overridden

- Build verification (MG-001) — with incident ticket
- Host compatibility test (MG-002) — with incident ticket
- Code analysis (MG-003) — with incident ticket documenting false positives

### What CANNOT Be Overridden

- Two-reviewer minimum (MG-004)
- Architecture reviewer requirement (MG-005)
- Architecture review checklist completion (MG-006)
- No regression requirement (MG-007)

### Override Procedure

1. Create an incident ticket documenting the failure
2. Comment on the PR: "Override approved per incident #{ticket-number}. Manual verification completed."
3. Administrator merges with elevated privileges
4. All overrides are logged in the PR audit trail

---

## Configuration via GitHub CLI

### Setting up `develop` branch protection

```bash
# Create develop branch if it doesn't exist
git checkout -b develop master
git push -u origin develop

# Configure branch protection
gh api repos/{owner}/{repo}/branches/develop/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["build","host-test","code-analysis"]}' \
  --field required_pull_request_reviews='{"required_approving_review_count":2,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}' \
  --field restrictions=null \
  --field allow_force_pushes=false \
  --field allow_deletions=false \
  --field enforce_admins=true
```

### Setting up `main`/`master` branch protection

```bash
gh api repos/{owner}/{repo}/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["build","host-test"]}' \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"require_code_owner_reviews":true}' \
  --field restrictions=null \
  --field allow_force_pushes=false \
  --field allow_deletions=false \
  --field enforce_admins=true
```

---

## Validation

To verify branch protection is working:

```bash
# Test direct push to develop (should be rejected)
git checkout develop
echo "test" >> test.txt
git add test.txt
git commit -m "test: direct push should fail"
git push origin develop
# Expected: rejection

# Test PR creation (should work)
git checkout -b feature/phase-00-test-protection
echo "test" >> test.txt
git add test.txt
git commit -m "test: verify branch protection"
git push -u origin feature/phase-00-test-protection
gh pr create --base develop --title "Test: Verify Branch Protection" --body "Testing branch protection"
# Expected: PR created successfully
```

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21