using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Som3a.AI.Configuration;
using Som3a.AI.Orchestration;
using Som3a.AI.Providers;
using Som3a.Bridge;
using Som3a.Contracts;

var pipeName = args.Length > 0 ? args[0] : "Som3a.AI.Bridge";

Console.Error.WriteLine($"[AI.Host] Starting on pipe: {pipeName}");

while (true)
{
    try
    {
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        Console.Error.WriteLine("[AI.Host] Waiting for connection...");
        await server.WaitForConnectionAsync();
        Console.Error.WriteLine("[AI.Host] Client connected.");

        await HandleClientAsync(server);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[AI.Host] Error: {ex.Message}");
    }
}

static async Task HandleClientAsync(NamedPipeServerStream pipe)
{
    var lenBuf = new byte[4];
    await ReadExactAsync(pipe, lenBuf, 0, 4);
    var reqLen = BitConverter.ToInt32(lenBuf, 0);
    if (reqLen <= 0 || reqLen > 10 * 1024 * 1024)
    {
        await SendResponseAsync(pipe, new AIBridgeResponse
        {
            IsSuccess = false,
            ErrorMessage = $"Invalid request length: {reqLen}"
        });
        return;
    }

    var reqBuf = new byte[reqLen];
    await ReadExactAsync(pipe, reqBuf, 0, reqLen);
    var reqJson = Encoding.UTF8.GetString(reqBuf);

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    var bridgeRequest = JsonSerializer.Deserialize<AIBridgeRequest>(reqJson, options);
    if (bridgeRequest == null)
    {
        await SendResponseAsync(pipe, new AIBridgeResponse
        {
            IsSuccess = false,
            ErrorMessage = "Failed to deserialize request"
        });
        return;
    }

    var startTime = DateTime.UtcNow;

    try
    {
        var aiRequest = new AIRequest
        {
            SystemPrompt = bridgeRequest.SystemPrompt,
            UserPrompt = bridgeRequest.UserPrompt,
            Temperature = bridgeRequest.Temperature,
            MaxTokens = bridgeRequest.MaxTokens
        };

        IAIProvider provider;

        if (string.Equals(bridgeRequest.ProviderType, "ollama", StringComparison.OrdinalIgnoreCase))
        {
            var endpoint = !string.IsNullOrEmpty(bridgeRequest.Endpoint)
                ? bridgeRequest.Endpoint.TrimEnd('/') + "/v1/"
                : "http://localhost:11434/v1/";
            var model = !string.IsNullOrEmpty(bridgeRequest.Model) ? bridgeRequest.Model : "llama3";
            provider = new OpenAIProvider("ollama", model, endpoint);
        }
        else if (!string.IsNullOrEmpty(bridgeRequest.ApiKey))
        {
            var model = !string.IsNullOrEmpty(bridgeRequest.Model) ? bridgeRequest.Model : "gpt-4o-mini";
            provider = new OpenAIProvider(bridgeRequest.ApiKey, model);
        }
        else
        {
            // Fall back to OpenRouter if no API key provided (free models)
            var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (!string.IsNullOrEmpty(key))
            {
                provider = new OpenAIProvider(key, "meta-llama/llama-3.2-3b-instruct",
                    "https://openrouter.ai/api/v1/",
                    new Dictionary<string, string> { { "X-Title", "Som3a Addin 2026" } });
            }
            else
            {
                var elapsed = DateTime.UtcNow - startTime;
                await SendResponseAsync(pipe, new AIBridgeResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No API key configured. Set your API key in Settings > AI or set the OPENROUTER_API_KEY environment variable.",
                    DurationMs = elapsed.TotalMilliseconds
                });
                return;
            }
        }

        var response = await provider.ExecutePromptAsync(aiRequest);
        var totalMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        await SendResponseAsync(pipe, new AIBridgeResponse
        {
            Content = response.Content,
            ParsedJson = response.ParsedJson,
            IsSuccess = response.IsSuccess,
            ErrorMessage = response.ErrorMessage,
            ProviderId = response.ProviderId ?? "dynamic",
            DurationMs = totalMs,
            PromptTokens = response.Usage?.PromptTokens ?? 0,
            CompletionTokens = response.Usage?.CompletionTokens ?? 0
        });
    }
    catch (Exception ex)
    {
        var elapsed = DateTime.UtcNow - startTime;
        await SendResponseAsync(pipe, new AIBridgeResponse
        {
            IsSuccess = false,
            ErrorMessage = $"AI execution error: {ex.Message}",
            DurationMs = elapsed.TotalMilliseconds
        });
    }
}

static async Task ReadExactAsync(NamedPipeServerStream pipe, byte[] buffer, int offset, int count)
{
    while (count > 0)
    {
        var n = await pipe.ReadAsync(buffer, offset, count);
        if (n == 0) throw new EndOfStreamException("Pipe closed by client.");
        offset += n;
        count -= n;
    }
}

static async Task SendResponseAsync(NamedPipeServerStream pipe, AIBridgeResponse response)
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    var json = JsonSerializer.Serialize(response, options);
    var bytes = Encoding.UTF8.GetBytes(json);
    var lenBytes = BitConverter.GetBytes(bytes.Length);

    await pipe.WriteAsync(lenBytes, 0, 4);
    await pipe.WriteAsync(bytes, 0, bytes.Length);
    await pipe.FlushAsync();
}
