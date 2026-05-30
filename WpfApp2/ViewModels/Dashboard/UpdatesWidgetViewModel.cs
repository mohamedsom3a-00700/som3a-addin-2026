using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class UpdatesWidgetViewModel : WidgetViewModel
    {
        private readonly IChangelogService _changelogService;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private IReadOnlyList<string> _changes;

        public UpdatesWidgetViewModel(IChangelogService changelogService)
        {
            _changelogService = changelogService ?? throw new ArgumentNullException(nameof(changelogService));
            Title = "Latest Updates";
            Icon = "\U000F0117";
        }

        protected override Task LoadAsync()
        {
            var entry = _changelogService.GetLatestEntry();
            if (entry != null && !string.IsNullOrEmpty(entry.Version))
            {
                Version = entry.Version;
                Date = entry.Date;
                Changes = entry.Changes;
            }
            else
            {
                ErrorMessage = "No updates available";
            }
            return Task.CompletedTask;
        }
    }
}
