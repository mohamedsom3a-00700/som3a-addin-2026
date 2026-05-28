# Engine Contracts: Duration Estimator

Internal interfaces between plugin sub-modules.

---

## IProductivityEngine

```csharp
public interface IProductivityEngine
{
    /// <param name="quantity">Activity quantity (must be > 0)</param>
    /// <param name="productivityRate">Rate in quantity per crew-day (must be > 0)</param>
    /// <param name="crewSize">Number of crew (must be >= 1)</param>
    /// <param name="hoursPerDay">Working hours per day (must be > 0, <= 24)</param>
    /// <returns>Duration in working days</returns>
    decimal CalculateWorkingDays(
        decimal quantity,
        decimal productivityRate,
        int crewSize,
        decimal hoursPerDay
    );

    /// <summary>Applies modifiers to base rate.</summary>
    decimal ApplyModifiers(
        decimal baseRate,
        IEnumerable<ProductivityModifier> modifiers
    );
}
```

**Contract Rules**:
- Returns 0 if quantity is 0 (activity flagged as invalid)
- Modifiers are additive percentages; result clamped to minimum of 0.1% of base rate
- No rounding — callers round for display (2 decimal places)

---

## IBenchmarkLibrary

```csharp
public interface IBenchmarkLibrary
{
    ProductivityRate GetById(string id);
    IEnumerable<ProductivityRate> GetByTradeCategory(string tradeCategoryId);
    IEnumerable<ProductivityRate> Search(string query); // searches ActivityDescription
    ProductivityRate Add(ProductivityRate rate);
    ProductivityRate Update(ProductivityRate rate);
    void Delete(string id);
    void ImportBuiltIn(); // reloads embedded benchmark data
}
```

**Contract Rules**:
- GetById throws KeyNotFoundException if not found
- Add rejects duplicates (same TradeCategoryId + ActivityDescription + UnitOfMeasure)
- Delete performs soft delete (sets IsActive = false)
- ImportBuiltIn preserves existing user rates; updates built-in rates by Id

---

## ICalendarEngine

```csharp
public interface ICalendarEngine
{
    void Configure(CalendarConfig config);
    CalendarConfig GetConfig();
    int CalculateCalendarDays(DateTime startDate, decimal workingDays);
    DateTime CalculateEndDate(DateTime startDate, decimal workingDays);
    bool IsWorkingDay(DateTime date);
    int CountWorkingDays(DateTime start, DateTime end);
}
```

**Contract Rules**:
- CalculateEndDate: startDate + workingDays - 1, skipping non-working days and holidays
- CountWorkingDays: count of working days in [start, end] inclusive
- Throws InvalidOperationException if CalendarConfig has zero working days

---

## IVarianceAnalyzer

```csharp
public interface IVarianceAnalyzer
{
    VarianceResult CalculateThreePoint(
        decimal optimisticRate,
        decimal mostLikelyRate,
        decimal pessimisticRate,
        decimal quantity,
        int crewSize,
        decimal hoursPerDay
    );

    VarianceResult CalculateFromSingle(
        decimal singleRate,
        decimal quantity,
        int crewSize,
        decimal hoursPerDay
    );
}

public record VarianceResult
{
    public decimal OptimisticDuration { get; init; }
    public decimal MostLikelyDuration { get; init; }
    public decimal PessimisticDuration { get; init; }
    public decimal ExpectedDuration { get; init; }     // (O + 4M + P) / 6
    public decimal StandardDeviation { get; init; }    // (P - O) / 6
    public decimal Confidence90Lower { get; init; }
    public decimal Confidence90Upper { get; init; }
    public decimal Confidence95Lower { get; init; }
    public decimal Confidence95Upper { get; init; }
    public bool HasSingleRate { get; init; }           // true if pessimistic=optimistic
}
```

**Contract Rules**:
- CalculateFromSingle sets HasSingleRate = true; all three durations equal the single-rate value
- StandardDeviation is 0 for single rate inputs; confidence intervals use MostLikely as center
- OptimisticRate must be >= MostLikelyRate >= PessimisticRate (higher rate = shorter duration)

---

## IAnomalyDetector

```csharp
public interface IAnomalyDetector
{
    AnomalyResult Analyze(
        DurationEstimate estimate,
        IEnumerable<DurationEstimate> peerEstimates // same trade category
    );
}

public record AnomalyResult
{
    public bool IsAnomaly { get; init; }
    public AnomalyType Type { get; init; }
    public string Explanation { get; init; }
    public decimal DeviationFactor { get; init; } // e.g., 3.5x standard deviation
}

public enum AnomalyType
{
    TooLong,       // Duration > 3x peer average
    TooShort,      // Duration < 0.3x peer average
    Impossible     // Duration > 5 years or < 1 hour
}
```

---

## IAIProductivitySuggestor

```csharp
public interface IAIProductivitySuggestor
{
    Task<ProductivitySuggestion> SuggestRateAsync(
        Activity activity,
        string projectType,
        CancellationToken ct
    );

    Task<IReadOnlyList<AnomalyResult>> AnalyzeDurationsAsync(
        IReadOnlyList<DurationEstimate> estimates,
        CancellationToken ct
    );
}

public record ProductivitySuggestion
{
    public decimal SuggestedRate { get; init; }
    public decimal ConfidenceScore { get; init; }  // 0.0 to 1.0
    public List<string> SourceReferences { get; init; }
    public string Reasoning { get; init; }
}
```

**Contract Rules**:
- SuggestRateAsync returns null if AI provider unavailable
- ConfidenceScore >= 0.7 is considered reliable; < 0.4 is flagged as low confidence
- AI suggestions are advisory; the user must explicitly accept to apply
