using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Som3a.Bridge;
using Som3a.Localization.Contracts;
using Som3a_WPF_UI.Contracts;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Views;
using Timer = System.Timers.Timer;
using LanguageChangedEventArgs = Som3a.Localization.Contracts.LanguageChangedEventArgs;

namespace Som3a_WPF_UI
{
    public partial class App : Application
    {
        public static IServiceContainer Container { get; } = new ServiceContainer();

        private SplashWindow _splashWindow;
        private PipeClientService _pipeClient;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            base.OnStartup(e);

            var isStandalone = e.Args.Contains("-standalone") || e.Args.Contains("--pipe");

            if (isStandalone)
            {
                RunStandalone(e);
                return;
            }

            CompositionRoot.RegisterServices(Container);

            ThemeManager.LoadSettings();

            ThemeManager.FreezeResources();

            var sidebarRegistration = Container.Resolve<ISidebarRegistrationProvider>();
            sidebarRegistration.RegisterStaticPages();

            var localizationService = Container.Resolve<ILocalizationService>();
            localizationService.LanguageChanged += OnLanguageChanged;
            localizationService.LoadLanguagePreference();

            ShowSplashWindow();

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

                    StartPeriodicGarbageCollection();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Deferred initialization failed: {ex}");
                }
            }));
        }

        private void RunStandalone(StartupEventArgs e)
        {
            CompositionRoot.RegisterServices(Container);
            ThemeManager.LoadSettings();
            ThemeManager.FreezeResources();

            var sidebarRegistration = Container.Resolve<ISidebarRegistrationProvider>();
            sidebarRegistration.RegisterStaticPages();

            var localizationService = Container.Resolve<ILocalizationService>();
            localizationService.LanguageChanged += OnLanguageChanged;
            localizationService.LoadLanguagePreference();

            CompositionRoot.InitializeModules(Container.Resolve<Services.IModuleRegistry>());

            _pipeClient = new PipeClientService();

            int retryCount = 0;
            int maxRetries = PipeConstants.HandshakeMaxRetries;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
            {
                try
                {
                    while (retryCount < maxRetries)
                    {
                        var connected = await _pipeClient.ConnectAsync(PipeConstants.HandshakeTimeoutMs);
                        if (!connected)
                        {
                            retryCount++;
                            System.Diagnostics.Trace.WriteLine($"[Standalone] Connection attempt {retryCount}/{maxRetries} failed. Retrying in {PipeConstants.HandshakeRetryMs}ms...");
                            await Task.Delay(PipeConstants.HandshakeRetryMs);
                            continue;
                        }

                        var handshakeOk = await _pipeClient.PerformHandshakeAsync();
                        if (handshakeOk)
                        {
                            System.Diagnostics.Trace.WriteLine("[Standalone] Handshake succeeded. Showing ShellWindow.");
                            ShowStandaloneShellWindow();
                            return;
                        }

                        retryCount++;
                        System.Diagnostics.Trace.WriteLine($"[Standalone] Handshake attempt {retryCount}/{maxRetries} failed.");
                        await Task.Delay(PipeConstants.HandshakeRetryMs);
                    }

                    System.Diagnostics.Trace.WriteLine("[Standalone] All handshake attempts failed. Showing error.");
                    MessageBox.Show(
                        "Could not connect to Excel. Please ensure Excel is running with the Som3a add-in loaded.\n\nThe application will now exit.",
                        "Connection Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[Standalone] Fatal error in startup: {ex}");
                    MessageBox.Show(
                        $"An unexpected error occurred during startup:\n\n{ex.Message}\n\nThe application will now exit.",
                        "Startup Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                Current.Shutdown();
            }));
        }

        private void ShowStandaloneShellWindow()
        {
            var watchdog = new CrashWatchdogService(_pipeClient);
            watchdog.Start();
            watchdog.ShutdownTriggered += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Trace.WriteLine("[App] Watchdog triggered shutdown. Exiting.");
                    Current.Shutdown();
                });
            };

            var shell = new ShellWindow();
            if (_pipeClient.ExcelHwnd != 0)
                shell.SetOwner(new IntPtr(_pipeClient.ExcelHwnd));
            shell.Title = "Planova Platform";
            MainWindow = shell;
            shell.Show();
        }

        private void ShowSplashWindow()
        {
            _splashWindow = new SplashWindow();
            _splashWindow.SplashComplete += OnSplashComplete;
            _splashWindow.Show();
        }

        private void OnSplashComplete(object sender, EventArgs e)
        {
            _splashWindow = null;
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            var shell = new ShellWindow();
            shell.Title = "Planova Platform";
            MainWindow = shell;
            shell.Show();
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnLanguageChanged(sender, e)));
                return;
            }

            var isRTL = e.IsRTL;

            ShellRTLManager.Instance.ApplyLayout(isRTL);

            CultureAwareFormattingService.Instance.RefreshCulture(e.NewLanguageCode);

            var fontService = Container.Resolve<FontService>();
            fontService?.ApplyLanguageFont(e.NewLanguageCode);

            foreach (var d in NavigationService.Instance.Destinations)
                d.RefreshLabel();
        }

        private static Timer _gcTimer;

        private static void StartPeriodicGarbageCollection()
        {
            _gcTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            _gcTimer.Elapsed += (_, _) =>
            {
                try
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                    GC.WaitForPendingFinalizers();
                }
                catch
                {
                    // Silently handle GC failures — non-critical
                }
            };
            _gcTimer.AutoReset = true;
            _gcTimer.Start();
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
