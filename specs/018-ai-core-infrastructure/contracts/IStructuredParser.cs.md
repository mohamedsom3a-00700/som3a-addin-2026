# Contract: IStructuredParser

**Layer**: Som3a.AI.Parsers
**Purpose**: Generic structured output parser — validates AI response against JSON Schema and produces typed domain entities.

```csharp
namespace Som3a.AI.Parsers;

public interface IStructuredParser<TEntity>
{
    string ParserType { get; }
    
    JsonDocument ExpectedSchema { get; }
    
    Task<ParserResult<TEntity>> ParseAsync(
        string rawResponse,
        CancellationToken ct);
}

public record ParserResult<TEntity>(
    bool IsValid,
    TEntity? Entity,
    IReadOnlyList<ParserError> Errors);

public record ParserError(
    string Field,
    string Message,
    ParserErrorSeverity Severity);

public enum ParserErrorSeverity { Warning, Error }

// Concrete parser contracts are provided by implementations:
// ActivityParser : IStructuredParser<List<Activity>>
// WBSParser : IStructuredParser<WBSNode>
// RelationshipParser : IStructuredParser<List<Relationship>>
// DurationParser : IStructuredParser<DurationResult>
// ReviewParser : IStructuredParser<ReviewResult>
```
