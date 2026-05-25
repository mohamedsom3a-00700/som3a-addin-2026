namespace Som3a.Contracts
{
    public interface ICommand
    {
        event EventHandler? CanExecuteChanged;
        bool CanExecute(object? parameter);
        void Execute(object? parameter);
    }

    public interface IPlugin
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        int Priority { get; }
        string[] Dependencies { get; }

        void Initialize(IPluginContext context);
        void RegisterSettings(ISettingsRegistry registry);
        void LoadUI(IPageHost pageHost);
        void RegisterCommands(ICommandRegistry registry);
        void Shutdown();
    }

    public interface IPluginContext
    {
        IServiceContainer ServiceContainer { get; }
        IEventBus EventBus { get; }
        IDiagnosticsProvider Diagnostics { get; }
        AppConfiguration Config { get; }
    }

    public interface IPageHost
    {
        void RegisterPage(string id, string title, Type pageType, string? category = null);
        void RegisterNavigationItem(string id, string title, string? category, int order);
    }

    public interface ICommandRegistry
    {
        void RegisterCommand(string id, string name, ICommand command, string? keyGesture = null);
        void RegisterMenuCommand(string id, string name, ICommand command, string parentMenu);
    }

    public interface ISettingsRegistry
    {
        void RegisterSection(SettingsSection section);
        void RegisterSetting(string sectionId, SettingDefinition setting);
    }

    public class SettingsSection
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? IconKey { get; set; }
    }

    public class SettingDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SettingValueType ValueType { get; set; }
        public object? DefaultValue { get; set; }
        public object? CurrentValue { get; set; }
        public List<ValidationRule>? ValidationRules { get; set; }
        public bool IsEncrypted { get; set; }
    }

    public enum SettingValueType
    {
        String,
        Integer,
        Decimal,
        Boolean,
        Enum,
        Color,
        FilePath,
        Secret
    }

    public class ValidationRule
    {
        public string RuleId { get; set; } = string.Empty;
        public string? Parameters { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt);
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler);
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter);
        void Unsubscribe(SubscriptionToken token);
    }

    public class SubscriptionToken
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
    }

    public interface IServiceContainer
    {
        T Resolve<T>() where T : class;
        object Resolve(Type type);
        void RegisterSingleton<T>(T instance) where T : class;
        void RegisterTransient<T>(Func<T> factory) where T : class;
    }

    public class AppConfiguration
    {
        public string GetValue(string key, string defaultValue = "") => defaultValue;
        public int GetIntValue(string key, int defaultValue = 0) => defaultValue;
        public bool GetBoolValue(string key, bool defaultValue = false) => defaultValue;
    }
}
