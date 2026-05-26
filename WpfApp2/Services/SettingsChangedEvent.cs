using System;

namespace Som3a_WPF_UI.Services
{
    public sealed class SettingsChangedEvent
    {
        public string ModuleId { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
