# Review Checklist

**Project**: Som3a Add-in 2026
**Version**: 1.1.0
**Date**: 2026-05-21
**Ratified by**: Architecture Review Team
**Applies to**: All phases, all pull requests
**Governed by**: Constitution v1.2.0, Merge Gate Contract, Review Checklist Contract

---

## Pre-Review: Automated Gate Verification

Before beginning manual review, confirm all automated gates have passed:

- [ ] Build verification passed (CI green)
- [ ] Host compatibility test passed (CI green)
- [ ] Automated code analysis has no unresolved critical issues

If any automated gate failed and was overridden, verify the incident ticket is linked and justification is documented.

> **Override Policy**: Build, host test, and code analysis gates may be overridden by tech leads/owners with a documented incident ticket. Architecture review and two-reviewer requirements are NEVER overrideable. See [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) for full details.

---

## Mandatory Review Gates

Every phase MUST pass ALL gates before proceeding to the next phase.

---

## Gate 1: Local Manual Testing

### Build

- [ ] `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` passes with 0 errors
- [ ] `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Release` passes with 0 errors
- [ ] No XAML parse errors
- [ ] No missing resource key warnings

### Excel Host Test

- [ ] Open window from Excel VSTO add-in
- [ ] No black window rendering
- [ ] Window can be moved, minimized, maximized, closed
- [ ] Excel remains interactive while window is open
- [ ] Focus returns to Excel after window closes
- [ ] No crash on rapid open/close

### DPI Test

- [ ] Test at 100% (96 DPI)
- [ ] Test at 125% (120 DPI)
- [ ] Test at 150% (144 DPI)
- [ ] All text readable
- [ ] No clipping or overflow
- [ ] Controls scale proportionally

### Popup Test

- [ ] ComboBox popup opens
- [ ] Popup content fully visible
- [ ] No clipping from parent containers
- [ ] Shadow renders correctly
- [ ] Keyboard navigation works (arrows, escape, tab, enter)

### Rendering Test

- [ ] No transparency artifacts
- [ ] No flicker during theme switch
- [ ] Animations smooth (if applicable)
- [ ] Safe mode renders correctly (if triggered)

---

## Gate 2: GitHub Pull Request Review

### Architecture Review

- [ ] New files follow project folder structure
- [ ] Token naming follows [TOKEN_RULES.md](TOKEN_RULES.md)
- [ ] Resource loading order documented if changed
- [ ] No duplicate architecture
- [ ] Existing structures reused where possible

### Style Review

- [ ] Consistent with existing code style
- [ ] Proper indentation and formatting
- [ ] Meaningful variable/resource names
- [ ] Comments where logic is non-obvious

### Performance Review

- [ ] No nested DropShadowEffect
- [ ] No BlurEffect on scrollable containers
- [ ] Virtualization enabled on large lists
- [ ] Animations ≤ 200ms
- [ ] No obvious memory leaks

---

## Gate 3: CodeRabbit Review

### Code Smells

- [ ] No magic numbers/strings
- [ ] No overly long methods
- [ ] No duplicated code blocks
- [ ] Proper exception handling

### Performance Issues

- [ ] No blocking UI thread operations
- [ ] Async/await used for I/O
- [ ] No unnecessary object creation

### MVVM Violations

- [ ] No business logic in code-behind
- [ ] ViewModels do not reference UI controls
- [ ] Services do not act as ViewModels

### Memory Issues

- [ ] IDisposable objects disposed
- [ ] Event handlers unsubscribed
- [ ] No static references to short-lived objects

### Resource Leaks

- [ ] No unreleased COM objects (Excel interop)
- [ ] No unreleased file handles
- [ ] Brushes shared, not duplicated

### Duplicate Logic

- [ ] No copy-pasted code
- [ ] Shared logic extracted to helpers/services
- [ ] No duplicate style definitions

### Inline Values

- [ ] No inline colors
- [ ] No inline shadows
- [ ] No inline margins
- [ ] No inline font sizes

---

## Gate 4: Manual Architectural Review

### Token & Resource Compliance

- [ ] **DynamicResource usage**: All themeable properties (Background, Foreground, BorderBrush, effects) use `{DynamicResource}`. No `StaticResource` for themeable properties.
- [ ] **No inline colors**: Zero hardcoded color values (`#......`) in control templates outside primitive definitions.
- [ ] **No inline shadows**: Zero inline `DropShadowEffect` definitions outside `Effects/Shadows.xaml` and `Effects/Glow.xaml`.
- [ ] **Token naming consistency**: New tokens follow the Primitive → Semantic → Component hierarchy:
  - Primitive: `Primitive.<Color>.<Shade>`
  - Semantic: `Brush.<Category>.<State>`
  - Component: `Component.<Control>.<Part>.<State>`

### Code Quality

- [ ] **No duplicate styles**: No duplicate or shadowed style definitions.
- [ ] **No hardcoded spacing**: All margins, padding, and sizes use spacing tokens (`Spacing.*`, `Padding.*`).
- [ ] **No hardcoded radius**: All corner radii use radius tokens (`Radius.*`).
- [ ] **No business logic in code-behind**: All business logic resides in Services, ViewModels, or Controllers.

### Excel & Rendering Safety

- [ ] **Excel-safe rendering**: Window changes consider [WindowRenderModeDetector](EXCEL_RENDERING_RULES.md) and fallback mode.
- [ ] **AllowsTransparency**: Popups use `AllowsTransparency="False"` where applicable.
- [ ] **SnapsToDevicePixels**: Controls use `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"`.
- [ ] **No BlurEffect on scrollables**: Scrollable containers do not use `BlurEffect`.

### Performance & Animation

- [ ] **Animation duration ≤200ms**: All animations complete within 200ms.
- [ ] **GPU-safe animations**: No layout thrashing; animations use GPU-composable properties.
- [ ] **Virtualization**: DataGrid and ListView use `EnableRowVirtualization="True"` where applicable.

### MVVM & Architecture

- [ ] **MVVM separation**: ViewModels do not access UI controls directly or manipulate ResourceDictionaries.
- [ ] **Service isolation**: Services do not behave as ViewModels.
- [ ] **No direct service creation in views**: Services are injected or resolved, not instantiated in XAML code-behind.

### Reuse & Extension

- [ ] **Inspect before create**: Contributors inspected existing structures before creating new tokens, controls, or dictionaries.
- [ ] **Reuse before duplicate**: Existing structures were reused or extended; no duplicate architecture introduced.

---

## Post-Review: Sign-Off

If ALL items pass:

- [ ] Submit "Approve" with comment: "Architecture review passed. All checklist items verified."

If ANY item fails:

- [ ] Submit "Request changes" with comment referencing specific failed checklist items
- [ ] Re-review after contributor addresses issues

---

## Review Gate Process

The review gate is a post-phase validation process confirming all quality checks were completed:

| Step | Gate | Description | Owner |
|------|------|-------------|-------|
| 1 | Local Testing | Build, Excel host, DPI, popup, rendering testing | Developer |
| 2 | PR Review | Pull request reviewed and approved | Reviewers |
| 3 | Automated Analysis | CI build, host test, code analysis | CI Pipeline |
| 4 | Architecture Review | Manual architecture review using this checklist | Architecture Reviewer |

A phase is considered complete ONLY when all four gates have passed. See [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) for merge gate enforcement details.

---

## Override Policy

### Who Can Override

Tech leads and repository owners may override automated gates.

### What Can Be Overridden

| Gate | Overrideable | Procedure |
|------|-------------|-----------|
| Build verification (MG-001) | Yes | Document incident ticket, comment on PR |
| Host compatibility test (MG-002) | Yes | Document incident ticket, comment on PR |
| Code analysis (MG-003) | Yes | Document incident ticket, comment on PR |
| Two-reviewer requirement (MG-004) | **No** | Never overrideable |
| Architecture reviewer (MG-005) | **No** | Never overrideable |
| Architecture review checklist (MG-006) | **No** | Never overrideable |
| No regressions (MG-007) | **No** | Never overrideable |

### Override Procedure

1. Create an incident ticket documenting the failure
2. Comment on the PR: "Override approved per incident #{ticket-number}. Manual verification completed."
3. Administrator merges with elevated privileges
4. All overrides are logged in the PR audit trail

---

## AI Agent Compliance

AI agents generating code for this project MUST:

1. Read this checklist before submitting code
2. Self-verify all applicable items before creating a pull request
3. Reference checklist items in PR description (e.g., "Verified: no inline colors, DynamicResource used throughout")
4. Target: **80% first-pass success rate** on architecture review (per SC-004)

AI self-verification checklist:

- [ ] All new colors use semantic tokens (no inline hex values)
- [ ] All new spacing uses spacing tokens (no hardcoded margins)
- [ ] All new radius uses radius tokens (no hardcoded corners)
- [ ] All themeable properties use DynamicResource
- [ ] No inline DropShadowEffect definitions
- [ ] No business logic in code-behind
- [ ] Excel rendering safety considered
- [ ] Animations ≤ 200ms

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [TOKEN_RULES.md](TOKEN_RULES.md) | Token naming conventions and layers |
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Rendering and animation budget |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [MVVM_RULES.md](MVVM_RULES.md) | MVVM architecture enforcement |
| [ACCESSIBILITY_RULES.md](ACCESSIBILITY_RULES.md) | Keyboard navigation and screen reader |
| [UI_GUIDELINES.md](UI_GUIDELINES.md) | Windows 11 Fluent design rules |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |
| [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) | Branch protection and merge rules |
| [BRANCH_NAMING.md](BRANCH_NAMING.md) | Branch naming conventions |

---

## Sign-Off

| Gate | Reviewer | Status | Date |
|------|----------|--------|------|
| Local Manual Testing | | Pass / Fail | |
| GitHub PR Review | | Pass / Fail | |
| CodeRabbit Review | | Pass / Fail | |
| Manual Architecture Review | | Pass / Fail | |

**Phase can proceed to next phase ONLY when ALL gates are PASS.**

---

**Version**: 1.1.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team