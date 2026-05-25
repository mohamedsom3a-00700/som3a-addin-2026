namespace Som3a.Infrastructure.Configuration
{
    public class FeatureFlags
    {
        private readonly AppConfiguration _config;

        public FeatureFlags(AppConfiguration config)
        {
            _config = config;
        }

        public bool IsAIEnabled
        {
            get => _config.GetBoolValue("features.ai.enabled", true);
            set => _config.SetValue("features.ai.enabled", value);
        }

        public bool IsExportEnabled
        {
            get => _config.GetBoolValue("features.export.enabled", true);
            set => _config.SetValue("features.export.enabled", value);
        }

        public bool IsPluginSystemEnabled
        {
            get => _config.GetBoolValue("features.plugins.enabled", true);
            set => _config.SetValue("features.plugins.enabled", value);
        }

        public bool IsDiagnosticsEnabled
        {
            get => _config.GetBoolValue("features.diagnostics.enabled", true);
            set => _config.SetValue("features.diagnostics.enabled", value);
        }

        public bool IsEnabled(string featureKey, bool defaultValue = true)
        {
            return _config.GetBoolValue($"features.{featureKey}.enabled", defaultValue);
        }

        public void SetEnabled(string featureKey, bool value)
        {
            _config.SetValue($"features.{featureKey}.enabled", value);
        }

        public void Save()
        {
            _config.Save();
        }
    }
}
