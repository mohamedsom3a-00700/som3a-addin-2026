using System.Text.RegularExpressions;
using Som3a.Contracts;

namespace Som3a.AI.Prompts
{
    public class PromptRepository : IPromptProvider
    {
        private readonly Dictionary<string, PromptTemplate> _templates = new();

        public PromptRepository()
        {
            InitializeDefaultTemplates();
        }

        private void InitializeDefaultTemplates()
        {
            _templates["boq-to-activities"] = new PromptTemplate
            {
                Id = "boq-to-activities",
                Category = "BOQ",
                Version = "1.0.0",
                SystemPrompt = "You are a construction planning assistant. Generate activities from BOQ items.",
                UserPromptTemplate = "Given the following BOQ, generate construction activities:\n\n{{boqContext}}",
                Temperature = 0.3f,
                MaxTokens = 4096
            };

            _templates["wbs-generate"] = new PromptTemplate
            {
                Id = "wbs-generate",
                Category = "WBS",
                Version = "1.0.0",
                SystemPrompt = "You are a construction WBS expert. Create a work breakdown structure.",
                UserPromptTemplate = "Create a WBS for a {{projectType}} project with {{numLevels}} levels.",
                Temperature = 0.3f,
                MaxTokens = 4096
            };

            _templates["review-schedule"] = new PromptTemplate
            {
                Id = "review-schedule",
                Category = "Review",
                Version = "1.0.0",
                SystemPrompt = "You are a construction schedule reviewer. Identify issues and suggest improvements.",
                UserPromptTemplate = "Review this schedule and identify issues:\n\n{{scheduleContext}}",
                Temperature = 0.3f,
                MaxTokens = 4096
            };
        }

        public Task<PromptTemplate> GetTemplateAsync(string templateId, CancellationToken ct = default)
        {
            if (_templates.TryGetValue(templateId, out var template))
                return Task.FromResult(template);

            throw new KeyNotFoundException($"Prompt template '{templateId}' not found.");
        }

        public Task<string> BuildContextAsync<T>(T entity, CancellationToken ct = default) where T : class
        {
            var context = entity?.ToString() ?? string.Empty;
            return Task.FromResult(context);
        }

        public ValidationResult ValidatePrompt(PromptTemplate template, Dictionary<string, string> parameters)
        {
            if (template == null)
                return ValidationResult.Failure("Template cannot be null.");

            if (string.IsNullOrWhiteSpace(template.UserPromptTemplate))
                return ValidationResult.Failure("UserPromptTemplate is empty.");

            var templateParams = Regex
                .Matches(template.UserPromptTemplate, @"\{\{(\w+)\}\}")
                .Select(m => m.Groups[1].Value)
                .Distinct();

            var missing = templateParams.Where(p => !parameters.ContainsKey(p)).ToList();
            if (missing.Count > 0)
                return ValidationResult.Failure($"Missing parameters: {string.Join(", ", missing)}");

            return ValidationResult.Success();
        }

        public Task<IReadOnlyList<PromptTemplate>> ListTemplatesAsync(string? category = null)
        {
            var templates = category == null
                ? _templates.Values.ToList()
                : _templates.Values.Where(t => t.Category == category).ToList();

            return Task.FromResult<IReadOnlyList<PromptTemplate>>(templates);
        }

        public void RegisterTemplate(PromptTemplate template)
        {
            _templates[template.Id] = template;
        }
    }
}
