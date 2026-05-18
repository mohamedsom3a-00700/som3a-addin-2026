using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI
{
    public sealed class TradeCodeLogRow
    {
        public int Row { get; set; }
        public string Activity { get; set; } = "";
        public string TradeCode { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public sealed class AssignTradeCodesViewModel : INotifyPropertyChanged
    {
        private readonly AssignTradeCodesService _service = new AssignTradeCodesService();
        private CancellationTokenSource? _cts;

        public ObservableCollection<string> Sheets { get; } = new();
        public ObservableCollection<TradeCodeLogRow> LogItems { get; } = new();

        private string? _mainSheet;
        public string? MainSheet
        {
            get => _mainSheet;
            set
            {
                _mainSheet = value;
                OnPropertyChanged();
                AutoDetectColumns();
                RefreshCanRun();
            }
        }

        private string? _refSheet;
        public string? RefSheet
        {
            get => _refSheet;
            set
            {
                _refSheet = value;
                OnPropertyChanged();
                RefreshCanRun();
            }
        }

        private string _codeColLetter = "";
        public string CodeColLetter { get => _codeColLetter; private set { _codeColLetter = value; OnPropertyChanged(); } }

        private string _codeColHeader = "";
        public string CodeColHeader { get => _codeColHeader; private set { _codeColHeader = value; OnPropertyChanged(); } }

        private string _activityColLetter = "";
        public string ActivityColLetter { get => _activityColLetter; private set { _activityColLetter = value; OnPropertyChanged(); } }

        private string _activityColHeader = "";
        public string ActivityColHeader { get => _activityColHeader; private set { _activityColHeader = value; OnPropertyChanged(); } }

        private string _statusColLetter = "";
        public string StatusColLetter { get => _statusColLetter; private set { _statusColLetter = value; OnPropertyChanged(); } }

        private int _progressPercent;
        public int ProgressPercent { get => _progressPercent; private set { _progressPercent = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); RefreshCanRun(); } }

        private bool _canRun;
        public bool CanRun { get => _canRun; private set { _canRun = value; OnPropertyChanged(); } }

        private string _statusText = "Ready";
        public string StatusText { get => _statusText; private set { _statusText = value; OnPropertyChanged(); } }

        public RelayCommand RunCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand CancelCommand { get; }

        public AssignTradeCodesViewModel()
        {
            RunCommand = new RelayCommand(async _ => await RunAsync(), _ => CanRun);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => IsBusy);
        }

        public void Initialize()
        {
            LoadSheets();
            StatusText = "Ready";
            ProgressPercent = 0;

            // زي VBA: Run Disabled لحد ما يحصل auto detect + ref selected
            RefreshCanRun();
        }

        private void LoadSheets()
        {
            Sheets.Clear();

            var app = GetExcel();
            if (app?.ActiveWorkbook == null)
            {
                StatusText = "No active workbook detected.";
                return;
            }

            foreach (Excel.Worksheet ws in app.ActiveWorkbook.Worksheets)
                Sheets.Add(ws.Name);

            MainSheet ??= Sheets.FirstOrDefault();
            RefSheet ??= Sheets.Skip(1).FirstOrDefault() ?? Sheets.FirstOrDefault();
        }

        private void AutoDetectColumns()
        {
            CodeColLetter = CodeColHeader = "";
            ActivityColLetter = ActivityColHeader = "";
            StatusColLetter = "";

            var app = GetExcel();
            if (app?.ActiveWorkbook == null) return;
            if (string.IsNullOrWhiteSpace(MainSheet)) return;

            Excel.Worksheet? ws = null;
            try { ws = (Excel.Worksheet)app.ActiveWorkbook.Worksheets[MainSheet!]; }
            catch { return; }

            if (ws == null) return;

            // نفس FindColumnInfo في VBA
            if (_service.TryFindColumnInfo(ws, "CODE", out var codeL, out var codeH))
            {
                CodeColLetter = codeL;
                CodeColHeader = codeH;
            }

            if (_service.TryFindColumnInfo(ws, "Activity ID", out var actL, out var actH))
            {
                ActivityColLetter = actL;
                ActivityColHeader = actH;
            }

            if (_service.TryFindColumnInfo(ws, "Status", out var stL, out _))
            {
                StatusColLetter = stL;
            }
        }

        private void RefreshCanRun()
        {
            CanRun = !IsBusy
                     && !string.IsNullOrWhiteSpace(MainSheet)
                     && !string.IsNullOrWhiteSpace(RefSheet)
                     && !string.IsNullOrWhiteSpace(CodeColLetter)
                     && !string.IsNullOrWhiteSpace(ActivityColLetter)
                     && !string.IsNullOrWhiteSpace(StatusColLetter);

            RunCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private async Task RunAsync()
        {
            var app = GetExcel();

            if (app?.ActiveWorkbook == null)
            {
                StatusText = "No active workbook detected.";
                return;
            }

            if (!CanRun)
            {
                StatusText = "Please fill in all required fields.";
                return;
            }

            IsBusy = true;
            _cts = new CancellationTokenSource();

            LogItems.Clear();
            ProgressPercent = 0;
            StatusText = "Starting...";

            try
            {
                var wb = app.ActiveWorkbook;
                var ws = (Excel.Worksheet)wb.Worksheets[MainSheet!];
                var wsRef = (Excel.Worksheet)wb.Worksheets[RefSheet!];

                StatusText = "Processing...";

                var progress = new Progress<AssignProgress>(p =>
                {
                    // UI Thread automatically
                    ProgressPercent = p.Percent;
                    StatusText = p.StatusText;

                    if (p.LogItem.HasValue)
                    {
                        var li = p.LogItem.Value;

                        LogItems.Add(new TradeCodeLogRow
                        {
                            Row = li.Row,
                            Activity = li.Activity,
                            TradeCode = li.Code,
                            Status = li.Status
                        });
                    }
                });

                await Task.Yield(); // يدي فرصة للـ UI

                _service.AssignTradeCodes(
                    ws,
                    wsRef,
                    CodeColLetter,
                    ActivityColLetter,
                    StatusColLetter,
                    progress,
                    _cts.Token);

                StatusText = _cts.IsCancellationRequested
                    ? "Process cancelled"
                    : "Completed successfully";

                ProgressPercent = 100;
            }
            catch (OperationCanceledException)
            {
                StatusText = "Process cancelled";
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void Stop()
        {
            _cts?.Cancel(); // زي btnStop_Click
            StatusText = "Cancelled by user";
        }

        private void Cancel()
        {
            _cts?.Cancel(); // زي btnCancel_Click
            StatusText = "Cancelled by user";
        }

        private Excel.Application? GetExcel()
        {
            try
            {
                return (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch
            {
                var app = new Excel.Application();
                app.Visible = true;
                return app;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _can;
        private readonly Action<object?> _exec;

        public RelayCommand(Action<object?> exec, Func<object?, bool>? can = null)
        {
            _exec = exec;
            _can = can;
        }

        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
