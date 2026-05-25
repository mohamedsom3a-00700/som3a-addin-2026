using System;
using System.Collections.Generic;
using System.Linq;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class ModuleDiagnosticsSnapshot
    {
        public string ModuleId { get; }
        public ModuleState State { get; }
        public string Version { get; }
        public long MemoryBytes { get; }
        public double MemoryMB => Math.Round(MemoryBytes / (1024.0 * 1024.0), 1);
        public long LoadTimeMs { get; }
        public string? LastError { get; }
        public IReadOnlyList<string> Capabilities { get; }

        public ModuleDiagnosticsSnapshot(string moduleId, ModuleState state, string version,
            long memoryBytes, long loadTimeMs, string? lastError, IEnumerable<string>? capabilities)
        {
            ModuleId = moduleId ?? "";
            State = state;
            Version = version ?? "";
            MemoryBytes = memoryBytes;
            LoadTimeMs = loadTimeMs;
            LastError = lastError;
            var caps = capabilities?.ToArray() ?? Array.Empty<string>();
            Capabilities = Array.AsReadOnly(caps);
        }
    }

    public interface IModuleDiagnosticsService
    {
        IReadOnlyList<ModuleDiagnosticsSnapshot> GetSnapshot();
        void RefreshSnapshot();
        event EventHandler? SnapshotUpdated;
    }

    public class ModuleDiagnosticsService : IModuleDiagnosticsService
    {
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _registry;
        private List<ModuleDiagnosticsSnapshot> _cachedSnapshot = new();

        public ModuleDiagnosticsService(Som3a_WPF_UI.Contracts.IModuleRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _registry.ModuleStateChanged += OnModuleStateChanged;
            RefreshSnapshot();
        }

        public event EventHandler? SnapshotUpdated;

        public IReadOnlyList<ModuleDiagnosticsSnapshot> GetSnapshot()
        {
            return _cachedSnapshot.AsReadOnly();
        }

        public void RefreshSnapshot()
        {
            var modules = _registry.GetAllModules();
            _cachedSnapshot = modules.Select(m => new ModuleDiagnosticsSnapshot(
                m.Id, m.State, m.Version, m.MemoryBytes, m.LoadTimeMs, m.LastError, m.Capabilities
            )).ToList();

            SnapshotUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnModuleStateChanged(object? sender, ModuleStateChangedEventArgs e)
        {
            RefreshSnapshot();
        }
    }
}
