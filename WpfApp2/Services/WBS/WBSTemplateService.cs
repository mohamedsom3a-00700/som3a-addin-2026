using System.Text.Json;
using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public class WBSTemplateService : IWBSTemplateService
{
    private readonly string _storagePath;
    private readonly List<WBSTemplate> _systemTemplates;
    private readonly Dictionary<string, List<string>> _categoryKeywords;

    public WBSTemplateService()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "wbs-templates");
        Directory.CreateDirectory(_storagePath);

        _systemTemplates = LoadSystemTemplates();
        _categoryKeywords = new Dictionary<string, List<string>>
        {
            ["Building"] = new() { "residential", "commercial", "industrial", "building", "office", "apartment", "warehouse", "retail", "hospitality", "hotel", "hospital" },
            ["Infrastructure"] = new() { "road", "bridge", "utility", "highway", "tunnel", "pipeline", "railway", "water", "wastewater", "drainage" },
            ["MEP"] = new() { "mechanical", "electrical", "plumbing", "hvac", "fire", "lighting", "power", "control" },
            ["Industrial"] = new() { "oil", "gas", "manufacturing", "petrochemical", "refinery", "factory", "plant", "processing" },
            ["Fitout"] = new() { "office fitout", "retail fitout", "hospitality fitout", "interior", "finish", "furnishing", "decoration" }
        };
    }

    public Task<List<WBSTemplateSummary>> ListTemplatesAsync(string? category = null)
    {
        var query = _systemTemplates.AsEnumerable();
        if (category != null)
            query = query.Where(t => t.Category == category);

        var summaries = query.Select(t => new WBSTemplateSummary(
            t.Id, t.Name, t.Category,
            t.RootNode != null ? GetMaxLevel(t.RootNode) : 0,
            t.RootNode != null ? CountNodes(t.RootNode) : 0,
            t.IsSystem
        )).ToList();

        return Task.FromResult(summaries);
    }

    public Task<WBSTemplate> GetTemplateAsync(string templateId)
    {
        var template = _systemTemplates.FirstOrDefault(t => t.Id == templateId);
        if (template != null)
            return Task.FromResult(template);

        var customPath = Path.Combine(_storagePath, $"{templateId}.json");
        if (File.Exists(customPath))
        {
            var json = File.ReadAllText(customPath);
            template = JsonSerializer.Deserialize<WBSTemplate>(json);
            if (template != null)
                return Task.FromResult(template);
        }

        throw new KeyNotFoundException($"Template '{templateId}' not found.");
    }

    public Task<WBSTemplate> CreateCustomTemplateAsync(string name, string category, WBSNode rootNode, string userId)
    {
        var template = new WBSTemplate
        {
            Name = name,
            Category = category,
            RootNode = rootNode,
            IsSystem = false,
            OwnerId = userId,
            Version = 1
        };

        template.Validate();
        var path = Path.Combine(_storagePath, $"{template.Id}.json");
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);

        return Task.FromResult(template);
    }

    public Task ImportTemplateAsync(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var template = JsonSerializer.Deserialize<WBSTemplate>(json);
        if (template == null)
            throw new InvalidDataException("Invalid template file.");

        template.Validate();
        var destPath = Path.Combine(_storagePath, $"{template.Id}.json");
        File.Copy(filePath, destPath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task ExportTemplateAsync(string templateId, string filePath)
    {
        var template = _systemTemplates.FirstOrDefault(t => t.Id == templateId);
        if (template == null)
        {
            var customPath = Path.Combine(_storagePath, $"{templateId}.json");
            if (!File.Exists(customPath))
                throw new KeyNotFoundException($"Template '{templateId}' not found.");
            File.Copy(customPath, filePath, overwrite: true);
            return Task.CompletedTask;
        }

        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }

    public List<WBSTemplate> GetRecommendedTemplates(string projectDescription)
    {
        if (string.IsNullOrWhiteSpace(projectDescription))
            return _systemTemplates.Take(3).ToList();

        var lower = projectDescription.ToLower();
        var scores = new Dictionary<string, int>();

        foreach (var kvp in _categoryKeywords)
        {
            var score = kvp.Value.Count(kw => lower.Contains(kw));
            if (score > 0)
                scores[kvp.Key] = score;
        }

        var bestCategory = scores.OrderByDescending(s => s.Value).FirstOrDefault().Key;
        if (bestCategory == null)
            return _systemTemplates.Take(3).ToList();

        return _systemTemplates.Where(t => t.Category == bestCategory).ToList();
    }

    private List<WBSTemplate> LoadSystemTemplates()
    {
        var templates = new List<WBSTemplate>();
        var categories = new[] { "Building", "Infrastructure", "MEP", "Industrial", "Fitout" };

        foreach (var cat in categories)
        {
            for (int i = 1; i <= 3; i++)
            {
                templates.Add(new WBSTemplate
                {
                    Name = $"{cat} Template {i}",
                    Category = cat,
                    Description = $"Standard {cat.ToLower()} WBS template variant {i}",
                    RootNode = CreateSampleTree(cat, i),
                    IsSystem = true
                });
            }
        }

        return templates;
    }

    private static WBSNode CreateSampleTree(string category, int variant)
    {
        var root = new WBSNode { Code = "1", Name = category };
        var child1 = new WBSNode { Code = "1.1", Name = $"Sub-area {variant}.1" };
        var child2 = new WBSNode { Code = "1.2", Name = $"Sub-area {variant}.2" };
        var grandchild = new WBSNode { Code = "1.1.1", Name = $"Detail {variant}.1.1" };

        child1.Children.Add(grandchild);
        grandchild.Parent = child1;
        root.Children.Add(child1);
        child1.Parent = root;
        root.Children.Add(child2);
        child2.Parent = root;

        return root;
    }

    private static int GetMaxLevel(WBSNode node)
    {
        if (node.Children.Count == 0) return 1;
        return 1 + node.Children.Max(GetMaxLevel);
    }

    private static int CountNodes(WBSNode node)
    {
        return 1 + node.Children.Sum(CountNodes);
    }
}
