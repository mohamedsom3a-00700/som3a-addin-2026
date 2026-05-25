using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class DiagnosticsViewModel : ViewModelBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly IValidationEngine _validationEngine;
        private readonly ILoggingService _loggingService;
        private readonly DispatcherTimer _refreshTimer;

        private DiagnosticSnapshot _currentSnapshot = new DiagnosticSnapshot();
        private bool _isScanning;
        private bool _isLogViewVisible;
        private string _statusMessage = "Ready";

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

        public ICommand RunValidationCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleLogViewCommand { get; }

        public DiagnosticsViewModel(IDiagnosticsService diagnosticsService, IValidationEngine validationEngine, ILoggingService loggingService)
        {
            _diagnosticsService = diagnosticsService;
            _validationEngine = validationEngine;
            _loggingService = loggingService;

            RunValidationCommand = new RelayCommand(_ => RunValidation(), _ => !IsScanning);
            RefreshCommand = new RelayCommand(_ => RefreshSnapshot());
            ToggleLogViewCommand = new RelayCommand(_ => IsLogViewVisible = !IsLogViewVisible);

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += (_, _) => RefreshSnapshot();

            RefreshSnapshot();
            _refreshTimer.Start();
        }

        public void RefreshSnapshot()
        {
            try
            {
                CurrentSnapshot = _diagnosticsService.CaptureSnapshot();
                StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Snapshot failed: {ex.Message}";
                _loggingService.Log("ERROR", "Diagnostics", $"Failed to capture snapshot: {ex.Message}", "DiagnosticsViewModel");
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

        public void Cleanup()
        {
            _refreshTimer.Stop();
        }
    }
}
