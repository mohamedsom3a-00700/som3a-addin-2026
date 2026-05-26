# Research: Shell Refactor

**Feature**: 015-shell-refactor
**Date**: 2026-05-26
**Status**: Complete

## R1: Categorized Sidebar with Collapsible Groups

**Decision**: Use WPF `CollectionViewSource` with `PropertyGroupDescription` grouping on the `Category` property, rendered via `GroupStyle` with collapsible `Expander` headers in the ListBox.

**Rationale**: The existing `SidebarControl` uses a flat `ListBox` bound to `NavigationService.Instance.Destinations`. Adding a `Category` property to `NavigationDestination` and applying grouping via `CollectionViewSource` is the minimal-change approach that preserves VirtualizationPanel (already configured with `IsVirtualizing="True"`) and existing item styles. Group headers become Expanders styled via ShellStyles.xaml, with collapse state persisted per category in ShellState.

**Alternatives considered**:
- **TreeView with hierarchical data** — Would require a new data model with parent-child hierarchy. Overkill for two-level navigation (category > item). Complicates keyboard navigation and loses ListBox virtualization.
- **Separate ListBox per category** — Requires dynamic ListBox creation/removal. Fragments focus management and selection tracking. Adds layout complexity.
- **Third-party sidebar control** — Violates Constitution XIV (no third-party UI frameworks except Material Design).

**Implementation notes**:
- `NavigationDestination` gains `Category` (string) and `ItemId` (string) properties
- `CollectionViewSource.GetDefaultView(Destinations)` with `PropertyGroupDescription("Category")`
- `GroupStyle` in SidebarControl.xaml renders category header as Expander
- Category header shows count of items (e.g., "Planning (4)")
- `NavigationService` preserves insert-order by Order within each category group

## R2: Dynamic Navigation Registration via [NavigationItem] Attribute

**Decision**: Define `NavigationItemAttribute` in `Som3a.Contracts` (Phase 14), create `SidebarRegistrationService` in WpfApp2.Services that discovers attribute-decorated types and registers them via `NavigationService.RegisterPage()`.

**Rationale**: The Plugin SDK's `AssemblyScanner` and `PluginDiscoverer` (Phase 14) provide assembly scanning infrastructure. A new `SidebarRegistrationService` wraps this: it scans loaded assemblies for types decorated with `[NavigationItem]`, reads Category, Label, Icon, Order, Priority from the attribute, and calls `NavigationService.Instance.RegisterPage()`. This bridges the Phase 14 .NET 8.0 Contracts layer to the .NET Framework 4.8 WPF host via the existing Som3a.Bridge interop.

**Alternatives considered**:
- **MEF/MAF plugin framework** — Over-architected for attribute-based navigation registration. Adds dependency on System.ComponentModel.Composition which isn't currently used.
- **Reflection-only scanning in WPF project** — Simpler but bypasses the Plugin SDK, missing version validation and dependency checking that the SDK provides.
- **JSON configuration file** — Static, requires manual updates, defeats dynamic plugin discovery goal.

**Implementation notes**:
- `SidebarRegistrationService` is a singleton registered in CompositionRoot
- Called during Shell initialization (after Plugin SDK has discovered plugins)
- Static built-in pages registered first, then plugin pages appended
- Duplicate ItemId detection: second registration is rejected with diagnostic log
- Invalid/unknown categories map to "Other" fallback category

## R3: Unsaved Changes Detection on Page Navigation

**Decision**: Pages implement `ISupportsDirtyTracking` interface. `WorkspaceHost` checks dirty state before unloading the current page, shows a modal dialog with Save/Discard/Cancel if dirty.

**Rationale**: The existing `WorkspaceHost` already manages page lifecycle (load/unload). Adding a dirty-check intercept in the unload path keeps the concern localized. The `PageBase` class already exists as a base for most pages — add `ISupportsDirtyTracking` to it with a virtual `IsDirty` property (defaults to false). Pages with editable content override it. The confirmation dialog uses `MaterialDesignThemes.DialogHost` per ADR-006.

**Alternatives considered**:
- **EventBus DirtyChanged event** — Over-engineered for a boolean query. EventBus overhead for every keystroke is wasteful.
- **ViewModel-based tracking** — MVVM-appropriate but requires ViewModel ↔ Page coordination. The interface on the Page itself is simpler and achieved by having the ViewModel expose IsDirty which the Page proxies.
- **Auto-save only (no prompt)** — Rejected in spec clarification (Option B chosen). Risk of saving partial/corrupt data.

**Implementation notes**:
- `ISupportsDirtyTracking` interface with `bool IsDirty { get; }` and `event EventHandler<bool> DirtyChanged`
- `PageBase` implements ISupportsDirtyTracking with virtual IsDirty → false
- Individual ViewModels set IsDirty = true on modification
- `WorkspaceHost.NavigateToPage()` calls `(currentPage as ISupportsDirtyTracking)?.IsDirty` before unloading
- NavigationEventArgs gains `bool Cancelled` property; ShellWindow checks it and reverts sidebar selection

## R4: Keyboard Navigation for Categorized Sidebar

**Decision**: Leverage existing WPF `KeyboardNavigation` attached properties plus explicit focus management in `SidebarControl.xaml.cs`. Tab enters sidebar, arrow keys navigate items within and across category groups, Enter selects, Left/Right collapse/expand categories.

**Rationale**: The existing `SidebarControl` already sets `KeyboardNavigation.TabNavigation="Continue"` and `KeyboardNavigation.DirectionalNavigation="Cycle"` on the ListBox. The enhancement adds: (a) `PreviewKeyDown` handler in code-behind for Left/Right to collapse/expand categories, (b) `IsTabStop="True"` on the sidebar toggle button, (c) a skip link (hidden `Button` with `IsTabStop="True"` before sidebar that moves focus to workspace). When sidebar is collapsed, `IsTabStop="False"` on the collapsed strip so Tab skips directly to workspace.

**Alternatives considered**:
- **Custom keyboard navigation service** — Over-architected. WPF's built-in keyboard navigation handles 90% of the requirement.
- **AccessKey/Mnemonic per item** — Not scalable for 50+ items. Arrow key navigation is more intuitive.
- **Ctrl+Number shortcuts per category** — Adds discoverability burden. Deferred to future enhancement.

**Implementation notes**:
- `SidebarControl` subscribes to `PreviewKeyDown` for Left/Right arrow handling
- Category expanders respond to Space/Enter for toggle
- Skip link: `Button` with `AutomationProperties.Name="Skip to content"`, `IsTabStop="True"`, placed before sidebar in tab order
- Collapsed state: set `SidebarListBox.IsTabStop = false` to skip

## R5: Sidebar Collapse/Expand Animation

**Decision**: Animate the sidebar column width via `Storyboard` with `DoubleAnimation` on `Grid.ColumnDefinitions[0].Width` from 220px to 48px (collapsed) and back. Use `Opacity` animation on item labels (fade out when collapsed). All animations ≤200ms per Constitution IX.

**Rationale**: The ShellWindow.xaml uses a Grid with `ColumnDefinition Width="220"` for the sidebar. Animating the column width is the simplest approach that preserves layout without fragmenting the visual tree. Item labels fade to `Opacity="0"` in collapsed state; icons remain at full opacity. The animation is GPU-safe (Opacity is composited; Width change triggers layout but the 200ms cap keeps it within budget).

**Alternatives considered**:
- **TranslateTransform slide** — More visually appealing but requires clipping and creates rendering artifacts in Excel VSTO safe mode (Constitution X).
- **Visibility toggle (instant)** — No animation, jarring UX. Rejected per spec clarification (auto-hide with hover).
- **SplitView control (custom)** — Reimplementation of WinUI SplitView. Unnecessary complexity; Grid column animation achieves the same effect.

**Implementation notes**:
- `ShellState` gains `bool SidebarCollapsed` property
- Toggle button in ShellWindow.xaml (hamburger menu icon) triggers collapse
- `Storyboard` targets `ColumnDefinition.Width` using `Storyboard.TargetName`
- Hover on collapsed sidebar triggers temporary expand (MouseEnter/MouseLeave handlers)
- Animation respects `SystemParameters.ClientAreaAnimation` (reduced motion)

## R6: Integration with Phase 14 (Platform Foundation)

**Decision**: Consume `INavigationItemAttribute` from `Som3a.Contracts` via the existing `Som3a.Bridge` interop layer. Use `AssemblyScanner` from `Som3a.Plugin.SDK` for plugin discovery.

**Rationale**: Phase 14 delivers Som3a.Contracts (with `[NavigationItem]` attribute definition) and Som3a.Plugin.SDK (with `AssemblyScanner`). Since these are .NET 8.0 projects and WpfApp2 is .NET Framework 4.8, the interop flows through Som3a.Bridge (.NET Standard 2.0). The attribute is defined as a .NET Standard 2.0-compatible custom attribute class in Som3a.Contracts, referenced by both the Plugin SDK (for scanning) and the WPF host (for consumption via Som3a.Bridge).

**Alternatives considered**:
- **Define attribute directly in WpfApp2** — Bypasses Phase 14 architecture. Plugin authors would need to reference the WPF host, breaking plugin isolation.
- **Define in Som3a.Bridge** — Violates project structure; Bridge is for interop only, not domain types.

**Implementation notes**:
- `Som3a.Contracts/NavigationItemAttribute.cs` defines the attribute with Category, Label, Icon, Order, Priority properties
- `Som3a.Plugin.SDK/Discovery/AssemblyScanner` scans for types with this attribute
- `WpfApp2/Services/SidebarRegistrationService.cs` receives discovered types and calls `NavigationService.RegisterPage()`
- Bridge passes `Type` objects back and forth
