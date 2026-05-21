# Branch Naming Convention

**Project**: Som3a Add-in 2026
**Version**: 1.0.0
**Date**: 2026-05-21
**Governed by**: Constitution v1.2.0, Merge Gate Contract

---

## 1. Branch Naming Pattern

All feature branches MUST follow this naming convention:

```text
feature/phase-<NN>-<short-kebab-name>
```

### Components

| Component | Format | Examples |
|-----------|--------|----------|
| `feature/` | Fixed prefix | Always `feature/` |
| `phase-<NN>` | Two-digit phase number | `phase-00`, `phase-01`, `phase-10` |
| `<short-name>` | Lowercase, hyphenated | `governance-foundation`, `design-system-core` |

### Validation Regex

```text
^feature/phase-\d{2}-[a-z0-9-]+$
```

---

## 2. Branch Types

| Branch Type | Pattern | Purpose | Merge Target | Merge Strategy |
|-------------|---------|---------|-------------|----------------|
| Feature | `feature/phase-<NN>-<name>` | Phase deliverables | `develop` | Squash and merge |
| Hotfix | `hotfix/<description>` | Emergency fixes | `develop` and `main` | Squash to develop, cherry-pick to main |
| Release | `release/phase-<NN>` | Release preparation | `main` | Create a merge commit |
| Integration | `develop` | Integration branch | — | — |
| Release branch | `main` / `master` | Production-ready code | — | — |

---

## 3. Phase Branch Names

| Phase | Branch Name | Description |
|-------|-------------|-------------|
| Phase 0 | `feature/phase-00-governance-foundation` | Repository governance and review workflow |
| Phase 1 | `feature/phase-01-design-system-core` | Primitive tokens, semantic tokens, typography |
| Phase 2 | `feature/phase-02-rendering-infrastructure` | WindowChrome, FallbackSafe, popup architecture |
| Phase 3 | `feature/phase-03-theme-engine-v2` | Theme switching, accent system, persistence |
| Phase 4 | `feature/phase-04-control-standardization` | Button, ComboBox, DataGrid styles |
| Phase 5 | `feature/phase-05-navigation-shell` | Navigation, dialogs, toast service |
| Phase 6 | `feature/phase-06-mvvm-cleanup` | DI, service registration, separation |
| Phase 7 | `feature/phase-07-settings-ux` | Settings window, theme editor, accent picker |
| Phase 8 | `feature/phase-08-diagnostics` | Debug overlay, theme diagnostics, logging |
| Phase 9 | `feature/phase-09-plugin-platform` | Plugin architecture, extension points |
| Phase 10 | `feature/phase-10-enterprise-polish` | Accessibility, performance, high contrast |
| Phase 11 | `feature/phase-11-legacy-migration` | Remaining migration, cleanup |

---

## 4. Feature Branch Lifecycle

### Creating a Feature Branch

```bash
# Start from integration branch
git checkout develop
git pull origin develop

# Create feature branch
git checkout -b feature/phase-01-design-system-core

# Push to remote
git push -u origin feature/phase-01-design-system-core
```

### Working on a Feature Branch

```bash
# Regular commits
git add .
git commit -m "feat(theme): add primitive token system"
git push

# Keep up to date with develop
git fetch origin
git merge origin/develop  # or rebase
```

### Opening a Pull Request

1. Go to GitHub → Pull Requests → New Pull Request
2. Base: `develop` ← Compare: `feature/phase-01-design-system-core`
3. Fill in the PR template with phase reference and merge gate checklist
4. Request reviewers (CODEOWNERS will auto-assign architecture reviewers for `.xaml` and `.cs` files)

### Merge Requirements

Before merging, ALL merge gates must pass:

| Gate | Enforced By |
|------|-------------|
| Build passes | CI status check |
| Host test passes | CI status check |
| Code analysis clean | CI status check |
| 2 approving reviews | GitHub branch protection |
| Architecture review | CODEOWNERS + team discipline |
| Architecture review checklist | Manual review |
| No regressions | Manual testing |
| Correct merge strategy | Branch protection rules |

### Merging

```bash
# Use GitHub UI: Squash and merge for feature → develop
# This keeps integration history clean

# After merge, delete the feature branch
git checkout develop
git pull origin develop
git branch -d feature/phase-01-design-system-core
git push origin --delete feature/phase-01-design-system-core
```

### Hotfix Process

```bash
# Create hotfix branch from develop
git checkout -b hotfix/critical-rendering-fix develop

# Fix, commit, push
git commit -m "fix(rendering): fix black window on DPI change"
git push -u origin hotfix/critical-rendering-fix

# Open PR to develop (squash merge)
# Cherry-pick to main if needed

# After both merges, delete hotfix branch
git branch -d hotfix/critical-rendering-fix
git push origin --delete hotfix/critical-rendering-fix
```

---

## 5. Examples

### Valid Branch Names

```text
feature/phase-00-governance-foundation
feature/phase-01-design-system-core
feature/phase-02-rendering-infrastructure
feature/phase-03-theme-engine-v2
hotfix/critical-rendering-fix
release/phase-01
```

### Invalid Branch Names

```text
feature/phase1-design           # Missing zero-padding
feature/phase-01-Design Core    # Uppercase and spaces
feature/design-system           # Missing phase number
fix/bug-123                     # Should use hotfix/ prefix
my-feature-branch               # Missing phase-NN format
```

---

## 6. AI Agent Branch Naming

AI agents creating branches MUST follow the same naming convention. When an AI agent creates a feature branch:

1. Determine the current phase number
2. Use the branch name format: `feature/phase-<NN>-<short-name>`
3. Include a descriptive short name related to the task
4. Follow the same lifecycle: create → work → PR → merge → delete

Example AI agent workflow:

```bash
# Agent creates branch for Phase 1 token work
git checkout -b feature/phase-01-primitive-tokens develop
git push -u origin feature/phase-01-primitive-tokens

# Agent commits and opens PR
git commit -m "feat(theme): add Primitive.Blue.500 through Primitive.Blue.900 tokens"
git push
gh pr create --base develop --title "Phase 01: Add Blue Primitive Tokens"
```

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21