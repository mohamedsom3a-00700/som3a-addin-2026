using System;

namespace Som3a_WPF_UI.Contracts
{
    public interface INavigationRegistrar
    {
        void RegisterPage(string id, string title, Type pageType);
        void RegisterPage(string id, string title, Type pageType, string category);
    }
}
