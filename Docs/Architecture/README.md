# Som3a Add-in 2026 — Architecture Governance

**Version**: 1.0.0
**Last Updated**: 2026-05-21
**Ratified by**: Architecture Review Team
**Update Frequency**: Per phase (before merge gate review)

---

## Purpose

This directory contains all governance documents that define architectural rules, review standards, and execution guidelines for the Som3a Add-in 2026 project. All contributors — human and AI — must follow these documents when making changes to the codebase.

---

## Documents

| Document | Purpose | Update Frequency |
|----------|---------|-------------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards, NEVER/ALWAYS rules, self-verification | Per phase |
| [TOKEN_RULES.md](TOKEN_RULES.md) | Token naming conventions (Primitive, Semantic, Component, utility layers) | Per phase |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist and merge gate requirements | Per phase |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Animation budget, virtualization, rendering constraints | Per phase |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules, Excel-safe popup guidelines | Per phase |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | WindowRenderModeDetector, safe mode, DPI handling | Per phase |
| [MVVM_RULES.md](MVVM_RULES.md) | Service/ViewModel separation, code-behind rules | Per phase |
| [ACCESSIBILITY_RULES.md](ACCESSIBILITY_RULES.md) | Keyboard navigation, focus visibility, screen reader preparation | Per phase |
| [UI_GUIDELINES.md](UI_GUIDELINES.md) | Windows 11 Fluent design system, DynamicResource mandate | Per phase |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture, no inline DropShadowEffect | Per phase |
| [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) | Branch protection rules, merge gates, override policy | As needed |
| [BRANCH_NAMING.md](BRANCH_NAMING.md) | Branch naming convention, feature branch lifecycle | As needed |
| [AUDIT_REPORT.md](AUDIT_REPORT.md) | Architecture audit findings and status tracking | Per phase |

---

## Governance Hierarchy

```text
Constitution (.specify/memory/constitution.md)
    ↓ overrides
Architecture Governance (Docs/Architecture/)
    ↓ informs
Review Process (REVIEW_CHECKLIST.md, BRANCH_PROTECTION.md)
    ↓ enforces
Branch Protection & Merge Gates
```

---

## How to Use These Documents

### For Developers

1. Read [BRANCH_NAMING.md](BRANCH_NAMING.md) before creating a feature branch
2. Follow [AGENT_RULES.md](AGENT_RULES.md) rules when writing code
3. Use [TOKEN_RULES.md](TOKEN_RULES.md) for naming new tokens
4. Check [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) before opening a PR

### For Architecture Reviewers

1. Use [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) as the primary review checklist
2. Reference specialized documents for domain-specific checks:
   - Token compliance → [TOKEN_RULES.md](TOKEN_RULES.md)
   - Performance → [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md)
   - Excel rendering → [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md)
   - MVVM → [MVVM_RULES.md](MVVM_RULES.md)

### For AI Agents

1. Read [AGENT_RULES.md](AGENT_RULES.md) before generating code
2. Self-verify against [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) before creating PRs
3. Target 80% first-pass success rate on architecture review
4. Inspect existing structures before creating new ones

---

## Document Maintenance

- Each document includes version, date, and ratification metadata
- Documents are updated per phase or as needed
- All changes must follow the merge gate process (see [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md))
- The constitution (v1.2.0) takes precedence over any governance document

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team