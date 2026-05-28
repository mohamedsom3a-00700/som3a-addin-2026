using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Som3a_WPF_UI.ViewModels
{
    public class RelationshipGeneratorViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private CancellationTokenSource? _cts;
        private bool _isBusy;
        private bool _canCancel;
        private string _statusText = "Ready";
        private string _generationStatus = "Click 'Generate Relationships' to start.";
        private string _validationSummary = "Validation results will appear here after generation.";
        private string _analysisSummary = "Parallel execution and critical path analysis will appear here.";
        private ValidationReport? _currentReport;

        public ObservableCollection<Relationship> Relationships { get; } = new();
        public ObservableCollection<ActivityItem> SourceActivities { get; } = new();
        public ObservableCollection<NetworkValidationIssue> ValidationIssues { get; } = new();
        public ObservableCollection<ParallelExecutionGroup> ParallelGroups { get; } = new();
        public ObservableCollection<ResourceConflict> ResourceConflicts { get; } = new();
        public CriticalPathResult? CriticalPath { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool CanCancel
        {
            get => _canCancel;
            set => SetProperty(ref _canCancel, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value ?? "");
        }

        public string GenerationStatus
        {
            get => _generationStatus;
            set => SetProperty(ref _generationStatus, value);
        }

        public string ValidationSummary
        {
            get => _validationSummary;
            set => SetProperty(ref _validationSummary, value);
        }

        public string AnalysisSummary
        {
            get => _analysisSummary;
            set => SetProperty(ref _analysisSummary, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AcceptAllCommand { get; }
        public ICommand RejectAllCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand AnalyzeCommand { get; }

        public event Action<bool>? BusyChanged;
        public event Action<bool>? CanCancelChanged;

        public RelationshipGeneratorViewModel(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            GenerateCommand = new RelayCommand(async () => await GenerateAsync(), () => !IsBusy);
            CancelCommand = new RelayCommand(CancelGeneration, () => CanCancel);
            AcceptAllCommand = new RelayCommand(AcceptAll, () => Relationships.Count > 0 && !IsBusy);
            RejectAllCommand = new RelayCommand(RejectAll, () => Relationships.Count > 0 && !IsBusy);
            ValidateCommand = new RelayCommand(async () => await ValidateAsync(), () => Relationships.Count > 0 && !IsBusy);
            AnalyzeCommand = new RelayCommand(async () => await AnalyzeAsync(), () => Relationships.Count > 0 && !IsBusy);
        }

        public void LoadSourceActivities(System.Collections.Generic.IEnumerable<ActivityItem> activities)
        {
            SourceActivities.Clear();
            foreach (var a in activities)
                SourceActivities.Add(a);

            GenerationStatus = $"Loaded {SourceActivities.Count} activities. Ready to generate relationships.";
            StatusText = $"{SourceActivities.Count} activities loaded.";
        }

        private async Task GenerateAsync()
        {
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            SetBusyState(true);
            CanCancel = true;
            CanCancelChanged?.Invoke(true);
            StatusText = "Generating relationships...";
            GenerationStatus = "Building activity context...";

            try
            {
                await Task.Yield();

                var genService = _container.Resolve<IRelationshipGenerationService>();
                var activities = SourceActivities.ToList();
                var progress = new Progress<GenerationProgress>(p =>
                {
                    GenerationStatus = p.StatusMessage;
                });

                var relationships = await genService.GenerateRelationshipsAsync(activities, progress, ct);

                Relationships.Clear();
                foreach (var r in relationships)
                    Relationships.Add(r);

                GenerationStatus = $"Generated {Relationships.Count} relationships from {activities.Count} activities.";
                StatusText = $"Generated {Relationships.Count} relationships. Review and accept/reject each one.";

                await ValidateAsync();
            }
            catch (OperationCanceledException)
            {
                GenerationStatus = "Generation was cancelled.";
                StatusText = "Cancelled.";
            }
            catch (Exception ex)
            {
                GenerationStatus = "Error during generation.";
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                CanCancel = false;
                CanCancelChanged?.Invoke(false);
                SetBusyState(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async Task ValidateAsync()
        {
            var network = new RelationshipNetwork
            {
                Relationships = Relationships.ToList(),
                Activities = SourceActivities.ToList()
            };

            try
            {
                var validationService = _container.Resolve<IRelationshipValidationService>();
                var report = await validationService.ValidateNetworkAsync(network);

                _currentReport = report;
                ValidationIssues.Clear();
                foreach (var issue in report.Issues)
                    ValidationIssues.Add(issue);

                ValidationSummary = report.HasErrors
                    ? $"Validation found {report.TotalIssueCount} issue(s) including errors."
                    : $"Validation passed with {report.TotalIssueCount} warning(s).";

                StatusText = ValidationSummary;
            }
            catch (Exception ex)
            {
                ValidationSummary = "Validation error: " + ex.Message;
            }
        }

        private async Task AnalyzeAsync()
        {
            var network = new RelationshipNetwork
            {
                Relationships = Relationships.ToList(),
                Activities = SourceActivities.ToList()
            };

            try
            {
                var analysisService = _container.Resolve<IRelationshipAnalysisService>();
                var groups = await analysisService.AnalyzeParallelGroupsAsync(network);
                var criticalPath = await analysisService.AnalyzeCriticalPathAsync(network);
                var conflicts = await analysisService.DetectResourceConflictsAsync(network);

                ParallelGroups.Clear();
                foreach (var g in groups)
                    ParallelGroups.Add(g);

                ResourceConflicts.Clear();
                foreach (var c in conflicts)
                    ResourceConflicts.Add(c);

                CriticalPath = criticalPath;

                AnalysisSummary = $"Found {groups.Count} parallel groups, critical path has {criticalPath.CriticalActivities} activities";
                StatusText = AnalysisSummary;
            }
            catch (Exception ex)
            {
                AnalysisSummary = "Analysis error: " + ex.Message;
            }
        }

        private void CancelGeneration()
        {
            _cts?.Cancel();
            GenerationStatus = "Cancelling...";
            StatusText = "Cancelling...";
        }

        private void AcceptAll()
        {
            foreach (var r in Relationships)
            {
                r.IsAccepted = true;
                r.IsUserModified = true;
            }
            RefreshRelationshipsView();
            StatusText = $"Accepted all {Relationships.Count} relationships.";
        }

        private void RejectAll()
        {
            foreach (var r in Relationships)
            {
                r.IsAccepted = false;
                r.IsUserModified = true;
            }
            RefreshRelationshipsView();
            StatusText = $"Rejected all {Relationships.Count} relationships.";
        }

        private void RefreshRelationshipsView()
        {
            var snapshot = Relationships.ToList();
            Relationships.Clear();
            foreach (var r in snapshot)
                Relationships.Add(r);
        }

        private void SetBusyState(bool busy)
        {
            IsBusy = busy;
            BusyChanged?.Invoke(busy);
            ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AcceptAllCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RejectAllCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ValidateCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AnalyzeCommand).RaiseCanExecuteChanged();
        }
    }
}
