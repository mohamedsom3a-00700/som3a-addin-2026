# Contract: IPageHost

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-23

## Purpose

Contract for the workspace hosting surface that manages the Frame and page lifecycle. The shell's workspace area implements this contract internally.

## Interface

```csharp
public interface IPageHost
{
    /// <summary>
    /// Navigate the host to a Page instance.
    /// </summary>
    void Navigate(Page page);

    /// <summary>
    /// Show an error state within the workspace.
    /// </summary>
    void ShowError(string message, Action retryAction);

    /// <summary>
    /// Show the welcome/dashboard page.
    /// </summary>
    void ShowWelcome();

    /// <summary>
    /// Clear the current content (show empty state).
    /// </summary>
    void Clear();

    /// <summary>
    /// Raised when navigation completes (success or failure).
    /// </summary>
    event EventHandler<NavigationEventArgs> NavigationCompleted;
}
```

## Behavior

- `Navigate(Page)` sets the Frame content and triggers entrance transition.
- `ShowError()` shows an inline error template with a retry button. Retry calls `retryAction`.
- `ShowWelcome()` navigates to the registered welcome page.
- Entrance transitions (fade in, ≤200ms) applied via the existing Animations.xaml storyboards.
