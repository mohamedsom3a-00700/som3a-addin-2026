using Som3a_WPF_UI.Models;
using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public enum AIProviderType
    {
        Cloud,
        Ollama
    }

    public static class AISettings
    {
        private static bool _isAIEnabled;
        private static AIProviderType _providerType = AIProviderType.Cloud;
        private static string _cloudApiKey = string.Empty;
        private static string _cloudMainModel = "gpt-4o-mini";
        private static string _cloudSubModel = "gpt-4o-mini";
        private static string _ollamaEndpoint = "http://localhost:11434";
        private static string _ollamaModel = "deepseek-coder";
        private static string _ollamaSubModel = "llama3.2";
        private static List<LocalProviderInfo> _detectedLocalProviders = new();
        private static string _selectedLocalProviderId = "ollama";
        private static readonly object _lock = new();

        static AISettings()
        {
            // Run detection immediately when the class is first loaded
            // This ensures GetEffectiveProvider() has provider info ready
            try
            {
                var detected = LocalProviderDetector.Detect();
                if (detected.Count > 0)
                {
                    _detectedLocalProviders = detected;
                    var preferred = detected.FirstOrDefault(p => p.Id == _selectedLocalProviderId) ?? detected[0];
                    _selectedLocalProviderId = preferred.Id;
                    _ollamaEndpoint = preferred.Endpoint;
                    _ollamaModel = preferred.DefaultModel;
                    _ollamaSubModel = preferred.FallbackModel;
                }
            }
            catch
            {
                // Detection failed — no local providers available
            }
        }

        public static bool IsAIEnabled
        {
            get { lock (_lock) { return _isAIEnabled; } }
            set { lock (_lock) { _isAIEnabled = value; } }
        }

        public static AIProviderType ProviderType
        {
            get { lock (_lock) { return _providerType; } }
            set { lock (_lock) { _providerType = value; } }
        }

        public static string CloudApiKey
        {
            get { lock (_lock) { return _cloudApiKey; } }
            set { lock (_lock) { _cloudApiKey = value ?? string.Empty; } }
        }

        public static string CloudMainModel
        {
            get { lock (_lock) { return _cloudMainModel; } }
            set { lock (_lock) { _cloudMainModel = value ?? "gpt-4o-mini"; } }
        }

        public static string CloudSubModel
        {
            get { lock (_lock) { return _cloudSubModel; } }
            set { lock (_lock) { _cloudSubModel = value ?? "gpt-4o-mini"; } }
        }

        public static string OllamaEndpoint
        {
            get { lock (_lock) { return _ollamaEndpoint; } }
            set { lock (_lock) { _ollamaEndpoint = value ?? "http://localhost:11434"; } }
        }

        public static string OllamaModel
        {
            get { lock (_lock) { return _ollamaModel; } }
            set { lock (_lock) { _ollamaModel = value ?? "deepseek-coder"; } }
        }

        public static string OllamaSubModel
        {
            get { lock (_lock) { return _ollamaSubModel; } }
            set { lock (_lock) { _ollamaSubModel = value ?? "llama3.2"; } }
        }

public static List<LocalProviderInfo> DetectedLocalProviders
        {
            get { lock (_lock) { return new List<LocalProviderInfo>(_detectedLocalProviders); } }
            set { lock (_lock) { _detectedLocalProviders = value != null ? new List<LocalProviderInfo>(value) : new List<LocalProviderInfo>(); } }
        }

        public static string SelectedLocalProviderId
        {
            get { lock (_lock) { return _selectedLocalProviderId; } }
            set { lock (_lock) { _selectedLocalProviderId = value ?? "ollama"; } }
        }

        public static bool IsLocalProviderDetected
        {
            get { lock (_lock) { return _detectedLocalProviders.Count > 0; } }
        }

        public static bool HasApiKey => !string.IsNullOrEmpty(CloudApiKey);

        public static (string providerType, string? apiKey, string model, string? endpoint) GetEffectiveProvider()
        {
            lock (_lock)
            {
                if (!_isAIEnabled)
                    return ("none", null, string.Empty, null);

                EnsureLocalProvidersDetected();

                if (!string.IsNullOrEmpty(_cloudApiKey))
                    return ("cloud", _cloudApiKey, _cloudMainModel, null);

                if (_detectedLocalProviders.Count > 0)
                    return ("ollama", null, _detectedLocalProviders[0].DefaultModel,
                        LocalProviderDetector.GetApiEndpoint(_detectedLocalProviders[0].Endpoint));

                return ("none", null, string.Empty, null);
            }
        }

        private static void EnsureLocalProvidersDetected()
        {
        }

        public static void ApplyLocalProviderSelection(LocalProviderInfo provider)
        {
            if (provider == null) return;
            lock (_lock)
            {
                _selectedLocalProviderId = provider.Id;
                _ollamaEndpoint = provider.Endpoint;
                _ollamaModel = provider.DefaultModel;
                _ollamaSubModel = provider.FallbackModel;
            }
        }
    }
}
