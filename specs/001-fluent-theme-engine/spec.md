# Feature Specification: WPF Fluent UI Migration — Theme Engine & Runtime Switching

**Feature Branch**: `[001-fluent-theme-engine]`

**Created**: 2026-05-18

**Status**: Draft

**Input**: User description: "WPF Fluent UI migration for Excel-hosted VSTO add-in. Runtime theme switching (Dark/Light/Custom), theme cards UI, centralized shadow/glow/animation system, VisualStateManager, DPI-aware, performance-budget-compliant."

## Clarifications

### Session 2026-05-18

- Q: Custom accent color selection mechanism → A: Preset accent swatches (6-8 options: Blue, Green, Purple, Orange, Pink, Teal, Red, Cyan) displayed as clickable color circles on the Custom theme card.
- Q: Should the Custom theme's accent color also persist across restarts? → A: Both the theme selection and accent color persist. If user selects "Custom + Teal", it is restored on restart.
- Q: Which controls should receive VSM state management? → A: Focus migration on four high-impact controls: ComboBox (PopupOpen state), Button, ToggleButton, and ThemeCards. Others remain on Trigger-based states for this feature scope.
- Q: How should fallback-safe mode activate? → A: Automatic runtime detection. The system detects Excel hosting rendering issues at startup and self-activates fallback mode silently without user action.
- Q: What animation types should be included? → A: Control state transitions (hover, focus, press) plus popup open/close (slide-down, fade-in). All animations ≤200ms. Page/panel slide transitions are out of scope for this feature.

## User Scenarios & Testing

### User Story 1 - Theme Selection via Settings Window (Priority: P1)

A user opens the Settings window from within Excel and switches the application theme from Dark to Light. The UI updates instantly across all open windows without restarting Excel. The user closes and reopens the app, and the Light theme preference is restored.

**Why this priority**: Theme selection is the primary user-facing feature of this migration. It directly demonstrates the runtime theme switching capability and delivers immediate visual customization value.

**Independent Test**: Can be fully tested by opening SettingsWindow, clicking a theme card, observing instant UI updates across all windows, restarting the app, and confirming the theme persisted.

**Acceptance Scenarios**:

1. **Given** the app is running with Dark theme, **When** the user clicks the "White (Light)" theme card in SettingsWindow, **Then** all open windows immediately switch to Light theme colors without visual glitches.

2. **Given** the user has selected Light theme and closed the app, **When** the user reopens the app, **Then** Light theme is automatically applied on startup.

3. **Given** the user has selected "Custom" theme with accent color "Teal" and closed the app, **When** the user reopens the app, **Then** both the Custom theme and Teal accent are automatically restored on startup.

4. **Given** the user has selected "Custom" theme, **When** the user clicks a different accent color swatch (e.g., Purple) on the Custom theme card, **Then** all accent-colored UI elements immediately reflect the new Purple accent color.

---

### User Story 2 - Visual Quality & Consistency (Priority: P1)

A user interacts with buttons, ComboBoxes, text inputs, and data grids across Dark and Light themes. All controls display correct hover, focus, pressed, and disabled states with smooth transitions. Popups render above all content without clipping.

**Why this priority**: Consistent visual quality across all themes and controls is the core deliverable of the Fluent migration. Poor visual quality in any state/theme combination undermines the premium enterprise software goal.

**Independent Test**: Can be fully tested by iterating through all controls (Button, ComboBox, TextBox, CheckBox, RadioButton, ToggleButton, DataGrid) in both Dark and Light themes, triggering all interactive states, and verifying popup behavior.

**Acceptance Scenarios**:

1. **Given** the Dark theme is active, **When** the user hovers over a button, **Then** the button displays an accent-colored border and subtle glow transition.

2. **Given** any theme is active, **When** a ComboBox is opened, **Then** the dropdown popup renders above all other content with a visible drop shadow and no clipping.

3. **Given** the Light theme is active, **When** the user focuses a text input, **Then** a visible focus ring appears and all text maintains sufficient contrast ratio (≥4.5:1).

4. **Given** any theme is active, **When** a disabled control is displayed, **Then** the control is visually dimmed but clearly visible and legible.

---

### User Story 3 - DPI Scaling Across Configurations (Priority: P2)

A user works on a high-DPI monitor (150% scaling). All UI elements, including text, buttons, and data grids, scale correctly. No text is clipped, no elements overflow their containers, and popups render at the correct size and position.

**Why this priority**: DPI support is a non-negotiable requirement for desktop productivity software. Users on high-DPI displays must have a fully functional experience. This is critical for Excel-hosted environments where system DPI settings vary.

**Independent Test**: Can be fully tested by running the app on a system with 125%, 150%, and 200% DPI scaling and verifying all UI elements scale proportionally.

**Acceptance Scenarios**:

1. **Given** the system DPI is set to 150%, **When** the user opens the SettingsWindow, **Then** all text and UI elements are crisp, properly sized, and no elements overflow their containers.

2. **Given** the system DPI is set to 200%, **When** a ComboBox is opened, **Then** the popup renders at the correct size with proper alignment to the parent control.

3. **Given** the system DPI is set to 100%, **When** the user switches themes, **Then** all UI elements maintain their proportions and layout.

---

### User Story 4 - Performance Stability in Excel Host (Priority: P2)

A user works with the add-in running inside Excel. The UI remains responsive during data grid scrolling, window resizing, and theme switching. No UI freezing, stuttering, or memory issues occur during normal operation.

**Why this priority**: VSTO add-ins hosted inside Excel have strict performance constraints. A sluggish or unstable UI inside Excel destroys user trust and adoption. Performance compliance is a blocking requirement per the architecture constitution.

**Independent Test**: Can be fully tested by running the add-in inside Excel, performing normal operations (scroll DataGrid, resize windows, switch themes 10 times), and monitoring for UI freezes or memory growth.

**Acceptance Scenarios**:

1. **Given** the add-in is running inside Excel with a DataGrid displayed, **When** the user scrolls through 500+ rows, **Then** scrolling is smooth with no visible frame drops or stuttering.

2. **Given** the add-in is running inside Excel, **When** the user switches themes 10 times in rapid succession, **Then** no UI freezing or noticeable lag occurs.

3. **Given** the add-in is running inside Excel, **When** the user resizes a window, **Then** the window resizes smoothly without visual artifacts.

---

### User Story 5 - Keyboard Navigation & Accessibility (Priority: P3)

A user navigates the entire application using only the keyboard. All interactive elements are reachable via Tab, activatable via Enter/Space, and focus indicators are clearly visible. The app maintains WCAG 2.1 AA contrast ratios.

**Why this priority**: Keyboard accessibility and WCAG compliance are enterprise-grade requirements. Many users rely on keyboard navigation in productivity environments. Accessibility is a core part of the Feature Completeness Standard.

**Independent Test**: Can be fully tested by navigating the entire app using only Tab, Arrow keys, Enter, Space, and Escape, verifying all elements are reachable and activatable, and checking contrast ratios.

**Acceptance Scenarios**:

1. **Given** the SettingsWindow is open, **When** the user tabs through all controls, **Then** every interactive element receives a visible focus indicator.

2. **Given** the SettingsWindow is open, **When** the user navigates theme cards with Tab and activates one with Enter, **Then** the theme switches exactly as if the card was clicked.

3. **Given** the Dark theme is active, **When** all text is measured against its background, **Then** every text element meets a minimum 4.5:1 contrast ratio (WCAG AA).

---

### Edge Cases

- **Theme switch during popup open**: When a popup (ComboBox dropdown) is open and the user switches themes, the popup should close gracefully before the theme applies to avoid visual artifacts.
- **DPI change while app is running**: Windows DPI change events should be handled gracefully; the app should re-scale UI elements if possible.
- **WebView2 instability during theme switch**: If WebView2 content is present, theme switches should not cause WebView2 to reload or crash.
- **Excel hosting edge cases**: On some Excel configurations, `AllowsTransparency=True` may cause rendering issues; the fallback-safe mode should automatically activate without user action.
- **Theme card rapid clicking**: Clicking theme cards in rapid succession should not cause race conditions or duplicate theme applications.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST provide three built-in themes: Dark, Light, and Custom.
- **FR-002**: The system MUST allow users to switch themes at runtime without restarting the application.
- **FR-003**: The system MUST persist the selected theme preference and, for the Custom theme, the selected accent color across application restarts.
- **FR-004**: The system MUST apply the selected theme to all open windows simultaneously upon theme change.
- **FR-004b**: The system MUST implement VisualStateManager for four high-priority controls: ComboBox (PopupOpen state), Button, ToggleButton, and ThemeCards. Other controls remain on Trigger-based states for this feature scope.
- **FR-005**: The system MUST render theme selection via visual theme cards (Dark, White, Custom) in the Settings window.
- **FR-006**: The system MUST display a preview thumbnail, theme name, and brief description on each theme card.
- **FR-007**: The system MUST apply a glow effect to the selected theme card.
- **FR-008**: The system MUST centralize all color definitions in a two-tier token system (primitive + semantic tokens).
- **FR-009**: All control templates MUST reference colors via DynamicResource, enabling runtime theme switching.
- **FR-010**: The system MUST support a Custom theme with a user-selectable accent color. Accent selection is provided via 6-8 preset swatches (Blue, Green, Purple, Orange, Pink, Teal, Red, Cyan) displayed as clickable color circles on the Custom theme card.
- **FR-011**: The system MUST render popups (ComboBox dropdowns, etc.) above all other content without clipping.
- **FR-012**: The system MUST maintain DPI scaling correctness at 100%, 125%, 150%, and 200% DPI.
- **FR-013**: The system MUST provide a centralized animation library with control state transitions (hover, focus, press) and popup open/close animations (slide-down, fade-in). All animations MUST complete within 200ms.
- **FR-014**: The system MUST avoid performance bottlenecks: no nested DropShadows, no BlurEffect on scrolling containers, virtualized DataGrid rows.
- **FR-015**: The system MUST use WindowChrome for borderless window rendering. Fallback-safe mode (WindowStyle=None + AllowsTransparency=True) MUST automatically activate via runtime detection when Excel hosting rendering issues are detected, without user action.
- **FR-016**: The system MUST validate every theme against an 8-gate Theme Validation Checklist before completion.

### Key Entities

- **Theme**: Represents a visual configuration containing color token overrides. Attributes: name, color palette, accent color, isCustom flag.
- **ThemeCard**: Visual component representing a selectable theme option. Attributes: theme reference, preview gradient, name label, description, isSelected flag.
- **ColorToken**: A named color value used in the UI. Two types: Primitive (raw palette value) and Semantic (UI-meaning alias referencing a primitive).
- **ThemeManager**: Service responsible for loading theme dictionaries, swapping theme resources at runtime, and persisting user preferences.
- **ControlStyle**: A WPF style/template defining the visual appearance and interactive states of a control type.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can switch between Dark, Light, and Custom themes in under 1 second with no visible flicker.
- **SC-002**: Theme preference persists across application restarts with 100% reliability.
- **SC-003**: All open windows update simultaneously to the new theme within 500ms of user selection.
- **SC-004**: 100% of controls (Button, ComboBox, TextBox, CheckBox, RadioButton, ToggleButton, DataGrid) display all required interactive states (Normal, Hover, Pressed, Focused, Disabled) in both Dark and Light themes.
- **SC-005**: All text maintains a minimum 4.5:1 contrast ratio against its background in both Dark and Light themes.
- **SC-006**: UI rendering remains stable and responsive inside Excel VSTO host during DataGrid scrolling (no frame drops for lists under 1000 rows).
- **SC-007**: All theme cards in SettingsWindow are fully keyboard navigable (Tab to reach, Enter/Space to select).
- **SC-008**: 100% of ComboBox dropdowns render above all other content with a visible drop shadow and no clipping on standard DPI configurations.
- **SC-009**: All UI elements scale correctly at 100%, 125%, 150%, and 200% DPI with no text clipping or element overflow.
- **SC-010**: Theme switching does not cause WebView2 reload or crash when WebView2 content is present.

## Assumptions

- **Target users**: Enterprise users working with Excel-based productivity workflows. Users expect a polished, consistent desktop application experience similar to modern Microsoft Office apps.
- **Platform**: Windows desktop running Excel 2016 or later. The add-in is a VSTO solution targeting .NET Framework 4.8.
- **Performance expectations**: VSTO add-ins have stricter performance constraints than standalone apps due to Excel hosting overhead. The Performance Budget Rules (no nested DropShadows, virtualization, ≤200ms animations) are non-negotiable.
- **Existing architecture**: The project already has a partial theme system (Colors.xaml, base control styles). The migration builds on this foundation incrementally, per the Incremental Migration Rules.
- **Excel hosting compatibility**: Some Excel configurations have rendering issues with `AllowsTransparency=True`. WindowChrome is the preferred rendering path, with a fallback-safe mode available for edge cases.
- **No third-party UI frameworks**: The constitution explicitly prohibits introducing third-party UI frameworks. All UI work uses native WPF with custom styling.
- **MVVM structure**: Business logic remains in ViewModels; code-behind is minimal and only used where Office interop requires it.
- **Token naming convention**: Controls use semantic tokens (Brush.Background.Primary, Brush.Text.Primary, etc.) alongside legacy flat keys for backward compatibility. No inline color values in control templates.
- **Custom theme extensibility**: The Custom theme supports a user-selectable accent color via preset swatches (Blue, Green, Purple, Orange, Pink, Teal, Red, Cyan). Both the theme selection (Dark/Light/Custom) and the accent color persist across application restarts.