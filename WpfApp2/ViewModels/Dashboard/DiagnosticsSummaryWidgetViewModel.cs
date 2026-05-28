using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class DiagnosticsSummaryWidgetViewModel : WidgetViewModel
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly DispatcherTimer _refreshTimer;
        private string _renderMode;
        private string _activeTheme;
        private double? _memoryUsageMB;
        private bool _gpuAvailable;

        public string RenderMode
        {
            get => _renderMode;
            set => SetProperty(ref _renderMode, value);
        }

        public string ActiveTheme
        {
            get => _activeTheme;
            set => SetProperty(ref _activeTheme, value);
        }

        public double? MemoryUsageMB
        {
            get => _memoryUsageMB;
            set => SetProperty(ref _memoryUsageMB, value);
        }

        public bool GpuAvailable
        {
            get => _gpuAvailable;
            set => SetProperty(ref _gpuAvailable, value);
        }

        public DiagnosticsSummaryWidgetViewModel(IDiagnosticsService diagnosticsService)
        {
            _diagnosticsService = diagnosticsService ?? throw new ArgumentNullException(nameof(diagnosticsService));
            Title = "Diagnostics";
            Icon = "\U000F0209";

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _refreshTimer.Tick += async (s, e) => await RefreshAsync();

            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }

        protected override Task LoadAsync()
        {
            var snapshot = _diagnosticsService.CaptureSnapshot();
            UpdateFromSnapshot(snapshot);
            _refreshTimer.Start();
            return Task.CompletedTask;
        }

        private void UpdateFromSnapshot(DiagnosticSnapshot snapshot)
        {
            if (snapshot == null) return;
            RenderMode = snapshot.RenderMode;
            ActiveTheme = snapshot.ActiveTheme;
            MemoryUsageMB = snapshot.MemoryWorkingSetMB;
            GpuAvailable = snapshot.GpuAvailable;
        }

        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            App.Current?.Dispatcher?.Invoke(async () => await RefreshAsync());
        }

        public override void Cleanup()
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= async (s, e) => await RefreshAsync();
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            base.Cleanup();
        }
    }
}
