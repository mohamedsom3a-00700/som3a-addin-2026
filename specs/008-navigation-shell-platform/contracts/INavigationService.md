# Contract: INavigationService

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-23

## Purpose

Central service for page registration, navigation requests, and navigation state tracking. Acts as the single point of contact between navigation sources (sidebar, ribbon, command palette) and page hosting.

## Interface

```csharp
public interface INavigationService
{
    /// <summary>
    /// Register a page type for navigation.
    /// </summary>
    void RegisterPage<T>(string key, string displayName, string icon = null, int order = 100)
        where T : Page, new();

    /// <summary>
    /// Navigate the shell workspace to the page registered under the given key.
    /// </summary>
    void NavigateTo(string key);

    /// <summary>
    /// Navigate to the previously active page.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Current active page key. Null if shell just opened (welcome page).
    /// </summary>
    string ActivePageKey { get; }

    /// <summary>
    /// Raised when ActivePageKey changes.
    /// </summary>
    event EventHandler<NavigationEventArgs> NavigationChanged;

    /// <summary>
    /// All registered destinations for sidebar and command palette binding.
    /// </summary>
    ObservableCollection<NavigationDestination> Destinations { get; }

    /// <summary>
    /// Filtered destinations matching the given search text.
    /// </summary>
    IEnumerable<NavigationDestination> Search(string searchText);
}
```

## NavigationEventArgs

```csharp
public class NavigationEventArgs : EventArgs
{
    public string PreviousKey { get; }
    public string NewKey { get; }
    public bool Success { get; }
    public Exception Error { get; }
}
```

## Error Handling

- `NavigateTo` with unregistered key: no-op (log warning, no crash).
- Page instantiation failure: set `Success = false`, fill `Error` in event args, workspace shows error state.
- `GoBack` with empty history: no-op.
