# Feature Specification: Fluent Icon & Control Migration

**Feature Branch**: `032-fluent-icon-control-migration`

**Created**: 2026-05-31

**Status**: Draft

**Input**: User description: "Phase 3: Fluent Icon & Control Migration — Replace all materialDesign:PackIcon and MaterialDesign* style references with Fluent 2 icons and WPF-UI native controls."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent Fluent Icons Across All Pages (Priority: P1)

As a user, I want every icon in the application to use the Fluent 2 icon set so that the UI has a consistent, modern appearance across all pages and panels.

**Why this priority**: Icon consistency is the core deliverable of this phase. Without it, the application retains a disjointed look with mixed icon libraries, undermining the Fluent UI migration effort.

**Independent Test**: Navigate to Sidebar, Shell titlebar, Settings page, and Diagnostics page. Verify all icons render as Fluent 2 icons in both Dark and Light themes. No missing or broken icons.

**Acceptance Scenarios**:

1. **Given** the application is running in Dark theme, **When** the user views the Sidebar, **Then** all sidebar category icons display as Fluent 2 icons with correct colors and sizing.
2. **Given** the application is running in Light theme, **When** the user views the Sidebar, **Then** all sidebar category icons display as Fluent 2 icons with correct colors and sizing.
3. **Given** the user opens the Settings page, **When** viewing any settings panel, **Then** all icons (navigation, buttons, status indicators) use Fluent 2 icons.
4. **Given** the user opens the Diagnostics page, **When** viewing diagnostics cards, **Then** all status icons and action icons use Fluent 2 icons.

---

### User Story 2 - Fluent UI Control Rendering (Priority: P1)

As a user, I want all UI controls (scrollbars, progress bars, buttons, chips) to use WPF-UI native controls so that the application has a consistent Fluent design language.

**Why this priority**: Controls are tightly coupled with icons in the visual identity. Replacing MaterialDesign controls with WPF-UI equivalents ensures visual consistency and removes the MaterialDesign dependency.

**Independent Test**: Scroll a page with a ScrollViewer, observe a progress bar in a widget, click FlatButton-style buttons in Diagnostics and Dashboard widgets. All controls render with Fluent styling.

**Acceptance Scenarios**:

1. **Given** the user opens HomePage or DiagnosticsPage, **When** scrolling content, **Then** the scrollbar renders with Fluent styling (no MaterialDesign scrollbar).
2. **Given** a Dashboard widget shows a loading state, **When** the progress bar is visible, **Then** it renders as a Fluent-styled linear progress bar.
3. **Given** the user views DiagnosticsPage or Dashboard widgets, **When** FlatButton-style actions are present, **Then** they render as Fluent-styled buttons.
4. **Given** the user views Theme/Controls content, **When** chips are displayed, **Then** they render as WPF-UI or custom Fluent-styled chips (no MaterialDesign Chip).

---

### User Story 3 - Sidebar Icon System Migration (Priority: P2)

As a user, I want the sidebar to dynamically load Fluent 2 icons based on registered page metadata so that any page added to the sidebar automatically gets the correct icon.

**Why this priority**: The sidebar icon system is the runtime icon resolution mechanism. It must be migrated to support Fluent 2 icons so that new pages registered in the future automatically get correct icons without manual XAML changes.

**Independent Test**: Register a new test page with a Fluent 2 icon name in SidebarRegistrationService. Navigate to Sidebar. Verify the new page icon renders correctly.

**Acceptance Scenarios**:

1. **Given** a page is registered with a Fluent 2 icon name in SidebarRegistrationService, **When** the sidebar renders, **Then** the icon is resolved and displayed correctly.
2. **Given** the old MaterialIconConverter was used to map icon strings, **When** the new FluentIconConverter is in place, **Then** all existing icon string mappings resolve to correct Fluent 2 icons.
3. **Given** a page has an invalid or missing icon name, **When** the sidebar renders, **Then** a sensible fallback icon is displayed (no crash or blank space).

---

### User Story 4 - WPF-UI Compatibility Pilot (Priority: P2)

As a developer, I want a single low-risk page migrated to WPF-UI controls first to verify compatibility before migrating all pages, so that we can detect and address rendering issues early.

**Why this priority**: The pilot de-risks the entire phase. If WPF-UI proves incompatible with the existing theme engine, the fallback strategy (FluentIcons only, no WPF-UI controls) must be documented before proceeding.

**Independent Test**: Migrate DiagnosticsPage to WPF-UI controls and Fluent 2 icons. Verify it renders correctly in both Dark and Light themes with no layout regressions. Confirm the fallback decision is documented.

**Acceptance Scenarios**:

1. **Given** DiagnosticsPage has been migrated to WPF-UI controls, **When** the user views the page in Dark theme, **Then** all controls and icons render correctly with no layout breaks.
2. **Given** DiagnosticsPage has been migrated to WPF-UI controls, **When** the user views the page in Light theme, **Then** all controls and icons render correctly with no layout breaks.
3. **Given** the WPF-UI pilot reveals incompatibility, **When** the fallback is activated, **Then** the decision is documented and only FluentIcons.WPF is used (no WPF-UI controls).

---

### User Story 5 - MaterialDesign Resource Cleanup (Priority: P3)

As a developer, I want all MaterialDesign resource key references removed from the theme system so that the MaterialDesign dependency can be fully eliminated.

**Why this priority**: Removing resource key references is the final cleanup step. It ensures no runtime errors from missing MaterialDesign resources and allows the MaterialDesign NuGet packages to be uninstalled.

**Independent Test**: Search all XAML and C# files for "MaterialDesign" references. Verify zero matches. Build succeeds with no missing resource errors.

**Acceptance Scenarios**:

1. **Given** all MaterialDesign resource key references have been replaced, **When** the application starts, **Then** no runtime errors or missing resource warnings appear in diagnostics.
2. **Given** the MaterialDesign resource keys (MaterialDesignBody, MaterialDesignPaper, MaterialDesignCardBackground, MaterialDesignTextBoxBorder) were used in styles, **When** they are replaced with direct Brush.* tokens, **Then** all affected controls render with correct colors in both themes.
3. **Given** obsolete/empty theme definition files exist (e.g., DefinitionColors), **When** they are removed, **Then** the build succeeds and no references to them remain.

---

### Edge Cases

- What happens if a Fluent 2 icon name does not have a direct equivalent to the old PackIconKind? A fallback icon (e.g., generic "circle" or "question") should be used, and the mapping should be logged for review.
- What happens if WPF-UI is incompatible with the existing theme engine? The fallback strategy is documented: keep custom theme engine, use FluentIcons.WPF only for icons, skip WPF-UI controls.
- What happens if a sidebar page has no icon name registered? A default icon is displayed and a diagnostic warning is logged.
- What happens if the FluentIconConverter encounters an unrecognized icon string? It returns a fallback icon and logs a warning.
- What happens if removing MaterialDesign resource keys causes a control to lose its styling? The control falls back to the nearest Brush.* token in the theme dictionary.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST replace all `materialDesign:PackIcon` references with Fluent 2 `FluentIcon` equivalents across SidebarControl, ShellWindow, SettingsPage, and ShellStyles.
- **FR-002**: System MUST replace all `MaterialDesignFlatButton` references with Fluent-styled buttons across DiagnosticsPage, DashboardWidgetDataTemplates, and WidgetCardStyles.
- **FR-003**: System MUST replace all `MaterialDesignScrollViewer` references with WPF-UI ScrollViewer across HomePage and DiagnosticsPage.
- **FR-004**: System MUST replace all `MaterialDesignLinearProgressBar` references with WPF-UI ProgressBar in WidgetCardStyles.
- **FR-005**: System MUST replace all `MaterialDesignFont` references with Fluent theme font definitions in WidgetCardStyles.
- **FR-006**: System MUST replace all `materialDesign:Chip` references with WPF-UI or custom Fluent-styled chips in Theme/Controls content.
- **FR-007**: System MUST update SidebarRegistrationService to use Fluent 2 icon names and implement FluentIconConverter to replace MaterialIconConverter.
- **FR-008**: System MUST replace MaterialDesign resource key references (MaterialDesignBody, MaterialDesignPaper, MaterialDesignCardBackground, MaterialDesignTextBoxBorder) with direct Brush.* tokens.
- **FR-009**: System MUST remove obsolete/empty theme definition files (e.g., DefinitionColors) if they exist.
- **FR-010**: System MUST complete a WPF-UI compatibility pilot on a single low-risk page (e.g., DiagnosticsPage) before migrating all other pages.
- **FR-011**: System MUST document the fallback decision in `specs/032-fluent-icon-control-migration/fallback-decision.md` if WPF-UI proves incompatible during the pilot.
- **FR-012**: System MUST map each old `PackIconKind` value to an equivalent Fluent 2 icon name.
- **FR-013**: All XAML pages in the application MUST render correctly in both Dark and Light themes after migration (full sweep — not limited to explicitly listed files).
- **FR-014**: Zero `MaterialDesign*` references MUST remain in XAML or C# files after migration is complete.
- **FR-015**: VSTO smoke test MUST pass after migration (ribbon visible, Shell opens, sidebar renders, navigation works, theme switch works, Excel cell write works).

### Key Entities

- **FluentIcon**: The WPF-UI icon control that replaces `materialDesign:PackIcon`. Resolved at runtime by icon name string.
- **FluentIconConverter**: A value converter that maps a string icon name to a FluentIcon instance. Replaces `MaterialIconConverter`.
- **SidebarRegistrationService**: Service that registers pages with metadata (including icon name) for sidebar rendering. Updated to use Fluent 2 icon names.
- **Theme Resource Tokens**: Brush.* tokens in the theme dictionary that replace MaterialDesign resource keys (e.g., `Brush.MaterialDesignBody` → `Brush.TextPrimary`).
- **Icon Mapping Table**: A mapping document listing every PackIconKind value and its Fluent 2 equivalent (or fallback icon for gaps).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero `MaterialDesign*` references remain in the codebase (XAML and C# files) after migration.
- **SC-002**: All pages render with Fluent 2 icons in both Dark and Light themes without layout regressions.
- **SC-003**: The WPF-UI pilot page (DiagnosticsPage) renders correctly in both themes, or fallback decision is documented in `specs/032-fluent-icon-control-migration/fallback-decision.md`.
- **SC-004**: VSTO smoke test passes with zero failures after migration.
- **SC-005**: Build succeeds with zero compile errors after migration.
- **SC-006**: Sidebar dynamically resolves Fluent 2 icons for all registered pages without manual XAML changes.

## Clarifications

### Session 2026-05-31

- Q: How should unmappable icons (PackIconKind with no direct Fluent 2 equivalent) be handled? → A: Use closest Fluent 2 equivalent; document gaps in a mapping table; fallback icon for true mismatches.
- Q: Where should the WPF-UI fallback decision be documented if the pilot reveals incompatibility? → A: `specs/032-fluent-icon-control-migration/fallback-decision.md`
- Q: Does FR-013 "all pages" mean only explicitly listed files or every XAML page in the application? → A: Every XAML page in the application (full sweep).

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Phases 1A, 1B, 1C, and 2 are complete and verified before Phase 3 begins (MaterialDesign packages are still installed but will be removed in Phase 1C; this phase replaces icon/control references).
- WPF-UI (`Wpf.Ui`) package is already installed from Phase 1C.
- FluentIcons.WPF package is already installed from Phase 1C.
- CommunityToolkit.Mvvm is already installed from Phase 2.
- The existing theme engine (ThemeManager, Brush.* tokens, DynamicResource pattern) is retained.
- PackIconKind → Fluent 2 icon mappings use closest Fluent 2 equivalent where no 1:1 match exists. A mapping table documents all gaps; true mismatches use a fallback icon.
- The DiagnosticsPage is a suitable low-risk pilot target due to its limited interactivity.
- All affected files listed in the plan (SidebarControl.xaml, ShellWindow.xaml, SettingsPage.xaml, ShellStyles.xaml, DiagnosticsPage.xaml, DashboardWidgetDataTemplates.xaml, WidgetCardStyles.xaml, MaterialControls.xaml, MaterialIconConverter.cs, SidebarRegistrationService.cs, ThemeResources.xaml) exist in the current codebase.
