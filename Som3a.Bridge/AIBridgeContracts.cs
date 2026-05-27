using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a.Bridge
{
    public class AIBridgeRequest : InteropDtoBase
    {
        public string SystemPrompt { get; set; } = string.Empty;
        public string UserPrompt { get; set; } = string.Empty;
        public string? JsonSchema { get; set; }
        public float Temperature { get; set; } = 0.3f;
        public int MaxTokens { get; set; } = 4096;
        public string? ProviderId { get; set; }
        public string? ProviderType { get; set; }
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public string? Endpoint { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public override string Serialize() => JsonSerializer.Serialize(this, _jsonOptions);
        public override void Deserialize(string json)
        {
            var other = JsonSerializer.Deserialize<AIBridgeRequest>(json, _jsonOptions);
            if (other != null)
            {
                Id = other.Id;
                SystemPrompt = other.SystemPrompt;
                UserPrompt = other.UserPrompt;
                JsonSchema = other.JsonSchema;
                Temperature = other.Temperature;
                MaxTokens = other.MaxTokens;
                ProviderId = other.ProviderId;
                ProviderType = other.ProviderType;
                ApiKey = other.ApiKey;
                Model = other.Model;
                Endpoint = other.Endpoint;
            }
        }
    }

    public class AIBridgeResponse
    {
        public string Content { get; set; } = string.Empty;
        public string? ParsedJson { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ProviderId { get; set; }
        public double DurationMs { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public string Serialize() => JsonSerializer.Serialize(this, _jsonOptions);
        public static AIBridgeResponse? Deserialize(string json) =>
            JsonSerializer.Deserialize<AIBridgeResponse>(json, _jsonOptions);
    }

    public interface IAIBridge
    {
        Task<AIBridgeResponse> ExecutePromptAsync(AIBridgeRequest request, CancellationToken ct = default);
        bool IsHostRunning { get; }
        Task StartHostAsync();
        Task StopHostAsync();
    }
}
