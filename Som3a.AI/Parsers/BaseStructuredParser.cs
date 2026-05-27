using System.Text.Json;

namespace Som3a.AI.Parsers;

public abstract class BaseStructuredParser<TEntity>
{
    public abstract string ParserType { get; }
    public abstract JsonDocument ExpectedSchema { get; }

    public async Task<ParserResult<TEntity>> ParseAsync(string rawResponse, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return ParserResult<TEntity>.Failure("Response content is empty.");

        try
        {
            using var doc = JsonDocument.Parse(rawResponse);
            var schemaErrors = ValidateAgainstSchema(doc.RootElement);
            if (schemaErrors.Count > 0)
                return ParserResult<TEntity>.Failure(schemaErrors);

            var entity = await ParseEntityAsync(doc.RootElement, ct);
            return ParserResult<TEntity>.Success(entity);
        }
        catch (JsonException ex)
        {
            return ParserResult<TEntity>.Failure($"Invalid JSON format: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ParserResult<TEntity>.Failure($"Parsing failed: {ex.Message}");
        }
    }

    protected abstract Task<TEntity> ParseEntityAsync(JsonElement element, CancellationToken ct);

    protected virtual List<ParserError> ValidateAgainstSchema(JsonElement element)
    {
        var errors = new List<ParserError>();
        ValidateRecursive(ExpectedSchema.RootElement, element, "$", errors);
        return errors;
    }

    private void ValidateRecursive(JsonElement schema, JsonElement element, string path, List<ParserError> errors)
    {
        if (schema.TryGetProperty("required", out var required))
        {
            foreach (var field in required.EnumerateArray())
            {
                var fieldName = field.GetString();
                if (fieldName != null && !element.TryGetProperty(fieldName, out _))
                {
                    errors.Add(new ParserError($"{path}.{fieldName}", $"Missing required field: {fieldName}", ParserErrorSeverity.Error));
                }
            }
        }

        if (schema.TryGetProperty("properties", out var properties) && element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in properties.EnumerateObject())
            {
                if (element.TryGetProperty(prop.Name, out var childElement))
                {
                    var childSchema = prop.Value;
                    ValidateRecursive(childSchema, childElement, $"{path}.{prop.Name}", errors);
                }
            }
        }

        if (schema.TryGetProperty("items", out var itemsSchema) && element.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (var item in element.EnumerateArray())
            {
                ValidateRecursive(itemsSchema, item, $"{path}[{index}]", errors);
                index++;
            }
        }
    }
}

public record ParserResult<TEntity>(bool IsValid, TEntity? Entity, List<ParserError> Errors)
{
    public static ParserResult<TEntity> Success(TEntity entity) => new(true, entity, new());
    public static ParserResult<TEntity> Failure(string message) =>
        new(false, default, new() { new ParserError("", message, ParserErrorSeverity.Error) });
    public static ParserResult<TEntity> Failure(List<ParserError> errors) =>
        new(false, default, errors);
}

public record ParserError(string Field, string Message, ParserErrorSeverity Severity);

public enum ParserErrorSeverity { Warning, Error }
