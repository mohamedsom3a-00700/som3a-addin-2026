using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Views
{
    public partial class PluginsPanel : UserControl
    {
        public PluginsPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is null)
            {
                try
                {
                    var diagnostics = App.Container.Resolve<ModuleDiagnosticsService>();
                    var registry = App.Container.Resolve<Som3a_WPF_UI.Contracts.IModuleRegistry>();
                    var vm = new PluginDiagnosticsViewModel(diagnostics, registry);
                    vm.Refresh();
                    DataContext = vm;
                }
                catch
                {
                }
            }
        }
    }
}
