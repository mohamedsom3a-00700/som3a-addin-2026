using System;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI
{
    public partial class UnmergeFillDownWindow : ModernWindow
    {
        private readonly Excel.Application _excelApp;

        public UnmergeFillDownWindow(Excel.Application excelApp)
        {
            InitializeComponent();
            _excelApp = excelApp ?? throw new ArgumentNullException(nameof(excelApp));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new UnmergeFillDownViewModel(App.Container, _excelApp, closeAction: Close);
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
