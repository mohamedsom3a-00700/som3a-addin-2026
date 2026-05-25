using Microsoft.Office.Tools.Excel;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

namespace Som3a_Addin_2026
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            System.Windows.Application app = System.Windows.Application.Current;

            if (app == null)
            {
                app = new System.Windows.Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
            }

            Som3a_WPF_UI.CompositionRoot.RegisterServices(Som3a_WPF_UI.App.Container);
            Som3a_WPF_UI.CompositionRoot.InitializeModules(Som3a_WPF_UI.App.Container.Resolve<Som3a_WPF_UI.Services.IModuleRegistry>());

            var pluginLoader = Som3a_WPF_UI.App.Container.Resolve<PluginLoader>();
            var orchestrator = Som3a_WPF_UI.App.Container.Resolve<ModuleLoadOrchestrator>();
            orchestrator.SetNavigationService(NavigationService.Instance);
            var manifests = pluginLoader.DiscoverModules();
            orchestrator.OnModulesDiscovered(manifests);

            ThemeManager.InitializeApplicationResources();

            ThemeManager.LoadSettings();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
