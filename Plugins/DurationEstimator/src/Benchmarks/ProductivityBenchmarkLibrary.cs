using System.Text.Json;

namespace Som3a.DurationEstimator.Benchmarks;

public class ProductivityBenchmarkLibrary : IBenchmarkLibrary
{
    private readonly List<ProductivityRate> _rates = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductivityBenchmarkLibrary()
    {
        ImportBuiltIn();
        LoadCustom();
    }

    public ProductivityRate GetById(string id)
    {
        var rate = _rates.FirstOrDefault(r => r.Id == id && r.IsActive);
        if (rate == null)
            throw new KeyNotFoundException($"Productivity rate '{id}' not found.");
        return rate;
    }

    public IEnumerable<ProductivityRate> GetByTradeCategory(string tradeCategoryId)
    {
        return _rates.Where(r => r.TradeCategoryId == tradeCategoryId && r.IsActive);
    }

    public IEnumerable<ProductivityRate> Search(string query)
    {
        return _rates.Where(r => r.IsActive && r.ActivityDescription.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public ProductivityRate Add(ProductivityRate rate)
    {
        if (_rates.Any(r => r.IsActive && r.TradeCategoryId == rate.TradeCategoryId && r.ActivityDescription == rate.ActivityDescription && r.UnitOfMeasure == rate.UnitOfMeasure))
            throw new InvalidOperationException("Duplicate benchmark: same category, description, and unit already exists.");

        rate.Id = Guid.NewGuid().ToString("N");
        rate.IsActive = true;
        rate.IsBuiltIn = false;
        rate.Version = 1;
        _rates.Add(rate);
        SaveCustom();
        return rate;
    }

    public ProductivityRate Update(ProductivityRate rate)
    {
        var existing = _rates.FirstOrDefault(r => r.Id == rate.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Productivity rate '{rate.Id}' not found.");

        if (existing.IsBuiltIn)
        {
            rate.IsBuiltIn = true;
            rate.IsActive = existing.IsActive;
        }

        rate.Version = existing.Version + 1;
        _rates.Remove(existing);
        _rates.Add(rate);
        SaveCustom();
        return rate;
    }

    public void Delete(string id)
    {
        var existing = _rates.FirstOrDefault(r => r.Id == id);
        if (existing == null)
            throw new KeyNotFoundException($"Productivity rate '{id}' not found.");

        if (existing.IsBuiltIn)
            throw new InvalidOperationException("Built-in benchmarks cannot be deleted.");

        existing.IsActive = false;
        SaveCustom();
    }

    public void ImportBuiltIn()
    {
        _rates.RemoveAll(r => r.IsBuiltIn);
        try
        {
            var assembly = typeof(ProductivityBenchmarkLibrary).Assembly;
            var resourceName = "Som3a.DurationEstimator.Benchmarks.BenchmarkData.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var doc = JsonDocument.Parse(json);
            var categories = doc.RootElement.GetProperty("categories");

            foreach (var category in categories.EnumerateArray())
            {
                var categoryId = category.GetProperty("id").GetString()!;
                var rates = category.GetProperty("rates");

                foreach (var rateObj in rates.EnumerateArray())
                {
                    _rates.Add(new ProductivityRate
                    {
                        Id = rateObj.GetProperty("id").GetString()!,
                        TradeCategoryId = categoryId,
                        ActivityDescription = rateObj.GetProperty("activityDescription").GetString()!,
                        ProductivityValue = rateObj.GetProperty("productivityValue").GetDecimal(),
                        UnitOfMeasure = rateObj.GetProperty("unitOfMeasure").GetString()!,
                        CrewSize = rateObj.GetProperty("crewSize").GetInt32(),
                        IsBuiltIn = true,
                        IsActive = true,
                        Version = 1
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImportBuiltIn failed: {ex.Message}");
        }
    }

    public IEnumerable<ProductivityRate> GetAllActive()
    {
        return _rates.Where(r => r.IsActive).OrderBy(r => r.TradeCategoryId).ThenBy(r => r.ActivityDescription);
    }

    private void LoadCustom()
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Som3a", "DurationEstimator");
            var path = Path.Combine(dir, "benchmarks.json");
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var custom = JsonSerializer.Deserialize<List<ProductivityRate>>(json, JsonOptions);
            if (custom != null)
            {
                foreach (var rate in custom)
                {
                    if (!_rates.Any(r => r.Id == rate.Id))
                    {
                        rate.IsBuiltIn = false;
                        rate.IsActive = true;
                        _rates.Add(rate);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCustom failed: {ex.Message}");
        }
    }

    private void SaveCustom()
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Som3a", "DurationEstimator");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "benchmarks.json");
            var custom = _rates.Where(r => !r.IsBuiltIn && r.IsActive);
            var json = JsonSerializer.Serialize(custom, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveCustom failed: {ex.Message}");
        }
    }
}
