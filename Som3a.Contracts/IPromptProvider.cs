namespace Som3a.Contracts
{
    public interface IPromptProvider
    {
        Task<PromptTemplate> GetTemplateAsync(string templateId, CancellationToken ct = default);
        Task<string> BuildContextAsync<T>(T entity, CancellationToken ct = default) where T : class;
        ValidationResult ValidatePrompt(PromptTemplate template, Dictionary<string, string> parameters);
        Task<IReadOnlyList<PromptTemplate>> ListTemplatesAsync(string? category = null);
    }

    public class PromptTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public string UserPromptTemplate { get; set; } = string.Empty;
        public string? JsonSchema { get; set; }
        public int MaxTokens { get; set; } = 4096;
        public float Temperature { get; set; } = 0.3f;
        public List<string>? Examples { get; set; }
    }

    public class PromptContext
    {
        public Dictionary<string, object> EntityData { get; set; } = new();
        public Dictionary<string, string> Parameters { get; set; } = new();
        public int EstimatedTokenCount { get; set; }
    }
}
