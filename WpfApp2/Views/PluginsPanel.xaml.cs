using System;
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
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is null)
            {
                try
                {
                    var vm = new PluginDiagnosticsViewModel(App.Container);
                    vm.Refresh();
                    DataContext = vm;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PluginsPanel initialization failed: {ex.Message}");
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PluginDiagnosticsViewModel vm)
            {
                vm.Cleanup();
            }
        }
    }
}
