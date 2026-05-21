---
phase: ""
summary: ""
testing: ""
---

## Summary

<!-- Describe your changes in detail -->

## Phase Reference

<!-- Which phase does this PR belong to? e.g., "Phase 01 - Design System Core" -->
Phase: 

## Type of Change

- [ ] Feature (new functionality)
- [ ] Bug fix (non-breaking fix)
- [ ] Refactor (no functional change)
- [ ] Documentation
- [ ] Theme/visual change
- [ ] Performance improvement

## Testing Performed

<!-- Describe testing done: local build, Excel host test, DPI test, etc. -->

- [ ] Debug build passes: `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug`
- [ ] Release build passes
- [ ] Tested in Excel VSTO host
- [ ] Tested at 100% and 150% DPI

## Merge Gate Checklist

Before merging, verify ALL gates are satisfied:

### Automated Gates

- [ ] Build verification passes (CI green)
- [ ] Host compatibility test passes
- [ ] Automated code analysis has no unresolved critical issues

### Review Gates

- [ ] At least 2 approving reviews (one must be architecture reviewer)
- [ ] Architecture review checklist completed: [REVIEW_CHECKLIST.md](../../Docs/Architecture/REVIEW_CHECKLIST.md)

### Quality Gates

- [ ] No inline colors outside primitive definitions
- [ ] No inline DropShadowEffect outside Effects/ directory
- [ ] All themeable properties use DynamicResource
- [ ] New tokens follow naming standards: [TOKEN_RULES.md](../../Docs/Architecture/TOKEN_RULES.md)
- [ ] Excel-safe rendering considered: [EXCEL_RENDERING_RULES.md](../../Docs/Architecture/EXCEL_RENDERING_RULES.md)

### Merge Strategy

<!-- Select the correct merge strategy for this PR -->

- [ ] **Feature → Develop**: Use **Squash and merge** (keeps integration history clean)
- [ ] **Develop → Main**: Use **Create a merge commit** (preserves milestone history)

> :warning: Incorrect merge strategy will be rejected by branch protection rules.

## Related Issues

<!-- Link related issues: Closes #123, Related #456 -->

## Screenshots (if applicable)

<!-- Add screenshots for visual changes, theme changes, or UI refactors -->

## Override Justification (if applicable)

<!-- If overriding any automated gate, document here with incident ticket reference -->
<!-- Non-overrideable: architecture review count, reviewer requirements -->

N/A