using System;
using System.Windows;
using System.Windows.Threading;
using Som3a_WPF_UI.Services;
using WpfApp2 = Som3a_WPF_UI;

namespace Som3a_Addin_2026
{
    public static class Som3aAddinBridge
    {
        private static Application _wpfApp;
        private static Dispatcher _wpfDispatcher;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            if (Application.Current == null)
            {
                _wpfApp = new Application();
                _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _wpfDispatcher = Dispatcher.CurrentDispatcher;
            }

            WpfApp2.Services.ThemeManager.LoadSettings();
            _initialized = true;
        }

        public static void OpenSettings()
        {
            if (_wpfDispatcher != null && _wpfDispatcher.CheckAccess())
            {
                NavigationService.Instance.NavigateTo("settings.general");
                return;
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    NavigationService.Instance.NavigateTo("settings.general"));
            }
            else
            {
                var thread = new System.Threading.Thread(() =>
                {
                    _wpfApp = new Application();
                    _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    _wpfDispatcher = Dispatcher.CurrentDispatcher;
                    WpfApp2.Services.ThemeManager.LoadSettings();
                    NavigationService.Instance.NavigateTo("settings.general");
                    Dispatcher.Run();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public static void Shutdown()
        {
            _wpfApp?.Shutdown();
            _wpfApp = null;
            _initialized = false;
        }
    }
}