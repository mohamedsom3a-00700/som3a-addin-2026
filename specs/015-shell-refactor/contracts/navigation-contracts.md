# Navigation Contracts: Shell Refactor

**Feature**: 015-shell-refactor
**Date**: 2026-05-26

## Interface: INavigationService *(extended)*

Existing interface in `WpfApp2/Services/NavigationService.cs`. Extended with category support.

```csharp
public interface INavigationService
{
    // EXISTING — unchanged
    void RegisterPage<T>(string key, string displayName, string icon = null, int order = 50) where T : Page, new();
    void RegisterPage(Type pageType, string key, string displayName, string icon = null, int order = 50);
    void NavigateTo(string key, bool pushToHistory = true, string overridePreviousKey = null);
    bool GoBack();
    Page CreatePage(string key);
    bool IsPageRegistered(string key);
    List<NavigationDestination> Search(string query);
    ObservableCollection<NavigationDestination> Destinations { get; }
    event EventHandler<NavigationEventArgs> NavigationChanged;

    // NEW — Phase 15 extensions
    void RegisterPage(string category, Type pageType, string key, string displayName, string icon = null, int order = 50);
    IReadOnlyList<string> GetCategories();
    NavigationDestination GetActiveDestination();
    bool RequestNavigation(string targetKey); // Returns false if cancelled by dirty check
}
```

**New method contracts**:

- `RegisterPage(category, ...)` — Register a page with a category. Category must be a known value or "Other". If category is null/empty, defaults to "Other".
- `GetCategories()` — Returns distinct category names present in Destinations, ordered by predefined category order.
- `GetActiveDestination()` — Returns the currently selected NavigationDestination (or null).
- `RequestNavigation(string targetKey)` — Initiates navigation with dirty-check guard. Returns false if navigation was cancelled by user (Cancel on unsaved changes dialog). Subscribers to NavigationChanged only fire on successful navigation.

## Interface: ISupportsDirtyTracking *(new)*

```csharp
public interface ISupportsDirtyTracking
{
    bool IsDirty { get; }
    event EventHandler<bool> DirtyChanged;
}
```

Implemented by `PageBase` and any Page with editable state. WorkspaceHost checks this before unloading a Page.

## Interface: ISidebarRegistrationProvider *(new)*

Bridge between Plugin SDK discovery and NavigationService registration.

```csharp
public interface ISidebarRegistrationProvider
{
    void RegisterStaticPages();                          // Register built-in (non-plugin) pages
    void RegisterPluginPages(IEnumerable<Type> pageTypes); // Register pages from discovered plugins
}
```

Implemented by `SidebarRegistrationService`. Called during Shell initialization (CompositionRoot).

## Interface: INavigationItemAttribute *(new, in Som3a.Contracts)*

Defined in Phase 14's Som3a.Contracts as a .NET Standard 2.0 compatible attribute.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NavigationItemAttribute : Attribute
{
    public string Category { get; set; }      // "Planning", "Analysis", "Excel", "AI", "Settings"
    public string Label { get; set; }          // Display text in sidebar
    public string Icon { get; set; }           // Material Design icon name (e.g., "ChartTimelineVariant")
    public int Order { get; set; } = 50;       // Sort order within category
    public int Priority { get; set; } = 0;     // Conflict resolution priority
}
```

## Contract: NavigationDestination *(extended)*

```csharp
public class NavigationDestination : INotifyPropertyChanged
{
    // EXISTING
    string Key { get; set; }
    string Label { get; set; }
    string Icon { get; set; }
    int Order { get; set; }
    bool IsVisible { get; set; }
    bool IsSelected { get; set; }

    // NEW — Phase 15
    string Category { get; set; }    // Category group name
    string ItemId { get; set; }      // Plugin-scoped unique identifier
    bool IsEnabled { get; set; }     // False when target Page unresolved
}
```

## Contract: NavigationEventArgs *(extended)*

```csharp
public class NavigationEventArgs : EventArgs
{
    // EXISTING
    string PreviousKey { get; set; }
    string NewKey { get; set; }
    bool Success { get; set; }
    string Error { get; set; }

    // NEW — Phase 15
    bool Cancelled { get; set; }     // True when navigation was cancelled by user
}
```

## Contract: SidebarCategory *(new, runtime)*

```csharp
public class SidebarCategory : INotifyPropertyChanged
{
    string Name { get; set; }              // e.g., "Planning"
    string DisplayName { get; set; }       // e.g., "Planning"
    int DisplayOrder { get; set; }         // 1 = top
    bool IsCollapsed { get; set; }         // Category header collapsed
    ObservableCollection<NavigationDestination> Items { get; }  // Items in this category
}
```

## Static Page → Category Mapping

Built-in pages registered at startup (before plugin pages):

| Page | Key | Category | Icon | Order |
|------|-----|----------|------|-------|
| WelcomePage | "home" | (home — special, shown by default) | "Home" | 0 |
| ProjectAnalysisPage | "planning.analysis" | Planning | "ChartTimelineVariant" | 10 |
| FloatPathPage | "analysis.floatpath" | Analysis | "Routes" | 10 |
| PrimaveraComparePage | "planning.primavera.compare" | Planning | "Compare" | 20 |
| PrimaveraResultsPage | "planning.primavera.results" | Planning | "FileDocument" | 30 |
| XerEditorPage | "planning.xereditor" | Planning | "FileCode" | 40 |
| LinksManagerPage | "excel.links" | Excel | "LinkVariant" | 10 |
| SubDailyReportPage | "excel.subdaily" | Excel | "FileTable" | 20 |
| AssignTradeCodesPage | "excel.tradecodes" | Excel | "Tag" | 30 |
| FixPieColorsPage | "excel.piecolors" | Excel | "Palette" | 40 |
| StyleSelectorPage | "excel.styles" | Excel | "FormatPaint" | 50 |
| UnmergeFillDownPage | "excel.unmerge" | Excel | "TableMergeCells" | 60 |
| SettingsPage | "settings.general" | Settings | "Cog" | 10 |
