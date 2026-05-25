# Quickstart: Platform Foundation Developer Guide

**Feature**: Phase 14 — Platform Foundation
**Date**: 2026-05-25

## Prerequisites

- Windows 10/11 (x64)
- Visual Studio 2022 or newer (Community edition is fine)
- .NET 8.0 SDK
- .NET Framework 4.8 targeting pack
- Existing WpfApp2 solution open

## 1. Build the Solution

```powershell
# Build .NET 8.0 libraries
dotnet build Som3a.Domain/Som3a.Domain.csproj
dotnet build Som3a.Contracts/Som3a.Contracts.csproj
dotnet build Som3a.Bridge/Som3a.Bridge.csproj
dotnet build Som3a.Plugin.SDK/Som3a.Plugin.SDK.csproj
dotnet build Som3a.AI/Som3a.AI.csproj
dotnet build Som3a.Exporting/Som3a.Exporting.csproj
dotnet build Som3a.Localization/Som3a.Localization.csproj
dotnet build Som3a.Validation/Som3a.Validation.csproj
dotnet build Som3a.Diagnostics/Som3a.Diagnostics.csproj
dotnet build Som3a.Infrastructure/Som3a.Infrastructure.csproj

# Build full solution (including WPF host)
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## 2. Creating a Domain Entity

```csharp
using Som3a.Domain.BOQ;
using Som3a.Domain.Activities;

// Create a BOQ
var boq = new BOQDocument
{
    ProjectName = "Office Tower - Phase 1",
    CreatedDate = DateTimeOffset.Now
};

var section = new BOQSection
{
    SectionName = "Concrete Works",
    SectionCode = "CW-01",
    DisplayOrder = 1
};

section.Items.Add(new BOQItem
{
    ItemCode = "CW-01-001",
    Description = "Supply and install C40 concrete for raft foundation",
    Quantity = 1500m,
    Unit = "m3",
    Classification = "Structural"
});

boq.Sections.Add(section);

// Derive an Activity
var activity = new Activity
{
    ActivityId = "A001",
    Name = "Pour Raft Foundation Concrete",
    Description = "C40 concrete pour for raft foundation as per BOQ CW-01-001",
    Quantity = 1500m,
    Unit = "m3",
    BOQReferences = { "CW-01-001" }
};
```

## 3. Implementing a Minimal Plugin

```csharp
using Som3a.Contracts;
using Som3a.Plugin.SDK.Attributes;

[Plugin(Id = "com.example.hello", Name = "Hello Plugin", Version = "1.0.0", Priority = 100)]
public class HelloPlugin : IPlugin
{
    public string Id => "com.example.hello";
    public string Name => "Hello Plugin";
    public string Version => "1.0.0";
    public int Priority => 100;
    public string[] Dependencies => Array.Empty<string>();

    public void Initialize(IPluginContext context)
    {
        // Access platform services: context.ServiceContainer, context.EventBus
        context.Diagnostics.LogDiagnostic(new DiagnosticEvent
        {
            Level = DiagnosticLevel.Information,
            Source = "HelloPlugin",
            Category = "Plugin.Init",
            Message = "HelloPlugin initialized successfully"
        });
    }

    public void RegisterSettings(ISettingsRegistry registry) { }
    public void LoadUI(IPageHost pageHost) { }
    public void RegisterCommands(ICommandRegistry registry) { }
    public void Shutdown() { }
}
```

Build the plugin, place the assembly in `Plugins/`, and restart the platform. It will appear in the Plugin Status dashboard.

## 4. Making an AI Call

```csharp
using Som3a.Contracts;
using Som3a.AI.Orchestration;

// Orchestrator handles provider selection and failover
var orchestrator = serviceContainer.Resolve<IAIOrchestrator>();

var request = new AIRequest
{
    SystemPrompt = "You are a construction planning assistant.",
    UserPrompt = "Generate activities for a raft foundation pour.",
    JsonSchema = "{ \"type\": \"array\", \"items\": { \"$ref\": \"#/Activity\" } }",
    Temperature = 0.3f
};

var response = await orchestrator.ExecuteAsync(request);
if (response.IsSuccess)
{
    var activities = JsonSerializer.Deserialize<List<Activity>>(response.ParsedJson);
    Console.WriteLine($"Generated {activities.Count} activities");
    Console.WriteLine($"Token usage: {response.Usage.TotalTokens}");
}
else
{
    Console.WriteLine($"AI error: {response.ErrorMessage}");
}
```

## 5. Exporting Data to Excel

```csharp
using Som3a.Contracts;
using Som3a.Exporting.Pipeline;

var pipeline = serviceContainer.Resolve<IExportPipeline>();

var request = new ExportRequest
{
    Format = ExportFormat.Excel,
    TargetPath = @"C:\Exports\activities.xlsx",
    Data = activities,
    Options = new ExportOptions { SheetName = "Activities", ApplyStyling = true }
};

// Validate first
var validation = await pipeline.ValidateAsync(request);
if (!validation.IsSuccess)
{
    Console.WriteLine($"Validation failed: {validation.ErrorMessage}");
    return;
}

// Export
var result = await pipeline.ExportAsync(request);
Console.WriteLine($"Exported {result.RowCount} rows to {result.OutputPath}");
```

## 6. Storing API Keys Securely

```csharp
using Som3a.Infrastructure.Security;

var storage = new SecureStorage(); // Uses DPAPI internally

// Store
await storage.StoreSecretAsync("openai-api-key", "sk-abc123...");

// Retrieve
var key = await storage.GetSecretAsync("openai-api-key");
```

## 7. Running Tests

```powershell
# Run all .NET 8.0 unit tests
dotnet test Som3a.Domain.Tests/
dotnet test Som3a.Contracts.Tests/
dotnet test Som3a.AI.Tests/
dotnet test Som3a.Exporting.Tests/
```

## Project Dependency Graph

```text
Som3a.Contracts (no dependencies)
    ↑
Som3a.Domain → Som3a.Contracts
Som3a.Plugin.SDK → Som3a.Contracts
Som3a.AI → Som3a.Contracts, Som3a.Domain
Som3a.Exporting → Som3a.Contracts, Som3a.Domain
Som3a.Validation → Som3a.Contracts, Som3a.Domain
Som3a.Diagnostics → Som3a.Contracts
Som3a.Infrastructure → Som3a.Contracts
Som3a.Localization → Som3a.Contracts
Som3a.Bridge (.NET Standard 2.0) → Som3a.Contracts (subset)
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Build fails with "Type or namespace not found" | Ensure all project references are added; run `dotnet restore` first |
| Plugin not discovered | Verify assembly is in `Plugins/` directory and decorated with `[Plugin]` |
| AI provider fails | Check `IsAvailable` first; review diagnostics log for provider errors |
| DPAPI fails | Must run under the same Windows user account that stored the key |
| Interop bridge returns null | Check that both processes are running; verify named pipe name matches |
