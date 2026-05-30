using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed partial class SubDailyReportViewModel : ViewModelBase
    {
        private readonly Excel.Application _app;
        private readonly Action _close;
        private readonly SubDlyReportService _svc;

        public ObservableCollection<PrevFileItem> PrevFiles { get; } = new();
        public ObservableCollection<string> PrevSheets { get; } = new();
        public ObservableCollection<string> TodaySheets { get; } = new();
        public ObservableCollection<string> ColumnLetters { get; } = new();
        public ObservableCollection<int> StartRows { get; } = new();

        public ObservableCollection<NamePickItem> NameItems { get; } = new();
        public ObservableCollection<PreviewRow> PreviewRows { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPrevFileSelected))]
        private PrevFileItem? _selectedPrevFile;

        partial void OnSelectedPrevFileChanged(PrevFileItem? value)
        {
            ClearPreviewState();
            LoadPrevSheets();
            RecalcButtons();
        }

        public bool IsPrevFileSelected => SelectedPrevFile != null && !string.IsNullOrEmpty(SelectedPrevFile.DisplayName);

        [ObservableProperty]
        private string? _selectedPrevSheet;

        partial void OnSelectedPrevSheetChanged(string? value)
        {
            ClearPreviewState();
            RecalcButtons();
        }

        [ObservableProperty]
        private string? _selectedTodaySheet;

        partial void OnSelectedTodaySheetChanged(string? value)
        {
            RefreshNames();
            RecalcButtons();
        }

        [ObservableProperty]
        private string? _nameCol;

        partial void OnNameColChanged(string? value)
        {
            RefreshNames();
            RecalcButtons();
        }

        [ObservableProperty]
        private int _nameStartRow = 2;

        partial void OnNameStartRowChanged(int value)
        {
            RefreshNames();
            RecalcButtons();
        }

        [ObservableProperty]
        private string? _countCol;

        partial void OnCountColChanged(string? value)
        {
            ClearPreviewState();
            RecalcButtons();
        }

        [ObservableProperty]
        private int _countStartRow = 2;

        partial void OnCountStartRowChanged(int value)
        {
            ClearPreviewState();
            RecalcButtons();
        }

        [ObservableProperty]
        private double _progressPercent;

        [ObservableProperty]
        private string _statusText = "Ready";

        private bool _hasPreview;
        public bool CanToggleChecks => NameItems.Count > 0;
        public bool CanPreview { get; private set; }
        public bool CanApply => _hasPreview;

        private string _prevTopLeftAddr = "A1";
        private double _prevTotal = 0;
        private System.Collections.Generic.Dictionary<string, (double prev, double today)>? _merged;

        public SubDailyReportViewModel(IServiceContainer container, Excel.Application app, Action close)
        {
            _app = app;
            _close = close;
            _svc = container.Resolve<SubDlyReportService>();

            LoadTodaySheets();
            LoadColumnLettersAndRows();
            RefreshFiles();
        }

        private void LoadTodaySheets()
        {
            TodaySheets.Clear();
            var wb = (Excel.Workbook)_app.ActiveWorkbook;
            foreach (var s in _svc.GetSheetNames(wb)) TodaySheets.Add(s);
            SelectedTodaySheet = TodaySheets.FirstOrDefault();
        }

        private void LoadColumnLettersAndRows()
        {
            ColumnLetters.Clear();
            for (char c = 'A'; c <= 'Z'; c++) ColumnLetters.Add(c.ToString());

            StartRows.Clear();
            for (int r = 2; r <= 1000; r++) StartRows.Add(r);

            NameCol = "A";
            CountCol = "B";
            NameStartRow = 2;
            CountStartRow = 2;
        }

        [RelayCommand]
        private void RefreshFiles()
        {
            PrevFiles.Clear();
            var wb = (Excel.Workbook)_app.ActiveWorkbook;
            foreach (var f in _svc.GetPrevFilesFromTodayFolder(_app, wb))
                PrevFiles.Add(f);

            SelectedPrevFile = null;
            StatusText = PrevFiles.Count == 0 ? "No matching reports found." : $"Found {PrevFiles.Count} report(s).";
        }

        private void LoadPrevSheets()
        {
            PrevSheets.Clear();
            if (SelectedPrevFile == null) return;

            Excel.Workbook? wb = null;
            try
            {
                wb = _svc.OpenReportHidden(_app, SelectedPrevFile.FullPath);
                if (wb == null) return;

                foreach (var s in _svc.GetSheetNames(wb)) PrevSheets.Add(s);
                SelectedPrevSheet = PrevSheets.FirstOrDefault();
            }
            finally
            {
                if (wb != null) wb.Close(false);
            }
        }

        private void RefreshNames()
        {
            NameItems.Clear();
            _hasPreview = false;
            PreviewRows.Clear();
            _merged = null;

            if (string.IsNullOrWhiteSpace(SelectedTodaySheet) || string.IsNullOrWhiteSpace(NameCol) || NameStartRow < 1)
                return;

            var wb = (Excel.Workbook)_app.ActiveWorkbook;
            var ws = (Excel.Worksheet)wb.Worksheets[SelectedTodaySheet];

            int lastRow = ws.Cells[ws.Rows.Count, NameCol].End[Excel.XlDirection.xlUp].Row;
            if (lastRow < NameStartRow) return;

            var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int r = NameStartRow; r <= lastRow; r++)
            {
                string raw = (Convert.ToString(((Excel.Range)ws.Cells[r, NameCol]).Value2) ?? "").Trim();
                string key = SubDlyReportService.NormalizeName(raw);
                if (key.Length == 0) continue;
                if (!seen.Add(key)) continue;

                NameItems.Add(new NamePickItem { IsChecked = true, NameRaw = raw, Row = r, NormKey = key });
            }

            RecalcButtons();
        }

        private void RecalcButtons()
        {
            bool okFile = SelectedPrevFile != null;
            bool okPrevSheet = !string.IsNullOrWhiteSpace(SelectedPrevSheet);
            bool okTodaySheet = !string.IsNullOrWhiteSpace(SelectedTodaySheet);
            bool okCols = !string.IsNullOrWhiteSpace(NameCol) && NameStartRow > 0
                       && !string.IsNullOrWhiteSpace(CountCol) && CountStartRow > 0;
            bool anyChecked = NameItems.Any(x => x.IsChecked);

            CanPreview = okFile && okPrevSheet && okTodaySheet && okCols && anyChecked;

            PreviewCommand.NotifyCanExecuteChanged();
            ApplyCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(CanPreview));
            OnPropertyChanged(nameof(CanApply));
            OnPropertyChanged(nameof(CanToggleChecks));
        }

        private void ClearPreviewState()
        {
            _hasPreview = false;
            _merged = null;
            PreviewRows.Clear();
            OnPropertyChanged(nameof(CanApply));
        }

        [RelayCommand(CanExecute = nameof(CanPreview))]
        private void Preview()
        {
            try
            {
                ProgressPercent = 0;
                StatusText = "Reading previous...";
                ProgressPercent = 5;

                var selectedKeys = NameItems.Where(x => x.IsChecked).Select(x => x.NormKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var prev = _svc.ReadPrevDictFromRange(_app, SelectedPrevFile!.FullPath, SelectedPrevSheet!, out _prevTopLeftAddr, out _prevTotal);

                ProgressPercent = 40;
                StatusText = "Reading today...";

                var wb = (Excel.Workbook)_app.ActiveWorkbook;
                var wsCur = (Excel.Worksheet)wb.Worksheets[SelectedTodaySheet];

                var today = _svc.ReadTodayFiltered(wsCur, NameCol!, NameStartRow, CountCol!, CountStartRow, selectedKeys);

                ProgressPercent = 65;
                StatusText = "Merging...";

                _merged = _svc.MergePrevToday(prev, today);

                ProgressPercent = 85;
                StatusText = "Building preview...";

                PreviewRows.Clear();
                foreach (var row in _svc.BuildPreviewRows(_merged, _prevTotal))
                    PreviewRows.Add(row);

                _hasPreview = true;
                ProgressPercent = 100;
                StatusText = $"Preview ready | PrevTotal={_prevTotal:0} | Selected={selectedKeys.Count} | Today={today.Count} | Merged={_merged.Count}";
            }
            catch (Exception ex)
            {
                ProgressPercent = 0;
                StatusText = "Error: " + ex.Message;
                _hasPreview = false;
            }
            finally
            {
                RecalcButtons();
            }
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        private void Apply()
        {
            if (_merged == null || string.IsNullOrWhiteSpace(SelectedTodaySheet)) return;

            var wb = (Excel.Workbook)_app.ActiveWorkbook;
            var wsCur = (Excel.Worksheet)wb.Worksheets[SelectedTodaySheet];

            StatusText = "Applying...";
            ProgressPercent = 20;

            _svc.ApplyToNewSheet(wb, wsCur, _merged, _prevTopLeftAddr);

            ProgressPercent = 100;
            StatusText = "Done";
            _close();
        }

        [RelayCommand]
        private void CheckAll()
        {
            foreach (var x in NameItems) x.IsChecked = true;
            RecalcButtons();
        }

        [RelayCommand]
        private void CheckNone()
        {
            foreach (var x in NameItems) x.IsChecked = false;
            RecalcButtons();
        }

        [RelayCommand]
        private void Close()
        {
            _close();
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>((DependencyObject)sender);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is T t)
                    return t;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
