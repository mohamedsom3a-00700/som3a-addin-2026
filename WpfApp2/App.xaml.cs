using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI
{
    public partial class App : Application
    {
        public static IServiceContainer Container { get; } = new ServiceContainer();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            base.OnStartup(e);

            CompositionRoot.RegisterServices(Container);

            ThemeManager.LoadSettings();

            CompositionRoot.InitializeModules(Container.Resolve<Services.IModuleRegistry>());

            var pluginLoader = Container.Resolve<PluginLoader>();
            var orchestrator = Container.Resolve<ModuleLoadOrchestrator>();
            orchestrator.SetNavigationService(NavigationService.Instance);
            var manifests = pluginLoader.DiscoverModules();
            orchestrator.OnModulesDiscovered(manifests);
        }

        private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
                return null;

            var modulesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
            if (!Directory.Exists(modulesDir))
                return null;

            foreach (var subDir in Directory.GetDirectories(modulesDir))
            {
                var assemblyPath = Path.Combine(subDir, $"{assemblyName}.dll");
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }
}
