/// <summary>
/// Immutable snapshot combining manifest metadata with current state and diagnostics.
/// Returned by IModuleRegistry — callers receive a defensive copy.
/// </summary>
public class ModuleInfo
{
    public string Id { get; }
    public string Version { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public ModuleState State { get; }
    public string[] Capabilities { get; }
    public long MemoryBytes { get; }
    public long LoadTimeMs { get; }
    public string? LastError { get; }

    public ModuleInfo(string id, string version, string displayName, string description,
        ModuleState state, string[] capabilities, long memoryBytes = 0, long loadTimeMs = 0, string? lastError = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Description = description ?? "";
        State = state;
        Capabilities = capabilities?.ToArray() ?? Array.Empty<string>();
        MemoryBytes = memoryBytes;
        LoadTimeMs = loadTimeMs;
        LastError = lastError;
    }
}

public enum ModuleState
{
    Registered,
    Loading,
    Active,
    Failed,
    Unloaded,
    Disabled
}

public class ModuleStateChangedEventArgs : EventArgs
{
    public string ModuleId { get; }
    public ModuleState OldState { get; }
    public ModuleState NewState { get; }

    public ModuleStateChangedEventArgs(string moduleId, ModuleState oldState, ModuleState newState)
    {
        ModuleId = moduleId ?? throw new ArgumentNullException(nameof(moduleId));
        OldState = oldState;
        NewState = newState;
    }
}
