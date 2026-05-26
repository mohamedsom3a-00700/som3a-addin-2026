# Quickstart: Shell Refactor

**Feature**: 015-shell-refactor
**Date**: 2026-05-26

## Overview

This guide helps a developer understand the Shell navigation system and how to add new pages or plugins.

## Key Files

| File | Purpose |
|------|---------|
| `WpfApp2/Controls/Shell/SidebarControl.xaml` | Sidebar with categorized navigation items |
| `WpfApp2/Controls/Shell/ShellWindow.xaml` | Main Shell container (sidebar + workspace) |
| `WpfApp2/Controls/Shell/WorkspaceHost.cs` | Page loading/unloading with dirty check |
| `WpfApp2/Controls/Shell/NavigationDestination.cs` | Sidebar item data model |
| `WpfApp2/Services/NavigationService.cs` | Page registration and navigation logic |
| `WpfApp2/Services/SidebarRegistrationService.cs` | Discovers `[NavigationItem]` attributes |
| `WpfApp2/Services/PageStateTracker.cs` | Tracks IsDirty state per page |
| `WpfApp2/Theme/ShellStyles.xaml` | Sidebar, workspace, category header styles |

## Adding a Built-in Page

1. Create your Page class (e.g., `MyFeaturePage.xaml`) in `WpfApp2/Pages/`
2. Extend `PageBase` for MVVM support and dirty tracking
3. Register in `SidebarRegistrationService.RegisterStaticPages()`:

```csharp
NavigationService.Instance.RegisterPage(
    category: "Planning",
    pageType: typeof(MyFeaturePage),
    key: "planning.myfeature",
    displayName: "My Feature",
    icon: "Star",
    order: 15
);
```

4. The page appears in the Planning category at order 15

## Adding a Plugin Page

1. In your .NET 8.0 plugin assembly, add the attribute to your Page class:

```csharp
[NavigationItem(Category = "AI", Label = "Schedule Reviewer", Icon = "Robot", Order = 10)]
public class ScheduleReviewerPage : Page
{
    // ...
}
```

2. Place the plugin assembly in `WpfApp2/Modules/`
3. On next Shell initialization, `SidebarRegistrationService` discovers and registers it
4. The item appears in the AI category automatically

## Making a Page Dirty-Aware

```csharp
public partial class MyFeaturePage : PageBase
{
    public override bool IsDirty => _viewModel.HasUnsavedChanges;

    public MyFeaturePage()
    {
        InitializeComponent();
        _viewModel = new MyFeatureViewModel();
        _viewModel.DirtyChanged += (s, dirty) => OnPropertyChanged(nameof(IsDirty));
        DataContext = _viewModel;
    }
}
```

The Shell will prompt Save/Discard/Cancel when navigating away with unsaved changes.

## Sidebar Behavior

- **Collapse**: Click the hamburger button (top of sidebar) to collapse to icon strip. Hover to temporarily expand.
- **Keyboard**: Tab into sidebar, arrow keys navigate items, Enter selects, Left/Right collapse/expand categories. Skip link (Tab before sidebar) jumps to workspace.
- **Categories**: Click category header to collapse/expand its items. State persists per session.

## Build Commands

```powershell
# Build the WPF host
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Build supporting libraries (Phase 14 prereq)
dotnet build Som3a.Contracts/Som3a.Contracts.csproj
dotnet build Som3a.Plugin.SDK/Som3a.Plugin.SDK.csproj
dotnet build Som3a.Bridge/Som3a.Bridge.csproj
```

## Testing in Excel VSTO

1. Build the solution
2. Open Excel, load the Som3a add-in
3. Click "Open Shell" from the ribbon
4. Verify all 5 categories appear with correct items
5. Test keyboard navigation: Tab → Arrow keys → Enter
6. Test collapse: Click hamburger → hovers → auto-collapse
7. Test unsaved changes: Edit a page with dirty tracking → click different sidebar item → verify Save/Discard/Cancel dialog
