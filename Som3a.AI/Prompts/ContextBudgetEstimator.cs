namespace Som3a.AI.Prompts;

public class ContextBudgetEstimator
{
    private const double CharactersPerToken = 4.0;
    private const int DefaultMaxTokens = 8192;
    private const int ResponseReserve = 2048;

    public BudgetEstimate Estimate(EnhancedPromptTemplate template, string userContext, int? providerMaxTokens = null)
    {
        var systemTokens = EstimateTokenCount(template.SystemPrompt);
        var userTokens = EstimateTokenCount(template.UserPrompt);
        var contextTokens = EstimateTokenCount(userContext);
        var maxTokens = providerMaxTokens ?? DefaultMaxTokens;

        var totalEstimated = systemTokens + userTokens + contextTokens + ResponseReserve;
        var fitsInWindow = totalEstimated <= maxTokens;
        var availableForResponse = maxTokens - (systemTokens + userTokens + contextTokens);

        return new BudgetEstimate
        {
            SystemPromptTokens = systemTokens,
            UserPromptTokens = userTokens,
            ContextTokens = contextTokens,
            ResponseReserve = ResponseReserve,
            EstimatedTotal = totalEstimated,
            ProviderMaxTokens = maxTokens,
            FitsInWindow = fitsInWindow,
            AvailableForResponse = Math.Max(0, availableForResponse)
        };
    }

    public static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)Math.Ceiling(text.Length / CharactersPerToken);
    }
}

public class BudgetEstimate
{
    public int SystemPromptTokens { get; set; }
    public int UserPromptTokens { get; set; }
    public int ContextTokens { get; set; }
    public int ResponseReserve { get; set; }
    public int EstimatedTotal { get; set; }
    public int ProviderMaxTokens { get; set; }
    public bool FitsInWindow { get; set; }
    public int AvailableForResponse { get; set; }
}
