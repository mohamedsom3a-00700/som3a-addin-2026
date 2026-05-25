using System;

namespace Som3a.Plugin.SDK.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SettingsSectionAttribute : Attribute
    {
        public string Category { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Icon { get; set; }
    }
}
