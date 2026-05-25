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
    }
}
