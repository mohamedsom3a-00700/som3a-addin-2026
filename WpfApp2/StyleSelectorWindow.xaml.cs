using System.Windows;
using System.Windows.Input;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI
{
    public partial class StyleSelectorWindow : ModernWindow
    {
        public StyleSelectorWindow()
        {
            InitializeComponent();
            var vm = App.Container.Resolve<ViewModels.WbsStyleSelectorViewModel>();
            vm.CloseWindow = () => Close();
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
    }
}
