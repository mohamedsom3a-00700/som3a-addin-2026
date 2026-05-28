using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Som3a.Bridge;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class AIProviderStatusWidgetViewModel : WidgetViewModel
    {
        private readonly IAIBridge _aiBridge;
        private readonly DispatcherTimer _refreshTimer;
        private bool _isHostRunning;
        private string _status;

        public bool IsHostRunning
        {
            get => _isHostRunning;
            set => SetProperty(ref _isHostRunning, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ICommand NavigateToSettingsCommand { get; }

        public AIProviderStatusWidgetViewModel(IAIBridge aiBridge, INavigationService navigationService)
        {
            _aiBridge = aiBridge ?? throw new ArgumentNullException(nameof(aiBridge));
            Title = "AI Providers";
            Icon = "\U000F0068";

            NavigateToSettingsCommand = new RelayCommand(() =>
            {
                try { navigationService.NavigateTo("settings.general"); }
                catch { }
            });

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _refreshTimer.Tick += async (s, e) => await RefreshAsync();
        }

        protected override Task LoadAsync()
        {
            IsHostRunning = _aiBridge.IsHostRunning;
            Status = IsHostRunning ? "AI Host Running" : "AI Host Offline";

            if (!IsHostRunning)
                ErrorMessage = "No AI providers configured";

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
