# IServiceContainer Contract

## Purpose

Central service registration and resolution mechanism. All application services are registered here and resolved by dependent components.

## Contract

```csharp
public interface IServiceContainer
{
    // Registration
    void RegisterSingleton<TService, TImplementation>() where TImplementation : TService;
    void RegisterSingleton<TService>(TService instance);
    void RegisterTransient<TService, TImplementation>() where TImplementation : TService;
    void RegisterScoped<TService, TImplementation>() where TImplementation : TService;

    // Resolution
    TService Resolve<TService>();
    object Resolve(Type serviceType);

    // Scope management
    IServiceScope CreateScope();

    // Diagnostics
    event EventHandler<ServiceResolutionEventArgs> ServiceResolved;
    event EventHandler<ServiceRegistrationEventArgs> ServiceRegistered;
}

public interface IServiceScope : IDisposable
{
    TService Resolve<TService>();
    object Resolve(Type serviceType);
}
```

## Behaviors

- **Singleton registration**: One instance created on first resolution, returned for all subsequent resolutions across all scopes
- **Transient registration**: New instance created for every resolution call
- **Scoped registration**: One instance per `IServiceScope`; disposed when the scope is disposed
- **Duplicate registration**: Throws `InvalidOperationException` with message identifying the service type
- **Unregistered service**: Throws `InvalidOperationException` with message identifying the unregistered type
- **Circular dependency**: Throws `InvalidOperationException` at resolution time identifying the circular chain
- **Observability**: `ServiceResolved` and `ServiceRegistered` events fire for each operation, enabling downstream diagnostics
