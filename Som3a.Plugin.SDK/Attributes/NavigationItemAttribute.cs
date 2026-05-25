using System;

namespace Som3a.Plugin.SDK.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class NavigationItemAttribute : Attribute
    {
        public string Category { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Order { get; set; }
        public Type? PageType { get; set; }
    }
}
