using System.Text.Json;
using Som3a.AI.Orchestration;
using Som3a.AI.Parsers;
using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public class WBSAIService : IWBSAIService
{
    private readonly OrchestrationEngine? _engine;
    private readonly WBSParser _parser;

    public bool IsAIAvailable => _engine != null;

    public WBSAIService(OrchestrationEngine? engine = null)
    {
        _engine = engine;
        _parser = new WBSParser();
    }

    public async Task<WBSNode> GenerateWBSAsync(string projectDescription, string? preferredCategory = null, CancellationToken ct = default)
    {
        if (_engine == null)
            throw new InvalidOperationException("AI is not available. Use manual WBS editing instead.");

        var context = new Dictionary<string, object>
        {
            ["projectDescription"] = projectDescription,
            ["preferredCategory"] = preferredCategory ?? ""
        };

        var result = await _engine.ExecuteAsync("wbs-generate", context, ct);

        if (!result.IsSuccess || result.Response == null)
            throw new InvalidOperationException(result.Execution.ErrorMessage ?? "AI generation failed.");

        var parseResult = await _parser.ParseAsync(result.Response.Content, ct);
        if (!parseResult.IsValid || parseResult.Entity == null)
            throw new InvalidOperationException("AI generated an invalid WBS structure.");

        return parseResult.Entity;
    }

    public async Task<WBSNode> RegenerateWBSAsync(string projectDescription, string previousTreeJson, CancellationToken ct = default)
    {
        return await GenerateWBSAsync(projectDescription + " (revised)", null, ct);
    }
}
