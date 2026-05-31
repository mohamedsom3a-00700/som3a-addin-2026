using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a.Bridge;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class AIProviderStatusWidgetViewModel : WidgetViewModel
    {
        private readonly IAIBridge _aiBridge;
        private readonly INavigationService _navigationService;
        private readonly DispatcherTimer _refreshTimer;

        [ObservableProperty]
        private bool _isHostRunning;

        [ObservableProperty]
        private string _status;

        public AIProviderStatusWidgetViewModel(IAIBridge aiBridge, INavigationService navigationService)
        {
            _aiBridge = aiBridge ?? throw new ArgumentNullException(nameof(aiBridge));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "AI Providers";
            Icon = "Bot";

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _refreshTimer.Tick += async (s, e) => await RefreshAsync();
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            try { _navigationService.NavigateTo("settings.general"); }
            catch { }
        }

        protected override Task LoadAsync()
        {
            IsHostRunning = _aiBridge.IsHostRunning;
            Status = IsHostRunning ? "AI Host Running" : "AI Host Offline";

            if (!IsHostRunning)
                ErrorMessage = "No AI providers configured";
            else
                ErrorMessage = null;

            _refreshTimer.Start();
            return Task.CompletedTask;
        }

        public override void Cleanup()
        {
            _refreshTimer.Stop();
            base.Cleanup();
        }
    }
}
