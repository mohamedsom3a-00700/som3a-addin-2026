# Feature Specification: Full Platform Rebranding & Visual Identity System

**Feature Branch**: `026-platform-rebranding`

**Created**: 2026-05-29

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 25"

## Clarifications

### Session 2026-05-29

- Q: Splash animation vs Animation Governance → A: Splash is exempt from 200ms rule; treated as a startup sequence, can run up to 3s.
- Q: Accessibility compliance level for rebranded colors → A: WCAG 2.1 AA (4.5:1 contrast) with formal contrast validation during this phase.
- Q: Font fallback strategy when font files are missing → A: Font stack with fallback chain (e.g., Cairo → Arial → System UI for Arabic; Inter → Segoe UI → System UI for English).
- Q: Theme switching performance target with new effects → A: 1 second max for full theme switch (matching SC-003 and master plan goal).
- Q: Brand asset creation responsibility and timing → A: Assets are a prerequisite (external designer delivers logos, master reference first); this phase implements infrastructure that consumes them.

## User Scenarios & Testing

### User Story 1 - User Sees New Planova Branding on Startup (Priority: P1)

As a planning engineer, when I launch the platform, I see a new animated splash screen with the Planova logo, brand colors, and engineering-themed animation (blueprint lines, building formation, logo reveal). The platform title and window metadata display "Planova Platform" instead of "Som3a Addin".

**Why this priority**: First impression of rebranding is the startup experience. Without the splash screen and window identity, users would not perceive the platform transformation.

**Independent Test**: Can be fully tested by launching the application and verifying the splash screen renders with Planova branding, the window title shows "Planova Platform", and the animation plays correctly.

**Acceptance Scenarios**:

1. **Given** the platform is installed, **When** the user launches the application, **Then** an animated splash screen appears with the Planova logo, blueprint animation, and glow effects
2. **Given** the splash screen completes, **When** the main window opens, **Then** the title bar reads "Planova Platform"
3. **Given** the platform is running, **When** the user checks the system tray or taskbar, **Then** the Planova icon is displayed

---

### User Story 2 - User Navigates Rebranded Shell with New Visual Identity (Priority: P1)

As a planning engineer, I see the Planova brand identity consistently across the entire shell — sidebar with Planova logo, updated brand color palette (dark engineering surfaces with neon cyan highlights, light engineering-white surfaces with blueprint accents), and rebranded ribbon icons with unified engineering-style iconography.

**Why this priority**: The shell and sidebar are the persistent workspace. All user interaction happens within this environment, so brand consistency here is the core of the rebranding.

**Independent Test**: Can be fully tested by opening the shell and verifying the sidebar shows the Planova logo, all theme colors match the new brand palette, and ribbon icons display the new engineering-style icons.

**Acceptance Scenarios**:

1. **Given** the shell is open, **When** the user views the sidebar, **Then** the Planova logo is displayed at the top with a mini animated icon
2. **Given** the dark theme is active, **When** the user inspects any surface, **Then** colors match the dark brand palette (Background #0E1720, Surface #13202B, Accent Blue #2D9CFF, Accent Cyan #00D1FF, Accent Orange #FF8A3D)
3. **Given** the light theme is active, **When** the user inspects any surface, **Then** colors match the light brand palette (Background #F5F7FA, Surface #FFFFFF, Accent Blue #2D9CFF, Accent Cyan #00B8E6, Accent Orange #FF8A3D)
4. **Given** the user browses ribbon tools, **When** they view any icon, **Then** all ribbon icons follow the unified engineering-style visual language

---

### User Story 3 - User Benefits from New Typography System (Priority: P2)

As a planning engineer working in English or Arabic, I can choose from new typography presets (Inter, Segoe UI Variable, Cairo, IBM Plex Sans Arabic, Tajawal) and switch between them dynamically. The fonts render correctly with proper shaping, ligatures, and RTL support.

**Why this priority**: Typography directly impacts readability and professional appearance but is secondary to overall brand identity being visible.

**Independent Test**: Can be fully tested by opening settings, selecting different font presets, and verifying the shell UI updates immediately with correct rendering.

**Acceptance Scenarios**:

1. **Given** the user opens appearance settings, **When** they select an English font preset (Inter, Segoe UI Variable), **Then** the entire shell UI updates to the selected font family
2. **Given** the user opens appearance settings, **When** they select an Arabic font preset (Cairo, IBM Plex Sans Arabic, Tajawal), **Then** Arabic text renders with correct shaping and ligatures
3. **Given** the user switches fonts, **When** the UI updates, **Then** no text overflow, clipping, or rendering artifacts occur

---

### User Story 4 - Administrator Accesses Brand Assets for Enterprise Deployment (Priority: P3)

As an IT administrator or enterprise stakeholder, I can find all brand assets (logos in SVG, PNG, ICO formats; brand color specifications; font files) organized in a dedicated Assets/Branding/ folder structure, enabling consistent use in documentation, presentations, and installer packaging.

**Why this priority**: Enterprise deployments require proper brand assets for documentation and installer, but this is a secondary concern to the in-app experience.

**Independent Test**: Can be fully tested by navigating to the Assets/Branding/ directory and verifying the complete folder structure with all required logo formats, color specifications, and font files.

**Acceptance Scenarios**:

1. **Given** the installation directory, **When** the user navigates to Assets/Branding/Logos/, **Then** they find logo files in SVG, PNG (64x64, 128x128, 256x256, 512x512, 1024x1024), and ICO formats
2. **Given** the Assets/Branding/ directory, **When** the user looks for brand specifications, **Then** they find the dark and light theme color palettes documented
3. **Given** the Assets/Branding/ directory, **When** the user looks for font files, **Then** they find the supported font families listed

---

### Edge Cases

- What happens when the animated splash screen encounters a rendering limitation (e.g., low-end GPU, VSTO host constraints)? — Splash falls back to a static Planova logo display without animation.
- How does the system handle missing brand asset files? — Each asset reference falls back to a default or placeholder; missing assets are logged but do not block application startup.
- How does font switching interact with RTL language mode? — Arabic fonts automatically apply correct shaping and bidirectional text handling when RTL mode is active.
- What if namespace migration is attempted before stabilization? — Namespace migration is explicitly scoped for post-stabilization only; no renaming occurs until architecture freeze.
- What happens when a selected font file is missing or corrupted? — The system falls back through the defined font stack chain (e.g., Cairo → Arial → System UI) and logs the failure. No error dialog is shown to the user.

## Requirements

### Functional Requirements

- **FR-001**: System MUST display an animated splash screen on startup with Planova logo, blueprint line animation, building formation, logo reveal, and glow effects
- **FR-002**: System MUST show "Planova Platform" in the main window title bar, taskbar, and system tray
- **FR-003**: System MUST display the Planova logo in the sidebar at the top with a mini animated icon
- **FR-004**: System MUST include a branding footer in the sidebar
- **FR-005**: Dark theme MUST use the official brand palette: Background #0E1720, Surface #13202B, Border #243647, Accent Blue #2D9CFF, Accent Cyan #00D1FF, Accent Orange #FF8A3D, Text Primary #FFFFFF, Text Secondary #B7C5D3
- **FR-006**: Light theme MUST use the official brand palette: Background #F5F7FA, Surface #FFFFFF, Border #D6E2EE, Accent Blue #2D9CFF, Accent Cyan #00B8E6, Accent Orange #FF8A3D, Text Primary #102030, Text Secondary #5B7186
- **FR-007**: Dark theme MUST feature neon cyan highlights, blueprint-inspired overlays, deep dark surfaces, and glass effects
- **FR-008**: Light theme MUST feature engineering-white style, soft gray surfaces, blueprint-inspired accents, and professional contrast
- **FR-009**: Custom theme MUST support background images, blur intensity, accent override, font selection, and font preview thumbnails
- **FR-010**: All ribbon icons MUST be rebranded to a unified engineering-style gradient icon language compatible with both dark and light themes
- **FR-011**: Users MUST be able to choose English font presets (Inter, Segoe UI Variable) from appearance settings with immediate UI update. Each preset MUST have a defined fallback chain (e.g., Inter → Segoe UI → System UI) for resilience.
- **FR-012**: Users MUST be able to choose Arabic font presets (Cairo, IBM Plex Sans Arabic, Tajawal) from appearance settings with immediate UI update. Each preset MUST have a defined fallback chain (e.g., Cairo → Arial → System UI) for resilience.
- **FR-013**: Font switching MUST support RTL text with correct shaping, ligatures, and bidirectional handling
- **FR-014**: Platform MUST include a complete Assets/Branding/ folder structure with Logos (SVG, PNG, ICO, Transparent, Dark, Light, Monochrome), Ribbon, Splash, Wallpapers, Icons, Theme, and Fonts subdirectories
- **FR-015**: Logo assets MUST be provided in SVG, PNG (64x64 through 1024x1024), and ICO formats
- **FR-016**: A Master Brand Reference file MUST be stored at Assets/Branding/Master/planova-master-brand-reference.png as the official visual source of truth
- **FR-017**: Namespace migration (Som3a.* → Planova.*) MUST be deferred until after feature stabilization, plugin stabilization, and architecture freeze
- **FR-018**: Home page MUST display a product branding section with version information and release notes card
- **FR-019**: Solution, project, assembly, and package names MUST be renamed from Som3a to Planova only after the stabilization gate
- **FR-020**: EXE metadata (description, company, copyright, icon) MUST reflect Planova Platform identity in production builds
- **FR-021**: All text and background color combinations in both dark and light brand palettes MUST pass WCAG 2.1 AA contrast ratios (4.5:1 for normal text, 3:1 for large text)

### Key Entities

- **Brand Assets**: Logo files in multiple formats (SVG, PNG, ICO), organized by variant (transparent, dark, light, monochrome) and use case (ribbon, splash, sidebar)
- **Brand Theme Token**: Named color values extracted from the master branding (PrimaryBlue, PrimaryCyan, AccentOrange, DarkSurface, BlueprintGlow, GlassBorder, EngineeringWhite) used as global theme tokens
- **Typography Preset**: Named font configuration (Inter, Segoe UI Variable, Cairo, IBM Plex Sans Arabic, Tajawal) with associated font files and RTL support metadata
- **Master Brand Reference**: The authoritative planova-master-brand-reference.png file that defines all visual direction (engineering aesthetic, color direction, UI language)

## Success Criteria

### Measurable Outcomes

- **SC-001**: Splash screen completes within 3 seconds and transitions smoothly to the main shell on all supported hardware
- **SC-002**: All brand palette colors render correctly in both dark and light themes — verified by automated color sampling at 10+ surface and accent locations
- **SC-003**: Font switching takes effect in under 1 second across all shell UI elements
- **SC-004**: All 30+ ribbon icons updated to new engineering-style design — verified by visual audit against master brand reference
- **SC-005**: Assets/Branding/ directory contains all required subdirectories with at least one file each — verified by automated directory scan
- **SC-006**: Logo files exist in all required formats (SVG, PNG at 5 sizes, ICO) for at least the primary logo variant
- **SC-007**: 100% of existing UI functionality continues to work after rebranding — no regressions in theme switching, navigation, or VSTO interop
- **SC-008**: All text/background color combinations in the new brand palettes pass WCAG 2.1 AA contrast ratios (4.5:1 normal text, 3:1 large text) — verified by automated color contrast audit
- **SC-009**: Full theme switch (dark → light, light → dark, or custom → any) completes in under 1 second across all shell windows

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX). Splash screen startup animation is exempt as a loading/startup sequence and may run up to 3s.
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The animated splash screen uses existing animation infrastructure and does not introduce a new animation framework.
- Logo files and the master brand reference image are prerequisites delivered by an external designer. This phase creates the infrastructure (folder structure, theme tokens, splash implementation, ribbon icon system) that consumes them.
- Brand color palette values must pass WCAG 2.1 AA contrast ratio (4.5:1 minimum for normal text, 3:1 for large text). Formal contrast validation is included in this phase.
- Font files are either system-installed (Segoe UI Variable) or loaded from bundled TTF/WOFF files in Assets/Branding/Fonts/.
- Namespace migration (FR-017) is explicitly deferred; this phase only prepares the visual and asset foundation.
- Existing shell, theme engine, and ribbon infrastructure is reused and extended — no new UI framework is introduced.
