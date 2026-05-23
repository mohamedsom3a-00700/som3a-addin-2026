# Quickstart: MVVM & Architecture Cleanup

## Adding a New Service

1. Define the service interface and implementation:

```csharp
public interface IUserService
{
    User GetCurrentUser();
}

public class UserService : IUserService
{
    public User GetCurrentUser() { /* ... */ }
}
```

2. Register in the composition root (`App.xaml.cs` or `CompositionRoot.cs`):

```csharp
container.RegisterSingleton<IUserService, UserService>();
// or container.RegisterTransient<IUserService, UserService>();
```

3. Inject into any ViewModel or service via constructor:

```csharp
public class DashboardViewModel
{
    public DashboardViewModel(IUserService userService)
    {
        // userService is automatically provided
    }
}
```

## Publishing and Subscribing to Events

1. Define an event class:

```csharp
public class DataRefreshRequested
{
    public string EntityType { get; set; }
}
```

2. Publish from any component:

```csharp
eventBus.Publish(new DataRefreshRequested { EntityType = "Projects" });
```

3. Subscribe in any component:

```csharp
public class ProjectListViewModel
{
    public ProjectListViewModel(IEventBus eventBus)
    {
        eventBus.Subscribe<DataRefreshRequested>(OnDataRefresh);
    }

    private void OnDataRefresh(DataRefreshRequested e)
    {
        // React to the event
    }
}
```

4. Optional — unsubscribe when no longer needed:

```csharp
// Subscribe returns a token; dispose to unsubscribe
_subscription = eventBus.Subscribe<DataRefreshRequested>(OnDataRefresh);
_subscription.Dispose(); // Unsubscribes
```

## Creating a Module

1. Implement `IModule`:

```csharp
public class ProjectModule : IModule
{
    public string ModuleId => "ProjectModule";
    public string Name => "Project Management";
    public int Priority => 100;

    public void Initialize(IServiceContainer container, IEventBus eventBus)
    {
        container.RegisterSingleton<IProjectService, ProjectService>();
        container.RegisterTransient<IProjectRepository, ProjectRepository>();
        eventBus.Subscribe<DataRefreshRequested>(OnDataRefresh);
    }

    private void OnDataRefresh(DataRefreshRequested e) { /* ... */ }
}
```

2. Register in the composition root:

```csharp
registry.RegisterModule(new ProjectModule());
```

3. Initialize all modules at application startup:

```csharp
registry.InitializeAll();
```

## Moving Business Logic Out of Code-Behind

1. Identify code-behind that contains business logic (data access, calculations, service calls, complex conditionals).
2. Extract the logic into a ViewModel or service class.
3. Add the ViewModel's dependencies as constructor parameters.
4. Register the ViewModel and its dependencies in the service container.
5. Set the ViewModel as the view's `DataContext` (via container resolution).
6. Replace code-behind event handlers with Command bindings in XAML.
7. Remove the extracted logic from code-behind — leave only InitializeComponent and UI-specific code.

## MVVM Compliance Audit Checklist

For each view file, verify:

- [ ] No data access code (database queries, file I/O, network calls) in code-behind
- [ ] No service instantiation with `new` in code-behind
- [ ] No business rules or calculations in code-behind
- [ ] No complex conditional logic (if/else chains, switch statements) in code-behind
- [ ] ViewModel receives all dependencies via constructor injection
- [ ] ViewModel does not reference UI types (Window, Control, MessageBox)
- [ ] ViewModel does not manipulate ResourceDictionaries
- [ ] Commands are used for user actions instead of code-behind event handlers where feasible
