using Som3a_WPF_UI.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed partial class SettingsSectionViewModel : ViewModelBase
    {
        private readonly SettingsSection _section;

        [ObservableProperty]
        private bool _isExpanded = true;

        public SettingsSectionViewModel(SettingsSection section, SettingsValidator validator)
        {
            _section = section;

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

        public ObservableCollection<SettingControlViewModel> Controls { get; } = new();
    }
}
