using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class PluginStatusWidgetViewModel : WidgetViewModel
    {
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _moduleRegistry;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _activeCount;

        [ObservableProperty]
        private int _failedCount;

        [ObservableProperty]
        private string _healthStatus;

        public PluginStatusWidgetViewModel(
            Som3a_WPF_UI.Contracts.IModuleRegistry moduleRegistry,
            INavigationService navigationService)
        {
            _moduleRegistry = moduleRegistry ?? throw new ArgumentNullException(nameof(moduleRegistry));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Plugins";
            Icon = "Puzzle";

            _moduleRegistry.ModuleStateChanged += OnModuleStateChanged;
        }

        protected override Task LoadAsync()
        {
            UpdateCounts();
            return Task.CompletedTask;
        }

        private void UpdateCounts()
        {
            try
            {
                var modules = _moduleRegistry.GetAllModules();
                TotalCount = modules.Count;
                ActiveCount = modules.Count(m => m.State == ModuleState.Active);
                FailedCount = modules.Count(m => m.State == ModuleState.Failed);

                if (FailedCount > 0)
                    HealthStatus = "Unhealthy";
                else if (ActiveCount == TotalCount && TotalCount > 0)
                    HealthStatus = "Healthy";
                else
                    HealthStatus = "Degraded";
            }
            catch
            {
                HealthStatus = "Unknown";
            }
        }

        [RelayCommand]
        private void NavigateToPluginRegistry()
        {
            try { _navigationService.NavigateTo("settings.general"); }
            catch { }
        }

        private void OnModuleStateChanged(object sender, ModuleStateChangedEventArgs e)
        {
            App.Current?.Dispatcher?.Invoke(UpdateCounts);
        }

        public override void Cleanup()
        {
            _moduleRegistry.ModuleStateChanged -= OnModuleStateChanged;
            base.Cleanup();
        }
    }
}
