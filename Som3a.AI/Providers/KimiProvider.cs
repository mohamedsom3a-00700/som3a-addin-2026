namespace Som3a.AI.Providers
{
    public class KimiProvider : OpenAIProvider
    {
        public override string ProviderId => "kimi";
        public override string ProviderName => "Kimi (Moonshot)";
        public override bool IsAvailable => !string.IsNullOrEmpty(ApiKey);

        private string ApiKey { get; }

        public KimiProvider(string apiKey, string model = "moonshot-v1-8k")
            : base(apiKey, model, "https://api.moonshot.cn/v1/")
        {
            ApiKey = apiKey;
        }
    }
}
