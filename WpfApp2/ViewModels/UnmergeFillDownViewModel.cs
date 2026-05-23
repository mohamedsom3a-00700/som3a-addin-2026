using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class UnmergeFillDownViewModel : ViewModelBase
    {
        private readonly Excel.Application _app;
        private readonly Action _close;
        private readonly UnmergeFillDownService _service;

        private CancellationTokenSource? _cts;

        public ObservableCollection<string> Columns { get; } = new();

        private string _activeSheetName = "";
        public string ActiveSheetName
        {
            get => _activeSheetName;
            private set => SetProperty(ref _activeSheetName, value);
        }

        private int _headerRow = 1;
        public int HeaderRow
        {
            get => _headerRow;
            set
            {
                if (SetProperty(ref _headerRow, value < 1 ? 1 : value))
                    ReloadColumns();
            }
        }

        private string? _selectedColumn;
        public string? SelectedColumn
        {
            get => _selectedColumn;
            set
            {
                if (SetProperty(ref _selectedColumn, value))
                    RefreshCanRun();
            }
        }

        private string _infoText = "Select a column that contains merged cells/data.";
        public string InfoText
        {
            get => _infoText;
            set => SetProperty(ref _infoText, value);
        }

        private string _statusText = "Ready.";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private double _progressPercent;
        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    RefreshCanRun();
            }
        }

        public bool CanRun => !IsBusy && !string.IsNullOrWhiteSpace(SelectedColumn);

        public ICommand RunCommand { get; }
        public ICommand CloseCommand { get; }

        public UnmergeFillDownViewModel(IServiceContainer container, Excel.Application app, Action closeAction)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _close = closeAction ?? throw new ArgumentNullException(nameof(closeAction));
            _service = container.Resolve<UnmergeFillDownService>();

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

                _service.UnmergeAndFillDownColumn(ws, SelectedColumn!, startRow: HeaderRow, progress: progress, token: _cts.Token);

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

    }
}
