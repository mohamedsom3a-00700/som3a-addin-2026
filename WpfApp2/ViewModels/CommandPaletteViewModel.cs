using System.Collections.ObjectModel;
using System.Windows.Input;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class CommandPaletteViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    UpdateResults();
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public ObservableCollection<NavigationDestination> Results { get; } = new();

        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand ExecuteCommand { get; }
        public ICommand CloseCommand { get; }

        public event System.Action? RequestClose;

        public CommandPaletteViewModel(IServiceContainer container)
        {
            _navigationService = container.Resolve<INavigationService>();

            SelectNextCommand = new RelayCommand(SelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious);
            ExecuteCommand = new RelayCommand(Execute);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void UpdateResults()
        {
            Results.Clear();
            foreach (var item in _navigationService.Search(_searchText))
                Results.Add(item);
            SelectedIndex = Results.Count > 0 ? 0 : -1;
        }

        private void SelectNext()
        {
            if (SelectedIndex < Results.Count - 1)
                SelectedIndex++;
        }

        private void SelectPrevious()
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
        }

        private void Execute()
        {
            if (SelectedIndex >= 0 && SelectedIndex < Results.Count)
            {
                _navigationService.NavigateTo(Results[SelectedIndex].Key);
                RequestClose?.Invoke();
            }
        }

        public void Open()
        {
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            UpdateResults();
        }
    }
}
