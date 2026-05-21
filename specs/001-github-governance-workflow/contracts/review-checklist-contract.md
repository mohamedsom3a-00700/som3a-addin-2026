# Review Checklist Contract

**Feature**: GitHub Enterprise Governance & Review Workflow  
**Date**: 2026-05-21  
**Source**: [spec.md](spec.md)

---

## Purpose

Defines the mandatory checklist items that the architecture reviewer must verify before approving a pull request. This contract ensures compliance with the project constitution and implementation plan standards.

---

## Contract

### Reviewer Responsibility

The designated architecture reviewer is accountable for verifying all items in this checklist. Approval without completing the checklist is a violation of the merge gate contract.

---

### Pre-Review: Automated Gate Verification

Before beginning manual review, confirm:

- [ ] Build verification passed (CI green)
- [ ] Host compatibility test passed (CI green)
- [ ] Automated code analysis has no unresolved critical issues

If any automated gate failed and was overridden, verify the incident ticket is linked and justification is documented.

---

### Architecture Review Checklist

#### Token & Resource Compliance

- [ ] **DynamicResource usage**: All themeable properties (Background, Foreground, BorderBrush, effects) use `{DynamicResource}`. No `StaticResource` for themeable properties.
- [ ] **No inline colors**: Zero hardcoded color values (`#......`) in control templates outside primitive definitions.
- [ ] **No inline shadows**: Zero inline `DropShadowEffect` definitions outside `Effects/Shadows.xaml` and `Effects/Glow.xaml`.
- [ ] **Token naming consistency**: New tokens follow the Primitive → Semantic → Component hierarchy:
  - Primitive: `Primitive.<Color>.<Shade>`
  - Semantic: `Brush.<Category>.<State>`
  - Component: `Component.<Control>.<Part>.<State>`

#### Code Quality

- [ ] **No duplicate styles**: No duplicate or shadowed style definitions.
- [ ] **No hardcoded spacing**: All margins, padding, and sizes use spacing tokens (`Spacing.*`, `Padding.*`).
- [ ] **No hardcoded radius**: All corner radii use radius tokens (`Radius.*`).
- [ ] **No business logic in code-behind**: All business logic resides in Services, ViewModels, or Controllers.

#### Excel & Rendering Safety

- [ ] **Excel-safe rendering**: Window changes consider `WindowRenderModeDetector` and fallback mode.
- [ ] **AllowsTransparency**: Popups use `AllowsTransparency="False"` where applicable.
- [ ] **SnapsToDevicePixels**: Controls use `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"`.
- [ ] **No BlurEffect on scrollables**: Scrollable containers do not use `BlurEffect`.

#### Performance & Animation

- [ ] **Animation duration ≤200ms**: All animations complete within 200ms.
- [ ] **GPU-safe animations**: No layout thrashing; animations use GPU-composable properties.
- [ ] **Virtualization**: DataGrid and ListView use `EnableRowVirtualization="True"` where applicable.

#### MVVM & Architecture

- [ ] **MVVM separation**: ViewModels do not access UI controls directly or manipulate ResourceDictionaries.
- [ ] **Service isolation**: Services do not behave as ViewModels.
- [ ] **No direct service creation in views**: Services are injected or resolved, not instantiated in XAML code-behind.

#### Reuse & Extension

- [ ] **Inspect before create**: Contributors inspected existing structures before creating new tokens, controls, or dictionaries.
- [ ] **Reuse before duplicate**: Existing structures were reused or extended; no duplicate architecture introduced.

---

### Post-Review: Sign-Off

If ALL items pass:

- [ ] Submit "Approve" with comment: "Architecture review passed. All checklist items verified."

If ANY item fails:

- [ ] Submit "Request changes" with comment referencing specific failed checklist items
- [ ] Re-review after contributor addresses issues

---

## AI Agent Compliance

AI agents generating code for this project MUST:

1. Read this checklist before submitting code
2. Self-verify all applicable items before creating a pull request
3. Reference checklist items in PR description (e.g., "Verified: no inline colors, DynamicResource used throughout")

Target: 80% of AI-generated contributions pass architecture review on first attempt (SC-004).

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-05-21 | Initial contract based on implementation_plan.md governance requirements |
