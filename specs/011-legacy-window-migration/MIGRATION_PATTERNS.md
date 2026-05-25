# Migration Patterns: Standalone Window → Shell Page

**Feature**: 011-legacy-window-migration  
**Date**: 2026-05-25

## Window-to-Page Transformation Rules

### XAML Transformation

1. **Root element**: Change `<controls:ModernWindow>` to `<Page>`
2. **Remove Window attributes**: `WindowStyle`, `AllowsTransparency`, `ResizeMode`, `WindowStartupLocation`, `ShowMaximizeButton`, `ShowInTaskbar`, `Topmost`, `TitleBarHeight`, `SizeToContent`
3. **Remove Background="Transparent"**: Pages don't need transparent backgrounds
4. **Set Page Background**: `Background="{DynamicResource Brush.Background.Primary}"`
5. **Remove custom title bar buttons**: Min/Max/Close buttons — Shell provides these
6. **Remove `RootBorder`**: The outer `Border` with `CornerRadius="{DynamicResource WindowRadius}"` and `BorderBrush="{DynamicResource CardStrokeBrush}"` is a window chrome element, not needed for Pages
7. **Keep inner content**: All `Grid` content, bindings, styles, resources remain unchanged
8. **Keep SnapsToDevicePixels + UseLayoutRounding**: Always `True`
9. **DynamicResource-only**: No inline colors, no StaticResource for themeable properties

### Code-Behind Transformation

| Original Code | Page Equivalent |
|--------------|-----------------|
| `ModernWindow` base class | `Page` base class |
| `DragMove()` | Remove — Shell provides window dragging |
| `WindowState = WindowState.Minimized` | Remove — Shell provides minimize |
| `Close()` | `NavigationService.Instance.GoBack()` |
| `ShowDialog()` | `NavigationService.Instance.NavigateTo(key)` |
| `_window.Show()` | `NavigationService.Instance.NavigateTo(key)` |
| `Closed` event | `Unloaded` event |
| `Loaded` event | Keep as is |
| `this` as Window reference | `this` as Page reference |

### Close/Back Handler Adaptation

Windows that use `CloseCommand` or `RequestClose` should redirect to navigation:

```csharp
// For ViewModels that accept a closeAction:
vm = new SomeViewModel(container, closeAction: () => NavigationService.Instance.GoBack());

// For ViewModels with CloseWindow property:
vm.CloseWindow = result => NavigationService.Instance.GoBack();

// For ViewModels with RequestClose event:
vm.RequestClose += () => NavigationService.Instance.GoBack();
```

### Excel Application Attachment

Windows that receive `Excel.Application` in constructor need adaptation:

```csharp
// Pattern: Add InitializeWithExcel method to Page
public void InitializeWithExcel(Excel.Application app)
{
    _vm?.AttachExcel(app);
    DataContext = new SomeViewModel(container, app, () => NavigationService.Instance.GoBack());
}
```

If using service container, prefer resolving Excel app from a shared service rather than passing through constructor.

## Ribbon Integration Update Patterns

### Before (Standalone Window)
```csharp
private void ribbonButton_Click(object sender, RibbonControlEventArgs e)
{
    var app = Globals.ThisAddIn.Application;
    var window = new TargetWindow(app);
    var helper = new WindowInteropHelper(window);
    helper.Owner = new IntPtr(app.Hwnd);
    window.Closed += (s, args) => { /* cleanup */ };
    window.Show();
    window.Activate();
}
```

### After (Shell Page)
```csharp
private void ribbonButton_Click(object sender, RibbonControlEventArgs e)
{
    RegisterShellPages();
    NavigationService.Instance.NavigateTo("target-page-key");
}
```

### Page Registration in Ribbon
```csharp
private static void RegisterShellPages()
{
    if (_shellPagesRegistered) return;
    var nav = NavigationService.Instance;
    nav.RegisterPage<TargetPage>("target-page-key", "Display Name", null, order);
    _shellPagesRegistered = true;
}
```

## Validation Checklist Template

| # | Criterion | Method |
|---|-----------|--------|
| 1 | Page opens without black/blank backgrounds | Load page in Shell, check visual appearance |
| 2 | DPI 100% | Test at standard DPI, verify all controls render |
| 3 | DPI 125% | Test at 125% scaling, verify layout adapts |
| 4 | DPI 150% | Test at 150% scaling, verify no clipping |
| 5 | Theme switch | Switch Dark→Light→Custom, verify all colors update |
| 6 | Popup rendering | Test ComboBox, ContextMenu — verify no clipping/transparency issues |
| 7 | Keyboard nav | Tab through all controls, Enter/Escape trigger expected actions |
| 8 | FallbackSafe | Test in FallbackSafe mode, verify correct rendering |
| 9 | Functional parity | Run all operations side-by-side with original window |

## Rollback Procedure

1. Set `MigrationStatus = RolledBack` in `data-model.md`
2. Document failure reason in Notes field
3. Original standalone XAML remains in place (never deleted until Validated)
4. Revert ribbon launcher to original `new Window().Show()` pattern
5. Fix issues and retry migration

## File Locations

| File Type | Original | Migrated |
|-----------|----------|----------|
| Window XAML | `WpfApp2/{Name}.xaml` or `WpfApp2/Views/{Name}.xaml` | `WpfApp2/Pages/{Name}Page.xaml` |
| Code-behind | `WpfApp2/{Name}.xaml.cs` | `WpfApp2/Pages/{Name}Page.xaml.cs` |
| ViewModel | `WpfApp2/ViewModels/{Name}ViewModel.cs` | Unchanged |
