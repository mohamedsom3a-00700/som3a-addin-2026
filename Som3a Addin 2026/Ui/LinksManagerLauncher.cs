using Som3a_WPF_UI.Services;

namespace Som3a_Addin_2026.Ui
{
    public static class LinksManagerLauncher
    {
        public static void Show()
        {
            NavigationService.Instance.NavigateTo("excel.links");
        }
    }
}
