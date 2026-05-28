namespace Som3a_WPF_UI.Models
{
    public enum WidgetType
    {
        Version = 0,
        Updates = 1,
        RecentTools = 2,
        RecentProjects = 3,
        DiagnosticsSummary = 4,
        AIProviderStatus = 5,
        PerformanceSummary = 6,
        QuickActions = 7,
        PluginStatus = 8
    }

    public class WidgetDefinition
    {
        public WidgetType WidgetType { get; }
        public string Title { get; }
        public string Icon { get; }
        public int DisplayOrder { get; }

        public WidgetDefinition(WidgetType widgetType, string title, string icon, int displayOrder)
        {
            WidgetType = widgetType;
            Title = title;
            Icon = icon;
            DisplayOrder = displayOrder;
        }
    }
}
