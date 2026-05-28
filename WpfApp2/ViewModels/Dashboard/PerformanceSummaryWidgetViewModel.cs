using System;
using System.Threading.Tasks;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class PerformanceSummaryWidgetViewModel : WidgetViewModel
    {
        private readonly IPerformanceMonitor _performanceMonitor;
        private double _startupTimeMs;
        private double _lastNavigationTimeMs;
        private string _lastNavigationTarget;

        public double StartupTimeMs
        {
            get => _startupTimeMs;
            set => SetProperty(ref _startupTimeMs, value);
        }

        public double LastNavigationTimeMs
        {
            get => _lastNavigationTimeMs;
            set => SetProperty(ref _lastNavigationTimeMs, value);
        }

        public string LastNavigationTarget
        {
            get => _lastNavigationTarget;
            set => SetProperty(ref _lastNavigationTarget, value);
        }

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
