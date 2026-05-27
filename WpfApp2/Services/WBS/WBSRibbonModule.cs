using System;
using System.Windows;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Services.WBS;

public static class WBSRibbonModule
{
    public static void Register(IRibbonRegistrar ribbon, NavigationService navigation)
    {
        ribbon.AddMenu("wbs", "WBS Engine", new[]
        {
            new RibbonMenuItem { Id = "wbs.browser", Label = "WBS Templates", Tooltip = "Browse and select WBS templates", OnClick = () => NavigateTo(navigation, "planning.wbs.browser") },
            new RibbonMenuItem { Id = "wbs.generator", Label = "AI WBS Generator", Tooltip = "Generate WBS using AI from project description", OnClick = () => NavigateTo(navigation, "planning.wbs.generator") },
            new RibbonMenuItem { Id = "wbs.editor", Label = "WBS Editor", Tooltip = "Manually edit WBS tree structure", OnClick = () => NavigateTo(navigation, "planning.wbs.editor") }
        });
    }

    private static void NavigateTo(NavigationService navigation, string pageKey)
    {
        try
        {
            navigation.NavigateTo(pageKey);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open WBS page: {ex.Message}", "WBS Engine", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
