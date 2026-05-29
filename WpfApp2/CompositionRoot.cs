using Som3a.Bridge;
using Som3a.Shared.Core;
using Som3a.Shared.Core.Primavera;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Modules;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;
using Som3a_WPF_UI.ViewModels.Primavera;
using System.Linq;
using System.Threading.Tasks;

namespace Som3a_WPF_UI
{
    public static class CompositionRoot
    {
        public static void RegisterServices(IServiceContainer container)
        {
            container.RegisterSingleton<IServiceContainer>(container);

            var eventBus = new EventBus();
            container.RegisterSingleton<IEventBus>(eventBus);

            var moduleRegistry = new ModuleRegistry(container, eventBus);
            moduleRegistry.RegisterModule(new Modules.BOQActivityGeneratorModule());
            moduleRegistry.RegisterModule(new Modules.RelationshipGeneratorModule());
            container.RegisterSingleton<Services.IModuleRegistry>(moduleRegistry);

            container.RegisterSingleton<ThemeManager>(ThemeManager.Instance);
            container.RegisterSingleton<INavigationService>(NavigationService.Instance);

            container.RegisterTransient<FixPieColorsService, FixPieColorsService>();
            container.RegisterTransient<SubDlyReportService, SubDlyReportService>();
            container.RegisterTransient<UnmergeFillDownService, UnmergeFillDownService>();
            container.RegisterTransient<XerParser, XerParser>();

            container.RegisterTransient<FixPieColorsViewModel, FixPieColorsViewModel>();
            container.RegisterTransient<ProjectAnalysisViewModel, ProjectAnalysisViewModel>();
            container.RegisterTransient<FloatPathViewModel, FloatPathViewModel>();

            container.RegisterTransient<PrimaveraCompareViewModel, PrimaveraCompareViewModel>();
            container.RegisterTransient<PrimaveraResultsViewModel, PrimaveraResultsViewModel>();
            container.RegisterTransient<AssignTradeCodesViewModel, AssignTradeCodesViewModel>();

            container.RegisterTransient<MainViewModel, MainViewModel>();
            container.RegisterTransient<ShellViewModel, ShellViewModel>();

            container.RegisterSingleton<SettingsRegistry>(SettingsRegistry.Instance);
            container.RegisterSingleton<SettingsValidator, SettingsValidator>();
            container.RegisterSingleton<SettingsMigrationService, SettingsMigrationService>();
            container.RegisterSingleton<SettingsModuleIntegration, SettingsModuleIntegration>();
            container.RegisterTransient<SettingsPersistenceService, SettingsPersistenceService>();
            container.RegisterTransient<SettingsViewModel, SettingsViewModel>();
            container.RegisterTransient<WbsStyleSelectorViewModel, WbsStyleSelectorViewModel>();

            container.RegisterTransient<ExcelProjectAnalysisService, ExcelProjectAnalysisService>();
            container.RegisterTransient<XerExportService, XerExportService>();
            container.RegisterTransient<FloatPathService, FloatPathService>();
            container.RegisterTransient<GraphService, GraphService>();
            container.RegisterTransient<WbsBuilder, WbsBuilder>();

            container.RegisterTransient<IPrimaveraDbService, PrimaveraDbService>();
            container.RegisterTransient<IPrimaveraDataLoaderService, PrimaveraDataLoaderService>();
            container.RegisterTransient<IPrimaveraComparisonService, PrimaveraComparisonService>();

            container.RegisterTransient<BOQActivityGeneratorViewModel, BOQActivityGeneratorViewModel>();

            container.RegisterTransient<IBOQContextBuilder, Services.BOQContextBuilder>();
            container.RegisterTransient<IActivityGenerationService, Services.ActivityGenerationService>();
            container.RegisterTransient<IActivityValidationService, Services.ActivityValidationService>();
            container.RegisterTransient<IActivitySequencingService, Services.ActivitySequencingService>();
            container.RegisterTransient<IActivityExportService, Services.ActivityExportService>();

            container.RegisterTransient<IRelationshipGenerationService, Services.RelationshipGenerationService>();
            container.RegisterTransient<IRelationshipValidationService, Services.RelationshipValidationService>();
            container.RegisterTransient<IRelationshipAnalysisService, Services.RelationshipAnalysisService>();
            container.RegisterTransient<IRelationshipExportService, Services.RelationshipExportService>();
            container.RegisterTransient<RelationshipGeneratorViewModel, RelationshipGeneratorViewModel>();
            container.RegisterTransient<DurationEstimatorPageViewModel, DurationEstimatorPageViewModel>();
            container.RegisterSingleton<IDurationEstimatorBridge, DurationCalculationService>();

            container.RegisterTransient<ToastViewModel, ToastViewModel>();
            container.RegisterTransient<CommandPaletteViewModel, CommandPaletteViewModel>();

            container.RegisterSingleton<IRecentItemsService, RecentItemsService>();
            container.RegisterSingleton<IPerformanceMonitor, PerformanceMonitor>();
            container.RegisterSingleton<IChangelogService, ChangelogService>();

            container.RegisterSingleton<ILoggingService, LoggingService>();
            container.RegisterSingleton<IDiagnosticsService, DiagnosticsService>();
            container.RegisterSingleton<IValidationEngine, ValidationEngine>();
            container.RegisterSingleton<DiagnosticsViewModel, DiagnosticsViewModel>();

            container.RegisterTransient<ViewModels.Settings.LanguagePageViewModel, ViewModels.Settings.LanguagePageViewModel>();

            container.RegisterSingleton<ISidebarRegistrationProvider, SidebarRegistrationService>();
            container.RegisterSingleton<Som3a_WPF_UI.Contracts.IModuleRegistry, PluginRegistry>();
            container.RegisterSingleton<PluginLoader, PluginLoader>();
            container.RegisterSingleton<IModuleDiagnosticsService, ModuleDiagnosticsService>();
            container.RegisterSingleton<ModuleLoadOrchestrator, ModuleLoadOrchestrator>();

            // AI Bridge: registers singleton that manages host process lifecycle
            var aiBridge = new AIBridgeClient();
            container.RegisterSingleton<IAIBridge>(aiBridge);
        }

        public static void InitializeModules(Services.IModuleRegistry registry)
        {
            registry.InitializeAll();
        }

        public static async Task RunStartupTasksAsync(IServiceContainer container)
        {
            var migrationService = container.Resolve<SettingsMigrationService>();
            await migrationService.MigrateAllUnmigratedAsync();

            // Start AI host process in background (don't block startup)
            var aiBridge = container.Resolve<IAIBridge>();
            _ = Task.Run(async () =>
            {
                try { await aiBridge.StartHostAsync(); }
                catch { /* AI host unavailable - services will report offline */ }
            });

            // Auto-detect running local AI providers (Ollama, LM Studio, etc.)
            // This runs in background and populates AISettings.DetectedLocalProviders
            // so GetEffectiveProvider() can auto-fallback when no API key is configured.
            _ = Task.Run(() =>
            {
                try
                {
                    var detected = LocalProviderDetector.Detect();
                    if (detected.Count > 0)
                    {
                        AISettings.DetectedLocalProviders = detected;

                        // If user has a saved local provider preference, select it
                        var savedId = AISettings.SelectedLocalProviderId;
                        var preferred = detected.FirstOrDefault(p => p.Id == savedId) ?? detected[0];
                        AISettings.SelectedLocalProviderId = preferred.Id;

                        // Auto-fallback: if Cloud selected but no API key, switch to local
                        if (AISettings.ProviderType == AIProviderType.Cloud && string.IsNullOrEmpty(AISettings.CloudApiKey))
                        {
                            AISettings.ProviderType = AIProviderType.Ollama;
                            AISettings.OllamaEndpoint = preferred.Endpoint;
                            AISettings.OllamaModel = preferred.DefaultModel;
                        }
                    }
                }
                catch
                {
                    // Detection failed - services will report no local providers available
                }
            });
        }
    }
}
