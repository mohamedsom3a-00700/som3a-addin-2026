using System.Collections.Generic;

namespace Som3a_WPF_UI.Contracts
{
    public interface IPluginLoader
    {
        IReadOnlyList<ModuleManifest> DiscoverModules();
        IModule LoadModule(string moduleId);
        string ModulesDirectory { get; }
    }
}
