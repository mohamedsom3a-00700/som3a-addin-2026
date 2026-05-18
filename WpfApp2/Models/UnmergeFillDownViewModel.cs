using Som3a.Shared.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI
{
    public sealed class UnmergeFillDownViewModel : INotifyPropertyChanged
    {
        private readonly Excel.Application _app;
        private readonly Action _close;
        private readonly UnmergeFillDownService _service = new UnmergeFillDownService();

        private CancellationTokenSource? _cts;

        public ObservableCollection<string> Columns { get; } = new();

        private string _activeSheetName = "";
        public string ActiveSheetName
        {
            get => _activeSheetName;
            private set { _activeSheetName = value; OnPropertyChanged(); }
        }

        private int _headerRow = 1;
        public int HeaderRow
        {
            get => _headerRow;
            set { _headerRow = value < 1 ? 1 : value; OnPropertyChanged(); ReloadColumns(); }
        }

        private string? _selectedColumn;
        public string? SelectedColumn
        {
            get => _selectedColumn;
            set { _selectedColumn = value; OnPropertyChanged(); RefreshCanRun(); }
        }

        private string _infoText = "Select a column that contains merged cells/data.";
        public string InfoText
        {
            get => _infoText;
            set { _infoText = value; OnPropertyChanged(); }
        }

        private string _statusText = "Ready.";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private double _progressPercent;
        public double ProgressPercent
        {
            get => _progressPercent;
            set { _progressPercent = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set { _isBusy = value; OnPropertyChanged(); RefreshCanRun(); }
        }

        public bool CanRun => !IsBusy && !string.IsNullOrWhiteSpace(SelectedColumn);

        public ICommand RunCommand { get; }
        public ICommand CloseCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public UnmergeFillDownViewModel(Excel.Application app, Action closeAction)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _close = closeAction ?? throw new ArgumentNullException(nameof(closeAction));

            RunCommand = new RelayCommand(async () => await RunAsync(), () => CanRun);
            CloseCommand = new RelayCommand(() => _close());

            LoadActiveSheet();
            ReloadColumns();
        }

        private void LoadActiveSheet()
        {
            Excel.Worksheet ws = (Excel.Worksheet)_app.ActiveSheet;
            ActiveSheetName = ws?.Name ?? "";
        }

        private void ReloadColumns()
        {
            try
            {
                Columns.Clear();
                Excel.Worksheet ws = (Excel.Worksheet)_app.ActiveSheet;

                foreach (var col in _service.GetUsedColumnLetters(ws, HeaderRow))
                    Columns.Add(col);

                if (Columns.Count > 0 && string.IsNullOrWhiteSpace(SelectedColumn))
                    SelectedColumn = Columns[0];

                StatusText = "Ready.";
            }
            catch (Exception ex)
            {
                StatusText = "Error loading columns: " + ex.Message;
            }
            finally
            {
                RefreshCanRun();
            }
        }

        private void RefreshCanRun()
        {
            if (RunCommand is RelayCommand rc)
                rc.RaiseCanExecuteChanged();

            OnPropertyChanged(nameof(CanRun));
        }

        private async Task RunAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedColumn))
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                IsBusy = true;
                ProgressPercent = 0;
                StatusText = $"Running on column {SelectedColumn}...";

                Excel.Worksheet ws = (Excel.Worksheet)_app.ActiveSheet;

                var progress = new Progress<double>(p => ProgressPercent = p);

                await Task.Run(() =>
                {
                    _service.UnmergeAndFillDownColumn(ws, SelectedColumn!, startRow: 2, progress: progress, token: _cts.Token);
                });

                StatusText = $"Complete Unmerge Cells {SelectedColumn}.";
                ProgressPercent = 100;
            }
            catch (OperationCanceledException)
            {
                StatusText = "Canceled.";
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
