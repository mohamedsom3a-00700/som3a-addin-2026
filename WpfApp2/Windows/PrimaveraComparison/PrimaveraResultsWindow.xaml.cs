using Som3a.Shared.Core.Primavera;
using Som3a_WPF_UI.ViewModels.Primavera;
using System.Windows;
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI;

namespace Som3a_WPF_UI.Windows.PrimaveraComparison
{
    public partial class PrimaveraResultsWindow : ModernWindow
    {
        public PrimaveraResultsWindow(
            ComparisonResult result)
        {
            InitializeComponent();

            DataContext =
                new PrimaveraResultsViewModel(App.Container, result);
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