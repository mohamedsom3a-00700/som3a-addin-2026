# IModule Contract

## Purpose

Self-contained feature unit that registers its own services and event subscriptions during application startup. Modules enable independent feature development without modifying shared application code.

## Contract

```csharp
public interface IModule
{
    string ModuleId { get; }
    string Name { get; }
    int Priority { get; } // Default: 100. Lower = initialized first.

    void Initialize(IServiceContainer container, IEventBus eventBus);
}
```

## Behavior

- **Registration**: `Initialize()` is called during application startup. The module registers its services on `container` and any event subscriptions on `eventBus`.
- **Ordering**: Modules initialize in ascending `Priority` order. Modules with the same priority initialize in registration order.
- **Idempotency**: Multiple calls to `Initialize()` on the same module instance must be safe (no duplicate registrations).
- **Error handling**: If a module's `Initialize()` throws, the error is reported and remaining modules still initialize. The application continues with partially loaded modules.
- **No shutdown contract**: Modules do not require an explicit shutdown method. Services and subscriptions are managed by the container and event bus lifetimes.

## Module Registry Contract

```csharp
public interface IModuleRegistry
{
    void RegisterModule(IModule module);
    void InitializeAll();
    IEnumerable<IModule> GetRegisteredModules();

    // Diagnostics
    event EventHandler<ModuleRegisteredEventArgs> ModuleRegistered;
    event EventHandler<ModuleInitializedEventArgs> ModuleInitialized;
}
```

## Behaviors

- **Duplicate ModuleId**: Throws `InvalidOperationException` at registration time
- **Initialization order**: Determined by `Priority`, then registration order for ties
- **InitializeAll**: Called once during application startup. Subsequent calls are no-ops.
- **Observability**: `ModuleRegistered` and `ModuleInitialized` events fire for each operation
