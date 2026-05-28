using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Services
{
    public interface IChangelogService
    {
        ChangelogEntry GetLatestEntry();
        IReadOnlyList<ChangelogEntry> GetAllEntries();
    }

    public class ChangelogEntry
    {
        public string Version { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public IReadOnlyList<string> Changes { get; set; } = Array.Empty<string>();
    }
}
