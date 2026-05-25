using System;

namespace Som3a_WPF_UI.Models
{
    public sealed class SettingsCategory
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Icon { get; set; } = "";
        public Type PanelType { get; set; } = null!;
        public int Order { get; set; }
    }
}
