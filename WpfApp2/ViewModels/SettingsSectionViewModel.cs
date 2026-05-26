using Som3a_WPF_UI.Services;
using System.Collections.ObjectModel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class SettingsSectionViewModel : ViewModelBase
    {
        private readonly SettingsSection _section;
        private bool _isExpanded;

        public SettingsSectionViewModel(SettingsSection section, SettingsValidator validator)
        {
            _section = section;
            _isExpanded = true;

            foreach (var setting in section.Settings)
            {
                Controls.Add(new SettingControlViewModel(setting, validator));
            }
        }

        public string Id => _section.Id;
        public string PluginId => _section.PluginId;
        public string Category => _section.Category;
        public string DisplayName => _section.DisplayName;
        public string? Description => _section.Description;
        public int Order => _section.Order;
        public string? IconKey => _section.IconKey;
        public int Version => _section.Version;

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public ObservableCollection<SettingControlViewModel> Controls { get; } = new();
    }
}
