using System.Text;
using System.Text.Json;
using Som3a.Contracts;

namespace Som3a.AI.Providers
{
    public class OpenAIProvider : AIProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public override string ProviderId => "openai";
        public override string ProviderName => "OpenAI";
        public override bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

        public OpenAIProvider(string apiKey, string model = "gpt-4", string? baseUrl = null, Dictionary<string, string>? additionalHeaders = null)
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            if (additionalHeaders != null)
            {
                foreach (var h in additionalHeaders)
                    _httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);
            }
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
                    temperature = request.Temperature,
                    response_format = request.JsonSchema != null
                        ? new { type = "json_object" } as object
                        : null
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("chat/completions", content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new AIResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"OpenAI API error: {response.StatusCode}",
                        ProviderId = ProviderId,
                        Duration = sw.Elapsed
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(responseJson);
                var choice = doc.RootElement.GetProperty("choices")[0];
                var msg = choice.GetProperty("message");
                var responseContent = msg.GetProperty("content").GetString() ?? string.Empty;
                var usage = doc.RootElement.GetProperty("usage");

                return new AIResponse
                {
                    Content = responseContent,
                    ParsedJson = request.JsonSchema != null ? responseContent : null,
                    Usage = new TokenUsage
                    {
                        PromptTokens = usage.GetProperty("prompt_tokens").GetInt32(),
                        CompletionTokens = usage.GetProperty("completion_tokens").GetInt32()
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
            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = request.UserPrompt }
                },
                max_tokens = request.MaxTokens,
                temperature = request.Temperature,
                stream = true
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", content, ct);
            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            int index = 0;
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line == null || !line.StartsWith("data: ")) continue;
                var data = line.Substring(6);
                if (data == "[DONE]") break;

                string text = "";
                if (TryParseDelta(data, out text))
                {
                    yield return new AIStreamChunk { Delta = text, Index = index++, IsFinal = false };
                }
            }

            yield return new AIStreamChunk { Delta = "", Index = index, IsFinal = true };
        }

        private static bool TryParseDelta(string data, out string text)
        {
            text = "";
            try
            {
                using var doc = JsonDocument.Parse(data);
                var choices = doc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() > 0)
                {
                    var delta = choices[0].GetProperty("delta");
                    text = delta.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
