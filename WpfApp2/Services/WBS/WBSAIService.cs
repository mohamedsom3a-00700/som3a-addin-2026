using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a.Bridge;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSAIService : IWBSAIService
{
    private readonly IAIBridge _aiBridge;

    public WBSAIService() : this(null) { }

    public WBSAIService(IAIBridge? aiBridge)
    {
        _aiBridge = aiBridge ?? (IAIBridge?)App.Container?.Resolve(typeof(IAIBridge)) ?? throw new InvalidOperationException("IAIBridge not registered in container. Ensure AI bridge is configured.");
    }

    public bool IsAIAvailable => AISettings.IsAIEnabled;

    public async Task<WBSNode> GenerateWBSAsync(string projectDescription, string? preferredCategory = null, CancellationToken ct = default)
    {
        var systemPrompt = "You are a construction planning assistant. Generate a Work Breakdown Structure (WBS) from a project description. Output as a hierarchical JSON tree of WBS nodes with code, name, and description.";
        var userPrompt = $@"Generate a WBS for the following project:

{projectDescription}

{(preferredCategory != null ? $"Preferred category: {preferredCategory}" : "")}

Output as a JSON array of WBSNode objects with properties: code, name, description, children (recursive).";

        var request = new AIBridgeRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.3f,
            MaxTokens = 4096,
            ProviderType = AISettings.ProviderType == AIProviderType.Ollama ? "ollama" : "cloud",
            ApiKey = AISettings.CloudApiKey,
            Model = AISettings.CloudMainModel,
            Endpoint = AISettings.ProviderType == AIProviderType.Ollama ? AISettings.OllamaEndpoint : null
        };

        var response = await _aiBridge.ExecutePromptAsync(request, ct);
        ct.ThrowIfCancellationRequested();

        if (!response.IsSuccess)
            throw new InvalidOperationException($"WBS AI generation failed: {response.ErrorMessage}");

        return ParseWbsResponse(response.Content, projectDescription);
    }

    public async Task<WBSNode> RegenerateWBSAsync(string projectDescription, string previousTreeJson, CancellationToken ct = default)
    {
        var systemPrompt = "You are a construction planning assistant. Refine an existing WBS structure based on a project description.";
        var userPrompt = $@"Improve the following WBS for the project:

{projectDescription}

Current WBS:
{previousTreeJson}

Output as a JSON array of WBSNode objects with properties: code, name, description, children (recursive).";

        var request = new AIBridgeRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.3f,
            MaxTokens = 4096,
            ProviderType = AISettings.ProviderType == AIProviderType.Ollama ? "ollama" : "cloud",
            ApiKey = AISettings.CloudApiKey,
            Model = AISettings.CloudMainModel,
            Endpoint = AISettings.ProviderType == AIProviderType.Ollama ? AISettings.OllamaEndpoint : null
        };

        var response = await _aiBridge.ExecutePromptAsync(request, ct);
        ct.ThrowIfCancellationRequested();

        if (!response.IsSuccess)
            throw new InvalidOperationException($"WBS re-generation failed: {response.ErrorMessage}");

        return ParseWbsResponse(response.Content, projectDescription);
    }

    public async Task<WBSNode> GenerateWBSFromExcelAsync(string excelContext, CancellationToken ct = default)
    {
        var systemPrompt = "You are a construction planning assistant. Generate a Work Breakdown Structure (WBS) from Excel BOQ data.";
        var userPrompt = $@"Generate a WBS from the following Excel data:

{excelContext}

Output as a JSON array of WBSNode objects with properties: code, name, description, children (recursive).";

        var request = new AIBridgeRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.3f,
            MaxTokens = 4096,
            ProviderType = AISettings.ProviderType == AIProviderType.Ollama ? "ollama" : "cloud",
            ApiKey = AISettings.CloudApiKey,
            Model = AISettings.CloudMainModel,
            Endpoint = AISettings.ProviderType == AIProviderType.Ollama ? AISettings.OllamaEndpoint : null
        };

        var response = await _aiBridge.ExecutePromptAsync(request, ct);
        ct.ThrowIfCancellationRequested();

        if (!response.IsSuccess)
            throw new InvalidOperationException($"WBS Excel generation failed: {response.ErrorMessage}");

        return ParseWbsResponse(response.Content, "Excel Import");
    }

    private static WBSNode ParseWbsResponse(string json, string fallbackName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new WBSNode { Name = fallbackName, Code = "1", Children = new() };

        try
        {
            var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<WBSNode>>(json);
            if (nodes != null && nodes.Count > 0)
            {
                var root = new WBSNode
                {
                    Name = fallbackName,
                    Code = "1",
                    Children = nodes
                };
                return root;
            }
        }
        catch { }

        return new WBSNode { Name = fallbackName, Code = "1", Children = new() };
    }
}
