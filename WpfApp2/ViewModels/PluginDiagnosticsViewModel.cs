using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public class PluginDiagnosticsViewModel : ViewModelBase
    {
        private readonly ModuleDiagnosticsService _diagnosticsService;
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _registry;

        public ObservableCollection<PluginEntryViewModel> Modules { get; } = new();

        public PluginDiagnosticsViewModel(IServiceContainer serviceContainer)
        {
            if (serviceContainer is null)
                throw new ArgumentNullException(nameof(serviceContainer));

            _diagnosticsService = serviceContainer.Resolve<ModuleDiagnosticsService>();
            _registry = serviceContainer.Resolve<Som3a_WPF_UI.Contracts.IModuleRegistry>();
            _diagnosticsService.SnapshotUpdated += (s, e) =>
            {
                if (Application.Current.Dispatcher.CheckAccess())
                    Refresh();
                else
                    Application.Current.Dispatcher.Invoke(Refresh);
            };
            RefreshCommand = new RelayCommand(_ => Refresh());
        }

        public ICommand RefreshCommand { get; }

        public void Refresh()
        {
            Modules.Clear();
            var snapshot = _diagnosticsService.GetSnapshot();
            foreach (var item in snapshot)
            {
                Modules.Add(new PluginEntryViewModel
                {
                    ModuleId = item.ModuleId,
                    Version = item.Version,
                    State = item.State,
                    MemoryBytes = item.MemoryBytes,
                    LoadTimeMs = item.LoadTimeMs,
                    LastError = item.LastError ?? "",
                    Capabilities = string.Join(", ", item.Capabilities ?? Array.Empty<string>()),
                });
            }
        }
    }

    public class PluginEntryViewModel : ViewModelBase
    {
        public string ModuleId { get; set; } = "";
        public string Version { get; set; } = "";
        public ModuleState State { get; set; }
        public long MemoryBytes { get; set; }
        public long LoadTimeMs { get; set; }
        public string LastError { get; set; } = "";
        public string Capabilities { get; set; } = "";

        public string StateDisplay => State switch
        {
            ModuleState.Registered => "Registered",
            ModuleState.Loading => "Loading...",
            ModuleState.Active => "Active",
            ModuleState.Failed => "Failed",
            ModuleState.Unloaded => "Unloaded",
            ModuleState.Disabled => "Disabled",
            _ => State.ToString()
        };

        public string MemoryDisplay => MemoryBytes > 0 ? $"{MemoryBytes / 1024.0:F1} KB" : "N/A";
        public string LoadTimeDisplay => LoadTimeMs > 0 ? $"{LoadTimeMs} ms" : "N/A";
    }
}
