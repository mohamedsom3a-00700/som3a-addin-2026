using System.Windows;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.LoadSettings();
        }
    }
}