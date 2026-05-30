using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class CommandPaletteViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            UpdateResults();
        }

        [ObservableProperty]
        private int _selectedIndex;

        public ObservableCollection<NavigationDestination> Results { get; } = new();

        [RelayCommand]
        private void SelectNext()
        {
            if (SelectedIndex < Results.Count - 1)
                SelectedIndex++;
        }

        [RelayCommand]
        private void SelectPrevious()
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
        }

        [RelayCommand]
        private void Execute()
        {
            if (SelectedIndex >= 0 && SelectedIndex < Results.Count)
            {
                _navigationService.NavigateTo(Results[SelectedIndex].Key);
                RequestClose?.Invoke();
            }
        }

        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke();
        }

        public event System.Action? RequestClose;

        public CommandPaletteViewModel(IServiceContainer container)
        {
            if (container is null) throw new ArgumentNullException(nameof(container));
            _navigationService = container.Resolve<INavigationService>();
        }

        private void UpdateResults()
        {
            Results.Clear();
            foreach (var item in _navigationService.Search(_searchText))
                Results.Add(item);
            SelectedIndex = Results.Count > 0 ? 0 : -1;
        }

        public void Open()
        {
            SearchText = string.Empty;
            UpdateResults();
        }
    }
}
