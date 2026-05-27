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
        private static string _ollamaModel = "llama3";
        private static readonly object _lock = new();

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
            set { lock (_lock) { _ollamaModel = value ?? "llama3"; } }
        }
    }
}
