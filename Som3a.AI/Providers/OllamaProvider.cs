namespace Som3a.AI.Providers
{
    public class OllamaProvider : OpenAIProvider
    {
        public override string ProviderId => "ollama";
        public override string ProviderName => "Ollama (Local)";
        public override bool IsAvailable => true;

        public OllamaProvider(string model = "deepseek-coder", string baseUrl = "http://localhost:11434/v1/")
            : base("", model, baseUrl)
        {
        }
    }
}
