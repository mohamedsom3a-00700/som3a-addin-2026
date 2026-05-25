using System;
using System.Collections.Generic;
using System.Linq;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class PluginRegistry : Contracts.IModuleRegistry
    {
        private readonly Dictionary<string, ModuleInfo> _modules = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ModuleManifest> _manifests = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        public string ExpectedContractVersion { get; set; } = "1.0.0";

        public event EventHandler<ModuleStateChangedEventArgs>? ModuleStateChanged;

        public void Register(ModuleManifest manifest)
        {
            if (manifest is null)
                throw new ArgumentNullException(nameof(manifest));

            if (string.IsNullOrWhiteSpace(manifest.Id))
                throw new ArgumentException("Module Id cannot be null or empty.", nameof(manifest));

            if (string.IsNullOrWhiteSpace(manifest.Version))
                throw new ArgumentException("Module version cannot be null or empty.", nameof(manifest));

            if (string.IsNullOrWhiteSpace(manifest.DisplayName))
                throw new ArgumentException("Module display name cannot be null or empty.", nameof(manifest));

            ValidateVersion(manifest);

            if (manifest.Dependencies?.Contains(manifest.Id, StringComparer.OrdinalIgnoreCase) == true)
                throw new InvalidOperationException($"Module '{manifest.Id}' declares a self-referencing dependency.");

            lock (_lock)
            {
                if (_modules.ContainsKey(manifest.Id))
                    throw new InvalidOperationException($"Module '{manifest.Id}' is already registered.");

                _manifests[manifest.Id] = manifest;

                var info = new ModuleInfo(
                    manifest.Id,
                    manifest.Version,
                    manifest.DisplayName,
                    manifest.Description ?? "",
                    ModuleState.Registered,
                    manifest.Capabilities ?? Array.Empty<string>());

                _modules[manifest.Id] = info;
            }
        }

        internal void ValidateResolvedDependencies()
        {
            lock (_lock)
            {
                foreach (var manifest in _manifests.Values)
                {
                    ValidateDependencies(manifest);
                    DetectCircularDependencies(manifest);
                }
            }
        }

        public ModuleInfo GetModuleInfo(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentNullException(nameof(moduleId));

            lock (_lock)
            {
                if (!_modules.TryGetValue(moduleId, out var info))
                    throw new InvalidOperationException($"Module '{moduleId}' is not registered.");
                return new ModuleInfo(info); // defensive copy
            }
        }

        public IReadOnlyList<ModuleInfo> GetAllModules()
        {
            lock (_lock)
            {
                return _modules.Values.Select(m => new ModuleInfo(m)).ToList().AsReadOnly();
            }
        }

        public IReadOnlyList<ModuleInfo> GetModulesByState(ModuleState state)
        {
            lock (_lock)
            {
                return _modules.Values.Where(m => m.State == state).Select(m => new ModuleInfo(m)).ToList().AsReadOnly();
            }
        }

        public void RetryModule(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentNullException(nameof(moduleId));

            lock (_lock)
            {
                if (!_modules.TryGetValue(moduleId, out var info))
                    throw new InvalidOperationException($"Module '{moduleId}' is not registered.");

                if (info.State != ModuleState.Failed)
                    throw new InvalidOperationException($"Module '{moduleId}' is in state '{info.State}'. Only Failed modules can be retried.");

                var oldState = info.State;
                _modules[moduleId] = info.With(state: ModuleState.Registered, lastError: null);

                RaiseStateChanged(moduleId, oldState, ModuleState.Registered);
            }
        }

        public void UnloadModule(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentNullException(nameof(moduleId));

            lock (_lock)
            {
                if (!_modules.TryGetValue(moduleId, out var info))
                    throw new InvalidOperationException($"Module '{moduleId}' is not registered.");

                if (info.State != ModuleState.Active)
                    throw new InvalidOperationException($"Module '{moduleId}' is in state '{info.State}'. Only Active modules can be unloaded.");

                var oldState = info.State;
                _modules[moduleId] = info.With(state: ModuleState.Unloaded);

                RaiseStateChanged(moduleId, oldState, ModuleState.Unloaded);
            }
        }

        public void DisableModule(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentNullException(nameof(moduleId));

            lock (_lock)
            {
                if (!_modules.TryGetValue(moduleId, out var info))
                    throw new InvalidOperationException($"Module '{moduleId}' is not registered.");

                if (info.State != ModuleState.Active)
                    throw new InvalidOperationException($"Module '{moduleId}' is in state '{info.State}'. Only Active modules can be disabled.");

                var oldState = info.State;
                _modules[moduleId] = info.With(state: ModuleState.Disabled);

                RaiseStateChanged(moduleId, oldState, ModuleState.Disabled);
            }
        }

        internal void TransitionTo(string moduleId, ModuleState newState, string? lastError = null)
        {
            lock (_lock)
            {
                if (!_modules.TryGetValue(moduleId, out var info))
                    throw new InvalidOperationException($"Module '{moduleId}' is not registered.");

                var oldState = info.State;
                _modules[moduleId] = info.With(state: newState, lastError: lastError);

                RaiseStateChanged(moduleId, oldState, newState);
            }
        }

        internal void UpdateDiagnostics(string moduleId, long memoryBytes, long loadTimeMs)
        {
            lock (_lock)
            {
                if (_modules.TryGetValue(moduleId, out var info))
                {
                    _modules[moduleId] = info.With(memoryBytes: memoryBytes, loadTimeMs: loadTimeMs);
                }
            }
        }

        internal ModuleManifest? GetManifest(string moduleId)
        {
            lock (_lock)
                return _manifests.TryGetValue(moduleId, out var m) ? m : null;
        }

        private void ValidateVersion(ModuleManifest manifest)
        {
            if (!string.IsNullOrWhiteSpace(manifest.Version) &&
                !string.IsNullOrWhiteSpace(ExpectedContractVersion) &&
                !string.Equals(manifest.Version, ExpectedContractVersion, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Module '{manifest.Id}' version '{manifest.Version}' does not match expected host contract version '{ExpectedContractVersion}'. " +
                    $"The module needs to be rebuilt against the current contract version.");
            }
        }

        private void ValidateDependencies(ModuleManifest manifest)
        {
            if (manifest.Dependencies is null || manifest.Dependencies.Length == 0)
                return;

            var dependencySet = new HashSet<string>(manifest.Dependencies, StringComparer.OrdinalIgnoreCase);

            if (dependencySet.Contains(manifest.Id))
                throw new InvalidOperationException($"Module '{manifest.Id}' declares a self-referencing dependency.");

            lock (_lock)
            {
                var missing = new List<string>();
                foreach (var depId in manifest.Dependencies)
                {
                    if (!_modules.ContainsKey(depId) && !_manifests.ContainsKey(depId))
                        missing.Add(depId);
                }

                if (missing.Count > 0)
                    throw new InvalidOperationException(
                        $"Module '{manifest.Id}' has missing dependencies: {string.Join(", ", missing)}. " +
                        $"All dependency modules must be registered first.");
            }

            DetectCircularDependencies(manifest);
        }

        private void DetectCircularDependencies(ModuleManifest manifest)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var recursionStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            visited.Add(manifest.Id);
            recursionStack.Add(manifest.Id);

            foreach (var depId in manifest.Dependencies ?? Array.Empty<string>())
            {
                if (HasCycle(depId, visited, recursionStack))
                {
                    throw new InvalidOperationException(
                        $"Circular dependency detected involving module '{manifest.Id}' and dependency '{depId}'. " +
                        $"The dependency graph must be acyclic.");
                }
            }
        }

        private bool HasCycle(string moduleId, HashSet<string> visited, HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(moduleId))
                return true;

            if (visited.Contains(moduleId))
                return false;

            visited.Add(moduleId);
            recursionStack.Add(moduleId);

            string[]? deps;
            lock (_lock)
            {
                if (!_manifests.TryGetValue(moduleId, out var manifest) || manifest.Dependencies is null)
                    deps = null;
                else
                    deps = manifest.Dependencies.ToArray();
            }

            if (deps is not null)
            {
                foreach (var depId in deps)
                {
                    if (HasCycle(depId, visited, recursionStack))
                        return true;
                }
            }

            recursionStack.Remove(moduleId);
            return false;
        }

        private void RaiseStateChanged(string moduleId, ModuleState oldState, ModuleState newState)
        {
            ModuleStateChanged?.Invoke(this, new ModuleStateChangedEventArgs(moduleId, oldState, newState));
        }
    }
}
