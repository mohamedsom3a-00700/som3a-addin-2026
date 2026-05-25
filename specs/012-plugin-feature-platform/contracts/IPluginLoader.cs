/// <summary>
/// Discovers and loads module assemblies from disk.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Scans the Modules directory for module manifests and returns discovered modules.
    /// Does NOT load any assemblies into memory.
    /// </summary>
    IReadOnlyList<ModuleManifest> DiscoverModules();

    /// <summary>
    /// Loads a module assembly into memory by module ID.
    /// Performs SHA256 integrity validation before loading.
    /// Returns the IModule instance after calling Initialize().
    /// </summary>
    IModule LoadModule(string moduleId);

    /// <summary>
    /// Provides the base directory path where module assemblies are stored.
    /// </summary>
    string ModulesDirectory { get; }
}
