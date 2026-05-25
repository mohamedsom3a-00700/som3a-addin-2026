using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Contracts
{
    public interface IModuleRegistry
    {
        void Register(ModuleManifest manifest);
        ModuleInfo GetModuleInfo(string moduleId);
        IReadOnlyList<ModuleInfo> GetAllModules();
        IReadOnlyList<ModuleInfo> GetModulesByState(ModuleState state);
        void RetryModule(string moduleId);
        void UnloadModule(string moduleId);
        void DisableModule(string moduleId);
        event EventHandler<ModuleStateChangedEventArgs> ModuleStateChanged;
    }
}
