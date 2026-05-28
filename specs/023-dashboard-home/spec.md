# Feature Specification: Dashboard & Home

**Feature Branch**: `023-dashboard-home`

**Created**: 2026-05-28

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 23"

## Clarifications

### Session 2026-05-28

- Q: Can users customize the dashboard (hide/show/reorder/resize widgets)? → A: Fixed layout — all 9 widgets always visible, predefined order, no user customization.
- Q: Does this phase create a new plugin management page when clicking Plugin Status widget? → A: Navigate to existing plugin registry view — no new page created this phase.
- Q: What metrics does the Performance Summary widget track? → A: Startup time + last Shell page navigation duration.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Home Dashboard Landing Page (Priority: P1)

When a planning engineer opens the Shell workspace, they immediately see a Home dashboard that provides a centralized overview of the platform status, recent activity, and quick access to commonly used tools — eliminating the need to navigate through menus to understand the current state of their environment.

**Why this priority**: The Home page is the first interaction point for every session. It sets the tone for the entire platform experience and provides immediate orientation, reducing time-to-productivity for planning engineers who open the tool multiple times per day.

**Independent Test**: Can be fully tested by launching the Shell workspace and verifying the Home page renders with all widget types visible and populated with live data.

**Acceptance Scenarios**:

1. **Given** the Shell workspace is launched, **When** the application finishes loading, **Then** the Home dashboard page is displayed as the default landing page within 1 second.
2. **Given** the Home dashboard is displayed, **When** the user views the page, **Then** all 9 widget types are rendered: Current Version, Latest Updates, Recent Tools, Recent Projects, Diagnostics Summary, AI Provider Status, Performance Summary, Quick Actions, and Plugin Status.
3. **Given** the Home dashboard is displayed, **When** the user clicks a Quick Action button (e.g., "New BOQ Analysis"), **Then** the corresponding feature page opens immediately within the Shell workspace.
4. **Given** the Home dashboard is displayed, **When** the user clicks a Recent Tool entry, **Then** the Shell navigates to that tool's page.

---

### User Story 2 - Diagnostics Dashboard (Priority: P2)

When a planning engineer or support team member needs to understand the current health and configuration of the platform, they can view a Diagnostics dashboard that shows real-time system information including render mode, active theme, DPI scale, memory usage, GPU status, popup diagnostics, and the last error log entry — enabling rapid troubleshooting without external tools.

**Why this priority**: Diagnostics visibility is critical for support workflows and self-service troubleshooting, but it is secondary to the core Home landing page experience. It becomes essential when issues arise but does not block daily usage.

**Independent Test**: Can be fully tested by navigating to the Diagnostics section from the Home page and verifying all diagnostic indicators display live, accurate values that update in real-time.

**Acceptance Scenarios**:

1. **Given** the Diagnostics dashboard is open, **When** the user views the render mode indicator, **Then** it accurately reflects the current WindowRenderMode (FallbackSafe or WindowChrome).
2. **Given** the Diagnostics dashboard is open, **When** memory usage changes, **Then** the memory gauge updates in real-time without requiring a page refresh.
3. **Given** the Diagnostics dashboard is open, **When** the active theme is changed via settings, **Then** the theme indicator updates to reflect the new theme and accent swatch.
4. **Given** an error has occurred in the current session, **When** the user views the Diagnostics dashboard, **Then** the last error log entry is displayed with timestamp and message.

---

### User Story 3 - AI Provider Status Monitoring (Priority: P2)

When a planning engineer is about to use AI-powered features (WBS generation, BOQ activity generation, relationship generation), they can glance at the AI Provider Status widget on the Home dashboard to see which providers are online or offline and how many tokens have been consumed in the current session — preventing failed AI operations and enabling informed provider selection.

**Why this priority**: AI features are a core differentiator of the platform. Knowing provider availability before initiating an AI operation prevents wasted time and frustration, but this is secondary to the core dashboard experience.

**Independent Test**: Can be fully tested by viewing the AI Provider Status widget on the Home page and verifying it shows online/offline status for each configured provider along with session token usage.

**Acceptance Scenarios**:

1. **Given** the Home dashboard is displayed, **When** an AI provider is reachable, **Then** its status indicator shows "Online" with a visual green state.
2. **Given** the Home dashboard is displayed, **When** an AI provider is unreachable or not configured, **Then** its status indicator shows "Offline" with a visual red or gray state.
3. **Given** the user has executed AI operations in the current session, **When** they view the AI Provider Status widget, **Then** the total token usage for the session is displayed.
4. **Given** no AI providers are configured, **When** the Home dashboard is displayed, **Then** the AI Provider Status widget shows a clear message indicating no providers are configured with a link to settings.

---

### User Story 4 - Plugin Status Overview (Priority: P3)

When a planning engineer or platform administrator needs to verify that all expected plugins are loaded and healthy, they can view the Plugin Status widget on the Home dashboard showing the count of loaded plugins and their health status — providing confidence that all planning tools are available before starting work.

**Why this priority**: Plugin health awareness is valuable but not a daily interaction for most users. It supports power users and administrators who need platform visibility, making it a lower priority than the core dashboard and diagnostics.

**Independent Test**: Can be fully tested by viewing the Plugin Status widget and verifying it accurately reflects the number of loaded plugins and their health states (healthy, degraded, unhealthy).

**Acceptance Scenarios**:

1. **Given** the Home dashboard is displayed, **When** all plugins are loaded and healthy, **Then** the Plugin Status widget shows the total count with a healthy indicator.
2. **Given** a plugin has failed to load or is in a degraded state, **When** the user views the Plugin Status widget, **Then** the unhealthy or degraded plugin count is highlighted distinctly.
3. **Given** the Home dashboard is displayed, **When** the user clicks the Plugin Status widget, **Then** the Shell navigates to the existing plugin registry view (no new page is created in this phase).

---

### Edge Cases

- What happens when the Shell workspace opens but one or more widget data sources are unavailable (e.g., AI provider health check times out)? The affected widget displays a graceful degraded state with a "Data unavailable" message rather than blocking the entire page.
- What happens when no recent tools or recent projects exist (first-time user)? The Recent Tools and Recent Projects widgets display an empty state with helpful guidance text (e.g., "Your recently used tools will appear here").
- How does the system handle the Home dashboard when running in Excel VSTO FallbackSafe render mode? All widgets render correctly without GPU-dependent effects; animations and glow effects are suppressed per Excel Rendering Safety rules.
- What happens when the user rapidly switches between the Home page and other Shell pages? The Home page state is preserved and widgets refresh data on re-entry without causing memory leaks or stale data.
- How does the dashboard handle high-DPI displays and multi-monitor setups? All widget layouts scale correctly using existing DPI-aware infrastructure; no clipping or overflow occurs.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the Home dashboard as the default landing page when the Shell workspace opens.
- **FR-002**: System MUST render a Current Version widget showing application version, .NET runtime version, Excel version, and Windows version.
- **FR-003**: System MUST render a Latest Updates widget displaying changelog information from the most recent release.
- **FR-004**: System MUST render a Recent Tools widget showing the last 5 used features with quick-launch capability.
- **FR-005**: System MUST render a Recent Projects widget showing the last opened Excel files or projects.
- **FR-006**: System MUST render a Diagnostics Summary widget showing render mode, GPU acceleration status, active theme, and memory usage.
- **FR-007**: System MUST render an AI Provider Status widget showing online/offline state per configured provider and session token usage.
- **FR-008**: System MUST render a Performance Summary widget showing Shell startup time and the duration of the most recent Shell page navigation.
- **FR-009**: System MUST render a Quick Actions widget with buttons for common operations (New BOQ Analysis, Generate WBS, Open Settings).
- **FR-010**: System MUST render a Plugin Status widget showing loaded plugin count and health state (healthy, degraded, unhealthy).
- **FR-011**: System MUST provide a dedicated Diagnostics dashboard page accessible from the Home page showing: render mode indicator, active theme with accent swatch, current DPI scale, memory usage gauge, GPU acceleration status, popup diagnostics panel, and last error log entry.
- **FR-012**: System MUST update the Diagnostics dashboard metrics in real-time without requiring manual refresh.
- **FR-013**: System MUST persist the Recent Tools and Recent Projects lists across application sessions.
- **FR-014**: System MUST navigate to the correct Shell page when a Quick Action button or Recent Tool entry is clicked.
- **FR-015**: System MUST display graceful degraded states for widgets whose data sources are unavailable.
- **FR-016**: System MUST render the Home dashboard within 1 second of Shell workspace launch.
- **FR-017**: System MUST register the Home page in the Shell sidebar navigation under a prominent position (first item or dedicated "Home" category).
- **FR-018**: System MUST render all 9 widgets in a fixed, predefined order with no user customization (hide, show, reorder, or resize) — all widgets are always visible.

### Key Entities

- **DashboardWidget**: Represents a single widget on the Home dashboard. Key attributes: WidgetType (enum of 9 types), Title, DataProvider, RefreshInterval, IsVisible, DisplayOrder.
- **RecentToolEntry**: Represents a recently used tool. Key attributes: ToolId, ToolName, LastUsedTimestamp, NavigationTarget, IconIdentifier.
- **RecentProjectEntry**: Represents a recently opened project. Key attributes: FilePath, ProjectName, LastOpenedTimestamp, FileSize.
- **AIProviderStatus**: Represents the health state of an AI provider. Key attributes: ProviderId, ProviderName, IsOnline, LastCheckedTimestamp, SessionTokenUsage.
- **PluginHealthStatus**: Represents the loaded state and health of a plugin. Key attributes: PluginId, PluginName, IsLoaded, HealthState (Healthy/Degraded/Unhealthy), LastError.
- **DiagnosticsSnapshot**: Represents a point-in-time collection of diagnostic data. Key attributes: RenderMode, ActiveTheme, AccentColor, DPIScale, MemoryUsageMB, IsGPUAccelerated, LastErrorEntry.
- **PerformanceSummary**: Represents performance metrics for the Performance Summary widget. Key attributes: ShellStartupTimeMs, LastPageNavigationTimeMs, LastPageNavigationTarget.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users see the Home dashboard within 1 second of launching the Shell workspace.
- **SC-002**: All 9 widget types render correctly and display live data on first view.
- **SC-003**: Users can navigate to any feature from the Home dashboard in a single click (Quick Actions or Recent Tools).
- **SC-004**: Diagnostics dashboard reflects system state changes (theme switch, memory fluctuation) within 2 seconds of the change occurring.
- **SC-005**: Recent Tools and Recent Projects lists persist correctly across application restarts and display up to 5 entries each.
- **SC-006**: Plugin Status widget accurately reflects loaded/unloaded/healthy/unhealthy plugin counts matching the actual plugin registry state.
- **SC-007**: AI Provider Status widget correctly identifies online and offline providers with a health check accuracy above 95%.
- **SC-008**: Home dashboard renders correctly in both WindowChrome and FallbackSafe (Excel VSTO) render modes without visual degradation.
- **SC-009**: Dashboard layout remains fully functional across DPI scales from 100% to 200% without clipping or overflow.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).
- All new features MUST be implemented as Pages within the Shell workspace — no standalone windows (Constitution §XI, ADR-001).
- All new UI MUST follow MVVM patterns with ViewModels in the `ViewModels/` directory (Constitution §XIII).

## Assumptions

- The Shell workspace navigation infrastructure (from Phase 15) supports registering and navigating to the Home page as the default landing page.
- The Dynamic Settings Platform (from Phase 16) provides the necessary APIs for widgets to read configuration values (theme, render mode, etc.).
- The AI Core Infrastructure (from Phase 18) exposes provider health check endpoints and token usage tracking APIs that the AI Provider Status widget can consume.
- The Plugin SDK (from Phase 14) exposes a plugin registry API that provides loaded plugin count and health state information.
- The DiagnosticsService (existing) provides APIs for render mode detection, memory usage, GPU status, and error log access.
- The Home page will be implemented as a WPF Page within the existing Shell navigation framework, not as a standalone window.
- Widget data refresh will use existing EventBus subscriptions where applicable (e.g., ThemeChanged, PluginLoaded events) rather than polling.
- The Latest Updates widget will read from a local changelog file or embedded resource; it does not require network access to fetch release notes.
- Recent Tools and Recent Projects persistence will use the existing AppData/Som3a storage pattern (JSON file) consistent with other settings persistence.
