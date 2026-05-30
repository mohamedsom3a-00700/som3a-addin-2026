using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed partial class FixPieColorsViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private Excel.Application? _xlApp;

        [ObservableProperty]
        private string _categoryRange = "R6:R13";

        [ObservableProperty]
        private string _colorTableRange = "A2:B9";

        [ObservableProperty]
        private string _selectedSheet = "";

        [ObservableProperty]
        private bool _isBusy;

        partial void OnIsBusyChanged(bool value)
        {
            RunCommand.NotifyCanExecuteChanged();
            PickRangeCommand.NotifyCanExecuteChanged();
            PickColorTableCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        private string _busyText = "";

        public bool IsNotBusy => !IsBusy;

        public Excel.Application? ExcelApp => _xlApp;

        public ObservableCollection<string> SheetNames { get; } = new();

        public event Action<string>? PickRangeRequested;
        public event Action<string>? PickColorTableRequested;
        public event Action? RequestClose;

        public FixPieColorsViewModel(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
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

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task Run()
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
                await Task.Yield();

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

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void PickRange()
        {
            PickRangeRequested?.Invoke("Select the range (مثال: A1:A8)");
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void PickColorTable()
        {
            PickColorTableRequested?.Invoke("Select the color table range (2 columns: Label | Color). مثال: A2:B9");
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
