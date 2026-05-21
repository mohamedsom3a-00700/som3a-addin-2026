# MVVM Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Framework**: .NET Framework 4.8 WPF

---

## 1. Layer Responsibilities

### View (XAML + Code-Behind)

**Allowed:**
- Visual layout and styling
- Event routing to ViewModel commands
- Focus management
- Animation triggers
- Visual state transitions
- `DataContext` assignment

**Prohibited:**
- Business logic
- Data transformation
- Service instantiation
- ResourceDictionary manipulation
- Direct database/file access
- Theme mutation (only ThemeManager)

### ViewModel

**Allowed:**
- Property exposure with `INotifyPropertyChanged`
- Command definitions (`ICommand`)
- Input validation
- Navigation coordination
- Service delegation

**Prohibited:**
- Direct UI control access
- ResourceDictionary manipulation
- Rendering logic
- Service implementation (call, don't implement)

### Service

**Allowed:**
- Business logic implementation
- External system integration (Excel, database, file system)
- Computation and data transformation
- Theme management (ThemeManager)
- Rendering mode detection (WindowRenderModeDetector)

**Prohibited:**
- ViewModel behavior (no property change notification)
- Direct UI interaction
- XAML resource manipulation

---

## 2. Code-Behind Rules

### Event Handlers

**Correct:**

```csharp
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // Delegate to ViewModel command
    if (ViewModel.SaveCommand.CanExecute(null))
        ViewModel.SaveCommand.Execute(null);
}
```

**Incorrect:**

```csharp
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // Business logic in code-behind
    var data = ParseFormData();
    Database.Save(data);
    MessageBox.Show("Saved!");
}
```

### Window Lifecycle

**Allowed in code-behind:**

```csharp
public partial class MyWindow : ModernWindow
{
    public MyWindow(MyViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Focus management only
        FirstInput.Focus();
        Keyboard.Focus(FirstInput);
    }
}
```

---

## 3. ViewModel Rules

### Property Pattern

```csharp
public class MyViewModel : NotifyBase // implements INotifyPropertyChanged
{
    private string _statusText;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}
```

### Command Pattern

```csharp
public ICommand SaveCommand => new RelayCommand(
    execute: () => _service.Save(Data),
    canExecute: () => !IsBusy && IsValid);
```

### Service Access

**Phase 6+ (with DI):**

```csharp
public class MyViewModel
{
    private readonly IDataService _dataService;

    public MyViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }
}
```

**Pre-Phase 6 (without DI):**

```csharp
public class MyViewModel
{
    private readonly IDataService _dataService = ServiceLocator.Get<IDataService>();
}
```

---

## 4. Service Rules

### Singleton Services

Services that manage global state (ThemeManager, RenderModeService) SHOULD be singletons.

```csharp
public sealed class ThemeManager
{
    private static readonly ThemeManager _instance = new ThemeManager();
    public static ThemeManager Instance => _instance;
    private ThemeManager() { }
}
```

### Service Separation

Each service MUST have a single responsibility:

| Service | Responsibility |
|---------|---------------|
| `ThemeManager` | Theme switching, accent, persistence |
| `RenderModeService` | Safe mode detection, caching |
| `BackgroundManager` | Background type management (Phase 3) |
| `AccentGeneratorService` | Accent variant generation (Phase 3) |
| `DialogService` | Dialog creation and management |
| `ToastService` | Toast notification display |

---

## 5. Anti-Patterns

### ❌ God ViewModel

ViewModel with 50+ properties and mixed concerns. Split into multiple ViewModels or use child ViewModels.

### ❌ Service-as-ViewModel

Service implementing `INotifyPropertyChanged`. Services are logic-only; ViewModels handle notification.

### ❌ ViewModel-knows-View

ViewModel referencing UI controls (`TextBox`, `DataGrid`). Use data binding and commands instead.

### ❌ Code-Behind Business Logic

Any file I/O, calculation, or validation in `.xaml.cs` files.

---

## 6. Validation Checklist

- [ ] No business logic in code-behind
- [ ] ViewModels use commands, not direct service calls from events
- [ ] Services do not implement property change notification
- [ ] No ViewModel references UI controls
- [ ] DataContext assigned in view constructor or XAML
- [ ] IDisposable resources cleaned up

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
