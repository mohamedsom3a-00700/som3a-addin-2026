# Feature Specification: Theme Expansion

**Feature Branch**: `017-theme-expansion`

**Created**: 2026-05-26

**Status**: Draft

**Input**: User description: "Enhance the existing Fluent theme engine with Material Design integration, background image support, blur effects, font selection, and shell window controls."

## Clarifications

### Session 2026-05-26

- Q: Where should the custom background image be visible? → A: Shell workspace only (the main content area behind pages)
- Q: Should there be explicit file size and/or resolution limits for background images? → A: Max 10MB file size, max 4096px in any dimension
- Q: Should the dark theme also be required to meet WCAG 2.1 AA contrast ratios? → A: Yes, both dark and light themes must meet WCAG 2.1 AA
- Q: Should users also be able to adjust global font size in this phase? → A: Font size customization is out of scope for Phase 17; deferred to Phase 24
- Q: Which specific Material Design controls beyond icons and dialogs should be adopted? → A: Sliders, toggles, and chips (per enterprise plan P17-T001)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - User Browses with Material Design Icons (Priority: P1)

A planning engineer navigates the application sidebar, toolbar, and ribbon and sees modern Material Design icons replacing all generic glyphs. Icons are uniformly styled, theme-aware, and scale correctly across DPI settings.

**Why this priority**: Visual consistency is the most noticeable improvement for users. Material icons provide a modern, professional appearance across every interaction point immediately upon launch.

**Independent Test**: Can be fully tested by opening the Shell workspace and verifying all sidebar categories, navigation items, toolbar buttons, and ribbon groups display Material Design icons consistently in both dark and light themes.

**Acceptance Scenarios**:

1. **Given** the Shell workspace is open in dark theme, **When** the user views the sidebar, **Then** all navigation categories display Material Design icons with correct accent coloring
2. **Given** the Shell workspace is open, **When** the user switches from dark to light theme, **Then** all Material icons remain visible and adapt to the active theme's foreground colors
3. **Given** a Material DialogHost is configured, **When** any system dialog opens (settings confirm, error, warning), **Then** it renders through the Material DialogHost with consistent styling

---

### User Story 2 - User Customizes Background Image and Blur (Priority: P2)

A user opens the Custom theme settings, selects a background image from their filesystem, adjusts blur intensity with a slider, and sees the workspace background update in real-time.

**Why this priority**: Background personalization significantly enhances user experience and brand customization but is aesthetic rather than functional. The core Fluent theme works without it.

**Independent Test**: Can be fully tested by opening Custom theme settings, picking a JPG/PNG image, adjusting blur from 0% to 100%, and verifying the Shell workspace background reflects changes immediately.

**Acceptance Scenarios**:

1. **Given** the Custom theme settings page, **When** the user selects a background image file (PNG/JPG), **Then** the image is applied as the workspace background within 1 second
2. **Given** an active background image, **When** the user adjusts the blur slider, **Then** blur intensity changes in real-time from sharp (0%) to fully blurred (100%)
3. **Given** a blurred background image, **When** the user toggles blur off, **Then** the background returns to sharp without image reloading
4. **Given** a background image is active, **When** the user switches to Dark or Light theme preset, **Then** the background image is preserved and reusable when returning to Custom theme

---

### User Story 3 - User Selects Application Font (Priority: P2)

A user browses available font families, previews each with a thumbnail showing sample text, selects a preferred font, and sees all application text update dynamically without restart.

**Why this priority**: Font customization is important for accessibility and regional preferences (especially Arabic script support for Phase 24), but the application is fully usable with the default font.

**Independent Test**: Can be fully tested by navigating to font settings, selecting a different font family, and verifying that all window text (shell, settings, dialogs) renders in the new font without restart.

**Acceptance Scenarios**:

1. **Given** the font selection page in Custom theme settings, **When** the user views available fonts, **Then** each font displays a preview thumbnail with sample text ("The quick brown fox jumps over the lazy dog")
2. **Given** the user selects a new font family, **When** they confirm the selection, **Then** all open windows update their text to the new font within 500ms
3. **Given** Arabic localization is active (Phase 24 readiness), **When** the user selects an Arabic font family, **Then** Arabic script renders correctly with proper shaping and ligatures
4. **Given** a custom font is selected, **When** the user switches between dark and light themes, **Then** the font selection is preserved

---

### User Story 4 - User Interacts with Refined Light Theme (Priority: P3)

A user switches to the light theme and sees corrected contrast ratios, consistent accent colors, and all UI elements remain clearly distinguishable. The theme passes WCAG 2.1 AA standards.

**Why this priority**: Light theme usability is important for daylight/office environments but the dark theme is the primary default and works without these fixes.

**Independent Test**: Can be fully tested by switching to the light theme, running a contrast checker against all semantic token pairs, and verifying all ratios meet or exceed 4.5:1 for normal text and 3:1 for large text.

**Acceptance Scenarios**:

1. **Given** the light theme is active, **When** text is rendered against its surface background, **Then** the contrast ratio is at least 4.5:1 for normal text and 3:1 for large text
2. **Given** the light theme is active, **When** the accent color is applied to buttons, links, and interactive elements, **Then** the accent color is consistent with dark theme accent values
3. **Given** the light theme, **When** a disabled control is rendered, **Then** its visual state is clearly distinguishable from its enabled state

---

### User Story 5 - User Customizes Accent Color (Priority: P2)

A user opens the accent color picker, uses a color wheel to select a custom accent, enters a hex code directly, or chooses from auto-generated accent variants. The accent updates across all windows in real-time.

**Why this priority**: Accent customization is a core feature of the existing ThemeManager. Enhancing it with a color wheel and hex input improves user expressiveness without changing the underlying architecture.

**Independent Test**: Can be fully tested by opening the accent picker in Settings, using each input method (color wheel, hex input, variant swatches), and verifying consistent accent application across the Shell, ribbon, and all open pages.

**Acceptance Scenarios**:

1. **Given** the accent picker is open, **When** the user drags on the color wheel, **Then** the accent preview updates in real-time
2. **Given** the accent picker is open, **When** the user enters a hex color code (e.g., `#FF5733`), **Then** the accent updates to the exact specified color
3. **Given** an accent color is selected, **When** the system generates accent variants, **Then** hover, pressed, glow, border, and subtle variants are computed with perceptually consistent relationships

---

### User Story 6 - Window Controls Render Consistently (Priority: P1)

A user interacts with the minimize, maximize, restore, and close buttons on any application window. All buttons have consistent styling, theme-aware hover/pressed states, and proper hit-test regions.

**Why this priority**: Window controls are a fundamental interaction surface used by every user in every session. Inconsistent controls erode trust and professionalism.

**Independent Test**: Can be fully tested by opening any window (Shell, Settings, dialogs), hovering and pressing each window control button, and verifying consistent visual feedback across all windows in both dark and light themes.

**Acceptance Scenarios**:

1. **Given** any application window (Shell, Settings, or dialog), **When** the user hovers over the close button, **Then** the button shows a red hover state with smooth transition
2. **Given** any application window, **When** the user hovers over minimize/maximize buttons, **Then** the buttons show accent-colored hover states
3. **Given** a maximized window, **When** the user hovers over the title bar drag region, **Then** only the drag region responds to drag (window control buttons are excluded from drag)

---

### Edge Cases

- What happens when a selected background image file is deleted or moved after being set? System should fall back to the default solid/gradient backdrop and notify the user.
- What happens when a font that was previously selected is no longer available on the system? System should fall back to the default Segoe UI font and show a notification.
- What happens when blur intensity is set to maximum (100%)? The background should be fully blurred but maintain reasonable performance (no rendering stutter).
- What happens when Material Design icons fail to load? System should fall back to existing Fluent glyphs or text labels without crashing.
- What happens when accent variant generation produces near-identical colors? System should enforce minimum perceptual distance between variants.
- What happens with window controls at high DPI (150%, 200%, 300%)? Buttons should scale correctly and remain clickable with proper hit-test areas.
- What happens when switching themes rapidly (multiple times per second)? System should debounce or queue theme changes to prevent visual flicker.
- What happens when a corrupt image file is selected as background? System should gracefully reject it with a clear error message.
- What happens when an image exceeds the 10MB size or 4096px dimension limits? System should reject the file with a clear error message indicating the limit that was exceeded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST integrate Material Design icon library for sidebar categories, navigation items, toolbar buttons, and ribbon groups
- **FR-002**: System MUST render all system dialogs through Material DialogHost with theme-consistent styling
- **FR-002a**: System MUST replace existing WPF sliders, toggles, and chips with Material Design themed equivalents
- **FR-003**: System MUST support setting a custom background image (PNG, JPG, BMP) for the Custom theme via file picker, displayed as the Shell workspace background only (not on dialogs or popups), with a max file size of 10MB and max dimension of 4096px
- **FR-004**: System MUST provide a blur intensity control (slider, 0-100%) that applies Gaussian blur to the background image in real-time
- **FR-005**: System MUST provide a font family selector showing all installed system fonts with preview thumbnails
- **FR-006**: System MUST apply font changes dynamically to all open windows without requiring application restart
- **FR-007**: System MUST preserve font and background selections across theme switches (persisted per Custom theme)
- **FR-008**: System MUST provide a color wheel accent picker with real-time preview
- **FR-009**: System MUST accept hex color code input for accent customization
- **FR-010**: System MUST generate accent variant colors (hover, pressed, glow, border, subtle) from the selected accent
- **FR-011**: System MUST render unified minimize, maximize, restore, and close buttons across all windows with theme-aware hover and pressed states
- **FR-012**: System MUST enforce correct title bar drag region that excludes window control buttons
- **FR-013**: System MUST meet WCAG 2.1 AA contrast requirements (4.5:1 for normal text, 3:1 for large text) in both dark and light themes
- **FR-014**: System MUST ensure accent color consistency between dark, light, and custom themes
- **FR-015**: System MUST provide fallback mechanisms for missing background images, unavailable fonts, and failed icon loads
- **FR-016**: All new theme resources MUST use `{DynamicResource}` for themeable properties
- **FR-017**: All theme mutations MUST route through ThemeManager exclusively
- **FR-018**: All new animations (hover, pressed, theme transitions) MUST complete within 200ms

### Key Entities

- **ThemePreset**: Represents a named theme configuration (Dark, Light, Custom) with associated color tokens, accent color, background settings, font selection, and blur settings
- **BackgroundSettings**: Encapsulates background type (solid/gradient/image/blur), image path, blur intensity, and fallback behavior
- **FontSettings**: Stores selected font family name and associated font preview data (font size scaling is out of scope; deferred to Phase 24 Localization & RTL)
- **AccentVariant**: A computed derivative of the primary accent color (hover, pressed, glow, border, subtle) with HSL/LAB adjustments
- **MaterialIconReference**: Maps a logical icon identifier to the Material Design icon pack, supporting theme-aware coloring

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify all navigation categories by their Material Design icons without reading labels (validated through usability testing with 5+ users)
- **SC-002**: Background image application and blur adjustment produce visible results in under 500ms
- **SC-003**: Font changes propagate to all open windows in under 500ms without visual flicker
- **SC-004**: Both dark and light theme contrast ratios pass WCAG 2.1 AA for all semantic token pairs (4.5:1 normal text, 3:1 large text)
- **SC-005**: Accent color changes appear across all windows within 500ms of selection
- **SC-006**: Window control buttons render identically across all window types (Shell, Settings, dialogs) with no visual discrepancies
- **SC-007**: Theme switching (dark to light, light to custom) completes within 1 second with all customizations preserved
- **SC-008**: Zero application crashes or blank windows when missing resources are encountered (background images, fonts, icons)

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).
- Material Design integration is authorized per ADR-006 for icons, dialogs, and selected controls only (Constitution §XIV).
- Resource dictionaries MUST be added at proper positions in ThemeResources.xaml loading order (Constitution §XV).
- New theme features MUST include fallback recovery paths for all theme-aware resources (Constitution §XVI).

## Assumptions

- The MaterialDesignThemes NuGet package is available and compatible with .NET Framework 4.8 WPF
- System-installed fonts detected via the operating system font enumeration are sufficient for font selection
- Background images will be stored as file references, not embedded in settings files (to avoid bloat)
- Image file formats supported are limited to PNG, JPG, and BMP (common Windows image formats)
- The existing ThemeManager singleton architecture is sufficient to handle new background, font, and accent variant properties
- Blur effect is implemented via Win32 DWM API (SetWindowCompositionAttribute) rather than WPF software rendering for performance
- Window control buttons inherit from the existing ModernWindow title bar implementation
- Arabic font presets (Phase 24 readiness) are stored as named font groups, not individual fonts
- Accent variants are computed using HSL color space transformations maintained by ThemeManager
- Material DialogHost replaces existing MessageBox and custom dialog implementations
