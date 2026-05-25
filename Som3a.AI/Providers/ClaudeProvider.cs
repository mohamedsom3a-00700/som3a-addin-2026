using System.Text;
using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Providers
{
    public class ClaudeProvider : AIProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public override string ProviderId => "claude";
        public override string ProviderName => "Claude";
        public override bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

        public ClaudeProvider(string apiKey, string model = "claude-3-sonnet-20240229")
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public override async Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var payload = new
                {
                    model = _model,
                    max_tokens = request.MaxTokens,
                    temperature = (double)request.Temperature,
                    system = request.SystemPrompt,
                    messages = new[]
                    {
                        new { role = "user", content = request.UserPrompt }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("messages", content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new AIResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Claude API error: {response.StatusCode}",
                        ProviderId = ProviderId,
                        Duration = sw.Elapsed
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(responseJson);
                var contentBlock = doc.RootElement.GetProperty("content")[0];
                var text = contentBlock.GetProperty("text").GetString() ?? string.Empty;
                var usage = doc.RootElement.GetProperty("usage");

                return new AIResponse
                {
                    Content = text,
                    Usage = new TokenUsage
                    {
                        PromptTokens = usage.GetProperty("input_tokens").GetInt32(),
                        CompletionTokens = usage.GetProperty("output_tokens").GetInt32()
                    },
                    ProviderId = ProviderId,
                    Duration = sw.Elapsed,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new AIResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ProviderId = ProviderId,
                    Duration = sw.Elapsed
                };
            }
        }

        public override async IAsyncEnumerable<AIStreamChunk> StreamPromptAsync(AIRequest request, CancellationToken ct = default)
        {
            var result = await ExecutePromptAsync(request, ct);
            int index = 0;
            foreach (var word in result.Content.Split(' '))
            {
                yield return new AIStreamChunk { Delta = word + " ", Index = index++, IsFinal = false };
            }
            yield return new AIStreamChunk { Delta = "", Index = index, IsFinal = true };
        }
    }
}
