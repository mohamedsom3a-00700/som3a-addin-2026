using Som3a.Shared.Models;
using Som3a_WPF_UI.Controls;
using System;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI
{
    public partial class MainWindow : ModernWindow
    {
        private readonly MainViewModel _vm = new MainViewModel();
        private Excel.Application _xlApp;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            this.Closed += MainWindow_Closed;
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            if (xlApp == null) throw new ArgumentNullException(nameof(xlApp));

            _xlApp = xlApp;

            // امنع الكتابة في Excel أثناء فتح الفورم
            try { _xlApp.Interactive = false; } catch { }

            _vm.AttachExcel(xlApp);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                if (_xlApp != null) _xlApp.Interactive = true;
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // خلي الفورم هو اللي ماسك الـ focus بدل Excel
            try
            {
                if (cmbOldSheet != null)
                {
                    cmbOldSheet.Focus();
                    Keyboard.Focus(cmbOldSheet);
                }
            }
            catch { }
        }

        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            await _vm.PreviewAsync();

            // رسالة بسيطة بعد الـ Preview
            if (_vm.StatusText.StartsWith("Error"))
            {
                MessageBox.Show(_vm.StatusText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (_vm.StatusText == "Cancelled")
            {
                MessageBox.Show("Preview cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show($"Preview completed.\nChanges: {_vm.LastChangesCount}", "Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            await _vm.StartAsync();

            // رسالة بعد الـ Update
            if (_vm.StatusText.StartsWith("Error"))
            {
                MessageBox.Show(_vm.StatusText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (_vm.StatusText == "Cancelled")
            {
                MessageBox.Show("Update cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show($"Update completed successfully.\nChanges: {_vm.LastChangesCount}", "Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _vm.Cancel();
        }
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return;
            DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    }
}
