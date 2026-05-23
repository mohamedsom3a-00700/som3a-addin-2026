using Som3a.Shared.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI
{
    public partial class FixPieColorsWindow : ModernWindow
    {
        private Excel.Application? _xlApp;

        public FixPieColorsWindow()
        {
            InitializeComponent();
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            _xlApp = xlApp;
            LoadSheets();
        }

        private void LoadSheets()
        {
            cmbSheets.Items.Clear();

            if (_xlApp?.ActiveWorkbook == null) return;

            foreach (Excel.Worksheet ws in _xlApp.ActiveWorkbook.Worksheets)
                cmbSheets.Items.Add(ws.Name);

            if (cmbSheets.Items.Count > 0)
                cmbSheets.SelectedIndex = 0;
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (_xlApp?.ActiveWorkbook == null)
            {
                MessageBox.Show("No active workbook.", "Error");
                return;
            }

            var sheetName = cmbSheets.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                MessageBox.Show("Select Sheet", "Warning");
                return;
            }

            var categoryRange = txtCategoryRange.Text?.Trim();
            if (string.IsNullOrWhiteSpace(categoryRange))
            {
                MessageBox.Show("Pick Category Range", "Warning");
                return;
            }

            var colorTableRange = txtColorTableRange.Text?.Trim();
            if (string.IsNullOrWhiteSpace(colorTableRange))
            {
                MessageBox.Show("Pick Color Table Range (Label+Color)", "Warning");
                return;
            }

            SetBusy(true, "Fixing colors...");
            SetUiStatus("Fixing pie colors...");
            SetExcelStatus("Fixing pie colors...");

            try
            {
                await System.Threading.Tasks.Task.Yield();

                var service = App.Container.Resolve<FixPieColorsService>();
                var res = service.ApplyColors(_xlApp, sheetName, categoryRange, colorTableRange);

                MessageBox.Show(
                    $"Charts: {res.ChartsCount}\nSeries: {res.SeriesCount}\nPoints: {res.PointsTotal}\nMatched: {res.Matched}\nUpdated: {res.Updated}\n\n" +
                    $"Not matched (up to 30):\n{string.Join("\n", res.NotMatchedLabels.Take(30))}",
                    "Done"
                );

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                ClearExcelStatus();
                SetBusy(false);
                SetUiStatus("Ready");
            }
        }

        private void SetBusy(bool isBusy, string? msg = null)
        {
            BusyOverlay.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

            if (!string.IsNullOrWhiteSpace(msg))
            {
                txtBusy.Text = msg;
                txtStatus.Text = msg;
            }

            btnRun.IsEnabled = !isBusy;
            btnPickRange.IsEnabled = !isBusy;
            btnPickColorTable.IsEnabled = !isBusy;
        }

        private void SetUiStatus(string text) => txtStatus.Text = text;

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

        private void btnPickRange_Click(object sender, RoutedEventArgs e)
        {
            PickExcelRange(
                title: "Pick Range",
                prompt: "Select the range (مثال: A1:A8)",
                onPicked: rng => txtCategoryRange.Text = rng.Address[RowAbsolute: false, ColumnAbsolute: false]
            );
        }

        private void btnPickColorTable_Click(object sender, RoutedEventArgs e)
        {
            PickExcelRange(
                title: "Pick Color Table Range",
                prompt: "Select the color table range (2 columns: Label | Color). مثال: A2:B9",
                onPicked: rng => txtColorTableRange.Text = rng.Address[RowAbsolute: false, ColumnAbsolute: false]
            );
        }

        private void PickExcelRange(string title, string prompt, Action<Excel.Range> onPicked)
        {
            if (_xlApp?.ActiveWorkbook == null)
            {
                MessageBox.Show("No active workbook.", "Error");
                return;
            }

            var wasTopmost = Topmost;
            Topmost = false;

            Hide();

            try
            {
                try { _xlApp.ActiveWindow?.Activate(); } catch { }

                object picked = _xlApp.InputBox(
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
            finally
            {
                Show();
                Activate();
                Topmost = wasTopmost;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
