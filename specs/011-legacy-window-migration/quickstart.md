# Quickstart: Migrating a Standalone Window to a Shell Page

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-25
**Prerequisite**: Phase 8 Shell platform must be implemented and merged.

## Overview

This guide walks through migrating an existing standalone window (`.xaml` + `.xaml.cs`) to a `Page` hosted within the Shell. Follow these steps in order.

## Migration Priority Table

| Priority | Window | Tier | Complexity | PageKey | Nav Order |
|----------|--------|------|------------|---------|-----------|
| 1 | SettingsWindow | 1 | Medium | settings | 20 |
| 2 | MainWindow | 1 | High | main | 10 |
| 3 | ProjectAnalysisWindow | 2 | High | project-analysis | 30 |
| 4 | Float_path | 2 | Medium | float-path | 40 |
| 5 | LinksManagerWindow | 2 | Medium | links-manager | 50 |
| 6 | SubDailyReportWindow | 2 | Medium | sub-daily-report | 60 |
| 7 | AssignTradeCodesWindow | 3 | Medium | assign-trade-codes | 70 |
| 8 | PrimaveraCompareWindow | 3 | High | primavera-compare | 80 |
| 9 | PrimaveraResultsWindow | 3 | High | primavera-results | 90 |
| 10 | XerEditorWindow | 3 | High | xer-editor | 100 |
| 11 | Fixpiecolors | 4 | Low | fix-pie-colors | 110 |
| 12 | StyleSelectorWindow | 4 | Low | style-selector | 120 |
| 13 | UnmergeFillDownWindow | 4 | Low | unmerge-fill-down | 130 |
| 14 | ToastWindow | 4 | Low | toast | 140 |

---

## Step 1: Create the MigrationRecord

Before starting, document the migration in `data-model.md`:

```markdown
| WindowName | OriginalPath | PagePath | MigrationStatus | PageKey | Priority | Complexity |
|------------|--------------|----------|-----------------|---------|----------|------------|
| MyWindow   | MyWindow.xaml | Pages/MyPage.xaml | InProgress | my-window | 5 | Medium |
```

Set `MigrationStatus = InProgress` to signal work has begun.

---

## Step 2: Create the Page Class

Create a new `Page` subclass in `WpfApp2/Pages/`:

```xml
<!-- WpfApp2/Pages/MyPage.xaml -->
<Page x:Class="Som3a_WPF_UI.Pages.MyPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="My Window"
      Background="{DynamicResource Brush.Background.Primary}">
    <!-- Content from original window goes here -->
</Page>
```

```csharp
// WpfApp2/Pages/MyPage.xaml.cs
namespace Som3a_WPF_UI.Pages
{
    public partial class MyPage : Page
    {
        public MyPage()
        {
            InitializeComponent();
        }
    }
}
```

**Rules**:
- Use `{DynamicResource}` for all themeable properties (backgrounds, borders, foregrounds, effects).
- No inline colors, no `StaticResource` for themeable properties.
- Keep `Background` — Pages do not have a Window chrome; the Shell provides it.

---

## Step 3: Extract Content XAML

From the original standalone window (e.g., `MyWindow.xaml`):

1. Copy the **content** inside the `<Window>` root element (typically the inner `<Grid>`, `<StackPanel>`, or other container).
2. Paste it inside the new `<Page>` element.
3. **Do not copy**:
   - `Window` attributes (`WindowStyle`, `AllowsTransparency`, `Background`, etc.)
   - `Window` tags themselves
   - Title bar buttons (min/max/close) — Shell provides these
   - Standalone window resize/move logic

4. **Update resource references**:
   - Change `Background="{DynamicResource Brush.Background.Window}"` to `Background="{DynamicResource Brush.Background.Primary}"` (or appropriate semantic brush)
   - Ensure all brushes and colors use `{DynamicResource}`

---

## Step 4: Handle Code-Behind Logic

Original code-behind (`MyWindow.xaml.cs`) typically contains:

| Code | Action |
|------|--------|
| `InitializeComponent()` | Keep in Page |
| Event handlers (`Button_Click`, etc.) | Keep in Page code-behind if simple; move to ViewModel if complex |
| Service injections | Add to Page constructor via `App.Container.Resolve<T>()` or inject via `IServiceContainer` |
| Window-specific state | Some may need to become Page fields |
| `Show()`, `Close()`, `Hide()` calls | Remove — Pages are shown/hidden by Shell |
| Dialog `ShowDialog()` calls | Replace with `DialogService.ShowDialog()` or similar |

**Important**: If the original window had business logic in code-behind, consider creating a ViewModel and using MVVM binding. See Phase 6 MVVM patterns.

---

## Step 5: Register the Page with NavigationService

In the appropriate module initialization or startup code:

```csharp
var nav = NavigationService.Instance;
nav.RegisterPage<MyPage>(
    key: "my-window",           // Must match MigrationRecord.PageKey
    displayName: "My Window",    // Sidebar display name
    icon: "MyWindowIcon",        // Optional — references existing icon
    order: 50                   // Lower = higher in sidebar
);
```

---

## Step 6: Update the Ribbon Launcher

Find the ribbon callback that opens the original window:

```csharp
// BEFORE (standalone window):
var window = new MyWindow();
window.Show();

// AFTER (shell page):
NavigationService.Instance.NavigateTo("my-window");
```

**Tip**: If the Shell is not yet open when navigation is requested, the `NavigationService` opens the Shell first, then navigates to the page. No separate Shell-open call needed.

---

## Step 7: Validate in Excel VSTO Host

Run the validation checklist for the migrated page:

- [ ] Page opens in shell without black/blank backgrounds
- [ ] DPI 100%: All elements render correctly
- [ ] DPI 125%: All elements scale correctly
- [ ] DPI 150%: All elements scale correctly
- [ ] Theme switch: Page updates when theme changes
- [ ] Popup rendering: ComboBox and popups render without clipping
- [ ] Keyboard navigation: Tab, Enter, Escape work
- [ ] FallbackSafe mode: Page renders correctly when FallbackSafe active
- [ ] Functional parity: All operations produce same results as standalone window

Document results in `ValidationChecklistItem` entries.

---

## Step 8: Mark Migration Complete

After validation passes:

1. Update `MigrationRecord` in `data-model.md`:
   - Set `MigrationStatus = Validated`
   - Set `ValidatedBy` and `ValidatedDate`

2. **Preserve original XAML** for 1 week after validation (rollback buffer)

3. **After rollback buffer expires**, delete original standalone files:
   - `WpfApp2/MyWindow.xaml`
   - `WpfApp2/MyWindow.xaml.cs`

---

## Rollback Procedure

If validation fails:

1. Set `MigrationStatus = RolledBack` in `data-model.md`
2. Document the failure reason in `Notes`
3. Keep original standalone XAML files in place
4. Ribbon launcher can remain pointing to NavigationService (will show the in-progress Page) OR temporarily revert to original window instantiation
5. Fix issues and retry from Step 4

---

## Common Issues

| Issue | Solution |
|-------|----------|
| Page appears blank | Check `Background` is set; Shell chrome may be covering content |
| Controls not scaling with DPI | Use `{DynamicResource}` for all sizes; avoid hardcoded pixel values |
| Popup clips or renders incorrectly | Ensure ComboBox uses `AllowsTransparency="False"` and proper `PlacementTarget` |
| Theme switch doesn't update Page | Subscribe to `ThemeManager.ThemeChanged` and force refresh if needed |
| Code-behind has business logic | Move to ViewModel using Phase 6 patterns; do not put business logic in Page |

---

## Reference: File Locations

| File Type | Original Location | Migrated Location |
|-----------|------------------|-------------------|
| Window XAML | `WpfApp2/` root | DELETE after validation |
| Window code-behind | `WpfApp2/` root | DELETE after validation |
| Page XAML | NEW: `WpfApp2/Pages/` | Keep |
| Page code-behind | NEW: `WpfApp2/Pages/` | Keep |
| ViewModel (if needed) | NEW: `WpfApp2/ViewModels/` | Keep |