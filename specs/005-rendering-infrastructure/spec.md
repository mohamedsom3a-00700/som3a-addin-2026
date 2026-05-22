# Feature Specification: Rendering & Window Infrastructure

**Feature Branch**: `005-rendering-infrastructure`

**Created**: 2026-05-22

**Status**: Draft

**Input**: Phase 2 from implementation_plan.md — Create Excel-safe rendering architecture. Extend existing ModernWindow, WindowRenderModeDetector with new services. Audit all windows. Standardize shadows, DPI handling, accessibility, and window animations. Validate Excel host stability.

## User Scenarios & Testing

### User Story 1 - Excel-Safe Window Rendering (Priority: P1)

As an Excel VSTO user, I want every window in the add-in to render correctly within the Excel host environment — no black windows, no transparency artifacts, no ComboBox popup issues — so I can work without visual glitches or application freezes.

**Why this priority**: Excel VSTO hosting is the primary deployment environment. If windows fail to render inside Excel, the entire add-in is unusable regardless of other features.

**Independent Test**: Open each window from the Excel VSTO host, verify correct rendering (no black windows, no artifacts), and confirm the window can be moved, minimized, and closed without Excel freezing.

**Acceptance Scenarios**:

1. **Given** the add-in is loaded in Excel, **When** any window is opened from a ribbon button, **Then** the window renders with correct backgrounds and borders — no black rectangles or transparency artifacts.
2. **Given** an Excel VSTO host with limited GPU support, **When** a window opens, **Then** the system automatically detects FallbackSafe mode and disables animations, effects, and transparency that would cause rendering failures.
3. **Given** a window with a ComboBox control, **When** the user opens the dropdown, **Then** the popup renders fully visible with correct shadows and no clipping.
4. **Given** a window opened inside Excel, **When** the user moves, minimizes, maximizes, or closes the window, **Then** Excel remains responsive and does not freeze.

---

### User Story 2 - Unified Shadow & Effect System (Priority: P1)

As a developer, I want a single centralized shadow system with safe-mode fallback variants so that every window and popup uses consistent shadows that degrade gracefully when GPU-accelerated effects cause rendering problems in Excel.

**Why this priority**: Inconsistent or broken shadows are the most visible rendering defect in VSTO-hosted WPF applications. A unified system ensures all windows look consistent and have a safe fallback path.

**Independent Test**: All open windows and popups use shadow effect keys from `Effects/Shadows.xaml` only — grep for `<DropShadowEffect` outside that file returns zero results. Safe-mode variants exist and are applied when `RenderModeService` reports FallbackSafe.

**Acceptance Scenarios**:

1. **Given** the shadow system, **When** a developer inspects any control template or window style that uses shadows, **Then** all references use `{DynamicResource Shadow.*}` keys from the centralized `Shadows.xaml`.
2. **Given** FallbackSafe render mode is active, **When** a window or popup displays, **Then** it uses `Shadow.*.Safe` variants with reduced blur and opacity instead of full-effect shadows.
3. **Given** WindowChrome render mode is active, **When** a window displays, **Then** it uses full-resolution shadows with normal blur and opacity.

---

### User Story 3 - DPI-Aware Window Scaling (Priority: P2)

As a user with a high-DPI display, I want all windows to scale correctly at 100%, 125%, and 150% DPI settings so that the UI remains readable and properly proportioned regardless of my monitor configuration.

**Why this priority**: Many enterprise users work on high-DPI laptops or multi-monitor setups with mixed DPI scales. Windows that don't scale correctly become unusable at non-standard DPI settings.

**Independent Test**: Open every window at 100%, 125%, and 150% DPI scaling. Verify all text, controls, and layout are proportionally scaled and no content is clipped or overlapped.

**Acceptance Scenarios**:

1. **Given** the system DPI is set to 125%, **When** any window opens, **Then** all text, controls, and spacing scale proportionally without clipping or overlapping.
2. **Given** the system DPI is set to 150%, **When** any window opens, **Then** all layout elements remain within their containers and are fully visible.
3. **Given** the DPI helper utility, **When** a developer calls `ScaleValue(value)`, **Then** the returned value is correctly scaled by the current DPI factor.

---

### User Story 4 - Keyboard & Accessibility Support (Priority: P3)

As a keyboard-only user, I want all windows and controls to be fully navigable via keyboard with visible focus indicators so that I can operate the add-in without a mouse.

**Why this priority**: Accessibility is an enterprise requirement. Keyboard navigation and screen reader preparation ensure the add-in meets workplace accessibility standards.

**Independent Test**: Navigate through every window using Tab, Enter, Escape, and arrow keys. Verify focus indicators are visible and all interactive elements are reachable.

**Acceptance Scenarios**:

1. **Given** any open window, **When** the user presses Tab repeatedly, **Then** focus moves through all interactive controls in logical order and each focused control shows a visible focus indicator.
2. **Given** a Button, ComboBox, or CheckBox, **When** the user presses Enter or Space, **Then** the control activates as expected.
3. **Given** any window with a close button, **When** the user presses Escape, **Then** the window closes.

---

### Edge Cases

- What happens when the VSTO host has no GPU (software rendering only)?
- How does the system handle DPI changes while a window is already open?
- What happens if a user rapidly opens and closes windows — does Excel become unresponsive?
- How does the system behave when transparency is not supported by the rendering pipeline?
- What happens when a safe-mode window is moved to a monitor with different DPI scaling?
- How does the system recover from a failed animation or effect in Excel host?
- How does the system handle RenderModeService initialization failure — defaults to FallbackSafe?

## Requirements

### Functional Requirements

- **FR-001**: System MUST audit all 14 window files and document their current rendering configuration (ModernWindow inheritance, AllowsTransparency, inline effects, SnapsToDevicePixels, UseLayoutRounding).
- **FR-002**: ModernWindow MUST expose the following new dependency properties: `RenderMode` (current mode), `IsSafeMode` (read-only), `WindowBackdrop` (solid/gradient/image), and `DpiScale` (current DPI scale).
- **FR-003**: The shadow system MUST define safe-mode variants (`Shadow.Window.Safe`, `Shadow.Card.Safe`, `Shadow.Popup.Safe`) with reduced blur and opacity for FallbackSafe rendering.
- **FR-004**: RenderModeService MUST detect and cache the current render mode (FallbackSafe or WindowChrome) once at app startup (fixed for the session), check GPU availability and transparency support, and expose a `RenderModeChanged` event emitted once at startup or when initialization re-runs (not as a runtime change signal). If RenderModeService fails to initialize, the system MUST default to FallbackSafe mode.
- **FR-005**: When FallbackSafe mode is active, the system MUST disable fade animations, use `Shadow.*.Safe` variants, disable backdrop effects, and force `AllowsTransparency="False"`.
- **FR-006**: Every window MUST inherit from ModernWindow, use `{DynamicResource Brush.Background.Root}` for backgrounds, `{DynamicResource CardStrokeBrush}` for borders, and have `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"`.
- **FR-007**: DPI helper MUST provide `GetCurrentDpiScale()` (returns current scale factor), `ScaleValue(double)` (scales by current DPI), and `IsHighDpi()` (true when scale >= 1.5).
- **FR-008**: Every interactive control MUST have `FocusVisualStyle` using `Glow.Focus`, `AutomationProperties.Name` set, keyboard navigation (Tab order, Enter/Space activation), and high contrast awareness.
- **FR-009**: Window open animations MUST complete within 200ms and window close animations within 150ms, using `Motion.*` tokens for durations. Safe mode MUST skip all animations.
- **FR-010**: System MUST produce an Excel stability test checklist documenting manual test procedures for window opening, DPI scaling, popup rendering, theme switching, and safe mode activation within the Excel VSTO host.

### Key Entities

- **Window**: Any `.xaml` window file in the project that renders UI content within the Excel VSTO host. Each window has a render mode, DPI scale, backdrop configuration, and accessibility properties.
- **RenderMode**: The current rendering strategy — either `WindowChrome` (full effects, animations, shadows) or `FallbackSafe` (reduced effects, no animations, safe shadows). Determined once at app startup by GPU availability and transparency support, fixed for the session.
- **Shadow Variant**: A set of DropShadowEffect definitions organized by render mode. Full variants (`Shadow.Window`, `Shadow.Card`, `Shadow.Popup`) for WindowChrome mode and safe variants (`Shadow.Window.Safe`, `Shadow.Card.Safe`, `Shadow.Popup.Safe`) for FallbackSafe mode.
- **DPI Scale**: The current display scaling factor (1.0 = 100%, 1.25 = 125%, 1.5 = 150%). Used by the DPI helper to scale layout values and by ModernWindow for DPI-aware rendering.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All 14 window files are audited and documented in a window audit report with findings and remediation status.
- **SC-002**: ModernWindow v2 exposes 4 new dependency properties (RenderMode, IsSafeMode, WindowBackdrop, DpiScale) with correct default values matching current behavior.
- **SC-003**: Zero inline `DropShadowEffect` definitions exist outside `Effects/Shadows.xaml` and `Effects/Glow.xaml` after standardization.
- **SC-004**: RenderModeService correctly identifies FallbackSafe vs WindowChrome mode and fires `RenderModeChanged` when the mode changes.
- **SC-005**: All windows open, render, and close correctly inside Excel VSTO host at 100%, 125%, and 150% DPI without black windows, transparency artifacts, popup clipping, or Excel freezes.
- **SC-006**: Every interactive control in every window is reachable via keyboard Tab navigation and shows a visible focus indicator.
- **SC-007**: Window open animations complete within 200ms and close animations within 150ms, measured from trigger to completion.
- **SC-008**: Excel stability test checklist is created and documents at least 8 manual test scenarios covering window opening, DPI scaling, popup rendering, theme switching, and safe mode activation.

## Clarifications

### Session 2026-05-22

- Q: When RenderModeService fails to initialize, what should the system do by default? → A: Default to FallbackSafe on any failure. Safety-first: degraded visuals are acceptable, broken rendering is not.
- Q: When can render mode change mid-session? → A: Determine once at app startup, fixed for session. In VSTO, GPU/transparency don't change without restart. RenderModeChanged event reserved for diagnostics.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- The 14 window files identified in the implementation plan audit scope are the complete set of windows requiring review.
- Existing ModernWindow, WindowRenderModeDetector, and ThemeManager implementations are functional and will be extended, not replaced.
- All windows are expected to eventually inherit from ModernWindow; windows that do not currently inherit are documented for Phase 11 migration.
- DPI helper already exists in the codebase and requires extension rather than creation from scratch.
- Window animations are already defined in WindowAnimations.xaml and require standardization (duration, safe mode gating) rather than creation.
- The FallbackSafe render mode is triggered by GPU unavailability or lack of transparency support — detected by the existing WindowRenderModeDetector once at app startup.
- Render mode is determined once at app startup and fixed for the session; no mid-session transitions are expected in VSTO hosting.
- Safe-mode shadow variants use reduced blur radius (50% of normal) and reduced opacity (75% of normal).
- If RenderModeService fails to initialize, the system defaults to FallbackSafe mode as a safety measure.
