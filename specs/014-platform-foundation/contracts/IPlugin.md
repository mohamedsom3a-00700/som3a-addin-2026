# Contract: IPlugin

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the lifecycle contract that every plugin must implement to integrate with the Som3a platform. Plugins are discovered, validated, initialized, and managed through this interface.

## Interface

```csharp
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
```

## Lifecycle

| Phase | Method | Description |
|-------|--------|-------------|
| Discovery | (Attribute) | Plugin found by `[Plugin]` attribute on assembly |
| Validation | (PluginValidator) | Dependencies checked, contracts verified |
| Initialize | `Initialize(context)` | Receive platform services (DI, event bus, config) |
| Settings | `RegisterSettings(registry)` | Register settings sections and controls |
| UI | `LoadUI(pageHost)` | Register pages and navigation items in shell |
| Commands | `RegisterCommands(registry)` | Register ribbon/command palette commands |
| Shutdown | `Shutdown()` | Release resources, unsubscribe from events |

## Error Isolation

- If `Initialize()` throws → plugin marked `Error`, not registered.
- If `RegisterSettings()` throws → plugin registered but no settings shown.
- If `LoadUI()` throws → plugin registered but no UI pages available.
- If any method throws → other plugins unaffected (SC-007).

## Registration Pattern

Plugins use attributes for declarative registration:

```csharp
[Plugin(Id = "com.som3a.boq-generator", Name = "BOQ Activity Generator", Version = "1.0.0", Priority = 100)]
public class BOQGeneratorPlugin : IPlugin { ... }
```
