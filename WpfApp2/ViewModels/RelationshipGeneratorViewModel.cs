using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class RelationshipGeneratorViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private CancellationTokenSource? _cts;
        private ValidationReport? _currentReport;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _canCancel;

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        private string _generationStatus = "Click 'Generate Relationships' to start.";

        [ObservableProperty]
        private string _validationSummary = "Validation results will appear here after generation.";

        [ObservableProperty]
        private string _analysisSummary = "Parallel execution and critical path analysis will appear here.";

        public ObservableCollection<Relationship> Relationships { get; } = new();
        public ObservableCollection<ActivityItem> SourceActivities { get; } = new();
        public ObservableCollection<NetworkValidationIssue> ValidationIssues { get; } = new();
        public ObservableCollection<ParallelExecutionGroup> ParallelGroups { get; } = new();
        public ObservableCollection<ResourceConflict> ResourceConflicts { get; } = new();
        public CriticalPathResult? CriticalPath { get; set; }

        public event Action<bool>? BusyChanged;
        public event Action<bool>? CanCancelChanged;

        public RelationshipGeneratorViewModel(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void LoadSourceActivities(System.Collections.Generic.IEnumerable<ActivityItem> activities)
        {
            SourceActivities.Clear();
            foreach (var a in activities)
                SourceActivities.Add(a);

            GenerationStatus = $"Loaded {SourceActivities.Count} activities. Ready to generate relationships.";
            StatusText = $"{SourceActivities.Count} activities loaded.";
        }

        [RelayCommand(CanExecute = nameof(CanGenerate))]
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

        private bool CanGenerate() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void Cancel()
        {
            _cts?.Cancel();
            GenerationStatus = "Cancelling...";
            StatusText = "Cancelling...";
        }

        [RelayCommand(CanExecute = nameof(CanAcceptAll))]
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

        private bool CanAcceptAll() => Relationships.Count > 0 && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanRejectAll))]
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

        private bool CanRejectAll() => Relationships.Count > 0 && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanValidate))]
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

        private bool CanValidate() => Relationships.Count > 0 && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanAnalyze))]
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

        private bool CanAnalyze() => Relationships.Count > 0 && !IsBusy;

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
            GenerateCommand.NotifyCanExecuteChanged();
            AcceptAllCommand.NotifyCanExecuteChanged();
            RejectAllCommand.NotifyCanExecuteChanged();
            ValidateCommand.NotifyCanExecuteChanged();
            AnalyzeCommand.NotifyCanExecuteChanged();
        }
    }
}
