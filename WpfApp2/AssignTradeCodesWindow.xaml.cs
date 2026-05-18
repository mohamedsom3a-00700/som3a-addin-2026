using System.Windows;
using System.Windows.Input;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI
{
    public partial class AssignTradeCodesWindow : ModernWindow
    {
        private readonly AssignTradeCodesViewModel _vm;

        public AssignTradeCodesWindow()
        {
            InitializeComponent();

            _vm = new AssignTradeCodesViewModel();
            DataContext = _vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Initialize();
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
