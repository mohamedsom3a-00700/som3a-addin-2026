using System;
using Newtonsoft.Json;

namespace Som3a_WPF_UI.Models
{
    public class RecentItem
    {
        [JsonProperty("toolId")]
        public string ToolId { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
