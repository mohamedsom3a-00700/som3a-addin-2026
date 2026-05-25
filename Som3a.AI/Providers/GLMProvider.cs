using System.Text;
using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Providers
{
    public class GLMProvider : AIProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public override string ProviderId => "glm";
        public override string ProviderName => "GLM (ZhipuAI)";
        public override bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

        public GLMProvider(string apiKey, string model = "glm-4")
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://open.bigmodel.cn/api/paas/v4/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public override async Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var payload = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = request.SystemPrompt },
                        new { role = "user", content = request.UserPrompt }
                    },
                    max_tokens = request.MaxTokens,
                    temperature = request.Temperature
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("chat/completions", content, ct);

                if (!response.IsSuccessStatusCode)
                    return new AIResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"GLM API error: {response.StatusCode}",
                        ProviderId = ProviderId,
                        Duration = sw.Elapsed
                    };

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(responseJson);
                var choice = doc.RootElement.GetProperty("choices")[0];
                var msg = choice.GetProperty("message");
                var responseContent = msg.GetProperty("content").GetString() ?? "";

                int promptTokens = 0, completionTokens = 0;
                if (doc.RootElement.TryGetProperty("usage", out var usage))
                {
                    promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0;
                    completionTokens = usage.TryGetProperty("completion_tokens", out var ctok) ? ctok.GetInt32() : 0;
                }

                return new AIResponse
                {
                    Content = responseContent,
                    Usage = new TokenUsage { PromptTokens = promptTokens, CompletionTokens = completionTokens },
                    ProviderId = ProviderId,
                    Duration = sw.Elapsed,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new AIResponse { IsSuccess = false, ErrorMessage = ex.Message, ProviderId = ProviderId, Duration = sw.Elapsed };
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
