/// <summary>
/// Central registry for module registration, lifecycle management, and querying.
/// </summary>
public interface IModuleRegistry
{
    /// <summary>Registers a module from its manifest metadata. Assembly is NOT loaded at this point.</summary>
    void Register(ModuleManifest manifest);

    /// <summary>Gets a registered module's metadata and current state.</summary>
    ModuleInfo GetModuleInfo(string moduleId);

    /// <summary>Returns all registered modules.</summary>
    IReadOnlyList<ModuleInfo> GetAllModules();

    /// <summary>Gets modules in a specific state.</summary>
    IReadOnlyList<ModuleInfo> GetModulesByState(ModuleState state);

    /// <summary>Retries a failed module (moves from Failed back to Registered).</summary>
    void RetryModule(string moduleId);

    /// <summary>Unloads an active module.</summary>
    void UnloadModule(string moduleId);

    /// <summary>Disables a module without unloading.</summary>
    void DisableModule(string moduleId);

    /// <summary>Event raised when any module's state changes.</summary>
    event EventHandler<ModuleStateChangedEventArgs> ModuleStateChanged;
}
