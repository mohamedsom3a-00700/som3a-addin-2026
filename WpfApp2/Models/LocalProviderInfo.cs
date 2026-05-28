using System.Collections.Generic;

namespace Som3a_WPF_UI.Models
{
    public sealed class LocalProviderInfo
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = string.Empty;
        public string FallbackModel { get; set; } = string.Empty;
        public List<string> AvailableModels { get; set; } = new();

        public override string ToString() => DisplayName;
    }
}
