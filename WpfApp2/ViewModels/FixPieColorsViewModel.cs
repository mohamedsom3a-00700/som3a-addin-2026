using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class FixPieColorsViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private Excel.Application? _xlApp;

        private string _categoryRange = "R6:R13";
        private string _colorTableRange = "A2:B9";
        private string _selectedSheet = "";
        private bool _isBusy;
        private string _statusText = "Ready";
        private string _busyText = "";

        public ObservableCollection<string> SheetNames { get; } = new();

        public string CategoryRange
        {
            get => _categoryRange;
            set => SetProperty(ref _categoryRange, value ?? "");
        }

        public string ColorTableRange
        {
            get => _colorTableRange;
            set => SetProperty(ref _colorTableRange, value ?? "");
        }

        public string SelectedSheet
        {
            get => _selectedSheet;
            set => SetProperty(ref _selectedSheet, value ?? "");
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    ((RelayCommand)RunCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)PickRangeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)PickColorTableCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value ?? "");
        }

        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value ?? "");
        }

        public Excel.Application? ExcelApp => _xlApp;

        public ICommand RunCommand { get; }
        public ICommand PickRangeCommand { get; }
        public ICommand PickColorTableCommand { get; }

        public event Action<string>? PickRangeRequested;
        public event Action<string>? PickColorTableRequested;
        public event Action? RequestClose;

        public FixPieColorsViewModel(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            RunCommand = new RelayCommand(() => _ = RunAsync(), () => !IsBusy);
            PickRangeCommand = new RelayCommand(_ => PickRangeRequested?.Invoke("Select the range (مثال: A1:A8)"), _ => !IsBusy);
            PickColorTableCommand = new RelayCommand(_ => PickColorTableRequested?.Invoke("Select the color table range (2 columns: Label | Color). مثال: A2:B9"), _ => !IsBusy);
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            _xlApp = xlApp ?? throw new ArgumentNullException(nameof(xlApp));
            LoadSheets();
        }

        private void LoadSheets()
        {
            SheetNames.Clear();
            if (_xlApp?.ActiveWorkbook == null) return;

            foreach (Excel.Worksheet ws in _xlApp.ActiveWorkbook.Worksheets)
                SheetNames.Add(ws.Name);

            if (SheetNames.Count > 0)
                SelectedSheet = SheetNames[0];
        }

        private async System.Threading.Tasks.Task RunAsync()
        {
            if (_xlApp?.ActiveWorkbook == null)
            {
                StatusText = "No active workbook.";
                return;
            }

            var sheetName = SelectedSheet;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                StatusText = "Select Sheet.";
                return;
            }

            var categoryRange = CategoryRange?.Trim();
            if (string.IsNullOrWhiteSpace(categoryRange))
            {
                StatusText = "Pick Category Range.";
                return;
            }

            var colorTableRange = ColorTableRange?.Trim();
            if (string.IsNullOrWhiteSpace(colorTableRange))
            {
                StatusText = "Pick Color Table Range (Label+Color).";
                return;
            }

            IsBusy = true;
            BusyText = "Fixing colors...";
            StatusText = "Fixing pie colors...";
            SetExcelStatus("Fixing pie colors...");

            try
            {
                await System.Threading.Tasks.Task.Yield();

                var service = _container.Resolve<FixPieColorsService>();
                var res = service.ApplyColors(_xlApp, sheetName, categoryRange, colorTableRange);

                StatusText =
                    $"Charts: {res.ChartsCount}, Series: {res.SeriesCount}, Points: {res.PointsTotal}, " +
                    $"Matched: {res.Matched}, Updated: {res.Updated}";

                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                ClearExcelStatus();
                IsBusy = false;
                BusyText = "";
            }
        }

        private void SetExcelStatus(string text)
        {
            try { _xlApp!.StatusBar = text; } catch { }
        }

        private void ClearExcelStatus()
        {
            try
            {
                if (_xlApp != null)
                    _xlApp.StatusBar = false;
            }
            catch { }
        }
    }
}
