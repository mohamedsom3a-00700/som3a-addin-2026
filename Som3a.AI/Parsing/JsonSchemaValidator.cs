using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Parsing
{
    public class JsonSchemaValidator
    {
        public ValidationResult Validate(string json, string schemaJson)
        {
            if (string.IsNullOrWhiteSpace(json))
                return ValidationResult.Failure("JSON content is empty.");

            try
            {
                using var doc = JsonDocument.Parse(json);
                return ValidationResult.Success();
            }
            catch (JsonException ex)
            {
                return ValidationResult.Failure($"JSON parse error: {ex.Message}");
            }
        }
    }
}
