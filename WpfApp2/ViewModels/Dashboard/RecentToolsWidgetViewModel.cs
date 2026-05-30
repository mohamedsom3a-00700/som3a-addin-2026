using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class RecentToolsWidgetViewModel : WidgetViewModel
    {
        private readonly IRecentItemsService _recentItemsService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<RecentItem> RecentTools { get; } = new ObservableCollection<RecentItem>();

        public RecentToolsWidgetViewModel(IRecentItemsService recentItemsService, INavigationService navigationService)
        {
            _recentItemsService = recentItemsService ?? throw new ArgumentNullException(nameof(recentItemsService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Recent Tools";
            Icon = "\U000F024B";

            _navigationService.NavigationChanged += OnNavigationChanged;
        }

        protected override Task LoadAsync()
        {
            RecentTools.Clear();
            var tools = _recentItemsService.GetRecentTools();
            foreach (var tool in tools)
                RecentTools.Add(tool);

            if (RecentTools.Count == 0)
                ErrorMessage = "Your recently used tools will appear here";

            return Task.CompletedTask;
        }

        [RelayCommand]
        private void NavigateToTool(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return;
            try
            {
                _navigationService.NavigateTo(toolId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigateToTool failed for '{toolId}': {ex.Message}");
            }
        }

        private void OnNavigationChanged(object sender, Som3a_WPF_UI.Controls.Shell.NavigationEventArgs e)
        {
            if (e.Success && !string.IsNullOrEmpty(e.NewKey))
            {
                try
                {
                    _recentItemsService.AddRecentTool(e.NewKey, e.NewKey);
                    App.Current?.Dispatcher?.Invoke(async () => await RefreshAsync());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnNavigationChanged failed for '{e.NewKey}': {ex.Message}");
                }
            }
        }

        public override void Cleanup()
        {
            _navigationService.NavigationChanged -= OnNavigationChanged;
            base.Cleanup();
        }
    }
}
