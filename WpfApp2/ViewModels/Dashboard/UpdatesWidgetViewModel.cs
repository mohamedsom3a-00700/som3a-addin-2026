using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class UpdatesWidgetViewModel : WidgetViewModel
    {
        private readonly IChangelogService _changelogService;
        private string _version;
        private DateTime _date;
        private IReadOnlyList<string> _changes;

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public IReadOnlyList<string> Changes
        {
            get => _changes;
            set => SetProperty(ref _changes, value);
        }

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
