using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.Ui
{
    public partial class ProjectAnalysisWindow : ModernWindow
    {
        public ProjectAnalysisWindow(object excelApp)
        {
            InitializeComponent();
            DataContext = new ProjectAnalysisViewModel(excelApp, this);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
