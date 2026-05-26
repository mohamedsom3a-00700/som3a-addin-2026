# Contract: IWBSTemplateService

**Layer**: WpfApp2.Services.WBS
**Purpose**: Template CRUD, browsing, recommendation, and import/export.

```csharp
namespace WpfApp2.Services.WBS;

public interface IWBSTemplateService
{
    Task<List<WBSTemplateSummary>> ListTemplatesAsync(string? category = null);
    
    Task<WBSTemplate> GetTemplateAsync(string templateId);
    
    Task<WBSTemplate> CreateCustomTemplateAsync(string name, string category, WBSNode rootNode, string userId);
    
    Task ImportTemplateAsync(string filePath);
    
    Task ExportTemplateAsync(string templateId, string filePath);
    
    List<WBSTemplate> GetRecommendedTemplates(string projectDescription);
}

public record WBSTemplateSummary(
    string Id, string Name, string Category,
    int LevelCount, int NodeCount, bool IsSystem);
```
