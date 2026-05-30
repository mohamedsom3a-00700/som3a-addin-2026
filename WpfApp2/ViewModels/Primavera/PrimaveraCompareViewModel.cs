using Som3a.Shared.Core.Primavera;
using Som3a.Shared.Models.Primavera;
using Som3a_WPF_UI.Helpers;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Som3a_WPF_UI.ViewModels.Primavera
{
    public partial class PrimaveraCompareViewModel : ViewModelBase
    {
        private readonly IPrimaveraDbService _dbService;
        private readonly IPrimaveraDataLoaderService _loaderService;
        private readonly IPrimaveraComparisonService _comparisonService;

        [ObservableProperty]
        private string _connectionString = string.Empty;

        [ObservableProperty]
        private string _selectedDatabaseType = "SQLite";

        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            ApplyProjectFilter();
        }

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "Select a Primavera database to begin.";

        public PrimaveraCompareViewModel(IServiceContainer container)
        {
            _dbService = container.Resolve<IPrimaveraDbService>();
            _loaderService = container.Resolve<IPrimaveraDataLoaderService>();
            _comparisonService = container.Resolve<IPrimaveraComparisonService>();

            Projects = new ObservableCollection<ProjectDto>();
            FilteredProjects = new ObservableCollection<ProjectDto>();
            SelectedProjects = new ObservableCollection<ProjectDto>();
        }

        public ObservableCollection<ProjectDto> Projects { get; }

        public ObservableCollection<ProjectDto> FilteredProjects { get; }

        public ObservableCollection<ProjectDto> SelectedProjects { get; }

        [RelayCommand]
        private async Task Connect()
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

        [RelayCommand]
        private async Task Compare()
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
