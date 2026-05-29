namespace Som3a.DurationEstimator.Benchmarks;

public interface ITradeCategoryRegistry
{
    IEnumerable<TradeCategory> GetAll();
    TradeCategory GetById(string id);
}

public class TradeCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsBuiltIn { get; set; } = true;
}

public class TradeCategoryRegistry : ITradeCategoryRegistry
{
    private static readonly List<TradeCategory> BuiltInCategories = new()
    {
        new() { Id = "concrete", Name = "Concrete", DisplayOrder = 1 },
        new() { Id = "steel", Name = "Steel", DisplayOrder = 2 },
        new() { Id = "masonry", Name = "Masonry", DisplayOrder = 3 },
        new() { Id = "mep", Name = "MEP (Mechanical, Electrical, Plumbing)", DisplayOrder = 4 },
        new() { Id = "finishes", Name = "Finishes", DisplayOrder = 5 },
        new() { Id = "earthwork", Name = "Earthwork", DisplayOrder = 6 },
        new() { Id = "roofing", Name = "Roofing", DisplayOrder = 7 },
        new() { Id = "insulation", Name = "Insulation", DisplayOrder = 8 },
        new() { Id = "glazing", Name = "Glazing", DisplayOrder = 9 },
        new() { Id = "painting", Name = "Painting", DisplayOrder = 10 },
        new() { Id = "flooring", Name = "Flooring", DisplayOrder = 11 },
        new() { Id = "tiling", Name = "Tiling", DisplayOrder = 12 },
        new() { Id = "landscaping", Name = "Landscaping", DisplayOrder = 13 },
        new() { Id = "roadwork", Name = "Roadwork", DisplayOrder = 14 },
        new() { Id = "piling", Name = "Piling", DisplayOrder = 15 },
        new() { Id = "waterproofing", Name = "Waterproofing", DisplayOrder = 16 }
    };

    private readonly List<TradeCategory> _categories = new(BuiltInCategories);

    public IEnumerable<TradeCategory> GetAll() =>
        _categories.OrderBy(c => c.DisplayOrder).Select(c => Clone(c));

    public TradeCategory GetById(string id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            throw new KeyNotFoundException($"Trade category '{id}' not found.");
        return Clone(category);
    }

    private static TradeCategory Clone(TradeCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        DisplayOrder = c.DisplayOrder,
        IsBuiltIn = c.IsBuiltIn
    };
}
