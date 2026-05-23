# Contract: ISidebarModel

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-23

## Purpose

Data model contract for the sidebar control. The sidebar binds to an ObservableCollection and updates visual state based on selection.

## Interface

```csharp
public interface ISidebarModel
{
    /// <summary>
    /// All visible navigation destinations bound to the sidebar.
    /// </summary>
    ObservableCollection<NavigationDestination> Items { get; }

    /// <summary>
    /// The currently selected destination. Setting this triggers navigation.
    /// </summary>
    NavigationDestination SelectedItem { get; set; }
}
```

## NavigationDestination (shared entity)

```csharp
public class NavigationDestination : INotifyPropertyChanged
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string Icon { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }  // Bound to ListBoxItem.IsSelected
}
```

## Binding

- Sidebar `ListBox` ItemsSource binds to `Items`.
- `ListBox.SelectedItem` two-way binds to `SelectedItem`.
- `ListBox.ItemTemplate` displays Icon + Label.
- `ListBox.ItemContainerStyle` maps `IsSelected` to visual states (highlighted background, glow indicator).
- Keyboard navigation handled natively by `ListBox` (arrow keys, Home, End, Enter).
