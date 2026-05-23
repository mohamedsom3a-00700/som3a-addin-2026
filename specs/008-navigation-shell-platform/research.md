# Research: Navigation Shell Platform

**Phase**: 0 — Outline & Research
**Date**: 2026-05-23

## Navigation Patterns for WPF Shell

### Decision: Frame-based workspace with MVVM navigation

**Rationale**: WPF provides `Frame` and `Page` primitives that handle journaling, navigation events, and content lifecycle. A `Frame` inside the shell workspace provides built-in forward/back navigation support, which maps to future Phase 11 legacy migration needs. Binding `Frame.Source` or using `Frame.Navigate()` via a ViewModel fits the existing MVVM pattern.

**Alternatives considered**:
- **ContentControl with DataTemplates**: Simpler but lacks navigation journal, URI-based routing, and page lifecycle events. Breaking change path for Phase 11 migration.
- **TabControl**: Suitable for multi-document workflows but not aligned with sidebar → single-page workspace pattern.
- **Third-party shell frameworks**: Prohibited by Constitution §XIV (No Third-Party UI Frameworks).

### Decision: Singleton NavigationService with page registry

**Rationale**: A singleton `INavigationService` registered via constructor injection (or service locator for existing code-behind windows) provides a single source of truth for page registration and navigation. Pages register with a string key; ribbons and sidebar send navigation requests by key. This decouples navigation sources from page implementations.

**Key patterns**:
- `INavigationService.RegisterPage<T>(string key, string displayName, string icon)` — registers a page type with its metadata
- `INavigationService.NavigateTo(string key)` — navigates workspace to the registered page
- `INavigationService.NavigationChanged` event — notifies shell of current page changes (for sidebar highlight, status bar updates)

### Decision: Sidebar as ItemsControl with dynamic item source

**Rationale**: WPF `ItemsControl` or `ListBox` bound to an `ObservableCollection<NavigationDestination>` provides dynamic sidebar item registration. ItemTemplate binding to icon, display name, and active state enables full keyboard navigation via default listbox keyboard handling (arrow keys, Home/End, Enter).

**Active item indication**: Use a data-bound `IsSelected` property on `NavigationDestination` and a `ListBox` with styled `IsSelected` visual state. This satisfies FR-003 (visual indication of active item) and FR-004 (keyboard navigation) with zero custom UI logic.

### Decision: Command palette as popup overlay with TextBox + ListBox

**Rationale**: A `Popup` (Excel-safe: `AllowsTransparency="False"`) containing a `TextBox` for input and a `ListBox` for filtered results. Filtering happens against the `INavigationService.RegisteredPages` collection. Keyboard: up/down arrows move selection, Enter navigates, Escape closes. Style sourced from existing Effects/Shadows.xaml and Theme/Controls/.

### Decision: Welcome page as a simple static Page

**Rationale**: The welcome/dashboard page is a static `Page` subclass registered like any other page. It lists registered navigation destinations and provides quick-launch buttons. No special shell integration needed beyond standard page lifecycle.

### Decision: Ribbon integration via NavigationService

**Rationale**: Excel ribbon callbacks (existing `ThisAddIn.cs` or ribbon XML) call `NavigationService.Instance.NavigateTo("pageKey")` instead of `new SomeWindow().Show()`. The NavigationService checks if the shell window is open; if not, creates it via existing `ModernWindow` pattern; then navigates the workspace frame. No ribbon code changes beyond the callback target.

## Excel VSTO Constraints

- `Frame` works inside Excel VSTO hosting but must set `JournalOwnership="OwnsJournal"` to avoid shared journal issues.
- `Popup` for command palette must use `AllowsTransparency="False"` (existing ComboBox pattern) — Excel-safe.
- All shell windows inherit from `ModernWindow` to get WindowRenderModeDetector integration automatically.
- No Windows Aero/glass effects in shell (Constitution §X — Excel Rendering Safety).

## Decisions Consolidated

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Page hosting | Frame + Page | Built-in nav journal, URI routing, Phase 11 migration path |
| Navigation pattern | Singleton service with string-key registry | Decouples ribbon/sidebar from pages |
| Sidebar | ListBox bound to ObservableCollection | Native keyboard nav, dynamic items, minimal custom code |
| Command palette | Popup + TextBox + ListBox | Excel-safe popup pattern, fast filtering |
| Welcome page | Static Page in Pages/ folder | Simple, no special integration |
| Ribbon integration | NavigationService call from ribbon callback | Zero shell awareness in ribbon code |
