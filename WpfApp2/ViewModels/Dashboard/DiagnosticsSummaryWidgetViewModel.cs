using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class DiagnosticsSummaryWidgetViewModel : WidgetViewModel
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly EventHandler _refreshTickHandler;

        [ObservableProperty]
        private string _renderMode;

        [ObservableProperty]
        private string _activeTheme;

        [ObservableProperty]
        private double? _memoryUsageMB;

        [ObservableProperty]
        private bool _gpuAvailable;

        public DiagnosticsSummaryWidgetViewModel(IDiagnosticsService diagnosticsService)
        {
            _diagnosticsService = diagnosticsService ?? throw new ArgumentNullException(nameof(diagnosticsService));
            Title = "Diagnostics";
            Icon = "\U000F0209";

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _refreshTickHandler = async (s, e) => await RefreshAsync();
            _refreshTimer.Tick += _refreshTickHandler;

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
            _refreshTimer.Tick -= _refreshTickHandler;
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            base.Cleanup();
        }
    }
}
