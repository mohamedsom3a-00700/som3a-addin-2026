using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Controls.Shell;

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

            ThemeManager.FreezeResources();

            var sidebarRegistration = Container.Resolve<ISidebarRegistrationProvider>();
            sidebarRegistration.RegisterStaticPages();

            LocalizationBridgeService.Instance.LoadLanguagePreference();

            LocalizationBridgeService.Instance.LanguageChanged += OnLanguageChanged;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
            {
                try
                {
                    CompositionRoot.InitializeModules(Container.Resolve<Services.IModuleRegistry>());

                    var pluginLoader = Container.Resolve<PluginLoader>();
                    var orchestrator = Container.Resolve<ModuleLoadOrchestrator>();
                    orchestrator.SetNavigationService(NavigationService.Instance);
                    var manifests = pluginLoader.DiscoverModules();
                    orchestrator.OnModulesDiscovered(manifests);

                    foreach (var manifest in manifests)
                    {
                        if (!orchestrator.IsModuleLoaded(manifest.Id))
                            orchestrator.EnsureModuleLoaded(manifest.Id);
                    }

                    var pluginTypes = orchestrator.GetAllPluginPageTypes();
                    if (pluginTypes.Count > 0)
                        sidebarRegistration.RegisterPluginPages(pluginTypes);

                    await CompositionRoot.RunStartupTasksAsync(Container);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Deferred initialization failed: {ex}");
                }
            }));
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            var isRTL = e.IsRTL;

            ShellRTLManager.Instance.ApplyLayout(isRTL);

            CultureAwareFormattingService.Instance.RefreshCulture(e.NewLanguageCode);

            if (isRTL)
            {
                ArabicFontManager.Instance.SetArabicFont("Cairo");
            }
            else
            {
                ArabicFontManager.Instance.ResetFont();
            }

            foreach (var d in NavigationService.Instance.Destinations)
                d.RefreshLabel();

            TranslationSource.Instance.Refresh();
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AssemblyResolve failed for '{assemblyPath}': {ex.Message}");
                    }
                }
            }

            return null;
        }
    }
}
