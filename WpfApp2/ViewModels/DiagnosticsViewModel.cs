using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class DiagnosticsViewModel : ViewModelBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly IValidationEngine _validationEngine;
        private readonly ILoggingService _loggingService;
        private readonly IModuleDiagnosticsService _moduleDiagnosticsService;
        private readonly DispatcherTimer _refreshTimer;

        private DiagnosticSnapshot _currentSnapshot = new DiagnosticSnapshot();
        private bool _isScanning;
        private bool _isLoading;
        private bool _isLogViewVisible;
        private bool _hasError;
        private string _statusMessage = "Ready";
        private string _errorMessage = string.Empty;

        public DiagnosticSnapshot CurrentSnapshot
        {
            get => _currentSnapshot;
            set => SetProperty(ref _currentSnapshot, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                    OnPropertyChanged(nameof(IsDataAvailable));
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (SetProperty(ref _hasError, value))
                    OnPropertyChanged(nameof(IsDataAvailable));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsDataAvailable => !IsLoading && !HasError && CurrentSnapshot != null;

        public bool IsLogViewVisible
        {
            get => _isLogViewVisible;
            set
            {
                if (SetProperty(ref _isLogViewVisible, value))
                {
                    if (value)
                        LoadRecentLogs();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<ValidationResult> ValidationResults { get; } = new ObservableCollection<ValidationResult>();
        public ObservableCollection<LogEntry> RecentLogs { get; } = new ObservableCollection<LogEntry>();
        public ObservableCollection<ModuleDiagnosticsSnapshot> ModuleSnapshots { get; } = new ObservableCollection<ModuleDiagnosticsSnapshot>();

        public ICommand RunValidationCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleLogViewCommand { get; }

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

            RunValidationCommand = new RelayCommand(_ => RunValidation(), _ => !IsScanning);
            RefreshCommand = new RelayCommand(_ => RefreshSnapshot());
            ToggleLogViewCommand = new RelayCommand(_ => IsLogViewVisible = !IsLogViewVisible);

            _moduleDiagnosticsService.SnapshotUpdated += OnModuleSnapshotUpdated;

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += OnTimerTick;

            RefreshSnapshot();
            _refreshTimer.Start();
        }

        public async void RefreshSnapshot()
        {
            IsLoading = true;
            HasError = false;
            StatusMessage = "Capturing diagnostics...";

            try
            {
                CurrentSnapshot = await System.Threading.Tasks.Task.Run(() => _diagnosticsService.CaptureSnapshot());
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

        public void RunValidation()
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
