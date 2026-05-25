namespace Som3a.AI.Providers
{
    public class CodexProvider : OpenAIProvider
    {
        public override string ProviderId => "codex";
        public override string ProviderName => "Codex";
        public override bool IsAvailable => !string.IsNullOrEmpty(ApiKey);

        private string ApiKey { get; }

        public CodexProvider(string apiKey, string model = "gpt-4")
            : base(apiKey, model)
        {
            ApiKey = apiKey;
        }
    }
}
