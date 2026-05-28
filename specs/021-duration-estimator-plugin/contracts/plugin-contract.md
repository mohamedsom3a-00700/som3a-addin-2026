# Plugin Contract: Duration Estimator

## Identity

| Attribute | Value |
|-----------|-------|
| PluginId | `duration-estimator` |
| Name | Duration Estimator |
| Version | 1.0.0 |
| Priority | 50 (default) |
| Dependencies | Som3a.Domain (≥1.0.0), Som3a.AI (≥1.0.0) |

## Registration

The plugin registers via Som3a.Plugin.SDK attributes:

### Plugin Attribute
```csharp
[Plugin(
    Id = "duration-estimator",
    Name = "Duration Estimator",
    Version = "1.0.0",
    Priority = 50,
    Dependencies = new[] { "som3a.domain", "som3a.ai" }
)]
```

### Navigation Registration
```csharp
[NavigationItem(
    Category = "Planning",
    Order = 4,
    Icon = MaterialIcon.Timer,
    PageType = typeof(DurationEstimatorPage)
)]
```

### Settings Registration
```csharp
[SettingsSection(
    Category = "Plugins",
    Order = 3,
    DisplayName = "Duration Estimator"
)]
```

## IPlugin Lifecycle

| Phase | Method | Action |
|-------|--------|--------|
| 1. Initialize | `Initialize(IServiceContainer, IEventBus)` | Register internal services; load benchmark library; subscribe to ActivityGenerated events from Phase 20 |
| 2. Register | `Register(IModuleRegistry)` | Register settings sections; register navigation items |
| 3. LoadUI | `LoadUI(INavigationService)` | Register Shell pages for duration estimation workspace |
| 4. RegisterSettings | `RegisterSettings(ISettingsModule)` | Register productivity defaults, calendar presets, AI toggle |

## Published Events

| Event | Payload | Trigger |
|-------|---------|---------|
| DurationCalculated | ActivityId, DurationWorkingDays, CalendarDurationDays | After successful calculation |
| BenchmarkModified | TradeCategoryId, ProductivityRateId | After add/edit/delete of benchmark |
| DurationExported | ExportTarget (Excel/SchedulingPipeline), ActivityCount | After export completes |
| AnomalyDetected | ActivityId, AnomalyType, Explanation | When anomaly detection flags activity |

## Consumed Events

| Event | Source | Action |
|-------|--------|--------|
| ActivityGenerated (Phase 20) | BOQActivityGenerator | Import new activities for duration estimation |
| ActivityModified | Any source | Recalculate duration for modified activity |
| CalendarChanged | Any source | Recalculate all durations with new calendar |
