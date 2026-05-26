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
    }

    protected abstract Task<TEntity> ParseEntityAsync(JsonElement element, CancellationToken ct);

    protected virtual List<ParserError> ValidateAgainstSchema(JsonElement element)
    {
        var errors = new List<ParserError>();
        var schema = ExpectedSchema.RootElement;

        if (schema.TryGetProperty("required", out var required))
        {
            foreach (var field in required.EnumerateArray())
            {
                var fieldName = field.GetString();
                if (fieldName != null && !element.TryGetProperty(fieldName, out _))
                {
                    errors.Add(new ParserError(fieldName, $"Missing required field: {fieldName}", ParserErrorSeverity.Error));
                }
            }
        }

        return errors;
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
