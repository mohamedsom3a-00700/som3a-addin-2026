using Som3a.Shared.Core;
using Som3a.Shared.Core.Primavera;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;
using Som3a_WPF_UI.ViewModels.Primavera;

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
            container.RegisterSingleton<IModuleRegistry>(moduleRegistry);

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

            container.RegisterTransient<ToastViewModel, ToastViewModel>();
            container.RegisterTransient<CommandPaletteViewModel, CommandPaletteViewModel>();

            container.RegisterSingleton<ILoggingService, LoggingService>();
            container.RegisterSingleton<IDiagnosticsService, DiagnosticsService>();
            container.RegisterSingleton<IValidationEngine, ValidationEngine>();
            container.RegisterSingleton<DiagnosticsViewModel, DiagnosticsViewModel>();

            container.RegisterSingleton<Som3a_WPF_UI.Contracts.IModuleRegistry, PluginRegistry>();
            container.RegisterSingleton<PluginLoader, PluginLoader>();
            container.RegisterSingleton<IModuleDiagnosticsService, ModuleDiagnosticsService>();
            container.RegisterSingleton<ModuleLoadOrchestrator, ModuleLoadOrchestrator>();
        }

        public static void InitializeModules(IModuleRegistry registry)
        {
            registry.InitializeAll();
        }
    }
}
