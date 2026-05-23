using Microsoft.Win32;
using Som3a_WPF_UI.ViewModels.Primavera;
using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI;

namespace Som3a_WPF_UI.Windows.PrimaveraComparison
{
    public partial class PrimaveraCompareWindow : ModernWindow
    {
        public PrimaveraCompareWindow()
        {
            InitializeComponent();

            DataContext = App.Container.Resolve<PrimaveraCompareViewModel>();
        }

        private PrimaveraCompareViewModel? ViewModel =>
            DataContext as PrimaveraCompareViewModel;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Primavera SQLite database file",
                Filter = "SQLite database files (*.db)|*.db|SQLite files (*.sqlite;*.sqlite3)|*.sqlite;*.sqlite3|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog(this) == true)
                ViewModel?.UseDatabaseFile(dialog.FileName);
        }

        private void ProjectsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                ViewModel?.SyncSelectedProjects(dataGrid.SelectedItems);
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
