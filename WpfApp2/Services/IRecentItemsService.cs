using System.Collections.Generic;

namespace Som3a_WPF_UI.Services
{
    public interface IRecentItemsService
    {
        IReadOnlyList<Models.RecentItem> GetRecentTools();
        IReadOnlyList<Models.RecentItem> GetRecentProjects();
        void AddRecentTool(string toolId, string displayName);
        void AddRecentProject(string filePath, string displayName);
        void ClearRecentTools();
        void ClearRecentProjects();
    }
}
