using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class QuickActionsWidgetViewModel : WidgetViewModel
    {
        private readonly INavigationService _navigationService;

        public ICommand NavigateToWBSCommand { get; }
        public ICommand NavigateToBOQCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public QuickActionsWidgetViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Quick Actions";
            Icon = "\U000F0417";

            NavigateToWBSCommand = new RelayCommand(() => NavigateSafe("planning.wbs.generator"));
            NavigateToBOQCommand = new RelayCommand(() => NavigateSafe("planning.boq.activity"));
            NavigateToSettingsCommand = new RelayCommand(() => NavigateSafe("settings.general"));
        }

        private void NavigateSafe(string key)
        {
            try
            {
                if (_navigationService.IsPageRegistered(key))
                    _navigationService.NavigateTo(key);
            }
            catch
            {
            }
        }

        protected override Task LoadAsync()
        {
            return Task.CompletedTask;
        }
    }
}
