using System;
using System.Collections.Generic;
using System.Diagnostics;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class ModuleLoadOrchestrator
    {
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _registry;
        private readonly PluginLoader _loader;
        private readonly IServiceContainer _container;
        private readonly Dictionary<string, NavigationRegistrar> _moduleRegistrars = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _pageToModuleMap = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _initializedModules = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Contracts.IModule> _loadedModuleInstances = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();
        private NavigationService? _navigationService;

        public ModuleLoadOrchestrator(
            Som3a_WPF_UI.Contracts.IModuleRegistry registry,
            PluginLoader loader,
            IServiceContainer container)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void SetNavigationService(NavigationService? service)
        {
            _navigationService = service;
        }

        public NavigationService? NavigationService => _navigationService;

        public void OnModulesDiscovered(IReadOnlyList<ModuleManifest> manifests)
        {
            foreach (var manifest in manifests)
            {
                try
                {
                    _registry.Register(manifest);
                    LogLifecycleEvent(manifest.Id, "Discovered", $"Module '{manifest.Id}' v{manifest.Version} discovered.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to register module '{manifest.Id}': {ex.Message}");
                }
            }

            if (_registry is PluginRegistry pluginRegistry)
            {
                try
                {
                    pluginRegistry.ValidateResolvedDependencies();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dependency validation failed: {ex.Message}");
                }
            }
        }

        public Contracts.IModule? EnsureModuleLoaded(string moduleId)
        {
            lock (_lock)
            {
                if (_initializedModules.Contains(moduleId))
                {
                    _loadedModuleInstances.TryGetValue(moduleId, out var loaded);
                    return loaded;
                }

                var info = _registry.GetModuleInfo(moduleId);
                if (info.State != ModuleState.Registered)
                    return null;

                LogLifecycleEvent(moduleId, "LoadStart", $"Loading module '{moduleId}'...");

                try
                {
                    TransitionTo(moduleId, ModuleState.Loading);

                    var stopwatch = Stopwatch.StartNew();
                    var module = _loader.LoadModule(moduleId);
                    stopwatch.Stop();

                    var navReg = new NavigationRegistrar();
                    var ribbonReg = new RibbonRegistrar();
                    var cmdReg = new CommandRegistrar();
                    var context = new ModuleInitializationContext(_container, navReg, ribbonReg, cmdReg);

                    module.Initialize(context);

                    _moduleRegistrars[moduleId] = navReg;

                    foreach (var (pageId, title, pageType) in navReg.RegisteredPages)
                    {
                        var fullKey = $"{moduleId}.{pageId}";
                        _pageToModuleMap[fullKey] = moduleId;

                        try
                        {
                            _navigationService?.RegisterPage(pageType, fullKey, title);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to register page '{fullKey}' to navigation: {ex.Message}");
                        }
                    }

                    var memBefore = GC.GetTotalMemory(false);
                    var memAfter = GC.GetTotalMemory(false);

                    if (_registry is PluginRegistry pluginRegistry)
                        pluginRegistry.UpdateDiagnostics(moduleId, memAfter - memBefore, stopwatch.ElapsedMilliseconds);

                    _initializedModules.Add(moduleId);
                    _loadedModuleInstances[moduleId] = module;

                    TransitionTo(moduleId, ModuleState.Active);
                    LogLifecycleEvent(moduleId, "LoadSuccess", $"Module '{moduleId}' loaded successfully in {stopwatch.ElapsedMilliseconds}ms.");

                    return module;
                }
                catch (Exception ex)
                {
                    TransitionTo(moduleId, ModuleState.Failed, ex.Message);
                    LogLifecycleEvent(moduleId, "LoadFailed", $"Module '{moduleId}' failed to load: {ex.Message}");
                    Debug.WriteLine($"Failed to load module '{moduleId}': {ex.Message}");
                    return null;
                }
            }
        }

        public string? GetModuleIdForPage(string pageKey)
        {
            return _pageToModuleMap.TryGetValue(pageKey, out var moduleId) ? moduleId : null;
        }

        public NavigationRegistrar? GetModuleNavigationRegistrar(string moduleId)
        {
            return _moduleRegistrars.TryGetValue(moduleId, out var reg) ? reg : null;
        }

        public bool IsModulePage(string pageKey)
        {
            return _pageToModuleMap.ContainsKey(pageKey);
        }

        public IReadOnlyList<(string PageId, string Title, Type PageType)> GetModulePages(string moduleId)
        {
            return _moduleRegistrars.TryGetValue(moduleId, out var reg)
                ? reg.RegisteredPages
                : Array.Empty<(string, string, Type)>();
        }

        public bool IsModuleLoaded(string moduleId)
        {
            lock (_lock)
                return _initializedModules.Contains(moduleId);
        }

        private void TransitionTo(string moduleId, ModuleState state, string? error = null)
        {
            if (_registry is PluginRegistry pluginRegistry)
            {
                pluginRegistry.TransitionTo(moduleId, state, error);
                LogLifecycleEvent(moduleId, "StateChanged", $"Module '{moduleId}' transitioned to {state}.");
            }
        }

        private void LogLifecycleEvent(string moduleId, string eventType, string message)
        {
            try
            {
                var loggingService = _container.Resolve<ILoggingService>();
                loggingService?.Log("Info", "ModuleLifecycle", message, source: moduleId);
            }
            catch
            {
                Debug.WriteLine($"[ModuleLifecycle|{eventType}|{moduleId}] {message}");
            }
        }
    }
}
