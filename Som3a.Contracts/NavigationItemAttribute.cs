namespace Som3a.Contracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NavigationItemAttribute : Attribute
{
    public string Category { get; set; } = "Other";
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "HelpCircleOutline";
    public int Order { get; set; } = 50;
    public int Priority { get; set; } = 0;
}
