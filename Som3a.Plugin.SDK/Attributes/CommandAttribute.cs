using System;

namespace Som3a.Plugin.SDK.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? KeyGesture { get; set; }
        public string? RibbonGroup { get; set; }
    }
}
