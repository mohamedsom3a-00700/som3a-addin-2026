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
            _navigationService.RegisterPage("Planning", typeof(Pages.WBS.WBSTemplateBrowserPage), "planning.wbs.browser", "WBS Templates", "FileTree", 50);
            _navigationService.RegisterPage("Planning", typeof(Pages.WBS.WBSGeneratorPage), "planning.wbs.generator", "WBS Generator", "Creation", 60);
            _navigationService.RegisterPage("Planning", typeof(Pages.WBS.WBSEditorPage), "planning.wbs.editor", "WBS Editor", "PencilRuler", 70);
            _navigationService.RegisterPage("Planning", typeof(Pages.BOQActivityGeneratorPage), "planning.boq.activity", "BOQ Activity Generator", "Creation", 80);
            _navigationService.RegisterPage("Planning", typeof(Pages.RelationshipGeneratorPage), "planning.relationship", "Relationship Generator", "Connection", 90);
            _navigationService.RegisterPage("Planning", typeof(Pages.DurationEstimatorPage), "planning.duration", "Duration Estimator", "Timer", 100);
            _navigationService.RegisterPage("Analysis", typeof(Pages.FloatPathPage), "analysis.floatpath", "Float Path", "Routes", 10);
            _navigationService.RegisterPage("Excel", typeof(Pages.LinksManagerPage), "excel.links", "Links Manager", "LinkVariant", 10);
            _navigationService.RegisterPage("Excel", typeof(Pages.SubDailyReportPage), "excel.subdaily", "Sub-Daily Report", "FileTable", 20);
            _navigationService.RegisterPage("Excel", typeof(Pages.AssignTradeCodesPage), "excel.tradecodes", "Assign Trade Codes", "Tag", 30);
            _navigationService.RegisterPage("Excel", typeof(Pages.FixPieColorsPage), "excel.piecolors", "Fix Pie Colors", "Palette", 40);
            _navigationService.RegisterPage("Excel", typeof(Pages.StyleSelectorPage), "excel.styles", "Style Selector", "FormatPaint", 50);
            _navigationService.RegisterPage("Excel", typeof(Pages.UnmergeFillDownPage), "excel.unmerge", "Unmerge Fill Down", "TableMergeCells", 60);
            _navigationService.RegisterPage("Home", typeof(Pages.HomePage), "home", "Home", "Home", 0);
            _navigationService.RegisterPage("Settings", typeof(Pages.SettingsPage), "settings.general", "Settings", "Cog", 10);
            _navigationService.RegisterPage("Settings", typeof(Pages.DiagnosticsPage), "diagnostics", "Diagnostics", "MonitorDashboard", 99);
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

                var category = ReadAttrProperty(pageType, "Category") ?? "Other";
                var label = ReadAttrProperty(pageType, "Label") ?? pageType.Name;
                var icon = ReadAttrProperty(pageType, "Icon") ?? "Puzzle";
                var order = ReadAttrPropertyAsInt(pageType, "Order");
                var priority = ReadAttrPropertyAsInt(pageType, "Priority");

                var key = $"{category}.{label}".ToLowerInvariant().Replace(" ", ".");
                _navigationService.RegisterPage(category, pageType, key, label, icon, priority);
            }
        }

        public void RegisterPluginPage(Type pageType)
        {
            if (pageType == null) return;

            var category = ReadAttrProperty(pageType, "Category") ?? "Other";
            var label = ReadAttrProperty(pageType, "Label") ?? pageType.Name;
            var icon = ReadAttrProperty(pageType, "Icon") ?? "Puzzle";
            var order = ReadAttrPropertyAsInt(pageType, "Order");
            var priority = ReadAttrPropertyAsInt(pageType, "Priority");

            var key = $"{category}.{label}".ToLowerInvariant().Replace(" ", ".");
            _navigationService.RegisterPage(category, pageType, key, label, icon, priority);
        }

        public void ClearPluginPages()
        {
            _pluginPagesRegistered = false;
        }

        private static string ReadAttrProperty(Type type, string propertyName)
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                var attrType = attr.GetType();
                if (attrType.FullName == "Som3a.Plugin.SDK.Attributes.NavigationItemAttribute" ||
                    attrType.FullName == "Som3a.Contracts.NavigationItemAttribute")
                {
                    var prop = attrType.GetProperty(propertyName);
                    if (prop != null && prop.GetValue(attr) is string val)
                        return val;
                }
            }
            return null;
        }

        private static int ReadAttrPropertyAsInt(Type type, string propertyName)
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                var attrType = attr.GetType();
                if (attrType.FullName == "Som3a.Plugin.SDK.Attributes.NavigationItemAttribute" ||
                    attrType.FullName == "Som3a.Contracts.NavigationItemAttribute")
                {
                    var prop = attrType.GetProperty(propertyName);
                    if (prop != null && prop.GetValue(attr) is int val)
                        return val;
                }
            }
            return 0;
        }

        private static bool IsValidCategory(string category)
        {
            var valid = new HashSet<string> { "Home", "Planning", "Analysis", "Excel", "Settings", "Other" };
            return valid.Contains(category);
        }
    }
}
