using System.Collections.Generic;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services.WBS;

public interface IWBSTemplateService
{
    Task<List<WBSTemplateSummary>> ListTemplatesAsync(string? category = null);
    Task<WBSTemplate> GetTemplateAsync(string templateId);
    Task<WBSTemplate> CreateCustomTemplateAsync(string name, string category, WBSNode rootNode, string userId);
    Task ImportTemplateAsync(string filePath);
    Task ExportTemplateAsync(string templateId, string filePath);
    Task ExportTemplateToExcelAsync(string templateId, object excelApp);
    Task<WBSTemplate> ImportTemplateFromExcelAsync(object excelApp, string? sheetName = null, string? category = null);
    List<WBSTemplate> GetRecommendedTemplates(string projectDescription);
}

public class WBSTemplateSummary
{
    public string Id { get; }
    public string Name { get; }
    public string Category { get; }
    public int LevelCount { get; }
    public int NodeCount { get; }
    public bool IsSystem { get; }

    public WBSTemplateSummary(string id, string name, string category, int levelCount, int nodeCount, bool isSystem)
    {
        Id = id; Name = name; Category = category;
        LevelCount = levelCount; NodeCount = nodeCount; IsSystem = isSystem;
    }
}
