using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Data;

namespace Som3a_WPF_UI.ViewModels
{
    public class BOQActivityGeneratorViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private ILoggingService? _logger;
        private BOQContext? _currentContext;
        private CancellationTokenSource? _cts;
        private bool _isBusy;
        private bool _hasBoqLoaded;
        private bool _hasConsented;
        private bool _canCancel;
        private string _generationStatus = "Click 'Generate Activities' to start.";
        private string _statusText = "Ready";
        private IActivityValidationService? _validationService;
        private CancellationTokenSource? _validationCts;
        private Excel.Application? _xlApp;
        private bool _confirmOverwrite;

        public ObservableCollection<BOQItem> BoqItems { get; } = new();
        public ObservableCollection<GeneratedActivity> Activities { get; } = new();
        public ObservableCollection<ActivitySequenceOrder> SequenceOrder { get; } = new();
        public ObservableCollection<ActivityDependency> Dependencies { get; } = new();

        public bool HasBoqLoaded => _currentContext != null;
        public bool HasConsented
        {
            get => _hasConsented;
            set
            {
                if (SetProperty(ref _hasConsented, value))
                    OnPropertyChanged(nameof(CanGenerate));
            }
        }
        public bool CanLoadBoq => !IsBusy;
        public bool CanGenerate => HasConsented && !IsBusy;
        public bool CanExport => Activities.Count > 0 && !IsBusy;
        private bool _includeDependencies = true;
        public bool IncludeDependencies
        {
            get => _includeDependencies;
            set => SetProperty(ref _includeDependencies, value);
        }

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

        // Commands
        public ICommand LoadBoqCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ConsentCommand { get; }
        public ICommand SequenceCommand { get; }
        public ICommand SuggestDependenciesCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand MergeDuplicatesCommand { get; }
        public ICommand RemoveDuplicateCommand { get; }
        public ICommand RemoveActivityCommand { get; }
        public ICommand AcceptAllDependenciesCommand { get; }

        public event Action? RequestClose;
        public event Action<bool>? BusyChanged;
        public event Action<bool>? CanCancelChanged;

        public BOQActivityGeneratorViewModel(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            LoadBoqCommand = new RelayCommand(async () => await LoadBoqAsync(), () => CanLoadBoq);
            GenerateCommand = new RelayCommand(async () => await GenerateAsync(), () => CanGenerate);
            CancelCommand = new RelayCommand(CancelGeneration, () => CanCancel);
            SequenceCommand = new RelayCommand(async () => await SequenceAsync(), () => Activities.Count > 0 && !IsBusy);
            SuggestDependenciesCommand = new RelayCommand(async () => await SuggestDependenciesAsync(), () => SequenceOrder.Count > 0 && !IsBusy);
            ExportCommand = new RelayCommand(async () => await ExportAsync(), () => Activities.Count > 0 && !IsBusy);
            ConsentCommand = new RelayCommand(SetConsent);
            MergeDuplicatesCommand = new RelayCommand(MergeDuplicates, () => Activities.Count > 0 && !IsBusy);
            RemoveDuplicateCommand = new RelayCommand(param => RemoveDuplicate(param as GeneratedActivity), _ => Activities.Count > 0 && !IsBusy);
            RemoveActivityCommand = new RelayCommand(param => RemoveActivity(param as GeneratedActivity), _ => Activities.Count > 0 && !IsBusy);
            AcceptAllDependenciesCommand = new RelayCommand(AcceptAllDependencies, () => Dependencies.Count > 0 && !IsBusy);
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            _xlApp = xlApp ?? throw new ArgumentNullException(nameof(xlApp));
            StatusText = $"Connected to {xlApp.ActiveWorkbook?.Name ?? "active workbook"}";
        }

        public void HandleCellEdit(DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is not GeneratedActivity activity)
                return;

            if (e.Column is DataGridBoundColumn boundCol && boundCol.Binding is Binding binding)
            {
                var propertyName = binding.Path?.Path ?? string.Empty;
                var element = e.EditingElement;

                string? newValue = null;
                if (element is System.Windows.Controls.TextBox tb)
                    newValue = tb.Text;

                if (string.IsNullOrEmpty(propertyName))
                    return;

                switch (propertyName)
                {
                    case "Name":
                        if (!activity.IsUserModified)
                        {
                            activity.OriginalName = activity.Name;
                            activity.IsUserModified = true;
                        }
                        activity.Name = newValue ?? string.Empty;
                        break;
                    case "Description":
                        activity.Description = newValue ?? string.Empty;
                        activity.IsUserModified = true;
                        break;
                    case "Quantity":
                        if (decimal.TryParse(newValue, out var qty) && qty >= 0)
                            activity.Quantity = qty;
                        break;
                    case "Unit":
                        activity.Unit = newValue ?? string.Empty;
                        activity.IsUserModified = true;
                        break;
                }

                ScheduleValidation(activity);
            }
        }

        public void RunValidation()
        {
            if (_currentContext == null || Activities.Count == 0)
                return;

            _validationService ??= _container.Resolve<IActivityValidationService>();
            var all = Activities.ToList();
            foreach (var activity in all)
            {
                _validationService.ValidateSingle(activity, all, _currentContext);
            }

            RefreshActivitiesView();
        }

        private void ScheduleValidation(GeneratedActivity activity)
        {
            _validationCts?.Cancel();
            _validationCts = new CancellationTokenSource();
            var ct = _validationCts.Token;
            var delay = Task.Delay(500, ct);

            _ = delay.ContinueWith(async _ =>
            {
                if (ct.IsCancellationRequested) return;
                try
                {
                    _validationService ??= _container.Resolve<IActivityValidationService>();
                    var all = Activities.ToList();
                    _validationService.ValidateSingle(activity, all, _currentContext);
                    RefreshActivitiesView();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ScheduleValidation] Validation failed for activity '{activity?.Name}': {ex.Message}");
                }
            }, ct, TaskContinuationOptions.NotOnCanceled, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void MergeDuplicates()
        {
            if (Activities.Count == 0) return;

            var merged = new List<GeneratedActivity>();
            var groups = Activities
                .GroupBy(a => a.Name?.Trim().ToLowerInvariant())
                .ToList();

            foreach (var group in groups)
            {
                var items = group.ToList();
                if (items.Count == 1)
                {
                    merged.Add(items[0]);
                }
                else
                {
                    var primary = items[0];
                    foreach (var dup in items.Skip(1))
                    {
                        if (dup.BoqReferences != null)
                        {
                            primary.BoqReferences ??= new List<string>();
                            foreach (var refId in dup.BoqReferences)
                            {
                                if (!primary.BoqReferences.Contains(refId, StringComparer.OrdinalIgnoreCase))
                                    primary.BoqReferences.Add(refId);
                            }
                        }
                        if (!string.IsNullOrEmpty(dup.Description) && string.IsNullOrEmpty(primary.Description))
                            primary.Description = dup.Description;
                    }
                    merged.Add(primary);
                }
            }

            Activities.Clear();
            foreach (var a in merged)
                Activities.Add(a);

            RunValidation();
            StatusText = $"Merged into {Activities.Count} unique activities.";
        }

        public void RemoveDuplicate(GeneratedActivity activity)
        {
            if (activity == null) return;
            Activities.Remove(activity);
            RunValidation();
        }

        public void RemoveActivity(GeneratedActivity? activity)
        {
            if (activity == null) return;
            Activities.Remove(activity);
            RunValidation();
        }

        public void AcceptAllDependencies()
        {
            foreach (var dep in Dependencies)
                dep.IsAccepted = true;
            RefreshDependenciesView();
            StatusText = $"Accepted all {Dependencies.Count} suggested dependencies.";
        }

        private void RefreshDependenciesView()
        {
            var snapshot = Dependencies.ToList();
            Dependencies.Clear();
            foreach (var d in snapshot)
                Dependencies.Add(d);
        }

        private void RefreshActivitiesView()
        {
            var snapshot = Activities.ToList();
            Activities.Clear();
            foreach (var a in snapshot)
                Activities.Add(a);
        }

        private void SetConsent(object? consented)
        {
            if (consented is bool b && b)
            {
                HasConsented = true;
                StatusText = "Data privacy consent accepted. Ready to generate.";
            }
            else
            {
                StatusText = "Consent declined. AI generation is unavailable.";
            }
        }

        private async Task LoadBoqAsync()
        {
            if (IsBusy) return;
            SetBusyState(true);
            StatusText = "Reading BOQ data from workbook...";
            try
            {
                await Task.Yield();
                var contextBuilder = _container.Resolve<IBOQContextBuilder>();
                var context = await contextBuilder.BuildContextAsync(CancellationToken.None);
                _currentContext = context;

                BoqItems.Clear();
                foreach (var item in context.Items)
                    BoqItems.Add(item);

                OnPropertyChanged(nameof(HasBoqLoaded));
                StatusText = $"Loaded {context.ItemCount} BOQ items from '{context.SheetName}'.";
                GenerationStatus = $"Loaded {context.ItemCount} BOQ items. Ready to generate.";
            }
            catch (Exception ex)
            {
                StatusText = "Error loading BOQ: " + ex.Message;
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private async Task GenerateAsync()
        {
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            SetBusyState(true);
            CanCancel = true;
            CanCancelChanged?.Invoke(true);
            GenerationStatus = "Reading BOQ data from workbook...";
            StatusText = "Generating activities...";

            try
            {
                await Task.Yield();

                if (_currentContext == null)
                {
                    var contextBuilder = _container.Resolve<IBOQContextBuilder>();
                    var context = await contextBuilder.BuildContextAsync(ct);
                    _currentContext = context;

                    BoqItems.Clear();
                    foreach (var item in context.Items)
                        BoqItems.Add(item);
                }

                OnPropertyChanged(nameof(HasBoqLoaded));
                GenerationStatus = $"Found {_currentContext.ItemCount} BOQ items. Generating activities...";

                var generationService = _container.Resolve<IActivityGenerationService>();
                var progress = new Progress<GenerationProgress>(p =>
                {
                    GenerationStatus = p.StatusMessage;
                });

                var activities = await generationService.GenerateActivitiesAsync(_currentContext, progress, ct);

                Activities.Clear();
                foreach (var a in activities)
                    Activities.Add(a);

                RunValidation();

                GenerationStatus = $"Generated {Activities.Count} activities.";
                StatusText = $"Generated {Activities.Count} activities from {_currentContext.ItemCount} BOQ items.";

                _logger ??= _container.Resolve<ILoggingService>();
                _logger.Log("Info", "BOQActivityGenerator", $"Generated {Activities.Count} activities from {_currentContext.ItemCount} BOQ items. Sheet: {_currentContext.SheetName}");
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
                _logger ??= _container.Resolve<ILoggingService>();
                _logger.Log("Error", "BOQActivityGenerator", ex.Message, "Generation", ex.ToString());
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

        private void CancelGeneration()
        {
            _cts?.Cancel();
            GenerationStatus = "Cancelling generation...";
            StatusText = "Cancelling...";
        }

        private async Task SequenceAsync()
        {
            SetBusyState(true);
            StatusText = "Sequencing activities...";
            try
            {
                await Task.Yield();

                var sequencingService = _container.Resolve<IActivitySequencingService>();
                var activities = Activities.ToList();
                var sequence = await sequencingService.SequenceAsync(activities);

                SequenceOrder.Clear();
                foreach (var s in sequence)
                    SequenceOrder.Add(s);

                Dependencies.Clear();

                StatusText = $"Sequenced {SequenceOrder.Count} activities.";
            }
            catch (Exception ex)
            {
                StatusText = "Sequencing error: " + ex.Message;
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private async Task SuggestDependenciesAsync()
        {
            SetBusyState(true);
            StatusText = "Suggesting dependencies...";
            try
            {
                await Task.Yield();

                var sequencingService = _container.Resolve<IActivitySequencingService>();
                var activities = Activities.ToList();
                var dependencies = await sequencingService.SuggestDependenciesAsync(activities);

                Dependencies.Clear();
                foreach (var d in dependencies)
                    Dependencies.Add(d);

                StatusText = $"Suggested {Dependencies.Count} dependencies.";
            }
            catch (Exception ex)
            {
                StatusText = "Dependency suggestion error: " + ex.Message;
            }
            finally
            {
                SetBusyState(false);
            }
        }

        public bool ConfirmOverwrite { get => _confirmOverwrite; set => SetProperty(ref _confirmOverwrite, value); }

        private async Task ExportAsync()
        {
            SetBusyState(true);
            StatusText = "Exporting to Excel...";
            try
            {
                await Task.Yield();

                var exportService = _container.Resolve<IActivityExportService>();
                var config = new ActivityExportConfig
                {
                    TargetSheetName = "Generated Activities",
                    IncludeDependencies = IncludeDependencies,
                    OverwriteExisting = false
                };

                if (exportService.SheetExists(config.TargetSheetName) && !ConfirmOverwrite)
                {
                    StatusText = $"Sheet '{config.TargetSheetName}' already exists. Export cancelled. Click Export again to overwrite.";
                    return;
                }
                config.OverwriteExisting = ConfirmOverwrite;

                var activities = Activities.ToList();
                await exportService.ExportAsync(activities, config);

                ConfirmOverwrite = false;
                StatusText = "Exported successfully to 'Generated Activities' sheet.";

                _logger ??= _container.Resolve<ILoggingService>();
                _logger.Log("Info", "BOQActivityGenerator", $"Exported {activities.Count} activities to '{config.TargetSheetName}' sheet.");
            }
            catch (Exception ex)
            {
                StatusText = "Export error: " + ex.Message;
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private void SetBusyState(bool busy)
        {
            IsBusy = busy;
            BusyChanged?.Invoke(busy);
            ((RelayCommand)LoadBoqCommand).RaiseCanExecuteChanged();
            ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SequenceCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SuggestDependenciesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
            ((RelayCommand)MergeDuplicatesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveActivityCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AcceptAllDependenciesCommand).RaiseCanExecuteChanged();
        }
    }
}
