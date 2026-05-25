using Som3a.Shared.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Pages
{
    public partial class MainPage : Page
    {
        private readonly MainViewModel _vm;
        private Excel.Application _xlApp;

        public MainPage()
        {
            _vm = App.Container.Resolve<MainViewModel>();
            InitializeComponent();
            DataContext = _vm;

            _vm.NotificationRaised += OnViewModelNotification;
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        public void AttachExcel(Excel.Application xlApp)
        {
            if (xlApp == null) throw new ArgumentNullException(nameof(xlApp));

            _xlApp = xlApp;

            try
            {
                try { _xlApp.Interactive = false; } catch { }

                _vm.AttachExcel(xlApp);
            }
            catch
            {
                try { _xlApp.Interactive = true; } catch { }
                throw;
            }
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

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
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

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _vm.NotificationRaised -= OnViewModelNotification;
            try
            {
                if (_xlApp != null) _xlApp.Interactive = true;
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
    }
}
