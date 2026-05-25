namespace Som3a.AI.Providers
{
    public class DeepSeekProvider : OpenAIProvider
    {
        public override string ProviderId => "deepseek";
        public override string ProviderName => "DeepSeek";
        public override bool IsAvailable => !string.IsNullOrEmpty(ApiKey);

        private string ApiKey { get; }

        public DeepSeekProvider(string apiKey, string model = "deepseek-chat")
            : base(apiKey, model, "https://api.deepseek.com/v1/")
        {
            ApiKey = apiKey;
        }
    }
}
