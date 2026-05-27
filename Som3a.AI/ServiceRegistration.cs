using Som3a.AI.Configuration;
using Som3a.AI.Orchestration;
using Som3a.AI.Parsers;
using Som3a.AI.Parsing;
using Som3a.AI.Prompts;
using Som3a.AI.Providers;
using Som3a.AI.Tracking;
using Som3a.Contracts;

namespace Som3a.AI;

public static class ServiceRegistration
{
    public static void RegisterAIServices(Action<Type, Type, ServiceLifetime> register)
    {
        register(typeof(EncryptionService), typeof(EncryptionService), ServiceLifetime.Singleton);
        register(typeof(PromptTemplateRegistry), typeof(PromptTemplateRegistry), ServiceLifetime.Singleton);
        register(typeof(TemplateValidator), typeof(TemplateValidator), ServiceLifetime.Singleton);
        register(typeof(ContextBudgetEstimator), typeof(ContextBudgetEstimator), ServiceLifetime.Singleton);
        register(typeof(AuditTrail), typeof(AuditTrail), ServiceLifetime.Singleton);
        register(typeof(RetryHandler), typeof(RetryHandler), ServiceLifetime.Singleton);
        register(typeof(RequestQueue), typeof(RequestQueue), ServiceLifetime.Singleton);
        register(typeof(StreamingHandler), typeof(StreamingHandler), ServiceLifetime.Singleton);
        register(typeof(TokenTracker), typeof(TokenTracker), ServiceLifetime.Singleton);
        register(typeof(UsageReporter), typeof(UsageReporter), ServiceLifetime.Singleton);
        register(typeof(ContextBuilder), typeof(ContextBuilder), ServiceLifetime.Singleton);
        register(typeof(StructuredOutputParser), typeof(StructuredOutputParser), ServiceLifetime.Singleton);
        register(typeof(JsonSchemaValidator), typeof(JsonSchemaValidator), ServiceLifetime.Singleton);
        register(typeof(ActivityParser), typeof(ActivityParser), ServiceLifetime.Transient);
        register(typeof(WBSParser), typeof(WBSParser), ServiceLifetime.Transient);
        register(typeof(RelationshipParser), typeof(RelationshipParser), ServiceLifetime.Transient);
        register(typeof(DurationParser), typeof(DurationParser), ServiceLifetime.Transient);
        register(typeof(ReviewParser), typeof(ReviewParser), ServiceLifetime.Transient);
    }

    public static void RegisterProviders(Action<Type, object> registerInstance, EncryptionService encryption)
    {
        registerInstance(typeof(IAIProvider), new OpenAIProvider(""));
        registerInstance(typeof(IAIProvider), new ClaudeProvider(""));
        registerInstance(typeof(IAIProvider), new DeepSeekProvider(""));
        registerInstance(typeof(IAIProvider), new GLMProvider(""));
        registerInstance(typeof(IAIProvider), new KimiProvider(""));
        registerInstance(typeof(IAIProvider), new CodexProvider(""));
        registerInstance(typeof(IAIProvider), new OllamaProvider());
    }
}

public enum ServiceLifetime { Singleton, Transient, Scoped }
