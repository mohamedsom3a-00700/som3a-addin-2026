using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class PluginStatusWidgetViewModel : WidgetViewModel
    {
        private readonly Som3a_WPF_UI.Contracts.IModuleRegistry _moduleRegistry;
        private readonly INavigationService _navigationService;
        private int _totalCount;
        private int _activeCount;
        private int _failedCount;
        private string _healthStatus;

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public int ActiveCount
        {
            get => _activeCount;
            set => SetProperty(ref _activeCount, value);
        }

        public int FailedCount
        {
            get => _failedCount;
            set => SetProperty(ref _failedCount, value);
        }

        public string HealthStatus
        {
            get => _healthStatus;
            set => SetProperty(ref _healthStatus, value);
        }

        public ICommand NavigateToPluginRegistryCommand { get; }

        public PluginStatusWidgetViewModel(
            Som3a_WPF_UI.Contracts.IModuleRegistry moduleRegistry,
            INavigationService navigationService)
        {
            _moduleRegistry = moduleRegistry ?? throw new ArgumentNullException(nameof(moduleRegistry));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Plugins";
            Icon = "\U000F0445";

            NavigateToPluginRegistryCommand = new RelayCommand(() =>
            {
                try { _navigationService.NavigateTo("settings.general"); }
                catch { }
            });

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
