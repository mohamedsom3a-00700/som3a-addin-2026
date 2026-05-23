# Data Model: MVVM & Architecture Cleanup

## Service Registration

| Field | Type | Description |
|-------|------|-------------|
| ServiceType | Type | The interface or base type being registered (e.g., `ILogger`) |
| ImplementationType | Type | The concrete type that implements the service (e.g., `FileLogger`) |
| Lifetime | Lifetime | One of: `Singleton` (one instance for lifetime of container), `Transient` (new instance per resolution), `Scoped` (one instance per named scope) |
| Instance (optional) | Object | Pre-created instance for singleton registrations where the caller manages creation |

### Validation Rules

- ServiceType must be an interface or class
- ImplementationType must be a concrete class that can be instantiated
- ImplementationType must implement or inherit from ServiceType
- Duplicate registration of the same ServiceType is an error (override must be explicit)
- Circular dependencies must be detected at resolution time and reported as an error

### State Transitions

```
Registered → (first resolution) → Instance Created → (subsequent resolution within scope/lifetime) → Same Instance Returned
Registered → Resolution Requested → Circular Dependency Detected → Error Reported (no instance returned)
Registered → Resolution Requested → Service Not Found → Error Reported (no instance returned)
```

## Event Bus

| Field | Type | Description |
|-------|------|-------------|
| EventType | Type | The type of event being published (e.g., `DataRefreshRequested`) |
| Subscriber | WeakReference | Reference to the subscriber instance — weak to prevent memory leaks |
| Handler | Delegate | The method to invoke when the event is published |
| Filter (optional) | Func<TEvent, bool> | Optional predicate to filter which events reach this subscriber |

### Validation Rules

- EventType must be a class (events are reference types for consistency)
- Subscriber must be alive at time of delivery — dead subscribers are automatically pruned
- Handler must accept the EventType as its single parameter
- Multiple subscribers for the same EventType are allowed
- Same subscriber registering the same handler twice is idempotent (second registration is ignored)

### State Transitions

```
Publisher → Publish(Event) → Event Bus → Subscriber Alive? → Yes → Invoke Handler
                                         → Subscriber Alive? → No → Prune Subscription (silent)
                                         → No Subscribers → Complete (no-op)
```

## Module

| Field | Type | Description |
|-------|------|-------------|
| ModuleId | String | Unique identifier for the module (e.g., `"ThemeModule"`) |
| Name | String | Human-readable display name |
| Priority | Int32 | Initialization order (lower numbers first). Default: 100. |
| RegisteredServices | List<ServiceRegistration> | Services registered by this module |
| RegisteredEvents | List<EventRegistration> | Event subscriptions registered by this module |

### Validation Rules

- ModuleId must be unique across all registered modules
- Duplicate ModuleId at registration is an error
- Modules cannot unregister other modules' services
- Module initialization order is deterministic (by Priority, then by registration order for equal priorities)

### State Transitions

```
Registered → Initialize() → Services Registered → Event Subscriptions Active → Module Active
Module Active → Application Shutdown → (implicit cleanup — no explicit shutdown required)
```

## ViewModel

| Field | Type | Description |
|-------|------|-------------|
| Services | Injected via constructor | All service dependencies received through the service container |
| Commands | ICommand | User actions exposed as commands for data binding |
| ObservableProperties | INotifyPropertyChanged | Data-bound properties for the view to consume |

### Rules

- ViewModels must receive all dependencies through constructor injection
- ViewModels must not reference UI types (Window, Control, MessageBox etc.)
- ViewModels must not manipulate ResourceDictionaries
- ViewModels must implement INotifyPropertyChanged or use a base class

## View (Code-Behind)

| Field | Type | Description |
|-------|------|-------------|
| DataContext | ViewModel | Set via binding or programmatically in constructor-only |

### Rules

- Code-behind must contain only: constructor + InitializeComponent, event handlers that delegate to ViewModel commands, UI-specific behavior (animation triggers, focus management, clipboard access)
- Code-behind must not contain: data access, business logic, service instantiation, complex conditionals, calculations
- Code-behind must not create service instances with `new` — all services come from the container
