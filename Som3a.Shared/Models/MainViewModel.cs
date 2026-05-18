using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a.Shared.Controllers;
using Som3a.Shared.Core;

namespace Som3a.Shared.Models
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly CompareController _controller;
        private CancellationTokenSource _cts;

        private Excel.Workbook _wb;

        public ObservableCollection<string> Sheets { get; private set; }
        public ObservableCollection<CompareResultItem> Items { get; private set; }

        private bool _autoFillBusy;

        private int _lastChangesCount;
        public int LastChangesCount
        {
            get { return _lastChangesCount; }
            private set { _lastChangesCount = value; OnPropertyChanged(); }
        }

        private string _oldSheet;
        public string OldSheet
        {
            get { return _oldSheet; }
            set
            {
                if (_oldSheet == value) return;
                _oldSheet = value;
                OnPropertyChanged();
                AutoDetectFromOldSheet();
                Validate();
            }
        }

        private string _newSheet;
        public string NewSheet
        {
            get { return _newSheet; }
            set
            {
                if (_newSheet == value) return;
                _newSheet = value;
                OnPropertyChanged();
                AutoDetectFromNewSheet();
                Validate();
            }
        }

        private string _idCol;
        public string IdCol { get { return _idCol; } set { _idCol = value; OnPropertyChanged(); Validate(); } }

        private string _compareCol;
        public string CompareCol { get { return _compareCol; } set { _compareCol = value; OnPropertyChanged(); Validate(); } }

        private string _copyCol;
        public string CopyCol { get { return _copyCol; } set { _copyCol = value; OnPropertyChanged(); Validate(); } }

        private int _startRow;
        public int StartRow { get { return _startRow; } set { _startRow = value; OnPropertyChanged(); Validate(); } }

        private int _progress;
        public int ProgressPercent { get { return _progress; } private set { _progress = value; OnPropertyChanged(); } }

        private string _status;
        public string StatusText { get { return _status; } private set { _status = value; OnPropertyChanged(); } }

        private string _rowsText;
        public string RowsText { get { return _rowsText; } private set { _rowsText = value; OnPropertyChanged(); } }

        private string _changesText;
        public string ChangesText { get { return _changesText; } private set { _changesText = value; OnPropertyChanged(); } }

        private bool _canRun;
        public bool CanRun { get { return _canRun; } private set { _canRun = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; OnPropertyChanged(); Validate(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _controller = new CompareController();
            Sheets = new ObservableCollection<string>();
            Items = new ObservableCollection<CompareResultItem>();

            _oldSheet = "";
            _newSheet = "";
            _idCol = "";
            _compareCol = "";
            _copyCol = "";
            _startRow = 3;

            _status = "Ready";
            _rowsText = "Rows: 0";
            _changesText = "Changes: 0";
            _canRun = false;
            _isBusy = false;
            _lastChangesCount = 0;
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            if (xlApp == null) throw new ArgumentNullException("xlApp");
            _wb = xlApp.ActiveWorkbook;
            if (_wb == null) throw new InvalidOperationException("No active workbook.");

            LoadSheets(_wb);

            StatusText = "Ready";
            Validate();
        }

        public void LoadSheets(Excel.Workbook wb)
        {
            Sheets.Clear();
            foreach (Excel.Worksheet ws in wb.Worksheets)
                Sheets.Add(ws.Name);
        }

        public Task PreviewAsync() { return RunAsync(true); }
        public Task StartAsync() { return RunAsync(false); }

        public void Cancel()
        {
            if (_cts != null) _cts.Cancel();
            StatusText = "Cancelled";
        }

        private async Task RunAsync(bool previewMode)
        {
            if (!CanRun) return;

            Items.Clear();
            LastChangesCount = 0;

            ProgressPercent = 0;
            RowsText = "Rows: 0";
            ChangesText = "Changes: 0";
            StatusText = previewMode ? "Preview..." : "Processing...";

            _cts = new CancellationTokenSource();
            IsBusy = true;

            try
            {
                Progress<ExcelCompareService.ProgressInfo> prog =
                    new Progress<ExcelCompareService.ProgressInfo>(p =>
                    {
                        ProgressPercent = p.Percent;
                        RowsText = "Rows: " + p.RowsDone.ToString(CultureInfo.InvariantCulture);
                        ChangesText = "Changes: " + p.Changes.ToString(CultureInfo.InvariantCulture);
                    });

                var list = await _controller.RunAsync(
                    _wb,
                    OldSheet,
                    NewSheet,
                    IdCol,
                    CompareCol,
                    CopyCol,
                    StartRow,
                    previewMode,
                    prog,
                    _cts.Token);

                Items.Clear();
                for (int i = 0; i < list.Count; i++)
                    Items.Add(list[i]);

                LastChangesCount = Items.Count;

                StatusText = _cts.IsCancellationRequested ? "Cancelled" : "Completed";
            }
            catch (OperationCanceledException)
            {
                StatusText = "Cancelled";
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
                if (_cts != null) { _cts.Dispose(); _cts = null; }
            }
        }

        private void Validate()
        {
            bool ok =
                (_wb != null) &&
                !IsBusy &&
                !string.IsNullOrWhiteSpace(OldSheet) &&
                !string.IsNullOrWhiteSpace(NewSheet) &&
                !string.IsNullOrWhiteSpace(IdCol) &&
                !string.IsNullOrWhiteSpace(CompareCol) &&
                StartRow > 0;

            CanRun = ok;
        }


        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }



        // =========================
        // Auto-Detect Columns
        // =========================
        private void AutoDetectFromOldSheet()
        {
            if (_autoFillBusy) return;
            if (_wb == null) return;
            if (string.IsNullOrWhiteSpace(OldSheet)) return;

            _autoFillBusy = true;
            try
            {
                Excel.Worksheet ws = (Excel.Worksheet)_wb.Worksheets[OldSheet];

                // ID
                string id = FindColumnLetter(ws, 1, new[] { "task_code", "activity_id", "task id", "taskid" });
                if (!string.IsNullOrWhiteSpace(id)) IdCol = id;

                // Compare
                string comp = FindColumnLetter(ws, 1, new[] { "complete_pct", "perfm_complete_pct", "complete%", "percent_complete" });
                if (!string.IsNullOrWhiteSpace(comp)) CompareCol = comp;

                // WBS from OLD
                string wbs = FindColumnLetter(ws, 1, new[] { "wbs_id", "wbs", "wbs id" });
                if (!string.IsNullOrWhiteSpace(wbs)) CopyCol = wbs;

                // Start row default (لو لسه متغيرتش)
                if (StartRow <= 0) StartRow = 3;

                StatusText = "Columns detected from OLD sheet";
            }
            catch (Exception ex)
            {
                StatusText = "Auto-detect OLD failed: " + ex.Message;
            }
            finally
            {
                _autoFillBusy = false;
            }
        }

        private void AutoDetectFromNewSheet()
        {
            if (_autoFillBusy) return;
            if (_wb == null) return;
            if (string.IsNullOrWhiteSpace(NewSheet)) return;

            _autoFillBusy = true;
            try
            {
                Excel.Worksheet ws = (Excel.Worksheet)_wb.Worksheets[NewSheet];

                // لو CompareCol فاضي أو مش موجود في الشيت الجديد، حاول تلقائيًا
                if (string.IsNullOrWhiteSpace(CompareCol) || !ColumnExists(ws, 1, CompareCol))
                {
                    string comp = FindColumnLetter(ws, 1, new[] { "complete_pct", "perfm_complete_pct", "complete%", "percent_complete" });
                    if (!string.IsNullOrWhiteSpace(comp)) CompareCol = comp;
                }

                // ID لو فاضي
                if (string.IsNullOrWhiteSpace(IdCol) || !ColumnExists(ws, 1, IdCol))
                {
                    string id = FindColumnLetter(ws, 1, new[] { "task_code", "activity_id", "task id", "taskid" });
                    if (!string.IsNullOrWhiteSpace(id)) IdCol = id;
                }

                StatusText = "Checked columns on NEW sheet";
            }
            catch (Exception ex)
            {
                StatusText = "Auto-detect NEW failed: " + ex.Message;
            }
            finally
            {
                _autoFillBusy = false;
            }
        }

        private static bool ColumnExists(Excel.Worksheet ws, int headerRow, string colLetter)
        {
            if (ws == null) return false;
            if (string.IsNullOrWhiteSpace(colLetter)) return false;

            try
            {
                int col = ColLetterToNum(colLetter);
                object v = ws.Cells[headerRow, col].Value2;
                return v != null; // وجود أي قيمة في هيدر الخانة
            }
            catch
            {
                return false;
            }
        }

        private static string FindColumnLetter(Excel.Worksheet ws, int headerRow, string[] candidates)
        {
            if (ws == null) return "";
            if (candidates == null || candidates.Length == 0) return "";

            Excel.Range last = ws.Cells[headerRow, ws.Columns.Count].End[Excel.XlDirection.xlToLeft];
            int lastCol = last.Column;

            for (int c = 1; c <= lastCol; c++)
            {
                string header = Convert.ToString(ws.Cells[headerRow, c].Value2) ?? "";
                header = header.Trim();

                if (header.Length == 0) continue;

                for (int i = 0; i < candidates.Length; i++)
                {
                    if (string.Equals(header, candidates[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return ColumnNumToLetter(c);
                    }
                }
            }

            return "";
        }

        private static int ColLetterToNum(string colLetter)
        {
            colLetter = (colLetter ?? "").Trim().ToUpperInvariant();
            int sum = 0;
            for (int i = 0; i < colLetter.Length; i++)
            {
                char c = colLetter[i];
                if (c < 'A' || c > 'Z') throw new ArgumentException("Invalid column letter");
                sum = sum * 26 + (c - 'A' + 1);
            }
            return sum;
        }

        private static string ColumnNumToLetter(int col)
        {
            if (col <= 0) return "";
            string s = "";
            while (col > 0)
            {
                int mod = (col - 1) % 26;
                s = (char)('A' + mod) + s;
                col = (col - mod) / 26;
                col--;
            }
            return s;
        }

    }
}
