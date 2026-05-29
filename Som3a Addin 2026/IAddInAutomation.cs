using System.Runtime.InteropServices;

namespace Som3a_Addin_2026
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IAddInAutomation
    {
        string OpenWindow(string name);
        string SwitchTheme(string theme);
        string GetCurrentTheme();
        string GetStatus();
        string CloseWindow(string name);
        string RunDiagnostics();
        string VerifyTheme();
        string ExportWBSTemplate(string templateName);
        string ImportWBSTemplateFromExcel(string category);
        string SetWBSCodeMode(string mode);
        string SetWBSExportStyle(string style);
        string GetWBSMode();
        string GetTemplatePreviewData(string templateName);
        string BoqConfigureOllama(string model = "deepseek-coder", string endpoint = "http://localhost:11434");
        string BoqLoad();
        string BoqConsent();
        string BoqGenerate();
        string BoqGetStatus();
        string BoqGetProviderInfo();
        string BoqDebug();
        string DeCalculateDuration(string activityId, decimal quantity, decimal rate, int crew, decimal hours);
        string DeSearchBenchmarks(string category, string query);

        // Phase 23 — Dashboard & Home
        string GetHomePageStatus();
        string GetWidgetStatus(string widgetName);
        string WidgetClick(string widgetTitle);

        // Phase 24 — Localization & RTL
        string SwitchLanguage(string cultureCode);
        string GetCurrentLanguage();
        string IsRTLMode();
    }
}
