using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class RecentProjectsWidgetViewModel : WidgetViewModel
    {
        private readonly IRecentItemsService _recentItemsService;

        public ObservableCollection<RecentItem> RecentProjects { get; } = new ObservableCollection<RecentItem>();

        public RecentProjectsWidgetViewModel(IRecentItemsService recentItemsService)
        {
            _recentItemsService = recentItemsService ?? throw new ArgumentNullException(nameof(recentItemsService));
            Title = "Recent Projects";
            Icon = "\U000F0214";
        }

        protected override Task LoadAsync()
        {
            RecentProjects.Clear();
            var projects = _recentItemsService.GetRecentProjects();
            foreach (var project in projects)
                RecentProjects.Add(project);

            if (RecentProjects.Count == 0)
                ErrorMessage = "Your recently opened projects will appear here";

            return Task.CompletedTask;
        }
    }
}
