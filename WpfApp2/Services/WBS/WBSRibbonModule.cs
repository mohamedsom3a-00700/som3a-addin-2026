using System;
using System.Windows;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSRibbonModule : IModule
{
    public string ModuleId => "wbs.ribbon";
    public string Name => "WBS Engine Ribbon";
    public int Priority => 100;

    public void Initialize(IServiceContainer container, IEventBus eventBus)
    {
        try
        {
            var navigation = container.Resolve<NavigationService>();
            var registrar = container.Resolve<IRibbonRegistrar>();
            registrar.AddMenu("wbs", "WBS Engine", new[]
            {
                new RibbonMenuItem { Id = "wbs.browser", Label = "WBS Templates", Tooltip = "Browse and select WBS templates", OnClick = () => NavigateTo(navigation, "planning.wbs.browser") },
                new RibbonMenuItem { Id = "wbs.generator", Label = "AI WBS Generator", Tooltip = "Generate WBS using AI from project description", OnClick = () => NavigateTo(navigation, "planning.wbs.generator") },
                new RibbonMenuItem { Id = "wbs.editor", Label = "WBS Editor", Tooltip = "Manually edit WBS tree structure", OnClick = () => NavigateTo(navigation, "planning.wbs.editor") }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[WBSRibbonModule] Initialization failed: {ex.Message}");
        }
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
