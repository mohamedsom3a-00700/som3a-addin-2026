using System.Windows;
using System.Windows.Input;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI.Views
{
    public partial class SettingsWindow : ModernWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var vm = App.Container.Resolve<ViewModels.SettingsViewModel>();
            vm.CloseWindow = result => { DialogResult = result; Close(); };
            DataContext = vm;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
                vm.Cleanup();
            base.OnClosed(e);
        }
    }
}
