using System;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class NavigationPage
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public Type PageType { get; set; }

        public Page CreateInstance()
        {
            return Activator.CreateInstance(PageType) as Page;
        }
    }
}
