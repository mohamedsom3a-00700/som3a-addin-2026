# Feature Specification: Design System Core

**Feature Branch**: `004-design-system-core`

**Created**: 2026-05-22

**Status**: Draft

**Input**: Phase 1 from implementation_plan.md — Convert current WPF styling into enterprise token architecture. Fix all inline violations. Create Primitive → Semantic → Component token layers. Standardize spacing, typography, effects, elevation, motion, opacity, and z-index. Update ThemeResources.xaml aggregator.

## User Scenarios & Testing

### User Story 1 - Complete Token Coverage (Priority: P1)

As a developer or themer, I want every design value (color, spacing, typography, effects, elevation, motion, opacity, z-index) accessible through a named token so the entire UI is consistently themeable at runtime without embedding visual values directly in control templates.

**Why this priority**: Token coverage is the foundation that makes all subsequent theming work possible. Without complete token coverage, inline values persist and theme switching remains inconsistent.

**Independent Test**: Run a grep audit across all `.xaml` files — every visual value (color, thickness, radius, shadow, opacity, duration) must map to a named resource key. No hardcoded hex colors, margins, font sizes, or effect parameters should remain outside Base/ and Effects/ dictionaries.

**Acceptance Scenarios**:

1. **Given** a control template in `ButtonStyles.xaml`, **When** a themer changes the `Brush.Button.Background` token value in `Colors.xaml`, **Then** all buttons immediately reflect the new value at runtime without any code changes.
2. **Given** a developer needs a new accent shade (e.g., hover variant), **When** they add a `Primitive.*` color to the primitive layer, **Then** semantic tokens can reference it and all consuming controls update automatically.
3. **Given** the Dark theme is active, **When** a themer inspects all control templates, **Then** no template contains a hardcoded `Color="#"`, `Margin=""`, `FontSize=""`, or `CornerRadius=""` — all visual properties reference named tokens.

---

### User Story 2 - Inline Value Elimination (Priority: P1)

As a developer, I want all inline hex colors, hardcoded corner radii, and inline DropShadowEffect definitions replaced with semantic or component tokens so that changing a single token value propagates correctly across the entire UI without manual search-and-replace.

**Why this priority**: Inline values are the primary source of visual inconsistencies and theme-switching failures. Removing them is the most impactful quality improvement in Phase 1.

**Independent Test**: Grep all `.xaml` files for `Color="#`, `CornerRadius="` (outside base dictionaries), and `<DropShadowEffect` (outside Effects/) — all results must be zero.

**Acceptance Scenarios**:

1. **Given** the codebase, **When** a grep is run for `Color="#` across `Controls/*.xaml` and all window files, **Then** zero matches are found (all hex colors moved to primitive token definitions in `Base/Colors.xaml`).
2. **Given** `ButtonStyles.xaml`, **When** `Brush.Button.Background` token value changes, **Then** all button backgrounds that previously used `#2FFFFFFF` immediately reflect the new value.
3. **Given** `DataGridStyles.xaml`, **When** `Brush.DataGrid.AlternatingRow` token value changes, **Then** all row backgrounds update without any per-file changes.
4. **Given** any XAML file outside the `Effects/` directory, **When** a grep is run for `<DropShadowEffect`, **Then** zero matches are found.

---

### User Story 3 - Token Layer Integrity (Priority: P2)

As a themer, I can depend on a strict three-layer token chain (Primitive → Semantic → Component → Control Template) with no broken references, circular dependencies, or direct hex values in semantic tokens, enabling safe theme editing at any layer.

**Why this priority**: Token layer integrity ensures that the token system is actually usable for theme customization. Without it, tokens may have broken chains or duplicate definitions that undermine the architecture.

**Independent Test**: Validate every `Brush.*` token references a `Primitive.*` color (not a hex value); every `Component.*` token references a `Brush.*` or `Spacing.*` token; no token references itself.

**Acceptance Scenarios**:

1. **Given** a `Brush.Background.Primary` semantic token, **When** its definition is inspected, **Then** it references `Primitive.Slate.900` (or similar) and not a hex color like `#0E1720`.
2. **Given** the entire token chain from `Colors.xaml`, **When** circular references are checked (token A references token B which references token A), **Then** zero circular references exist.
3. **Given** a `Component.Button.Padding` token, **When** its definition is inspected, **Then** it references `Spacing.Medium` (or similar spacing token) and not a hardcoded `Thickness` value.

---

### User Story 4 - Spacing & Typography Standardization (Priority: P3)

As a developer, I apply consistent margins, padding, and text styles through named spacing and typography tokens rather than ad-hoc hardcoded values, ensuring visual rhythm and alignment across all controls.

**Why this priority**: Spacing and typography standardization eliminates visual inconsistencies and makes the design system predictable, but it depends on the token system (P1/P2) being established first.

**Independent Test**: Grep for hardcoded `Margin="`, `Padding="`, `FontSize="`, and `FontWeight="` in control style files — all must reference named tokens from `Spacing.xaml` and `Typography.xaml`.

**Acceptance Scenarios**:

1. **Given** a developer needs to adjust button padding across the entire application, **When** they modify the `Component.Button.Padding` token, **Then** all button controls using that token update consistently.
2. **Given** a developer creates a new TextBlock for body text, **When** they apply `Style.Text.Body`, **Then** the font family, size, weight, and foreground all resolve from typography tokens.
3. **Given** the `Spacing.xaml` file, **When** inspected, **Then** it contains a complete set of spacing tokens (`Spacing.XSmall` through `Spacing.XXLarge`) and padding tokens (`Padding.XSmall` through `Padding.XLarge`).

---

### User Story 5 - Resource Aggregator Correctness (Priority: P2)

As a developer, ThemeResources.xaml loads all dictionaries in the correct dependency order so that no missing-resource errors occur at design time or runtime, and all themes (Dark, Light, Custom) render correctly after token migration.

**Why this priority**: Without correct loading order, resources may fail to resolve, causing silent fallbacks or runtime exceptions. This is a blocking requirement for any theme switching to work.

**Independent Test**: Build the project with `msbuild` — zero XAML parse errors. Switch between all three themes at runtime — all controls render correctly with no missing-resource warnings.

**Acceptance Scenarios**:

1. **Given** the `ThemeResources.xaml` file, **When** its `MergedDictionaries` order is inspected, **Then** it loads in the documented order: Primitive Tokens → Semantic Tokens → Typography → Spacing → Radius → Elevation → Motion → ZIndex → Opacity → Component Tokens → Effects → Controls → Window Styles → Theme Overrides.
2. **Given** a newly added token file (e.g., `Elevation.xaml`), **When** it is included in `ThemeResources.xaml`, **Then** all tokens it defines are resolvable by any control loaded after it.
3. **Given** the application is running, **When** the user switches between Dark, Light, and Custom themes, **Then** all controls render correctly with no missing-resource exceptions or visual fallbacks.

---

### Edge Cases

- What happens when a legacy flat key (e.g., `Blue500`) and a new primitive token (`Primitive.Blue.500`) resolve to the same color? This is valid — the legacy key acts as an alias. Both must coexist without duplicate resource exceptions.
- What happens if a semantic token accidentally references another semantic token instead of a primitive? This creates an illegal semantic-to-semantic chain per FR-004. The token validation must detect and flag this as an error. Derived state tokens (hover/pressed) must compose from Primitive base color + Primitive transparency token, not from another Brush.* token.
- What happens if `ThemeResources.xaml` loads dictionaries in the wrong order (e.g., Controls before Base)? WPF will silently use fallback values, leading to incorrect rendering. The aggregator must enforce the documented order.
- What happens when a `Component.*` token references a `Spacing.*` token that does not yet exist? The component token must wait for the spacing token to be created, or the build will fail with a missing resource error.
- What happens to existing runtime theme switching (Feature 001/002) during this migration? All existing theme-switching functionality must continue to work throughout Phase 1.
- What happens if `Elevation.xaml`, `Motion.xaml`, `ZIndex.xaml`, or `Opacity.xaml` tokens reference effects or values defined in `Shadows.xaml` or `Glow.xaml`? The dependency order ensures Effects are loaded after Base tokens, so elevation tokens should not directly reference effect keys — they should provide abstraction values that are consumed by control templates which reference effects.

## Clarifications

### Session 2026-05-22

- Q: Should semantic tokens be allowed to reference other semantic tokens for derived states, or must every semantic token reference a Primitive directly? → A: Semantic tokens reference Primitives for flat values; derived states (hover/pressed) reference Primitive.* with opacity/alpha from Primitive.* Transparency tokens — no semantic-to-semantic chain.
- Q: What is the scope of hardcoded spacing/margin/padding replacement — control styles only or window files too? → A: Replace hardcoded spacing in both Theme/Controls/*.xaml AND all window .xaml files — full coverage eliminates all inline values across the UI layer.
- Q: What validation mechanism should token compliance use? → A: Both: build-time lint script for inline values and StaticResource usage, plus runtime ThemeManager validation for missing/broken tokens during theme load.
- Q: Should legacy flat keys be permanent aliases or have a deprecation path? → A: Deprecate with planned removal — legacy keys annotated as deprecated, targeted for removal in Phase 6 (MVVM Cleanup) after all consumers migrate to semantic tokens.
- Q: Should Phase 1 explicitly require regression testing of existing theme switching (Features 001/002) after token migration? → A: Require explicit regression testing — Phase 1 acceptance includes verifying that Dark/Light/Custom theme switching still works correctly after token migration (no visual regressions, no missing resources).

## Requirements

### Functional Requirements

- **FR-001**: The design system MUST provide a complete inventory of all existing tokens in `Colors.xaml`, `Typography.xaml`, `Spacing.xaml`, and `Radius.xaml`, identifying each as primitive, semantic, or legacy-flat type, and documenting which control styles consume it.
- **FR-002**: The design system MUST detect and document all duplicate color hex values across XAML files, identifying semantic tokens that resolve to the same color with different names and legacy flat keys that shadow semantic tokens.
- **FR-003**: The design system MUST define `Primitive.*` Color resources in `Theme/Base/Colors.xaml` providing the full palette of raw color values. Existing flat keys (e.g., `Blue500`, `Slate900`) MUST be preserved as aliases referencing the corresponding `Primitive.*` token, annotated as deprecated with a comment marking them for removal in Phase 6 (MVVM Cleanup). No existing references MUST break during Phase 1.
- **FR-004**: The design system MUST define `Brush.*` semantic tokens in `Theme/Base/Colors.xaml` that reference `Primitive.*` colors for flat values. Derived state tokens (e.g., `Brush.Button.HoverBackground`, `Brush.Button.PressedBackground`) MUST compose from a `Primitive.*` base color combined with a `Primitive.*.Transparency` alpha value — no semantic-to-semantic reference chains are permitted. No direct hex values are allowed in any semantic token definition. All inline hex colors found in the audit (e.g., `#2FFFFFFF` in ButtonStyles, `#0AFFFFFF` in DataGridStyles) MUST be replaced with corresponding `Brush.*` tokens.
- **FR-005**: The design system MUST define `Component.*` tokens in a new `Theme/Base/ComponentTokens.xaml` that reference semantic tokens or spacing/radius tokens. Component tokens isolate control styling from direct semantic token consumption, enabling per-component theming.
- **FR-006**: The design system MUST replace all inline hex colors in control templates (`ButtonStyles.xaml`, `DataGridStyles.xaml`, `ComboBoxStyles.xaml`, `MainWindow.xaml`) with the corresponding `Brush.*` semantic tokens. Zero inline hex color values MUST remain in any control template file.
- **FR-007**: Zero `DropShadowEffect` definitions MUST exist in any XAML file outside `Effects/Shadows.xaml` and `Effects/Glow.xaml`. All shadow and glow effects MUST be centralized in the Effects directory.
- **FR-008**: The design system MUST provide standardized spacing tokens (`Spacing.XSmall` through `Spacing.XXLarge`) and padding tokens (`Padding.XSmall` through `Padding.XLarge`) in `Theme/Base/Spacing.xaml`. Hardcoded `Margin`, `Padding`, and spacing values in both `Theme/Controls/*.xaml` and all window `.xaml` files MUST be replaced with the corresponding spacing or padding tokens.
- **FR-009**: The design system MUST provide standardized text styles (`Style.Text.Title`, `Style.Text.Subtitle`, `Style.Text.Body`, `Style.Text.Caption`, `Style.Text.Button`) in `Theme/Base/Typography.xaml`, each referencing typography tokens for font family, size, weight, and foreground.
- **FR-010**: The design system MUST validate token compliance through both build-time and runtime mechanisms. A build-time lint script MUST detect inline hex values, `StaticResource` on themeable properties, and missing `DynamicResource` usage. At runtime, ThemeManager MUST validate token integrity (missing keys, broken chains) when loading a theme and log warnings for any unresolved resources.
- **FR-011**: The design system MUST create new token files: `Theme/Base/Elevation.xaml` (elevation-to-shadow mappings), `Theme/Base/Motion.xaml` (animation duration and easing tokens), `Theme/Base/ZIndex.xaml` (layering constants), and `Theme/Base/Opacity.xaml` (standard opacity values for disabled, hover, pressed, subtle states).
- **FR-012**: `Theme/ThemeResources.xaml` MUST be updated to load all dictionaries in the documented dependency order: Primitive Tokens → Semantic Tokens → Typography → Spacing → Radius → Elevation → Motion → ZIndex → Opacity → Component Tokens → Effects (Shadows, Glow, Animations) → Controls → Window Styles → Theme Overrides → Runtime Overrides. No dictionary MUST be loaded before its dependencies.
- **FR-013**: Token migration MUST NOT break existing theme switching functionality (Features 001/002). After token migration, Dark/Light/Custom theme switching MUST work correctly with no visual regressions, missing resources, or exceptions.

### Key Entities

- **Primitive Token**: An immutable raw palette value (e.g., `Primitive.Blue.500`, `Primitive.Slate.900`). Never mutates at runtime. Defines the color palette from which all semantic tokens derive.
- **Semantic Token**: A meaning-based brush or color alias referencing a primitive token (e.g., `Brush.Background.Primary`, `Brush.Text.Secondary`, `Brush.Button.HoverBackground`). Derived state tokens compose from Primitive color + Primitive transparency token. Consumed by control templates and component tokens. Semantic-to-semantic reference chains are prohibited.
- **Component Token**: A control-specific design value referencing semantic or spacing tokens (e.g., `Component.Button.Padding`, `Component.Button.Radius`, `Component.Card.Padding`). Isolates per-control styling from global semantic values.
- **Token Layer**: The three-tier hierarchy (Primitive → Semantic → Component → Control Template) that ensures safe runtime theme editing without breaking UI consistency.
- **Legacy Flat Key**: An existing pre-migration resource key (e.g., `Blue500`, `Slate900`, `BackgroundBrush`) preserved as a deprecated alias to the corresponding primitive or semantic token. Annotated for removal in Phase 6 (MVVM Cleanup). Consumers should migrate to semantic tokens; legacy keys must not be used for new work.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Zero inline hex color values remain in any control style file within the `Theme/Controls/` directory and all window `.xaml` files — verified by regex grep audit excluding `Theme/Base/Colors.xaml` and `Theme/Dark/`, `Theme/Light/`, `Theme/Custom/` color definition files.
- **SC-002**: Zero inline `DropShadowEffect` definitions exist outside `Theme/Effects/Shadows.xaml` and `Theme/Effects/Glow.xaml` — verified by grep audit.
- **SC-003**: Zero `StaticResource` references on themeable properties (Background, Foreground, BorderBrush, Fill, Stroke, Effect) — verified by grep audit.
- **SC-004**: Every `Brush.*` semantic token resolves to a `Primitive.*` color reference — no direct hex values in any `Brush.*` definition in `Theme/Base/Colors.xaml`.
- **SC-005**: All three themes (Dark, Light, Custom) render correctly after token migration — verified by visual comparison of controls before and after migration, with no visual regressions.
- **SC-006**: The project builds without XAML parse errors or missing-resource warnings — verified by a clean build with zero errors and zero resource-related warnings.
- **SC-007**: Zero hardcoded `Margin=""`, `Padding=""`, `FontSize=""`, `FontWeight=""` values remain in `Theme/Controls/*.xaml` files AND all window `.xaml` files — all replaced by named tokens from `Spacing.xaml` and `Typography.xaml`.
- **SC-008**: `ThemeResources.xaml` loads all dictionaries in the documented dependency order with no missing-resource or duplicate-key exceptions at runtime.
- **SC-009**: A build-time lint script passes with zero violations (inline colors, StaticResource on themeable properties, inline DropShadowEffects, hardcoded spacing).
- **SC-010**: ThemeManager logs zero token integrity warnings during theme load for all three themes (Dark, Light, Custom).
- **SC-011**: Existing theme switching (Dark/Light/Custom) works correctly after token migration — verified by switching between all themes at runtime with no visual regressions, missing resources, or exceptions.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution A-III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution A-IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution A-IX).
- Excel VSTO rendering safety MUST be considered for all token changes (Constitution A-X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution A-XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution A-XII).
- Primitive tokens MUST remain immutable during application execution (Constitution A-V).
- Controls MUST consume Semantic Tokens only, not Primitive Tokens directly (Constitution A-V).
- Theme resources MUST load in the documented order: Primitive → Semantic → Typography → Spacing → Radius → Effects → Controls → Theme Overrides → Window Chrome → Runtime Overrides (Constitution A-XV).

## Assumptions

- Existing legacy flat keys (`Blue500`, `Slate900`, `BackgroundBrush`) will be preserved as deprecated aliases pointing to their corresponding `Primitive.*` or `Brush.*` tokens, annotated for removal in Phase 6 (MVVM Cleanup). They must not be used for new work. This ensures backward compatibility with existing references during Phase 1.
- The three existing themes (Dark, Light, Custom) will remain functional throughout migration. Token changes must not break runtime theme switching.
- Phase 1 creates the token infrastructure and eliminates inline values. Runtime accent color generation (Phase 3) and detailed control template redesign (Phase 4) are future work that will build on this foundation.
- Component tokens reference existing spacing/radius tokens from `Spacing.xaml` and `Radius.xaml`. If a referenced token does not yet exist, it will be created as part of P1-T008 (Standardize Spacing) or P1-T011 (New Token Files).
- `CornerRadius` values in XAML attribute syntax cannot use `DynamicResource` in all WPF scenarios. Where `DynamicResource` is not possible, component tokens with sensible defaults will be used.
- The existing audit findings from P0-T001 (inline colors in ButtonStyles, DataGridStyles, ComboBoxStyles, MainWindow) represent the known set of inline violations. A comprehensive search may reveal additional instances that must also be migrated.