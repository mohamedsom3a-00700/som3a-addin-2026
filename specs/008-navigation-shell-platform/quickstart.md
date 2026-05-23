# Quickstart: Adding a New Shell Page

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-23
**Prerequisite**: Shell framework must be implemented and merged.

## Steps

### Step 1: Create the Page

Create a new `Page` subclass in `WpfApp2/Pages/`:

```xml
<!-- WpfApp2/Pages/MyFeaturePage.xaml -->
<Page x:Class="Som3a_WPF_UI.Pages.MyFeaturePage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="My Feature">
    <Grid Background="{DynamicResource Brush.Background.Primary}">
        <TextBlock Text="My Feature Content"
                   Style="{DynamicResource Style.Text.Body}" />
    </Grid>
</Page>
```

```csharp
// WpfApp2/Pages/MyFeaturePage.xaml.cs
namespace Som3a_WPF_UI.Pages
{
    public partial class MyFeaturePage : Page
    {
        public MyFeaturePage()
        {
            InitializeComponent();
        }
    }
}
```

**Important**: Use `{DynamicResource}` for all themeable properties. No inline colors or hardcoded values.

### Step 2: Register the Page

In your ribbon callback or module initialization, register the page with `NavigationService`:

```csharp
var nav = NavigationService.Instance;
nav.RegisterPage<MyFeaturePage>(
    key: "my-feature",
    displayName: "My Feature",
    icon: "FeatureIcon",      // optional
    order: 50                 // lower = higher in sidebar
);
```

### Step 3: Navigate from Ribbon

Replace window-open code with navigation call:

```csharp
// Before (standalone window):
// var window = new MyFeatureWindow();
// window.Show();

// After (shell page):
NavigationService.Instance.NavigateTo("my-feature");
```

### Step 4: Add Ribbon Button (if needed)

Map the ribbon button's action to the navigation call above. The shell opens automatically if not already displayed.

## Registration Patterns

### Single call (module init)
```csharp
nav.RegisterPage<MyFeaturePage>("my-feature", "My Feature", order: 10);
```

### Grouped registration (multiple pages)
```csharp
nav.RegisterPage<DashboardPage>("dashboard", "Dashboard", order: 0);
nav.RegisterPage<ReportsPage>("reports", "Reports", order: 10);
nav.RegisterPage<SettingsPage>("settings", "Settings", order: 20);
```

## Rules

- Page class MUST have a parameterless constructor.
- Page MUST use DynamicResource for themeable properties.
- Page SHOULD keep a reasonable visual tree (avoid deep nesting for Excel performance).
- Page MUST NOT open standalone windows for features that should be shell pages.
- If the feature needs modal dialogs, use the existing DialogService (not new windows).
