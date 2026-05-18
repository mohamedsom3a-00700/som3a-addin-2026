using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Views;
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
                ShowSettingsWindow();
                return;
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(ShowSettingsWindow);
            }
            else
            {
                var thread = new System.Threading.Thread(() =>
                {
                    var app = new Application();
                    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    WpfApp2.Services.ThemeManager.LoadSettings();
                    ShowSettingsWindow();
                    Dispatcher.Run();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private static void ShowSettingsWindow()
        {
            var existing = Application.Current?.Windows
                .OfType<WpfApp2.Views.SettingsWindow>()
                .FirstOrDefault();

            if (existing != null)
            {
                existing.Activate();
                existing.Focus();
                return;
            }

            var win = new WpfApp2.Views.SettingsWindow();
            win.ShowDialog();
        }

        public static void Shutdown()
        {
            _wpfApp?.Shutdown();
            _wpfApp = null;
            _initialized = false;
        }
    }
}