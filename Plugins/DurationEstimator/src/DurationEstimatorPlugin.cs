using Som3a.Contracts;
using Som3a.DurationEstimator.Benchmarks;
using Som3a.DurationEstimator.Calendar;
using Som3a.DurationEstimator.Engine;

namespace Som3a.DurationEstimator;

[Som3a.Plugin.SDK.Attributes.Plugin(
    Id = "duration-estimator",
    Name = "Duration Estimator",
    Version = "1.0.0",
    Priority = 50,
    Dependencies = new[] { "som3a.domain", "som3a.ai" }
)]
[Som3a.Plugin.SDK.Attributes.NavigationItem(
    Category = "Planning",
    Order = 4
)]
[Som3a.Plugin.SDK.Attributes.SettingsSection(
    Category = "Plugins",
    Order = 3
)]
public class DurationEstimatorPlugin : IPlugin
{
    public string Id => "duration-estimator";
    public string Name => "Duration Estimator";
    public string Version => "1.0.0";
    public int Priority => 50;
    public string[] Dependencies => new[] { "som3a.domain", "som3a.ai" };

    private IProductivityEngine? _productivityEngine;
    private IBenchmarkLibrary? _benchmarkLibrary;
    private ICalendarEngine? _calendarEngine;
    private SubscriptionToken? _activitySubscription;
    private IEventBus? _eventBus;

    public void Initialize(IPluginContext context)
    {
        _productivityEngine = context.ServiceContainer.Resolve<IProductivityEngine>();
        _benchmarkLibrary = context.ServiceContainer.Resolve<IBenchmarkLibrary>();
        _calendarEngine = context.ServiceContainer.Resolve<ICalendarEngine>();
        _eventBus = context.EventBus;

        _activitySubscription = context.EventBus.Subscribe<ActivityGeneratedEvent>(OnActivityGenerated);
    }

    public void RegisterSettings(ISettingsRegistry registry)
    {
        registry.RegisterSection(new SettingsSection
        {
            Id = "duration-estimator",
            Category = "Plugins",
            DisplayName = "Duration Estimator",
            Order = 3
        });

        registry.RegisterSetting("duration-estimator", new SettingDefinition
        {
            Key = "default-productivity-rate",
            DisplayName = "Default Productivity Rate",
            Description = "Default productivity rate for new activities",
            ValueType = SettingValueType.Decimal,
            DefaultValue = 10m
        });

        registry.RegisterSetting("duration-estimator", new SettingDefinition
        {
            Key = "default-crew-size",
            DisplayName = "Default Crew Size",
            Description = "Default crew size for new activities",
            ValueType = SettingValueType.Integer,
            DefaultValue = 2
        });

        registry.RegisterSetting("duration-estimator", new SettingDefinition
        {
            Key = "default-hours-per-day",
            DisplayName = "Hours Per Day",
            Description = "Default working hours per day",
            ValueType = SettingValueType.Decimal,
            DefaultValue = 8m
        });

        registry.RegisterSetting("duration-estimator", new SettingDefinition
        {
            Key = "ai-suggestions-enabled",
            DisplayName = "AI Suggestions",
            Description = "Enable AI-powered productivity suggestions and anomaly detection",
            ValueType = SettingValueType.Boolean,
            DefaultValue = true
        });
    }

    public void LoadUI(IPageHost pageHost)
    {
    }

    public void RegisterCommands(ICommandRegistry registry)
    {
    }

    public void Shutdown()
    {
        if (_activitySubscription != null)
        {
            _eventBus?.Unsubscribe(_activitySubscription);
            _activitySubscription = null;
        }
    }

    private void OnActivityGenerated(ActivityGeneratedEvent evt)
    {
    }
}

public class ActivityGeneratedEvent
{
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string TradeCategory { get; set; } = string.Empty;
}


