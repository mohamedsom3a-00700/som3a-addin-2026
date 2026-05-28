using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Som3a_WPF_UI.Services
{
    public static class LocalProviderDetector
    {
        private static readonly LocalProviderInfo[] KnownProviders =
        {
            new()
            {
                Id = "ollama",
                DisplayName = "Ollama (Local)",
                Endpoint = "http://localhost:11434",
                DefaultModel = "llama3.2",
                FallbackModel = "llama3.2"
            },
            new()
            {
                Id = "lmstudio",
                DisplayName = "LM Studio (Local)",
                Endpoint = "http://localhost:1234",
                DefaultModel = "local-model",
                FallbackModel = "local-model"
            },
            new()
            {
                Id = "localai",
                DisplayName = "LocalAI",
                Endpoint = "http://localhost:8080",
                DefaultModel = "gpt-4",
                FallbackModel = "gpt-4"
            }
        };

        public static List<LocalProviderInfo> Detect()
        {
            var detected = new List<LocalProviderInfo>();

            foreach (var provider in KnownProviders)
            {
                try
                {
                    var uri = new Uri(provider.Endpoint);
                    using var tcp = new TcpClient();
                    var ar = tcp.BeginConnect(uri.Host, uri.Port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(2000))
                    {
                        tcp.Close();
                        continue;
                    }
                    tcp.EndConnect(ar);

                    var info = new LocalProviderInfo
                    {
                        Id = provider.Id,
                        DisplayName = provider.DisplayName,
                        Endpoint = provider.Endpoint,
                        DefaultModel = provider.DefaultModel,
                        FallbackModel = provider.FallbackModel,
                        AvailableModels = new List<string> { provider.DefaultModel, provider.FallbackModel }
                    };
                    detected.Add(info);
                }
                catch
                {
                    // Provider not reachable
                }
            }

            return detected;
        }

        public static string GetApiEndpoint(string baseEndpoint)
        {
            return baseEndpoint.TrimEnd('/');
        }
    }
}
