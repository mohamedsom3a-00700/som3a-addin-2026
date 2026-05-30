using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class PerformanceSummaryWidgetViewModel : WidgetViewModel
    {
        private readonly IPerformanceMonitor _performanceMonitor;

        [ObservableProperty]
        private double _startupTimeMs;

        [ObservableProperty]
        private double _lastNavigationTimeMs;

        [ObservableProperty]
        private string _lastNavigationTarget;

        public PerformanceSummaryWidgetViewModel(IPerformanceMonitor performanceMonitor)
        {
            _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
            Title = "Performance";
            Icon = "\U000F0520";

            _performanceMonitor.NavigationCompleted += OnNavigationCompleted;
        }

        protected override Task LoadAsync()
        {
            StartupTimeMs = _performanceMonitor.StartupTimeMs;
            LastNavigationTimeMs = _performanceMonitor.LastNavigationTimeMs;
            LastNavigationTarget = _performanceMonitor.LastNavigationTarget;
            return Task.CompletedTask;
        }

        private void OnNavigationCompleted(object sender, PerformanceMetricsEventArgs e)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                LastNavigationTimeMs = e.NavigationTimeMs;
                LastNavigationTarget = e.TargetKey;
            });
        }

        public override void Cleanup()
        {
            _performanceMonitor.NavigationCompleted -= OnNavigationCompleted;
            base.Cleanup();
        }
    }
}
