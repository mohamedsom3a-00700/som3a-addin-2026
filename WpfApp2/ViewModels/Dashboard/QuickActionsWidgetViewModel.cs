using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class QuickActionsWidgetViewModel : WidgetViewModel
    {
        private readonly INavigationService _navigationService;

        public QuickActionsWidgetViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Quick Actions";
            Icon = "\U000F0417";
        }

        [RelayCommand]
        private void NavigateToWBS()
        {
            NavigateSafe("planning.wbs.generator");
        }

        [RelayCommand]
        private void NavigateToBOQ()
        {
            NavigateSafe("planning.boq.activity");
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            NavigateSafe("settings.general");
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
