using System.Text.Json;
using System.Text.RegularExpressions;

namespace Som3a.AI.Prompts;

public class TemplateValidator
{
    public TemplateValidationResult Validate(EnhancedPromptTemplate template)
    {
        if (template == null)
            return TemplateValidationResult.Failure("Template cannot be null.");

        if (string.IsNullOrWhiteSpace(template.Name))
            return TemplateValidationResult.Failure("Template name is required.");

        if (string.IsNullOrWhiteSpace(template.SystemPrompt))
            return TemplateValidationResult.Failure("System prompt is required.");

        if (string.IsNullOrWhiteSpace(template.UserPrompt))
            return TemplateValidationResult.Failure("User prompt is required.");

        if (!string.IsNullOrEmpty(template.JsonSchema))
        {
            var schemaResult = ValidateJsonSchema(template.JsonSchema);
            if (!schemaResult.IsSuccess)
                return schemaResult;
        }

        var templateParams = Regex.Matches(template.UserPrompt, @"\{\{(\w+)\}\}")
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        if (templateParams.Count > 0 && template.ContextRequirements.Count == 0)
            return TemplateValidationResult.Warning("Template uses parameters but has no context requirements defined.");

        return TemplateValidationResult.Success();
    }

    private TemplateValidationResult ValidateJsonSchema(string schemaJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(schemaJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp) || typeProp.GetString() != "object")
                return TemplateValidationResult.Failure("JSON Schema must have type 'object' at root.");

            if (!root.TryGetProperty("properties", out _))
                return TemplateValidationResult.Warning("JSON Schema has no properties defined.");

            return TemplateValidationResult.Success();
        }
        catch (JsonException ex)
        {
            return TemplateValidationResult.Failure($"Invalid JSON Schema: {ex.Message}");
        }
    }
}

public class TemplateValidationResult
{
    public bool IsSuccess { get; private set; }
    public bool IsWarning { get; private set; }
    public string Message { get; private set; }

    private TemplateValidationResult(bool success, string message, bool warning = false)
    {
        IsSuccess = success;
        Message = message;
        IsWarning = warning;
    }

    public static TemplateValidationResult Success() => new(true, string.Empty);
    public static TemplateValidationResult Warning(string message) => new(true, message, true);
    public static TemplateValidationResult Failure(string message) => new(false, message);
}
