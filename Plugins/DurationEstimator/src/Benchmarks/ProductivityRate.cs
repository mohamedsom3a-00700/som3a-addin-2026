namespace Som3a.DurationEstimator.Benchmarks;

public class ProductivityRate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TradeCategoryId { get; set; } = string.Empty;
    public string ActivityDescription { get; set; } = string.Empty;
    public decimal ProductivityValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public int CrewSize { get; set; } = 1;
    public string? Source { get; set; }
    public bool IsBuiltIn { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
}
