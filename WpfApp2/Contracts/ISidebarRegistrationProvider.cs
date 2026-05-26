using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Contracts
{
    public interface ISidebarRegistrationProvider
    {
        void RegisterStaticPages();
        void RegisterPluginPages(IEnumerable<Type> pageTypes);
    }
}
