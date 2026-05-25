using System;
using System.Collections.Generic;
using System.Linq;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class DiagnosticsSnapshot
    {
        public string ModuleId { get; set; } = "";
        public ModuleState State { get; set; }
        public string Version { get; set; } = "";
        public long MemoryBytes { get; set; }
        public long LoadTimeMs { get; set; }
        public string? LastError { get; set; }
        public string[] Capabilities { get; set; } = Array.Empty<string>();
    }

    public class ModuleDiagnosticsService
    {
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _registry;
        private List<DiagnosticsSnapshot> _cachedSnapshot = new();

        public ModuleDiagnosticsService(Som3a_WPF_UI.Contracts.IModuleRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _registry.ModuleStateChanged += OnModuleStateChanged;
            RefreshSnapshot();
        }

        public event EventHandler? SnapshotUpdated;

        public IReadOnlyList<DiagnosticsSnapshot> GetSnapshot()
        {
            return _cachedSnapshot.AsReadOnly();
        }

        public void RefreshSnapshot()
        {
            var modules = _registry.GetAllModules();
            _cachedSnapshot = modules.Select(m => new DiagnosticsSnapshot
            {
                ModuleId = m.Id,
                State = m.State,
                Version = m.Version,
                MemoryBytes = m.MemoryBytes,
                LoadTimeMs = m.LoadTimeMs,
                LastError = m.LastError,
                Capabilities = m.Capabilities
            }).ToList();

            SnapshotUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnModuleStateChanged(object? sender, ModuleStateChangedEventArgs e)
        {
            RefreshSnapshot();
        }
    }
}
