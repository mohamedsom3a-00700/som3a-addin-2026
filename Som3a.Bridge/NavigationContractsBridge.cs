using System;
using System.Collections.Generic;

namespace Som3a.Bridge
{
    public class NavigationContractsBridge
    {
        public List<DiscoveredNavigationPage> DiscoverPagesFromTypes(IEnumerable<Type> pageTypes)
        {
            var pages = new List<DiscoveredNavigationPage>();

            if (pageTypes == null) return pages;

            foreach (var type in pageTypes)
            {
                if (type == null) continue;
                foreach (var attr in type.GetCustomAttributes(false))
                {
                    var attrType = attr.GetType();
                    if (attrType.FullName == "Som3a.Contracts.NavigationItemAttribute" || attrType.FullName == "Som3a.Plugin.SDK.Attributes.NavigationItemAttribute")
                    {
                        var category = GetAttributeProperty<string>(attr, "Category") ?? "Other";
                        var label = GetAttributeProperty<string>(attr, "Label") ?? type.Name;
                        var icon = GetAttributeProperty<string>(attr, "Icon") ?? "HelpCircleOutline";
                        var order = GetAttributeProperty<int>(attr, "Order");
                        var priority = GetAttributeProperty<int>(attr, "Priority");

                        pages.Add(new DiscoveredNavigationPage
                        {
                            PageType = type,
                            Category = category,
                            Label = label,
                            Icon = icon,
                            Order = order,
                            Priority = priority
                        });
                    }
                }
            }

            return pages;
        }

        private static T GetAttributeProperty<T>(object attr, string propertyName)
        {
            var prop = attr.GetType().GetProperty(propertyName);
            if (prop != null && prop.GetValue(attr) is T value)
                return value;
            return default;
        }
    }

    public class DiscoveredNavigationPage
    {
        public Type PageType { get; set; }
        public string Category { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public int Priority { get; set; }
    }
}
