using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public sealed class ChangelogService : IChangelogService
    {
        private readonly string _filePath;
        private List<ChangelogEntry> _cachedEntries;

        public ChangelogService()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            _filePath = Path.Combine(appDir, "CHANGELOG.md");

            if (!File.Exists(_filePath))
            {
                var repoRoot = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", ".."));
                var altPath = Path.Combine(repoRoot, "CHANGELOG.md");
                if (File.Exists(altPath))
                    _filePath = altPath;
            }
        }

        public ChangelogEntry GetLatestEntry()
        {
            var entries = GetAllEntries();
            return entries.Count > 0 ? entries[0] : new ChangelogEntry
            {
                Version = "0.0.0",
                Date = DateTime.MinValue,
                Changes = new List<string> { "No changelog available" }
            };
        }

        public IReadOnlyList<ChangelogEntry> GetAllEntries()
        {
            if (_cachedEntries != null)
                return _cachedEntries.AsReadOnly();

            _cachedEntries = ParseChangelog();
            return _cachedEntries.AsReadOnly();
        }

        private List<ChangelogEntry> ParseChangelog()
        {
            var entries = new List<ChangelogEntry>();

            try
            {
                if (!File.Exists(_filePath))
                    return entries;

                var lines = File.ReadAllLines(_filePath);
                ChangelogEntry current = null;
                var changes = new List<string>();

                foreach (var line in lines)
                {
                    if (line.StartsWith("## ["))
                    {
                        if (current != null)
                        {
                            current.Changes = changes.AsReadOnly();
                            entries.Add(current);
                            changes = new List<string>();
                        }

                        current = ParseHeaderLine(line);
                    }
                    else if (current != null && line.TrimStart().StartsWith("- "))
                    {
                        changes.Add(line.TrimStart().Substring(2).Trim());
                    }
                }

                if (current != null)
                {
                    current.Changes = changes.AsReadOnly();
                    entries.Add(current);
                }
            }
            catch
            {
            }

            return entries;
        }

        private ChangelogEntry ParseHeaderLine(string line)
        {
            var entry = new ChangelogEntry();

            try
            {
                var startBracket = line.IndexOf('[');
                var endBracket = line.IndexOf(']');
                if (startBracket >= 0 && endBracket > startBracket)
                {
                    entry.Version = line.Substring(startBracket + 1, endBracket - startBracket - 1);
                }

                var dashIndex = line.IndexOf(" - ", endBracket > 0 ? endBracket : 0);
                if (dashIndex >= 0)
                {
                    var dateStr = line.Substring(dashIndex + 3).Trim();
                    if (DateTime.TryParse(dateStr, out var date))
                    {
                        entry.Date = date;
                    }
                }
            }
            catch
            {
            }

            return entry;
        }
    }
}
