/// <summary>
/// Allows modules to register pages for display in the navigation shell.
/// </summary>
public interface INavigationRegistrar
{
    /// <summary>
    /// Registers a page that appears in the application's navigation.
    /// </summary>
    /// <param name="id">Unique page identifier within this module.</param>
    /// <param name="title">Display title shown in navigation UI.</param>
    /// <param name="pageType">Type of the WPF Page to navigate to.</param>
    void RegisterPage(string id, string title, Type pageType);
}
