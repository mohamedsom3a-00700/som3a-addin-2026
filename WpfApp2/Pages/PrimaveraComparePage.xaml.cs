using Microsoft.Win32;
using Som3a_WPF_UI.ViewModels.Primavera;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public partial class PrimaveraComparePage : Page
    {
        public PrimaveraComparePage()
        {
            InitializeComponent();
            DataContext = App.Container.Resolve<PrimaveraCompareViewModel>();
        }

        private PrimaveraCompareViewModel ViewModel =>
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

            if (dialog.ShowDialog() == true)
                ViewModel?.UseDatabaseFile(dialog.FileName);
        }

        private void ProjectsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                ViewModel?.SyncSelectedProjects(dataGrid.SelectedItems);
        }
    }
}
