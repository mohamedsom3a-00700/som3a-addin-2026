using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed partial class UnmergeFillDownViewModel : ViewModelBase
    {
        private readonly Excel.Application _app;
        private readonly Action _close;
        private readonly UnmergeFillDownService _service;

        private CancellationTokenSource? _cts;

        public ObservableCollection<string> Columns { get; } = new();

        [ObservableProperty]
        private string _activeSheetName = "";

        [ObservableProperty]
        private int _headerRow = 1;

        partial void OnHeaderRowChanged(int value)
        {
            ReloadColumns();
        }

        [ObservableProperty]
        private string? _selectedColumn;

        partial void OnSelectedColumnChanged(string? value)
        {
            RefreshCanRun();
        }

        [ObservableProperty]
        private string _infoText = "Select a column that contains merged cells/data.";

        [ObservableProperty]
        private string _statusText = "Ready.";

        [ObservableProperty]
        private double _progressPercent;

        [ObservableProperty]
        private bool _isBusy;

        partial void OnIsBusyChanged(bool value)
        {
            RefreshCanRun();
        }

        public bool CanRun => !IsBusy && !string.IsNullOrWhiteSpace(SelectedColumn);

        public UnmergeFillDownViewModel(IServiceContainer container, Excel.Application app, Action closeAction)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _close = closeAction ?? throw new ArgumentNullException(nameof(closeAction));
            _service = container.Resolve<UnmergeFillDownService>();

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
            RunCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanRun));
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task Run()
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

        [RelayCommand]
        private void Close()
        {
            _close();
        }
    }
}
