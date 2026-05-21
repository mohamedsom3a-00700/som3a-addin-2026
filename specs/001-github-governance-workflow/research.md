# Research: GitHub Enterprise Governance & Review Workflow

**Feature**: GitHub Enterprise Governance & Review Workflow  
**Date**: 2026-05-21  
**Source**: [spec.md](spec.md)

---

## 1. Branch Protection Strategy

**Unknown**: How to enforce the branch model (release, integration, feature) in GitHub.

**Research**: GitHub provides native branch protection rules at Settings > Branches. Key capabilities:
- Require pull request reviews before merging
- Require status checks to pass (CI builds, tests)
- Restrict pushes that create files larger than a specified size
- Require linear history
- Include administrators in restrictions

**Decision**: Use GitHub native branch protection rules for both `main` (release) and `develop` (integration) branches.

**Rationale**: Native rules are sufficient, require no third-party tools, and integrate directly with GitHub Actions CI. The spec requires branch protection as a MUST; GitHub's built-in feature satisfies this.

**Alternatives considered**:
- Third-party tools like Mergify or Kodiak — rejected because they add complexity and cost; native rules are sufficient.
- Git pre-commit hooks — rejected because they are client-side and can be bypassed; server-side branch protection is required.

---

## 2. Reviewer Requirements and CODEOWNERS

**Unknown**: How to enforce "two reviewers minimum: one general code reviewer and one designated architecture reviewer."

**Research**: GitHub CODEOWNERS file can assign required reviewers to specific directories or files. GitHub branch protection can require a minimum number of approving reviews. However, GitHub cannot natively enforce "one of the reviewers must be from the architecture team."

**Decision**: Use CODEOWNERS to assign architecture reviewers to all `.xaml`, `.cs`, and `Docs/Architecture/` files. Set branch protection to require 2 approving reviews. Rely on team discipline + review checklist to ensure one review is architecture-focused.

**Rationale**: GitHub's required review count + CODEOWNERS assignment is the practical maximum automation. The architecture review checklist (FR-012) serves as the enforcement mechanism for quality, not just count.

**Alternatives considered**:
- Custom GitHub App to validate reviewer roles — rejected as over-engineering for a single repository.
- Manual assignment by tech leads — rejected because it doesn't scale with AI-generated contributions.

---

## 3. Merge Commit Strategy Enforcement

**Unknown**: How to enforce squash-and-merge for feature branches and standard merge for release PRs.

**Research**: GitHub branch protection rules allow enabling/disabling specific merge methods per branch:
- "Allow squash merging"
- "Allow rebase merging"
- "Allow merge commits"

However, GitHub cannot enforce different strategies for PRs *targeting* the same branch based on *source* branch type.

**Decision**: Enable squash merging for `develop` (integration) branch; disable merge commits on `develop`. Enable standard merge commits for `main` (release) branch; disable squash on `main`. Document in quickstart.md that feature PRs must use squash, and release PRs must use standard merge.

**Rationale**: This is the closest achievable with GitHub's native controls. The merge strategy is enforced at the branch level, which aligns with the spec's intent: feature → integration is squash, integration → release is standard merge.

**Alternatives considered**:
- GitHub Actions workflow to validate merge method — rejected as fragile and adds CI complexity.
- Team discipline + PR template reminder — accepted as complementary to branch-level enforcement.

---

## 4. CI Pipeline Integration

**Unknown**: How automated build, host test, and code analysis gates integrate with pull requests.

**Research**: GitHub Actions workflows triggered on `pull_request` events automatically register as status checks. Branch protection can require specific status checks to pass. The existing CI pipeline (per spec assumptions) should already produce these checks.

**Decision**: Configure branch protection on `develop` to require all existing CI status checks (build, host test, code analysis). No new CI pipeline needed.

**Rationale**: The spec assumes CI exists (Assumption 3). Reusing existing infrastructure avoids duplication.

**Alternatives considered**:
- Create a new, separate CI workflow just for merge gates — rejected; unnecessary duplication.

---

## 5. Administrator Override Mechanism

**Unknown**: How to implement the documented override policy (tech leads/owners may override automated analysis and host test failures after an incident ticket).

**Research**: GitHub branch protection has an "Include administrators" checkbox. If unchecked, even admins cannot bypass rules. There is no native "override with justification" feature. The policy must be procedural, not technical.

**Decision**: Document the override policy in `Docs/Architecture/AGENT_RULES.md` and `REVIEW_CHECKLIST.md`. Keep "Include administrators" checked (so admins can merge if needed), but make the override policy a mandatory team procedure. All overrides are logged in the PR comments and linked to an incident ticket.

**Rationale**: Complete technical enforcement of override policy would require custom GitHub Apps. A documented procedural policy with audit logging (PR comments + incident tickets) is sufficient for a single-repository team.

**Alternatives considered**:
- Custom GitHub App to require ticket references — rejected as over-engineering.

---

## 6. AI Execution Rules Documentation Format

**Unknown**: Best format for AI execution rules and token naming standards so AI agents can consume them.

**Research**: AI agents read Markdown files. Structured rules with clear MUST/SHOULD/MAY statements, examples, and checklists are most parseable. The existing `Docs/Architecture/` structure from `implementation_plan.md` provides the right granularity.

**Decision**: Use Markdown with structured sections: Rules (MUST/NEVER/ALWAYS), Examples, Checklist. Follow the exact format from `implementation_plan.md` Phase 0 governance deliverables.

**Rationale**: This matches the existing project documentation style and is human-readable while being structured enough for AI agents.

**Alternatives considered**:
- YAML/JSON machine-readable rules — rejected; harder for humans to maintain and review.

---

## 7. Pull Request Template Design

**Unknown**: What fields/checklists should be in the PR template.

**Research**: Best practice PR templates include: description, type of change, testing performed, checklist (build pass, tests pass, review done), and linked issues.

**Decision**: Create a PR template with:
- Phase reference (e.g., "Phase 1 - Design System Core")
- Summary of changes
- Testing performed
- Merge gate checklist (build, host test, analysis, architecture review)
- Related issues/tasks

**Rationale**: The template serves as both a reminder and a lightweight audit trail for merge gates.

---

## Summary of Resolutions

| Unknown | Decision | Status |
|---------|----------|--------|
| Branch protection mechanism | GitHub native branch protection | Resolved |
| Two-reviewer enforcement (one architecture) | CODEOWNERS + branch protection (2 reviews) + checklist | Resolved |
| Merge strategy enforcement | Branch-level merge method settings + documentation | Resolved |
| CI gate integration | Reuse existing GitHub Actions; require status checks | Resolved |
| Admin override mechanism | Procedural policy + PR comment audit trail | Resolved |
| AI rules format | Markdown with MUST/NEVER/ALWAYS structure | Resolved |
| PR template content | Template with phase, summary, testing, gate checklist | Resolved |

All NEEDS CLARIFICATION items resolved. Phase 0 complete.
