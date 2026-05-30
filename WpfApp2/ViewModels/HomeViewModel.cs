using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels.Dashboard;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class HomeViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private readonly INavigationService _navigationService;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isErrorVisible;

        public ObservableCollection<WidgetViewModel> Widgets { get; } = new ObservableCollection<WidgetViewModel>();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set => SetProperty(ref _isErrorVisible, value);
        }

        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        public ICommand NavigateToDiagnosticsCommand { get; }
        public ICommand WidgetClickCommand { get; }

        public HomeViewModel(IServiceContainer container, INavigationService navigationService)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            NavigateToDiagnosticsCommand = new RelayCommand(() =>
            {
                try { navigationService.NavigateTo("diagnostics"); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"NavigateToDiagnostics failed: {ex.Message}"); }
            });

            WidgetClickCommand = new RelayCommand(param =>
            {
                var title = param as string;
                if (string.IsNullOrEmpty(title)) return;
                var navKey = title switch
                {
                    "Current Version" => "diagnostics",
                    "Latest Updates" => "settings.general",
                    "Recent Tools" => "home",
                    "Recent Projects" => "home",
                    "Diagnostics Summary" => "diagnostics",
                    "AI Providers" => "settings.general",
                    "Performance Summary" => "diagnostics",
                    "Quick Actions" => "welcome",
                    "Plugin Status" => "diagnostics",
                    _ => null
                };
                if (navKey == null) return;
                try { navigationService.NavigateTo(navKey); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"WidgetClick failed: {ex.Message}"); }
            });
        }

        public async Task LoadAsync()
        {
            if (Widgets.Count > 0) return;

            IsLoading = true;
            try
            {
                var versionWidget = new VersionWidgetViewModel();
                var updatesWidget = new UpdatesWidgetViewModel(_container.Resolve<IChangelogService>());
                var recentToolsWidget = new RecentToolsWidgetViewModel(
                    _container.Resolve<IRecentItemsService>(),
                    _container.Resolve<INavigationService>());
                var recentProjectsWidget = new RecentProjectsWidgetViewModel(
                    _container.Resolve<IRecentItemsService>());
                var diagnosticsWidget = new DiagnosticsSummaryWidgetViewModel(
                    _container.Resolve<IDiagnosticsService>());
                var aiWidget = new AIProviderStatusWidgetViewModel(
                    _container.Resolve<Som3a.Bridge.IAIBridge>(),
                    _container.Resolve<INavigationService>());
                var performanceWidget = new PerformanceSummaryWidgetViewModel(
                    _container.Resolve<IPerformanceMonitor>());
                var quickActionsWidget = new QuickActionsWidgetViewModel(
                    _container.Resolve<INavigationService>());
                var pluginWidget = new PluginStatusWidgetViewModel(
                    _container.Resolve<Som3a_WPF_UI.Contracts.IModuleRegistry>(),
                    _container.Resolve<INavigationService>());

                Widgets.Add(versionWidget);
                Widgets.Add(updatesWidget);
                Widgets.Add(recentToolsWidget);
                Widgets.Add(recentProjectsWidget);
                Widgets.Add(diagnosticsWidget);
                Widgets.Add(aiWidget);
                Widgets.Add(performanceWidget);
                Widgets.Add(quickActionsWidget);
                Widgets.Add(pluginWidget);

                var loadTasks = Widgets.Select(w => w.LoadDataAsync());
                await Task.WhenAll(loadTasks);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Cleanup()
        {
            foreach (var widget in Widgets)
                widget.Cleanup();
            Widgets.Clear();
        }
    }
}
