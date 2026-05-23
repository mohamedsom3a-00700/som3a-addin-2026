using System;
using System.Windows;
using System.Windows.Interop;
using Som3a_WPF_UI.Controls.Shell;

namespace Som3a_WPF_UI.Services
{
    public static class ShellNavigationHelper
    {
        public static void NavigateToShellPage(string pageKey, IntPtr excelWindowHandle)
        {
            try
            {
                NavigationService.Instance.NavigateTo(pageKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open shell page '{pageKey}': {ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static void NavigateToShellPage(string pageKey)
        {
            try
            {
                NavigationService.Instance.NavigateTo(pageKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open shell page '{pageKey}': {ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static bool IsShellPageRegistered(string pageKey)
        {
            var page = NavigationService.Instance.CreatePage(pageKey);
            return page != null;
        }
    }
}
