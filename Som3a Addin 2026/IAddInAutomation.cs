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
        string BoqLoad();
        string BoqConsent();
        string BoqGenerate();
        string BoqGetStatus();
        string BoqDebug();
    }
}
