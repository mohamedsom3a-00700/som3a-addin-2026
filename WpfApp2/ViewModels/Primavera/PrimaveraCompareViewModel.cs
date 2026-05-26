using Som3a.Shared.Core.Primavera;
using Som3a.Shared.Models.Primavera;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.ViewModels.Primavera
{
    public class PrimaveraCompareViewModel : ViewModelBase
    {
        private readonly IPrimaveraDbService _dbService;
        private readonly IPrimaveraDataLoaderService _loaderService;
        private readonly IPrimaveraComparisonService _comparisonService;

        private string _connectionString = string.Empty;
        private string _selectedDatabaseType = "SQLite";
        private string _searchText = string.Empty;
        private int _progressValue;
        private bool _isBusy;
        private string _statusMessage = "Select a Primavera database to begin.";

        public PrimaveraCompareViewModel(IServiceContainer container)
        {
            _dbService = container.Resolve<IPrimaveraDbService>();
            _loaderService = container.Resolve<IPrimaveraDataLoaderService>();
            _comparisonService = container.Resolve<IPrimaveraComparisonService>();

            Projects = new ObservableCollection<ProjectDto>();
            FilteredProjects = new ObservableCollection<ProjectDto>();
            SelectedProjects = new ObservableCollection<ProjectDto>();

            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            CompareCommand = new AsyncRelayCommand(CompareAsync);
        }

        public ObservableCollection<ProjectDto> Projects { get; }

        public ObservableCollection<ProjectDto> FilteredProjects { get; }

        public ObservableCollection<ProjectDto> SelectedProjects { get; }

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (_connectionString == value)
                    return;

                _connectionString = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDatabaseType
        {
            get => _selectedDatabaseType;
            set
            {
                if (_selectedDatabaseType == value)
                    return;

                _selectedDatabaseType = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;
                OnPropertyChanged();
                ApplyProjectFilter();
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue == value)
                    return;

                _progressValue = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; }

        public ICommand CompareCommand { get; }

        public void UseDatabaseFile(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
                return;

            SelectedDatabaseType = "SQLite";
            ConnectionString = $"Data Source={databaseFilePath};Version=3;Read Only=True;";
            StatusMessage = "Connection string generated. Click Connect to load projects.";
        }

        public void SyncSelectedProjects(System.Collections.IEnumerable selectedItems)
        {
            SelectedProjects.Clear();

            if (selectedItems == null)
                return;

            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem is ProjectDto project)
                    SelectedProjects.Add(project);
            }

            StatusMessage = SelectedProjects.Count == 0
                ? "Select exactly two projects to compare."
                : $"{SelectedProjects.Count} project(s) selected.";
        }

        private async Task ConnectAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ConnectionString))
                {
                    MessageBox.Show(
                        "Select a Primavera database file or enter a connection string before connecting.",
                        "Primavera Connection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                IsBusy = true;
                ProgressValue = 10;
                StatusMessage = "Testing database connection...";

                var canConnect = await _dbService.TestConnectionAsync(
                    ConnectionString,
                    SelectedDatabaseType);

                if (!canConnect)
                {
                    ProgressValue = 0;
                    StatusMessage = "Database connection failed.";

                    MessageBox.Show(
                        "Could not connect to the selected Primavera database. Check the connection string, database type, and database availability.",
                        "Primavera Connection Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ProgressValue = 35;
                StatusMessage = "Loading Primavera projects...";

                var projects = await _dbService.GetProjectsAsync(
                    ConnectionString,
                    SelectedDatabaseType);

                Projects.Clear();
                SelectedProjects.Clear();

                foreach (var project in projects)
                    Projects.Add(project);

                ApplyProjectFilter();
                ProgressValue = 100;
                StatusMessage = $"Loaded {Projects.Count} Primavera project(s).";

                MessageBox.Show(
                    $"Connected successfully and loaded {Projects.Count} project(s).",
                    "Primavera Connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProgressValue = 0;
                StatusMessage = "Unable to load projects.";
                MessageBox.Show(
                    $"Unable to load Primavera projects. {ex.Message}",
                    "Primavera Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CompareAsync()
        {
            try
            {
                if (SelectedProjects.Count != 2)
                {
                    MessageBox.Show(
                        "Select exactly two projects from the grid before comparing.",
                        "Primavera Compare",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                IsBusy = true;
                ProgressValue = 10;
                StatusMessage = "Loading first selected project...";

                var p1 = await _loaderService.LoadProjectDataAsync(
                    ConnectionString,
                    SelectedDatabaseType,
                    SelectedProjects[0].ProjectId);

                ProgressValue = 50;
                StatusMessage = "Loading second selected project...";

                var p2 = await _loaderService.LoadProjectDataAsync(
                    ConnectionString,
                    SelectedDatabaseType,
                    SelectedProjects[1].ProjectId);

                ProgressValue = 80;
                StatusMessage = "Comparing project data...";

                var result = await _comparisonService.CompareProjectsAsync(
                    p1,
                    p2);

                ProgressValue = 100;
                StatusMessage = $"Comparison complete. {result.Summary.TotalDifferences} difference(s) found.";

                NavigationService.Instance.NavigationData["ComparisonResult"] = result;
                NavigationService.Instance.NavigateTo("planning.primavera.results");
            }
            catch (Exception ex)
            {
                ProgressValue = 0;
                StatusMessage = "Comparison failed.";
                MessageBox.Show(
                    $"The comparison could not be completed. {ex.Message}",
                    "Primavera Compare Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyProjectFilter()
        {
            FilteredProjects.Clear();

            foreach (var project in Projects)
            {
                if (MatchesSearch(project))
                    FilteredProjects.Add(project);
            }
        }

        private bool MatchesSearch(ProjectDto project)
        {
            if (project == null)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            return (project.ProjectName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (project.ProjectCode?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }


    }
}
