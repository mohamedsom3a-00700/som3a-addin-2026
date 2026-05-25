/// <summary>
/// Contract that every module must implement.
/// </summary>
public interface IModule
{
    /// <summary>Unique module identifier (e.g., "som3a.project-analysis").</summary>
    string Id { get; }

    /// <summary>Semantic version (e.g., "1.0.0").</summary>
    string Version { get; }

    /// <summary>Human-readable display name for UI.</summary>
    string DisplayName { get; }

    /// <summary>Short description of the module's purpose.</summary>
    string Description { get; }

    /// <summary>
    /// Called by the system after the module assembly is loaded.
    /// The module uses this opportunity to register pages, commands, ribbon actions,
    /// and resolve its own dependencies from the service container.
    /// </summary>
    /// <param name="context">Provides access to service container, navigation registration, ribbon registration, and command registration.</param>
    void Initialize(IModuleInitializationContext context);
}
