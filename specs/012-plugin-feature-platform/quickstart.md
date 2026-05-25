# Quickstart: Creating a New Module

This guide walks through creating a new module for the Plugin & Feature Platform.

## Step 1: Create Project Structure

Create a new Class Library project in the solution:

```
WpfApp2.Modules.YourFeature/
├── YourFeatureModule.cs        # IModule implementation
├── YourFeaturePage.xaml         # Page(s) for navigation
├── YourFeaturePage.xaml.cs
├── module.json                  # Module manifest
└── YourFeature.csproj
```

## Step 2: Implement IModule

```csharp
using Som3a_WPF_UI.Contracts;

public class YourFeatureModule : IModule
{
    public string Id => "som3a.your-feature";
    public string Version => "1.0.0";
    public string DisplayName => "Your Feature";
    public string Description => "Does something useful.";

    public void Initialize(IModuleInitializationContext context)
    {
        // Register navigation page
        context.Navigation.RegisterPage(
            "your-feature-main",
            "Your Feature",
            typeof(YourFeaturePage));

        // Register ribbon button
        context.Ribbon.AddButton(
            "yourFeatureButton",
            "Your Feature",
            "Opens Your Feature",
            () => context.Navigation.NavigateTo("your-feature-main"));

        // Register command (optional)
        context.Commands.RegisterCommand("YourFeature.Run", new RelayCommand(ExecuteRun));
    }

    private void ExecuteRun() { /* ... */ }
}
```

## Step 3: Create module.json

```json
{
  "id": "som3a.your-feature",
  "version": "1.0.0",
  "displayName": "Your Feature",
  "description": "Does something useful.",
  "hash": "<SHA256 of assembly>",
  "hashAlgorithm": "SHA256",
  "capabilities": ["pages", "ribbonActions", "commands"],
  "dependencies": []
}
```

Generate the hash:
```powershell
$bytes = [System.IO.File]::ReadAllBytes("bin\Debug\WpfApp2.Modules.YourFeature.dll")
$hash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)) -replace '-', ''
$hash.ToUpper()
```

## Step 4: Build & Deploy

1. Build your module project
2. Copy the output DLL + `module.json` to `WpfApp2/Modules/YourFeature/`
3. The plugin loader discovers it on next app start

## Step 5: Verify

1. Start the application
2. Navigate to "Your Feature" in the navigation shell
3. Verify the page renders
4. Check the ribbon for the registered button
5. Open Plugin Diagnostics to see module status

## Contract Reference

| Interface | Purpose |
|-----------|---------|
| `IModule` | Main module contract — every module implements this |
| `IModuleInitializationContext` | Provided during Initialize — access registrar services |
| `INavigationRegistrar` | Register navigation pages |
| `IRibbonRegistrar` | Register ribbon buttons/menus |
| `ICommandRegistrar` | Register commands |
| `IModuleRegistry` | Core registry (system-side) |
| `IPluginLoader` | Assembly loading (system-side) |
