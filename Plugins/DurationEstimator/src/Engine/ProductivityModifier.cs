namespace Som3a.DurationEstimator.Engine;

public enum ModifierType
{
    ZonePhased,
    Weather,
    SiteCondition,
    LearningCurve,
    Custom
}

public class ProductivityModifier
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ModifierType ModifierType { get; set; }
    public decimal Percentage { get; set; }
    public string? Description { get; set; }
}
