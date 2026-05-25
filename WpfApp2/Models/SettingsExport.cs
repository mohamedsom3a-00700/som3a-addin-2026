using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Models
{
    public sealed class SettingsExport
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime ExportedAt { get; set; }
        public UserSettings Settings { get; set; } = new();
        public string AppVersion { get; set; } = "";
    }

    public sealed class ImportResult
    {
        public UserSettings Settings { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
