using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI
{
    public static class CompositionRoot
    {
        public static void RegisterServices(IServiceContainer container)
        {
            var eventBus = new EventBus();
            container.RegisterSingleton<IEventBus>(eventBus);

            var moduleRegistry = new ModuleRegistry(container, eventBus);
            container.RegisterSingleton<IModuleRegistry>(moduleRegistry);

            container.RegisterSingleton<ThemeManager>(ThemeManager.Instance);
            container.RegisterSingleton<INavigationService>(NavigationService.Instance);

            container.RegisterTransient<FixPieColorsService, FixPieColorsService>();
        }

        public static void InitializeModules(IModuleRegistry registry)
        {
            registry.InitializeAll();
        }
    }
}
