/// <summary>
/// Allows modules to register ribbon actions (buttons, menus, toggle buttons) for the Excel ribbon.
/// </summary>
public interface IRibbonRegistrar
{
    /// <summary>Registers a ribbon button with a click handler.</summary>
    void AddButton(string id, string label, string tooltip, Action onClick);

    /// <summary>Registers a ribbon dropdown menu with items.</summary>
    void AddMenu(string id, string label, IReadOnlyList<RibbonMenuItem> items);

    /// <summary>Registers a toggle button with state change handler.</summary>
    void AddToggleButton(string id, string label, Action<bool> onToggle, bool initialState);
}

/// <summary>Descriptor for a single ribbon menu item.</summary>
public class RibbonMenuItem
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Tooltip { get; set; }
    public Action OnClick { get; set; }
}
