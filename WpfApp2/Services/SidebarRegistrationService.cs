using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Controls.Shell;

namespace Som3a_WPF_UI.Services
{
    public class SidebarRegistrationService : ISidebarRegistrationProvider
    {
        private readonly NavigationService _navigationService;
        private readonly HashSet<string> _registeredItemIds = new HashSet<string>();
        private bool _staticPagesRegistered;
        private bool _pluginPagesRegistered;

        public SidebarRegistrationService()
        {
            _navigationService = NavigationService.Instance;
        }

        public void RegisterStaticPages()
        {
            if (_staticPagesRegistered) return;
            _staticPagesRegistered = true;

            _navigationService.RegisterPage("Planning", typeof(Pages.ProjectAnalysisPage), "planning.analysis", "Project Analysis", "ChartTimelineVariant", 10);
            _navigationService.RegisterPage("Planning", typeof(Pages.PrimaveraComparePage), "planning.primavera.compare", "Primavera Compare", "Compare", 20);
            _navigationService.RegisterPage("Planning", typeof(Pages.PrimaveraResultsPage), "planning.primavera.results", "Primavera Results", "FileDocument", 30);
            _navigationService.RegisterPage("Planning", typeof(Pages.XerEditorPage), "planning.xereditor", "XER Editor", "FileCode", 40);

            _navigationService.RegisterPage("Analysis", typeof(Pages.FloatPathPage), "analysis.floatpath", "Float Path", "Routes", 10);

            _navigationService.RegisterPage("Excel", typeof(Pages.LinksManagerPage), "excel.links", "Links Manager", "LinkVariant", 10);
            _navigationService.RegisterPage("Excel", typeof(Pages.SubDailyReportPage), "excel.subdaily", "Sub-Daily Report", "FileTable", 20);
            _navigationService.RegisterPage("Excel", typeof(Pages.AssignTradeCodesPage), "excel.tradecodes", "Assign Trade Codes", "Tag", 30);
            _navigationService.RegisterPage("Excel", typeof(Pages.FixPieColorsPage), "excel.piecolors", "Fix Pie Colors", "Palette", 40);
            _navigationService.RegisterPage("Excel", typeof(Pages.StyleSelectorPage), "excel.styles", "Style Selector", "FormatPaint", 50);
            _navigationService.RegisterPage("Excel", typeof(Pages.UnmergeFillDownPage), "excel.unmerge", "Unmerge Fill Down", "TableMergeCells", 60);

            _navigationService.RegisterPage("Settings", typeof(Pages.SettingsPage), "settings.general", "Settings", "Cog", 10);

            _navigationService.RegisterPage("Other", typeof(Pages.WelcomePage), "welcome", "Home", null, 0);
            _navigationService.RegisterPage("Other", typeof(Pages.MainPage), "main", "Comparison P6", null, 10);

            foreach (var d in _navigationService.Destinations)
            {
                if (d.Key == "welcome" || d.Key == "main")
                    d.IsVisible = false;
            }
        }

        public void RegisterPluginPages(IEnumerable<Type> pageTypes)
        {
            if (pageTypes == null)
                return;

            ClearPluginPages();
            _pluginPagesRegistered = true;

            foreach (var pageType in pageTypes)
            {
                if (pageType == null) continue;
                try
                {
                    RegisterPluginPage(pageType);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register plugin page '{pageType?.FullName ?? "<null>"}': {ex.Message}");
                }
            }
        }

        private void RegisterPluginPage(Type pageType)
        {
            var attr = pageType.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.FullName is "Som3a.Contracts.NavigationItemAttribute" or "Som3a.Plugin.SDK.Attributes.NavigationItemAttribute");

            var category = ReadAttrProperty(attr, "Category") ?? "Other";
            var label = ReadAttrProperty(attr, "Label") ?? pageType.Name;
            var icon = ReadAttrProperty(attr, "Icon") ?? "HelpCircleOutline";
            var order = ReadAttrPropertyAsInt(attr, "Order") ?? 50;
            var itemId = pageType.FullName;

            if (_registeredItemIds.Contains(itemId))
            {
                System.Diagnostics.Debug.WriteLine($"Duplicate ItemId '{itemId}' rejected for plugin page '{pageType.FullName}'.");
                return;
            }

            if (!IsValidCategory(category))
            {
                System.Diagnostics.Debug.WriteLine($"Invalid category '{category}' for plugin page '{pageType.FullName}'. Falling back to 'Other'.");
                category = "Other";
            }

            var key = $"plugin.{itemId}";

            _navigationService.RegisterPage(category, pageType, key, label, icon, order);
            _registeredItemIds.Add(itemId);
        }

        private static bool IsValidCategory(string category)
        {
            return category switch
            {
                "Planning" => true,
                "Analysis" => true,
                "Excel" => true,
                "AI" => true,
                "Settings" => true,
                "Other" => true,
                _ => false
            };
        }

        public void ClearPluginPages()
        {
            _registeredItemIds.Clear();
            _pluginPagesRegistered = false;
        }

        private static string ReadAttrProperty(System.Reflection.CustomAttributeData attr, string propertyName)
        {
            if (attr == null) return null;
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.MemberName == propertyName)
                    return namedArg.TypedValue.Value?.ToString();
            }
            if (attr.ConstructorArguments.Count > 0 && attr.Constructor != null)
            {
                var parameters = attr.Constructor.GetParameters();
                for (int i = 0; i < parameters.Length && i < attr.ConstructorArguments.Count; i++)
                {
                    if (string.Equals(parameters[i].Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return attr.ConstructorArguments[i].Value?.ToString();
                }
            }
            return null;
        }

        private static int? ReadAttrPropertyAsInt(System.Reflection.CustomAttributeData attr, string propertyName)
        {
            if (attr == null) return null;
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.MemberName == propertyName && namedArg.TypedValue.Value is int intVal)
                    return intVal;
            }
            return null;
        }
    }
}
