# Tasks: Design System Core

**Input**: Design documents from `/specs/004-design-system-core/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/token-api.md, quickstart.md

**Tests**: Not explicitly requested — no test tasks included.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1–US5) this task belongs to
- File paths are relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare workspace, create new token files, and audit existing state

- [X] T001 Verify git branch `004-design-system-core` exists and is checked out
- [X] T002 Audit existing token inventory in `WpfApp2/Theme/Base/Colors.xaml` — catalog all Primitive, Semantic Color, Semantic Brush, and Legacy Flat keys; document gaps against spec FR-001
- [X] T003 [P] Audit existing `WpfApp2/Theme/Base/Typography.xaml` — verify all `Style.Text.*` styles reference DynamicResource tokens per contract
- [X] T004 [P] Audit existing `WpfApp2/Theme/Base/Spacing.xaml` — catalog all Spacing.*, Padding.*, and size keys; identify missing `Padding.XSmall` per plan
- [X] T005 [P] Audit existing `WpfApp2/Theme/Base/Radius.xaml` — verify all CornerRadius keys present and complete
- [X] T006 [P] Audit existing `WpfApp2/Theme/Base/Elevation.xaml` — catalog all DropShadowEffect keys and document V8 violation (effects in Base/ instead of Effects/)
- [X] T007 [P] Audit existing `WpfApp2/Theme/Base/ComponentTokens.xaml` — catalog all 17 Component.* tokens; identify hardcoded values that should reference Spacing.*/Brush.* tokens
- [X] T008 [P] Audit existing `WpfApp2/Theme/Effects/Shadows.xaml` and `WpfApp2/Theme/Effects/Glow.xaml` — verify no duplication with Elevation.xaml; catalog all effect keys
- [X] T009 [P] Audit existing `WpfApp2/Theme/Effects/Animations.xaml` — verify all animation durations are ≤200ms per Constitution IX
- [X] T010 [P] Audit existing `WpfApp2/Theme/Base/Motion.xaml` and `WpfApp2/Theme/Base/ZIndex.xaml` and `WpfApp2/Theme/Base/Opacity.xaml` — verify completeness against contract

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core token infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T011 Add missing `Primitive.*` tokens to `WpfApp2/Theme/Base/Colors.xaml` — add `Primitive.White.33`, `Primitive.Black.8`, `Primitive.Black.53`, and all `Primitive.Transparency.*` tokens (Subtle, Light, MediumLow, Medium, Strong) per data-model
- [X] T012 [P] Add missing `Color.*` semantic tokens to `WpfApp2/Theme/Base/Colors.xaml` — add `Color.ScrollBar.Thumb`, `Color.GroupBox.Background`, `Color.Overlay.Background`, `Color.Animation.ThumbFade`, `Color.Animation.ButtonHoverDanger` and any other tokens needed for inline violation replacements per research tasks 2–3
- [X] T013 [P] Add missing `Brush.*` semantic tokens to `WpfApp2/Theme/Base/Colors.xaml` — create brushes for `Brush.ScrollBar.Thumb`, `Brush.GroupBox.Background`, `Brush.Overlay.Background` and all new Color.* tokens; each Brush MUST reference Color.* via StaticResource per FR-004
- [X] T014 [P] Add `Padding.XSmall` token to `WpfApp2/Theme/Base/Spacing.xaml` — value `4` (Thickness `4,2`) per plan requirement
- [X] T015 [P] Convert `WpfApp2/Theme/Base/Elevation.xaml` to abstraction-only — remove DropShadowEffect definitions, replace with `sys:Double` tokens (OffsetX, OffsetY, BlurRadius, Opacity) per research task 4; ensure no `<DropShadowEffect>` elements remain in this file
- [X] T016 Deploy DropShadowEffect definitions to `WpfApp2/Theme/Effects/Shadows.xaml` — add `Shadow.Card`, `Shadow.Popup`, `Shadow.Window` aliases for the former Elevation.* effects per research task 4; ensure all DropShadowEffect keys referenced by controls are present
- [X] T017 Convert legacy flat keys in `WpfApp2/Theme/Base/Colors.xaml` — for each legacy key (e.g., `Blue500`, `Slate900`, `BackgroundBrush`), add deprecation comment `<!-- DEPRECATED: Use Brush.* instead. Removal: Phase 6 -->` and redirect to reference Primitive.* or Brush.* tokens per research task 5 and FR-003
- [X] T018 Update `WpfApp2/Services/ThemeManager.cs` — add token integrity validation method per research task 6: log Debug.WriteLine warnings for missing keys and broken chains during theme load; call this method from existing theme application flow per FR-010
- [X] T019 Build and verify project compiles with no XAML parse errors after Phase 2 changes: `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug`

**Checkpoint**: Foundation ready — all Primitive, Semantic Color, Semantic Brush, Spacing, and Elevation tokens defined; DropShadowEffects centralized; ThemeManager has validation

---

## Phase 3: User Story 1 — Complete Token Coverage (Priority: P1) 🎯 MVP

**Goal**: Every design value is accessible through a named token — no hardcoded visual values remain outside token definition files

**Independent Test**: Run grep audit across all `.xaml` files for `Color="#`, hardcoded `Margin="`, `Padding="`, `FontSize="`, `FontWeight="`, `<DropShadowEffect` — all should resolve to named tokens

### Implementation for User Story 1

- [X] T020 [US1] Ensure all `Brush.*` semantic tokens in `WpfApp2/Theme/Base/Colors.xaml` have corresponding overrides in `WpfApp2/Theme/Dark/DarkColors.xaml`, `WpfApp2/Theme/Light/LightColors.xaml`, and `WpfApp2/Theme/Custom/CustomColors.xaml` — add any missing semantic overrides per FR-001
- [X] T021 [US1] Verify all `Component.*` tokens in `WpfApp2/Theme/Base/ComponentTokens.xaml` reference `Brush.*` or `Spacing.*` tokens via DynamicResource where possible per spec FR-005; replace any hardcoded values with token references
- [X] T022 [US1] Verify all `Style.Text.*` styles in `WpfApp2/Theme/Base/Typography.xaml` reference `FontFamilyPrimary`, font size tokens, font weight tokens, and `Brush.Text.*` tokens via DynamicResource per contract and spec FR-009
- [X] T023 [US1] Complete token inventory catalog — produce a comprehensive list of all Primitive.*, Color.*, Brush.*, Spacing.*, Padding.*, Typography.*, Radius.*, Elevation.*, Motion.*, ZIndex.*, Opacity.*, and Component.* tokens and verify each is referenced by at least one consumer per FR-001

**Checkpoint**: All token layers are fully defined, named, and referenceable. Grep audit should find zero unmapped visual values in token definition files.

---

## Phase 4: User Story 2 — Inline Value Elimination (Priority: P1)

**Goal**: Zero inline hex colors, hardcoded corner radii, and inline DropShadowEffect definitions remain in control templates or window files

**Independent Test**: Grep all `.xaml` files for `Color="#` (outside Base/Colors, Dark/DarkColors, Light/LightColors, Custom/CustomColors), `CornerRadius="` (outside base dictionaries), and `<DropShadowEffect` (outside Effects/) — all results must be zero

### Implementation for User Story 2

- [X] T024 [P] [US2] Replace V1 — 6 inline hex GradientStop colors in `WpfApp2/Views/SettingsWindow.xaml` (lines 206–258) with `{DynamicResource Color.Background.RootStart}`, `{DynamicResource Color.Background.RootEnd}`, and new Light theme color tokens per research task 2
- [X] T025 [P] [US2] Replace V2 — inline hex `#55FFFFFF` in `WpfApp2/Theme/Controls/ScrollBarStyles.xaml` (line 22) with `{DynamicResource Brush.ScrollBar.Thumb}` or `{DynamicResource Color.ScrollBar.Thumb}`
- [X] T026 [P] [US2] Replace V3 — inline hex `#14000000` in `WpfApp2/Theme/Controls/GroupBoxStyles.xaml` (line 13) with `{DynamicResource Brush.GroupBox.Background}`
- [X] T027 [P] [US2] Replace V4 — inline hex `#88000000` in `WpfApp2/Controls/LoadingOverlay.xaml` (line 13) with `{DynamicResource Brush.Overlay.Background}`
- [X] T028 [P] [US2] Replace V5 — named color `White` Foreground in `WpfApp2/Controls/Toast/ToastWindow.xaml` (line 21) with `{DynamicResource Brush.Text.OnAccent}`
- [X] T029 [P] [US2] Replace V6 — inline hex `#22FF4757` ColorAnimation in `WpfApp2/Theme/WindowAnimations.xaml` (line 82) with `Color.Animation.ButtonHoverDanger` token or Opacity animation approach per research task 3
- [X] T030 [P] [US2] Replace V7 — inline hex `#66FFFFFF` ColorAnimation in `WpfApp2/Theme/Controls/ScrollViewerStyles.xaml` (line 78) with `Color.Animation.ThumbFade` token or Opacity animation per research task 3
- [X] T031 [US2] Replace all inline hex colors in `WpfApp2/Theme/Controls/ButtonStyles.xaml` with `Brush.Button.*` semantic tokens per FR-006
- [X] T032 [P] [US2] Replace all inline hex colors in `WpfApp2/Theme/Controls/DataGridStyles.xaml` with `Brush.DataGrid.*` or `Brush.Control.*` semantic tokens per FR-006
- [X] T033 [P] [US2] Replace all inline hex colors in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` with `Brush.ComboBox.*` semantic tokens per FR-006
- [X] T034 [P] [US2] Resolve V8 — remove DropShadowEffect definitions from `WpfApp2/Theme/Base/Elevation.xaml` (completed in T015); verify controls reference `Shadow.*` keys from `WpfApp2/Theme/Effects/Shadows.xaml` per FR-007
- [X] T035 [US2] Replace any remaining inline hex colors in `WpfApp2/MainWindow.xaml` with corresponding `Brush.*` semantic tokens per FR-006
- [X] T036 [US2] Audit all other `WpfApp2/Theme/Controls/*.xaml` files for inline hex colors not covered by V1–V8 — replace any found with `Brush.*` or `Color.*` tokens

**Checkpoint**: Grep for `Color="#` in Controls/*.xaml and all window .xaml files returns zero. Grep for `<DropShadowEffect` outside Effects/ returns zero. Theme switching still works.

---

## Phase 5: User Story 3 — Token Layer Integrity (Priority: P2)

**Goal**: Strict Primitive → Semantic → Component → Control Template chain with no broken references, circular dependencies, or direct hex in semantic tokens

**Independent Test**: Validate every `Brush.*` token references a `Primitive.*` or `Color.*` (not hex); every `Component.*` token references a `Brush.*` or `Spacing.*` token; no token references itself

### Implementation for User Story 3

- [X] T037 [US3] Verify all `Brush.*` definitions in `WpfApp2/Theme/Base/Colors.xaml` reference `Color.*` or `Primitive.*` tokens — no direct hex values in any Brush definition per FR-004 and data-model validation rule
- [X] T038 [P] [US3] Verify all `Color.*` semantic tokens in `WpfApp2/Theme/Base/Colors.xaml` reference `Primitive.*` tokens — no direct hex values allowed per FR-004
- [X] T039 [US3] Verify no `Brush.*` token references another `Brush.*` token (semantic-to-semantic chain prohibited) — grep or manual audit of `WpfApp2/Theme/Base/Colors.xaml`
- [X] T040 [US3] Verify all `Component.*` tokens in `WpfApp2/Theme/Base/ComponentTokens.xaml` reference `Spacing.*`, `Padding.*`, `Radius.*`, `Brush.*`, or have documented hardcoded defaults where DynamicResource is unsupported per FR-005
- [X] T041 [US3] Verify Dark theme overrides in `WpfApp2/Theme/Dark/DarkColors.xaml` only override Semantic tokens (Color.*, Brush.*) — no Primitive.* overrides and no direct hex in Brush overrides per layer integrity rules
- [X] T042 [P] [US3] Verify Light theme overrides in `WpfApp2/Theme/Light/LightColors.xaml` follow the same integrity rules as Dark
- [X] T043 [P] [US3] Verify Custom theme overrides in `WpfApp2/Theme/Custom/CustomColors.xaml` follow the same integrity rules as Dark
- [X] T044 [US3] Add token chain validation to `WpfApp2/Services/ThemeManager.cs` — at theme load, verify no circular references exist in the Primitive → Semantic → Component chain; log warnings for any broken chains per FR-010

**Checkpoint**: All token layers form a valid chain. No Brush.* references hex. No Brush.* references another Brush.*. Theme switching produces zero token integrity warnings.

---

## Phase 6: User Story 5 — Resource Aggregator Correctness (Priority: P2)

**Goal**: ThemeResources.xaml loads all dictionaries in correct dependency order; all three themes render correctly

**Independent Test**: Build with `msbuild` yields zero XAML parse errors. Switch between Dark, Light, Custom themes at runtime — all controls render correctly with no missing-resource warnings

### Implementation for User Story 5

- [X] T045 [US5] Update `WpfApp2/Theme/ThemeResources.xaml` MergedDictionaries loading order to match documented dependency: Base/Colors → Typography → Spacing → Radius → Elevation → Motion → ZIndex → Opacity → ComponentTokens → Effects/Shadows → Effects/Glow → Effects/Animations → Controls/*.xaml → ModernWindow.xaml → WindowAnimations.xaml per FR-012 and data-model ThemeResourcesAggregator
- [X] T046 [US5] Verify no duplicate `x:Key` entries exist across dictionaries in `WpfApp2/Theme/ThemeResources.xaml` MergedDictionaries — each key must be unique within the same merge level per data-model validation rules
- [X] T047 [US5] Resolve any missing-resource errors from the build — verify all DynamicResource keys referenced in Controls/*.xaml and window files resolve to tokens defined in Base/ or Effects/ dictionaries per data-model loading order
- [X] T048 [US5] Test theme switching regression — switch between Dark, Light, and Custom themes at runtime; verify all controls render correctly per FR-013; verify no XAML parse exceptions or missing-resource warnings in Debug output
- [X] T049 [US5] Validate that newly added token files (Elevation.xaml with Double-only tokens, new Primitive.* tokens, new Brush.* tokens) are correctly loaded by ThemeResources.xaml and resolvable by all consumers per FR-012

**Checkpoint**: ThemeResources.xaml loads in documented order. Clean build with zero parse errors. All three themes render correctly with no visual regressions.

---

## Phase 7: User Story 4 — Spacing & Typography Standardization (Priority: P3)

**Goal**: All Margin, Padding, FontSize, FontWeight values in Control styles and window files reference named tokens from Spacing.xaml and Typography.xaml

**Independent Test**: Grep for hardcoded `Margin="`, `Padding="`, `FontSize="`, `FontWeight="` in Control style files and window XAML files — all must reference named tokens

### Implementation for User Story 4

- [X] T050 [P] [US4] Replace all hardcoded `Margin=""` values in `WpfApp2/Theme/Controls/ButtonStyles.xaml` with `{DynamicResource Spacing.*}` or `{DynamicResource Padding.*}` tokens per FR-008
- [X] T051 [P] [US4] Replace all hardcoded `Padding=""` values in `WpfApp2/Theme/Controls/ButtonStyles.xaml` with `{DynamicResource Padding.*}` or `{DynamicResource Component.Button.Padding}` tokens per FR-008
- [X] T052 [P] [US4] Replace all hardcoded `Margin=""` and `Padding=""` values in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` with `{DynamicResource Spacing.*}` or `{DynamicResource Padding.*}` tokens per FR-008
- [X] T053 [P] [US4] Replace all hardcoded `Margin=""` and `Padding=""` values in `WpfApp2/Theme/Controls/DataGridStyles.xaml` with `{DynamicResource Spacing.*}` or `{DynamicResource Component.DataGrid.*}` tokens per FR-008
- [X] T054 [P] [US4] Replace all hardcoded `FontSize=""` and `FontWeight=""` values in `WpfApp2/Theme/Controls/*.xaml` with `{DynamicResource Style.Text.*}` styles or individual typography tokens per FR-009
- [X] T055 [US4] Replace all hardcoded `CornerRadius=""` values outside base dictionaries in `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` and other control files with `{DynamicResource MediumRadius}` or `{DynamicResource Component.*.Radius}` per research task 2 note about DynamicResource limitations
- [X] T056 [P] [US4] Audit and replace all hardcoded `Margin=""`, `Padding=""`, `FontSize=""`, `FontWeight=""` values in `WpfApp2/MainWindow.xaml` with named tokens per FR-008
- [X] T057 [P] [US4] Audit and replace all hardcoded spacing/typography values in `WpfApp2/Views/SettingsWindow.xaml` with named tokens per FR-008
- [X] T058 [P] [US4] Audit and replace all hardcoded spacing/typography values in other window files (`WpfApp2/Controls/LoadingOverlay.xaml`, `WpfApp2/Controls/Toast/ToastWindow.xaml`, etc.) with named tokens per FR-008

**Checkpoint**: Grep for hardcoded `Margin="`, `Padding="`, `FontSize="`, `FontWeight="` in Controls/*.xaml and window files returns zero matches outside base dictionaries.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validation, build-time linting, and final compliance verification

- [X] T059 Create build-time token validation script `scripts/Validate-Tokens.ps1` — PowerShell script that regex-greps all `.xaml` files for: inline hex outside token definition files, StaticResource on themeable DPs, inline DropShadowEffect outside Effects/, hardcoded spacing in Controls/ and windows per FR-010
- [X] T060 Run `scripts/Validate-Tokens.ps1` and fix any remaining violations until exit code is 0 per FR-010 and SC-009
- [X] T061 Run full build verification: `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` — zero XAML parse errors and zero resource-related warnings per SC-006
- [ ] T062 Run manual theme switching regression test — launch add-in in Excel, open SettingsWindow, cycle Dark → Light → Custom themes, verify all controls render correctly with no visual regressions per SC-005 and SC-011
- [ ] T063 Verify ThemeManager token integrity validation produces zero warnings for all three themes (Dark, Light, Custom) per SC-010
- [ ] T064 Final compliance check — verify all success criteria SC-001 through SC-011 pass per spec
- [X] T065 Update `WpfApp2/Theme/ThemeResources.xaml` XML comments to document the loading order and rationale per data-model ThemeResourcesAggregator section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 audit results — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion — Token coverage foundation
- **User Story 2 (Phase 4)**: Depends on Phase 2 completion — Can run in parallel with Phase 3 (different files)
- **User Story 3 (Phase 5)**: Depends on Phases 3 and 4 (validates token layers that US1/US2 built)
- **User Story 5 (Phase 6)**: Depends on Phase 2 completion — Can run in parallel with Phases 3–4 (different files)
- **User Story 4 (Phase 7)**: Depends on Phases 2 and 5 (Spacing/Padding tokens must be defined before replacement)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — No dependency on other stories
- **US2 (P1)**: Can start after Phase 2 — Parallel with US1 (replaces inline values)
- **US3 (P2)**: Depends on US1 and US2 (validates what they built)
- **US5 (P2)**: Can start after Phase 2 — Aggregator correctness, independent of US1–US3 content
- **US4 (P3)**: Depends on US3 (needs verified token chain before replacing spacing values)

### Within Each Phase

- Audit tasks (T002–T010) can all run in parallel
- Token additions (T011–T014) can all run in parallel once audit is done
- Inline violation replacements (T024–T030) can all run in parallel (different files)
- Verification tasks depend on their respective implementation tasks

### Parallel Opportunities

- T002–T010: All audit tasks are parallelizable
- T012, T013, T014: Token additions are parallel
- T024–T033: Most inline replacements are parallel (different files)
- T037–T043: Most token integrity verifications are parallel
- T050–T058: Most spacing replacements are parallel (different files)

---

## Parallel Example: Phase 4 (User Story 2)

```text
# Launch these in parallel (different files, no dependencies):
T024: Replace V1 GradientStop colors in SettingsWindow.xaml
T025: Replace V2 ScrollBarStyles.xaml
T026: Replace V3 GroupBoxStyles.xaml
T027: Replace V4 LoadingOverlay.xaml
T028: Replace V5 ToastWindow.xaml
T029: Replace V6 WindowAnimations.xaml
T030: Replace V7 ScrollViewerStyles.xaml
T032: Replace DataGridStyles.xaml inline hex
T033: Replace ComboBoxStyles.xaml inline hex
T034: Verify DropShadowEffect removal from Elevation.xaml
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup — audit existing inventory
2. Complete Phase 2: Foundational — add missing tokens, deprecate legacy keys, centralize effects
3. Complete Phase 3: User Story 1 — token coverage complete
4. Complete Phase 4: User Story 2 — all inline violations replaced
5. **STOP and VALIDATE**: Run lint script, grep audit, build, theme switching regression
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Token infrastructure ready
2. Add US1 + US2 → Zero inline violations, full token coverage (MVP!)
3. Add US3 → Token layer integrity verified
4. Add US5 → Resource aggregator loading order correct
5. Add US4 → Spacing and typography fully tokenized
6. Polish → Lint script, full compliance verification

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1–2 together
2. Once Foundational is done:
   - Developer A: US1 (token coverage) + US3 (integrity verification)
   - Developer B: US2 (inline elimination) + US4 (spacing standardization)
   - Developer C: US5 (aggregator correctness)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Research decisions (GradientStop DynamicResource, ColorAnimation workarounds, Elevation split, legacy deprecation, Primitive.Transparency, build+runtime validation) are documented in research.md and inform task implementation
- The lint script (T059) is the final enforcement mechanism for FR-010; it should only be created after all inline violations are already fixed to ensure the script passes on first run