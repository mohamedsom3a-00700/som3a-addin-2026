using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public abstract class PageBase : Page
    {
        protected PageBase()
        {
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        protected virtual void OnPageLoaded(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
