# AI Agent Execution Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Applies to**: OpenCode, Spec Kit, CodeRabbit, all AI agents

---

## 1. Reuse-First Mandate

### BEFORE creating ANY new file, token, control, dictionary, or architecture:

1. **Inspect** current implementation
2. **Reuse** existing structures
3. **Extend** existing systems
4. **Avoid** duplicate architecture

### Forbidden Actions

- Creating a new token when a semantically equivalent token already exists
- Creating a new control style when an existing style can be extended
- Creating a new service when an existing service can be enhanced
- Duplicating ResourceDictionary content across multiple files

---

## 2. Inline Value Prohibition

### NEVER use inline values for:

- Colors (`Color="#......"`, `Color="White"` outside primitive definitions)
- Shadows (`DropShadowEffect` defined inside a control template)
- Margins (`Margin="12,8"` without token reference)
- Font sizes (`FontSize="14"` without token reference)
- Border thicknesses (`BorderThickness="1"` without token reference)
- Corner radius (`CornerRadius="10"` without token reference)

### ALWAYS use:

- `{DynamicResource TokenName}` for themeable properties
- `{StaticResource TokenName}` for immutable primitive references only
- Centralized effect keys from `Effects/Shadows.xaml` and `Effects/Glow.xaml`

---

## 3. Token Layer Compliance

### Controls MUST consume tokens in this priority:

1. **Component Tokens** (`Component.Button.Padding`)
2. **Semantic Tokens** (`Brush.Background.Primary`)
3. **Primitive Tokens** (`Primitive.Blue.500`) — only inside semantic definitions

### NEVER:

- Reference primitive tokens directly from control templates
- Hardcode hex values in semantic token definitions
- Mix semantic and legacy flat keys in new work

---

## 4. MVVM Enforcement

### Business logic MUST reside in:

- Services
- ViewModels
- Controllers (if applicable)

### Code-behind MUST remain UI-only:

- Event routing
- Focus management
- Animation triggers
- Visual state transitions

### NEVER in code-behind:

- Business logic
- Data transformation
- Service instantiation (use DI in Phase 6+)
- ResourceDictionary manipulation
- Theme mutation (only ThemeManager)

---

## 5. Excel-Safe Rendering

### ALL window and popup code MUST:

- Respect `WindowRenderModeDetector` output
- Support `FallbackSafe` mode (reduced effects, no transparency)
- Test in Excel VSTO host before merge
- Avoid `AllowsTransparency="True"` unless explicitly required and tested

---

## 6. Branch Naming Standards

### Pattern

```text
feature/phase-<NN>-<short-kebab-name>
```

### Examples

| Phase | Branch Name |
|-------|-------------|
| Phase 0 | `feature/phase-00-governance-foundation` |
| Phase 1 | `feature/phase-01-design-system-core` |
| Phase 2 | `feature/phase-02-rendering-infrastructure` |
| Phase 3 | `feature/phase-03-theme-engine-v2` |
| Phase 4 | `feature/phase-04-control-standardization` |
| Phase 5 | `feature/phase-05-navigation-shell` |
| Phase 6 | `feature/phase-06-mvvm-cleanup` |
| Phase 7 | `feature/phase-07-settings-ux` |
| Phase 8 | `feature/phase-08-diagnostics` |
| Phase 9 | `feature/phase-09-plugin-platform` |
| Phase 10 | `feature/phase-10-enterprise-polish` |
| Phase 11 | `feature/phase-11-legacy-migration` |

### Hotfix Branches

```text
hotfix/<description>
```

---

## 7. Commit Message Standards

### Format

```text
type(scope): subject

body (optional)
```

### Types

- `feat` — new feature
- `fix` — bug fix
- `docs` — documentation only
- `style` — formatting, missing semicolons, etc.
- `refactor` — code change that neither fixes a bug nor adds a feature
- `perf` — performance improvement
- `test` — adding tests
- `chore` — build process, dependencies, etc.

### Examples

```text
feat(theme): add elevation token system
fix(button): replace inline hover color with semantic token
docs(architecture): add popup rendering rules
refactor(colors): extract primitive layer from semantic tokens
```

---

## 8. Self-Check Before File Creation

Before writing any new file, ask:

1. Does an equivalent file already exist?
2. Can I extend an existing file instead?
3. Am I following the token layer rules?
4. Am I using DynamicResource for themeable properties?
5. Am I referencing centralized effects (not inline)?
6. Would this pass the architecture review checklist?

---

## 9. Review Gate Compliance

### AI agents MUST NOT proceed to the next phase until:

- [ ] Build passes (`msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug`)
- [ ] No inline colors detected in changed files
- [ ] No inline shadows detected in changed files
- [ ] All new tokens follow naming standards
- [ ] All new controls use semantic tokens
- [ ] Excel host compatibility considered

---

## 10. AI Self-Verification & First-Pass Success

### Target: 80% First-Pass Success Rate (SC-004)

AI agents MUST self-verify code before creating a pull request. The target is that **80% of AI-generated contributions pass architecture review on the first attempt** without requiring corrections.

### Self-Verification Checklist

Before submitting any PR, verify:

- [ ] All new colors use semantic tokens (no inline hex values outside primitive definitions)
- [ ] All new spacing uses spacing tokens (no hardcoded margins without token reference)
- [ ] All new radius uses radius tokens (no hardcoded corners without token reference)
- [ ] All themeable properties use `{DynamicResource}` (no `StaticResource` for themeable properties)
- [ ] No inline `DropShadowEffect` definitions (use centralized effects from `Effects/Shadows.xaml`)
- [ ] No business logic in code-behind (all logic in Services, ViewModels, Controllers)
- [ ] Excel rendering safety considered (WindowRenderModeDetector, AllowsTransparency="False" for popups)
- [ ] Animations ≤ 200ms duration
- [ ] MVVM separation maintained (ViewModels do not access UI controls)
- [ ] No duplicate styles or tokens (inspect before create, reuse before duplicate)

### Reference Checklist Items in PR Description

When creating a PR, reference the checklist items you verified:

```text
Verified: no inline colors, DynamicResource used throughout, no inline shadows,
MVVM separation maintained, Excel-safe rendering considered.
```

See [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) for the full architecture review checklist.

---

**Enforcement**: Constitutional violations MUST block implementation approval.

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Full architecture review checklist |
| [TOKEN_RULES.md](TOKEN_RULES.md) | Token naming conventions |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Rendering and animation budget |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [MVVM_RULES.md](MVVM_RULES.md) | MVVM architecture enforcement |
| [ACCESSIBILITY_RULES.md](ACCESSIBILITY_RULES.md) | Accessibility rules |
| [UI_GUIDELINES.md](UI_GUIDELINES.md) | UI design guidelines |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |
| [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md) | Branch protection and merge rules |
| [BRANCH_NAMING.md](BRANCH_NAMING.md) | Branch naming conventions |
