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
        private readonly MainViewModel _vm;
        private Excel.Application _xlApp;

        public MainWindow()
        {
            _vm = App.Container.Resolve<MainViewModel>();
            InitializeComponent();
            DataContext = _vm;

            _vm.NotificationRaised += OnViewModelNotification;
            this.Closed += MainWindow_Closed;
        }

        private void OnViewModelNotification(string message, MainViewModel.NotificationIcon icon)
        {
            var image = icon switch
            {
                MainViewModel.NotificationIcon.Error => MessageBoxImage.Error,
                MainViewModel.NotificationIcon.Warning => MessageBoxImage.Warning,
                _ => MessageBoxImage.Information
            };
            var title = icon switch
            {
                MainViewModel.NotificationIcon.Error => "Error",
                MainViewModel.NotificationIcon.Warning => "Cancelled",
                _ => "Done"
            };
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
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
            _vm.NotificationRaised -= OnViewModelNotification;
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

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.PreviewCommand.CanExecute(null))
                _vm.PreviewCommand.Execute(null);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.StartCommand.CanExecute(null))
                _vm.StartCommand.Execute(null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _vm.CancelCommand.Execute(null);
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
