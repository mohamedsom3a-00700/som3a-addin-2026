using Som3a_WPF_UI.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Pages
{
    public partial class FixPieColorsPage : Page
    {
        private FixPieColorsViewModel _vm;

        public FixPieColorsPage()
        {
            InitializeComponent();
            _vm = App.Container.Resolve<FixPieColorsViewModel>();
            DataContext = _vm;
            WireViewModel();
        }

        private void WireViewModel()
        {
            if (_vm == null) return;

            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.SheetNames.CollectionChanged += OnSheetNamesChanged;
            _vm.PickRangeRequested += prompt => DoPickExcelRange("Pick Range", prompt, rng =>
                _vm.CategoryRange = rng.Address[RowAbsolute: false, ColumnAbsolute: false]);
            _vm.PickColorTableRequested += prompt => DoPickExcelRange("Pick Color Table Range", prompt, rng =>
                _vm.ColorTableRange = rng.Address[RowAbsolute: false, ColumnAbsolute: false]);
            _vm.RequestClose += () => Services.NavigationService.Instance.GoBack();

            cmbSheets.SelectionChanged += OnCmbSheetsSelectionChanged;

            SyncFromViewModel();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_vm == null) return;

            switch (e.PropertyName)
            {
                case nameof(_vm.CategoryRange):
                    txtCategoryRange.Text = _vm.CategoryRange;
                    break;
                case nameof(_vm.ColorTableRange):
                    txtColorTableRange.Text = _vm.ColorTableRange;
                    break;
                case nameof(_vm.SelectedSheet):
                    cmbSheets.SelectedItem = _vm.SelectedSheet;
                    break;
                case nameof(_vm.IsBusy):
                    BusyOverlay.Visibility = _vm.IsBusy ? Visibility.Visible : Visibility.Collapsed;
                    btnRun.IsEnabled = !_vm.IsBusy;
                    btnPickRange.IsEnabled = !_vm.IsBusy;
                    btnPickColorTable.IsEnabled = !_vm.IsBusy;
                    break;
                case nameof(_vm.BusyText):
                    txtBusy.Text = _vm.BusyText;
                    break;
                case nameof(_vm.StatusText):
                    txtStatus.Text = _vm.StatusText;
                    break;
            }
        }

        private void OnSheetNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSheetList();
        }

        private void UpdateSheetList()
        {
            cmbSheets.Items.Clear();
            foreach (var name in _vm.SheetNames)
                cmbSheets.Items.Add(name);
            if (_vm.SheetNames.Count > 0)
                cmbSheets.SelectedItem = _vm.SelectedSheet;
        }

        private void OnCmbSheetsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSheets.SelectedItem is string name)
                _vm.SelectedSheet = name;
        }

        private void SyncFromViewModel()
        {
            if (_vm == null) return;

            txtCategoryRange.Text = _vm.CategoryRange;
            txtColorTableRange.Text = _vm.ColorTableRange;
            UpdateSheetList();
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            _vm?.AttachExcel(xlApp);
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            _vm?.RunCommand.Execute(null);
        }

        private void btnPickRange_Click(object sender, RoutedEventArgs e)
        {
            _vm?.PickRangeCommand.Execute(null);
        }

        private void btnPickColorTable_Click(object sender, RoutedEventArgs e)
        {
            _vm?.PickColorTableCommand.Execute(null);
        }

        private void DoPickExcelRange(string title, string prompt, Action<Excel.Range> onPicked)
        {
            var xlApp = _vm?.ExcelApp;
            if (xlApp?.ActiveWorkbook == null)
            {
                MessageBox.Show("No active workbook.", "Error");
                return;
            }

            try
            {
                try { xlApp.ActiveWindow?.Activate(); } catch { }

                object picked = xlApp.InputBox(
                    Prompt: prompt,
                    Title: title,
                    Default: Type.Missing,
                    Type: 8
                );

                if (picked is bool b && b == false)
                    return;

                if (picked is Excel.Range rng)
                {
                    onPicked(rng);
                }
                else
                {
                    MessageBox.Show("Invalid selection. Please select a cell range.", "Warning");
                }
            }
            catch (COMException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}
