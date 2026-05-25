/// <summary>
/// Provides module initialization capabilities: service resolution, navigation registration,
/// ribbon action registration, and command registration.
/// </summary>
public interface IModuleInitializationContext
{
    /// <summary>Resolves a service from the global service container.</summary>
    T ResolveService<T>() where T : class;

    /// <summary>Gets the navigation registrar for registering pages.</summary>
    INavigationRegistrar Navigation { get; }

    /// <summary>Gets the ribbon registrar for registering ribbon actions.</summary>
    IRibbonRegistrar Ribbon { get; }

    /// <summary>Gets the command registrar for registering commands.</summary>
    ICommandRegistrar Commands { get; }
}
