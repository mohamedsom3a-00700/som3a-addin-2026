using System;

namespace Som3a_WPF_UI.Contracts
{
    public class ModuleInfo
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ModuleState State { get; set; }
        public string[] Capabilities { get; set; }
        public long MemoryBytes { get; set; }
        public long LoadTimeMs { get; set; }
        public string LastError { get; set; }
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
        public string ModuleId { get; set; }
        public ModuleState OldState { get; set; }
        public ModuleState NewState { get; set; }
    }
}
