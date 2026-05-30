# Quickstart: CommunityToolkit.Mvvm ViewModel Refactor

**Date**: 2026-05-31
**Feature**: CommunityToolkit.Mvvm ViewModel Refactor

## Overview

This guide explains how to migrate an existing ViewModel from manual `INotifyPropertyChanged` and custom `RelayCommand` to CommunityToolkit.Mvvm source-generated `[ObservableProperty]` and `[RelayCommand]`.

## Prerequisites

- CommunityToolkit.Mvvm package is already installed in the project.
- `ViewModelBase` has been updated to inherit `ObservableObject` (Batch 1).
- The ViewModel you are migrating is declared as `partial class`.

## Step-by-Step Migration

### Step 1: Make the class `partial`

```csharp
// BEFORE
public class HomeViewModel : ViewModelBase

// AFTER
public partial class HomeViewModel : ViewModelBase
```

### Step 2: Replace manual properties with `[ObservableProperty]`

```csharp
// BEFORE
private string _searchText;
public string SearchText
{
    get => _searchText;
    set
    {
        _searchText = value;
        OnPropertyChanged(nameof(SearchText));
    }
}

// AFTER
[ObservableProperty]
private string _searchText;
```

> **Note**: The source generator will create a public `SearchText` property automatically. The generated code uses the exact same `SetProperty(ref _searchText, value)` logic.

### Step 3: Handle custom setter side effects (if any)

If the old setter had side effects:

```csharp
// BEFORE
private string _searchText;
public string SearchText
{
    get => _searchText;
    set
    {
        _searchText = value;
        OnPropertyChanged(nameof(SearchText));
        PerformSearch(); // side effect
    }
}

// AFTER
[ObservableProperty]
private string _searchText;

partial void OnSearchTextChanged(string value)
{
    PerformSearch(); // side effect moved to hook
}
```

> **Tip**: For simple dependent-property notifications, use `[NotifyPropertyChangedFor(nameof(OtherProperty))]` instead of a partial method.

### Step 4: Replace manual commands with `[RelayCommand]`

```csharp
// BEFORE
public ICommand SearchCommand { get; }

public HomeViewModel()
{
    SearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
}

private void ExecuteSearch()
{
    // ...
}

private bool CanExecuteSearch()
{
    return !string.IsNullOrEmpty(SearchText);
}

// AFTER
[RelayCommand(CanExecute = nameof(CanSearch))]
private void Search()
{
    // ...
}

private bool CanSearch()
{
    return !string.IsNullOrEmpty(SearchText);
}
```

> **Note**: The source generator creates a public `SearchCommand` property automatically. Use `[RelayCommand]` for synchronous commands and `[AsyncRelayCommand]` (or the async method pattern) for asynchronous commands.

### Step 5: Build and verify

1. Build the solution (`dotnet build` or MSBuild).
2. Check `obj/` folder for `*.g.cs` files containing generated code.
3. Run the application and verify the page/screen still functions correctly.
4. Run the verification script: `Scripts\Verify-NoManualMvvmPatterns.ps1`

## Common Patterns

### Async Command

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    // async logic
}
```

The generator produces `LoadDataCommand : IAsyncRelayCommand`.

### Command with Parameter

```csharp
[RelayCommand]
private void SelectItem(string itemId)
{
    // ...
}
```

The generator produces `SelectItemCommand : IRelayCommand<string>`.

### Property Change Notification for Another Property

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private string _firstName;
```

This automatically raises `PropertyChanged` for `FullName` and `CanExecuteChanged` for `SaveCommand` whenever `FirstName` changes.

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| Build error: "Type must be partial" | Missing `partial` keyword on class | Add `partial` to class declaration |
| Build error: "Property already defined" | Manual property not deleted | Remove the manual property; only keep the `[ObservableProperty]` field |
| Command not updating CanExecute | Missing `[NotifyCanExecuteChangedFor]` | Add attribute or call `Command.NotifyCanExecuteChanged()` manually |
| Generated file not found | Build not run | Build the project; `*.g.cs` lives in `obj/Debug/net8.0-windows/` |

## Verification

After migrating a batch, run the verification script from the repo root:

```powershell
powershell -File WpfApp2/Scripts/Verify-NoManualMvvmPatterns.ps1
```

Expected output: `✅ No manual MVVM patterns found.`
