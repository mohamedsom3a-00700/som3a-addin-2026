using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed partial class ProjectAnalysisViewModel : ViewModelBase
    {
        private readonly object _excelApp;
        private readonly Window _win;
        private readonly ExcelProjectAnalysisService _svc;

        public ObservableCollection<string> AvailableSheets { get; } = new();
        public ObservableCollection<ProjectAnalysisSummaryRow> SummaryRows { get; } = new();
        public ObservableCollection<ProjectAnalysisLogRow> LogRows { get; } = new();
        public ObservableCollection<SheetCheckRow> SheetChecks { get; } = new();

        [ObservableProperty]
        private string? _selectedActivitiesSheet;

        partial void OnSelectedActivitiesSheetChanged(string? value)
        {
            AutoAnalyzeIfNeeded();
        }

        [ObservableProperty]
        private string? _selectedLogicSheet;

        partial void OnSelectedLogicSheetChanged(string? value)
        {
            AutoAnalyzeIfNeeded();
        }

        [ObservableProperty]
        private string? _selectedResourcesSheet;

        partial void OnSelectedResourcesSheetChanged(string? value)
        {
            AutoAnalyzeIfNeeded();
        }

        [ObservableProperty]
        private bool _inProSplit;

        partial void OnInProSplitChanged(bool value)
        {
            RefreshCanRun();
        }

        [ObservableProperty]
        private bool _inProRela;

        partial void OnInProRelaChanged(bool value)
        {
            RefreshCanRun();
        }

        [ObservableProperty]
        private bool _inProRes;

        partial void OnInProResChanged(bool value)
        {
            RefreshCanRun();
        }

        [ObservableProperty]
        private string _finishDateText = "";

        partial void OnFinishDateTextChanged(string value)
        {
            RefreshCanRun();
        }

        [ObservableProperty]
        private string _statusText = "Not validated. Click 'Check Sheets' to validate.";

        [ObservableProperty]
        private double _progressPct;

        [ObservableProperty]
        private string _progressText = "Ready.";

        private bool _validationOK;
        public bool CanRun => _validationOK;

        public ProjectAnalysisViewModel(IServiceContainer container, object excelApp, Window win, ExcelProjectAnalysisService service)
        {
            _excelApp = excelApp;
            _win = win;
            _svc = service ?? throw new ArgumentNullException(nameof(service));

            LoadSheets();
            AutoSelectDefaults();
            Validate();
            SetProgress(0, "Ready.");
        }

        private void LoadSheets()
        {
            AvailableSheets.Clear();
            foreach (var name in _svc.GetWorksheetNames())
                AvailableSheets.Add(name);

            Log("Sheets loaded.");
        }

        private void AutoSelectDefaults()
        {
            if (AvailableSheets.Contains("TASK")) SelectedActivitiesSheet = "TASK";
            if (AvailableSheets.Contains("TASKPRED")) SelectedLogicSheet = "TASKPRED";
            if (AvailableSheets.Contains("TASKRSRC")) SelectedResourcesSheet = "TASKRSRC";
        }

        private void RefreshCanRun()
        {
            RunCommand.NotifyCanExecuteChanged();
        }

        private void SetProgress(double pct, string text)
        {
            ProgressPct = pct;
            ProgressText = text;
        }

        private void Log(string msg, string level = "INFO")
        {
            LogRows.Add(new ProjectAnalysisLogRow
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Level = level.ToUpperInvariant(),
                Message = msg
            });
        }

        [RelayCommand]
        private void ClearSummary()
        {
            SummaryRows.Clear();
            Log("Summary cleared by user.");
        }

        [RelayCommand]
        private void SetToday()
        {
            FinishDateText = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        [RelayCommand]
        private void Close()
        {
            _win?.Close();
        }

        private void AutoAnalyzeIfNeeded()
        {
            // نفس فكرة VBA: Analyze on combo change
            // هنخليها safe: لو شيت متحدد، نعمل analyze مختصر
            try
            {
                if (!string.IsNullOrWhiteSpace(SelectedActivitiesSheet))
                    AppendAnalyze("status_code", SelectedActivitiesSheet!, 3);

                if (!string.IsNullOrWhiteSpace(SelectedLogicSheet))
                    AppendAnalyze("pred_type", SelectedLogicSheet!, 3);

                if (!string.IsNullOrWhiteSpace(SelectedResourcesSheet))
                    AppendAnalyze("rsrc_type", SelectedResourcesSheet!, 3);
            }
            catch (Exception ex)
            {
                Log($"Auto analyze error: {ex.Message}", "WARN");
            }
        }

        private void AppendAnalyze(string header, string sheet, int startRow)
        {
            var counts = _svc.AnalyzeColumnCounts(sheet, header, startRow);
            var details = string.Join(", ", counts.Take(8).Select(kv => $"{kv.Key}={kv.Value}"));
            if (counts.Count > 8) details += " ...";

            SummaryRows.Add(new ProjectAnalysisSummaryRow
            {
                Type = header,
                Count = counts.Sum(x => x.Value),
                SheetName = sheet,
                Details = details
            });

            Log($"Header '{header}' analyzed on sheet '{sheet}'.");
        }

        [RelayCommand]
        private void Validate()
        {
            SheetChecks.Clear();
            _validationOK = false;
            RefreshCanRun();

            if (string.IsNullOrWhiteSpace(SelectedActivitiesSheet) ||
                string.IsNullOrWhiteSpace(SelectedLogicSheet) ||
                string.IsNullOrWhiteSpace(SelectedResourcesSheet))
            {
                StatusText = "Select Activities/Relationships/Resources sheets first.";
                Log("Validation failed: missing sheet selection.", "WARN");
                return;
            }

            var report = _svc.ValidateRequiredHeaders(
                SelectedActivitiesSheet!, SelectedLogicSheet!, SelectedResourcesSheet!);

            foreach (var row in report.Rows)
                SheetChecks.Add(row);

            _validationOK = report.IsOk;
            StatusText = report.IsOk ? "Validation OK. You can run analysis." : "Validation Failed. Fix missing headers.";

            Log(report.IsOk ? "Validation OK." : "Validation FAILED.", report.IsOk ? "INFO" : "ERROR");
            RefreshCanRun();
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private void Run()
        {
            try
            {
                if (!_validationOK)
                {
                    MessageBox.Show("Please click 'Check Sheets' and ensure validation is successful before running analysis.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (InProRela && !InProSplit)
                {
                    MessageBox.Show("To generate the new relationships sheet, please check 'inpro_split' first (activities split must run before relationships).",
                        "Dependency", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                DateTime finishDate = default;
                if (InProSplit)
                {
                    if (!DateTime.TryParse(FinishDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out finishDate))
                    {
                        MessageBox.Show("Please enter a valid Finish date (e.g., 2025-12-31).",
                            "Finish Date", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }

                SetProgress(5, "Starting analysis...");
                Log("Run Analysis started.");

                // Summary refresh like VBA does at start
                AppendAnalyze("status_code", SelectedActivitiesSheet!, 3);
                AppendAnalyze("pred_type", SelectedLogicSheet!, 3);

                var activitiesUpName = SelectedActivitiesSheet! + " up";

                if (InProSplit)
                {
                    SetProgress(30, "Creating activities split sheet...");
                    _svc.CreateInProgressSplit_KeepOriginal(
                        activitiesSheetName: SelectedActivitiesSheet!,
                        startRow: 3,
                        suffix: " up",
                        finishDate: finishDate
                    );
                    Log("Activities split created.");
                }

                if (InProRela)
                {
                    SetProgress(70, "Creating relationships sheet (copy & generate FS)...");
                    _svc.CreateRelationshipsSplitAndGenerateFS(
                        relationshipsSheetName: SelectedLogicSheet!,
                        activitiesSourceSheetName: SelectedActivitiesSheet!,
                        activitiesNewSheetName: activitiesUpName,
                        startRow: 3
                    );
                    Log("Relationships sheet created.");
                }

                if (InProRes)
                {
                    SetProgress(85, "Creating resources up sheet...");
                    _svc.CreateResourcesUp_WithSplitAB(
                        resourcesSheetName: SelectedResourcesSheet!,
                        activitiesSheetName: SelectedActivitiesSheet!,
                        activitiesUpSheetName: activitiesUpName,
                        startRow: 3,
                        suffix: " up",
                        clearExisting: true,
                        copyColWidths: true
                    );
                    Log("Resources up sheet created.");
                }

                SetProgress(100, "Analysis finished.");
                Log("Run Analysis finished.");
            }
            catch (Exception ex)
            {
                StatusText = "Error.";
                Log($"Run error: {ex.Message}", "ERROR");
                MessageBox.Show("Run Analysis error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
