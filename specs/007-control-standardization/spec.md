# Feature Specification: Control Standardization

**Feature Branch**: `007-control-standardization`

**Created**: 2026-05-22

**Status**: Draft

**Input**: User description: "implementation_plan.md phase 4"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent Control Appearance Across All Windows (Priority: P1)

An enterprise user working in Excel opens different windows of the add-in (Settings, Assign Codes, Project Analysis) and expects every instance of a control — buttons, text boxes, dropdowns, checkboxes — to look and behave identically regardless of which window it appears in. No two windows should show differently styled versions of the same control type.

**Why this priority**: Visual consistency is the foundation of professional UX. Inconsistent control styling erodes user trust and makes the application feel unfinished. Every user interaction involves controls, making this the highest-impact area.

**Independent Test**: Open any three different windows side by side. Compare the appearance of buttons, text boxes, dropdowns, and checkboxes across all three windows — each control type must look identical in all windows.

**Acceptance Scenarios**:

1. **Given** multiple windows are open, **When** the user examines the same control type (e.g., a button) across different windows, **Then** all instances share identical styling — same size, color, font, corner radius, and hover/active states.
2. **Given** a TextBox control in any window, **When** the user focuses it, **Then** it shows the same focus indicator as every other TextBox in the application.
3. **Given** the user switches themes, **When** they view any control across all windows, **Then** every instance updates consistently to the new theme.

---

### User Story 2 - ComboBox Dropdowns Work Reliably Inside Excel (Priority: P1)

An analyst working in Excel opens a dropdown (ComboBox) in any add-in window. The popup list appears directly below the selected item, matches the width of the ComboBox, shows a consistent shadow, and does not clip or get cut off by window boundaries. This behavior is identical across all windows and both rendering modes (normal and safe).

**Why this priority**: ComboBox popup issues (clipping, width mismatch, missing shadows, Excel rendering artifacts) are the most frequently reported control-level bug. Fixing this has the highest direct impact on daily usability.

**Independent Test**: Open every window that contains a ComboBox, click each dropdown, and verify the popup opens with correct width, visible shadow, and no clipping. Test in both Dark and Light themes, and in safe rendering mode.

**Acceptance Scenarios**:

1. **Given** any ComboBox in any window, **When** the user opens the dropdown, **Then** the popup list appears with width equal to the ComboBox control (or wider if content requires), with a visible shadow, and no portion clipped by the window edge.
2. **Given** the application is in safe rendering mode (Excel VSTO), **When** the user opens a ComboBox, **Then** the popup renders correctly with `AllowsTransparency="False"` and the safe-mode shadow variant.
3. **Given** the ComboBox is near the bottom of a window, **When** opened, **Then** the popup opens above the control (not below) to stay within screen boundaries.

---

### User Story 3 - Smooth Scrolling in Large Data Sets (Priority: P2)

An analyst opens a window with a DataGrid or ListView containing hundreds or thousands of rows. They scroll through the data using the scroll bar, mouse wheel, or arrow keys. Scrolling is smooth, responsive, and does not cause the UI to stutter or freeze.

**Why this priority**: Enterprise users regularly work with large data sets (project schedules, trade codes, report results). Slow or jerky scrolling directly impacts productivity and creates a perception of poor quality.

**Independent Test**: Open a DataGrid loaded with 1000+ rows. Scroll to the bottom using the scroll bar, then the mouse wheel, then the down arrow key. Verify no stuttering or freezing occurs during any scrolling method.

**Acceptance Scenarios**:

1. **Given** a DataGrid with 1000+ rows, **When** the user scrolls using the vertical scroll bar, **Then** scrolling is smooth with no visible frame drops or stuttering.
2. **Given** a ListView with 500+ items, **When** the user scrolls using the mouse wheel, **Then** the list responds immediately with smooth motion.
3. **Given** a scrollable list with virtualization enabled, **When** the user scrolls rapidly, **Then** only the visible rows are rendered — memory usage does not grow linearly with total row count.

---

### User Story 4 - Keyboard Navigation Across All Controls (Priority: P2)

A user who prefers keyboard navigation (or has a motor disability) can operate every interactive control in the application using only the keyboard. Tab moves focus in logical order, Enter/Space activates the focused element, Arrow keys navigate lists and dropdowns, and Escape closes popups. Every control shows a visible focus ring when focused.

**Why this priority**: Keyboard accessibility is an enterprise requirement and benefits all users. Power users navigate faster with the keyboard, and accessibility compliance protects against legal risk. Phase 3 established accessibility patterns; this phase applies them to all remaining controls.

**Independent Test**: Navigate through every interactive element in the three most complex windows using only Tab, Enter, Space, Arrow keys, and Escape. Verify every element is reachable and activatable, and focus rings are visible at all times.

**Acceptance Scenarios**:

1. **Given** any window with interactive controls, **When** the user presses Tab repeatedly, **Then** focus moves through all interactive elements in a logical top-to-bottom, left-to-right order.
2. **Given** a TreeView has focus, **When** the user presses Arrow Up/Down, **Then** selection moves between items. When pressing Arrow Left/Right, **Then** the tree expands or collapses nodes.
3. **Given** a dialog is open, **When** the user presses Escape, **Then** the dialog closes.
4. **Given** any interactive control has focus, **Then** a visible focus ring (consistent across all control types) is displayed.

---

### User Story 5 - No Duplicate or Conflicting Control Styles (Priority: P3)

A developer maintaining the application's theme files opens any control style dictionary and finds exactly one authoritative style definition per control type. No duplicate, conflicting, or legacy styles exist. Adding a new window does not require duplicating control styles.

**Why this priority**: Style duplication leads to maintenance burden and visual drift over time. As the application grows with new windows, a clean style architecture ensures consistency is preserved without manual effort.

**Independent Test**: Search all XAML files for duplicate style `TargetType` definitions for each control type. Verify no control type has more than one style definition with the same target.

**Acceptance Scenarios**:

1. **Given** the complete set of XAML resource dictionaries, **When** searching for styles targeting a specific control type (e.g., `TargetType="Button"`), **Then** exactly one authoritative style exists in the theme control styles directory.
2. **Given** any window file, **When** inspected, **Then** it contains no inline control styles — all styling is inherited from the theme's centralized control templates.

---

### Edge Cases

- **Empty ComboBox**: If a ComboBox has no items, the popup shows an empty list or is suppressed entirely — no empty box appears.
- **Single-row DataGrid**: A DataGrid with only one row still shows correct header and border styling.
- **Disabled controls during scrolling**: If a control inside a scrollable area is disabled, its state is visually distinct and unaffected by scroll position.
- **TreeView with deep nesting**: A TreeView with 10+ levels of nesting renders correctly with proper indentation, and parent nodes expand/collapse without layout jumps.
- **Dialog on top of dialog**: When one dialog opens over another, the new dialog receives focus and the underlying dialog is inaccessible (modal behavior).
- **Reduced motion preference**: If the system "Reduce motion" accessibility setting is enabled, all control animations (hover, focus, expand/collapse) are skipped — states change instantly without transitions.
- **Style load failure**: If a control style or resource dictionary fails to load at runtime, the application logs the error and the affected control renders with default WPF styling — no crash or visible error indicator shown to the user.
- **Duplicate style conflict**: When the audit discovers multiple styles targeting the same control type, each duplicate is flagged for manual review — a human decides whether to delete, merge, or preserve each case. Automated removal is not performed.
- **Empty data source**: A DataGrid, ListView, or TreeView bound to an empty collection displays a standardized "No data available" placeholder centered in the control area, not a blank space.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every WPF control type used across the application MUST have exactly one authoritative style defined in the centralized theme control styles directory — no inline styles or per-window overrides.
- **FR-002**: All ComboBox popups MUST use `AllowsTransparency="False"`, `Placement="Bottom"` (with smart direction to open upward when near screen bottom), and `PlacementTarget` bound to the owning ComboBox.
- **FR-003**: All ComboBox popups MUST match the width of the parent ComboBox control (or expand wider if content requires), and MUST display a consistent shadow using `{DynamicResource Shadow.Popup}` (or safe-mode variant).
- **FR-004**: All scrollable containers (DataGrid, ListView, TreeView, ScrollViewer) MUST use `VirtualizationMode="Recycling"` and `EnableRowVirtualization="True"` to ensure only visible rows are rendered.
- **FR-005**: All scrollable controls MUST use the standardized thin scrollbar style from `ScrollViewerStyles.xaml` with 4px track and 8px thumb.
- **FR-006**: All interactive controls MUST display a visible focus indicator when focused, using `{DynamicResource Glow.Focus}` as the `FocusVisualStyle`.
- **FR-007**: All interactive controls MUST be reachable via sequential Tab navigation in logical order, and activatable via Enter or Space keys.
- **FR-008**: TreeView controls MUST support full keyboard navigation: Arrow keys for item selection, Left/Right for expand/collapse, and consistent item height regardless of nesting depth.
- **FR-009**: All dialog windows MUST be modal (block interaction with parent window), display a consistent title bar, and close on Escape key press.
- **FR-010**: When the system "Reduce motion" accessibility setting is active, all control animations (hover transitions, focus animations, expand/collapse effects) MUST be disabled — state changes occur instantly.
- **FR-011**: All control styles MUST reference semantic tokens (`Brush.*`, `Spacing.*`, `Radius.*`) via `{DynamicResource}` — no hardcoded colors, margins, or corner radii in control templates.
- **FR-012**: All control shadows MUST reference centralized shadow effects from `Effects/Shadows.xaml` via `{DynamicResource Shadow.*}` — no inline `DropShadowEffect` definitions.
- **FR-013**: If a control style or resource dictionary fails to load at runtime, the system MUST log the error and render the affected control with default WPF styling — no crash or user-visible error indicator.
- **FR-014**: Data-bound controls (DataGrid, ListView, TreeView) MUST display a standardized "No data available" placeholder when their data source is empty, and a loading indicator while data is being populated.

### Key Entities *(include if feature involves data)*

- **Control Style**: The authoritative XAML `Style` definition for each WPF control type, stored in the `Theme/Controls/` directory. Exactly one per control type.
- **Control Template**: The XAML `ControlTemplate` that defines the visual structure and states of a control. May be embedded within a Style or defined separately.
- **Virtualization Configuration**: The settings (`VirtualizationMode`, `EnableRowVirtualization`, `ScrollUnit`) applied to scrollable containers to ensure memory-efficient rendering of large data sets.
- **Keyboard Navigation Map**: The logical Tab order and keyboard interaction patterns defined for each control type, ensuring consistent keyboard accessibility across all windows.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of control types (Button, TextBox, ComboBox, CheckBox, RadioButton, ToggleButton, DataGrid, ListView, TreeView, PasswordBox) have exactly one centralized style definition — zero duplicate styles found across the entire application.
- **SC-002**: All ComboBox popups across all windows render with correct width, visible shadow, and no clipping in 100% of test cases across Dark, Light, and Custom themes at 100% and 150% DPI.
- **SC-003**: DataGrid scrolling with 1000+ rows completes without stuttering or freezing — frame rate remains above 30 FPS during continuous scrolling.
- **SC-004**: 100% of interactive controls are reachable via Tab navigation and activatable via keyboard (Enter/Space) — verified by automated keyboard traversal of every window.
- **SC-005**: Zero hardcoded color, margin, corner radius, or shadow values exist in any control template — all values reference semantic tokens via `{DynamicResource}`.
- **SC-006**: All control hover, focus, and press animations complete within 200ms, or are fully disabled when system "Reduce motion" is enabled — verified by timing measurement and accessibility inspection.
- **SC-007**: Zero inline `DropShadowEffect` definitions exist outside of `Effects/Shadows.xaml` and `Effects/Glow.xaml` — verified by XAML search across the entire codebase.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- All window files inherit from ModernWindow and are WPF-based — no WinForms or interop hosting is involved.
- The existing control style files in `Theme/Controls/` are the canonical location for all control styles — no styles are defined outside this directory after standardization.
- All controls use native WPF controls (not third-party) — standard WPF styling and templating applies.
- The ComboBox popup fix follows the pattern established in Phase 3 (ComboBoxStyles.xaml) and extends it to cover edge cases (screen-edge detection, safe-mode shadows).
- Virtualization is applicable to DataGrid, ListView, TreeView, and any custom ItemsControl that binds to large data collections.
- Reduced motion detection uses the standard Windows accessibility setting `SystemParameters.ClientAreaAnimation` or `UIAnimation` flag — no custom detection mechanism is needed.
- Tab order follows the visual layout order (top-to-bottom, left-to-right) unless explicit `TabIndex` values are required for complex layouts.
- Tabs and Sidebar controls are out of scope for Phase 4 — their styling and architecture are deferred to Phase 5 (Navigation Shell).
- Full diagnostic infrastructure for style resolution issues is out of scope for Phase 4 — deferred to Phase 8 (Diagnostics & Stability Platform). Only minimal error logging (FR-013) is included.

## Clarifications

### Session 2026-05-22

- Q: Should Tabs and Sidebar styling be included in Phase 4, or deferred? → A: Excluded — deferred to Phase 5 (Navigation Shell), where their full architecture is defined.
- Q: What should the application do when a control style fails to resolve or a resource dictionary fails to load? → A: Log the error and fall back to default WPF styling — no crash or visible error indicator.
- Q: Should this phase include diagnostic infrastructure for style resolution issues? → A: Deferred to Phase 8 — only minimal error logging (FR-013) included now.
- Q: When the audit discovers duplicate styles for the same control type, what is the resolution strategy? → A: Flag for manual review — automated removal is not performed.
- Q: How should data-bound controls behave when they have no data or are loading? → A: Standardized "No data available" placeholder for empty states and a loading indicator during data population, consistent across DataGrid, ListView, and TreeView.
