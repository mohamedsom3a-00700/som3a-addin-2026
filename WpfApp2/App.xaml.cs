using System.Windows;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI
{
    public partial class App : Application
    {
        public static IServiceContainer Container { get; } = new ServiceContainer();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CompositionRoot.RegisterServices(Container);

            ThemeManager.LoadSettings();

            CompositionRoot.InitializeModules(Container.Resolve<IModuleRegistry>());
        }
    }
}
