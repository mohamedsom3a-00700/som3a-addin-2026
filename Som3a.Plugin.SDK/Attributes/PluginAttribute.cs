using System;

namespace Som3a.Plugin.SDK.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginAttribute : Attribute
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public int Priority { get; set; } = 100;
        public string[]? Dependencies { get; set; }
    }
}
