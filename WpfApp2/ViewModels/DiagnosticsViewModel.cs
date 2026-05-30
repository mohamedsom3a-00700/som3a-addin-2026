using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class DiagnosticsViewModel : ViewModelBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly IValidationEngine _validationEngine;
        private readonly ILoggingService _loggingService;
        private readonly IModuleDiagnosticsService _moduleDiagnosticsService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        [ObservableProperty]
        private DiagnosticSnapshot _currentSnapshot = new DiagnosticSnapshot();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
        private bool _isScanning;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isLogViewVisible;

        partial void OnIsLogViewVisibleChanged(bool value)
        {
            if (value)
                LoadRecentLogs();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
        private bool _hasError;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool IsDataAvailable => !IsLoading && !HasError && CurrentSnapshot != null;

        public ObservableCollection<ValidationResult> ValidationResults { get; } = new ObservableCollection<ValidationResult>();
        public ObservableCollection<LogEntry> RecentLogs { get; } = new ObservableCollection<LogEntry>();
        public ObservableCollection<ModuleDiagnosticsSnapshot> ModuleSnapshots { get; } = new ObservableCollection<ModuleDiagnosticsSnapshot>();

        [RelayCommand]
        private void RunValidation()
        {
            if (IsScanning) return;

            IsScanning = true;
            StatusMessage = "Running validation...";

            try
            {
                ValidationResults.Clear();
                var results = _validationEngine.RunValidation();
                foreach (var result in results)
                    ValidationResults.Add(result);

                _loggingService.Log("INFO", "Validation", $"Scan completed: {results.Count} issues found", "DiagnosticsViewModel");
                StatusMessage = $"Validation complete: {results.Count} issues found";
            }
            catch (Exception ex)
            {
                _loggingService.Log("ERROR", "Validation", $"Validation scan failed: {ex.Message}", "DiagnosticsViewModel", ex.ToString());
                StatusMessage = $"Validation failed: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        [RelayCommand]
        public void RefreshSnapshot()
        {
            _ = RefreshSnapshotAsync();
        }

        [RelayCommand]
        private void ToggleLogView()
        {
            IsLogViewVisible = !IsLogViewVisible;
        }

        public DiagnosticsViewModel(
            IDiagnosticsService diagnosticsService,
            IValidationEngine validationEngine,
            ILoggingService loggingService,
            IModuleDiagnosticsService moduleDiagnosticsService)
        {
            _diagnosticsService = diagnosticsService;
            _validationEngine = validationEngine;
            _loggingService = loggingService;
            _moduleDiagnosticsService = moduleDiagnosticsService;

            _moduleDiagnosticsService.SnapshotUpdated += OnModuleSnapshotUpdated;

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += OnTimerTick;

            RefreshSnapshot();
            _refreshTimer.Start();
        }

        public async Task RefreshSnapshotAsync()
        {
            if (!await _refreshSemaphore.WaitAsync(0))
                return;

            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = "Capturing diagnostics...";

                try
                {
                    CurrentSnapshot = await Task.Run(() => _diagnosticsService.CaptureSnapshot());
                    _moduleDiagnosticsService.RefreshSnapshot();
                    StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
                }
                catch (Exception ex)
                {
                    HasError = true;
                    ErrorMessage = ex.Message;
                    StatusMessage = $"Snapshot failed: {ex.Message}";
                    _loggingService.Log("ERROR", "Diagnostics", $"Failed to capture snapshot: {ex.Message}", "DiagnosticsViewModel");
                }
                finally
                {
                    IsLoading = false;
                }
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }

        public void LoadRecentLogs()
        {
            try
            {
                RecentLogs.Clear();
                var entries = _loggingService.GetRecentEntries(50);
                foreach (var entry in entries)
                    RecentLogs.Add(entry);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load logs: {ex.Message}";
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            RefreshSnapshot();
        }

        private void OnModuleSnapshotUpdated(object sender, EventArgs e)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                ModuleSnapshots.Clear();
                var snapshots = _moduleDiagnosticsService.GetSnapshot();
                foreach (var s in snapshots)
                    ModuleSnapshots.Add(s);
            });
        }

        public void Cleanup()
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= OnTimerTick;
            _moduleDiagnosticsService.SnapshotUpdated -= OnModuleSnapshotUpdated;
        }
    }
}
