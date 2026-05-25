using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Parsing
{
    public class StructuredOutputParser
    {
        private readonly JsonSchemaValidator _schemaValidator;

        public StructuredOutputParser(JsonSchemaValidator schemaValidator)
        {
            _schemaValidator = schemaValidator;
        }

        public T? Parse<T>(string content, string? schema = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            if (schema != null)
            {
                var validation = _schemaValidator.Validate(content, schema);
                if (!validation.IsSuccess)
                    return null;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<T>(content, options);
            }
            catch
            {
                return null;
            }
        }
    }
}
