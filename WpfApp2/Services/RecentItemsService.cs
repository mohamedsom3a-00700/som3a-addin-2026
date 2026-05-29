using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public sealed class RecentItemsService : IRecentItemsService
    {
        private const int MaxItems = 5;
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private RecentItemsStore _store;

        public RecentItemsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "Som3a");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "recent-items.json");
            _store = LoadStore();
        }

        public IReadOnlyList<RecentItem> GetRecentTools()
        {
            _semaphore.Wait();
            try
            {
                var copy = _store.RecentTools?.ToList() ?? new List<RecentItem>();
                return copy.AsReadOnly();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IReadOnlyList<RecentItem> GetRecentProjects()
        {
            _semaphore.Wait();
            try
            {
                var copy = _store.RecentProjects?.ToList() ?? new List<RecentItem>();
                return copy.AsReadOnly();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void AddRecentTool(string toolId, string displayName)
        {
            _semaphore.Wait();
            try
            {
                if (_store.RecentTools == null)
                    _store.RecentTools = new List<RecentItem>();

                var existing = _store.RecentTools.FirstOrDefault(x => x.ToolId == toolId);
                if (existing != null)
                {
                    existing.Timestamp = DateTime.UtcNow;
                    existing.DisplayName = displayName;
                }
                else
                {
                    _store.RecentTools.Insert(0, new RecentItem
                    {
                        ToolId = toolId,
                        DisplayName = displayName,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _store.RecentTools = _store.RecentTools
                    .OrderByDescending(x => x.Timestamp)
                    .Take(MaxItems)
                    .ToList();

                SaveStore();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void AddRecentProject(string filePath, string displayName)
        {
            _semaphore.Wait();
            try
            {
                if (_store.RecentProjects == null)
                    _store.RecentProjects = new List<RecentItem>();

                var existing = _store.RecentProjects.FirstOrDefault(x => x.FilePath == filePath);
                if (existing != null)
                {
                    existing.Timestamp = DateTime.UtcNow;
                    existing.DisplayName = displayName;
                }
                else
                {
                    _store.RecentProjects.Insert(0, new RecentItem
                    {
                        FilePath = filePath,
                        DisplayName = displayName,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _store.RecentProjects = _store.RecentProjects
                    .OrderByDescending(x => x.Timestamp)
                    .Take(MaxItems)
                    .ToList();

                SaveStore();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ClearRecentTools()
        {
            _semaphore.Wait();
            try
            {
                _store.RecentTools = new List<RecentItem>();
                SaveStore();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ClearRecentProjects()
        {
            _semaphore.Wait();
            try
            {
                _store.RecentProjects = new List<RecentItem>();
                SaveStore();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private RecentItemsStore LoadStore()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<RecentItemsStore>(json) ?? new RecentItemsStore();
                }
            }
            catch
            {
            }
            return new RecentItemsStore();
        }

        private void SaveStore()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_store, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
            }
        }

        private class RecentItemsStore
        {
            public List<RecentItem> RecentTools { get; set; } = new List<RecentItem>();
            public List<RecentItem> RecentProjects { get; set; } = new List<RecentItem>();
        }
    }
}
