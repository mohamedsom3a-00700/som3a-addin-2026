using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public class ModuleRegisteredEventArgs : EventArgs
    {
        public IModule Module { get; }

        public ModuleRegisteredEventArgs(IModule module)
        {
            Module = module;
        }
    }

    public class ModuleInitializedEventArgs : EventArgs
    {
        public IModule Module { get; }

        public ModuleInitializedEventArgs(IModule module)
        {
            Module = module;
        }
    }

    public interface IModule
    {
        string ModuleId { get; }
        string Name { get; }
        int Priority { get; }

        void Initialize(IServiceContainer container, IEventBus eventBus);
    }

    public interface IModuleRegistry
    {
        void RegisterModule(IModule module);
        void InitializeAll();
        IEnumerable<IModule> GetRegisteredModules();

        event EventHandler<ModuleRegisteredEventArgs> ModuleRegistered;
        event EventHandler<ModuleInitializedEventArgs> ModuleInitialized;
    }

    public sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly List<IModule> _modules = new();
        private readonly IServiceContainer _container;
        private readonly IEventBus _eventBus;
        private bool _initialized;

        public event EventHandler<ModuleRegisteredEventArgs>? ModuleRegistered;
        public event EventHandler<ModuleInitializedEventArgs>? ModuleInitialized;

        public ModuleRegistry(IServiceContainer container, IEventBus eventBus)
        {
            _container = container;
            _eventBus = eventBus;
        }

        public void RegisterModule(IModule module)
        {
            if (_modules.Any(m => m.ModuleId == module.ModuleId))
            {
                throw new InvalidOperationException($"Module already registered: {module.ModuleId} ({module.Name}). ModuleId must be unique.");
            }

            _modules.Add(module);
            ModuleRegistered?.Invoke(this, new ModuleRegisteredEventArgs(module));
        }

        public void InitializeAll()
        {
            if (_initialized) return;
            _initialized = true;

            var ordered = _modules.OrderBy(m => m.Priority).ToList();

            foreach (var module in ordered)
            {
                try
                {
                    module.Initialize(_container, _eventBus);
                    ModuleInitialized?.Invoke(this, new ModuleInitializedEventArgs(module));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Module {module.ModuleId} ({module.Name}) failed to initialize: {ex.Message}");
                }
            }
        }

        public IEnumerable<IModule> GetRegisteredModules()
        {
            return _modules.AsReadOnly();
        }
    }
}
