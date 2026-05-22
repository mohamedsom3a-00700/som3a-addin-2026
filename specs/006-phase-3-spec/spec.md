# Feature Specification: Theme Engine 2.0

**Feature Branch**: `006-phase-3-spec`

**Created**: 2026-05-22

**Status**: Draft

**Input**: User description: "implementation_plan.md phase 3"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Theme Selection and Switching (Priority: P1)

An enterprise user working in Excel needs to customize the add-in's appearance to match their preference or lighting environment. They open the settings window, see visual preview cards for Dark, Light, and Custom themes, click one, and the entire application updates instantly — all windows, controls, and effects. The next time they open Excel, their chosen theme is still active.

**Why this priority**: Theme switching is the MVP capability. Without it, users cannot personalize their experience. All subsequent personalization features (accent colors, backgrounds) depend on the theme infrastructure being in place.

**Independent Test**: Open the settings window, click the Light theme card, verify all open windows update to the light appearance within 1 second. Close and reopen the add-in — the Light theme must still be active.

**Acceptance Scenarios**:

1. **Given** the add-in is running inside Excel, **When** the user opens settings and clicks a theme card (Dark, Light, or Custom), **Then** all open windows immediately reflect the selected theme.
2. **Given** the user has selected a theme, **When** they close and reopen the add-in, **Then** the previously selected theme is restored automatically.
3. **Given** the user is on the Custom theme, **When** they click an accent color swatch, **Then** the accent color updates across all controls — buttons, highlights, progress bars, and glow effects — immediately.

---

### User Story 2 - Consistent Visual Quality Across Controls (Priority: P1)

A user interacts with various controls (buttons, text boxes, checkboxes, dropdowns, data grids) and expects consistent visual feedback — hover highlights, focus rings, pressed states, and disabled appearance — all respecting the active theme. Every control looks and behaves the same way regardless of which theme is selected.

**Why this priority**: Visual consistency is the hallmark of a professional enterprise application. Users expect controls to behave predictably. Inconsistent states erode trust and reduce productivity.

**Independent Test**: Open any window with interactive controls. Hover over a button, tab into a text box, click a checkbox, open a dropdown. Verify each has a visible state change. Switch the theme and repeat — all states must still be visible and consistent.

**Acceptance Scenarios**:

1. **Given** any interactive control (button, text box, checkbox, radio button, toggle, dropdown), **When** the user hovers over it, **Then** a visible hover state appears.
2. **Given** any interactive control, **When** the user tabs to focus it, **Then** a visible focus indicator appears.
3. **Given** a dropdown (ComboBox), **When** the user opens it, **Then** the popup list appears above other content with a visible shadow and no clipping.
4. **Given** any control in a disabled state, **When** viewed, **Then** its appearance is visually distinct from the enabled state.

---

### User Story 3 - DPI-Aware Rendering (Priority: P2)

A user works on a high-resolution display (125%, 150%, or 200% scaling) and expects all UI elements — controls, theme cards, accent swatches, popups — to scale proportionally without text clipping, overlapping elements, or pixelated rendering.

**Why this priority**: Enterprise users commonly use high-DPI laptops and external monitors. Unreadable or broken UI at non-standard scaling is a blocker for daily work.

**Independent Test**: Set the display to 150% scaling, open all major windows, verify every element is readable and properly sized. Repeat at 125% and 200%.

**Acceptance Scenarios**:

1. **Given** the display is set to 125%, 150%, or 200% scaling, **When** the user opens any window, **Then** all text is readable with no clipping or overflow.
2. **Given** the display is set to any supported scaling, **When** the user opens a dropdown, **Then** the popup is correctly sized and positioned.
3. **Given** the display is set to any supported scaling, **When** the user views theme cards and accent swatches, **Then** they are proportionally sized.

---

### User Story 4 - Stable Performance Inside Excel (Priority: P2)

An analyst processes a large project schedule inside Excel with the add-in open. They scroll through data grids with hundreds of rows and switch themes while working. The add-in remains responsive — no freezing, no stuttering, no frame drops.

**Why this priority**: Excel VSTO hosting is inherently constrained. Performance regressions directly impact user productivity and trust in the add-in.

**Independent Test**: Open a window with a data grid containing 500+ rows. Scroll continuously for 30 seconds — no stuttering. Switch themes rapidly 10 times in succession — no freezing, and only the final theme is applied.

**Acceptance Scenarios**:

1. **Given** a data grid with 1000+ rows, **When** the user scrolls through it, **Then** scrolling is smooth with no frame drops.
2. **Given** the user switches themes 10 times in rapid succession, **When** they stop, **Then** only the final theme is applied and the UI remains responsive.
3. **Given** the add-in is running inside Excel on a system with known rendering limitations, **When** it starts, **Then** it automatically detects the limitation and activates a safe rendering mode that prevents black windows and artifacts.

---

### User Story 5 - Keyboard Navigation and Accessibility (Priority: P3)

A user who relies on keyboard navigation (due to preference or accessibility need) can operate all settings and controls using only the keyboard — Tab to move between elements, Enter/Space to activate, Arrow keys to navigate lists, Escape to close. Focus indicators are clearly visible, and screen readers can identify all interactive elements.

**Why this priority**: Accessibility is a core enterprise requirement. Keyboard navigation benefits all users (power users prefer keyboard shortcuts) and is mandatory for users with motor disabilities. WCAG 2.1 AA compliance protects against legal risk.

**Independent Test**: Navigate the entire settings window using only Tab, Enter, Space, and Escape. Verify every interactive element is reachable and activatable. Verify focus rings are visible in both Dark and Light themes.

**Acceptance Scenarios**:

1. **Given** the settings window is open, **When** the user presses Tab repeatedly, **Then** focus moves through all interactive elements in a logical order.
2. **Given** a theme card or accent swatch has keyboard focus, **When** the user presses Enter or Space, **Then** the theme or accent is applied.
3. **Given** a dropdown (ComboBox) has focus, **When** the user presses Arrow keys, **Then** the dropdown opens and options are navigable.
4. **Given** the user presses Escape on any window, **Then** the window closes.

---

### Edge Cases

- **Corrupted theme preference**: If the saved theme setting is corrupted or missing, the system defaults to Dark theme automatically without crashing.
- **Rapid theme switching**: If the user clicks multiple theme cards in rapid succession, only the final selection is applied — intermediate themes are skipped to prevent visual flickering.
- **Accent color persistence**: When switching from Custom theme to Dark and back to Custom, the previously selected accent color is preserved.
- **Missing resource**: If a theme resource dictionary fails to load, the previous theme remains active and a fallback log entry is recorded.
- **Windows open during theme switch**: If multiple windows are open when the theme changes, all of them update simultaneously — none remain stuck on the old theme.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to choose between at least three visual themes — Dark, Light, and Custom — through the settings window.
- **FR-002**: When a user selects a theme, the change MUST apply immediately (within 1 second) to all open windows simultaneously.
- **FR-003**: The selected theme MUST persist across application restarts and be restored automatically on next launch.
- **FR-004**: Custom theme users MUST be able to select from exactly 8 predefined accent color swatches (no custom hex or color picker), and the accent MUST apply immediately to all accent-dependent elements (button highlights, progress fills, selection indicators, glow effects).
- **FR-005**: All interactive controls (buttons, text boxes, checkboxes, radio buttons, toggles, dropdowns, data grids) MUST show distinct visual states for normal, hover, focused, pressed, and disabled — respecting the active theme.
- **FR-006**: Dropdown popups (ComboBox) MUST render above all other content with consistent shadow, correct sizing, and no clipping in both normal and safe rendering modes.
- **FR-007**: The application MUST automatically detect when it is running inside Excel VSTO and activate a safe rendering mode that prevents black windows, transparency artifacts, and rendering failures. Safe mode activation MUST occur silently without any user notification or indicator.
- **FR-008**: All UI elements MUST be readable and correctly proportioned at 100%, 125%, 150%, and 200% DPI scaling — no text clipping or element overflow.
- **FR-009**: Data grids MUST render smoothly with 1000+ rows through automatic virtualization — no frame drops during scrolling.
- **FR-010**: All interactive elements MUST be reachable and activatable using only the keyboard (Tab, Enter, Space, Arrow keys, Escape).
- **FR-011**: Visible focus indicators MUST be present on all interactive elements in both Dark and Light themes.
- **FR-012**: Text elements MUST meet WCAG 2.1 AA contrast ratio standards (minimum 4.5:1 for normal text) in all themes.
- **FR-013**: When a user switches themes rapidly (10+ times in quick succession), the system MUST coalesce requests and apply only the final selection without crashing or visual flickering.
- **FR-014**: If a theme resource fails to load, the previously active theme MUST remain in effect and the failure MUST be gracefully handled without crashing the application.

### Key Entities *(include if feature involves data)*

- **Theme Preference**: A stored user setting indicating which visual theme is active (Dark, Light, or Custom). Persisted locally between sessions.
- **Accent Color**: A stored user setting representing the selected accent color hex value. Persisted alongside the theme preference and preserved when switching away from and back to Custom theme.
- **Theme Resource Set**: The collection of visual definitions (colors, brushes, spacing, effects) that define a complete theme. Three built-in sets exist (Dark, Light, Custom) with the Custom set incorporating the user's chosen accent color.
- **Render Mode**: A runtime-determined setting (Normal or Safe) that controls how windows are rendered, automatically detected based on the hosting environment (Excel VSTO) and system capabilities.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can switch between any two themes in under 1 second, measured from click to full visual update across all open windows.
- **SC-002**: Theme preference persists correctly across 100% of application restarts — no cases of the wrong theme loading or the default theme appearing unexpectedly.
- **SC-003**: Accent color changes reflect across all accent-dependent elements within 1 second (glow effects, progress bars, selection highlights, button hover states).
- **SC-004**: All 7 control types (button, text box, checkbox, radio button, toggle, dropdown, data grid) show correct visual states for hover, focus, press, and disabled in both Dark and Light themes.
- **SC-005**: Text elements in both Dark and Light themes achieve WCAG 2.1 AA contrast ratio (4.5:1 minimum for normal text).
- **SC-006**: The application remains responsive during rapid theme switching (10 switches in 5 seconds) with no crashes, freezes, or visual artifacts.
- **SC-007**: All interactive elements have visible keyboard focus indicators in both Dark and Light themes.
- **SC-008**: Dropdown popups render correctly (above content, with shadow, no clipping) in 100% of test cases across both Normal and Safe rendering modes.
- **SC-009**: Every window renders correctly at 100%, 125%, 150%, and 200% DPI — no text clipping, element overlap, or sizing errors.
- **SC-010**: No crashes, black windows, or rendering artifacts occur when the application runs inside Excel VSTO host.
- **SC-011**: When the application automatically detects a rendering-limited environment, safe mode activates without user intervention and all windows render correctly.

## Clarifications

### Session 2026-05-22

- Q: Should users be able to specify arbitrary accent colors beyond the 8 presets? → A: 8 presets only, no custom hex input or color picker.
- Q: Should the user be notified when safe rendering mode activates? → A: No notification — safe mode activates silently without user awareness.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The existing ThemeManager service and ModernWindow control will be extended, not replaced.
- Users have access to the settings window through the existing ribbon navigation.
- All users run Windows 10 or 11 with .NET Framework 4.8 installed.
- The application runs exclusively as an Excel VSTO add-in — standalone hosting is not in scope.
- Theme preferences are stored locally only — no cloud sync or roaming profiles are required.
- No third-party UI frameworks will be used — all theming is native WPF.
- The existing window set (14 windows) covers the full scope of the application — no new windows are created by this feature.
