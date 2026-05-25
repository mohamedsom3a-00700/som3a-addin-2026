using System;
using System.Linq;

namespace Som3a_WPF_UI.Contracts
{
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

        internal ModuleInfo(ModuleInfo other)
        {
            Id = other.Id;
            Version = other.Version;
            DisplayName = other.DisplayName;
            Description = other.Description;
            State = other.State;
            Capabilities = other.Capabilities?.ToArray() ?? Array.Empty<string>();
            MemoryBytes = other.MemoryBytes;
            LoadTimeMs = other.LoadTimeMs;
            LastError = other.LastError;
        }

        internal ModuleInfo With(ModuleState? state = null, string? lastError = null, long? memoryBytes = null, long? loadTimeMs = null)
        {
            return new ModuleInfo(Id, Version, DisplayName, Description,
                state ?? State, Capabilities,
                memoryBytes ?? MemoryBytes, loadTimeMs ?? LoadTimeMs,
                lastError ?? LastError);
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
}
