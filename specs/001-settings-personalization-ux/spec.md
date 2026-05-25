# Feature Specification: Settings & Personalization UX

**Feature Branch**: `010-settings-personalization-ux`

**Created**: 2026-05-25

**Status**: Draft

**Input**: User description: "Phase 7: Settings & Personalization UX - Build Windows 11 personalization experience with categorized settings, live preview, accent/background pickers, animation/density controls, and import/export"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Categorized Settings Navigation (Priority: P1)

A user opens the Settings window and sees a Windows 11-style sidebar with categories: Appearance, Performance, Accessibility, Diagnostics, Excel, and Plugins. Clicking each category reveals the relevant settings panel without navigating away from the window.

**Why this priority**: Navigation is the foundation of the settings experience. Without it, users cannot access any settings.

**Independent Test**: Can be fully tested by opening Settings and verifying that clicking each category shows the correct panel and that switching between categories is smooth and immediate.

**Acceptance Scenarios**:

1. **Given** the user is on any view in the add-in, **When** they open Settings, **Then** the settings window displays a sidebar with 6 categories
2. **Given** the settings sidebar is visible, **When** the user clicks "Appearance", **Then** the Appearance panel is displayed
3. **Given** the user is viewing a settings panel, **When** they click a different category, **Then** the new panel replaces the previous one
4. **Given** the settings window is resized, **When** the sidebar is too narrow, **Then** category icons are still visible with tooltip labels

---

### User Story 2 - Theme & Accent Live Preview (Priority: P1)

A user navigates to the Appearance panel and sees the current theme (Dark/Light/Custom) highlighted, along with accent color swatches. The user clicks a different theme card (e.g., Light) and immediately sees a live preview of how the theme would look without permanently changing it. After selecting the desired theme and accent, the user clicks "Apply" to persist the change.

**Why this priority**: Personalization is the core value of this phase. Live preview gives users confidence before committing to a theme change.

**Independent Test**: Can be fully tested by opening Appearance settings, clicking a different theme card, verifying the live preview renders correctly, and confirming that the main app theme only changes after clicking Apply.

**Acceptance Scenarios**:

1. **Given** the Appearance panel is visible, **When** the user clicks a different theme card, **Then** a live preview panel updates to show the selected theme
2. **Given** a live preview is active, **When** the user clicks "Apply", **Then** the main application theme updates to match the preview
3. **Given** a live preview is active, **When** the user clicks "Cancel" or switches to a different category, **Then** the theme reverts to its previous state
4. **Given** the user clicks an accent swatch, **When** the preview updates, **Then** the accent color reflects in the preview panel

---

### User Story 3 - Animation & Density Controls (Priority: P2)

A user navigates to the Performance category and sees a slider for animation speed (Off, Reduced, Full) and radio buttons for UI density (Compact, Normal, Spacious). Changing animation speed to "Off" immediately disables all window and control animations. Changing density to "Compact" reduces padding and spacing throughout the UI.

**Why this priority**: Performance settings directly impact user experience, especially in Excel VSTO where some users prefer reduced motion for stability.

**Independent Test**: Can be fully tested toggling each setting and verifying the UI responds accordingly without restart.

**Acceptance Scenarios**:

1. **Given** the Performance panel is visible, **When** the user sets animation speed to "Off", **Then** all window and control animations stop immediately
2. **Given** the Performance panel is visible, **When** the user sets density to "Compact", **Then** padding and spacing throughout the UI reduce immediately
3. **Given** animation speed is set to "Reduced", **When** the user opens any window, **Then** only essential animations play (≤100ms)

---

### User Story 4 - Settings Import & Export (Priority: P3)

A user navigates to the Diagnostics category and finds "Export Settings" and "Import Settings" buttons. Clicking "Export" saves current settings (theme, accent, animation, density) to a portable file. Clicking "Import" lets the user select a previously exported file and restore those settings.

**Why this priority**: Import/export provides backup capability and enables users to transfer settings between machines.

**Independent Test**: Can be fully tested by exporting settings, changing several settings, then importing the file and verifying all settings revert to the exported state.

**Acceptance Scenarios**:

1. **Given** the Diagnostics panel is visible, **When** the user clicks "Export Settings", **Then** a save file dialog opens with a suggested filename
2. **Given** the user has exported settings, **When** they click "Import Settings" and select the file, **Then** all settings restore to the exported values
3. **Given** the import file is corrupt or invalid, **When** the user attempts to import, **Then** an error message is displayed and no settings are changed

---

### Edge Cases

- What happens when all animation levels are set to "Off" but a critical notification needs user attention?
- How does the system handle importing a settings file from a newer version of the add-in that contains unknown settings keys?
- What happens if the user closes the Settings window while a live preview is active?
- How does the background picker behave when no custom images are available?
- What happens when the user opens the accent picker and the system has no predefined swatches?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Settings window MUST display a categorized sidebar with the following categories: Appearance, Performance, Accessibility, Diagnostics, Excel, Plugins
- **FR-002**: Each category MUST display a distinct settings panel when selected
- **FR-003**: Users MUST be able to preview theme changes (Dark/Light/Custom) in a dedicated preview area without the main UI changing
- **FR-004**: Users MUST be able to select accent color from 8 predefined swatches and a custom hex input
- **FR-005**: Users MUST be able to choose background style from solid or gradient options matching the current theme
- **FR-006**: Users MUST be able to set animation speed to one of: Off, Reduced, Full
- **FR-007**: Users MUST be able to set UI density to one of: Compact, Normal, Spacious
- **FR-008**: Users MUST be able to export all settings to a portable file format
- **FR-009**: Users MUST be able to import settings from a previously exported file
- **FR-010**: Settings changes MUST persist across application restarts
- **FR-011**: Settings MUST apply immediately and NOT require application restart
- **FR-012**: The Accessibility panel MUST include options for high contrast mode and focus indicator visibility
- **FR-013**: The Plugins category MUST display a message when no plugins are installed and list installed plugins when available
- **FR-014**: The Excel category MUST display VSTO-specific settings (render mode, safe mode toggle, DPI behavior)
- **FR-015**: Import MUST validate file format and report errors for corrupt or incompatible files without modifying existing settings
- **FR-016**: Export MUST NOT include sensitive data (no user data, only UI/theme preferences)
- **FR-017**: If settings persistence fails (disk full, permissions, corrupt file), system MUST display a non-blocking toast notification with a Retry button and MUST keep settings applied in-memory for the current session
- **FR-018**: Settings sidebar MUST support keyboard navigation: Tab to focus the sidebar, then Up/Down arrows to switch between categories

### Key Entities

- **SettingsCategory**: Represents a top-level settings group with a name, icon, and associated settings panel
- **UserSettings**: Represents the complete set of user-configurable preferences including theme, accent color, animation speed, density, and accessibility options
- **SettingsExport**: Represents a serialized snapshot of UserSettings for import/export operations, containing version metadata for forward compatibility

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can switch between any two category panels in under 500ms
- **SC-002**: Live preview renders the selected theme within 1 second of user selection
- **SC-003**: Theme changes applied from settings propagate to all open windows within 2 seconds
- **SC-004**: Animation speed change to "Off" stops all UI animations immediately (no animation plays longer than 50ms)
- **SC-005**: Density change to "Compact" reduces the vertical height of all control templates by at least 25%
- **SC-006**: Export completes within 1 second and produces a file under 100KB
- **SC-007**: Import of a valid file completes within 2 seconds and all settings match the exported values
- **SC-008**: Import of a corrupt file shows a clear error message and leaves all current settings unchanged

## Clarifications

### Session 2026-05-25

- Q: Live preview Apply/Cancel flow — how does category navigation interact with the preview? → A: Category switching auto-cancels the preview. User must explicitly click Apply before leaving Appearance to persist changes. Apply commits changes immediately; Cancel or navigating to another category reverts the preview.
- Q: How should the system handle settings save failures (disk full, permissions, corrupt file)? → A: Show a non-blocking toast notification with "Settings could not be saved" and a Retry button. Settings remain applied in-memory for the current session.
- Q: How should keyboard navigation work for the settings window sidebar categories? → A: Tab to focus the sidebar, then Up/Down arrows to switch between categories. Matches Windows 11 Settings pattern.
- Q: What are the default values for new settings (animation, density, background, accent)? → A: Animation: Full, Density: Normal, Background: Dark gradient, Accent: Blue (#3A86FF). Matches current app defaults.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Live preview uses a dedicated preview area within the Appearance panel (not full-window preview) to minimize complexity
- Image backgrounds are excluded from v1 scope due to Excel VSTO rendering complexity; only solid and gradient backgrounds are supported
- Plugins category shows a placeholder/stub since the plugin platform (Phase 9) has not been implemented yet
- Animation "Off" disables all storyboard-based animations but allows instant state transitions
- Density settings affect spacing tokens (padding, margins) but do not change font sizes or control layout fundamentals
- Import/export uses JSON format with version field for forward compatibility
- Excel category settings affect RenderModeService behavior (safe mode toggle, DPI behavior)
- Settings are persisted via the existing Properties/Settings.settings mechanism extended with export/import capabilities
- Default values upon first install or reset: Animation=Full, Density=Normal, Background=Dark gradient, Accent=Blue (#3A86FF)
