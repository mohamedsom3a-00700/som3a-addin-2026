# Feature Specification: Update Themes Manager

**Feature Branch**: `002-fluent-theme-engine`

**Created**: 2026-05-19

**Status**: Draft

**Input**: Implementation plan at `specs/002-themes-manager/plan.md`

## User Scenarios & Testing

### User Story 1 — Theme Switching Works Reliably Across All Windows (Priority: P1)

As a user, I want to switch between Dark, Light, and Custom themes from Settings and see every window update immediately — including the root background, cards, controls, and progress bars — so the application looks consistent regardless of which theme I choose.

**Why this priority**: Theme switching is the primary feature of the theme engine. Without it working correctly, the entire theme system is broken.

**Independent Test**: Can be tested by opening Settings, clicking each theme card (Dark/Light/Custom), and verifying that ALL open windows reflect the new theme instantly — especially the root background gradient and control colors.

**Acceptance Scenarios**:

1. **Given** the user is on any window (e.g., MainWindow, LinksManager), **When** they switch from Dark to Light theme in Settings, **Then** the window background, cards, text colors, and borders all update to light colors — no dark elements remain.
2. **Given** the user has multiple windows open simultaneously, **When** they switch themes, **Then** all open windows change theme in unison within 200ms.
3. **Given** the user switches to a theme they are already on (e.g., Dark -> Dark), **When** they click the same theme card, **Then** no duplicate dictionary replacement occurs and the UI does not flicker.

---

### User Story 2 — Accent Color Changes Reflect Everywhere (Priority: P1)

As a user, I want to change the accent color and see it applied to all accent-dependent UI — including button fills, selection highlights, progress bar fills, and glow effects — so the application matches my preferred color scheme.

**Why this priority**: Accent color customization is the second-most visible feature of the theme system. Hardcoded accent colors in glow effects and progress bars currently break this experience.

**Independent Test**: Can be tested by selecting a new accent swatch in Settings and verifying that buttons, progress bars, and glow effects use the new color.

**Acceptance Scenarios**:

1. **Given** the user has selected an accent color (e.g., Green), **When** they view any progress bar, **Then** the progress fill should use the green accent, not the hardcoded teal-to-green gradient.
2. **Given** the user has selected an accent color, **When** they hover over a button, **Then** the glow effect around the button should use the selected accent color.
3. **Given** the user switches themes (e.g., Dark to Light) without changing the accent color, **When** the new theme loads, **Then** the accent color remains the same as before the switch.

---

### User Story 3 — No Crashes on Startup or Theme Change (Priority: P1)

As a user, I expect the application to never crash when opening windows or changing themes — duplicate resource definitions or missing keys should not cause XAML parse exceptions.

**Why this priority**: Runtime crashes from duplicate converter keys or duplicate dictionary loads are P0 defects that block all other functionality.

**Independent Test**: Can be tested by opening every window in the application sequentially and switching themes multiple times — no XAML parse exceptions should occur.

**Acceptance Scenarios**:

1. **Given** the application is launched, **When** SettingsWindow is opened, **Then** no "item already defined" XAML parse exception occurs from duplicate resource keys.
2. **Given** the application is running, **When** the user switches themes rapidly 10 times, **Then** no crash or exception occurs, and only the most recently selected theme is applied (intermediate switches are debounced/coalesced).

---

### User Story 4 — Progress Bars Display Correctly in All Themes (Priority: P2)

As a user, I want progress bars to always show visible, correctly-colored fill regardless of which theme is active, so I can track operation progress without visual issues.

**Why this priority**: 7 windows have progress bars, and 4 have `Foreground="White"` text that becomes invisible on Light theme.

**Independent Test**: Can be tested by triggering a progress operation (e.g., loading data) in Dark theme then switching to Light — the progress fill and percentage text must be visible in both.

**Acceptance Scenarios**:

1. **Given** the Light theme is active, **When** a progress bar appears, **Then** the progress percentage text is readable (not white on light background).
2. **Given** any theme, **When** a progress bar fills, **Then** the fill color matches the current accent color, not a hardcoded teal-to-green gradient.

---

### User Story 5 — Window Background Matches the Active Theme (Priority: P2)

As a user, I want every window's background to match the selected theme — not remain stuck on the dark background gradient — so the application looks consistent across all windows.

**Why this priority**: All 12 windows currently have a hardcoded dark navy-to-teal gradient that does not change when Light or Custom themes are selected, making theme switching visually broken.

**Independent Test**: Can be tested by switching to Light theme and checking that every window's root background is light-colored, not dark.

**Acceptance Scenarios**:

1. **Given** the Light theme is active, **When** any window opens, **Then** the root background is a light color appropriate for the Light theme.
2. **Given** the user switches from Dark to Custom, **When** the theme applies, **Then** the root background uses the Custom theme's background, not the Dark theme's.

---

### Edge Cases

- What happens when a theme dictionary fails to load (e.g., file missing or corrupted)? If the new theme fails to load, the currently-active theme dictionary MUST be preserved to avoid leaving the application without a theme. Only if the current dictionary itself is invalidated should ThemeManager fall back to Dark theme as a last resort.
- What happens if the user opens Settings for the first time and no theme preference has been saved? The default Dark theme with blue accent should be applied.
- What happens when duplicate resource keys exist across dictionaries? WPF throws a XAML parse exception — this is prevented by removing duplicate converter definitions and redundant dictionary loads.
- What happens to the active accent color when a theme is switched without specifying a new accent? The accent MUST persist from the previous selection; it MUST NOT be reset to any theme default.
- What happens on extremely high-DPI monitors (>3.0x scaling)? WindowRenderModeDetector should already detect this and enable FallbackSafe mode — no theme change should override this detection.
- What happens when the user clicks theme cards rapidly in succession? Theme change requests MUST be debounced/coalesced (≈150ms window) so that only the most recently selected theme is applied, preventing transient visual states and race conditions.

## Requirements

### Functional Requirements

- **FR-000**: All newly introduced theme tokens MUST use the semantic `Brush.*` namespace convention (e.g., `Brush.Background.Root`, `Brush.Stroke.Info`) as defined in `Theme/Base/Colors.xaml`. Legacy flat keys (e.g., `BackgroundBrush`) MUST NOT be used for new work.
- **FR-001**: ThemeManager MUST treat `_currentAccentColor` as an independent user preference. When `ApplyTheme()` is called without an explicit `accentColor`, the current accent MUST be preserved and re-applied; it MUST NOT be reset to any theme default.
- **FR-002**: ThemeManager MUST guard against duplicate dictionary replacement when the same theme is selected again. Additionally, rapid sequential switches to different themes MUST be debounced/coalesced so that only the final selection is applied.
- **FR-003**: ThemeManager MUST wrap dictionary replacement in try/catch. If the requested new theme fails to load, the currently-active theme dictionary MUST be preserved. A fallback to Dark theme MUST only occur if the current dictionary itself is invalidated.
- **FR-004**: ThemeManager MUST dispatch `ThemeChanged` events on the UI dispatcher thread.
- **FR-005**: Application MUST NOT contain duplicate resource keys across App.xaml and ThemeResources.xaml that would cause XAML parse exceptions.
- **FR-006**: SettingsWindow MUST NOT load resource dictionaries (Shadows.xaml, Glow.xaml, ThemeCardStyles.xaml, AccentSwatchStyles.xaml) that are already included in ThemeResources.xaml.
- **FR-007**: All 12 window root backgrounds MUST be defined as per-theme brush tokens (e.g., `Brush.Background.Root`) and applied via `{DynamicResource Brush.Background.Root}`.
- **FR-008**: All progress bar fills MUST derive from the current accent color rather than hardcoded teal-to-green gradients.
- **FR-009**: All progress bar percentage text MUST use a theme-aware foreground brush to remain visible on both Dark and Light themes.
- **FR-010**: The orphaned `FluentEffects.xaml` file MUST be removed entirely, and its redundant `<ResourceDictionary Source="..."/>` entries in `App.xaml` and `ThemeResources.xaml` MUST be deleted. All glow effects are already centralized in `Theme/Effects/Glow.xaml`.
- **FR-011**: ModernWindow.xaml MUST use `{DynamicResource BackgroundBrush}` and `{DynamicResource TextMainBrush}` instead of hardcoded `White` and `Black`.
- **FR-012**: ModernWindow.xaml MUST centralize its DropShadowEffect via `{DynamicResource Shadow.Window}` instead of inline definition.
- **FR-013**: The orphaned FluentWhite.xaml file MUST be removed from the project to eliminate dead code.
- **FR-014**: Commented-out legacy theme imports in App.xaml MUST be removed.
- **FR-015**: All inline DropShadowEffect instances on progress bars MUST be replaced by a centralized `ProgressGlow` effect in Effects/Shadows.xaml that uses the current accent color.
- **FR-016**: All TextBlock elements with missing `Foreground` properties MUST have `Foreground="{DynamicResource TextMainBrush}"` to be visible on both themes.
- **FR-017**: The TreeView hover/selected highlight colors in Float_path.xaml MUST use theme-aware accent brushes instead of hardcoded `#007ACC`.
- **FR-018**: The named color `LimeGreen` in SubDailyReportWindow.xaml MUST be replaced with `{DynamicResource SuccessBrush}`.
- **FR-019**: All hardcoded semi-transparent border fills/strokes (`#12FFFFFF`, `#18FFFFFF`, `#1FFFFFFF`, `#22FFFFFF`) MUST be replaced with per-theme resource tokens using the semantic `Brush.Stroke.*` naming convention (e.g., `Brush.Stroke.Info`, `Brush.Stroke.Status`).

### Key Entities

- **Theme (Dark / Light / Custom)**: A named collection of visual token overrides that define the application's color scheme. Each theme has its own dictionary with semantic brush values.
- **Accent Color**: A user-selectable color that drives primary action UI (buttons, selections, progress, glows). Persisted independently of the theme selection.
- **Resource Dictionary**: A XAML file containing named resources (brushes, colors, effects) that WPF resolves via `{DynamicResource}` lookups at runtime.
- **Token**: A named design value (e.g., `Brush.Background.Root`, `Brush.Accent.ProgressFill`) that maps to a concrete color or gradient per theme. New tokens MUST use the semantic `Brush.*` namespace convention established in `Theme/Base/Colors.xaml`. Legacy flat keys (e.g., `BackgroundBrush`) are kept for backward compatibility but MUST NOT be used for new work.
- **ThemeManager**: The singleton service responsible for applying, persisting, and notifying theme changes across the application.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All 12 windows display the correct theme background immediately (<200ms) after a theme switch, verified by automated comparison of pixel regions before and after switch.
- **SC-002**: Zero XAML parse exceptions occur when opening any window or switching themes, verified by 10 consecutive open/close cycles of all windows.
- **SC-003**: Zero hardcoded `#HEX` colors remain in any window `.xaml` file after migration, verified by automated grep audit across all 12 window files.
- **SC-004**: Accent color changes are reflected in progress bar fills, glow effects, and button hover states across all 7 windows with progress bars, verified by visual inspection.
- **SC-005**: Theme switching and accent changes complete within 200ms as measured by stopwatch, ensuring no perceptible delay to the user.
- **SC-006**: The build compiles without errors after all changes, verified by `msbuild`.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The existing `Theme/Base/Colors.xaml` token system will be extended with new semantic brush keys as needed (e.g., `Brush.Background.Root`, `Brush.Accent.ProgressFill`).
- Windows that currently load ThemeResources.xaml individually in their own `MergedDictionaries` will continue to do so; this feature does not address global ThemeResources.xaml loading (that is an architectural concern for a future feature).
- The `ThemeChanged` event currently fires on the calling thread; this feature will ensure it dispatches to the UI thread, but subscribers (like SettingsWindow.xaml.cs) already listen correctly.
- The build verification command is `msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug` as documented in AGENTS.md.
- All changes are backward-compatible — existing theme settings files (Settings.settings) will continue to work without migration.

## Clarifications

### Session 2026-05-19

- **Q**: `FluentEffects.xaml` contains duplicate/unused effects (`FocusGlow`, `ButtonHoverGlow`, `PrimaryButtonGlow`, `CardShadow`, `WindowShadow`) that are never referenced by any control in the application; all active glows already live in centralized `Theme/Effects/Glow.xaml` and shadows in `Theme/Effects/Shadows.xaml`. Should the file be removed entirely or updated in place?  
  **A**: Remove `FluentEffects.xaml` entirely and delete its redundant `<ResourceDictionary Source="..."/>` entries from `App.xaml` and `ThemeResources.xaml`.
- **Q**: FR-001 and User Story 2 say the accent color must persist across theme switches, but the linked plan.md TASK-1001 describes resetting `_currentAccentColor` to the new theme's default as a "bug fix". Which behavior is correct?  
  **A**: The spec is authoritative. `_currentAccentColor` is an independent user preference and MUST persist across theme switches. The plan's TASK-1001 should be corrected to preserve the existing accent when `accentColor` is null.
- **Q**: FR-003 says ThemeManager should "fall back to Dark theme on failure," but the current code already preserves the old theme if the new one fails to load. Should the old theme be preserved or forcefully replaced with Dark on any failure?  
  **A**: Preserve the currently-active theme dictionary if the new theme fails to load. Only fall back to Dark theme if the current dictionary itself is invalidated. This avoids leaving the application with no theme at all.
- **Q**: User Story 3 requires that rapid theme switching (10 times) does not crash, but it does not specify whether intermediate visual states are acceptable. Should theme switches be debounced/coalesced or applied immediately?  
  **A**: Theme change requests MUST be debounced/coalesced (≈150ms window) so that only the most recently selected theme is applied. This prevents transient visual states, overlapping dictionary operations, and race conditions while still meeting the <200ms switch target.
- **Q**: The spec's Key Entities use legacy flat token names (`RootBackgroundBrush`, `ProgressBarFillBrush`) as examples, but `Theme/Base/Colors.xaml` already establishes a semantic `Brush.*` namespace convention. Which naming convention should new tokens follow?  
  **A**: All newly introduced tokens MUST use the semantic `Brush.*` namespace convention (e.g., `Brush.Background.Root`, `Brush.Accent.ProgressFill`). Legacy flat keys are retained for backward compatibility but MUST NOT be used for new work.
