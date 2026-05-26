# Implementation Plan: Shell Refactor

**Branch**: `015-shell-refactor` | **Date**: 2026-05-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/015-shell-refactor/spec.md`

## Summary

Refactor the Shell sidebar from a flat list into categorized navigation groups (Planning, Analysis, Excel, AI, Settings), implement dynamic `[NavigationItem]` attribute discovery from the Plugin SDK (Som3a.Contracts), add sidebar collapse/expand with auto-hide icon strip, full keyboard navigation (arrow keys + skip link), and unsaved-changes warnings. Register all 13 existing Pages into the sidebar and remove all 12 legacy standalone Window classes, replacing them with Shell-hosted Page equivalents. The Shell becomes the sole entry point for all add-in features.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for WpfApp2 WPF host; .NET 8.0 for Som3a.Contracts reference via Som3a.Bridge)

**Primary Dependencies**:
- **Existing**: ModernWindow, ShellWindow, SidebarControl, WorkspaceHost, NavigationService, ThemeManager, WindowRenderModeDetector, ModuleRegistry, ServiceContainer, EventBus
- **From Phase 14**: Som3a.Contracts (`INavigationItemAttribute` or `[NavigationItem]` attribute), Som3a.Plugin.SDK (`AssemblyScanner`, discovery)
- **Constants**: MaterialDesignThemes (icons per ADR-006), ShellStyles.xaml (existing sidebar/workspace styles)
- **Architecture**: Categorized SidebarControl, NavigationDestination with Category property, ShellState tracking

**Storage**: N/A — no new persistence. Navigation registrations are in-memory per session.

**Testing**: Manual Excel VSTO host test (Constitution X compliance), regression tests for all 14 migrated pages, keyboard-only navigation walkthrough, collapse/expand state verification, unsaved-changes dialog testing

**Target Platform**: Windows (x64) — Excel VSTO Add-in host (.NET Framework 4.8)

**Project Type**: WPF VSTO Add-in (desktop application hosted inside Excel)

**Performance Goals**: 
- Page navigation within 1 second (SC-003)
- Sidebar tree rebuild within 500ms for 50+ items (SC-004)
- Sidebar expand/collapse animation ≤200ms (Constitution IX)
- No blocking of Excel UI thread during navigation

**Constraints**:
- All sidebar UI MUST use `{DynamicResource}` for themeable properties (Constitution III)
- Sidebar animation ≤200ms (Constitution IX)
- Must not break Excel VSTO rendering; WindowRenderModeDetector respected (Constitution X)
- No new standalone windows; all features via Shell Pages (Constitution XI)
- No inline DropShadowEffect; effects from Effects/Shadows.xaml (Constitution XII)
- Material Design icons only (ADR-006); no other third-party frameworks (Constitution XIV)

**Scale/Scope**:
- 13 existing Pages to register as navigation items across 5 categories
- 12 legacy Window classes to deprecate/remove
- ~5 existing panel UserControls (Views/*) to migrate into their respective Settings pages
- Support 50+ dynamically registered items from plugins
- 6 key source files modified: SidebarControl, ShellWindow, NavigationService, NavigationDestination, ShellState, WorkspaceHost

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Library-First Modular Architecture** — Sidebar control, navigation categories, and workspace host remain isolated in `Controls/Shell/` directory. No monolithic changes. Feature adds Category property to existing NavigationDestination model and refactors SidebarControl template — no new ResourceDictionary libraries introduced.
- [x] **III. DynamicResource-Only** — All sidebar styling is in ShellStyles.xaml using `{DynamicResource}` tokens. No StaticResource introduced for themeable properties.
- [x] **IV. Runtime Theme Mutation Governance** — Navigation destinations and sidebar rendering do not mutate brushes directly. Theme changes route through ThemeManager as before. Sidebar responds to ThemeChanged event for re-rendering.
- [x] **IX. Animation Governance** — Sidebar expand/collapse uses opacity + width animation ≤200ms. Keyboard focus transitions are instantaneous (no animation). All animations GPU-safe (opacity only, no layout thrashing).
- [x] **X. Excel Rendering Safety** — ShellWindow already uses WindowRenderModeDetector. Sidebar collapse uses `Opacity` animation (safe in fallback mode). No new transparency layers introduced. WorkspaceHost error overlay reuses existing safe patterns.
- [x] **XI. WindowChrome Enforcement** — No new windows. All features accessed through existing ShellWindow (ModernWindow). Legacy windows are removed, not replaced with new ones. ShellWindow remains the sole top-level window.
- [x] **XII. Centralized Effects** — Sidebar selection highlights use existing `{DynamicResource}` effect tokens from Effects/Shadows.xaml and Effects/Glow.xaml. No inline effects added.
- [x] **XV. Resource Loading Order** — No new resource dictionaries added. ShellStyles.xaml already loaded at the correct position in ThemeResources.xaml.

## Project Structure

### Documentation (this feature)

```text
specs/015-shell-refactor/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── navigation-contracts.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
WpfApp2/                                  # .NET Framework 4.8 VSTO + WPF host
├── Controls/Shell/
│   ├── ShellWindow.xaml                  # MODIFY: Add collapse toggle button to layout
│   ├── ShellWindow.xaml.cs               # MODIFY: Bind collapse state, wire keyboard nav
│   ├── SidebarControl.xaml               # MODIFY: Refactor to categorized ListBox/ItemsControl
│   ├── SidebarControl.xaml.cs            # MODIFY: Add category grouping, collapse logic, arrow key nav
│   ├── WorkspaceHost.cs                  # MODIFY: Add unsaved-changes detection + prompt
│   ├── NavigationDestination.cs          # MODIFY: Add Category, ItemId, IsEnabled properties
│   ├── NavigationPage.cs                 # MODIFY: Add Category field
│   ├── NavigationEventArgs.cs            # MODIFY: Add cancellation support
│   ├── ShellState.cs                     # MODIFY: Track collapse state, last active page
│   ├── CommandPalette.xaml               # (unchanged)
│   └── CommandPalette.xaml.cs            # (unchanged)
│
├── Services/
│   ├── NavigationService.cs              # MODIFY: Add RegisterCategory(), categorized search
│   ├── NavigationRegistrar.cs            # MODIFY: Support category-aware registration
│   ├── ShellNavigationHelper.cs          # MODIFY: Route all entry points through Shell
│   ├── SidebarRegistrationService.cs     # NEW: Discovers [NavigationItem] attributes via Plugin SDK
│   └── PageStateTracker.cs               # NEW: Tracks dirty/unsaved state per page
│
├── Pages/
│   ├── PageBase.cs                       # MODIFY: Add IsDirty property, ISupportsDirtyTracking interface
│   ├── [13 existing pages]               # (aligned to correct categories, no structural changes)
│
├── Views/
│   └── SettingsWindow.xaml/.cs           # MODIFY: Convert to Page (SettingsPage already exists, wire panels)
│
├── Theme/
│   └── ShellStyles.xaml                  # MODIFY: Add category header style, collapse state styles
│
├── [12 legacy Window files]              # REMOVE (code deleted, references purged)
│   ├── MainWindow.xaml/.cs               # REMOVE
│   ├── AssignTradeCodesWindow.xaml/.cs   # REMOVE
│   ├── Fixpiecolors.xaml/.cs             # REMOVE
│   ├── LinksManagerWindow.xaml/.cs       # REMOVE
│   ├── SubDailyReportWindow.xaml/.cs     # REMOVE
│   ├── StyleSelectorWindow.xaml/.cs      # REMOVE
│   ├── UnmergeFillDownWindow.xaml/.cs    # REMOVE
│   ├── XerEditorWindow.xaml/.cs          # REMOVE
│   ├── UI/ProjectAnalysisWindow.xaml/.cs # REMOVE
│   ├── Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml/.cs # REMOVE
│   └── Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml/.cs # REMOVE
│
├── Som3a.Contracts/                      # .NET 8.0 (from Phase 14)
│   └── INavigationItemAttribute.cs       # NEW: Attribute interface for dynamic registration
│
└── Som3a.Bridge/                         # .NET Standard 2.0 interop
    └── NavigationContractsBridge.cs      # NEW: Bridge for .NET 8.0 ←→ .NET Framework 4.8

docs/                                     # (unchanged)
```

**Structure Decision**: Single project (WpfApp2) with modifications. Sidebar controls are enhanced in-place within `Controls/Shell/`. The `[NavigationItem]` attribute definition lives in `Som3a.Contracts` (Phase 14 deliverable) and is consumed via the existing `Som3a.Bridge` interop layer. Two new services (`SidebarRegistrationService`, `PageStateTracker`) are added to `Services/`. Legacy Window files are deleted.

## Complexity Tracking

> No constitutional violations to justify. All gates pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
